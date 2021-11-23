using DerpySimulation.Core;
using DerpySimulation.Render.Meshes;
using Silk.NET.OpenGL;
using System.Numerics;

namespace DerpySimulation.World.Terrain
{
    internal sealed class TerrainTile
    {
        private readonly Mesh _mesh;
        public readonly int SizeX, SizeZ;
        private readonly float[,] _heights; // There is one extra height in each direction than the size

        public TerrainTile(Mesh m, float[,] heights)
        {
            _mesh = m;
            // -1 because if you have 5 heights, there are 4 grid squares
            SizeX = heights.GetLength(0) - 1;
            SizeZ = heights.GetLength(1) - 1;
            _heights = heights;
        }

        public float GetHeight(float x, float z, float outOfBoundsResult = 0f)
        {
            // x and z are between 0,0(inclusive) and SIZE,SIZE(exclusive)
            int gridX = (int)x;
            int gridZ = (int)z;

            // Check if it's out of bounds
            if (gridX < 0 || gridZ < 0 || gridX >= SizeX || gridZ >= SizeZ)
            {
                return outOfBoundsResult;
            }

            // Coords between 0,0 and 1,1 of the grid square
            float gridSquareXCoord = x % 1 / 1;
            float gridSquareZCoord = z % 1 / 1;

            // Find which of the two triangles of our grid square that we are on
            // The line between our triangles is (x = 1 - Z)
            // So the top triangle is (x < 1 - Z) and the bottom one is (x > 1 - Z)
            int ofs = gridSquareXCoord <= (1f - gridSquareZCoord) ? 0 : 1;
            return Utils.BaryCentricInterpolation(
                new Vector3(ofs, _heights[gridX + ofs, gridZ], 0f),
                new Vector3(1f, _heights[gridX + 1, gridZ + ofs], ofs),
                new Vector3(0f, _heights[gridX, gridZ + 1], 1f),
                new Vector2(gridSquareXCoord, gridSquareZCoord)
                );
        }

        public void Render(GL gl)
        {
            _mesh.Render(gl);
        }

        public void Delete(GL gl)
        {
            _mesh.Delete(gl);
        }
    }
}
