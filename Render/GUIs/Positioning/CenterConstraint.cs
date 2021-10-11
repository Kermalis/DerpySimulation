namespace DerpySimulation.Render.GUIs.Positioning
{
    internal sealed class CenterConstraint : GUIConstraint
    {
        private GUIConstraint _axisConstraint = null!; // Set in EndInit()

        protected override void EndInit(GUIConstraints others)
        {
            _axisConstraint = IsXAxis ? others.W : others.H;
        }

        public override float GetRelativeValue()
        {
            float rel = _axisConstraint.GetRelativeValue();
            return (1f - rel) * 0.5f; // Center
        }
    }
}
