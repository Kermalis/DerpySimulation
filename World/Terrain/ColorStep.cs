using System.Numerics;

namespace DerpySimulation.World.Terrain
{
    internal struct ColorStep
    {
        public readonly float Height;
        public readonly Vector3 Color;

        public ColorStep(float height, in Vector3 color)
        {
            Height = height;
            Color = color;
        }
    }
}
