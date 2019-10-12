using System;

/*
 * Part of WakExtractor adapted from Unicorn by SaphireLattice
 * 
 * Copyright (c) 2019, Saphire Lattice
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this
 * software and associated documentation files (the "Software"), to deal in the Software
 * without restriction, including without limitation the rights to use, copy, modify,
 * merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies
 * or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 * PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

namespace Ommel {
    class NollaPrng {
        public static bool BETA_SEED = true;
        const int SEED_BASE = 23456789 + 1 + 11 * 11;
        public double Seed { get; private set; }

        public NollaPrng(int seed, int seedBase = SEED_BASE) {
            Seed = (double)(uint)seedBase + seed;
            if (BETA_SEED && Seed >= 2147483647.0) {
                Seed *= 0.5;
            }
            Next();
        }

        public NollaPrng(double seed) {
            Seed = seed;
            Next();
        }

        public double Next() {
            Seed = ((int)Seed) * 16807 + ((int)Seed) / 127773 * -int.MaxValue;
            //it's abs+1, because M A G I C, damn it
            if (Seed < 0) Seed += int.MaxValue;
            return Seed / int.MaxValue;
        }

        public byte[] Next16() {
            var value = new byte[16];
            for (int i = 0; i < 4; i++) {
                byte[] bytes = BitConverter.GetBytes((int)(Next() * int.MinValue));
                Buffer.BlockCopy(
                    bytes, 0,
                    value, i * 4,
                    4
                );
            }
            return value;
        }

        public static byte[] Get16Seeded(int seed, int seedBase = SEED_BASE) {
            var prng = new NollaPrng(seed, seedBase);
            return prng.Next16();
        }

        public static void DoMain(string[] args) {
            foreach (var arg in args) {
                if (uint.TryParse(arg, out var value)) {
                    var prng = new NollaPrng(value);
                    Console.WriteLine($"Next state for seed {value}: {prng.Seed}, error on float cast {prng.Seed - ((double)(float)(prng.Seed * ((double)1 / int.MaxValue))) * (0x8000_0000)}");
                    Console.WriteLine($"IV bytes for given seed are: {BitConverter.ToString(Get16Seeded((int)value))}");
                }
            }
        }
    }
}
