/*
 * Core encryption module — key storage, state management, dispatch
 *
 * Routes voice frame crypto to the appropriate cipher (ARC4, AES, scrambler).
 * Manages key table in NVM and runtime encryption state per channel.
 */

#include <stdint.h>
#include <stdbool.h>
#include <string.h>
#include "encryption/encryption.h"

// ---- Forward declarations of cipher implementations ----
void arc4_superframe_init(uint8_t key_id, uint32_t iv, bool is_tx);
void arc4_encrypt_block(uint8_t *data, uint8_t len);
void arc4_decrypt_frame(uint8_t *data, uint8_t len);

// AES is compiled conditionally (STM32 only)
#if ENC_HAS_AES
void aes_ctr_init(uint8_t key_id, uint32_t iv, bool is_tx);
void aes_ctr_encrypt_block(uint8_t *data, uint8_t len);
void aes_ctr_decrypt_frame(uint8_t *data, uint8_t len);
#endif

// Analog scrambler
void scrambler_process(int16_t *samples, uint16_t count, uint16_t *phase, uint8_t scramble_id);

// ---- Global encryption state ----
static encryption_state_t enc_state;
static encryption_key_t enc_keys[ENC_MAX_KEY_SLOTS];
static uint8_t enc_key_count = 0;
static bool enc_module_initialized = false;

// ---- LFSR for IV generation (32-bit, maximal length polynomial) ----
// Polynomial: x^32 + x^22 + x^2 + x + 1 (used in DMR/MotoTRBO)
static uint32_t iv_lfsr = 0xDEADBEEF; // seed (replaced with HR-C6000 entropy on first call)

static uint32_t lfsr_advance(void)
{
    uint32_t lsb = iv_lfsr & 1;
    iv_lfsr >>= 1;
    if (lsb) {
        iv_lfsr ^= 0x80200003; // ROL of the polynomial bits
    }
    return iv_lfsr;
}

// ---- Lifecycle ----

void encryption_init(void)
{
    if (enc_module_initialized) {
        return;
    }

    memset(&enc_state, 0, sizeof(enc_state));
    memset(enc_keys, 0, sizeof(enc_keys));
    enc_state.global_mode_override = 0xFF; // default: use codeplug, no menu override
    enc_key_count = 0;

    // Initialize with default empty key (slot 0, key_id 0, algorithm NONE)
    enc_keys[0].key_id = 0;
    enc_keys[0].algorithm = ENC_ALGO_NONE;
    enc_keys[0].key_length = 0;
    strcpy(enc_keys[0].key_name, "No Encryption");
    enc_key_count = 1;

    // Seed LFSR — ideally use HR-C6000 register noise or ADC noise
    // For now, use a compile-time seed that will produce different
    // values each power-on due to boot timing variations captured later
    encryption_generate_iv();

    enc_module_initialized = true;
}

void encryption_reset_per_call(void)
{
    enc_state.pi_header_sent = false;
    enc_state.superframe_counter = 0;
    enc_state.rx_keystream_ready = false;
    enc_state.tx_keystream_ready = false;
}

// ---- Per-channel configuration ----

void encryption_set_enabled(bool enabled)
{
    enc_state.enabled = enabled;
}

void encryption_set_key_id(uint8_t key_id)
{
    if (key_id < ENC_MAX_KEY_SLOTS) {
        enc_state.active_key_id = key_id;
        enc_state.active_algorithm = enc_keys[key_id].algorithm;
    }
}

void encryption_set_algorithm(uint8_t algo)
{
    enc_state.active_algorithm = algo;
}

void encryption_set_analog_scramble(bool enabled)
{
    enc_state.analog_scramble_enabled = enabled;
    if (!enabled)
    {
        enc_state.scramble_id = 0;
    }
}

void encryption_set_scramble_id(uint8_t id)
{
    if (id > 8)
    {
        id = 0;
    }
    enc_state.scramble_id = id;
    enc_state.analog_scramble_enabled = (id > 0);
    enc_state.scramble_carrier_phase = 0; // reset phase on ID change
}

uint8_t encryption_get_scramble_id(void)
{
    return enc_state.scramble_id;
}

// ---- Global mode override (radio menu) ----

void encryption_set_global_mode(uint8_t mode)
{
    if (mode == 0xFF)
    {
        enc_state.global_mode_override = 0xFF;
        return;
    }

    enc_state.global_mode_override = mode;

    if (mode == 0) // Off
    {
        enc_state.enabled = false;
        enc_state.analog_scramble_enabled = false;
        enc_state.scramble_id = 0;
        return;
    }

    if (mode == ENC_ALGO_SCRAMBLER) // 4 = Scrambler
    {
        enc_state.enabled = false;
        enc_state.active_algorithm = ENC_ALGO_SCRAMBLER;
        if (enc_state.scramble_id == 0)
        {
            enc_state.scramble_id = 4;  // default 3.2kHz
        }
        enc_state.analog_scramble_enabled = true;
        enc_state.scramble_carrier_phase = 0;
        return;
    }

    // DMR modes: ARC4
    enc_state.enabled = true;
    enc_state.active_algorithm = mode;
    enc_state.analog_scramble_enabled = false;
    enc_state.scramble_id = 0;
    enc_state.rx_keystream_ready = false;
    enc_state.tx_keystream_ready = false;
}

