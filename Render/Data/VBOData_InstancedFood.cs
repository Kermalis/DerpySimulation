using System.Numerics;

namespace DerpySimulation.Render.Data
{
    internal struct VBOData_InstancedFood
    {
        public const int OffsetOfColor = 0;
        public const int OffsetOfTransform = OffsetOfColor + (3 * sizeof(float));
        public const uint SizeOf = OffsetOfTransform + (16 * sizeof(float));

        public readonly Vector3 Color;
        public readonly Matrix4x4 Transform;

        public VBOData_InstancedFood(in Vector3 color, in Matrix4x4 transform)
        {
            Color = color;
            Transform = transform;
        }
    }
}
