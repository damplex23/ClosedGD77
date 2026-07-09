# ClosedGD77 STM32 Firmware Makefile (TEMPLATE - for clean rebuilds)
# Builds DM-1701 firmware with encryption support
# Requires: arm-none-eabi-gcc (Arm GNU Toolchain 15.2+)
#
# NOTE: This is a template. Current build uses flat .o naming in build_dm1701/.
# For clean rebuild, delete build_dm1701/ first, then: make -j$(nproc)
# AMBE codec symbols resolved via PROVIDE() in STM32F405VGTX_FLASH.ld.
# ambe_syms.s is an empty placeholder - do NOT include in link.

TOOLCHAIN ?= arm-none-eabi-
CC = $(TOOLCHAIN)gcc
CXX = $(TOOLCHAIN)g++
AS = $(TOOLCHAIN)gcc -c
LD = $(TOOLCHAIN)gcc
OBJCOPY = $(TOOLCHAIN)objcopy
OBJDUMP = $(TOOLCHAIN)objdump
SIZE = $(TOOLCHAIN)size

BUILD_DIR = build_dm1701
TARGET = $(BUILD_DIR)/firmware

# ---- MCU flags ----
MCU = -mcpu=cortex-m4 -mthumb -mfloat-abi=hard -mfpu=fpv4-sp-d16
ARCH = -march=armv7e-m+fp

# ---- Defines ----
DEFINES = \
	-DPLATFORM_STM32 \
	-DPLATFORM_RT84_DM1701 \
	-DPLATFORM_VARIANT_DM1701 \
	-DSTM32F405xx \
	-DUSE_HAL_DRIVER \
	-DHAS_GPS \
	-DHAS_COLOURS

# ---- Include paths ----
INC_DIRS = \
	Core/Inc \
	Drivers/CMSIS/Include \
	Drivers/CMSIS/Device/ST/STM32F4xx/Include \
	Drivers/STM32F4xx_HAL_Driver/Inc \
	Middlewares/ST/STM32_USB_Device_Library/Class/CDC/Inc \
	Middlewares/ST/STM32_USB_Device_Library/Core/Inc \
	Middlewares/Third_Party/FreeRTOS/Source/include \
	Middlewares/Third_Party/FreeRTOS/Source/CMSIS_RTOS_V2 \
	Middlewares/Third_Party/FreeRTOS/Source/portable/GCC/ARM_CM4F \
	application/include \
	application/include/dmr_codec \
	application/include/encryption \
	application/include/functions \
	application/include/hardware \
	application/include/interfaces \
	application/include/io \
	application/include/usb \
	application/include/user_interface \
	application/include/user_interface/languages \
	SeggerRTT/Config \
	SeggerRTT/RTT \
	SeggerRTT/Syscalls \
	USB_DEVICE/App \
	USB_DEVICE/Target

INCLUDES = $(addprefix -I, $(INC_DIRS))

# ---- Compiler flags ----
CFLAGS = $(MCU) $(ARCH) $(DEFINES) $(INCLUDES) \
	-Os -g3 -Wall \
	-ffunction-sections -fdata-sections -fno-common \
	-std=gnu23

ASFLAGS = $(MCU) $(ARCH) $(DEFINES) $(INCLUDES) -g3

LDFLAGS = $(MCU) $(ARCH) \
	-specs=nosys.specs -specs=nano.specs \
	-Wl,--gc-sections \
	-Wl,-Map=$(TARGET).map \
	-T STM32F405VGTX_FLASH.ld

