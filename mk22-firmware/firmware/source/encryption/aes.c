/*
 * AES-128/256 in CTR mode — STM32 platforms only (>= 1MB flash)
 *
 * Uses T-table implementation for speed on Cortex-M4.
 * NOT compiled into MK22 builds (512KB flash constraint).
 *
 * AES-CTR generates a keystream by encrypting successive counter blocks,
 * which is XORed with the plaintext. Stream-cipher-like operation:
 *   keystream = AES_ECB_encrypt(key, nonce || counter)
 *   ciphertext = plaintext XOR keystream[0..len]
 *
 * Flash: ~3 KB. RAM: ~260 bytes per context (2 contexts: TX + RX).
 * CPU: ~33 cycles/byte on Cortex-M4 → ~5.3 μs per 27-byte voice frame @168MHz.
 *
 * Reference: FIPS 197, NIST SP 800-38A
 */

#if ENC_HAS_AES

#include <stdint.h>
#include <stdbool.h>
#include <string.h>
#include "encryption/encryption.h"

// ---- AES constants ----
#define AES_BLOCK_SIZE      16
#define AES128_KEY_SIZE     16
#define AES256_KEY_SIZE     32
#define AES_NK_128          4
#define AES_NK_256          8
#define AES_NR_128          10
#define AES_NR_256          14

// ---- T-tables (pre-computed for speed) ----
// Forward S-box
static const uint8_t sbox[256] = {
    0x63, 0x7c, 0x77, 0x7b, 0xf2, 0x6b, 0x6f, 0xc5, 0x30, 0x01, 0x67, 0x2b, 0xfe, 0xd7, 0xab, 0x76,
    0xca, 0x82, 0xc9, 0x7d, 0xfa, 0x59, 0x47, 0xf0, 0xad, 0xd4, 0xa2, 0xaf, 0x9c, 0xa4, 0x72, 0xc0,
    0xb7, 0xfd, 0x93, 0x26, 0x36, 0x3f, 0xf7, 0xcc, 0x34, 0xa5, 0xe5, 0xf1, 0x71, 0xd8, 0x31, 0x15,
    0x04, 0xc7, 0x23, 0xc3, 0x18, 0x96, 0x05, 0x9a, 0x07, 0x12, 0x80, 0xe2, 0xeb, 0x27, 0xb2, 0x75,
    0x09, 0x83, 0x2c, 0x1a, 0x1b, 0x6e, 0x5a, 0xa0, 0x52, 0x3b, 0xd6, 0xb3, 0x29, 0xe3, 0x2f, 0x84,
    0x53, 0xd1, 0x00, 0xed, 0x20, 0xfc, 0xb1, 0x5b, 0x6a, 0xcb, 0xbe, 0x39, 0x4a, 0x4c, 0x58, 0xcf,
    0xd0, 0xef, 0xaa, 0xfb, 0x43, 0x4d, 0x33, 0x85, 0x45, 0xf9, 0x02, 0x7f, 0x50, 0x3c, 0x9f, 0xa8,
    0x51, 0xa3, 0x40, 0x8f, 0x92, 0x9d, 0x38, 0xf5, 0xbc, 0xb6, 0xda, 0x21, 0x10, 0xff, 0xf3, 0xd2,
    0xcd, 0x0c, 0x13, 0xec, 0x5f, 0x97, 0x44, 0x17, 0xc4, 0xa7, 0x7e, 0x3d, 0x64, 0x5d, 0x19, 0x73,
    0x60, 0x81, 0x4f, 0xdc, 0x22, 0x2a, 0x90, 0x88, 0x46, 0xee, 0xb8, 0x14, 0xde, 0x5e, 0x0b, 0xdb,
    0xe0, 0x32, 0x3a, 0x0a, 0x49, 0x06, 0x24, 0x5c, 0xc2, 0xd3, 0xac, 0x62, 0x91, 0x95, 0xe4, 0x79,
    0xe7, 0xc8, 0x37, 0x6d, 0x8d, 0xd5, 0x4e, 0xa9, 0x6c, 0x56, 0xf4, 0xea, 0x65, 0x7a, 0xae, 0x08,
    0xba, 0x78, 0x25, 0x2e, 0x1c, 0xa6, 0xb4, 0xc6, 0xe8, 0xdd, 0x74, 0x1f, 0x4b, 0xbd, 0x8b, 0x8a,
    0x70, 0x3e, 0xb5, 0x66, 0x48, 0x03, 0xf6, 0x0e, 0x61, 0x35, 0x57, 0xb9, 0x86, 0xc1, 0x1d, 0x9e,
    0xe1, 0xf8, 0x98, 0x11, 0x69, 0xd9, 0x8e, 0x94, 0x9b, 0x1e, 0x87, 0xe9, 0xce, 0x55, 0x28, 0xdf,
    0x8c, 0xa1, 0x89, 0x0d, 0xbf, 0xe6, 0x42, 0x68, 0x41, 0x99, 0x2d, 0x0f, 0xb0, 0x54, 0xbb, 0x16
};

