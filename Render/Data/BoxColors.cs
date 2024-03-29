﻿using DerpySimulation.Core;
using System.Numerics;

namespace DerpySimulation.Render.Data
{
    internal struct BoxColors
    {
        public const int OffsetOfEast = 0;
        public const int OffsetOfWest = OffsetOfEast + (3 * sizeof(float));
        public const int OffsetOfUp = OffsetOfWest + (3 * sizeof(float));
        public const int OffsetOfDown = OffsetOfUp + (3 * sizeof(float));
        public const int OffsetOfSouth = OffsetOfDown + (3 * sizeof(float));
        public const int OffsetOfNorth = OffsetOfSouth + (3 * sizeof(float));
        public const uint SizeOf = OffsetOfNorth + (3 * sizeof(float));

        public Vector3 East;
        public Vector3 West;
        public Vector3 Up;
        public Vector3 Down;
        public Vector3 South;
        public Vector3 North;

        public BoxColors(in Vector3 baseColor, LehmerRand rand)
        {
            East = rand.NextVector3Range(0.75f, 1f) * baseColor;
            West = rand.NextVector3Range(0.75f, 1f) * baseColor;
            Up = rand.NextVector3Range(0.75f, 1f) * baseColor;
            Down = rand.NextVector3Range(0.25f, 0.75f) * baseColor; // Under is darkest
            South = rand.NextVector3Range(0.5f, 0.9f) * baseColor; // Butt is slightly darker
            North = rand.NextVector3Range(1.1f, 1.5f) * baseColor; // Face is brightest
        }

        public static BoxColors Lerp(in BoxColors a, in Vector3 b, float progress)
        {
            BoxColors ret;
            ret.East = Vector3.Lerp(a.East, b, progress);
            ret.West = Vector3.Lerp(a.West, b, progress);
            ret.Up = Vector3.Lerp(a.Up, b, progress);
            ret.Down = Vector3.Lerp(a.Down, b, progress);
            ret.South = Vector3.Lerp(a.South, b, progress);
            ret.North = Vector3.Lerp(a.North, b, progress);
            return ret;
        }
    }
}
