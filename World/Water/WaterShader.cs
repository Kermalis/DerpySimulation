using DerpySimulation.Render;
using Silk.NET.OpenGL;

namespace DerpySimulation.World.Water
{
    internal sealed class WaterShader : GLShader
    {
        private const string VERTEX_SHADER_FILE = "Shaders\\Water.vert.glsl";
        private const string FRAGMENT_SHADER_FILE = "Shaders\\Water.frag.glsl";

        private readonly int _lProjectionView;
        //private readonly int _lModel;

        private readonly int _lNumLights;
        private readonly int[] _lLightPos;
        private readonly int[] _lLightColor;
        private readonly int[] _lLightAttenuation;

        private readonly int _lHeight;
        private readonly int _lWaveTime;
        private readonly int _lNearFarPlanes;
        private readonly int _lCameraPos;

        private readonly int _lReflectionTexture;
        private readonly int _lRefractionTexture;
        private readonly int _lDepthTexture;

        public WaterShader(GL gl)
            : base(gl, VERTEX_SHADER_FILE, FRAGMENT_SHADER_FILE)
        {
            _lProjectionView = GetUniformLocation(gl, "projectionViewMatrix");
            //_lModel = GetUniformLocation(gl, "model");

            _lHeight = GetUniformLocation(gl, "height");
            _lWaveTime = GetUniformLocation(gl, "waveTime");
            _lNearFarPlanes = GetUniformLocation(gl, "nearFarPlanes");
            _lCameraPos = GetUniformLocation(gl, "cameraPos");

            _lReflectionTexture = GetUniformLocation(gl, "reflectionTexture");
            _lRefractionTexture = GetUniformLocation(gl, "refractionTexture");
            _lDepthTexture = GetUniformLocation(gl, "depthTexture");

            _lNumLights = GetUniformLocation(gl, "numLights");
            _lLightPos = new int[LightController.MAX_LIGHTS];
            _lLightColor = new int[LightController.MAX_LIGHTS];
            _lLightAttenuation = new int[LightController.MAX_LIGHTS];
            for (int i = 0; i < LightController.MAX_LIGHTS; i++)
            {
                _lLightPos[i] = GetUniformLocation(gl, "lightPos[" + i + ']');
                _lLightColor[i] = GetUniformLocation(gl, "lightColor[" + i + ']');
                _lLightAttenuation[i] = GetUniformLocation(gl, "lightAttenuation[" + i + ']');
            }

            Use(gl);

            gl.Uniform1(_lReflectionTexture, 0);
            gl.Uniform1(_lRefractionTexture, 1);
            gl.Uniform1(_lDepthTexture, 2);

            gl.UseProgram(0);
        }

        public void SetCamera(GL gl, Camera c)
        {
            Matrix4(gl, _lProjectionView, c.CreateViewMatrix() * c.Projection);
            gl.Uniform3(_lCameraPos, c.PR.Position);
            gl.Uniform2(_lNearFarPlanes, Camera.NEAR_PLANE, Camera.RENDER_DISTANCE);
        }
        public void SetLights(GL gl, LightController lights)
        {
            gl.Uniform1(_lNumLights, (uint)lights.Count);
            for (int i = 0; i < lights.Count; i++)
            {
                PointLight l = lights[i];
                gl.Uniform3(_lLightPos[i], ref l.Pos);
                gl.Uniform3(_lLightColor[i], ref l.Color);
                gl.Uniform3(_lLightAttenuation[i], ref l.Attenuation);
            }
        }

        public void SetWaveTime(GL gl, float v)
        {
            gl.Uniform1(_lWaveTime, v);
        }
        public void SetHeight(GL gl, float v)
        {
            //Matrix4(gl, _lModel, Matrix4x4.CreateTranslation(new Vector3(0, v, 0))); // For now, always at 0,0
            gl.Uniform1(_lHeight, v);
        }
    }
}
