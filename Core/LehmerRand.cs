using System;
using System.Numerics;

namespace DerpySimulation.Core
{
    internal sealed class LehmerRand
    {
        private uint _state;
        private uint _boolState;

        public LehmerRand()
        {
            _state = CreateRandomSeed();
        }
        public LehmerRand(uint seed)
        {
            _state = seed;
        }

        public void SetState(uint state)
        {
            _state = state;
            _boolState = 0;
        }

        public static uint CreateRandomSeed()
        {
            uint s = (uint)Environment.TickCount;
            NextUint(ref s);
            return s;
        }

        public static void NextUint(ref uint state)
        {
            state += 0xE120FC15;
            ulong tmp = (ulong)state * 0x4A39B70D;
            state = (uint)((tmp >> 32) ^ tmp);
            tmp = (ulong)state * 0x12FAD5C9;
            state = (uint)((tmp >> 32) ^ tmp);
        }
        public uint NextUint()
        {
            NextUint(ref _state);
            return _state;
        }

        public static bool NextBoolean(ref uint state)
        {
            NextUint(ref state);
            return (state & 1) == 0;
        }
        public bool NextBoolean()
        {
            _boolState >>= 1;
            if (_boolState <= 1)
            {
                NextUint(ref _state);
                _boolState = _state;
            }
            return (_boolState & 1) == 0;
        }

        /// <summary>Gets a random value between 0 (inclusive) and 1 (inclusive).</summary>
        public static float NextFloat(ref uint state)
        {
            NextUint(ref state);
            return (float)state / uint.MaxValue;
        }
        /// <summary>Gets a random value between 0 (inclusive) and 1 (inclusive).</summary>
        public float NextFloat()
        {
            return NextFloat(ref _state);
        }

        /// <summary>Gets a random value between 0 (inclusive) and 1 (exclusive).</summary>
        public static float NextFloatNo1(ref uint state)
        {
            do
            {
                NextUint(ref state);
            } while (state == uint.MaxValue);
            return (float)state / uint.MaxValue;
        }
        /// <summary>Gets a random value between 0 (inclusive) and 1 (exclusive).</summary>
        public float NextFloatNo1()
        {
            return NextFloatNo1(ref _state);
        }

        /// <summary>Gets a random value between <paramref name="min"/> (inclusive) and <paramref name="max"/> (inclusive).</summary>
        public static float NextFloatRange(float min, float max, ref uint state)
        {
            return (NextFloat(ref state) * (max - min)) + min;
        }
        /// <summary>Gets a random value between <paramref name="min"/> (inclusive) and <paramref name="max"/> (inclusive).</summary>
        public float NextFloatRange(float min, float max)
        {
            return NextFloatRange(min, max, ref _state);
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
            return NextVector3(ref _state);
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
            return NextVector3Range(min, max, ref _state);
        }
    }
}
