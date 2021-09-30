#if DEBUG
using DerpySimulation.Debug;
#endif
using DerpySimulation.Render;
using Silk.NET.OpenGL;
using System.Numerics;

namespace DerpySimulation.World
{
    internal static class TerrainGenerator
    {
        public static Terrain GenerateTerrain(GL gl, HeightGenerator heightGen, ColorStep[] colors, uint size, out Vector3 peak)
        {
#if DEBUG
            Log.WriteLineWithTime("Generating heights...");
#endif
            // Generate heights

            peak = new Vector3(0, float.NegativeInfinity, 0);
            float[,] gridHeights = new float[size + 1, size + 1];
            for (int z = 0; z < size + 1; z++)
            {
                for (int x = 0; x < size + 1; x++)
                {
                    float y = heightGen.GenerateHeight(x, z);
                    if (y > peak.Y)
                    {
                        peak = new Vector3(x, y, z);
                    }
                    gridHeights[z, x] = y;
                }
            }

            // Generate colors
#if DEBUG
            Log.WriteLineWithTime("Generating colors...");
#endif
            Vector3[,] gridColors = ColorGenerator.GenerateColors(colors, gridHeights);

            // Create terrain
#if DEBUG
            Log.WriteLineWithTime("Generating terrain...");
#endif
            uint vertexCount = CalcVertexCount(size + 1);
            TerrainVBOData[] terrainData = CreateMeshData(gridHeights, gridColors, vertexCount);
            uint[] indices = IndexGenerator.GenerateIndexBuffer(size + 1);
            return new Terrain(Model.CreateTerrainModel(gl, terrainData, indices), gridHeights);
        }

        private static uint CalcVertexCount(uint vertexLength)
        {
            uint bottom2Rows = 2 * vertexLength;
            uint remainingRowCount = vertexLength - 2;
            uint topCount = remainingRowCount * (vertexLength - 1) * 2;
            return topCount + bottom2Rows;
        }

        /// <summary>Creates the vbo vertex data for the GPU.</summary>
        private static TerrainVBOData[] CreateMeshData(float[,] gridHeights, Vector3[,] gridColors, uint vertexCount)
        {
            var data = new TerrainVBOData[vertexCount];
            int dataIdx = 0;
            var lastRow = new GridDataBuilder[gridHeights.GetLength(0) - 1];
            for (int row = 0; row < gridHeights.GetLength(0) - 1; row++)
            {
                for (int col = 0; col < gridHeights.GetLength(1) - 1; col++)
                {
                    var builder = new GridDataBuilder(row, col, gridHeights, gridColors);
                    builder.StoreSquareData(data, ref dataIdx);
                    if (row == gridHeights.GetLength(0) - 2)
                    {
                        lastRow[col] = builder;
                    }
                }
            }
            for (int i = 0; i < lastRow.Length; i++)
            {
                lastRow[i].StoreBottomRowData(data, ref dataIdx);
            }
            return data;
        }
    }
}
