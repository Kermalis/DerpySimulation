using Silk.NET.OpenGL;
using System.Numerics;

namespace DerpySimulation.Render.Meshes
{
    internal sealed class SimpleRectMesh
    {
        public static SimpleRectMesh Instance { get; private set; } = null!; // Initialized in RenderManager

        private static readonly Vector2[] _verticesGL = new Vector2[4]
        {
            new Vector2(-1,  1), // Top Left
            new Vector2(-1, -1), // Bottom Left
            new Vector2( 1,  1), // Top Right
            new Vector2( 1, -1)  // Bottom Right
        };
        private static readonly Vector2[] _verticesRel = new Vector2[4]
        {
            new Vector2(0, 0), // Top Left
            new Vector2(0, 1), // Bottom Left
            new Vector2(1, 0), // Top Right
            new Vector2(1, 1)  // Bottom Right
        };

        private readonly uint _vaoGL;
        private readonly uint _vboGL;
        private readonly uint _vaoRel;
        private readonly uint _vboRel;

        public unsafe SimpleRectMesh(GL gl)
        {
            Instance = this;

            // Create vao
            _vaoGL = gl.GenVertexArray();
            gl.BindVertexArray(_vaoGL);

            // Create vbo
            _vboGL = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vboGL);
            fixed (void* data = _verticesGL)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)sizeof(Vector2) * 4, data, BufferUsageARB.StaticDraw);
            }

            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, (uint)sizeof(Vector2), null);

            // Create vao
            _vaoRel = gl.GenVertexArray();
            gl.BindVertexArray(_vaoRel);

            // Create vbo
            _vboRel = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vboRel);
            fixed (void* data = _verticesRel)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)sizeof(Vector2) * 4, data, BufferUsageARB.StaticDraw);
            }

            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, (uint)sizeof(Vector2), null);
        }

        public void RenderGL(GL gl)
        {
            gl.BindVertexArray(_vaoGL);
            gl.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        }
        public void RenderRel(GL gl)
        {
            gl.BindVertexArray(_vaoRel);
            gl.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        }

        public void Delete(GL gl)
        {
            gl.DeleteVertexArray(_vaoGL);
            gl.DeleteBuffer(_vboGL);
            gl.DeleteVertexArray(_vaoRel);
            gl.DeleteBuffer(_vboRel);
        }
    }
}