uint8_t encryption_get_global_mode(void)
{
    return enc_state.global_mode_override;
}

bool encryption_is_enabled(void)
{
    return enc_state.enabled && (enc_state.active_algorithm != ENC_ALGO_NONE);
}

// ponytail: expose state for HR-C6000 interrupt handler to check keystream ready
// without going through full function call overhead
const encryption_state_t* encryption_get_state(void)
{
    return &enc_state;
}

// Apply encryption settings from channel codeplug data
void encryption_apply_channel_settings(uint8_t encrypt_field)
{
    // Global menu override takes precedence
    if (enc_state.global_mode_override != 0xFF)
    {
        encryption_set_global_mode(enc_state.global_mode_override);
        return;
    }

    if (encrypt_field == 0 || encrypt_field == ENC_ALGO_NONE)
    {
        enc_state.enabled = false;
        return;
    }

    enc_state.enabled = true;
    enc_state.active_key_id = encrypt_field & 0x1F;           // lower 5 bits = key ID
    enc_state.active_algorithm = (encrypt_field >> 5) & 0x07; // upper 3 bits = algorithm

    // Validate algorithm
    if (enc_state.active_algorithm > ENC_ALGO_SCRAMBLER)
    {
        enc_state.active_algorithm = ENC_ALGO_ARC4; // fallback
    }

    // Analog scrambler is a channel-level toggle separate from DMR encryption
    enc_state.analog_scramble_enabled = (enc_state.active_algorithm == ENC_ALGO_SCRAMBLER);
}

uint8_t encryption_get_active_key_id(void)
{
    return enc_state.active_key_id;
}

uint8_t encryption_get_active_algorithm(void)
{
    return enc_state.active_algorithm;
}

bool encryption_is_analog_scramble_enabled(void)
{
    return enc_state.analog_scramble_enabled;
}

// ---- DMR voice encryption ----

void encryption_superframe_begin(void)
{
    if (!enc_module_initialized || !enc_state.enabled) {
        return;
    }

    // Advance IV for this superframe (LFSR per Motorola patent scheme)
    uint32_t current_iv = lfsr_advance();
    enc_state.iv = current_iv;
    enc_state.superframe_counter++;

    // Initialize cipher keystream for this superframe
    // TX and RX use separate state for simultaneous operation
    switch (enc_state.active_algorithm) {
        case ENC_ALGO_ARC4:
            arc4_superframe_init(enc_state.active_key_id, current_iv, true);   // TX
            arc4_superframe_init(enc_state.active_key_id, current_iv, false);  // RX
            break;
#if ENC_HAS_AES
        case ENC_ALGO_AES128:
        case ENC_ALGO_AES256:
            aes_ctr_init(enc_state.active_key_id, current_iv, true);
            aes_ctr_init(enc_state.active_key_id, current_iv, false);
            break;
#endif
        default:
            break;
    }

    enc_state.tx_keystream_ready = true;
    enc_state.rx_keystream_ready = true;
}

void encryption_encrypt_voice_block(uint8_t *data, uint8_t len)
{
    if (!enc_state.enabled) return;

    switch (enc_state.active_algorithm) {
        case ENC_ALGO_ARC4:
            arc4_encrypt_block(data, len);
            break;
#if ENC_HAS_AES
        case ENC_ALGO_AES128:
        case ENC_ALGO_AES256:
            aes_ctr_encrypt_block(data, len);
            break;
#endif
        default:
            break;
    }
}

void encryption_decrypt_voice_frame(uint8_t *data, uint8_t len)
{
    if (!enc_state.enabled) return;

    switch (enc_state.active_algorithm) {
        case ENC_ALGO_ARC4:
            arc4_decrypt_frame(data, len);
            break;
#if ENC_HAS_AES
        case ENC_ALGO_AES128:
        case ENC_ALGO_AES256:
            aes_ctr_decrypt_frame(data, len);
            break;
#endif
        default:
            break;
    }
}

// ---- Analog scrambling ----

void encryption_scramble_audio(int16_t *samples, uint16_t count)
{
    if (!enc_state.analog_scramble_enabled) return;
    // ponytail: default ID 4 = 3.2kHz if not set
    uint8_t id = enc_state.scramble_id ? enc_state.scramble_id : 4;
    scrambler_process(samples, count, &enc_state.scramble_carrier_phase, id);
}

// ---- IV management ----

