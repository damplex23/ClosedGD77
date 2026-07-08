/*
 * Copyright (C) 2026 ClosedGD77
 *
 * DMR Voice Encryption and Analog Scrambling Module
 *
 * Supports:
 *   - ARC4 (RC4-compatible) stream cipher — DMR Enhanced Privacy, 40-bit keys
 *   - AES-128/256-CTR — strong encryption for STM32 platforms (>= 1MB flash)
 *   - Analog FM frequency-inversion scrambling
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 */

#ifndef ENCRYPTION_H
#define ENCRYPTION_H

#include <stdint.h>
#include <stdbool.h>

// ---- Algorithm identifiers (match DMR PI Header Algorithm ID field) ----
typedef enum {
    ENC_ALGO_NONE      = 0,
    ENC_ALGO_ARC4      = 1,  // DMR Enhanced Privacy (40-bit key standard, also supports up to 256-bit)
    ENC_ALGO_AES128     = 2,  // AES-128-CTR (STM32 only)
    ENC_ALGO_AES256     = 3,  // AES-256-CTR (STM32 only)
    ENC_ALGO_SCRAMBLER  = 4,  // Analog FM frequency inversion
} encryption_algo_t;

// ---- Platform capabilities ----
// PLATFORM_STM32 is defined in the STM32 build config
// PLATFORM_MK22 is defined in the MK22 build config
#if defined(PLATFORM_STM32)
    #define ENC_HAS_AES         1
    #define ENC_MAX_KEY_SLOTS   32
    #define ENC_MAX_KEY_SIZE    32   // AES-256 key = 32 bytes
#elif defined(PLATFORM_MK22)
    #define ENC_HAS_AES         0
    #define ENC_MAX_KEY_SLOTS   16
    #define ENC_MAX_KEY_SIZE    32   // Allow storing longer keys (unused slots)
#else
    // Default: assume STM32 capabilities (building on desktop for tests)
    #define ENC_HAS_AES         1
    #define ENC_MAX_KEY_SLOTS   32
    #define ENC_MAX_KEY_SIZE    32
#endif

// ARC4 key sizes
#define ENC_ARC4_MIN_KEY_SIZE   5   // 40-bit (DMRA standard)
#define ENC_ARC4_MAX_KEY_SIZE   32  // 256-bit (non-standard, stronger)

// ---- Key storage ----
typedef struct {
    uint8_t  key_id;                      // 0..31
    uint8_t  algorithm;                   // encryption_algo_t
    uint8_t  key_length;                  // actual key bytes used
    char     key_name[16];                // user-visible label
    uint8_t  key_data[ENC_MAX_KEY_SIZE];  // raw key material
} encryption_key_t;

// ---- Runtime encryption state ----
typedef struct {
    bool     enabled;                     // encryption active on current channel
    uint8_t  active_key_id;              // which key is in use
    uint8_t  active_algorithm;           // algorithm for active key
    uint32_t iv;                         // current initialization vector
    uint32_t superframe_counter;         // superframe sequence number
    bool     pi_header_sent;             // PI Header already sent this TX
    bool     rx_keystream_ready;         // RX keystream initialized for current call
    bool     tx_keystream_ready;         // TX keystream initialized for current call
    bool     analog_scramble_enabled;    // analog scrambler active
    uint8_t  scramble_carrier_phase;     // phase accumulator for scrambler
} encryption_state_t;

// ---- API ----

// Lifecycle
void encryption_init(void);
void encryption_reset_per_call(void);

// Per-channel configuration (called when channel changes or TX starts)
void encryption_set_enabled(bool enabled);
void encryption_set_key_id(uint8_t key_id);
void encryption_set_algorithm(uint8_t algo);
void encryption_set_analog_scramble(bool enabled);
void encryption_apply_channel_settings(uint8_t encrypt_field); // convenience: parse codeplug encrypt field
bool encryption_is_enabled(void);
uint8_t encryption_get_active_key_id(void);
uint8_t encryption_get_active_algorithm(void);
bool encryption_is_analog_scramble_enabled(void);

// DMR voice frame encrypt/decrypt — operates on 9-byte (72-bit) AMBE blocks
// or 27-byte full DMR voice frames. In-place operations.
void encryption_encrypt_voice_block(uint8_t *data, uint8_t len);
void encryption_decrypt_voice_frame(uint8_t *data, uint8_t len);

// Initialize keystream for a new superframe (called at each superframe start)
// Uses: stored key + current IV — generates fresh keystream
void encryption_superframe_begin(void);

// Analog FM scrambling — in-place on 16-bit audio samples
// Scramble and descramble are identical operations (frequency inversion)
void encryption_scramble_audio(int16_t *samples, uint16_t count);

// IV management
void encryption_generate_iv(void);
uint32_t encryption_get_iv(void);
void encryption_set_iv(uint32_t new_iv);

// Key management — for CPS/key storage integration
void encryption_key_store(uint8_t slot, const encryption_key_t *key);
void encryption_key_load(uint8_t slot, encryption_key_t *key);
void encryption_key_erase(uint8_t slot);
uint8_t encryption_key_count(void);
const char* encryption_algo_name(uint8_t algo);

// DMR encryption signaling helpers
// Extract Key ID and Algorithm ID from Burst F embedded signaling
uint8_t encryption_extract_key_id_from_burst(const uint8_t *burst_data);
uint8_t encryption_extract_algo_id_from_burst(const uint8_t *burst_data);
// Build PI Header data (12 bytes for Slot Type 1 burst)
void encryption_build_pi_header(uint8_t *header_data, uint8_t key_id, uint8_t algo, uint32_t iv);

// State accessor (for interrupt context fast check)
const encryption_state_t* encryption_get_state(void);

#endif // ENCRYPTION_H
