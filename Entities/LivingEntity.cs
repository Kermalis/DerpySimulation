using DerpySimulation.Core;
using DerpySimulation.Render;
using DerpySimulation.Render.Data;
using DerpySimulation.Render.Renderers;
using DerpySimulation.World;
using Silk.NET.OpenGL;
using System;
using System.Numerics;

namespace DerpySimulation.Entities
{
    internal sealed class LivingEntity : Entity
    {
        const float MIN_LUNGE_SPEED = 1f;
        const float MAX_LUNGE_SPEED = 80f;

        const float MIN_SIZE = 0.1f;
        const float MAX_SIZE = 5f;

        const float MIN_SENSE_DIST = 15f;
        const float MAX_SENSE_DIST = 75f;

        const float STARVATION_TIME = 60f; // Seconds
        const float REPRODUCE_TIME = 80f; // Seconds

        public static int NumAliveDerps;

        // Genes
        private readonly LehmerRand _rand;
        private readonly Vector3 _baseColor;
        private readonly BoxColors _boxColors;
        public readonly Vector3 LungeSpeed; // public for now
        public readonly float SenseDist; // public for now
        private readonly float _senseDistSquared;

        // Requirements
        private float _timeWithoutFood;
        private float _timeWithoutReproduce;

        private Action _callback1 = null!; // Set in DecideCallbacks()
        private Action _callback2 = null!; // Set in DecideCallbacks()

        // Go to food callback
        private FoodEntity? _goFood;
        // Wander callback
        private Vector3 _wanderPos;

        /// <summary>Create a brand new living entity with random genetics.</summary>
        public LivingEntity(in Vector3 pos, LehmerRand rand, bool isInitialPopulation)
        {
            rand = new LehmerRand(rand.NextUint());
            _rand = rand;

            NumAliveDerps++;

            PR = new PositionRotation(pos, Rotation.Default);

            float size = rand.NextFloatRange(MIN_SIZE, MAX_SIZE);
            Scale = new Vector3(size, size, size);
            UpdateWeight();

            _baseColor = rand.NextVector3Range(0.25f, 1f);
            _boxColors = new BoxColors(_baseColor, rand);

            LungeSpeed.X = rand.NextFloatRange(MIN_LUNGE_SPEED, MAX_LUNGE_SPEED);
            LungeSpeed.Y = rand.NextFloatRange(MIN_LUNGE_SPEED, MAX_LUNGE_SPEED);
            LungeSpeed.Z = rand.NextFloatRange(MIN_LUNGE_SPEED, MAX_LUNGE_SPEED);
            SenseDist = rand.NextFloatRange(MIN_SENSE_DIST, MAX_SENSE_DIST);
            _senseDistSquared = SenseDist * SenseDist;

            // Don't decide callbacks here since the simulation isn't created yet.
            if (!isInitialPopulation)
            {
                DecideCallbacks();
            }
        }
        /// <summary>Create a new living entity based on an asexual parent.</summary>
        private LivingEntity(LivingEntity parent)
        {
            _rand = new LehmerRand(parent._rand.NextUint());

            NumAliveDerps++;

            PR = parent.PR;

            // Mutate size slightly
            float size = parent.Scale.X;
            size += _rand.NextFloatRange(-0.1f, 0.1f);
            size = Math.Clamp(size, MIN_SIZE, MAX_SIZE);
            Scale = new Vector3(size, size, size);
            UpdateWeight();

            // Mutate color slightly
            _baseColor = parent._baseColor;
            _baseColor += _rand.NextVector3Range(-0.05f, 0.05f);
            _baseColor = Utils.ClampVector3(_baseColor, 0.25f, 1f);
            _boxColors = new BoxColors(_baseColor, _rand);

            // Mutate lunge speed slightly
            LungeSpeed = parent.LungeSpeed;
            LungeSpeed += _rand.NextVector3Range(-0.1f, 0.1f);
            LungeSpeed = Utils.ClampVector3(LungeSpeed, MIN_LUNGE_SPEED, MAX_LUNGE_SPEED);

            // Mutate sense distance slightly
            SenseDist = parent.SenseDist;
            SenseDist += _rand.NextFloatRange(-0.1f, 0.1f);
            SenseDist = Math.Clamp(SenseDist, MIN_SENSE_DIST, MAX_SENSE_DIST);
            _senseDistSquared = SenseDist * SenseDist;

            // Babies get shot in at a random direction
            Velocity.X = _rand.NextFloatRange(1f, 2.5f);
            if (_rand.NextBoolean())
            {
                Velocity.X = -Velocity.X;
            }
            Velocity.Y = _rand.NextFloatRange(0.5f, 1.5f);
            Velocity.Z = _rand.NextFloatRange(1f, 2.5f);
            if (_rand.NextBoolean())
            {
                Velocity.Z = -Velocity.Z;
            }
            Velocity *= parent.LungeSpeed;

            DecideCallbacks();
        }

