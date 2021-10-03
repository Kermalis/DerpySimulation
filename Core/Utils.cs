using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace DerpySimulation.Core
{
    internal static class Utils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double GetProgress(TimeSpan end, TimeSpan cur)
        {
            if (cur >= end)
            {
                return 1;
            }
            return ((cur - end) / end) + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DegreesToRadiansF(float degrees)
        {
            return MathF.PI / 180 * degrees;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RadiansToDegreesF(float radians)
        {
            return 180 / MathF.PI * radians;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double DegreesToRadians(double degrees)
        {
            return Math.PI / 180 * degrees;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double RadiansToDegrees(double radians)
        {
            return 180 / Math.PI * radians;
        }
        public static float GetYawRadiansF(this Quaternion q)
        {
            return MathF.Atan2((2 * q.Y * q.W) - (2 * q.X * q.Z), 1 - (2 * q.Y * q.Y) - (2 * q.Z * q.Z));
        }
        public static float GetPitchRadiansF(this Quaternion q)
        {
            return MathF.Atan2((2 * q.X * q.W) - (2 * q.Y * q.Z), 1 - (2 * q.X * q.X) - (2 * q.Z * q.Z));
        }
        public static float GetRollRadiansF(this Quaternion q)
        {
            return MathF.Asin((2 * q.X * q.Y) + (2 * q.Z * q.W));
        }

        /// <summary>Calculates the normal of the triangle made from the 3 vertices. The vertices must be specified in counter-clockwise order.</summary>
        public static Vector3 CalcNormal(in Vector3 v0, in Vector3 v1, in Vector3 v2)
        {
            var tangentA = Vector3.Subtract(v1, v0);
            var tangentB = Vector3.Subtract(v2, v0);
            var normal = Vector3.Cross(tangentA, tangentB);
            return Vector3.Normalize(normal);
        }

        /// <summary>Blends between <paramref name="color1"/> and <paramref name="color2"/>. <paramref name="color2Amt"/> is between 0 and 1.</summary>
        public static Vector3 InterpolateColors(Vector3 color1, Vector3 color2, float color2Amt)
        {
            float color1Weight = 1f - color2Amt;
            float r = (color1Weight * color1.X) + (color2Amt * color2.X);
            float g = (color1Weight * color1.Y) + (color2Amt * color2.Y);
            float b = (color1Weight * color1.Z) + (color2Amt * color2.Z);
            return new Vector3(r, g, b);
        }

        public static uint LehmerRandomizer(uint state)
        {
            state += 0xE120FC15;
            ulong tmp = (ulong)state * 0x4A39B70D;
            uint m1 = (uint)((tmp >> 32) ^ tmp);
            tmp = (ulong)m1 * 0x12FAD5C9;
            uint m2 = (uint)((tmp >> 32) ^ tmp);
            return m2;
        }
        public static float CosineInterpolation(float a, float b, float blend)
        {
            float theta = blend * MathF.PI;
            float f = (1 - MathF.Cos(theta)) * 0.5f;
            return (a * (1 - f)) + (b * f);
        }
        public static float BarryCentric(in Vector3 p1, in Vector3 p2, in Vector3 p3, Vector2 pos)
        {
            float det = (p2.Z - p3.Z) * (p1.X - p3.X) + (p3.X - p2.X) * (p1.Z - p3.Z);
            float l1 = ((p2.Z - p3.Z) * (pos.X - p3.X) + (p3.X - p2.X) * (pos.Y - p3.Z)) / det;
            float l2 = ((p3.Z - p1.Z) * (pos.X - p3.X) + (p1.X - p3.X) * (pos.Y - p3.Z)) / det;
            float l3 = 1 - l1 - l2;
            return l1 * p1.Y + l2 * p2.Y + l3 * p3.Y;
        }
    }
}
