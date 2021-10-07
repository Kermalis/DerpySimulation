using Silk.NET.OpenGL;

namespace DerpySimulation.Render.Meshes
{
    internal sealed class Mesh
    {
        public readonly uint VAO;
        private readonly uint _count;
        private readonly bool _drawArrays;
        private readonly uint[] _vbos;

        public Mesh(uint vao, uint count, bool drawArrays, params uint[] vbos)
        {
            VAO = vao;
            _count = count;
            _drawArrays = drawArrays;
            _vbos = vbos;
        }

        public unsafe void Render(GL gl)
        {
            gl.BindVertexArray(VAO);

            if (_drawArrays)
            {
                gl.DrawArrays(PrimitiveType.Triangles, 0, _count);
            }
            else
            {
                gl.DrawElements(PrimitiveType.Triangles, _count, DrawElementsType.UnsignedInt, null);
            }

            gl.BindVertexArray(0);
        }
        public unsafe void RenderInstanced(GL gl, uint numInstances)
        {
            gl.BindVertexArray(VAO);

            if (_drawArrays)
            {
                gl.DrawArraysInstanced(PrimitiveType.Triangles, 0, _count, numInstances);
            }
            else
            {
                gl.DrawElementsInstanced(PrimitiveType.Triangles, _count, DrawElementsType.UnsignedInt, null, numInstances);
            }

            gl.BindVertexArray(0);
        }

        public void Delete(GL gl)
        {
            gl.DeleteVertexArray(VAO);
            gl.DeleteBuffers(_vbos);
        }
    }
}
