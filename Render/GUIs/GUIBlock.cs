using DerpySimulation.Render.GUIs.Positioning;
using DerpySimulation.Render.Shaders;
using Silk.NET.OpenGL;
using System.Numerics;

namespace DerpySimulation.Render.GUIs
{
    internal sealed class GUIBlock : GUIComponent, IGUIVisual
    {
        private static readonly Vector2[] _vertices = new Vector2[4]
        {
            new(0, 0),
            new(0, 1),
            new(1, 0),
            new(1, 1)
        };

        private static readonly uint _vao;
        private static readonly uint _vbo;

        static unsafe GUIBlock()
        {
            GL gl = Display.OpenGL;
            // Create vao
            _vao = gl.GenVertexArray();
            gl.BindVertexArray(_vao);

            // Create vbo
            _vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            const uint sizeOfVec2 = sizeof(float) * 2;
            fixed (void* d = _vertices)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, sizeOfVec2 * 4, d, BufferUsageARB.StaticDraw);
            }

            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, sizeOfVec2, (void*)0);

            gl.BindVertexArray(0);
        }

        private readonly Color4 _color;

        public GUIBlock(in Color4 color)
        {
            _color = color;
        }

        protected override void Init(GL gl)
        {
            //
        }
        protected override void Update(GL gl, float delta)
        {
            //
        }

        public void Render(GL gl, float delta)
        {
            gl.BindVertexArray(_vao);

            GUIShader shader = GUIShader.Instance;
            shader.Use(gl);
            ref GUIRect pos = ref RelPos;
            gl.Uniform2(shader.LRelPos, pos.X, pos.Y);
            gl.Uniform2(shader.LRelSize, pos.W, pos.H);
            Color4.PutInShader(gl, shader.LColor, _color);

            gl.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            gl.UseProgram(0);
            gl.BindVertexArray(0);
        }

        public static void Quit(GL gl)
        {
            gl.DeleteVertexArray(_vao);
            gl.DeleteBuffer(_vbo);
        }
    }
}
