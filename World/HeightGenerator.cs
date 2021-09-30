using DerpySimulation.Core;
using System;

namespace DerpySimulation.World
{
    // TODO: Optimize - The heights for neighboring values are constantly generated per octave (GetBiasedNoise), making it slow.

    /// <summary>A Perlin Noise generator.</summary>
    internal sealed class HeightGenerator
    {
        private readonly int _seed;

        private readonly float _amplitude;
        private readonly int _numOctaves; // Too many octaves smooths out the result and takes a lot longer to finish
        private readonly float _roughness; // Higher is more rough

        public HeightGenerator(float amplitude, int numOctaves, float roughness, int? seed = null)
        {
            _seed = seed ?? Environment.TickCount;
            _amplitude = amplitude;
            _numOctaves = numOctaves;
            _roughness = roughness;
        }

        /// <summary>Gets the final height of these coordinates.</summary>
        public float GenerateHeight(int x, int z)
        {
            float total = 0;
            float d = MathF.Pow(2, _numOctaves - 1);
            for (int i = 0; i < _numOctaves; i++)
            {
                float freq = MathF.Pow(2, i) / d;
                float amp = MathF.Pow(_roughness, i) * _amplitude;
                total += GetFinalHeight(x * freq, z * freq) * amp;
            }
            return total;
        }

        /// <summary>Get a final float y value for this octave based on the coordinates beside it.</summary>
        private float GetFinalHeight(float x, float z)
        {
            int intX = (int)x;
            int intZ = (int)z;
            float fracX = x - intX;
            float fracZ = z - intZ;

            float v1 = GetBiasedNoise(intX, intZ);
            float v2 = GetBiasedNoise(intX + 1, intZ);
            float v3 = GetBiasedNoise(intX, intZ + 1);
            float v4 = GetBiasedNoise(intX + 1, intZ + 1);
            float i1 = Utils.CosineInterpolation(v1, v2, fracX);
            float i2 = Utils.CosineInterpolation(v3, v4, fracX);
            return Utils.CosineInterpolation(i1, i2, fracZ);
        }

        /// <summary>Gets the y value for these coordinates based on all of the y values around it. Corner values have the least impact on the final value.</summary>
        private float GetBiasedNoise(int x, int z)
        {
            float corners = (GetNoise(x - 1, z - 1) + GetNoise(x + 1, z - 1) + GetNoise(x - 1, z + 1) + GetNoise(x + 1, z + 1)) / 16f;
            float sides = (GetNoise(x - 1, z) + GetNoise(x + 1, z) + GetNoise(x, z - 1) + GetNoise(x, z + 1)) / 8f;
            float center = GetNoise(x, z) / 4f;
            return corners + sides + center;
        }

        /// <summary>Returns a y value between -1 (inclusive) and 1 (inclusive) for these coordinates.</summary>
        private float GetNoise(int x, int z)
        {
            // Create a seed for these coordinates
            uint lehmerSeed = (uint)(((604171 * x) + (4393139 * z) + _seed) ^ 534742);
            // Call randomizer
            uint result = Utils.LehmerRandomizer(lehmerSeed);
            // Use as a float between -1 and 1
            return (result / (float)uint.MaxValue) * 2 - 1;
        }
    }
}
