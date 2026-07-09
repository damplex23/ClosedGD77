#!/bin/bash
# ClosedGD77 - Build CPS for Linux
# Requires: mono-complete, libusb-1.0-0-dev
#
# Usage: ./build_cps_linux.sh [Debug|Release]

set -e

CONFIG="${1:-Release}"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
CPS_DIR="$(dirname "$SCRIPT_DIR")/cps"
SLN="$CPS_DIR/OpenGD77CPS_Linux.sln"

echo "=== ClosedGD77 CPS Linux Build ==="
echo "Configuration: $CONFIG"
echo ""

# Check dependencies
check_dep() {
    if ! command -v "$1" &>/dev/null; then
        echo "ERROR: $1 not found. Install it first."
        case "$1" in
            msbuild|xbuild)
                echo "  sudo apt install mono-complete"
                ;;
            nuget)
                echo "  sudo apt install nuget"
                ;;
        esac
        exit 1
    fi
}

# Find msbuild (Mono's msbuild or xbuild)
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

# Restore NuGet packages
echo "--- Restoring NuGet packages ---"
cd "$CPS_DIR"
if command -v nuget &>/dev/null; then
    nuget restore OpenGD77CPS_Linux.sln 2>/dev/null || true
fi

# Build
echo ""
echo "--- Building CPS ($CONFIG) ---"
$MSBUILD "$SLN" \
    /p:Configuration="$CONFIG" \
    /p:Platform=x86 \
    /p:DefineConstants="LINUX_BUILD" \
    /v:minimal

# Show output
echo ""
echo "=== Build complete ==="
OUTDIR="$CPS_DIR/bin/$CONFIG"
if [ -f "$OUTDIR/OpenGD77CPS.exe" ]; then
    echo "Output: $OUTDIR/OpenGD77CPS.exe"
    echo ""
    echo "Run with: mono $OUTDIR/OpenGD77CPS.exe"
    echo ""
    echo "Linux notes:"
    echo "  - USB HID: Ensure libusb-1.0 is installed (sudo apt install libusb-1.0-0)"
    echo "  - For USB access without root: copy 99-opengd77.rules to /etc/udev/rules.d/"
    echo "  - Serial comms: System.IO.Ports works via /dev/ttyUSB*"
else
    echo "ERROR: Build failed. Check build output above."
    exit 1
fi
