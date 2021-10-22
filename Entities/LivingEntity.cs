using DerpySimulation.Core;
using DerpySimulation.Render;
using DerpySimulation.Render.Data;
using DerpySimulation.Render.Renderers;
using DerpySimulation.World;
using Silk.NET.OpenGL;
using System.Numerics;

namespace DerpySimulation.Entities
{
    internal sealed class LivingEntity : Entity
    {
        const float MIN_LUNGE_SPEED = 1f;
        const float MAX_LUNGE_SPEED = 40f;

        const float MIN_SIZE = 0.1f;
        const float MAX_SIZE = 5f;

        const float MIN_SENSE_DIST = 15f;
        const float MAX_SENSE_DIST = 75f;

        const float STARVATION_TIME = 60f; // Seconds

        public static int NumAliveDerps;

        private readonly LehmerRand _rand;
        private readonly Vector3 _baseColor;
        private readonly BoxColors _bColors;
        public readonly Vector3 LungeSpeed; // public for now
        public readonly float SenseDistSquared; // public for now

        private float _timeWithoutFood;

        private FoodEntity? _goFood;

        public LivingEntity(in Vector3 pos, LehmerRand rand)
        {
            rand = new LehmerRand(rand.Next());
            _rand = rand;

            NumAliveDerps++;

            PR = new PositionRotation(pos, Rotation.Default);

            float width = rand.NextFloatRange(MIN_SIZE, MAX_SIZE);
            float height = width;
            float length = width;

            Scale = new Vector3(width, height, length);
            Weight = Scale.Length() * 0.2f;

            _baseColor = rand.NextVector3Range(0.25f, 1f);
            _bColors.East = rand.NextVector3Range(0.75f, 1f) * _baseColor;
            _bColors.West = rand.NextVector3Range(0.75f, 1f) * _baseColor;
            _bColors.Up = rand.NextVector3Range(0.75f, 1f) * _baseColor;
            _bColors.Down = rand.NextVector3Range(0.25f, 0.75f) * _baseColor; // Under is darkest
            _bColors.South = rand.NextVector3Range(0.5f, 0.9f) * _baseColor; // Butt is slightly darker
            _bColors.North = rand.NextVector3Range(1.1f, 1.5f) * _baseColor; // Face is brightest

            LungeSpeed.X = rand.NextFloatRange(MIN_LUNGE_SPEED, MAX_LUNGE_SPEED);
            LungeSpeed.Y = rand.NextFloatRange(MIN_LUNGE_SPEED, MAX_LUNGE_SPEED);
            LungeSpeed.Z = rand.NextFloatRange(MIN_LUNGE_SPEED, MAX_LUNGE_SPEED);
            SenseDistSquared = rand.NextFloatRange(MIN_SENSE_DIST, MAX_SENSE_DIST);
            SenseDistSquared *= SenseDistSquared;
        }

        public override void Die()
        {
            NumAliveDerps--;
            base.Die();
        }

        public override void Update(GL gl, float delta)
        {
            _timeWithoutFood = (1f * delta) + _timeWithoutFood;
            if (_timeWithoutFood >= STARVATION_TIME)
            {
                Die();
                return;
            }

            // Find new food or look at food
            if (_goFood is null || _goFood.IsDead)
            {
                _goFood = Simulation.Instance.FindFood(PR.Position, SenseDistSquared);
            }
            else
            {
                PR.LookAt(_goFood.PR.Position);

                // Try to eat
                float eatDist = Scale.LengthSquared();
                if (Vector3.DistanceSquared(PR.Position, _goFood.PR.Position) < eatDist)
                {
                    _timeWithoutFood = 0;
                    _goFood.Die();
                    _goFood = null;
                }
            }

            ApplyPhysics(delta);
            bool touchingGround = ClampToBordersAndFloor();
            // Swim
            if (IsUnderwater)
            {
                if (Velocity.X < 0.1f && Velocity.Z < 0.1f)
                {
                    Vector3 forward = PR.Rotation.ForwardDirection;
                    Velocity = forward * LungeSpeed * _rand.NextVector3Range(0.25f, 1f);
                }
            }
            // Jump off the ground
            else if (touchingGround)
            {
                Vector3 forward = _goFood is null ? Vector3.UnitY : PR.Rotation.ForwardDirection;
                Velocity = new Vector3(forward.X, 1f, forward.Z) * LungeSpeed * _rand.NextVector3Range(0.25f, 1f);
            }

            Vector3 visualPos = PR.Position;
            visualPos.Y += Scale.Y * 0.5f;
            BoxRenderer.Instance.Add(gl, _bColors, RenderUtils.CreateTransform(Scale, PR.Rotation.Value, visualPos));
        }
    }
}
