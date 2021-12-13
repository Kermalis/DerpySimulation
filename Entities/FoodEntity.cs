using DerpySimulation.Core;
using DerpySimulation.Render;
using DerpySimulation.Render.Renderers;
using Silk.NET.OpenGL;
using System.Numerics;

namespace DerpySimulation.Entities
{
    internal sealed class FoodEntity : Entity
    {
        public const uint MAX_FOOD = 2_000;

        // Degrees per second rotation
        private const float ROTATION_SPEEDX = 225f;
        private const float ROTATION_SPEEDY = 540f;
        private const float BASE_VISUAL_Y = 0.5f;
        private const float BOUNCE_HEIGHT = 0.75f;
        private const float BOUNCE_SPEED = 0.9f;

        // The spins can be global to save CPU for a lot of food
        private static Rotation _globalRotation;
        public static int NumAliveFood; // Used for food thrower

        private readonly Vector3 _color;
        private float _bounceProgress;

        public FoodEntity(in Vector3 pos, LehmerRand rand)
        {
            NumAliveFood++;

            PR = new PositionRotation(pos, Rotation.Default);
            Size = Vector3.One;
            Weight = 0.55f;

            _color = rand.NextVector3Range(0.35f, 1f);
        }

        public static bool CanSpawnFood()
        {
            return NumAliveFood < MAX_FOOD;
        }

        public override void Die()
        {
            NumAliveFood--;
            base.Die();
        }

        public static void UpdateSpin(float delta)
        {
            float rotX = (delta * ROTATION_SPEEDX) + _globalRotation.Pitch;
            rotX %= 360;
            float rotY = (delta * ROTATION_SPEEDY) + _globalRotation.Yaw;
            rotY %= 360;
            _globalRotation.Set(rotY, rotX, 0f);
        }
        private float UpdateBounce(float delta)
        {
            _bounceProgress += delta * BOUNCE_SPEED;
            _bounceProgress %= 1; // Clamp to 0-1
            return (Easing.BellCurve2(_bounceProgress) * BOUNCE_HEIGHT) + BASE_VISUAL_Y;
        }
        public override void Update(float delta)
        {
            ApplyPhysics(delta);
            if (ClampToBordersAndFloor())
            {
                Velocity = Vector3.Zero;
            }
        }
        public override void UpdateVisual(GL gl, float delta)
        {
            Vector3 visualPos = PR.Position;
            if (Velocity.Y == 0f) // Don't bounce if falling or rising
            {
                visualPos.Y += UpdateBounce(delta);
            }
            FoodRenderer.Instance.Add(gl, _color, RenderUtils.CreateTransform_ScaleRotPos(Size, _globalRotation.Value, visualPos));
        }
        public override Vector3 Debug_GetColor()
        {
            return _color;
        }
    }
}
