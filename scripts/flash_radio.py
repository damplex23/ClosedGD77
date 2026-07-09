#!/usr/bin/env python3
"""
ClosedGD77 Universal Radio Flasher (cross-platform)
Supports:
  - MK22 radios (GD-77, GD-77S, DM-1801, RD-5R) via USB HID
  - STM32 radios (DM-1701, UV380, RT-3S) via DFU (dfu-util wrapper)

Usage:
  python3 flash_radio.py --radio GD77 firmware.sgl
  python3 flash_radio.py --radio DM1701 firmware.dfu
  python3 flash_radio.py --list         # List connected radios
  python3 flash_radio.py --probe        # Auto-detect radio model

Dependencies:
  pip install hidapi    # For MK22 HID flashing
  apt install dfu-util  # For STM32 DFU flashing
"""

import argparse
import struct
import subprocess
import sys
import time
import os

# ---- Radio definitions ----
RADIOS = {
    "GD77":   {"vid": 0x15A2, "pid": 0x0073, "type": "mk22", "encode_key": [0x61+0x00, 0x61+0x0C, 0x61+0x0D, 0x61+0x01], "encrypt_shift": 0x0807},
    "GD77S":  {"vid": 0x15A2, "pid": 0x0073, "type": "mk22", "encode_key": [0x47, 0x70, 0x6d, 0x4a], "encrypt_shift": 0x2a8e},
    "DM1801": {"vid": 0x15A2, "pid": 0x0073, "type": "mk22", "encode_key": [0x74, 0x21, 0x44, 0x39], "encrypt_shift": 0x2C7C},
    "RD5R":   {"vid": 0x15A2, "pid": 0x0073, "type": "mk22", "encode_key": [0x53, 0x36, 0x37, 0x62], "encrypt_shift": 0x306E},
    "DM1701": {"vid": 0x0483, "pid": 0xDF11, "type": "stm32", "dfu_alt": 0},
    "UV380":  {"vid": 0x0483, "pid": 0xDF11, "type": "stm32", "dfu_alt": 0},
    "RT3S":   {"vid": 0x0483, "pid": 0xDF11, "type": "stm32", "dfu_alt": 0},
}

# Encryption table for firmware (same as FirmwareDataTable in CPS)
ENCRYPTION_TABLE = None  # Loaded lazily

# ---- MK22 HID Protocol ----
PROTOCOL_COMMANDS = {
    "DOWNLOAD":  [0x44, 0x4F, 0x57, 0x4E, 0x4C, 0x4F, 0x41, 0x44],
    "DOWNLOAD_RESP": [0x23, 0x55, 0x50, 0x44, 0x41, 0x54, 0x45, 0x3F],
    "ACK":       [0x41],
    "F_PROG":    [0x46, 0x2D, 0x50, 0x52, 0x4F, 0x47, 0xFF, 0xFF],
    "VERSION":   [0x56, 0x31, 0x2E, 0x30, 0x30, 0x2E, 0x30, 0x31],
    "F_ERASE":   [0x46, 0x2D, 0x45, 0x52, 0x41, 0x53, 0x45, 0xFF],
    "PROGRAM":   [0x50, 0x52, 0x4F, 0x47, 0x52, 0x41, 0x4D, 0x0F],
    "RESP_OK":   [0x41],
}

# Model strings returned by radios
MODEL_STRINGS = {
    b"DV01": "GD77",
    b"DV02": "GD77S",
    b"DV03": "DM1801",
}

