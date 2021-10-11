namespace DerpySimulation.Render.GUIs.Positioning
{
    internal struct GUIRect
    {
        public float X;
        public float Y;
        public float W;
        public float H;

        public bool Contains(float x, float y)
        {
            return x >= X
                && y >= Y
                && x < X + W
                && y < Y + H;
        }
    }
}
