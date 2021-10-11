using Silk.NET.OpenGL;

namespace DerpySimulation.Render.GUIs
{
    internal interface IGUIVisual
    {
        /// <summary>Renders this visual. Does not render children.</summary>
        void Render(GL gl, float delta);
    }
}
