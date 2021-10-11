using System.Numerics;

namespace DerpySimulation.Render.Data
{
    internal struct VBOData_Font
    {
        public const int OffsetOfPos = 0;
        public const int OffsetOfUV = OffsetOfPos + (2 * sizeof(int));
        public const uint SizeOf = OffsetOfUV + (2 * sizeof(float));

        public readonly int X;
        public readonly int Y;
        public readonly Vector2 UV;

        public VBOData_Font(int x, int y, Vector2 uv)
        {
            X = x;
            Y = y;
            UV = uv;
        }
    }
}
