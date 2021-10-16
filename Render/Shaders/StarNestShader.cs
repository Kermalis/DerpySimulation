using DerpySimulation.Core;
using DerpySimulation.Render.Meshes;
using Silk.NET.OpenGL;
using System;

namespace DerpySimulation.Render.Shaders
{
    internal sealed class StarNestShader : GLShader
    {
        private const string VERTEX_SHADER_PATH = @"Shaders\EntireScreen.vert.glsl";
        private const string FRAGMENT_SHADER_PATH = @"Shaders\StarNestFractal.frag.glsl";
        private const float MAX_TIME = 50_000; // Stay within OpenGL's precision

        public static StarNestShader Instance { get; private set; } = null!; // Initialized in RenderManager

        private float _time;

        private readonly int _lScreenSize;
        private readonly int _lDisplayTime;

        public StarNestShader(GL gl)
            : base(gl, VERTEX_SHADER_PATH, FRAGMENT_SHADER_PATH)
        {
            Instance = this;
            // Set a random position in the fractal to start so it's different on each boot
            uint seed = (uint)Environment.TickCount;
            _time = Utils.LehmerRandomizerFloat(ref seed) * MAX_TIME * 0.5f;

            _lScreenSize = GetUniformLocation(gl, "screenSize", false);
            _lDisplayTime = GetUniformLocation(gl, "displayTime");
        }

        public void Static_SetScreenSize(GL gl)
        {
            Use(gl);
            gl.Uniform2(_lScreenSize, Display.CurrentWidth, Display.CurrentHeight);
        }

        public void Render(GL gl, float delta)
        {
            Use(gl);
            _time = (_time + delta) % MAX_TIME;
            gl.Uniform1(_lDisplayTime, _time);

            SimpleRectMesh.Instance.Render(gl);
        }
    }
}
