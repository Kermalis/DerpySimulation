using DerpySimulation.Render.Data;
using DerpySimulation.Render.Meshes;
using DerpySimulation.World.Terrain;
using Silk.NET.OpenGL;
using System.Numerics;

namespace DerpySimulation.World.Water
{
    /// <summary>Creates a flat mesh of water. Indicators are used to know which other vertices the current vertex is paired with to make a triangle.</summary>
    internal static class WaterGenerator
    {
        public static WaterTile Generate(GL gl, in SimulationCreationSettings settings, TerrainTile terrain)
        {
            VBOData_Water[] waterMeshData = CreateMeshData(settings, terrain, out uint numData);
            return new WaterTile(CreateMesh(gl, waterMeshData, numData), settings.WaterLevel);
        }

        private static VBOData_Water[] CreateMeshData(in SimulationCreationSettings settings, TerrainTile terrain, out uint bufferIdx)
        {
            var buffer = new VBOData_Water[6 * settings.SizeX * settings.SizeZ];
            bufferIdx = 0;
            for (int z = 0; z < settings.SizeZ; z++)
            {
                for (int x = 0; x < settings.SizeX; x++)
                {
                    StoreGridSquare(buffer, ref bufferIdx, x, z, settings.WaterLevel, terrain);
                }
            }
            return buffer;
        }
        private static unsafe Mesh CreateMesh(GL gl, VBOData_Water[] vertices, uint numVertices)
        {
            // Create vao
            uint vao = gl.CreateVertexArray();
            gl.BindVertexArray(vao);

            // Create vbo
            uint vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            fixed (void* data = vertices)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, VBOData_Water.SizeOf * numVertices, data, BufferUsageARB.StaticDraw);
            }

            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, VBOData_Water.SizeOf, (void*)VBOData_Water.OffsetOfPos);
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Byte, false, VBOData_Water.SizeOf, (void*)VBOData_Water.OffsetOfPartnerVertex1);
            gl.EnableVertexAttribArray(2);
            gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Byte, false, VBOData_Water.SizeOf, (void*)VBOData_Water.OffsetOfPartnerVertex2);

            gl.BindVertexArray(0);

            return new Mesh(vao, numVertices, true, vbo);
        }

        private static void StoreGridSquare(VBOData_Water[] buffer, ref uint bufferIdx, int x, int z, float waterY, TerrainTile terrain)
        {
            Vector2[] cornerPos = CalculateCornerPositions(x, z);
            StoreTriangle(buffer, ref bufferIdx, cornerPos, true, waterY, terrain);
            StoreTriangle(buffer, ref bufferIdx, cornerPos, false, waterY, terrain);
        }
        private static void StoreTriangle(VBOData_Water[] buffer, ref uint bufferIdx, Vector2[] cornerPos, bool left, float waterY, TerrainTile terrain)
        {
            int index0 = left ? 0 : 2;
            int index1 = 1;
            int index2 = left ? 2 : 3;
            // Check if all 3 vertices are under the terrain, and if so, don't place them
            Vector2 v0 = cornerPos[index0];
            Vector2 v1 = cornerPos[index1];
            Vector2 v2 = cornerPos[index2];
            if (terrain.GetHeight(v0.X, v0.Y) > waterY
                && terrain.GetHeight(v1.X, v1.Y) > waterY
                && terrain.GetHeight(v2.X, v2.Y) > waterY)
            {
                return;
            }
            StoreVertex(buffer, ref bufferIdx, v0, index0, cornerPos, index1, index2);
            StoreVertex(buffer, ref bufferIdx, v1, index1, cornerPos, index2, index0);
            StoreVertex(buffer, ref bufferIdx, v2, index2, cornerPos, index0, index1);
        }
        private static void StoreVertex(VBOData_Water[] buffer, ref uint bufferIdx, Vector2 cornerPos, int currentVertex, Vector2[] vertexPositions, int vertex1, int vertex2)
        {
            GetIndicators(currentVertex, vertexPositions, vertex1, vertex2, out Vector2 offset1, out Vector2 offset2);
            buffer[bufferIdx++] = new VBOData_Water(cornerPos, offset1, offset2);
        }

        private static Vector2[] CalculateCornerPositions(int x, int z)
        {
            var vertices = new Vector2[4];
            vertices[0] = new Vector2(x, z);
            vertices[1] = new Vector2(x, z + 1);
            vertices[2] = new Vector2(x + 1, z);
            vertices[3] = new Vector2(x + 1, z + 1);
            return vertices;
        }
        private static void GetIndicators(int currentVertex, Vector2[] vertexPositions, int vertex1, int vertex2, out Vector2 offset1, out Vector2 offset2)
        {
            Vector2 currentVertexPos = vertexPositions[currentVertex];
            Vector2 vertex1Pos = vertexPositions[vertex1];
            Vector2 vertex2Pos = vertexPositions[vertex2];
            offset1 = vertex1Pos - currentVertexPos;
            offset2 = vertex2Pos - currentVertexPos;
        }
    }
}
