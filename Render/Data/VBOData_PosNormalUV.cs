using System.Numerics;

namespace DerpySimulation.Render.Data
{
    internal struct VBOData_PosNormalUV
    {
        public const int OffsetOfPos = 0;
        public const int OffsetOfNormal = OffsetOfPos + (3 * sizeof(float));
        public const int OffsetOfUV = OffsetOfNormal + (3 * sizeof(float));
        public const uint SizeOf = OffsetOfUV + (2 * sizeof(float));

        public readonly Vector3 Pos;
        public readonly Vector3 Normal;
        public readonly Vector2 UV;

        public VBOData_PosNormalUV(in Vector3 pos, in Vector3 normal, Vector2 uv)
        {
            Pos = pos;
            Normal = normal;
            UV = uv;
        }
    }
}
