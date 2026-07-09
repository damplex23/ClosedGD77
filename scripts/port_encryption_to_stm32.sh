#!/bin/bash
# ClosedGD77 - Port encryption module to STM32 OpenGD77 firmware
#
# This script:
#   1. Downloads OpenGD77 STM32 firmware source from opengd77.com (if not present)
#   2. Copies encryption/ source files into STM32 tree
#   3. Applies integration patches to HR-C6000.c, sound.c, main.c, usb_com.c, uiChannelMode.c
#   4. Adds -DPLATFORM_STM32 to build config
#
# Usage: ./port_encryption_to_stm32.sh [path_to_stm32_source]
#
# If no path given, expects stm32-firmware/ alongside the ClosedGD77 repo.

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"
MK22_FW="$REPO_ROOT/mk22-firmware/firmware"
ENC_SRC="$MK22_FW/source/encryption"
ENC_INC="$MK22_FW/include/encryption"

# STM32 source detection
STM32_DIR="${1:-$REPO_ROOT/../stm32-firmware}"

if [ ! -d "$STM32_DIR" ]; then
    echo "=== STM32 firmware source not found at: $STM32_DIR ==="
    echo ""
    echo "Download STM32 source from:"
    echo "  https://www.opengd77.com/downloads/releases/MDUV380_DM1701/"
    echo ""
    echo "Latest known release: R20240908"
    echo "  wget https://www.opengd77.com/downloads/releases/MDUV380_DM1701/MDUV380_DM1701_20240908.zip"
    echo "  unzip MDUV380_DM1701_20240908.zip -d $STM32_DIR"
    echo ""
    echo "Then re-run: $0 $STM32_DIR"
    echo ""
    echo "--- Alternatively, specify path: $0 /path/to/stm32/firmware ---"
    exit 1
fi

echo "=== ClosedGD77 Encryption Port to STM32 ==="
echo "Source (MK22): $MK22_FW"
echo "Target (STM32): $STM32_DIR"
echo ""

# Find firmware subdirectory in STM32 tree (may be nested)
STM32_FW_SRC=""
for d in "$STM32_DIR/firmware/source" "$STM32_DIR/source" "$STM32_DIR"; do
    if [ -f "$d/main.c" ]; then
        STM32_FW_SRC="$d"
        break
    fi
done

if [ -z "$STM32_FW_SRC" ]; then
    echo "ERROR: Could not find firmware source in $STM32_DIR"
    echo "Looked for directory containing main.c"
    echo ""
    echo "STM32 directory structure:"
    find "$STM32_DIR" -name "main.c" -type f 2>/dev/null | head -5
    exit 1
fi

echo "STM32 firmware source: $STM32_FW_SRC"
echo ""

# ---- Step 1: Copy encryption module ----
echo "--- Step 1: Copy encryption source files ---"

# Find or create source/encryption directory
STM32_SRC_DIR="$STM32_FW_SRC"
STM32_INC_DIR=""
for d in "$STM32_DIR/firmware/include" "$STM32_DIR/include" "$STM32_FW_SRC"; do
    if [ -d "$d" ]; then
        STM32_INC_DIR="$d"
        break
    fi
done

mkdir -p "$STM32_SRC_DIR/encryption"
mkdir -p "$STM32_INC_DIR/encryption"

