using DerpySimulation.Core;
using DerpySimulation.Render;
using Silk.NET.OpenGL;
using System.Numerics;

namespace DerpySimulation.World.Terrain
{
    internal sealed class TerrainTile
    {
        public Model Model { get; }
        private readonly int _sizeX, _sizeZ;
        private readonly float[,] _heights; // There is one extra height in each direction than the size

        public TerrainTile(Model m, float[,] heights)
        {
            Model = m;
            _sizeX = heights.GetLength(0) - 1;
            _sizeZ = heights.GetLength(1) - 1;
            _heights = heights;
        }

        public float GetHeight(float x, float z)
        {
            // -1 because if you have 5 heights, there are 4 grid squares
            // The size in units of each grid square
            float gridXSize = (_sizeX + 1f) / _sizeX;
            float gridZSize = (_sizeZ + 1f) / _sizeZ;
            // The grid id
            int gridX = (int)(x / gridXSize);
            int gridZ = (int)(z / gridZSize);
            // Check if it's out of bounds
            if (gridX < 0 || gridZ < 0 || gridX >= _sizeX || gridZ >= _sizeZ)
            {
                return 0;
            }
            // Coords between 0,0 and 1,1 of the grid square
            float gridSquareXCoord = x % gridXSize / gridXSize;
            float gridSquareZCoord = z % gridZSize / gridZSize;
            // Find which of the two triangles of our grid square that we are on
            // The line between our triangles is x = 1 - Z
            // So the top triangle is x < 1 - Z and the bottom one is x > 1 - Z
            float finalY;
            if (gridSquareXCoord <= (1 - gridSquareZCoord))
            {
                finalY = Utils.BarryCentric(new Vector3(0, _heights[gridX, gridZ], 0),
                    new Vector3(1, _heights[gridX + 1, gridZ], 0),
                    new Vector3(0, _heights[gridX, gridZ + 1], 1),
                    new Vector2(gridSquareXCoord, gridSquareZCoord));
            }
            else
            {
                finalY = Utils.BarryCentric(new Vector3(1, _heights[gridX + 1, gridZ], 0),
                    new Vector3(1, _heights[gridX + 1, gridZ + 1], 1),
                    new Vector3(0, _heights[gridX, gridZ + 1], 1),
                    new Vector2(gridSquareXCoord, gridSquareZCoord));
            }
            return finalY;
        }

        public void Render(GL gl)
        {
            Model.Render(gl);
        }

        public void Delete(GL gl)
        {
            Model.Delete(gl);
        }
    }
}
