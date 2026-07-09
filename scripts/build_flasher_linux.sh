#!/bin/bash
# ClosedGD77 - Build firmware loader for Linux
# Requires: mono-complete, libusb-1.0-0-dev
#
# Usage: ./build_flasher_linux.sh [Debug|Release]

set -e

CONFIG="${1:-Release}"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
FWLOADER_DIR="$(dirname "$SCRIPT_DIR")/mk22-firmware/tools/firmware_loader"
SLN="$FWLOADER_DIR/FirmwareLoader_LibUsbDotNet.sln"

echo "=== ClosedGD77 Firmware Loader Linux Build ==="
echo "Configuration: $CONFIG"
echo ""

# Find msbuild
if command -v msbuild &>/dev/null; then
    MSBUILD="msbuild"
elif command -v xbuild &>/dev/null; then
    MSBUILD="xbuild"
elif command -v dotnet &>/dev/null; then
    MSBUILD="dotnet msbuild"
else
    echo "ERROR: msbuild not found. Install Mono:"
    echo "  sudo apt install mono-complete"
    exit 1
fi

echo "MSBuild: $MSBUILD"
echo ""

cd "$FWLOADER_DIR"

# Restore NuGet packages
if command -v nuget &>/dev/null; then
    nuget restore FirmwareLoader_LibUsbDotNet.sln 2>/dev/null || true
fi

# Build
echo "--- Building Firmware Loader ($CONFIG) ---"
$MSBUILD "$SLN" \
    /p:Configuration="$CONFIG" \
    /p:Platform=AnyCPU \
    /v:minimal

echo ""
echo "=== Build complete ==="
OUTDIR="$FWLOADER_DIR/bin/$CONFIG"
if [ -f "$OUTDIR/FirmwareLoader.exe" ]; then
    echo "Output: $OUTDIR/FirmwareLoader.exe"
    echo ""
    echo "Run with: mono $OUTDIR/FirmwareLoader.exe"
    echo ""
    echo "Supports: GD-77, GD-77S, DM-1801, RD-5R (MK22-based radios)"
    echo "For DM-1701/UV380/RT-3S (STM32): use dfu-util directly:"
    echo "  dfu-util -d 0483:df11 -D firmware.dfu -s :leave"
else
    echo "ERROR: Build failed."
    exit 1
fi
