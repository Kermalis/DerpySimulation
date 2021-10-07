using DerpySimulation.Core;
using System.Numerics;

namespace DerpySimulation.Entities
{
    internal sealed class FoodEntity : Entity
    {
        public const uint MAX_FOOD = 1_000;

        // Degrees per second rotation
        private const float ROTATION_SPEEDX = 225f;
        private const float ROTATION_SPEEDY = 540f;
        private const float BASE_VISUAL_Y = 0.5f;
        private const float BOUNCE_HEIGHT = 0.75f;
        private const float BOUNCE_SPEED = 0.9f;

        private float _bounceProgress;

        public readonly Vector3 Color;

        public FoodEntity(in Vector3 pos, in Vector3 color)
        {
            PR.Position = pos;
            Color = color;
        }

        private void UpdateSpin(float delta)
        {
            float rotX = (delta * ROTATION_SPEEDX) + PR.Rotation.Pitch;
            rotX %= 360;
            float rotY = (delta * ROTATION_SPEEDY) + PR.Rotation.Yaw;
            rotY %= 360;
            PR.Rotation.Set(rotY, rotX, 0f);
        }
        private float UpdateBounce(float delta)
        {
            _bounceProgress = (delta * BOUNCE_SPEED) + _bounceProgress;
            float use;
            if (_bounceProgress > 1f)
            {
                _bounceProgress = 0f;
                use = 1f;
            }
            else
            {
                use = _bounceProgress;
            }

            return (Easing.BellCurve2(use) * BOUNCE_HEIGHT) + BASE_VISUAL_Y;
        }
        public void UpdateVisual(float delta)
        {
            UpdateSpin(delta);
            float visualOfsY = UpdateBounce(delta);

            UpdateTransform(Vector3.One, new Vector3(0f, visualOfsY, 0f));
        }
    }
}