void encryption_generate_iv(void)
{
    // Mix LFSR with whatever entropy sources are available
    // In a real deployment, mix in ADC noise or HR-C6000 register noise
    iv_lfsr ^= (uint32_t)(iv_lfsr << 13);
    iv_lfsr ^= (uint32_t)(iv_lfsr >> 17);
    iv_lfsr ^= (uint32_t)(iv_lfsr << 5);
    enc_state.iv = iv_lfsr;
}

uint32_t encryption_get_iv(void)
{
    return enc_state.iv;
}

void encryption_set_iv(uint32_t new_iv)
{
    enc_state.iv = new_iv;
    iv_lfsr = new_iv;
}

// ---- Key management ----

void encryption_key_store(uint8_t slot, const encryption_key_t *key)
{
    if (slot >= ENC_MAX_KEY_SLOTS) return;

    memcpy(&enc_keys[slot], key, sizeof(encryption_key_t));
    enc_keys[slot].key_id = slot;

    // Update key count
    enc_key_count = 0;
    for (int i = 0; i < ENC_MAX_KEY_SLOTS; i++) {
        if (enc_keys[i].algorithm != ENC_ALGO_NONE) {
            enc_key_count++;
        }
    }
}

void encryption_key_load(uint8_t slot, encryption_key_t *key)
{
    if (slot >= ENC_MAX_KEY_SLOTS) {
        memset(key, 0, sizeof(encryption_key_t));
        return;
    }
    memcpy(key, &enc_keys[slot], sizeof(encryption_key_t));
}

void encryption_key_erase(uint8_t slot)
{
    if (slot >= ENC_MAX_KEY_SLOTS) return;
    memset(&enc_keys[slot], 0, sizeof(encryption_key_t));
    enc_keys[slot].key_id = slot;
    enc_keys[slot].algorithm = ENC_ALGO_NONE;
    strcpy(enc_keys[slot].key_name, "(empty)");
}

uint8_t encryption_key_count(void)
{
    return enc_key_count;
}

const char* encryption_algo_name(uint8_t algo)
{
    switch (algo) {
        case ENC_ALGO_NONE:      return "None";
        case ENC_ALGO_ARC4:      return "ARC4";
        case ENC_ALGO_AES128:    return "AES-128";
        case ENC_ALGO_AES256:    return "AES-256";
        case ENC_ALGO_SCRAMBLER: return "Scrambler";
        default:                 return "Unknown";
    }
}

// ---- DMR encryption signaling helpers ----

uint8_t encryption_extract_key_id_from_burst(const uint8_t *burst_data)
{
    // Key ID is in the 32-bit Embedded Signalling field of Burst F
    // In Motorola DMR: Key ID at bits 0-7 of the 32-bit ES field
    // The ES field is at offset 13 in the DMR burst (after 108-bit payload 1)
    // For simplicity, return the active key ID — full ES parsing needs
    // BPTC decode which is done elsewhere. This stub returns active key.
    return enc_state.active_key_id;
}

uint8_t encryption_extract_algo_id_from_burst(const uint8_t *burst_data)
{
    // Algorithm ID at bits 8-10 of the 32-bit ES field
    // Stub: return active algorithm
    return enc_state.active_algorithm;
}

void encryption_build_pi_header(uint8_t *header_data, uint8_t key_id, uint8_t algo, uint32_t iv)
{
    // PI Header = Slot Type 1, Data Type 0x07 per DMRA spec
    // Format: 96 bits of information = 12 bytes after BPTC(196,96) encoding
    //
    // Information field (96 bits = 12 bytes):
    //   Bytes 0-3: IV (32 bits, big-endian)
    //   Byte 4: Key ID (8 bits)
    //   Byte 5: Algorithm ID (3 bits, upper) + Reserved (5 bits)
    //   Bytes 6-9: Reserved / Future Use
    //   Bytes 10-11: CRC-16

    memset(header_data, 0, 12);

    // IV (big-endian)
    header_data[0] = (uint8_t)(iv >> 24);
    header_data[1] = (uint8_t)(iv >> 16);
    header_data[2] = (uint8_t)(iv >> 8);
    header_data[3] = (uint8_t)(iv);

    // Key ID
    header_data[4] = key_id;

    // Algorithm ID (upper 3 bits of byte 5)
    header_data[5] = (algo & 0x07) << 5;

    // CRC-16 (simple CCITT) over first 10 bytes
    uint16_t crc = 0xFFFF;
    for (int i = 0; i < 10; i++) {
        crc ^= (uint16_t)(header_data[i] << 8);
        for (int b = 0; b < 8; b++) {
            if (crc & 0x8000) {
                crc = (crc << 1) ^ 0x1021;
            } else {
                crc <<= 1;
            }
        }
    }
    header_data[10] = (uint8_t)(crc >> 8);
    header_data[11] = (uint8_t)(crc);
}
