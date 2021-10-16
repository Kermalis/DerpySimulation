using DerpySimulation.Render.Cameras;
using DerpySimulation.World.Terrain;
using DerpySimulation.World.Water;
using Silk.NET.OpenGL;
using System.Numerics;

namespace DerpySimulation.Render.Renderers
{
    internal sealed class SimulationRenderer
    {
        // These offsets prevent edge artifacts between the water and the terrain
        private const float REFLECT_OFFSET = 0.1f;
        private const float REFRACT_OFFSET = 1f;

        private readonly TerrainRenderer _terrainRenderer;
        private readonly WaterRenderer _waterRenderer;

        private bool _mustCreateFBOs;
        private uint _reflectionFBO;
        private uint _reflectionTexture;
        private uint _refractionFBO;
        private uint _refractionTexture;
        private uint _depthTexture;
        private uint _rbo;

        public SimulationRenderer(GL gl)
        {
            _terrainRenderer = new TerrainRenderer(gl);
            _waterRenderer = new WaterRenderer(gl);

            _mustCreateFBOs = true;
            Display.Resized += ProgramMain_Resized;
        }

        // On window resize; recreate water fbos
        private void ProgramMain_Resized()
        {
            _mustCreateFBOs = true;
        }

        private void CreateReflectionAndRefractionFBOs(GL gl, uint width, uint height)
        {
            DeleteReflectionAndRefractionStuff(gl);
            _reflectionFBO = RenderUtils.CreateReflectionFBO(gl, width, height, out _reflectionTexture, out _rbo);
            _refractionFBO = RenderUtils.CreateRefractionFBO(gl, width, height, out _refractionTexture, out _depthTexture);
        }

        public void Render(GL gl, Camera cam, TerrainTile terrain, WaterTile water)
        {
            if (_mustCreateFBOs)
            {
                _mustCreateFBOs = false;
                CreateReflectionAndRefractionFBOs(gl, Display.CurrentWidth, Display.CurrentHeight);
            }

#if DEBUG_WIREFRAME
            gl.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
#endif
            // Do not render water while below it
            // TODO: Underwater effect/shader (probably post processing effect)
            bool aboveWater = WaterRenderer.ShouldRenderWater(cam.PR.Position.Y, water.Y);
            if (aboveWater)
            {
                gl.Enable(EnableCap.ClipDistance0);
                ReflectionPass(gl, cam, terrain, water.Y);
                RefractionPass(gl, cam, terrain, water.Y);
                gl.Disable(EnableCap.ClipDistance0);
            }
            MainPass(gl, cam, terrain, water, aboveWater);
#if DEBUG_WIREFRAME
            gl.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill); // Reset
#endif
        }
        /// <summary>Prepares the current fbo for rendering.</summary>
        private static void PreparePass(GL gl)
        {
            gl.Enable(EnableCap.DepthTest);
            gl.Enable(EnableCap.CullFace);
            gl.CullFace(CullFaceMode.Back);
            gl.Enable(EnableCap.Multisample);
            gl.ProvokingVertex(VertexProvokingMode.FirstVertexConvention);
            //gl.ClearColor(0.49f, 0.89f, 0.98f, 1f); // Sky color
            gl.ClearColor(0f, 0f, 0f, 1f); // Black
            gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }
        private void ReflectionPass(GL gl, Camera cam, TerrainTile terrain, float waterY)
        {
            RenderUtils.BindFBO(gl, _reflectionFBO, DrawBufferMode.ColorAttachment0, Display.CurrentWidth, Display.CurrentHeight);
            PreparePass(gl);
            Matrix4x4 projectionView = cam.CreateReflectionViewMatrix(waterY) * cam.Projection;
            var clippingPlane = new Vector4(0, 1, 0, -waterY + REFLECT_OFFSET);

            RenderLandStuff(gl, terrain, cam.PR.Position, projectionView, clippingPlane);

            RenderUtils.UnbindFBO(gl);
        }
        private void RefractionPass(GL gl, Camera cam, TerrainTile terrain, float waterY)
        {
            RenderUtils.BindFBO(gl, _refractionFBO, DrawBufferMode.ColorAttachment0, Display.CurrentWidth, Display.CurrentHeight);
            PreparePass(gl);
            Matrix4x4 projectionView = cam.CreateViewMatrix() * cam.Projection;
            var clippingPlane = new Vector4(0, -1, 0, waterY + REFRACT_OFFSET);

            RenderLandStuff(gl, terrain, cam.PR.Position, projectionView, clippingPlane);

            RenderUtils.UnbindFBO(gl);
        }
        private void MainPass(GL gl, Camera cam, TerrainTile terrain, WaterTile water, bool aboveWater)
        {
            PreparePass(gl);
            Matrix4x4 projectionView = cam.CreateViewMatrix() * cam.Projection;
            Vector4 clippingPlane = Vector4.Zero;

            RenderLandStuff(gl, terrain, cam.PR.Position, projectionView, clippingPlane);

            if (aboveWater)
            {
                _waterRenderer.Render(gl, water, cam, _reflectionTexture, _refractionTexture, _depthTexture);
            }
        }

        private void RenderLandStuff(GL gl, TerrainTile terrain, in Vector3 camPos, in Matrix4x4 projectionView, in Vector4 clippingPlane)
        {
            _terrainRenderer.Render(gl, terrain, projectionView, clippingPlane);
            FoodRenderer.Instance.Render(gl, projectionView, camPos, clippingPlane);
        }

        public void Delete(GL gl)
        {
            _terrainRenderer.Delete(gl);
            _waterRenderer.Delete(gl);
            DeleteReflectionAndRefractionStuff(gl);
        }
        private void DeleteReflectionAndRefractionStuff(GL gl)
        {
            gl.DeleteFramebuffer(_refractionFBO);
            gl.DeleteFramebuffer(_reflectionFBO);
            gl.DeleteTexture(_refractionTexture);
            gl.DeleteTexture(_depthTexture);
            gl.DeleteTexture(_reflectionTexture);
            gl.DeleteRenderbuffer(_rbo);
        }
    }
}
