namespace DerpySimulation.Render.GUIs.Positioning
{
    internal abstract class GUIConstraint
    {
        public bool IsXAxis { get; private set; }
        public bool IsPositiveAxis { get; private set; }
        public GUIComponent Component { get; private set; } = null!; // Set in OnAttached()

        public void SetAxis(bool isXAxis, bool isPositiveAxis)
        {
            IsXAxis = isXAxis;
            IsPositiveAxis = isPositiveAxis;
        }

        protected float GetParentAbsSize()
        {
            GUIComponent p = Component.Parent;
            return IsXAxis ? p.GetAbsWidth() : p.GetAbsHeight();
        }

        public void OnAttached(GUIConstraints others, GUIComponent c)
        {
            Component = c;
            EndInit(others);
        }
        protected abstract void EndInit(GUIConstraints others);

        public abstract float GetRelativeValue();
    }
}
