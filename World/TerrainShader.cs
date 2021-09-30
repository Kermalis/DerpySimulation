using DerpySimulation.Render;
using Silk.NET.OpenGL;
using System.Numerics;

namespace DerpySimulation.World
{
    internal sealed class TerrainShader : GLShader
    {
        private const string VERTEX_SHADER_FILE = "Shaders\\Terrain.vert.glsl";
        private const string FRAGMENT_SHADER_FILE = "Shaders\\Terrain.frag.glsl";

        private readonly int _lProjectionView;
        private readonly int _lModel;

        private readonly int _lNumLights;
        private readonly int[] _lLightPos;
        private readonly int[] _lLightColor;
        private readonly int[] _lLightAttenuation;

        public TerrainShader(GL gl)
            : base(gl, VERTEX_SHADER_FILE, FRAGMENT_SHADER_FILE)
        {
            _lProjectionView = GetUniformLocation(gl, "projectionViewMatrix");
            _lModel = GetUniformLocation(gl, "model");

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

            // FOR NOW, MODEL IS ALWAYS DEFAULT
            Use(gl);
            Matrix4(gl, _lModel, Matrix4x4.CreateTranslation(Vector3.Zero));
            gl.UseProgram(0);
        }

        public void SetCamera(GL gl, Camera c)
        {
            Matrix4(gl, _lProjectionView, c.CreateViewMatrix() * c.Projection);
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
    }
}
