namespace DerpySimulation.Render.GUIs.Positioning
{
    internal sealed class GUIConstraints
    {
        public readonly GUIConstraint X;
        public readonly GUIConstraint Y;
        public readonly GUIConstraint W;
        public readonly GUIConstraint H;

        public GUIConstraints(GUIConstraint x, GUIConstraint y, GUIConstraint w, GUIConstraint h)
        {
            X = x;
            x.SetAxis(true, true);
            Y = y;
            y.SetAxis(false, true);
            W = w;
            w.SetAxis(true, false);
            H = h;
            h.SetAxis(false, false);
        }

        public void OnAttached(GUIComponent c)
        {
            X.OnAttached(this, c);
            Y.OnAttached(this, c);
            W.OnAttached(this, c);
            H.OnAttached(this, c);
        }
    }
}
