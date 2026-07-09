#!/usr/bin/env python3
"""Minimal STM32 DfuSe flasher using ctypes + libusb-1.0. No dependencies."""
import ctypes
import struct
import sys
import time

# libusb-1.0 bindings (just what we need)
libusb = ctypes.CDLL("libusb-1.0.so.0")

# Types
class libusb_device_descriptor(ctypes.Structure):
    _fields_ = [
        ("bLength", ctypes.c_uint8),
        ("bDescriptorType", ctypes.c_uint8),
        ("bcdUSB", ctypes.c_uint16),
        ("bDeviceClass", ctypes.c_uint8),
        ("bDeviceSubClass", ctypes.c_uint8),
        ("bDeviceProtocol", ctypes.c_uint8),
        ("bMaxPacketSize0", ctypes.c_uint8),
        ("idVendor", ctypes.c_uint16),
        ("idProduct", ctypes.c_uint16),
        ("bcdDevice", ctypes.c_uint16),
        ("iManufacturer", ctypes.c_uint8),
        ("iProduct", ctypes.c_uint8),
        ("iSerialNumber", ctypes.c_uint8),
        ("bNumConfigurations", ctypes.c_uint8),
    ]

# Constants
LIBUSB_SUCCESS = 0
LIBUSB_ENDPOINT_OUT = 0x00
LIBUSB_ENDPOINT_IN = 0x80
LIBUSB_REQUEST_TYPE_CLASS = 0x20
LIBUSB_RECIPIENT_INTERFACE = 0x01
LIBUSB_ERROR_TIMEOUT = -7

# DFU requests
DFU_DETACH = 0
DFU_DNLOAD = 1
DFU_UPLOAD = 2
DFU_GETSTATUS = 3
DFU_CLRSTATUS = 4
DFU_GETSTATE = 5
DFU_ABORT = 6

# DfuSe commands
DFUSE_CMD_SET_ADDRESS = 0x21
DFUSE_CMD_ERASE = 0x41

FLASH_BASE = 0x0800C000  # firmware linked at 0x0800C000 (bootloader at 0x08000000-0x0800BFFF)

def dfu_get_status(handle, interface):
    """Get DFU status. Returns (status, poll_timeout, state)"""
    buf = (ctypes.c_uint8 * 6)()
    ret = libusb.libusb_control_transfer(
        handle,
        LIBUSB_REQUEST_TYPE_CLASS | LIBUSB_RECIPIENT_INTERFACE | LIBUSB_ENDPOINT_IN,
        DFU_GETSTATUS, 0, interface,
        buf, 6, 1000
    )
    if ret < 0:
        raise OSError(f"GETSTATUS failed: {ret}")
    status = buf[0]
    poll_timeout = buf[1] | (buf[2] << 8) | (buf[3] << 16)
    state = buf[4]
    return status, poll_timeout, state

def dfu_clrstatus(handle, interface):
    """Clear DFU status."""
    ret = libusb.libusb_control_transfer(
        handle,
        LIBUSB_REQUEST_TYPE_CLASS | LIBUSB_RECIPIENT_INTERFACE | LIBUSB_ENDPOINT_OUT,
        DFU_CLRSTATUS, 0, interface,
        None, 0, 1000
    )
    if ret < 0:
        raise OSError(f"CLRSTATUS failed: {ret}")

def dfu_abort(handle, interface):
    """Abort ongoing operation."""
    ret = libusb.libusb_control_transfer(
        handle,
        LIBUSB_REQUEST_TYPE_CLASS | LIBUSB_RECIPIENT_INTERFACE | LIBUSB_ENDPOINT_OUT,
        DFU_ABORT, 0, interface,
        None, 0, 1000
    )
    if ret < 0:
        raise OSError(f"ABORT failed: {ret}")

def dfu_get_state(handle, interface):
    """Get DFU state."""
    buf = (ctypes.c_uint8 * 1)()
    ret = libusb.libusb_control_transfer(
        handle,
        LIBUSB_REQUEST_TYPE_CLASS | LIBUSB_RECIPIENT_INTERFACE | LIBUSB_ENDPOINT_IN,
        DFU_GETSTATE, 0, interface,
        buf, 1, 1000
    )
    if ret < 0:
        raise OSError(f"GETSTATE failed: {ret}")
    return buf[0]

