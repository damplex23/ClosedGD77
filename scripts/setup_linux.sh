#!/bin/bash
# ClosedGD77 - Complete Linux setup
# Installs dependencies, builds CPS and flasher, configures udev rules.
#
# Usage: sudo ./scripts/setup_linux.sh

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"

echo "============================================"
echo " ClosedGD77 Linux Setup"
echo "============================================"
echo ""
echo "This will:"
echo "  1. Install build dependencies (mono, libusb, dfu-util)"
echo "  2. Install udev rules for radio USB access"
echo "  3. Build CPS (Codeplug Programming Software)"
echo "  4. Build Firmware Loader (MK22 radios: GD-77, DM-1801, RD-5R)"
echo "  5. Check dfu-util for STM32 radio flashing (DM-1701, UV380)"
echo ""

# ---- Step 1: Dependencies ----
echo "--- Step 1: Installing dependencies ---"

if command -v apt-get &>/dev/null; then
    echo "Detected Debian/Ubuntu"
    apt-get update -qq
    apt-get install -y -qq \
        mono-complete \
        libusb-1.0-0 \
        libusb-1.0-0-dev \
        dfu-util \
        nuget \
        msbuild 2>/dev/null || apt-get install -y -qq mono-xbuild
elif command -v dnf &>/dev/null; then
    echo "Detected Fedora/RHEL"
    dnf install -y \
        mono-complete \
        libusb1 \
        libusb1-devel \
        dfu-util \
        nuget
elif command -v pacman &>/dev/null; then
    echo "Detected Arch Linux"
    pacman -S --noconfirm \
        mono \
        libusb \
        dfu-util \
        nuget
else
    echo "WARNING: Unknown package manager. Install manually:"
    echo "  - mono-complete (or mono >= 5.0)"
    echo "  - libusb-1.0-0 + dev headers"
    echo "  - dfu-util"
    echo "  - nuget"
fi

echo ""

# ---- Step 2: Udev rules ----
echo "--- Step 2: Installing udev rules ---"
UDEV_RULES="/etc/udev/rules.d/99-opengd77.rules"

if [ -f "$UDEV_RULES" ]; then
    echo "  Udev rules already installed: $UDEV_RULES"
else
    if [ -f "$SCRIPT_DIR/99-opengd77.rules" ]; then
        cp "$SCRIPT_DIR/99-opengd77.rules" "$UDEV_RULES"
        udevadm control --reload-rules
        udevadm trigger
        echo "  Installed: $UDEV_RULES"
        echo "  Unplug and reconnect your radio for permissions to take effect."
    else
        echo "  WARNING: 99-opengd77.rules not found. Create manually:"
        echo '    SUBSYSTEM=="usb", ATTR{idVendor}=="15a2", ATTR{idProduct}=="0073", MODE="0666", GROUP="plugdev"'
        echo '    SUBSYSTEM=="usb", ATTR{idVendor}=="0483", ATTR{idProduct}=="df11", MODE="0666", GROUP="plugdev"'
    fi
fi

echo ""

# ---- Step 3: Build CPS ----
echo "--- Step 3: Building CPS ---"
bash "$SCRIPT_DIR/build_cps_linux.sh" Release || {
    echo "  CPS build failed. You can still use:"
    echo "    * Windows CPS via Wine: wine OpenGD77CPS.exe"
    echo "    * Standalone firmware loader for flashing"
}

echo ""

# ---- Step 4: Build Firmware Loader ----
echo "--- Step 4: Building Firmware Loader ---"
bash "$SCRIPT_DIR/build_flasher_linux.sh" Release || {
    echo "  Firmware loader build failed."
    echo "  Pre-built may be available. Check mk22-firmware/tools/firmware_loader/bin/"
}

echo ""

# ---- Step 5: Verify dfu-util ----
echo "--- Step 5: Checking dfu-util (STM32 flashing) ---"
if command -v dfu-util &>/dev/null; then
    echo "  dfu-util found: $(dfu-util --version 2>&1 | head -1)"
    echo "  STM32 radios (DM-1701, UV380, RT-3S):"
    echo "    Put radio in DFU mode, then:"
    echo "    dfu-util -d 0483:df11 -D firmware.dfu -s :leave"
else
    echo "  dfu-util NOT found. Install: sudo apt install dfu-util"
fi

echo ""
echo "============================================"
echo " Setup complete!"
echo "============================================"
echo ""
echo "Usage:"
echo ""
echo "  CPS (Codeplug editor):"
echo "    mono cps/bin/Release/OpenGD77CPS.exe"
echo ""
echo "  Firmware Loader (MK22 radios - GD-77, DM-1801, RD-5R):"
echo "    mono mk22-firmware/tools/firmware_loader/bin/Release/FirmwareLoader.exe"
echo ""
echo "  STM32 Firmware Flash (DM-1701, UV380, RT-3S):"
echo "    dfu-util -d 0483:df11 -D <firmware.dfu> -s :leave"
echo ""
echo "  Encryption keys:"
echo "    Use CPS → Extras → Encryption Keys menu"
echo "    Keys transfer to radio via USB HID"
echo ""
echo "Build from source:"
echo "  scripts/build_cps_linux.sh"
echo "  scripts/build_flasher_linux.sh"
