﻿using DerpySimulation.Core;
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

        // The spins can be global to save CPU for a lot of food
        private static Rotation _globalRotation;

        private float _bounceProgress;

        public readonly Vector3 Color;

        public FoodEntity(in Vector3 pos, in Vector3 color)
        {
            PR.Position = pos;
            Color = color;
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
        public void UpdateVisual(float delta)
        {
            Vector3 pos = PR.Position;
            pos.Y += UpdateBounce(delta);
            UpdateTransform(_globalRotation.Value, Scale, pos);
        }
    }
}
