using Silk.NET.OpenGL;

namespace DerpySimulation.Render.Shaders
{
    internal sealed class GUIShader : GLShader
    {
        private const string VERTEX_SHADER_PATH = @"Shaders\GUI.vert.glsl";
        private const string FRAGMENT_SHADER_PATH = @"Shaders\GUI.frag.glsl";

        public static GUIShader Instance { get; private set; } = null!; // Initialized in RenderManager

        public readonly int LRelPos;
        public readonly int LRelSize;
        public readonly int LColor;

        public GUIShader(GL gl)
            : base(gl, VERTEX_SHADER_PATH, FRAGMENT_SHADER_PATH)
        {
            Instance = this;

            LRelPos = GetUniformLocation(gl, "relPos");
            LRelSize = GetUniformLocation(gl, "relSize");
            LColor = GetUniformLocation(gl, "color");
        }
    }
}
