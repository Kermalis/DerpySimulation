namespace DerpySimulation.World.Terrain
{
    /// <summary>This class does the magic of piecing together the index buffer for the low-poly terrain. Code adapted from TheThinMatrix.</summary>
    internal static class TerrainIndexGenerator
    {
        // The first rows alternate which vertex is the provoking vertex for its triangle
        // The last two rows have to be different, and there will be two vertices who don't share other vertices
        public static uint[] GenerateIndexBuffer(uint sizeX, uint sizeZ)
        {
            uint[] indices = new uint[6 * sizeX * sizeZ];
            uint rowLength = sizeX * 2;
            int pointer = 0;
            StoreTopSection(rowLength, sizeX, sizeZ, indices, ref pointer);
            StoreSecondLastLine(rowLength, sizeX, sizeZ, indices, ref pointer);
            StoreLastLine(rowLength, sizeX, sizeZ, indices, ref pointer);
            return indices;
        }

        private static void StoreTopSection(uint rowLength, uint sizeX, uint sizeZ, uint[] indices, ref int pointer)
        {
            for (uint z = 0; z < sizeZ - 2; z++)
            {
                for (uint x = 0; x < sizeX; x++)
                {
                    uint topLeft = (z * rowLength) + (x * 2);
                    uint topRight = topLeft + 1;
                    uint bottomLeft = topLeft + rowLength;
                    uint bottomRight = bottomLeft + 1;
                    StoreQuad(topLeft, topRight, bottomLeft, bottomRight, x % 2 != z % 2, indices, ref pointer);
                }
            }
        }
        private static void StoreSecondLastLine(uint rowLength, uint sizeX, uint sizeZ, uint[] indices, ref int pointer)
        {
            uint z = sizeZ - 2;
            for (uint x = 0; x < sizeX; x++)
            {
                uint topLeft = (z * rowLength) + (x * 2);
                uint topRight = topLeft + 1;
                uint bottomLeft = topLeft + rowLength - x;
                uint bottomRight = bottomLeft + 1;
                StoreQuad(topLeft, topRight, bottomLeft, bottomRight, x % 2 != z % 2, indices, ref pointer);
            }
        }
        private static void StoreLastLine(uint rowLength, uint sizeX, uint sizeZ, uint[] indices, ref int pointer)
        {
            uint z = sizeZ - 1;
            for (uint x = 0; x < sizeX; x++)
            {
                uint topLeft = (z * rowLength) + x;
                uint topRight = topLeft + 1;
                uint bottomLeft = topLeft + sizeX + 1;
                uint bottomRight = bottomLeft + 1;
                StoreLastRowQuad(topLeft, topRight, bottomLeft, bottomRight, x % 2 != z % 2, indices, ref pointer);
            }
        }

        private static void StoreQuad(uint topLeft, uint topRight, uint bottomLeft, uint bottomRight, bool rightHanded, uint[] indices, ref int pointer)
        {
            StoreLeftTriangle(topLeft, topRight, bottomLeft, bottomRight, rightHanded, indices, ref pointer);
            indices[pointer++] = topRight;
            indices[pointer++] = rightHanded ? topLeft : bottomLeft;
            indices[pointer++] = bottomRight;
        }
        private static void StoreLastRowQuad(uint topLeft, uint topRight, uint bottomLeft, uint bottomRight, bool rightHanded, uint[] indices, ref int pointer)
        {
            StoreLeftTriangle(topLeft, topRight, bottomLeft, bottomRight, rightHanded, indices, ref pointer);
            indices[pointer++] = bottomRight;
            indices[pointer++] = topRight;
            indices[pointer++] = rightHanded ? topLeft : bottomLeft;
        }
        private static void StoreLeftTriangle(uint topLeft, uint topRight, uint bottomLeft, uint bottomRight, bool rightHanded, uint[] indices, ref int pointer)
        {
            indices[pointer++] = topLeft;
            indices[pointer++] = bottomLeft;
            indices[pointer++] = rightHanded ? bottomRight : topRight;
        }
    }
}
