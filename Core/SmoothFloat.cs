namespace DerpySimulation.Core
{
    /// <summary>Moves a float smoothly across multiple frames.</summary>
    internal struct SmoothFloat
    {
        private readonly float _maxSpeed; // Max change per second
        public float Target; // The value we want to achieve currently
        public float Current; // The current value

        public SmoothFloat(float startValue, float maxSpeed)
        {
            Target = startValue;
            Current = startValue;
            _maxSpeed = maxSpeed;
        }

        public void Update(float delta)
        {
            float difference = Target - Current;
            float change = difference * delta * _maxSpeed;
            Current += change;
        }
    }
}