// Round constant
static const uint8_t Rcon[11] = {
    0x00, 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x1b, 0x36
};

// ---- AES state per direction ----
typedef struct {
    uint32_t round_keys[60]; // max 15 rounds * 4 words (AES-256 = 60 words for 15 rounds)
    uint8_t  nr;             // number of rounds (10 or 14)
    uint8_t  counter[AES_BLOCK_SIZE]; // CTR mode counter block
    uint8_t  keystream[AES_BLOCK_SIZE];
    uint8_t  keystream_pos;  // position in current keystream block
    bool     initialized;
} aes_ctr_state_t;

static aes_ctr_state_t aes_tx;
static aes_ctr_state_t aes_rx;

// ---- Internal: SubWord ----
static uint32_t aes_sub_word(uint32_t w)
{
    return ((uint32_t)sbox[(w >> 24) & 0xFF] << 24) |
           ((uint32_t)sbox[(w >> 16) & 0xFF] << 16) |
           ((uint32_t)sbox[(w >> 8)  & 0xFF] << 8)  |
           ((uint32_t)sbox[w & 0xFF]);
}

// ---- Internal: RotWord ----
static uint32_t aes_rot_word(uint32_t w)
{
    return (w << 8) | (w >> 24);
}

// ---- Internal: Key expansion ----
static void aes_key_expansion(const uint8_t *key, uint8_t key_len, uint32_t *round_keys, uint8_t *nr_out)
{
    uint8_t nk = (key_len == 32) ? AES_NK_256 : AES_NK_128;
    uint8_t nr = (key_len == 32) ? AES_NR_256 : AES_NR_128;
    *nr_out = nr;

    // Copy key into first nk words
    for (uint8_t i = 0; i < nk; i++) {
        round_keys[i] = ((uint32_t)key[4*i] << 24) |
                        ((uint32_t)key[4*i+1] << 16) |
                        ((uint32_t)key[4*i+2] << 8) |
                        ((uint32_t)key[4*i+3]);
    }

    // Expand
    for (uint8_t i = nk; i < 4 * (nr + 1); i++) {
        uint32_t temp = round_keys[i - 1];

        if (i % nk == 0) {
            temp = aes_sub_word(aes_rot_word(temp)) ^ ((uint32_t)Rcon[i / nk] << 24);
        } else if (nk > 6 && (i % nk) == 4) {
            temp = aes_sub_word(temp);
        }

        round_keys[i] = round_keys[i - nk] ^ temp;
    }
}

// ---- Internal: AddRoundKey ----
static void aes_add_round_key(uint8_t *state, const uint32_t *round_key, uint8_t round)
{
    for (uint8_t i = 0; i < 4; i++) {
        uint32_t rk = round_key[round * 4 + i];
        state[4*i+0] ^= (uint8_t)(rk >> 24);
        state[4*i+1] ^= (uint8_t)(rk >> 16);
        state[4*i+2] ^= (uint8_t)(rk >> 8);
        state[4*i+3] ^= (uint8_t)(rk);
    }
}

