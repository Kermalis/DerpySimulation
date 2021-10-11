namespace DerpySimulation.Render.GUIs.Positioning
{
    internal sealed class RelativeConstraint : GUIConstraint
    {
        private readonly float _value;

        public RelativeConstraint(float relValue)
        {
            _value = relValue;
        }
        protected override void EndInit(GUIConstraints others)
        {
            //
        }

        public override float GetRelativeValue()
        {
            return _value;
        }

        public static GUIConstraints CreateFillConstraints()
        {
            return new GUIConstraints(
                new RelativeConstraint(0),
                new RelativeConstraint(0),
                new RelativeConstraint(1),
                new RelativeConstraint(1)
                );
        }
    }
}
