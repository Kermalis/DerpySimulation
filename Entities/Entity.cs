using DerpySimulation.Core;
using DerpySimulation.World;
using Silk.NET.OpenGL;
using System.Numerics;

namespace DerpySimulation.Entities
{
    internal abstract class Entity
    {
        public bool IsDead;
        public bool IsUnderwater;

        public Vector3 Scale;
        public PositionRotation PR;

        public Vector3 Velocity;
        public float Weight;

        public abstract void Update(float delta);
        public abstract void UpdateVisual(GL gl, float delta);

        public abstract Vector3 Debug_GetColor();

        protected bool ClampToBordersAndFloor()
        {
            return Simulation.Instance.ClampToBordersAndFloor(ref PR.Position, Scale.X, Scale.Z, 0f);
        }

        protected void ApplyPhysics(float delta)
        {
            // Update position with current velocity
            PR.Position += Velocity * delta;
            IsUnderwater = Simulation.Instance.IsUnderwater(PR.Position.Y);

            // Update acceleration
            const float GRAVITY = 100f;
            const float FRICTION = 0.9f;
            Vector3 acceleration = Velocity * -FRICTION;
            acceleration.Y -= GRAVITY * Weight;

            // Update velocity
            const float UNDERWATER_MOD = 0.9f;
            Velocity += acceleration * delta;
            if (IsUnderwater)
            {
                Velocity *= UNDERWATER_MOD; // Slow down when hitting the water
            }
            // Stop moving if velocity is close to 0
            if (Velocity.LengthSquared() < 0.01f)
            {
                Velocity = Vector3.Zero;
            }
        }

        public virtual void Die()
        {
            IsDead = true;
            Simulation.Instance.SomethingDied(this);
        }
    }
}