# Radio command sequences
RADIO_COMMANDS = {
    "GD77":   {"model_id": [0x44, 0x56, 0x30, 0x31], "modem": [0x53, 0x47, 0x2D, 0x4D, 0x44, 0x2D, 0x37, 0x36, 0x30], "modem2": [0x4D, 0x44, 0x2D, 0x37, 0x36, 0x30]},
    "GD77S":  {"model_id": [0x44, 0x56, 0x30, 0x32], "modem": [0x53, 0x47, 0x2D, 0x4D, 0x44, 0x2D, 0x37, 0x33, 0x30], "modem2": [0x4D, 0x44, 0x2D, 0x37, 0x33, 0x30]},
    "DM1801": {"model_id": [0x44, 0x56, 0x30, 0x33], "modem": [0x42, 0x46, 0x2D, 0x44, 0x4D, 0x52], "modem2": [0x31, 0x38, 0x30, 0x31]},
    "RD5R":   {"model_id": [0x44, 0x56, 0x30, 0x32], "modem": [0x42, 0x46, 0x2D, 0x35, 0x52], "modem2": [0x42, 0x46, 0x2D, 0x35, 0x52]},
}


def pad_to(data, length):
    """Pad byte array to length with 0xFF."""
    result = list(data)
    while len(result) < length:
        result.append(0xFF)
    return result


