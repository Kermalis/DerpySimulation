using DerpySimulation.Render;
using DerpySimulation.World.Terrain;
using Silk.NET.OpenGL;
using System.Numerics;

namespace DerpySimulation.World.Water
{
    /// <summary>Creates a flat mesh of water. Indicators are used to know which other vertices are around.</summary>
    internal static class WaterGenerator
    {
        public static WaterTile Generate(GL gl, in SimulationCreationSettings settings, TerrainTile terrain)
        {
            WaterVBOData[] waterMeshData = CreateMeshData(settings, terrain, out uint numData);
            return new WaterTile(Model.CreateWaterModel(gl, waterMeshData, numData), settings.WaterLevel);
        }

        private static WaterVBOData[] CreateMeshData(in SimulationCreationSettings settings, TerrainTile terrain, out uint bufferIdx)
        {
            var buffer = new WaterVBOData[6 * settings.SizeX * settings.SizeZ];
            bufferIdx = 0;
            for (int z = 0; z < settings.SizeZ; z++)
            {
                for (int x = 0; x < settings.SizeX; x++)
                {
                    StoreGridSquare(x, z, settings.WaterLevel, terrain, buffer, ref bufferIdx);
                }
            }
            return buffer;
        }

        private static void StoreGridSquare(int x, int z, float waterY, TerrainTile terrain, WaterVBOData[] buffer, ref uint bufferIdx)
        {
            Vector2[] cornerPos = CalculateCornerPositions(x, z);
            StoreTriangle(cornerPos, true, waterY, terrain, buffer, ref bufferIdx);
            StoreTriangle(cornerPos, false, waterY, terrain, buffer, ref bufferIdx);
        }
        private static void StoreTriangle(Vector2[] cornerPos, bool left, float waterY, TerrainTile terrain, WaterVBOData[] buffer, ref uint bufferIdx)
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
            StoreVertex(v0, index0, cornerPos, index1, index2, buffer, ref bufferIdx);
            StoreVertex(v1, index1, cornerPos, index2, index0, buffer, ref bufferIdx);
            StoreVertex(v2, index2, cornerPos, index0, index1, buffer, ref bufferIdx);
        }
        private static void StoreVertex(Vector2 cornerPos, int currentVertex, Vector2[] vertexPositions, int vertex1, int vertex2, WaterVBOData[] buffer, ref uint bufferIdx)
        {
            GetIndicators(currentVertex, vertexPositions, vertex1, vertex2, out Vector2 offset1, out Vector2 offset2);
            buffer[bufferIdx++] = new WaterVBOData(cornerPos, offset1, offset2);
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
            offset1 = Vector2.Subtract(vertex1Pos, currentVertexPos);
            offset2 = Vector2.Subtract(vertex2Pos, currentVertexPos);
        }
    }
}