def dfu_download(handle, interface, block_num, data):
    """Send DFU_DNLOAD. wValue=block_num (0 for DfuSe commands, sequence for firmware data)."""
    buf = (ctypes.c_uint8 * len(data))(*data) if data else None
    ret = libusb.libusb_control_transfer(
        handle,
        LIBUSB_REQUEST_TYPE_CLASS | LIBUSB_RECIPIENT_INTERFACE | LIBUSB_ENDPOINT_OUT,
        DFU_DNLOAD, block_num, interface,
        buf, len(data) if data else 0, 5000
    )
    if ret < 0:
        raise OSError(f"DNLOAD failed: {ret}")
    return ret

def dfu_wait_idle(handle, interface):
    """Poll status until device is idle (not in dnbusy state)."""
    for _ in range(100):
        status, timeout_ms, state = dfu_get_status(handle, interface)
        # state 2 = dfuIDLE, state 5 = dfuDNLOAD_IDLE, state 6 = dfuMANIFEST_SYNC
        if state in (2, 5, 6, 7):
            return status, state
        if status != 0:
            # Error - clear and retry
            dfu_clrstatus(handle, interface)
            continue
        if timeout_ms:
            time.sleep(max(timeout_ms / 1000.0, 0.01))
        else:
            time.sleep(0.01)
    raise TimeoutError("Device stuck in busy state")

def dfuse_set_address(handle, interface, addr):
    """DfuSe SET_ADDRESS command."""
    data = struct.pack("<BI", DFUSE_CMD_SET_ADDRESS, addr)
    dfu_download(handle, interface, 0, data)
    status, state = dfu_wait_idle(handle, interface)
    if state not in (2, 5, 6):
        raise OSError(f"SET_ADDRESS: unexpected state {state}, status={status}")

def dfuse_erase_page(handle, interface, addr):
    """DfuSe ERASE command for sector containing addr."""
    data = struct.pack("<BI", DFUSE_CMD_ERASE, addr)
    dfu_download(handle, interface, 0, data)
    status, state = dfu_wait_idle(handle, interface)
    if state not in (2, 5, 6):
        raise OSError(f"ERASE 0x{addr:08X}: unexpected state {state}, status={status}")