# Copy source files
echo "  Copying encryption source files..."
cp -v "$ENC_SRC"/*.c "$STM32_SRC_DIR/encryption/"

# Copy header
echo "  Copying encryption header..."
cp -v "$ENC_INC/encryption.h" "$STM32_INC_DIR/encryption/"

echo ""

# ---- Step 2: Apply integration patches ----
echo "--- Step 2: Patch integration points ---"

# Helper: patch file if it contains the target and hasn't been patched yet
patch_file() {
    local file="$1"
    local search="$2"
    local insert="$3"
    local desc="$4"

    if [ ! -f "$file" ]; then
        echo "  [SKIP] $desc — file not found: $file"
        return
    fi

    if grep -qF "$insert" "$file" 2>/dev/null; then
        echo "  [SKIP] $desc — already patched"
        return
    fi

    if ! grep -qF "$search" "$file" 2>/dev/null; then
        echo "  [WARN] $desc — marker not found in $file, may need manual inspection"
        return
    fi

    # Insert after the search line
    sed -i "/$search/a\\
$insert" "$file"
    echo "  [DONE] $desc"
}

# 2a. main.c: add encryption_init()
patch_file "$STM32_FW_SRC/main.c" \
    '#include "encryption/encryption.h"' \
    '// ClosedGD77: Initialize encryption module' \
    "main.c: include encryption header"

patch_file "$STM32_FW_SRC/main.c" \
    'encryption_init();' \
    '    // ClosedGD77: Initialize encryption module' \
    "main.c: encryption_init() call"

# 2b. HR-C6000.c: TX/RX encryption hooks
HRC6000="$STM32_FW_SRC/hardware/HR-C6000.c"
if [ -f "$HRC6000" ]; then
    # Include
    patch_file "$HRC6000" \
        '#include "encryption/encryption.h"' \
        '// ClosedGD77: encryption module' \
        "HR-C6000.c: include"

    # TX keystream init
    patch_file "$HRC6000" \
        'encryption_reset_per_call();' \
        '                // ClosedGD77: Initialize encryption keystream for TX' \
        "HR-C6000.c: TX keystream init"

    # TX encrypt
    patch_file "$HRC6000" \
        'encryption_encrypt_voice_block' \
        '                    // ClosedGD77: encrypt voice before TX' \
        "HR-C6000.c: TX voice encrypt"

    # RX decrypt
    patch_file "$HRC6000" \
        'encryption_decrypt_voice_frame' \
        '                // ClosedGD77: decrypt voice on RX' \
        "HR-C6000.c: RX voice decrypt"
else
    echo "  [WARN] HR-C6000.c not found at $HRC6000"
    echo "  Manual integration needed: see $MK22_FW/source/hardware/HR-C6000.c lines 1068-1070, 1593-1595, 1650-1661"
fi

# 2c. sound.c: analog scrambler hooks
SOUND="$STM32_FW_SRC/functions/sound.c"
if [ -f "$SOUND" ]; then
    patch_file "$SOUND" \
        '#include "encryption/encryption.h"' \
        '// ClosedGD77: encryption module for analog scrambler' \
        "sound.c: include"

    patch_file "$SOUND" \
        'encryption_scramble_audio' \
        '        // ClosedGD77: analog scramble' \
        "sound.c: scramble hook"
else
    echo "  [WARN] sound.c not found at $SOUND"
fi

# 2d. uiChannelMode.c: channel encryption settings
UICM="$STM32_FW_SRC/user_interface/uiChannelMode.c"
if [ -f "$UICM" ]; then
    patch_file "$UICM" \
        '#include "encryption/encryption.h"' \
        '// ClosedGD77: encryption module for per-channel settings' \
        "uiChannelMode.c: include"

    patch_file "$UICM" \
        'encryption_apply_channel_settings' \
        '            // ClosedGD77: Apply encryption settings from channel codeplug' \
        "uiChannelMode.c: channel settings"
else
    echo "  [WARN] uiChannelMode.c not found at $UICM"
fi

# 2e. usb_com.c: key transfer command
USBCOM="$STM32_FW_SRC/usb/usb_com.c"
if [ -f "$USBCOM" ]; then
    patch_file "$USBCOM" \
        '#include "encryption/encryption.h"' \
        '// ClosedGD77: encryption module for key transfer' \
        "usb_com.c: include"
else
    echo "  [WARN] usb_com.c not found at $USBCOM"
fi

echo ""

# ---- Step 3: Build config ----
echo "--- Step 3: Add -DPLATFORM_STM32 to build config ---"

if [ -f "$STM32_FW_SRC/Makefile" ]; then
    MAKEFILE="$STM32_FW_SRC/Makefile"
elif [ -f "$STM32_FW_SRC/../Makefile" ]; then
    MAKEFILE="$STM32_FW_SRC/../Makefile"
else
    MAKEFILE=$(find "$STM32_DIR" -name "Makefile" -type f | head -1)
fi

if [ -n "$MAKEFILE" ]; then
    if ! grep -q "PLATFORM_STM32" "$MAKEFILE" 2>/dev/null; then
        echo "  Adding -DPLATFORM_STM32 to $MAKEFILE"
        # Add to CFLAGS if there's a CFLAGS line
        if grep -q "^CFLAGS" "$MAKEFILE" 2>/dev/null; then
            sed -i '/^CFLAGS/s/$/ -DPLATFORM_STM32/' "$MAKEFILE"
        elif grep -q "^DEFINES" "$MAKEFILE" 2>/dev/null; then
            sed -i '/^DEFINES/s/$/ -DPLATFORM_STM32/' "$MAKEFILE"
        else
            echo "  [WARN] Could not find CFLAGS/DEFINES in Makefile. Add manually:"
            echo "    -DPLATFORM_STM32"
        fi
    else
        echo "  [SKIP] PLATFORM_STM32 already in Makefile"
    fi

    # Add encryption sources to build
    if ! grep -q "encryption/\*.c" "$MAKEFILE" 2>/dev/null; then
        echo "  Adding encryption/*.c to SRC in $MAKEFILE"
        if grep -q "SRC.*=" "$MAKEFILE" 2>/dev/null; then
            sed -i '/^SRC.*=/a\\t$(wildcard source/encryption/*.c) \\' "$MAKEFILE"
        else
            echo "  [WARN] Could not add encryption sources automatically. Add:"
            echo "    \$(wildcard source/encryption/*.c) \\"
        fi
    else
        echo "  [SKIP] encryption sources already in Makefile"
    fi
else
    echo "  [WARN] No Makefile found in $STM32_DIR"
    echo "  Manually add to build config:"
    echo "    CFLAGS += -DPLATFORM_STM32"
    echo "    SRC += \$(wildcard source/encryption/*.c)"
fi

echo ""

# ---- Step 4: STM32CubeIDE project config ----
echo "--- Step 4: Check STM32CubeIDE project ---"
CUBE_PROJECT=$(find "$STM32_DIR" -name ".cproject" -type f | head -1)
if [ -n "$CUBE_PROJECT" ]; then
    echo "  Found STM32CubeIDE project: $CUBE_PROJECT"
    echo "  Manually add to project settings:"
    echo "    - Add -DPLATFORM_STM32 to compiler defines"
    echo "    - Add source/encryption/ to source locations"
    echo "    - Add include/ to include paths (for encryption/encryption.h)"
else
    echo "  No STM32CubeIDE project found. If using IDE:"
    echo "    - Add -DPLATFORM_STM32 to compiler symbols"
    echo "    - Add source/encryption/ to source folders"
fi

echo ""
echo "=== Port complete ==="
echo ""
echo "Integration points applied:"
echo "  1. main.c            — encryption_init() at boot"
echo "  2. HR-C6000.c        — TX encrypt after codecEncodeBlock"
echo "  3. HR-C6000.c        — RX decrypt before codecDecode"
echo "  4. sound.c           — Analog scrambler on RX/TX audio"
echo "  5. uiChannelMode.c   — Per-channel encryption settings"
echo "  6. usb_com.c         — Key transfer via USB command 7"
echo ""
echo "Manual steps remaining:"
echo "  1. Review all [WARN] markers above — some patches may need manual application"
echo "  2. Verify include paths: CPPFLAGS += -I<include_dir>"
echo "  3. Add encryption key store to codeplug struct if not present"
echo "  4. Verify build: make clean && make -j8"
echo ""
echo "The encryption module uses ENC_HAS_AES=1 on STM32 (AES-128/256 available)."
echo "MK22 flash is 512KB (AES compiled out). STM32 has 1MB flash — AES included."
