using System.Numerics;

namespace DerpySimulation.World.Water
{
    internal unsafe struct WaterVBOData
    {
        public const int OffsetOfPos = 0;
        public const int OffsetOfPartnerVertex1 = OffsetOfPos + (2 * sizeof(float));
        public const int OffsetOfPartnerVertex2 = OffsetOfPartnerVertex1 + (2 * sizeof(sbyte));
        public const uint SizeOf = OffsetOfPartnerVertex2 + (2 * sizeof(sbyte));

        public readonly Vector2 Pos;
        public fixed sbyte PartnerVertex1[2];
        public fixed sbyte PartnerVertex2[2];

        public WaterVBOData(Vector2 pos, Vector2 indicator1, Vector2 indicator2)
        {
            Pos = pos;
            PartnerVertex1[0] = (sbyte)indicator1.X;
            PartnerVertex1[1] = (sbyte)indicator1.Y;
            PartnerVertex2[0] = (sbyte)indicator2.X;
            PartnerVertex2[1] = (sbyte)indicator2.Y;
        }
    }
}
