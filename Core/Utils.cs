using System;
using System.Numerics;

namespace DerpySimulation.Core
{
    internal static class Utils
    {
        public const float DegToRad = MathF.PI / 180f;
        public const float RadToDeg = 180f / MathF.PI;

        public static float GetYawRadians(this in Quaternion q)
        {
            return MathF.Atan2((2 * q.Y * q.W) - (2 * q.X * q.Z), 1 - (2 * q.Y * q.Y) - (2 * q.Z * q.Z));
        }
        public static float GetPitchRadians(this in Quaternion q)
        {
            return MathF.Atan2((2 * q.X * q.W) - (2 * q.Y * q.Z), 1 - (2 * q.X * q.X) - (2 * q.Z * q.Z));
        }
        public static float GetRollRadians(this in Quaternion q)
        {
            return MathF.Asin((2 * q.X * q.Y) + (2 * q.Z * q.W));
        }

        /// <summary>Calculates the normal of the triangle made from the 3 vertices. The vertices must be specified in counter-clockwise order.</summary>
        public static Vector3 CalcNormal(in Vector3 v0, in Vector3 v1, in Vector3 v2)
        {
            Vector3 tangentA = v1 - v0;
            Vector3 tangentB = v2 - v0;
            var normal = Vector3.Cross(tangentA, tangentB);
            return Vector3.Normalize(normal);
        }

        public static void LehmerRandomizer(ref uint state)
        {
            state += 0xE120FC15;
            ulong tmp = (ulong)state * 0x4A39B70D;
            state = (uint)((tmp >> 32) ^ tmp);
            tmp = (ulong)state * 0x12FAD5C9;
            state = (uint)((tmp >> 32) ^ tmp);
        }
        /// <summary>Gets a random value between 0 and 1 using <see cref="LehmerRandomizer"/></summary>
        public static float LehmerRandomizerFloat(ref uint state)
        {
            LehmerRandomizer(ref state);
            return (float)state / uint.MaxValue;
        }
        public static Vector3 RandomVector3(ref uint state)
        {
            return new Vector3(
                LehmerRandomizerFloat(ref state),
                LehmerRandomizerFloat(ref state),
                LehmerRandomizerFloat(ref state)
                );
        }

        public static float CosineInterpolation(float a, float b, float progress)
        {
            float theta = progress * MathF.PI;
            float f = (1 - MathF.Cos(theta)) * 0.5f;
            return (a * (1 - f)) + (b * f);
        }
        public static float BaryCentricInterpolation(in Vector3 p1, in Vector3 p2, in Vector3 p3, Vector2 pos)
        {
            float det = ((p2.Z - p3.Z) * (p1.X - p3.X)) + ((p3.X - p2.X) * (p1.Z - p3.Z));
            float a1 = (((p2.Z - p3.Z) * (pos.X - p3.X)) + ((p3.X - p2.X) * (pos.Y - p3.Z))) / det;
            float a2 = (((p3.Z - p1.Z) * (pos.X - p3.X)) + ((p1.X - p3.X) * (pos.Y - p3.Z))) / det;
            float a3 = 1f - a1 - a2;
            return (a1 * p1.Y) + (a2 * p2.Y) + (a3 * p3.Y);
        }
    }
}
