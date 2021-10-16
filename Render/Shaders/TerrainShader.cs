using Silk.NET.OpenGL;
using System.Numerics;

namespace DerpySimulation.Render.Shaders
{
    internal sealed class TerrainShader : GLShader
    {
        private const string VERTEX_SHADER_PATH = @"Shaders\Terrain.vert.glsl";
        private const string FRAGMENT_SHADER_PATH = @"Shaders\Terrain.frag.glsl";

        private readonly int _lProjectionView;
        private readonly int _lTransform;
        private readonly int _lClippingPlane;

        private readonly LitShaderUniforms _lightUniforms;

        public TerrainShader(GL gl)
            : base(gl, VERTEX_SHADER_PATH, FRAGMENT_SHADER_PATH)
        {
            _lProjectionView = GetUniformLocation(gl, "projectionView");
            _lTransform = GetUniformLocation(gl, "transform");
            _lClippingPlane = GetUniformLocation(gl, "clippingPlane");

            _lightUniforms = new LitShaderUniforms(gl, this);

            // FOR NOW, TRANSFORM IS ALWAYS DEFAULT
            Use(gl);
            Matrix4(gl, _lTransform, Matrix4x4.CreateTranslation(Vector3.Zero));
        }

        public void SetCamera(GL gl, in Matrix4x4 projectionView)
        {
            Matrix4(gl, _lProjectionView, projectionView);
        }
        public void SetClippingPlane(GL gl, in Vector4 v)
        {
            gl.Uniform4(_lClippingPlane, v);
        }
        public void SetLights(GL gl)
        {
            _lightUniforms.SetLights(gl);
        }
    }
}
