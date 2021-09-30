namespace DerpySimulation.World
{
    /// <summary>This class does the magic of piecing together the index buffer for the low-poly terrain. Code adapted from TheThinMatrix.</summary>
    internal static class IndexGenerator
    {
        public static uint[] GenerateIndexBuffer(uint vertexCount)
        {
            uint[] indices = new uint[6 * (vertexCount - 1) * (vertexCount - 1)];
            uint rowLength = (vertexCount - 1) * 2;
            int pointer = 0;
            StoreTopSection(indices, rowLength, vertexCount, ref pointer);
            StoreSecondLastLine(indices, rowLength, vertexCount, ref pointer);
            StoreLastLine(indices, rowLength, vertexCount, ref pointer);
            return indices;
        }

        private static void StoreTopSection(uint[] indices, uint rowLength, uint vertexLength, ref int pointer)
        {
            for (uint row = 0; row < vertexLength - 3; row++)
            {
                for (uint col = 0; col < vertexLength - 1; col++)
                {
                    uint topLeft = (row * rowLength) + (col * 2);
                    uint topRight = topLeft + 1;
                    uint bottomLeft = topLeft + rowLength;
                    uint bottomRight = bottomLeft + 1;
                    StoreQuad(indices, topLeft, topRight, bottomLeft, bottomRight, col % 2 != row % 2, ref pointer);
                }
            }
        }

        private static void StoreSecondLastLine(uint[] indices, uint rowLength, uint vertexLength, ref int pointer)
        {
            uint row = vertexLength - 3;
            for (uint col = 0; col < vertexLength - 1; col++)
            {
                uint topLeft = (row * rowLength) + (col * 2);
                uint topRight = topLeft + 1;
                uint bottomLeft = (topLeft + rowLength) - col;
                uint bottomRight = bottomLeft + 1;
                StoreQuad(indices, topLeft, topRight, bottomLeft, bottomRight, col % 2 != row % 2, ref pointer);
            }
        }

        private static void StoreLastLine(uint[] indices, uint rowLength, uint vertexLength, ref int pointer)
        {
            uint row = vertexLength - 2;
            for (uint col = 0; col < vertexLength - 1; col++)
            {
                uint topLeft = (row * rowLength) + col;
                uint topRight = topLeft + 1;
                uint bottomLeft = (topLeft + vertexLength);
                uint bottomRight = bottomLeft + 1;
                StoreLastRowQuad(indices, topLeft, topRight, bottomLeft, bottomRight, col % 2 != row % 2, ref pointer);
            }
        }

        private static void StoreQuad(uint[] indices, uint topLeft, uint topRight, uint bottomLeft, uint bottomRight, bool rightHanded, ref int pointer)
        {
            StoreLeftTriangle(indices, topLeft, topRight, bottomLeft, bottomRight, rightHanded, ref pointer);
            indices[pointer++] = topRight;
            indices[pointer++] = rightHanded ? topLeft : bottomLeft;
            indices[pointer++] = bottomRight;
        }

        private static void StoreLastRowQuad(uint[] indices, uint topLeft, uint topRight, uint bottomLeft, uint bottomRight, bool rightHanded, ref int pointer)
        {
            StoreLeftTriangle(indices, topLeft, topRight, bottomLeft, bottomRight, rightHanded, ref pointer);
            indices[pointer++] = bottomRight;
            indices[pointer++] = topRight;
            indices[pointer++] = rightHanded ? topLeft : bottomLeft;
        }

        private static void StoreLeftTriangle(uint[] indices, uint topLeft, uint topRight, uint bottomLeft, uint bottomRight, bool rightHanded, ref int pointer)
        {
            indices[pointer++] = topLeft;
            indices[pointer++] = bottomLeft;
            indices[pointer++] = rightHanded ? bottomRight : topRight;
        }
    }
}
