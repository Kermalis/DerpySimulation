using DerpySimulation.World.Terrain;
using DerpySimulation.World.Water;
using Silk.NET.OpenGL;

namespace DerpySimulation.Render
{
    internal sealed class Model
    {
        private readonly uint _vao;
        private readonly uint _count;
        private readonly bool _drawArrays;

        public Model(uint vao, uint count, bool drawArrays)
        {
            _vao = vao;
            _count = count;
            _drawArrays = drawArrays;
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

            return new Model(vao, (uint)indices.Length, false);
        }
        public unsafe static Model CreateWaterModel(GL gl, WaterVBOData[] vertices, uint numVertices)
        {
            // Create vao
            uint vao = gl.CreateVertexArray();
            gl.BindVertexArray(vao);

            // Create vbo
            uint vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            fixed (void* data = vertices)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, WaterVBOData.SizeOf * numVertices, data, BufferUsageARB.StaticDraw);
            }

            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, WaterVBOData.SizeOf, (void*)WaterVBOData.OffsetOfPos);
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Byte, false, WaterVBOData.SizeOf, (void*)WaterVBOData.OffsetOfPartnerVertex1);
            gl.EnableVertexAttribArray(2);
            gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Byte, false, WaterVBOData.SizeOf, (void*)WaterVBOData.OffsetOfPartnerVertex2);

            gl.BindVertexArray(0);

            return new Model(vao, numVertices, true);
        }

        public unsafe void Render(GL gl)
        {
            gl.BindVertexArray(_vao);

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

        public void Delete(GL gl)
        {
            gl.DeleteVertexArray(_vao);
            // TODO: Delete vbo and ebo? Or is it automatic?
        }
    }
}
