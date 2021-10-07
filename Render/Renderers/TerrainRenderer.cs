using DerpySimulation.Render.Shaders;
using DerpySimulation.World.Terrain;
using Silk.NET.OpenGL;
using System.Numerics;

namespace DerpySimulation.Render.Renderers
{
    internal sealed class TerrainRenderer
    {
        private readonly TerrainShader _terrainShader;

        public TerrainRenderer(GL gl)
        {
            _terrainShader = new TerrainShader(gl);
        }

        public void Render(GL gl, TerrainTile tile, in Matrix4x4 projectionView, in Vector4 clippingPlane)
        {
            _terrainShader.Use(gl);

            _terrainShader.SetCamera(gl, projectionView);
            _terrainShader.SetClippingPlane(gl, clippingPlane);
            _terrainShader.SetLights(gl);

            tile.Render(gl);

            gl.UseProgram(0);
        }

        public void Delete(GL gl)
        {
            _terrainShader.Delete(gl);
        }
    }
}
