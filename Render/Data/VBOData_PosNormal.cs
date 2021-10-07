using System.Numerics;

namespace DerpySimulation.Render.Data
{
    internal struct VBOData_PosNormal
    {
        public const int OffsetOfPos = 0;
        public const int OffsetOfNormal = OffsetOfPos + (3 * sizeof(float));
        public const uint SizeOf = OffsetOfNormal + (3 * sizeof(float));

        public readonly Vector3 Pos;
        public readonly Vector3 Normal;

        public VBOData_PosNormal(in Vector3 pos, in Vector3 normal)
        {
            Pos = pos;
            Normal = normal;
        }
    }
}