# ---- Source files ----
CORE_SRC = $(wildcard Core/Src/*.c)
HAL_SRC = $(wildcard Drivers/STM32F4xx_HAL_Driver/Src/*.c)
USB_LIB_SRC = $(wildcard Middlewares/ST/STM32_USB_Device_Library/Class/CDC/Src/*.c) \
              $(wildcard Middlewares/ST/STM32_USB_Device_Library/Core/Src/*.c)
FREERTOS_SRC = $(wildcard Middlewares/Third_Party/FreeRTOS/Source/*.c) \
               Middlewares/Third_Party/FreeRTOS/Source/portable/GCC/ARM_CM4F/port.c \
               Middlewares/Third_Party/FreeRTOS/Source/portable/MemMang/heap_4.c \
               Middlewares/Third_Party/FreeRTOS/Source/CMSIS_RTOS_V2/cmsis_os2.c
USB_DEV_SRC = $(wildcard USB_DEVICE/App/*.c) $(wildcard USB_DEVICE/Target/*.c)
SEGGER_SRC = SeggerRTT/RTT/SEGGER_RTT.c SeggerRTT/RTT/SEGGER_RTT_printf.c SeggerRTT/Syscalls/SEGGER_RTT_Syscalls_GCC.c
APP_SRC = $(wildcard application/source/*.c) \
          $(wildcard application/source/dmr_codec/*.c) \
          $(wildcard application/source/encryption/*.c) \
          $(wildcard application/source/functions/*.c) \
          $(wildcard application/source/hardware/*.c) \
          $(wildcard application/source/interfaces/*.c) \
          $(wildcard application/source/io/*.c) \
          $(wildcard application/source/usb/*.c) \
          $(wildcard application/source/user_interface/*.c)

# Exclude duplicate codec_interface.c (built separately without -fno-common workaround)
APP_SRC := $(filter-out %/codec_interface.c, $(APP_SRC))

SRC = $(CORE_SRC) $(HAL_SRC) $(USB_LIB_SRC) $(FREERTOS_SRC) $(USB_DEV_SRC) $(SEGGER_SRC) $(APP_SRC)

# ---- Object files ----
OBJ = $(SRC:%.c=$(BUILD_DIR)/%.o)

# Startup and special objects
STARTUP_SRC = Core/Startup/startup_stm32f405vgtx.s
STARTUP_OBJ = $(BUILD_DIR)/$(STARTUP_SRC:.s=.o)

# AMBE codec objects (pre-built blobs)
AMBE_SYMS_SRC = application/source/linkerdata/ambe_syms.s
AMBE_SYMS_OBJ = $(BUILD_DIR)/$(AMBE_SYMS_SRC:.s=.o)
CODEC_BIN = application/source/linkerdata/codec_bin_section_1.bin
CODEC_BIN_OBJ = $(BUILD_DIR)/codec_bin_section_1.o

# codec_interface_fixed.o: duplicate of codec_interface.o but without -fno-common
# The upstream build compiles codec_interface.c twice with different flags
CODEC_IFACE_FIXED_OBJ = $(BUILD_DIR)/codec_interface_fixed.o

# We exclude codec_interface.o from APP_SRC and build it separately
CODEC_IFACE_SRC = application/source/dmr_codec/codec_interface.c
CODEC_IFACE_OBJ = $(BUILD_DIR)/$(CODEC_IFACE_SRC:.c=.o)

ALL_OBJ = $(OBJ) $(STARTUP_OBJ) $(AMBE_SYMS_OBJ) $(CODEC_BIN_OBJ) $(CODEC_IFACE_FIXED_OBJ)
# NOTE: codec_interface.c compiled twice — the _fixed version is linked;
# the regular CODEC_IFACE_OBJ excluded (filtered from APP_SRC above).

# ---- Phony targets ----
.PHONY: all clean size

all: $(TARGET).bin $(TARGET).hex
	@$(SIZE) $(TARGET).elf

size: $(TARGET).elf
	@$(SIZE) $<

clean:
	rm -rf $(BUILD_DIR)

# ---- Link ----
$(TARGET).elf: $(ALL_OBJ) STM32F405VGTX_FLASH.ld
	@echo "  LD    $@"
	@mkdir -p $(dir $@)
	$(LD) $(LDFLAGS) $(ALL_OBJ) -o $@

$(TARGET).bin: $(TARGET).elf
	@echo "  BIN   $@"
	$(OBJCOPY) -O binary $< $@

$(TARGET).hex: $(TARGET).elf
	@echo "  HEX   $@"
	$(OBJCOPY) -O ihex $< $@

# ---- Compile C ----
$(BUILD_DIR)/%.o: %.c
	@echo "  CC    $<"
	@mkdir -p $(dir $@)
	$(CC) $(CFLAGS) -c $< -o $@

# ---- Compile startup assembly (explicit, avoids pattern clash with C) ----
$(BUILD_DIR)/Core/Startup/startup_stm32f405vgtx.o: Core/Startup/startup_stm32f405vgtx.s
	@echo "  AS    $<"
	@mkdir -p $(dir $@)
	$(CC) $(CFLAGS) -x assembler-with-cpp -c $< -o $@

# ---- Compile AMBE syms (empty placeholder, linker handles PROVIDE) ----
$(AMBE_SYMS_OBJ): $(AMBE_SYMS_SRC)
	@echo "  AS    $< (placeholder)"
	@mkdir -p $(dir $@)
	$(CC) $(ASFLAGS) -x assembler-with-cpp -c $< -o $@

# ---- Codec binary blob ----
$(CODEC_BIN_OBJ): $(CODEC_BIN)
	@echo "  OBJCOPY $<"
	@mkdir -p $(dir $@)
	$(OBJCOPY) -I binary -O elf32-littlearm -B arm $< $@
	$(OBJCOPY) --rename-section .data=.codec_bin_section_1 $@

# ---- codec_interface.c duplicate (required by upstream linker layout) ----
$(CODEC_IFACE_FIXED_OBJ): $(CODEC_IFACE_SRC)
	@echo "  CC    $< (fixed)"
	@mkdir -p $(dir $@)
	$(CC) $(CFLAGS) -c $< -o $@

-include $(ALL_OBJ:.o=.d)
