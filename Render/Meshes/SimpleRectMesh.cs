using Silk.NET.OpenGL;
using System.Numerics;

namespace DerpySimulation.Render.Meshes
{
    internal class SimpleRectMesh
    {
        public static SimpleRectMesh Instance { get; private set; } = null!; // Initialized in RenderManager

        private static readonly Vector2[] _vertices = new Vector2[4]
        {
            new(0, 0),
            new(0, 1),
            new(1, 0),
            new(1, 1)
        };

        private readonly uint _vao;
        private readonly uint _vbo;

        public unsafe SimpleRectMesh(GL gl)
        {
            Instance = this;

            // Create vao
            _vao = gl.GenVertexArray();
            gl.BindVertexArray(_vao);

            // Create vbo
            _vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            const uint sizeOfVec2 = sizeof(float) * 2;
            fixed (void* data = _vertices)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, sizeOfVec2 * 4, data, BufferUsageARB.StaticDraw);
            }

            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, sizeOfVec2, (void*)0);

            gl.BindVertexArray(0);
        }

        public void Render(GL gl)
        {
            gl.BindVertexArray(_vao);

            gl.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            gl.BindVertexArray(0);
        }

        public void Delete(GL gl)
        {
            gl.DeleteVertexArray(_vao);
            gl.DeleteBuffer(_vbo);
        }
    }
}
