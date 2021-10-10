using System.Numerics;

namespace DerpySimulation.Render
{
    internal sealed class PointLight
    {
        public Vector3 Pos;
        public Color3 Color;
        public Vector3 Attenuation;

        public PointLight(in Vector3 pos, in Color3 color)
        {
            Pos = pos;
            Color = color;
            Attenuation = new(1, 0, 0);
        }
        public PointLight(in Vector3 pos, in Color3 color, in Vector3 attenuation)
        {
            Pos = pos;
            Color = color;
            Attenuation = attenuation;
        }
    }
}
