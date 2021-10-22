using System.Numerics;

namespace DerpySimulation.Render.Data
{
    internal struct VBOData_InstancedBox
    {
        public const int OffsetOfColors = 0;
        public const int OffsetOfTransform = OffsetOfColors + (int)BoxColors.SizeOf;
        public const uint SizeOf = OffsetOfTransform + (16 * sizeof(float));

        public readonly BoxColors Colors;
        public readonly Matrix4x4 Transform;

        public VBOData_InstancedBox(in BoxColors colors, in Matrix4x4 transform)
        {
            Colors = colors;
            Transform = transform;
        }
    }
}
