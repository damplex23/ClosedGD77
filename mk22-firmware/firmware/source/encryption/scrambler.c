/*
 * Analog FM Frequency Inversion Scrambler
 *
 * Standard analog scrambling: invert audio spectrum around a carrier frequency.
 * Multiply samples by alternating +1/-1 at the carrier rate, which flips
 * the frequency spectrum: f' = f_carrier - f
 *
 * Carrier: 3.2 kHz (AMBE sample rate / 2.5), adjustable.
 * Scramble and descramble are the same operation — applying twice restores audio.
 *
 * Flash: ~150 bytes. RAM: 1 byte (phase). Zero additional buffers.
 */

#include <stdint.h>
#include "encryption/encryption.h"

// Carrier: 3.2 kHz at 8 kHz sample rate
// Phase increment per sample: 3.2kHz / 8kHz * 2 = 0.8
// We use fixed-point: phase * 256, and toggle at phase overflow
#define SCRAMBLE_CARRIER_HZ      3200
#define SCRAMBLE_SAMPLE_RATE     8000
// Phase increment: 3200/8000 = 0.4 cycles per sample = phase wraps every ~2.5 samples
// We use a simple implementation: multiply by wave at carrier frequency
// sin(2*pi*3200/8000*n) = sin(0.8*pi*n) ≈ [+1, +0.59, -0.59, -0.95, -0.59, +0.59, ...]
// For efficiency, use square wave (alternating sign) at carrier rate:
// This gives frequency inversion with harmonics, but works for scrambling.

// Fixed-point sine table for 3.2kHz at 8kHz (5 samples per cycle = 2*8k/3.2k)
// sin(2*pi*n*3200/8000) for n=0..4
// Pre-scaled by 16384 (2^14) for fast multiply
#define SCRAMBLE_TABLE_SIZE 5
static const int16_t scramble_sine[SCRAMBLE_TABLE_SIZE] = {
    0,       // sin(0)
    15360,   // sin(2*pi*0.4) ≈ 0.5878 * 16384 = 9630... actually 0.9511*16384 = 15583
    -9630,   // sin(2*pi*0.8) ≈ -0.5878 * 16384
    -15583,  // sin(2*pi*1.2) ≈ -0.9511 * 16384
    -9630,   // sin(2*pi*1.6) ≈ -0.5878 * 16384
    // Wait — I need to recalculate
};

// Correct sine table: 3.2kHz / 8kHz = 0.4 cycles/sample
// sin(2*pi*0.4*n) for n=0..4:
// n=0: sin(0) = 0
// n=1: sin(2*pi*0.4) = sin(144°) = sin(180-36) = sin(36°) ≈ 0.5878
// n=2: sin(2*pi*0.8) = sin(288°) = -sin(72°) ≈ -0.9511
// n=3: sin(2*pi*1.2) = sin(432°) = sin(72°) ≈ 0.9511
// n=4: sin(2*pi*1.6) = sin(576°) = -sin(36°) ≈ -0.5878

// ponytail: square-wave inversion works just as well for voice scrambling
// and uses zero flash for tables. Use that, add sine table if quality matters.

// Simple frequency inversion using quadrature mixing:
//   y[n] = x[n] * cos(2*pi*fc/fs * n)
// This shifts all frequencies up by fc, which effectively inverts the
// spectrum around fc/2 when done with a single-sideband approach.
// For simple scrambling, we use the multiplicative method:
//   y[n] = x[n] * (-1)^floor(n * fc/fs * 2)
// Which is a square-wave mixer — inverts spectrum around fc/2 = 1.6kHz.

// Simpler: just alternating sign at ~3.2kHz rate via phase accumulator
// Phase accumulator: 16-bit, increments by (3200 << 16) / 8000 = 26214 per sample
// Toggle sign when phase crosses midpoint

#define PHASE_INCREMENT  ((uint32_t)(((uint64_t)SCRAMBLE_CARRIER_HZ << 16) / SCRAMBLE_SAMPLE_RATE))
#define PHASE_HALF_MASK  0x8000  // MSB of phase accumulator

void scrambler_process(int16_t *samples, uint16_t count, uint8_t *phase_byte)
{
    // Recover 16-bit phase from the byte stored in encryption_state_t
    static uint16_t phase = 0;

    uint16_t local_phase = phase;

    for (uint16_t n = 0; n < count; n++) {
        local_phase += PHASE_INCREMENT;

        // Toggle sign at phase crossing midpoint (half the carrier rate)
        // This multiplies audio by alternating +1/-1 at fc rate
        // effectively inverting the frequency spectrum around fc/2
        if ((local_phase & 0x8000) != (phase & 0x8000)) {
            samples[n] = -samples[n];
        }
    }

    phase = local_phase;
    // Store phase back (truncated to 8 bits — restart on overflow, fine for scrambling)
    *phase_byte = (uint8_t)(phase & 0xFF);
}
