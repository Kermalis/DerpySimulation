using System;
using System.Numerics;

namespace DerpySimulation.Core
{
    internal sealed class LehmerRand
    {
        public uint State;

        public LehmerRand()
        {
            State = CreateRandomSeed();
        }
        public LehmerRand(uint seed)
        {
            State = seed;
        }

        public static uint CreateRandomSeed()
        {
            uint s = (uint)Environment.TickCount;
            Next(ref s);
            return s;
        }

        public static void Next(ref uint state)
        {
            state += 0xE120FC15;
            ulong tmp = (ulong)state * 0x4A39B70D;
            state = (uint)((tmp >> 32) ^ tmp);
            tmp = (ulong)state * 0x12FAD5C9;
            state = (uint)((tmp >> 32) ^ tmp);
        }
        public uint Next()
        {
            Next(ref State);
            return State;
        }

        /// <summary>Gets a random value between 0 (inclusive) and 1 (inclusive).</summary>
        public static float NextFloat(ref uint state)
        {
            Next(ref state);
            return (float)state / uint.MaxValue;
        }
        /// <summary>Gets a random value between 0 (inclusive) and 1 (inclusive).</summary>
        public float NextFloat()
        {
            return NextFloat(ref State);
        }

        /// <summary>Gets a random value between 0 (inclusive) and 1 (exclusive).</summary>
        public static float NextFloatNo1(ref uint state)
        {
            do
            {
                Next(ref state);
            } while (state == uint.MaxValue);
            return (float)state / uint.MaxValue;
        }
        /// <summary>Gets a random value between 0 (inclusive) and 1 (exclusive).</summary>
        public float NextFloatNo1()
        {
            return NextFloatNo1(ref State);
        }

        /// <summary>Gets a random value between <paramref name="min"/> (inclusive) and <paramref name="max"/> (inclusive).</summary>
        public static float NextFloatRange(float min, float max, ref uint state)
        {
            return (NextFloat(ref state) * (max - min)) + min;
        }
        /// <summary>Gets a random value between <paramref name="min"/> (inclusive) and <paramref name="max"/> (inclusive).</summary>
        public float NextFloatRange(float min, float max)
        {
            return NextFloatRange(min, max, ref State);
        }

        public static Vector3 NextVector3(ref uint state)
        {
            return new Vector3(
                NextFloat(ref state),
                NextFloat(ref state),
                NextFloat(ref state)
                );
        }
        public Vector3 NextVector3()
        {
            return NextVector3(ref State);
        }

        public static Vector3 NextVector3Range(float min, float max, ref uint state)
        {
            return new Vector3(
                NextFloatRange(min, max, ref state),
                NextFloatRange(min, max, ref state),
                NextFloatRange(min, max, ref state)
                );
        }
        public Vector3 NextVector3Range(float min, float max)
        {
            return NextVector3Range(min, max, ref State);
        }
    }
}
