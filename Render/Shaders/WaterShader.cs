using DerpySimulation.Render.Cameras;
using Silk.NET.OpenGL;

namespace DerpySimulation.Render.Shaders
{
    internal sealed class WaterShader : GLShader
    {
        private const string VERTEX_SHADER_PATH = @"Shaders\Water.vert.glsl";
        private const string FRAGMENT_SHADER_PATH = @"Shaders\Water.frag.glsl";

        private readonly int _lProjectionView;
        //private readonly int _lTransform;

        private readonly int _lHeight;
        private readonly int _lWaveTime;
        private readonly int _lNearFarPlanes;
        private readonly int _lCameraPos;

        private readonly int _lReflectionTexture;
        private readonly int _lRefractionTexture;
        private readonly int _lDepthTexture;

        private readonly LitShaderUniforms _lightUniforms;

        public WaterShader(GL gl)
            : base(gl, VERTEX_SHADER_PATH, FRAGMENT_SHADER_PATH)
        {
            _lProjectionView = GetUniformLocation(gl, "projectionView");
            //_lTransform = GetUniformLocation(gl, "transform");

            _lHeight = GetUniformLocation(gl, "height");
            _lWaveTime = GetUniformLocation(gl, "waveTime");
            _lNearFarPlanes = GetUniformLocation(gl, "nearFarPlanes");
            _lCameraPos = GetUniformLocation(gl, "cameraPos");

            _lReflectionTexture = GetUniformLocation(gl, "reflectionTexture");
            _lRefractionTexture = GetUniformLocation(gl, "refractionTexture");
            _lDepthTexture = GetUniformLocation(gl, "depthTexture");

            _lightUniforms = new LitShaderUniforms(gl, this);

            Use(gl);
            gl.Uniform1(_lReflectionTexture, 0);
            gl.Uniform1(_lRefractionTexture, 1);
            gl.Uniform1(_lDepthTexture, 2);
        }

        public void SetCamera(GL gl, Camera c)
        {
            Matrix4(gl, _lProjectionView, c.CreateViewMatrix() * c.Projection);
            gl.Uniform3(_lCameraPos, c.PR.Position);
            gl.Uniform2(_lNearFarPlanes, Camera.NEAR_PLANE, Camera.RENDER_DISTANCE);
        }
        public void SetLights(GL gl)
        {
            _lightUniforms.SetLights(gl);
        }

        public void SetWaveTime(GL gl, float v)
        {
            gl.Uniform1(_lWaveTime, v);
        }
        public void SetHeight(GL gl, float v)
        {
            //Matrix4(gl, _lTransform, Matrix4x4.CreateTranslation(new Vector3(0, v, 0))); // For now, always at 0,0
            gl.Uniform1(_lHeight, v);
        }
    }
}
