using Silk.NET.OpenGL;
using System.Numerics;

namespace DerpySimulation.Render.Shaders
{
    internal sealed class BoxShader : GLShader
    {
        private const string VERTEX_SHADER_PATH = @"Shaders\Box.vert.glsl";
        private const string FRAGMENT_SHADER_PATH = @"Shaders\Box.frag.glsl";

        private readonly int _lProjectionView;
        private readonly int _lClippingPlane;

        private readonly int _lCameraPos;

        private readonly LitShaderUniforms _lightUniforms;

        public BoxShader(GL gl)
            : base(gl, VERTEX_SHADER_PATH, FRAGMENT_SHADER_PATH)
        {
            _lProjectionView = GetUniformLocation(gl, "projectionView");
            _lClippingPlane = GetUniformLocation(gl, "clippingPlane");

            _lCameraPos = GetUniformLocation(gl, "cameraPos");

            // Skip the sun
            _lightUniforms = new LitShaderUniforms(gl, this, max: LightController.MAX_LIGHTS - 1);
        }

        public void SetCamera(GL gl, in Matrix4x4 projectionView, in Vector3 camPos)
        {
            Matrix4(gl, _lProjectionView, projectionView);
            gl.Uniform3(_lCameraPos, camPos);
        }
        public void SetClippingPlane(GL gl, in Vector4 v)
        {
            gl.Uniform4(_lClippingPlane, v);
        }
        public void SetLights(GL gl)
        {
            // Skip the sun
            _lightUniforms.SetLights(gl, startIndex: 1);
        }
    }
}
