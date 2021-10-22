using System;
using System.Collections.Generic;
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
        /// <summary><paramref name="forward"/> is calculated as (from - to) and must be normalized. Breaks if <paramref name="forward"/> is parallel to <paramref name="up"/>.</summary>
        public static Quaternion CreateLookRotation(in Vector3 forward, in Vector3 up)
        {
            // https://stackoverflow.com/a/52551983
            // First check for up and -up, because the cross product would be 0
            if (forward == up)
            {
                return Rotation.CreateQuaternion(up.X * 90, up.Y * 90, up.Z * 90); // These values due to OpenGL's coordinate system
            }
            if (forward == -up)
            {
                return Rotation.CreateQuaternion(up.X * -90, up.Y * -90, up.Z * -90);
            }
            var rSide = Vector3.Normalize(Vector3.Cross(up, forward)); // Rotated side (normalized again because forward and up are not perpendicular)
            var rUp = Vector3.Cross(forward, rSide); // Rotated up

            Quaternion q;
            float s;
            float trace = rSide.X + rUp.Y + forward.Z;
            if (trace > 0)
            {
                s = 0.5f / MathF.Sqrt(trace + 1);
                q.X = (rUp.Z - forward.Y) * s;
                q.Y = (forward.X - rSide.Z) * s;
                q.Z = (rSide.Y - rUp.X) * s;
                q.W = 0.25f / s;
                return q;
            }
            if (rSide.X > rUp.Y && rSide.X > forward.Z)
            {
                s = 2 * MathF.Sqrt(1 + rSide.X - rUp.Y - forward.Z);
                q.X = 0.25f * s;
                q.Y = (rUp.X + rSide.Y) / s;
                q.Z = (forward.X + rSide.Z) / s;
                q.W = (rUp.Z - forward.Y) / s;
                return q;
            }
            if (rUp.Y > forward.Z)
            {
                s = 2 * MathF.Sqrt(1 + rUp.Y - rSide.X - forward.Z);
                q.X = (rUp.X + rSide.Y) / s;
                q.Y = 0.25f * s;
                q.Z = (forward.Y + rUp.Z) / s;
                q.W = (forward.X - rSide.Z) / s;
                return q;
            }
            s = 2 * MathF.Sqrt(1 + forward.Z - rSide.X - rUp.Y);
            q.X = (forward.X + rSide.Z) / s;
            q.Y = (forward.Y + rUp.Z) / s;
            q.Z = 0.25f * s;
            q.W = (rSide.Y - rUp.X) / s;
            return q;
        }
        public static Vector3 Average<TSource>(this IEnumerable<TSource> source, Func<TSource, Vector3> selector)
        {
            Vector3 sum = Vector3.Zero;
            long count = 0;
            foreach (TSource s in source)
            {
                sum += selector(s);
                count++;
            }
            if (count > 0)
            {
                return sum / count;
            }
            throw new InvalidOperationException("source had no elements.");
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
