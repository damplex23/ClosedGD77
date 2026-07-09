/*
 * Analog FM Frequency Inversion Scrambler
 *
 * Standard analog scrambling: invert audio spectrum around a carrier frequency.
 * Multiply samples by alternating +1/-1 at the carrier rate, which flips
 * the frequency spectrum: f' = f_carrier - f
 *
 * Scramble and descramble are the same operation — applying twice restores audio.
 *
 * 8 scramble IDs for user-selectable carrier frequencies:
 *   ID 1: 2.0kHz  ID 5: 3.6kHz
 *   ID 2: 2.4kHz  ID 6: 4.0kHz
 *   ID 3: 2.8kHz  ID 7: 4.4kHz
 *   ID 4: 3.2kHz  ID 8: 4.8kHz
 *
 * Flash: ~200 bytes. RAM: 2 bytes (phase). Zero additional buffers.
 */

#include <stdint.h>
#include "encryption/encryption.h"

#define SCRAMBLE_SAMPLE_RATE     8000

// Carrier frequencies for scramble IDs 1-8 (Hz)
static const uint16_t carrier_freq_hz[8] = {
    2000, 2400, 2800, 3200, 3600, 4000, 4400, 4800
};

// Simple square-wave frequency inversion using phase accumulator.
// Multiplies audio by alternating +1/-1 at the carrier rate,
// inverting the frequency spectrum around fc/2.
//
// Phase accumulator: 16-bit, increments by (fc << 16) / 8000 per sample.
// Toggle sign when phase MSB changes.
void scrambler_process(int16_t *samples, uint16_t count, uint16_t *phase, uint8_t scramble_id)
{
    if (scramble_id < 1 || scramble_id > 8)
    {
        scramble_id = 4; // default 3.2kHz
    }

    uint32_t phase_inc = ((uint32_t)carrier_freq_hz[scramble_id - 1] << 16) / SCRAMBLE_SAMPLE_RATE;
    uint16_t local_phase = *phase;

    for (uint16_t n = 0; n < count; n++)
    {
        uint16_t prev_phase = local_phase;
        local_phase += (uint16_t)phase_inc;

        // Toggle sign when phase MSB changes (crosses midpoint)
        if ((local_phase & 0x8000) != (prev_phase & 0x8000))
        {
            samples[n] = -samples[n];
        }
    }

    *phase = local_phase;
}