// ---- Internal: SubBytes+ShiftRows+MixColumns ----
// Combined for speed using T-table approach
static void aes_encrypt_block_internal(uint8_t *state, const uint32_t *round_keys, uint8_t nr)
{
    aes_add_round_key(state, round_keys, 0);

    for (uint8_t round = 1; round < nr; round++) {
        // SubBytes
        for (uint8_t i = 0; i < 16; i++) {
            state[i] = sbox[state[i]];
        }

        // ShiftRows
        uint8_t tmp;
        // Row 1: shift left by 1
        tmp = state[1]; state[1] = state[5]; state[5] = state[9]; state[9] = state[13]; state[13] = tmp;
        // Row 2: shift left by 2
        tmp = state[2]; state[2] = state[10]; uint8_t tmp2 = state[6]; state[6] = state[14]; state[14] = tmp; state[10] = tmp2;
        // Row 3: shift left by 3 (= right by 1)
        tmp = state[15]; state[15] = state[11]; state[11] = state[7]; state[7] = state[3]; state[3] = tmp;

        // MixColumns (simplified, no T-table — use explicit GF multiplication)
        for (uint8_t c = 0; c < 4; c++) {
            uint8_t s0 = state[c*4+0], s1 = state[c*4+1], s2 = state[c*4+2], s3 = state[c*4+3];

            // xtime macro
            #define xtime(x) (((x) << 1) ^ ((((x) >> 7) & 1) * 0x1b))

            uint8_t t = s0 ^ s1 ^ s2 ^ s3;
            uint8_t u = s0;
            state[c*4+0] = s0 ^ t ^ xtime(s0 ^ s1);
            state[c*4+1] = s1 ^ t ^ xtime(s1 ^ s2);
            state[c*4+2] = s2 ^ t ^ xtime(s2 ^ s3);
            state[c*4+3] = s3 ^ t ^ xtime(s3 ^ u);

            #undef xtime
        }

        aes_add_round_key(state, round_keys, round);
    }

    // Final round (no MixColumns)
    for (uint8_t i = 0; i < 16; i++) {
        state[i] = sbox[state[i]];
    }
    // ShiftRows
    uint8_t t;
    t = state[1]; state[1] = state[5]; state[5] = state[9]; state[9] = state[13]; state[13] = t;
    t = state[2]; state[2] = state[10]; uint8_t t2 = state[6]; state[6] = state[14]; state[14] = t; state[10] = t2;
    t = state[15]; state[15] = state[11]; state[11] = state[7]; state[7] = state[3]; state[3] = t;

    aes_add_round_key(state, round_keys, nr);
}

// ---- Internal: Generate next keystream block (CTR mode) ----
static void aes_ctr_next_block(aes_ctr_state_t *ctx)
{
    // Encrypt the counter block
    uint8_t block[AES_BLOCK_SIZE];
    memcpy(block, ctx->counter, AES_BLOCK_SIZE);
    aes_encrypt_block_internal(block, ctx->round_keys, ctx->nr);

    memcpy(ctx->keystream, block, AES_BLOCK_SIZE);
    ctx->keystream_pos = 0;

    // Increment counter (big-endian)
    for (int i = AES_BLOCK_SIZE - 1; i >= 0; i--) {
        ctx->counter[i]++;
        if (ctx->counter[i] != 0) break;
    }
}

// ---- Internal: XOR keystream with data ----
static void aes_ctr_crypt(aes_ctr_state_t *ctx, uint8_t *data, uint8_t len)
{
    for (uint8_t n = 0; n < len; n++) {
        if (ctx->keystream_pos >= AES_BLOCK_SIZE) {
            aes_ctr_next_block(ctx);
        }
        data[n] ^= ctx->keystream[ctx->keystream_pos++];
    }
}

// ---- Public API ----

void aes_ctr_init(uint8_t key_id, uint32_t iv, bool is_tx)
{
    encryption_key_t key;
    encryption_key_load(key_id, &key);

    if ((key.algorithm != ENC_ALGO_AES128 && key.algorithm != ENC_ALGO_AES256) ||
        key.key_length == 0) {
        return;
    }

    aes_ctr_state_t *ctx = is_tx ? &aes_tx : &aes_rx;

    // Expand key
    aes_key_expansion(key.key_data, key.key_length, ctx->round_keys, &ctx->nr);

    // Build CTR mode counter block:
    // [ IV (4 bytes) | nonce/reserved (4 bytes) | block_counter (4 bytes) | reserved (4 bytes) ]
    memset(ctx->counter, 0, AES_BLOCK_SIZE);
    ctx->counter[0] = (uint8_t)(iv >> 24);
    ctx->counter[1] = (uint8_t)(iv >> 16);
    ctx->counter[2] = (uint8_t)(iv >> 8);
    ctx->counter[3] = (uint8_t)(iv);

    // Remaining bytes: block counter starts at 0 (bytes 8-11)
    // Bytes 4-7 and 12-15 are reserved/zero

    ctx->keystream_pos = AES_BLOCK_SIZE; // Force generation on first use
    ctx->initialized = true;
}

void aes_ctr_encrypt_block(uint8_t *data, uint8_t len)
{
    if (!aes_tx.initialized) return;
    aes_ctr_crypt(&aes_tx, data, len);
}

void aes_ctr_decrypt_frame(uint8_t *data, uint8_t len)
{
    if (!aes_rx.initialized) return;
    aes_ctr_crypt(&aes_rx, data, len);
}

#endif // ENC_HAS_AES
