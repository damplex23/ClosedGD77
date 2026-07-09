#!/bin/bash
# ClosedGD77 - Launch CPS on Linux
# Wrapper that sets up Mono environment and runs the CPS

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
CPS_DIR="$SCRIPT_DIR/../cps"

# Find the CPS binary
CPS_EXE=""
for d in "$CPS_DIR/bin/Release" "$CPS_DIR/bin/Debug" \
    "$CPS_DIR/bin/ReleaseOpenGD77" "$CPS_DIR/bin/Debug_OpenGD77"; do
    if [ -f "$d/OpenGD77CPS.exe" ]; then
        CPS_EXE="$d/OpenGD77CPS.exe"
        break
    fi
done

if [ -z "$CPS_EXE" ]; then
    echo "CPS binary not found. Build first:"
    echo "  ./scripts/build_cps_linux.sh"
    exit 1
fi

# Check for LibUsbDotNet.dll alongside the exe
LIBUSB_DLL="$(dirname "$CPS_EXE")/LibUsbDotNet.dll"
if [ ! -f "$LIBUSB_DLL" ]; then
    # Copy it from cps directory
    if [ -f "$CPS_DIR/LibUsbDotNet.dll" ]; then
        cp "$CPS_DIR/LibUsbDotNet.dll" "$(dirname "$CPS_EXE")/"
    fi
fi

# Run with Mono
export MONO_LOG_LEVEL=warning
exec mono "$CPS_EXE" "$@"
