﻿using DerpySimulation.Core;
using DerpySimulation.Render.Data;
using System.Numerics;
using System.Threading;

namespace DerpySimulation.World.Terrain
{
    internal sealed class TerrainMeshGenerator
    {
        private readonly int _x;
        private readonly int _z;
        private readonly int _lastXIndex;
        private readonly int _lastZIndex;
        // 0 - Top Left, 1 - Bottom Left, 2 - Top Right, 3 - Bottom Right
        private readonly Vector3[] _positions;
        private readonly Vector3[] _colors;
        private readonly Vector3 _normalLeft;
        private readonly Vector3 _normalRight;

        private TerrainMeshGenerator(int x, int z, float[,] gridHeights, Vector3[,] gridColors)
        {
            _positions = CalcCornerPositions(x, z, gridHeights);
            _colors = CalcCornerColors(x, z, gridColors);
            _lastXIndex = gridHeights.GetLength(0) - 2;
            _lastZIndex = gridHeights.GetLength(1) - 2;
            _x = x;
            _z = z;
            bool rightHanded = x % 2 != z % 2;
            _normalLeft = Utils.CalcNormal(_positions[0], _positions[1], _positions[rightHanded ? 3 : 2]);
            _normalRight = Utils.CalcNormal(_positions[2], _positions[rightHanded ? 0 : 1], _positions[3]);
        }

        /// <summary>Creates the vbo vertex data for the GPU.</summary>
        public static VBOData_PosNormalColor[] Generate(CancellationTokenSource cts, uint sizeX, uint sizeZ, float[,] gridHeights, Vector3[,] gridColors, out uint numVertices)
        {
            numVertices = CalcVertexCount(sizeX, sizeZ);
            var data = new VBOData_PosNormalColor[numVertices];
            int dataIdx = 0;
            var lastRow = new TerrainMeshGenerator[gridHeights.GetLength(0) - 1];
            for (int z = 0; z < gridHeights.GetLength(1) - 1; z++)
            {
                for (int x = 0; x < gridHeights.GetLength(0) - 1; x++)
                {
                    cts.Token.ThrowIfCancellationRequested();
                    var builder = new TerrainMeshGenerator(x, z, gridHeights, gridColors);
                    builder.StoreSquareData(data, ref dataIdx);
                    if (z == gridHeights.GetLength(1) - 2)
                    {
                        lastRow[x] = builder;
                    }
                }
            }
            for (int i = 0; i < lastRow.Length; i++)
            {
                cts.Token.ThrowIfCancellationRequested();
                lastRow[i].StoreBottomRowData(data, ref dataIdx);
            }
            cts.Token.ThrowIfCancellationRequested();
            return data;
        }

        private void StoreSquareData(VBOData_PosNormalColor[] data, ref int dataIdx)
        {
            StoreTopLeftVertex(data, ref dataIdx);
            if (_x == _lastXIndex || _z != _lastZIndex)
            {
                StoreTopRightVertex(data, ref dataIdx);
            }
        }
        private void StoreBottomRowData(VBOData_PosNormalColor[] data, ref int dataIdx)
        {
            if (_x == 0)
            {
                StoreBottomLeftVertex(data, ref dataIdx);
            }
            StoreBottomRightVertex(data, ref dataIdx);
        }

        private void StoreTopLeftVertex(VBOData_PosNormalColor[] data, ref int dataIdx)
        {
            data[dataIdx++] = new VBOData_PosNormalColor(_positions[0], _normalLeft, _colors[0]);
        }
        private void StoreBottomLeftVertex(VBOData_PosNormalColor[] data, ref int dataIdx)
        {
            data[dataIdx++] = new VBOData_PosNormalColor(_positions[1], _normalLeft, _colors[1]);
        }
        private void StoreTopRightVertex(VBOData_PosNormalColor[] data, ref int dataIdx)
        {
            data[dataIdx++] = new VBOData_PosNormalColor(_positions[2], _normalRight, _colors[2]);
        }
        private void StoreBottomRightVertex(VBOData_PosNormalColor[] data, ref int dataIdx)
        {
            data[dataIdx++] = new VBOData_PosNormalColor(_positions[3], _normalRight, _colors[3]);
        }

        private static uint CalcVertexCount(uint sizeX, uint sizeZ)
        {
            uint topCount = (sizeX - 1) * sizeZ * 2;
            uint bottom2Rows = 2 * (sizeZ + 1);
            return topCount + bottom2Rows;
        }
        private static Vector3[] CalcCornerColors(int x, int z, Vector3[,] gridColors)
        {
            var cornerCols = new Vector3[4];
            cornerCols[0] = gridColors[x, z];
            cornerCols[1] = gridColors[x, z + 1];
            cornerCols[2] = gridColors[x + 1, z];
            cornerCols[3] = gridColors[x + 1, z + 1];
            return cornerCols;
        }
        private static Vector3[] CalcCornerPositions(int x, int z, float[,] gridHeights)
        {
            var vertices = new Vector3[4];
            vertices[0] = new Vector3(x, gridHeights[x, z], z);
            vertices[1] = new Vector3(x, gridHeights[x, z + 1], z + 1);
            vertices[2] = new Vector3(x + 1, gridHeights[x + 1, z], z);
            vertices[3] = new Vector3(x + 1, gridHeights[x + 1, z + 1], z + 1);
            return vertices;
        }
    }
}
