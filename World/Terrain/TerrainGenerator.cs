#if DEBUG
using DerpySimulation.Debug;
#endif
using DerpySimulation.Render;
using Silk.NET.OpenGL;
using System.Numerics;

namespace DerpySimulation.World.Terrain
{
    internal static class TerrainGenerator
    {
        public static TerrainTile GenerateTerrain(GL gl, in SimulationCreationSettings settings, out Vector3 peak)
        {
#if DEBUG
            Log.WriteLineWithTime("Generating heights...");
#endif
            // Generate heights

            peak = new Vector3(0, float.NegativeInfinity, 0);
            var heightGen = new HeightGenerator(settings.HeightGenAmplitude, settings.HeightGenNumOctaves, settings.HeightGenRoughness, settings.HeightGenSeed);
            // size + 1 because we are also calculating the point right outside of the terrain so it can still interpolate there
            float[,] gridHeights = new float[settings.SizeX + 1, settings.SizeZ + 1];
            for (int z = 0; z < settings.SizeZ + 1; z++)
            {
                for (int x = 0; x < settings.SizeX + 1; x++)
                {
                    float y = heightGen.GenerateHeight(x, z);
                    if (y > peak.Y)
                    {
                        peak = new Vector3(x, y, z);
                    }
                    gridHeights[x, z] = y;
                }
            }

            // Generate colors
#if DEBUG
            Log.WriteLineWithTime("Generating colors...");
#endif
            Vector3[,] gridColors = ColorGenerator.GenerateColors(settings.Colors, gridHeights);

            // Create terrain
#if DEBUG
            Log.WriteLineWithTime("Generating terrain...");
            Log.ModifyIndent(+1);
#endif
            uint numVertices = CalcVertexCount(settings.SizeX, settings.SizeZ);
#if DEBUG
            Log.WriteLineWithTime("Generating mesh data...");
#endif
            TerrainVBOData[] terrainData = CreateMeshData(gridHeights, gridColors, numVertices);
#if DEBUG
            Log.WriteLineWithTime("Generating indices...");
#endif
            uint[] indices = TerrainIndexGenerator.GenerateIndexBuffer(settings.SizeX, settings.SizeZ);
#if DEBUG
            Log.ModifyIndent(-1);
#endif
            return new TerrainTile(Model.CreateTerrainModel(gl, terrainData, indices), gridHeights);
        }

        private static uint CalcVertexCount(uint sizeX, uint sizeZ)
        {
            uint topCount = (sizeX - 1) * sizeZ * 2;
            uint bottom2Rows = 2 * (sizeZ + 1);
            return topCount + bottom2Rows;
        }

        /// <summary>Creates the vbo vertex data for the GPU.</summary>
        private static TerrainVBOData[] CreateMeshData(float[,] gridHeights, Vector3[,] gridColors, uint numVertices)
        {
            var data = new TerrainVBOData[numVertices];
            int dataIdx = 0;
            var lastRow = new GridDataBuilder[gridHeights.GetLength(0) - 1];
            for (int z = 0; z < gridHeights.GetLength(1) - 1; z++)
            {
                for (int x = 0; x < gridHeights.GetLength(0) - 1; x++)
                {
                    var builder = new GridDataBuilder(x, z, gridHeights, gridColors);
                    builder.StoreSquareData(data, ref dataIdx);
                    if (z == gridHeights.GetLength(1) - 2)
                    {
                        lastRow[x] = builder;
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