def dfuse_download_firmware(handle, interface, fw_data, xfer_size):
    """Download firmware binary in blocks."""
    total = len(fw_data)
    block_num = 0
    offset = 0

    while offset < total:
        chunk = fw_data[offset:offset + xfer_size]
        # wBlockNum = (block_num + 2) per DfuSe spec (0/1 reserved for commands)
        w_value = block_num + 2
        dfu_download(handle, interface, w_value, chunk)
        status, state = dfu_wait_idle(handle, interface)

        offset += len(chunk)
        block_num += 1
        pct = min(100, offset * 100 // total)
        print(f"\r  Writing: {offset}/{total} ({pct}%)", end="", flush=True)

    print()
    # Final zero-length DNLOAD to signal end of transfer
    dfu_download(handle, interface, block_num + 2, b"")
    try:
        status, state = dfu_wait_idle(handle, interface)
        print(f"  Manifest phase: state={state}, status={status}")
    except OSError:
        # Device reboots after manifest — USB disconnect is expected
        print("  Manifest phase: device rebooting (USB disconnect expected)")

def main():
    if len(sys.argv) < 2:
        print(f"Usage: {sys.argv[0]} firmware.bin [mass_erase=0|1]")
        sys.exit(1)

    fw_path = sys.argv[1]
    mass_erase = int(sys.argv[2]) if len(sys.argv) > 2 else 0

    with open(fw_path, "rb") as f:
        fw_data = f.read()
    print(f"Firmware: {fw_path} ({len(fw_data)} bytes)")

    # Init libusb
    ctx = ctypes.c_void_p()
    libusb.libusb_init(ctypes.byref(ctx))

    # Open DFU device
    handle = libusb.libusb_open_device_with_vid_pid(ctx, 0x0483, 0xDF11)
    if not handle:
        print("ERROR: DFU device 0483:df11 not found")
        libusb.libusb_exit(ctx)
        sys.exit(1)

    print("Device found: 0483:df11 STM32 DFU")

    try:
        # Claim interface 0
        ret = libusb.libusb_claim_interface(handle, 0)
        if ret == LIBUSB_ERROR_TIMEOUT:
            # Try detaching kernel driver
            if libusb.libusb_kernel_driver_active(handle, 0):
                libusb.libusb_detach_kernel_driver(handle, 0)
            ret = libusb.libusb_claim_interface(handle, 0)
        if ret != LIBUSB_SUCCESS:
            # If already claimed, continue (might be in dfuIDLE)
            pass

        # Get DFU state and ensure we're in idle
        state = dfu_get_state(handle, 0)

        # If in error, clear it
        if state == 10:  # dfuERROR
            dfu_clrstatus(handle, 0)
            state = dfu_get_state(handle, 0)

        # If stuck in DNLOAD-SYNC or other non-idle state, abort
        if state not in (2, 5, 6):  # dfuIDLE, dfuDNLOAD-IDLE, dfuMANIFEST-SYNC
            dfu_abort(handle, 0)
            time.sleep(0.1)
            state = dfu_get_state(handle, 0)

        print(f"  Initial state: {state}")

        # Determine transfer size from USB descriptor (try 2048, common for STM32F4)
        # STM32F4 ROM bootloader typically supports up to 2048
        xfer_size = 1024  # conservative, works even on problematic setups

        # Try setting address first (must be in IDLE)
        print(f"  Setting address: 0x{FLASH_BASE:08X}")
        dfuse_set_address(handle, 0, FLASH_BASE)

        # Erase
        if mass_erase:
            print("  Full wipe — erasing all sectors from 0x0800C000 to end of flash...")
            # Erase ALL sectors from firmware start to end of 1MB flash
            SECTORS = [
                (0x0800C000, 16384),
                (0x08010000, 65536), (0x08020000, 131072), (0x08040000, 131072),
                (0x08060000, 131072), (0x08080000, 131072), (0x080A0000, 131072),
                (0x080C0000, 131072), (0x080E0000, 131072),
            ]
            for saddr, ssize in SECTORS:
                print(f"  Erasing sector at 0x{saddr:08X}...")
                dfuse_erase_page(handle, 0, saddr)
        else:
            # Erase sectors covering firmware (starts after bootloader at 0x0800C000)
            fw_end = FLASH_BASE + len(fw_data)
            # STM32F405 sector boundaries (skip bootloader sectors 0-2)
            SECTORS = [
                (0x0800C000, 16384),
                (0x08010000, 65536), (0x08020000, 131072), (0x08040000, 131072),
                (0x08060000, 131072), (0x08080000, 131072), (0x080A0000, 131072),
                (0x080C0000, 131072), (0x080E0000, 131072),
            ]
            for saddr, ssize in SECTORS:
                if saddr >= fw_end:
                    break
                print(f"  Erasing sector at 0x{saddr:08X}...")
                dfuse_erase_page(handle, 0, saddr)

        # Re-set address after erase
        print(f"  Setting address: 0x{FLASH_BASE:08X}")
        dfuse_set_address(handle, 0, FLASH_BASE)

        # Download firmware
        print(f"  Downloading firmware ({len(fw_data)} bytes, block size {xfer_size})...")
        dfuse_download_firmware(handle, 0, fw_data, xfer_size)

        # Detach to reboot into firmware
        print("  Rebooting into firmware...")
        ret = libusb.libusb_control_transfer(
            handle,
            LIBUSB_REQUEST_TYPE_CLASS | LIBUSB_RECIPIENT_INTERFACE | LIBUSB_ENDPOINT_OUT,
            DFU_DETACH, 1000, 0, None, 0, 1000
        )
        # Detach may fail as device reboots - that's OK

        print("SUCCESS: Firmware flashed!")
        print("Radio should reboot into new firmware within seconds.")

    finally:
        libusb.libusb_release_interface(handle, 0)
        libusb.libusb_close(handle)
        libusb.libusb_exit(ctx)

if __name__ == "__main__":
    main()
