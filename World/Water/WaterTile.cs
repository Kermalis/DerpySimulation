using DerpySimulation.Render;
using Silk.NET.OpenGL;

namespace DerpySimulation.World.Water
{
    internal sealed class WaterTile
    {
        public Model Model { get; }
        public float Y { get; }

        public float AnimTime;

        public WaterTile(Model m, float y)
        {
            Model = m;
            Y = y;
        }

        public void Render(GL gl)
        {
            Model.Render(gl);
        }

        public void Delete(GL gl)
        {
            Model.Delete(gl);
        }
    }
}