        private void UpdateWeight()
        {
            Weight = Scale.Length() * 0.2f;
        }

        public override void Die()
        {
            NumAliveDerps--;
            base.Die();
        }

        public override void Update(float delta)
        {
            // Check for starvation
            _timeWithoutFood += delta;
            if (_timeWithoutFood >= STARVATION_TIME)
            {
                Die();
                return;
            }

            // Check for reproduction
            _timeWithoutReproduce += delta;
            if (_timeWithoutReproduce >= REPRODUCE_TIME)
            {
                _timeWithoutReproduce -= REPRODUCE_TIME;
                Simulation.Instance.AddEntity(new LivingEntity(this));
            }

            _callback1();
            ApplyPhysics(delta);
            _callback2();
        }
        public override void UpdateVisual(GL gl, float delta)
        {
            Vector3 visualPos = PR.Position;
            visualPos.Y += Scale.Y * 0.5f;
            // Shake slightly based on hunger
            const float startShakingPercent = 0.8f;
            float factor = _timeWithoutFood / STARVATION_TIME;
            if (factor >= startShakingPercent)
            {
                float shakeAmt = factor * 0.065f; // Shake more the closer we are to starvation
                visualPos += Scale * _rand.NextVector3Range(-shakeAmt, shakeAmt);
            }
            BoxRenderer.Instance.Add(gl, _boxColors, RenderUtils.CreateTransform(Scale, PR.Rotation.Value, visualPos));
        }
        public override Vector3 Debug_GetColor()
        {
            return _baseColor;
        }

        // public because of initial population in simulation
        public void DecideCallbacks()
        {
            if (TrySetGoFood())
            {
                return; // If food was found, callbacks were set
            }

            StartWander();
        }
        private bool TrySetGoFood()
        {
            _goFood = Simulation.Instance.FindFood(PR.Position, _senseDistSquared);
            if (_goFood is not null)
            {
                _callback1 = CB1_GoToFood;
                _callback2 = CB2_GoToTarget;
                return true;
            }
            return false;
        }
        private void StartWander()
        {
            Simulation sim = Simulation.Instance;
            _wanderPos = PR.Position;

            float w = _rand.NextFloatRange(15f, 30f);
            _wanderPos.X += _rand.NextBoolean() ? w : -w;
            w = _rand.NextFloatRange(15f, 30f);
            _wanderPos.Z += _rand.NextBoolean() ? w : -w;

            sim.ClampToBorders(ref _wanderPos.X, ref _wanderPos.Z, Scale.X, Scale.Z);
            _wanderPos.Y = sim.GetHeight(_wanderPos.X, _wanderPos.Z);

            float waterY = sim.Water.Y;
            if (_wanderPos.Y < waterY)
            {
                float dif = waterY - _wanderPos.Y;
                _wanderPos.Y += dif * _rand.NextFloat(); // Random height within the water
            }
            else
            {
                _wanderPos.Y += Scale.Y * 0.5f; // Don't stare at the floor
            }

            _callback1 = CB1_Wander;
            _callback2 = CB2_GoToTarget;
        }

        private void Lunge(in Vector3 dir)
        {
            Velocity = dir * LungeSpeed * _rand.NextVector3Range(0.25f, 1f);
        }
        private bool IsWithinReach(float distSqrd)
        {
            float eatDist = Scale.LengthSquared();
            return distSqrd < eatDist;
        }

        private void CB1_GoToFood()
        {
            // If food no longer exists, exit
            if (_goFood!.IsDead)
            {
                _goFood = null;
                DecideCallbacks();
                return;
            }
            float distSqrd = Vector3.DistanceSquared(PR.Position, _goFood.PR.Position);
            if (distSqrd >= _senseDistSquared)
            {
                _goFood = null; // Forget about food if it's too far
                DecideCallbacks();
                return;
            }
            if (IsWithinReach(distSqrd))
            {
                _timeWithoutFood = 0;
                _goFood.Die();
                _goFood = null;
                DecideCallbacks();
                return;
            }

            PR.LookAt(_goFood.PR.Position);
        }
        private void CB2_GoToTarget()
        {
            bool touchingGround = ClampToBordersAndFloor();
            // Swim
            if (IsUnderwater)
            {
                if (Velocity.X < 0.1f && Velocity.Z < 0.1f)
                {
                    Lunge(PR.Rotation.ForwardDirection);
                }
            }
            // Jump off the ground
            else if (touchingGround)
            {
                Vector3 f = PR.Rotation.ForwardDirection;
                Lunge(new Vector3(f.X, 1f, f.Z));
            }
        }

        private void CB1_Wander()
        {
            float distSqrd = Vector3.DistanceSquared(PR.Position, _wanderPos);
            if (IsWithinReach(distSqrd))
            {
                DecideCallbacks();
                return;
            }
            if (TrySetGoFood())
            {
                return; // If we wandered into food, exit
            }

            PR.LookAt(_wanderPos);
        }
    }
}
