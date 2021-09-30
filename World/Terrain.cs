using DerpySimulation.Render;
using Silk.NET.OpenGL;

namespace DerpySimulation.World
{
    internal sealed class Terrain
    {
        /// <summary>Size in GL units in the x and z directions.</summary>
        public const uint SIZE = 2000;

        public Model Model { get; }

        public Terrain(Model m)
        {
            Model = m;
        }

        public void Render(GL gl)
        {
            Model.Render(gl);
        }
    }
}
