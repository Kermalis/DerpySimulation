using System.Numerics;

namespace DerpySimulation.Render.Data
{
    internal struct VBOData_PosNormalColor
    {
        public const int OffsetOfPos = 0;
        public const int OffsetOfNormal = OffsetOfPos + (3 * sizeof(float));
        public const int OffsetOfColor = OffsetOfNormal + (3 * sizeof(float));
        public const uint SizeOf = OffsetOfColor + (3 * sizeof(float));

        public readonly Vector3 Pos;
        public readonly Vector3 Normal;
        public readonly Vector3 Color;

        public VBOData_PosNormalColor(in Vector3 pos, in Vector3 normal, in Vector3 color)
        {
            Pos = pos;
            Normal = normal;
            Color = color;
        }
    }
}
