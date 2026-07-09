/*
 * ClosedGD77: AMBE codec entry points.
 *
 * Symbols are now defined via PROVIDE() in the linker script (STM32F405VGTX_FLASH.ld)
 * at the correct absolute offsets within the Radioddity binary blob at 0x0807537C:
 *   AMBE_ENCODE_SYM     = 0x0807537C + 0x130
 *   AMBE_ENCODE_ECC_SYM = 0x0807537C + 0x4e8
 *   AMBE_DECODE_SYM     = 0x0807537C + 0x5d8
 *
 * This file is kept as an empty placeholder — the build system still references it
 * but the linker resolves symbols via PROVIDE instead of these stubs.
 */
