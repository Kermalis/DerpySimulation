using DerpySimulation.Core;
using System;
using System.Numerics;
using System.Threading;

namespace DerpySimulation.World.Terrain
{
    // TODO: Optimize - The heights for neighboring values are constantly generated per octave (GetBiasedNoise), making it slow.

    /// <summary>A Perlin Noise generator.</summary>
    internal sealed class TerrainHeightGenerator
    {
        private readonly int _seed;

        private readonly float _amplitude;
        private readonly int _numOctaves; // Too many octaves smooths out the result and takes a lot longer to finish
        private readonly float _roughness; // Higher is more rough

        public TerrainHeightGenerator(int seed, float amplitude, int numOctaves, float roughness)
        {
            _seed = seed;
            _amplitude = amplitude;
            _numOctaves = numOctaves;
            _roughness = roughness;
        }

        public static float[,] GenerateArea(CancellationTokenSource cts, uint sizeX, uint sizeZ, int seed, float amplitude, int numOctaves, float roughness, out Vector3 peak)
        {
            peak = new Vector3(0, float.NegativeInfinity, 0);
            var heightGen = new TerrainHeightGenerator(seed, amplitude, numOctaves, roughness);
            // size + 1 because we are also calculating the point right outside of the terrain so it can still interpolate there
            float[,] gridHeights = new float[sizeX + 1, sizeZ + 1];
            for (int z = 0; z < sizeZ + 1; z++)
            {
                for (int x = 0; x < sizeX + 1; x++)
                {
                    cts.Token.ThrowIfCancellationRequested();
                    float y = heightGen.GenerateHeight(x, z);
                    if (y > peak.Y)
                    {
                        peak = new Vector3(x, y, z);
                    }
                    gridHeights[x, z] = y;
                }
            }
            return gridHeights;
        }

        /// <summary>Gets the final height of these coordinates.</summary>
        public float GenerateHeight(int x, int z)
        {
            // TODO: Octaves should be sampling from radically different coordinates
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
            uint lehmer = (uint)(((604171 * x) + (4393139 * z) + _seed) ^ 534742);
            // Use as a float between -1 and 1
            return LehmerRand.NextFloatRange(-1, 1, ref lehmer);
        }
    }
}