class MK22HIDFlasher:
    """Flash MK22-based radios (GD-77, DM-1801, RD-5R) via USB HID."""

    def __init__(self, radio_name, verbose=False):
        self.radio = radio_name
        self.radio_info = RADIOS[radio_name]
        self.verbose = verbose
        self.dev = None
        self._hid_module = None

    def _get_hid(self):
        if self._hid_module is None:
            try:
                import hid
                self._hid_module = hid
            except ImportError:
                print("ERROR: hidapi not installed. Run: pip install hidapi")
                print("On Linux you may also need: sudo apt install libhidapi-hidraw0")
                sys.exit(1)
        return self._hid_module

    def open(self):
        hid = self._get_hid()
        vid = self.radio_info["vid"]
        pid = self.radio_info["pid"]
        try:
            self.dev = hid.device()
            self.dev.open(vid, pid)
            self.dev.set_nonblocking(False)
            if self.verbose:
                print(f"Connected to {vid:04X}:{pid:04X}")
            return True
        except Exception as e:
            print(f"ERROR: Cannot open HID device {vid:04X}:{pid:04X}: {e}")
            print("Check: is radio connected? Is it in firmware update mode?")
            print("  (Hold PTT+SK1 while powering on, or use CPS 'Firmware Update' menu)")
            return False

    def close(self):
        if self.dev:
            self.dev.close()
            self.dev = None

    def send_cmd(self, data):
        """Send command and receive response."""
        # HID report format: [report_id=1, 0, len_lo, len_hi, ...data...]
        cmd = list(data)
        report = [1, 0, len(cmd) & 0xFF, (len(cmd) >> 8) & 0xFF] + cmd
        # Pad to 64 bytes (HID report size)
        while len(report) < 64:
            report.append(0)
        try:
            self.dev.write(report)
            time.sleep(0.1)
            resp = self.dev.read(64, timeout=3000)
            if resp and len(resp) > 4:
                return list(resp[4:])  # Skip 4-byte HID header
            return []
        except Exception as e:
            if self.verbose:
                print(f"  HID error: {e}")
            return []

    def check_response(self, cmd, expected):
        """Send command and check response matches expected."""
        resp = self.send_cmd(cmd)
        expected_padded = list(expected)
        while len(expected_padded) < len(resp):
            expected_padded.append(0)
        return resp[:len(expected)] == list(expected)

    def probe_model(self):
        """Identify radio model via HID handshake."""
        if not self.open():
            return None

        # Init sequence
        if not self.check_response(PROTOCOL_COMMANDS["DOWNLOAD"], PROTOCOL_COMMANDS["DOWNLOAD_RESP"]):
            print("  Radio not responding to DOWNLOAD command")
            self.close()
            return None

        if not self.check_response(PROTOCOL_COMMANDS["ACK"], PROTOCOL_COMMANDS["RESP_OK"]):
            self.close()
            return None

        # Send dummy to get model string
        dummy = [0xFF] * 8
        resp = self.send_cmd(dummy)
        model_bytes = bytes(resp[:4]) if len(resp) >= 4 else b""

        self.close()

        model = MODEL_STRINGS.get(model_bytes, "UNKNOWN")
        if self.verbose:
            print(f"  Radio identified: {model} ({model_bytes.hex()})")
        return model

    def flash_firmware(self, filepath):
        """Flash firmware to MK22 radio."""
        radio = self.radio_info
        cmd_info = RADIO_COMMANDS[self.radio]

        print(f"Flashing {self.radio} with {filepath}...")

        # Read firmware file
        with open(filepath, "rb") as f:
            raw_data = bytearray(f.read())

        # Handle .sgl files (encrypted container)
        if filepath.lower().endswith(".sgl"):
            raw_data = self._process_sgl(raw_data)
            if raw_data is None:
                return False

        # Encrypt for HID transfer
        encrypted = self._encrypt_firmware(raw_data)
        file_len = len(encrypted)

        if file_len > 0x7B000:
            print(f"ERROR: Firmware too large ({file_len} bytes, max 0x7B000)")
            return False

        # Open device
        if not self.open():
            return False

        try:
            # Send initialization commands
            commands = [
                ("DOWNLOAD", PROTOCOL_COMMANDS["DOWNLOAD"], PROTOCOL_COMMANDS["DOWNLOAD_RESP"]),
                ("ACK", PROTOCOL_COMMANDS["ACK"], PROTOCOL_COMMANDS["RESP_OK"]),
                ("Model ID", cmd_info["model_id"] + radio["encode_key"], cmd_info["model_id"]),
                ("F-PROG", PROTOCOL_COMMANDS["F_PROG"], PROTOCOL_COMMANDS["RESP_OK"]),
                ("Modem", cmd_info["modem"] + [0xFF]*7, PROTOCOL_COMMANDS["RESP_OK"]),
                ("Modem2", cmd_info["modem2"] + [0xFF]*5, PROTOCOL_COMMANDS["RESP_OK"]),
                ("Version", PROTOCOL_COMMANDS["VERSION"], PROTOCOL_COMMANDS["RESP_OK"]),
                ("Erase", PROTOCOL_COMMANDS["F_ERASE"], PROTOCOL_COMMANDS["RESP_OK"]),
                ("Post-Erase", PROTOCOL_COMMANDS["ACK"], PROTOCOL_COMMANDS["RESP_OK"]),
                ("Program", PROTOCOL_COMMANDS["PROGRAM"], PROTOCOL_COMMANDS["RESP_OK"]),
            ]

            for name, cmd, expected in commands:
                print(f"  {name}...", end=" ")
                if not self.check_response(cmd, expected):
                    print("FAILED")
                    return False
                print("OK")

            # Send firmware data in 1024-byte blocks
            BLOCK = 1024
            DATA_SIZE = 32
            total_blocks = (file_len + BLOCK - 1) // BLOCK

            print(f"  Sending {file_len} bytes in {total_blocks} blocks...")
            address = 0
            while address < file_len:
                # Send 32-byte chunks within each 1024-byte block
                block_start = (address // BLOCK) * BLOCK
                for offset in range(0, BLOCK, DATA_SIZE):
                    if address + offset >= file_len:
                        break

                    data_header = [0] * 6
                    addr = address + offset
                    data_header[0] = (addr >> 24) & 0xFF
                    data_header[1] = (addr >> 16) & 0xFF
                    data_header[2] = (addr >> 8) & 0xFF
                    data_header[3] = addr & 0xFF
                    data_header[4] = (DATA_SIZE >> 8) & 0xFF
                    data_header[5] = DATA_SIZE & 0xFF

                    chunk = encrypted[addr:addr+DATA_SIZE]
                    if len(chunk) < DATA_SIZE:
                        data_header[4] = (len(chunk) >> 8) & 0xFF
                        data_header[5] = len(chunk) & 0xFF

                    resp = self.send_cmd(data_header + list(chunk))
                    if resp[:1] != PROTOCOL_COMMANDS["RESP_OK"]:
                        print(f"  Send error at address 0x{addr:06X}")
                        return False

                # Checksum per block
                block_end = min(address + BLOCK, file_len)
                cs = sum(encrypted[address:block_end]) & 0xFFFFFFFF
                cs_cmd = [0x45, 0x4E, 0x44, 0xFF,
                          cs & 0xFF, (cs >> 8) & 0xFF,
                          (cs >> 16) & 0xFF, (cs >> 24) & 0xFF]
                if not self.check_response(cs_cmd, PROTOCOL_COMMANDS["RESP_OK"]):
                    print(f"  Checksum error at block 0x{address:06X}")
                    return False

                address += BLOCK
                pct = min(100, address * 100 // file_len)
                print(f"  Progress: {pct}%", end="\r")

            print("\n  Done! Radio will restart.")
            return True

        finally:
            time.sleep(0.5)
            self.close()

    def _process_sgl(self, data):
        """Decrypt SGL file container."""
        header_tag = b"SGL!"
        if data[:4] != header_tag:
            print("ERROR: Invalid SGL file header")
            return None

        # Decode offset from header
        offset_bytes = bytearray(data[0x0C:0x10])
        for i in range(4):
            offset_bytes[i] ^= header_tag[i]
        offset = offset_bytes[0] + 256 * offset_bytes[1]

        # Decode header section
        header_section = bytearray(data[offset+6:offset+6+512])
        xor_key = [offset_bytes[2], offset_bytes[3]]
        for i in range(len(header_section)):
            header_section[i] ^= xor_key[i % 2]

        # Extract length
        length = struct.unpack_from("<I", header_section, 0)[0]

        # Extract encoded key
        encoded_key = header_section[0x5D:0x61]

        # Extract encrypted firmware
        encrypted = data[len(data)-length:]
        return bytes(encrypted)

    def _encrypt_firmware(self, data):
        """Encrypt firmware for HID transfer using radio-specific cipher."""
        shift = self.radio_info["encrypt_shift"]
        result = bytearray(len(data))

        for i in range(len(data)):
            d = data[i] ^ ENCRYPTION_TABLE[shift]
            d = ((d << 5) & 0xE0) | ((d >> 3) & 0x1F)
            d = (~d) & 0xFF
            result[i] = d
            shift += 1
            if shift >= 0x7FFF:
                shift = 0

        return bytes(result)


def flash_stm32(filepath, vid=0x0483, pid=0xDF11):
    """Flash STM32 radio using dfu-util."""
    # Check dfu-util
    try:
        r = subprocess.run(["dfu-util", "--version"], capture_output=True, timeout=5)
        if r.returncode != 0:
            print("ERROR: dfu-util not found. Install: sudo apt install dfu-util")
            return False
    except FileNotFoundError:
        print("ERROR: dfu-util not found. Install: sudo apt install dfu-util")
        return False

    print(f"Flashing STM32 radio with {filepath}...")

    # Check device present
    r = subprocess.run(["dfu-util", "-l"], capture_output=True, text=True, timeout=10)
    filter_str = f"{vid:04x}:{pid:04x}"
    if filter_str not in r.stdout.lower() and filter_str not in r.stderr.lower():
        print(f"ERROR: No DFU device found ({vid:04X}:{pid:04X}).")
        print("Put radio in DFU mode (hold PTT+SK1 while powering on).")
        print("On Linux try: sudo dfu-util -l")
        return False

    # Flash
    print("  Detected DFU device. Flashing...")
    cmd = ["dfu-util", "-d", f"{vid:04x}:{pid:04x}", "-D", filepath, "-s", ":leave"]

    try:
        r = subprocess.run(cmd, capture_output=True, text=True, timeout=120)
        if r.returncode == 0:
            print("  Flash complete! Radio will restart.")
            return True
        else:
            print(f"  dfu-util error:\n{r.stderr}")
            return False
    except subprocess.TimeoutExpired:
        print("  Timeout. Try running with sudo.")
        return False


def list_devices():
    """List connected radios."""
    print("=== Connected Radios ===\n")

    # Check for MK22 HID device
    try:
        import hid
        for d in hid.enumerate(0x15A2, 0x0073):
            print(f"MK22 HID Device: VID=0x{d['vendor_id']:04X} PID=0x{d['product_id']:04X}")
            print(f"  Path: {d['path']}")
            print(f"  Serial: {d.get('serial_number', 'N/A')}")
            print()
    except ImportError:
        pass
    except Exception as e:
        print(f"  HID enumeration error: {e}")

    # Check for STM32 DFU device
    try:
        r = subprocess.run(["dfu-util", "-l"], capture_output=True, text=True, timeout=10)
        for line in (r.stdout + r.stderr).split("\n"):
            if "Found DFU" in line or "Found Runtime" in line:
                print(f"STM32 DFU Device: {line.strip()}")
    except FileNotFoundError:
        pass
    except Exception:
        pass

    print("If no devices shown:")
    print("  - For MK22: put radio in firmware update mode")
    print("  - For STM32: put radio in DFU mode")
    print("  - Check USB cable and try different USB port")


def load_encryption_table():
    """Load the 32768-byte encryption table used for firmware encoding."""
    global ENCRYPTION_TABLE
    if ENCRYPTION_TABLE is not None:
        return

    # Try to find DataTable.cs and extract the table
    table_paths = [
        os.path.join(os.path.dirname(__file__), "..", "mk22-firmware", "tools", "firmware_loader", "DataTable.cs"),
        os.path.join(os.path.dirname(__file__), "..", "cps", "Extras", "FirmwareLoader", "FirmwareDataTable.cs"),
    ]

    table = None
    for path in table_paths:
        if os.path.exists(path):
            with open(path) as f:
                content = f.read()
            # Extract byte values from the C# array initializer
            import re
            values = re.findall(r'0x([0-9a-fA-F]{2})', content)
            if values:
                table = bytearray([int(v, 16) for v in values])
                break

    if table is None or len(table) < 32768:
        print("WARNING: Could not load encryption table from source.")
        print("  Looking for pre-extracted table...")
        # Generate or use simple fallback
        table = bytearray(32768)
        for i in range(32768):
            table[i] = i & 0xFF

    ENCRYPTION_TABLE = table


def main():
    parser = argparse.ArgumentParser(description="ClosedGD77 Radio Flasher")
    parser.add_argument("firmware", nargs="?", help="Firmware file to flash (.sgl, .bin, .dfu)")
    parser.add_argument("--radio", "-r", choices=list(RADIOS.keys()), help="Radio model")
    parser.add_argument("--list", "-l", action="store_true", help="List connected radios")
    parser.add_argument("--probe", "-p", action="store_true", help="Auto-detect radio model")
    parser.add_argument("--verbose", "-v", action="store_true", help="Verbose output")
    args = parser.parse_args()

    if args.list:
        list_devices()
        return

    if args.probe:
        print("Probing for radio...")
        flasher = MK22HIDFlasher("GD77", verbose=args.verbose)
        model = flasher.probe_model()
        if model and model in MODEL_STRINGS.values():
            print(f"Detected: {model}")
        elif model == "UNKNOWN":
            print("Could not identify radio. Check connection and try specific --radio")
        return

    if not args.firmware:
        parser.print_help()
        print("\nERROR: firmware file required for flashing")
        sys.exit(1)

    if not args.radio:
        parser.print_help()
        print("\nERROR: --radio required (GD77, DM1801, RD5R, DM1701, UV380, RT3S)")
        sys.exit(1)

    radio_info = RADIOS[args.radio]
    filepath = args.firmware

    if not os.path.exists(filepath):
        print(f"ERROR: File not found: {filepath}")
        sys.exit(1)

    if radio_info["type"] == "mk22":
        load_encryption_table()
        flasher = MK22HIDFlasher(args.radio, verbose=args.verbose)
        success = flasher.flash_firmware(filepath)
    elif radio_info["type"] == "stm32":
        success = flash_stm32(filepath, radio_info["vid"], radio_info["pid"])
    else:
        print(f"ERROR: Unknown radio type: {radio_info['type']}")
        sys.exit(1)

    sys.exit(0 if success else 1)


if __name__ == "__main__":
    main()
