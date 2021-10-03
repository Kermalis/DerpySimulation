using DerpySimulation.Render;
using Silk.NET.OpenGL;
using System.Numerics;

namespace DerpySimulation.World.Terrain
{
    internal sealed class TerrainRenderer
    {
        private readonly TerrainShader _terrainShader;

        public TerrainRenderer(GL gl)
        {
            _terrainShader = new TerrainShader(gl);
        }

        public void Render(GL gl, TerrainTile tile, in Matrix4x4 projectionViewMatrix, in Vector4 clippingPlane)
        {
            _terrainShader.Use(gl);

            _terrainShader.SetCamera(gl, projectionViewMatrix);
            _terrainShader.SetClippingPlane(gl, clippingPlane);
            _terrainShader.SetLights(gl, LightController.Instance);

            tile.Render(gl);

            gl.UseProgram(0);
        }
    }
}
