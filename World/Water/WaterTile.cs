using DerpySimulation.Render.Meshes;
using Silk.NET.OpenGL;

namespace DerpySimulation.World.Water
{
    internal sealed class WaterTile
    {
        private readonly Mesh _mesh;
        public float Y { get; }

        public float AnimTime;

        public WaterTile(Mesh m, float y)
        {
            _mesh = m;
            Y = y;
        }

        public void Render(GL gl)
        {
            _mesh.Render(gl);
        }

        public void Delete(GL gl)
        {
            _mesh.Delete(gl);
        }
    }
}
