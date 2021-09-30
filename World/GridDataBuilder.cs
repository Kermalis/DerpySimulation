using DerpySimulation.Core;
using System.Numerics;

namespace DerpySimulation.World
{
    internal sealed class GridDataBuilder
    {
        private readonly int _row;
        private readonly int _col;
        private readonly int _lastIndex;
        private readonly Vector3[] _positions;
        private readonly Vector3[] _colors;
        private readonly Vector3 _normalLeft;
        private readonly Vector3 _normalRight;

        public GridDataBuilder(int row, int col, TerrainGeneratorData[,] grid)
        {
            _positions = CalcCornerPositions(col, row, grid);
            _colors = CalcCornerColors(col, row, grid);
            _lastIndex = grid.GetLength(0) - 2;
            _row = row;
            _col = col;
            bool rightHanded = col % 2 != row % 2;
            _normalLeft = Utils.CalcNormal(_positions[0], _positions[1], _positions[rightHanded ? 3 : 2]);
            _normalRight = Utils.CalcNormal(_positions[2], _positions[rightHanded ? 0 : 1], _positions[3]);
        }

        private static Vector3[] CalcCornerColors(int col, int row, TerrainGeneratorData[,] grid)
        {
            var cornerCols = new Vector3[4];
            cornerCols[0] = grid[row, col].Color;
            cornerCols[1] = grid[row + 1, col].Color;
            cornerCols[2] = grid[row, col + 1].Color;
            cornerCols[3] = grid[row + 1, col + 1].Color;
            return cornerCols;
        }
        private static Vector3[] CalcCornerPositions(int col, int row, TerrainGeneratorData[,] grid)
        {
            var vertices = new Vector3[4];
            vertices[0] = new Vector3(col, grid[row, col].Height, row);
            vertices[1] = new Vector3(col, grid[row + 1, col].Height, row + 1);
            vertices[2] = new Vector3(col + 1, grid[row, col + 1].Height, row);
            vertices[3] = new Vector3(col + 1, grid[row + 1, col + 1].Height, row + 1);
            return vertices;
        }

        public void StoreSquareData(TerrainVBOData[] data, ref int dataIdx)
        {
            StoreTopLeftVertex(data, ref dataIdx);
            if (_row != _lastIndex || _col == _lastIndex)
            {
                StoreTopRightVertex(data, ref dataIdx);
            }
        }
        public void StoreBottomRowData(TerrainVBOData[] data, ref int dataIdx)
        {
            if (_col == 0)
            {
                StoreBottomLeftVertex(data, ref dataIdx);
            }
            StoreBottomRightVertex(data, ref dataIdx);
        }

        private void StoreTopLeftVertex(TerrainVBOData[] data, ref int dataIdx)
        {
            data[dataIdx++] = new TerrainVBOData(_positions[0], _normalLeft, _colors[0]);
        }
        private void StoreBottomLeftVertex(TerrainVBOData[] data, ref int dataIdx)
        {
            data[dataIdx++] = new TerrainVBOData(_positions[1], _normalLeft, _colors[1]);
        }
        private void StoreTopRightVertex(TerrainVBOData[] data, ref int dataIdx)
        {
            data[dataIdx++] = new TerrainVBOData(_positions[2], _normalRight, _colors[2]);
        }
        private void StoreBottomRightVertex(TerrainVBOData[] data, ref int dataIdx)
        {
            data[dataIdx++] = new TerrainVBOData(_positions[3], _normalRight, _colors[3]);
        }
    }
}
