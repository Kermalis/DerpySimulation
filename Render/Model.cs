using DerpySimulation.World;
using Silk.NET.OpenGL;

namespace DerpySimulation.Render
{
    internal sealed class Model
    {
        public uint VAO { get; }
        public uint ElementCount { get; }

        public Model(uint vao, uint elementCount)
        {
            VAO = vao;
            ElementCount = elementCount;
        }

        public unsafe static Model CreateTerrainModel(GL gl, TerrainVBOData[] vertices, uint[] indices)
        {
            // Create vao
            uint vao = gl.CreateVertexArray();
            gl.BindVertexArray(vao);

            // Create ebo
            uint ebo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
            fixed (void* data = indices)
            {
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, sizeof(uint) * (uint)indices.Length, data, BufferUsageARB.StaticDraw);
            }

            // Create vbo
            uint vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            fixed (void* data = vertices)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, TerrainVBOData.SizeOf * (uint)vertices.Length, data, BufferUsageARB.StaticDraw);
            }

            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, TerrainVBOData.SizeOf, (void*)TerrainVBOData.OffsetOfPos);
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, TerrainVBOData.SizeOf, (void*)TerrainVBOData.OffsetOfNormal);
            gl.EnableVertexAttribArray(2);
            gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, TerrainVBOData.SizeOf, (void*)TerrainVBOData.OffsetOfColor);

            gl.BindVertexArray(0);

            return new Model(vao, (uint)indices.Length);
        }

        public unsafe void Render(GL gl)
        {
            gl.BindVertexArray(VAO);

            gl.DrawElements(PrimitiveType.Triangles, ElementCount, DrawElementsType.UnsignedInt, null);

            gl.BindVertexArray(0);
        }

        public void Delete(GL gl)
        {
            gl.DeleteVertexArray(VAO);
            // TODO: Delete vbo and ebo? Or is it automatic?
        }
    }
}
