namespace DerpySimulation.Render.GUIs.Positioning
{
    internal sealed class PixelConstraint : GUIConstraint
    {
        private readonly int _value;

        public PixelConstraint(int value)
        {
            _value = value;
        }
        protected override void EndInit(GUIConstraints others)
        {
            //
        }

        public override float GetRelativeValue()
        {
            float size = GetParentAbsSize();
            return _value / size;
        }
    }
}
