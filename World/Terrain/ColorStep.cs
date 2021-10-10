using DerpySimulation.Render;

namespace DerpySimulation.World.Terrain
{
    internal struct ColorStep
    {
        public readonly float Height;
        public readonly Color3 Color;

        public ColorStep(float height, in Color3 color)
        {
            Height = height;
            Color = color;
        }
    }
}
