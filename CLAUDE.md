# CLAUDE.md

ClosedGD77 — commercial fork of OpenGD77 firmware adding DMR voice encryption (ARC4/AES-128) and analog FM scrambling. Target market: business/commercial DMR in Romania.

## Status (2026-07-09)

- **CPS builds on Linux** — Mono 6.14 xbuild, 0 errors, runs under `mono OpenGD77CPS.exe`
- **Flasher builds on Linux** — `mono FirmwareLoader.exe` (MK22 radios only)
- **DM-1701 firmware builds + flashes** — ARM GCC 15.2, 804KB binary (76.8% of 1MB flash), flashed and running on physical Baofeng DM-1701
- **DFU flash working** — Python flasher `scripts/dfu_flash.py` using ctypes+libusb-1.0, DfuSe protocol. Flash base corrected to **0x0800C000** (firmware linked there; bootloader occupies 0x08000000–0x0800BFFF). Was incorrectly 0x08000000 in previous builds — would overwrite bootloader.
- **Key upload** — `WriteKeysToRadio()` implemented, USB CDC-ACM protocol tested with `'A'` ACK
- **Encryption menu** — physical radio menu at Options→Encryption, global override for ARC4/AES-128/Scrambler
- **19 bugs fixed today** — see [Bug fixes (2026-07-09)](#bug-fixes-2026-07-09)

## Architecture

### Two firmware codebases

| Platform | MCU | Flash | RAM | Radios | Source |
|----------|-----|-------|-----|--------|--------|
| **MK22** | NXP MK22FN512 (120MHz) | 512KB | 128KB | GD-77, DM-1801, RD-5R | `mk22-firmware/` |
| **STM32** | STM32F405VGT (168MHz) | 1MB | 192KB | DM-1701, UV380, RT-3S | `stm32-firmware/OpenGD77_MDUV380_DM1701_20240908/` |

### Hardware pipeline

```
Antenna → AT1846S (RF transceiver, I2C) → HR-C6000 (DMR baseband, SPI) → MCU
                                                                             ↓
Mic/Speaker ← HR-C6000 audio codec (I2S) ← MCU handles AMBE vocoder + UI
```

## Build

### CPS (C# Windows Forms, cross-platform)

Linux (Mono 6.x):
```bash
xbuild cps/OpenGD77CPS_Linux.sln /p:Configuration=Release /p:Platform=x86
```
Output: `cps/bin/ReleaseOpenGD77/OpenGD77CPS.exe` → `mono OpenGD77CPS.exe`

**NuGet**: `Newtonsoft.Json 12.0.3` must be in `cps/packages/` — Mono `nuget restore` is broken, download manually from nuget.org.

**`#if LINUX_BUILD` gating**:
- `CodeplugComms.cs` — `HIDSpecifiedDevice` alias → `CrossPlatformSpecifiedDevice` (libusb). **Fixed**: `writeCodeplug()` was using bare `SpecifiedDevice` instead of alias.
- `CrossPlatformHIDDevice.cs` — Linux: LibUsbDotNet direct USB, Windows: `SpecifiedDevice` passthrough
- `.csproj` `DefineConstants`: Base config has `LINUX_BUILD;$(DefineConstants)`. Release|x86 and Debug|x86 were missing `LINUX_BUILD` — **fixed**.

**Path separator fix**: All `Application.StartupPath + "\\..."` replaced with `Path.Combine()` in `IniFileUtils.cs`, `ChannelForm.cs`, `MainForm.cs`, `Log.cs`, `VfoForm.cs`.

**Sql setter bug fixed**: Both `ChannelForm.cs` and `VfoForm.cs` validated `this.sql <= 21` instead of `value <= 21` in the `Sql` property setter — caused out-of-range writes.

### Flasher (cross-platform, MK22 only)

```bash
xbuild mk22-firmware/tools/firmware_loader/FirmwareLoader_LibUsbDotNet.sln /p:Configuration=Release /p:Platform="Any CPU"
```
Output: `mk22-firmware/tools/firmware_loader/bin/Release/FirmwareLoader.exe`

### MK22 firmware (GD-77, DM-1801, RD-5R)

Requires MCUXpresso IDE + NXP MK22FN512 SDK.
```bash
make -f ../Makefile -j8 RADIO=GD77
```

**MK22 encryption fixes applied today**:
- `encryption.h`: Added `scramble_id`, `global_mode_override` fields, `uint16_t scramble_carrier_phase` (was `uint8_t`), 4 new API declarations
- `encryption.c`: Added `encryption_set_scramble_id()`, `encryption_get_scramble_id()`, `encryption_set_global_mode()`, `encryption_get_global_mode()`, global override in `encryption_apply_channel_settings()`
- `scrambler.c`: Fixed static `phase` bug — was ignoring caller state; signature unified with STM32 (`uint16_t *phase, uint8_t scramble_id`)
- `sound.c`: Added `(int16_t*)(void*)` cast on scrambler calls (RX and TX)
- `uiChannelMode.c`: Added `encryption_apply_channel_settings()` in `loadChannelData()` so encryption loads on every channel switch
- `HR-C6000.c`: Added `encryption_reset_per_call()` at `DMR_STATE_RX_END` to reset RX keystream between calls
- `usb_com.c`: Added `return` after case 7 ACK to prevent rogue `'-'` byte overwrite

### STM32 firmware (DM-1701, UV380, RT-3S)

**Manual CLI build** (ARM GCC 15.2):
```bash
# 1. Generate codec blob from Radioddity firmware v3.1.1
cd stm32-firmware/.../tools && ./codec_cleaner -C
cp codec_bin_section_1.bin ../application/source/linkerdata/

# 2. Convert to linkable object
cd ../application/source/linkerdata
arm-none-eabi-objcopy -I binary -O elf32-littlearm -B arm codec_bin_section_1.bin codec_bin_section_1.o
arm-none-eabi-objcopy --rename-section .data=.codec_bin_section_1 codec_bin_section_1.o

# 3. AMBE codec entry points — defined via PROVIDE() in STM32F405VGTX_FLASH.ld:
#    PROVIDE(AMBE_ENCODE_SYM     = 0x0807537C + 0x130 + 1);
#    PROVIDE(AMBE_ENCODE_ECC_SYM = 0x0807537C + 0x4e8 + 1);
#    PROVIDE(AMBE_DECODE_SYM     = 0x0807537C + 0x5d8 + 1);
#    +1 = Thumb interwork bit. ambe_syms.s is an empty placeholder — not linked.
#    Old stubs placed sequentially with codec blob (GCC 15 sequential section placement),
#    never resolved to actual codec code → DMR audio was broken.

# 4. Compile all .c
TOOLCHAIN=/opt/arm-gnu-toolchain-15.2.rel1-x86_64-arm-none-eabi/bin
CFLAGS="-mcpu=cortex-m4 -mthumb -mfloat-abi=hard -mfpu=fpv4-sp-d16 -g3 -Os -ffunction-sections -fdata-sections -fno-common"
DEFINES="-DPLATFORM_STM32 -DPLATFORM_RT84_DM1701 -DPLATFORM_VARIANT_DM1701 -DSTM32F405xx -DUSE_HAL_DRIVER -DHAS_GPS -DHAS_COLOURS"
INCS="-ICore/Inc -IDrivers/CMSIS/Include -IDrivers/CMSIS/Device/ST/STM32F4xx/Include -IDrivers/STM32F4xx_HAL_Driver/Inc -IMiddlewares/ST/STM32_USB_Device_Library/Class/CDC/Inc -IMiddlewares/ST/STM32_USB_Device_Library/Core/Inc -IMiddlewares/Third_Party/FreeRTOS/Source/include -IMiddlewares/Third_Party/FreeRTOS/Source/CMSIS_RTOS_V2 -IMiddlewares/Third_Party/FreeRTOS/Source/portable/GCC/ARM_CM4F -Iapplication/include -Iapplication/include/dmr_codec -Iapplication/include/encryption -Iapplication/include/functions -Iapplication/include/hardware -Iapplication/include/interfaces -Iapplication/include/io -Iapplication/include/usb -Iapplication/include/user_interface -Iapplication/include/user_interface/languages -ISeggerRTT/Config -ISeggerRTT/RTT -ISeggerRTT/Syscalls -IUSB_DEVICE/App -IUSB_DEVICE/Target"

for src in $(find Core Drivers Middlewares USB_DEVICE SeggerRTT application -name "*.c"); do
    ${TOOLCHAIN}/arm-none-eabi-gcc $CFLAGS $DEFINES $INCS -c $src -o build_dm1701/$(basename $src .c).o
done

# 5. Link WITHOUT ambe_syms.o, WITH codec_bin_section_1.o and codec_interface_fixed.o
#    codec_interface.c is compiled TWICE — once normally (excluded from link),
#    once as codec_interface_fixed.o (included in link). This is the upstream pattern.
${TOOLCHAIN}/arm-none-eabi-gcc -mcpu=cortex-m4 -mthumb -mfloat-abi=hard -mfpu=fpv4-sp-d16 \
    -specs=nosys.specs -specs=nano.specs -Wl,--gc-sections \
    -T STM32F405VGTX_FLASH.ld -Wl,-Map=firmware.map \
    build_dm1701/*.o -o build_dm1701/firmware.elf -lm

# manual removal of duplicate codec_interface.c.o required before link
# rm build_dm1701/application_source_dmr_codec_codec_interface.c.o

${TOOLCHAIN}/arm-none-eabi-objcopy -O binary build_dm1701/firmware.elf build_dm1701/firmware.bin
```

A Makefile template exists at `stm32-firmware/.../MDUV380_firmware/Makefile`. Current build uses flat .o naming in `build_dm1701/`. For clean rebuild: delete build_dm1701/ first.

**Build config from `.cproject`**: `DM1701_FW` = STM32F405VGTx. **Fixed**: `PLATFORM_STM32` was missing from DM1701_FW, JA_DM1701_FW, RT84_FW, JA_RT84_FW configs (8 sections) — `ENC_HAS_AES` gate was fragile. All 8 configs now have it.

**GCC 15 compatibility fixes**:
| File | Issue | Fix |
|------|-------|-----|
| `codec.h` | `#define AMBE_DECODE 0x08075954` causes "unsupported relocation" | Changed to symbol names `AMBE_DECODE_SYM` |
| `STM32F405VGTX_FLASH.ld` | ambe_syms.s stubs placed sequentially after codec blob (NOT overlapped) — AMBE codec never called, DMR audio broken | PROVIDE() at correct absolute addresses with +1 Thumb bit |
| `ambe_syms.s` | Empty placeholder — PROVIDE in linker script takes precedence | Kept as empty file for build system compatibility; not linked |
| `aes.c` | `#if ENC_HAS_AES` before `#include "encryption/encryption.h"` → `ENC_HAS_AES` undefined | Moved includes before `#if` guard |
| `sound.c` | `encryption_scramble_audio(volatile uint8_t*)` vs `int16_t*` | Cast: `(int16_t*)(void*)` |
| `usb_com.c` | MK22-style `USB_DeviceCdcAcmSend` not on STM32 | STM32 pattern: `usbComSendBuf[0]='A'; replyLength=1` |
| `FirmwareUpdate.cs` | Out params uninitialized | Initialize before use |
| `FirmwareUpdate_CrossPlatform.cs` | Missing `IFirmwareUpdate` methods | Added `UpdateFirmware()`, `MassErase()`, `ParseDFU_File()` stubs |

**STM32CubeIDE**: Open `.cproject` in STM32CubeIDE, select `DM1701_FW` config, build. Requires AMBE codec blobs.

### STM32 firmware bugs fixed (2026-07-09)

- `usb_com.c`: Added `return` after case 8 ACK — same `'-'` overwrite bug as MK22
- `uiChannelMode.c`: Added `encryption_apply_channel_settings()` on channel switch
- `HR-C6000.c`: Added `encryption_reset_per_call()` at `DMR_STATE_RX_END`
- `.cproject`: `PLATFORM_STM32` added to DM1701_FW/JA_DM1701_FW/RT84_FW/JA_RT84_FW
- `STM32F405VGTX_FLASH.ld`: AMBE PROVIDE symbols with Thumb +1 bit
- `ambe_syms.s`: Emptied (PROVIDE handles symbols)

## Bug fixes (2026-07-09)

Comprehensive audit found 19 bugs across all components. All fixed.

### CPS (4 bugs)

| File | Bug | Fix |
|------|-----|-----|
| `CodeplugComms.cs:853` | `SpecifiedDevice` used directly in `writeCodeplug()` — Linux compile error | Use `HIDSpecifiedDevice` alias |
| `OpenGD77CPS_Linux.csproj:52,74` | Release\|x86 and Debug\|x86 overwrote `DefineConstants`, dropping `LINUX_BUILD` | Add `LINUX_BUILD;` prefix |
| `ChannelForm.cs:1604` | Sql setter checked `this.sql <= 21` not `value <= 21` | Check `value <= 21` |
| `VfoForm.cs:1383` | Same Sql setter bug | Same fix |

### MK22 firmware (7 bugs)

| File | Bug | Fix |
|------|-----|-----|
| `usb_com.c:340` | Case 7 ACK `'A'` overwritten by generic `'-'` at line 435 | `return` after ACK send |
| `uiChannelMode.c:455` | `encryption_apply_channel_settings()` only called on first run, not channel switch | Call in `loadChannelData()` |
| `scrambler.c:70` | `static uint16_t phase` ignored caller state; signature mismatched STM32 | Use `*phase` parameter, unify signature |
| `sound.c:262,316` | Missing `(int16_t*)(void*)` cast on scrambler calls | Add cast (RX and TX) |
| `encryption.h:64-75` | Missing `scramble_id`, `global_mode_override` fields; `uint8_t` phase (should be `uint16_t`) | Sync with STM32 header |
| `encryption.c` | Missing global override, scramble ID API, channel switch guard | Add all 4 functions + override logic |
| `HR-C6000.c:1028` | RX keystream never reset between calls (one-time `rx_keystream_ready = true`) | `encryption_reset_per_call()` at `DMR_STATE_RX_END` |

### STM32 firmware (6 bugs)

| File | Bug | Fix |
|------|-----|-----|
| `usb_com.c:906` | Case 8 ACK `'A'` overwritten by generic `'-'` at line 920 | `return` after ACK set |
| `uiChannelMode.c:565` | `encryption_apply_channel_settings()` only on first run | Call on every channel change |
| `HR-C6000.c:1685` | RX keystream never reset at call end | `encryption_reset_per_call()` at `DMR_STATE_RX_END` |
| `STM32F405VGTX_FLASH.ld` + `ambe_syms.s` | AMBE stubs placed sequentially with codec blob — DMR audio non-functional | PROVIDE() at correct absolute address +1 (Thumb) |
| `.cproject` (8 sections) | `PLATFORM_STM32` missing from DM1701_FW/JA_DM1701_FW/RT84_FW/JA_RT84_FW | Added to all configs |

### Scripts (2 bugs)

| File | Bug | Fix |
|------|-----|-----|
| `dfu_flash.py:51` | `FLASH_BASE = 0x08000000` — write firmware over bootloader | Changed to `0x0800C000` |
| `dfu_flash.py:244-254` | Sector erase started at `0x08000000` — erased bootloader | Start sector list at `0x0800C000`, remove dead code |

### Verified correct (no changes needed)

- All 22 language files: encryption strings present, `LANGUAGE_TAG_VERSION` 0x04
- `menuEncryption.c`: Correct GREEN save/RED restore, mode cycling, scrambler ID cycling, `ENC_HAS_AES` conditional
- Menu registration: All 4 arrays in sync (enum, menuFunctions, data[], optionsMenuItems)
- Encryption hooks in HR-C6000.c: TX encrypt at `codecEncodeBlock`, RX decrypt at voice frame, superframe init at TX start
- `encryption_apply_channel_settings()`: Global override check correct, no infinite re-entry

## Encryption module

### Files (`mk22-firmware/firmware/source/encryption/` and `stm32-firmware/.../application/source/encryption/`)

| File | What | Flash | When compiled |
|------|------|-------|---------------|
| `encryption.c` | Key store, state machine, dispatch, global override | ~3KB | Always |
| `arc4.c` | ARC4 stream cipher | ~500B | Always |
| `aes.c` | AES-128-CTR (T-table) | ~3KB | `#if ENC_HAS_AES` (STM32 only) |
| `scrambler.c` | Analog frequency inversion, 8 carrier IDs (STM32) / 1 fixed carrier (MK22) | ~200B | Always |
| `menuEncryption.c` | Radio menu: Options→Encryption, global mode override | ~300B | STM32 only |

Platform gating: `-DPLATFORM_STM32` → `ENC_HAS_AES=1`, 32 key slots, 32-byte keys. `-DPLATFORM_MK22` → `ENC_HAS_AES=0`, 16 key slots.

**MK22 note**: Encryption menu not ported (flash budget constraint). MK22 uses CPS codeplug-only encryption. Scrambler uses single fixed 3.2kHz carrier (no 8-ID table — flash budget).

### encryption_state_t (global override)

`global_mode_override` field added (0xFF = use per-channel codeplug, 0=Off, 1=ARC4, 2=AES, 4=Scrambler).
When set to non-0xFF, `encryption_apply_channel_settings()` ignores codeplug byte.
API: `encryption_set_global_mode(uint8_t)`, `encryption_get_global_mode()`.
Radio menu calls these — GREEN saves, RED restores previous.
Both MK22 and STM32 encryption_state_t now include `scramble_id`, `global_mode_override`, and `uint16_t scramble_carrier_phase`.

### Scrambler IDs

8 user-selectable carrier frequencies via `encryption_set_scramble_id(1-8)`:
2.0, 2.4, 2.8, 3.2, 3.6, 4.0, 4.4, 4.8 kHz.
Square-wave frequency inversion — scramble=descramble (self-inverse).
`scrambler_process()` takes `scramble_id` parameter + `uint16_t*` phase.
MK22: single 3.2kHz carrier only (ponytail: flash budget).

### DMR voice data path (encryption injection points)

```
TX: Mic → I2S → codecEncodeBlock() → 9-byte AMBE block
    → [ENCRYPT: encryption_encrypt_voice_block] → deferredUpdateBuffer
    → HR-C6000 SPI (page 0x03) → AT1846S → RF

RX: RF → AT1846S → HR-C6000 SPI read (page 0x03, 27 bytes)
    → DMR_frame_buffer → [DECRYPT: encryption_decrypt_voice_frame]
    → codecDecode() → I2S → Speaker
```

### Key transfer protocol

CPS → radio via USB CDC-ACM (virtual COM):
```
['C'][cmd=7|8][key_slot][algorithm][key_len][name(16)][key_data(32)]  (53 bytes total)
```
- MK22: command 7 (`cpsHandleCommand` → case 7)
- STM32: command 8 (command 7 reserved for GPS)

Algorithm enum: `0=None, 1=ARC4, 2=AES-128, 3=AES-256, 4=Scrambler`

Firmware sends `'A'` ACK after key store on both platforms. **Fixed**: `return` after ACK prevents generic `'-'` fallthrough overwrite.

## CPS key management

### New files

- `cps/DMR/EncryptionKeyStore.cs` — 32-slot key store (32-byte keys), binary file persistence (`encryption_keys.bin`), builds 53-byte transfer packets
- `cps/DMR/EncryptForm.cs` — heavily modified (see below)

### EncryptForm.cs changes (2026-07-09)

**EncryptType enum**: trimmed to 3 values (None=0, ARC4=1, AES128=2). Scrambler handled via radio menu (Options→Encryption) — radio-side menu toggle. AES-256 removed — won't fit legacy 8-byte `keyList`.

**KeyLen enum**: 4 values (32/64/40/128-bit). No 256-bit.

**ComboBox items**: 3 types + 4 lengths, clamped in DispData() to handle stale values from old codeplugs.

**WriteKeysToRadio()**: full USB CDC-ACM protocol — PROGRA/PROG2 handshake → send each active key → ENDW. Auto-detects command byte (7/8) from model string.

**Persistence**: key store auto-saves to `encryption_keys.bin` on form close.

### Radio menu: Options → Encryption

New `menuEncryption.c` in `application/source/user_interface/`.
Menu items:
- **Enc Mode** — cycles Off→ARC4→AES-128→Scrambler→Off (AES hidden if `!ENC_HAS_AES`)
- **Scram ID** — cycles 1-8 (auto-switches to Scrambler mode)

Menu registration (3 arrays must stay in sync):
1. `menuSystem.h`: `MENU_ENCRYPTION` in `enum MENU_SCREENS` (after `MENU_SOUND`)
2. `menuSystem.c` `menuFunctions[]`: `{ menuEncryption, NULL, NULL, 0 }`
3. `menuSystem.c` `menuDataGlobal.data[]`: `NULL,// Encryption options`
4. `menuSystem.c` `optionsMenuItems[]`: `{ 274, MENU_ENCRYPTION }` (string offset 274 → encryption field)

Language strings: 7 new fields in `stringsTable_t` (17 bytes each):
`encryption`, `enc_mode`, `enc_off`, `enc_arc4`, `enc_aes128`, `enc_scrambler`, `scramble_id`.
22 language files updated. `LANGUAGE_TAG_VERSION` bumped to `{0,0,0,4}`.

### Legacy Encrypt struct (unchanged)

136 bytes at codeplug address 4976. `keyList` = 128 bytes / 16 slots = 8 bytes per key. Only ARC4 keys fit fully (max 8 bytes). AES-128 keys (16 bytes) truncated — full keys live in `encryption_keys.bin`.

## Critical integration files (modified from upstream OpenGD77)

**MK22** (`mk22-firmware/firmware/source/`):
- `hardware/HR-C6000.c` — TX encrypt, RX decrypt, keystream init, RX_END reset
- `functions/sound.c` — scrambler with `(int16_t*)(void*)` cast
- `main.c` — `encryption_init()` at boot
- `user_interface/uiChannelMode.c` — `encryption_apply_channel_settings()` in `loadChannelData()`
- `usb/usb_com.c` — command 7 key handler + ACK with return guard
- `include/encryption/encryption.h` — full state struct + API parity with STM32
- `source/encryption/encryption.c` — global override, scramble ID functions
- `source/encryption/scrambler.c` — unified signature, per-call phase state

**STM32** (`stm32-firmware/.../MDUV380_firmware/`):
- `application/source/hardware/HR-C6000.c` — TX encrypt, RX decrypt, keystream init, RX_END reset
- `application/source/functions/sound.c` — scrambler with `(int16_t*)(void*)` cast
- `Core/Src/main.c` — `encryption_init()` at boot
- `application/source/user_interface/uiChannelMode.c` — `encryption_apply_channel_settings()` on channel switch
- `application/source/user_interface/menuEncryption.c` — **new**: Options→Encryption menu (global mode override)
- `application/source/user_interface/menuSystem.c` — menu registration (4 arrays)
- `application/source/encryption/encryption.c` — `encryption_set_global_mode()`, override logic
- `application/source/encryption/scrambler.c` — 8 carrier frequencies (2.0-4.8kHz via `scramble_id`)
- `application/include/encryption/encryption.h` — `global_mode_override`, `scramble_id`, API
- `application/include/user_interface/languages/uiLanguage.h` — 7 new strings, version 0x04
- `application/source/usb/usb_com.c` — command 8 key handler + ACK with return guard (STM32 pattern: `usbComSendBuf`/`replyLength`)
- `application/include/dmr_codec/codec.h` — AMBE symbols renamed for GCC 15 compat
- `application/source/linkerdata/ambe_syms.s` — empty placeholder (PROVIDE in linker script handles AMBE symbols)
- `STM32F405VGTX_FLASH.ld` — PROVIDE for AMBE symbols at correct codec blob offsets with Thumb +1 bit
- `.cproject` — PLATFORM_STM32 in all 8 build configs

## USB HID vs CDC-ACM

MK22 firmware uses **USB CDC-ACM** (virtual COM port), NOT HID. The CPS `CodeplugComms.cs` sends raw bytes via USB bulk endpoints. `CrossPlatformHIDDevice.cs` is misnamed — it actually does raw libusb control/bulk transfers, not HID reports.

STM32 firmware also uses **USB CDC-ACM** (`usbd_cdc_if.c`, `usb_com.c` with `CDC_Transmit_FS`).

## DFU flasher for STM32

Python flasher `scripts/dfu_flash.py` — uses ctypes + libusb-1.0 (zero dependencies).
Implements DfuSe protocol: SET_ADDRESS → sector erase → download → manifest.
```bash
python3 scripts/dfu_flash.py stm32-firmware/.../build_dm1701/firmware.bin
```
Requires `/etc/udev/rules.d/99-stm32-dfu.rules` for non-root access:
```
SUBSYSTEM=="usb", ATTR{idVendor}=="0483", ATTR{idProduct}=="df11", MODE="0666"
```
DM-1701 DFU entry: hold PTT+SK1 while powering on, or SK2+Power.
DFU VID/PID: `0483:df11`. Flash base: **`0x0800C000`** (firmware linked at 0x0800C000; bootloader occupies 0x08000000–0x0800BFFF). Previously was incorrectly 0x08000000.

## Archive

Old firmware builds archived to `stm32-firmware/archive_old_builds/`:
- `firmware_stock_opengd77_no_encryption.*` — 507KB, stock OpenGD77 without codec blob (broken DMR audio)
