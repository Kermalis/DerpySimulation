using System.Numerics;

namespace DerpySimulation.Render
{
    internal struct AtlasPos
    {
        public readonly Vector2 Start;
        public readonly Vector2 End;

        public AtlasPos(int x, int y, uint w, uint h,
            float atlasW, float atlasH,
            bool xFlip = false, bool yFlip = false)
        {
            int exclusiveRight = x + (int)w;
            int exclusiveBottom = y + (int)h;
            Start.X = (xFlip ? exclusiveRight : x) / atlasW;
            Start.Y = (yFlip ? exclusiveBottom : y) / atlasH;
            End.X = (xFlip ? x : exclusiveRight) / atlasW;
            End.Y = (yFlip ? y : exclusiveBottom) / atlasH;
        }

        public Vector2 GetBottomLeft()
        {
            return new Vector2(Start.X, End.Y);
        }
        public Vector2 GetTopRight()
        {
            return new Vector2(End.X, Start.Y);
        }
        public Vector2 GetBottomRight()
        {
            return new Vector2(End.X, End.Y);
        }
    }
}
