using DerpySimulation.Render.Cameras;
using DerpySimulation.Render.Shaders;
using DerpySimulation.World.Water;
using Silk.NET.OpenGL;

namespace DerpySimulation.Render.Renderers
{
    internal sealed class WaterRenderer
    {
        private const float WAVE_SPEED = 0.002f;
        private const float WAVE_HEIGHT = 0.25f; // Same value as in the shader

        private readonly WaterShader _shader;

        public WaterRenderer(GL gl)
        {
            _shader = new WaterShader(gl);
        }

        public static bool ShouldRenderWater(float camY, float waterY)
        {
            return camY > waterY + WaterRenderer.WAVE_HEIGHT;
        }
        public void Render(GL gl, WaterTile tile, Camera cam, uint reflectionTexture, uint refractionTexture, uint depthTexture)
        {
            tile.AnimTime += WAVE_SPEED;

            gl.Enable(EnableCap.Blend);
            gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            _shader.Use(gl);
            _shader.SetWaveTime(gl, tile.AnimTime);
            _shader.SetCamera(gl, cam);
            _shader.SetLights(gl);
            _shader.SetHeight(gl, tile.Y);

            gl.ActiveTexture(TextureUnit.Texture0);
            gl.BindTexture(TextureTarget.Texture2D, reflectionTexture);
            gl.ActiveTexture(TextureUnit.Texture1);
            gl.BindTexture(TextureTarget.Texture2D, refractionTexture);
            gl.ActiveTexture(TextureUnit.Texture2);
            gl.BindTexture(TextureTarget.Texture2D, depthTexture);

            tile.Render(gl);

            // Done
            gl.Disable(EnableCap.Blend);
        }

        public void Delete(GL gl)
        {
            _shader.Delete(gl);
        }
    }
}
