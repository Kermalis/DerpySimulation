using System.Threading;

namespace DerpySimulation.World.Terrain
{
    /// <summary>This class does the magic of piecing together the index buffer for the low-poly terrain. Code adapted from TheThinMatrix.</summary>
    internal static class TerrainIndexGenerator
    {
        // The first rows alternate which vertex is the provoking vertex for its triangle
        // The last two rows have to be different, and there will be two vertices who don't share other vertices
        public static uint[] Generate(CancellationTokenSource cts, uint sizeX, uint sizeZ)
        {
            uint[] indices = new uint[6 * sizeX * sizeZ];
            int bufferIdx = 0;
            uint rowLength = sizeX * 2;
            StoreTopSection(cts, indices, ref bufferIdx, rowLength, sizeX, sizeZ);
            cts.Token.ThrowIfCancellationRequested();
            StoreSecondLastLine(cts, indices, ref bufferIdx, rowLength, sizeX, sizeZ);
            cts.Token.ThrowIfCancellationRequested();
            StoreLastLine(cts, indices, ref bufferIdx, rowLength, sizeX, sizeZ);
            return indices;
        }

        private static void StoreTopSection(CancellationTokenSource cts, uint[] indices, ref int bufferIdx, uint rowLength, uint sizeX, uint sizeZ)
        {
            for (uint z = 0; z < sizeZ - 2; z++)
            {
                for (uint x = 0; x < sizeX; x++)
                {
                    cts.Token.ThrowIfCancellationRequested();
                    uint topLeft = (z * rowLength) + (x * 2);
                    uint topRight = topLeft + 1;
                    uint bottomLeft = topLeft + rowLength;
                    uint bottomRight = bottomLeft + 1;
                    StoreQuad(indices, ref bufferIdx, topLeft, topRight, bottomLeft, bottomRight, x % 2 != z % 2);
                }
            }
        }
        private static void StoreSecondLastLine(CancellationTokenSource cts, uint[] indices, ref int bufferIdx, uint rowLength, uint sizeX, uint sizeZ)
        {
            uint z = sizeZ - 2;
            for (uint x = 0; x < sizeX; x++)
            {
                cts.Token.ThrowIfCancellationRequested();
                uint topLeft = (z * rowLength) + (x * 2);
                uint topRight = topLeft + 1;
                uint bottomLeft = topLeft + rowLength - x;
                uint bottomRight = bottomLeft + 1;
                StoreQuad(indices, ref bufferIdx, topLeft, topRight, bottomLeft, bottomRight, x % 2 != z % 2);
            }
        }
        private static void StoreLastLine(CancellationTokenSource cts, uint[] indices, ref int bufferIdx, uint rowLength, uint sizeX, uint sizeZ)
        {
            uint z = sizeZ - 1;
            for (uint x = 0; x < sizeX; x++)
            {
                cts.Token.ThrowIfCancellationRequested();
                uint topLeft = (z * rowLength) + x;
                uint topRight = topLeft + 1;
                uint bottomLeft = topLeft + sizeX + 1;
                uint bottomRight = bottomLeft + 1;
                StoreLastRowQuad(indices, ref bufferIdx, topLeft, topRight, bottomLeft, bottomRight, x % 2 != z % 2);
            }
        }

        private static void StoreQuad(uint[] indices, ref int bufferIdx, uint topLeft, uint topRight, uint bottomLeft, uint bottomRight, bool rightHanded)
        {
            StoreLeftTriangle(indices, ref bufferIdx, topLeft, topRight, bottomLeft, bottomRight, rightHanded);
            indices[bufferIdx++] = topRight;
            indices[bufferIdx++] = rightHanded ? topLeft : bottomLeft;
            indices[bufferIdx++] = bottomRight;
        }
        private static void StoreLastRowQuad(uint[] indices, ref int bufferIdx, uint topLeft, uint topRight, uint bottomLeft, uint bottomRight, bool rightHanded)
        {
            StoreLeftTriangle(indices, ref bufferIdx, topLeft, topRight, bottomLeft, bottomRight, rightHanded);
            indices[bufferIdx++] = bottomRight;
            indices[bufferIdx++] = topRight;
            indices[bufferIdx++] = rightHanded ? topLeft : bottomLeft;
        }
        private static void StoreLeftTriangle(uint[] indices, ref int bufferIdx, uint topLeft, uint topRight, uint bottomLeft, uint bottomRight, bool rightHanded)
        {
            indices[bufferIdx++] = topLeft;
            indices[bufferIdx++] = bottomLeft;
            indices[bufferIdx++] = rightHanded ? bottomRight : topRight;
        }
    }
}
