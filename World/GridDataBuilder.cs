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

        public GridDataBuilder(int row, int col, float[,] gridHeights, Vector3[,] gridColors)
        {
            _positions = CalcCornerPositions(col, row, gridHeights);
            _colors = CalcCornerColors(col, row, gridColors);
            _lastIndex = gridHeights.GetLength(0) - 2;
            _row = row;
            _col = col;
            bool rightHanded = col % 2 != row % 2;
            _normalLeft = Utils.CalcNormal(_positions[0], _positions[1], _positions[rightHanded ? 3 : 2]);
            _normalRight = Utils.CalcNormal(_positions[2], _positions[rightHanded ? 0 : 1], _positions[3]);
        }

        private static Vector3[] CalcCornerColors(int col, int row, Vector3[,] gridColors)
        {
            var cornerCols = new Vector3[4];
            cornerCols[0] = gridColors[row, col];
            cornerCols[1] = gridColors[row + 1, col];
            cornerCols[2] = gridColors[row, col + 1];
            cornerCols[3] = gridColors[row + 1, col + 1];
            return cornerCols;
        }
        private static Vector3[] CalcCornerPositions(int col, int row, float[,] gridHeights)
        {
            var vertices = new Vector3[4];
            vertices[0] = new Vector3(col, gridHeights[row, col], row);
            vertices[1] = new Vector3(col, gridHeights[row + 1, col], row + 1);
            vertices[2] = new Vector3(col + 1, gridHeights[row, col + 1], row);
            vertices[3] = new Vector3(col + 1, gridHeights[row + 1, col + 1], row + 1);
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
