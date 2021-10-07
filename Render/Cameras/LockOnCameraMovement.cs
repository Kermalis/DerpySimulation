using DerpySimulation.Core;
using DerpySimulation.Input;
using DerpySimulation.World;
using System;
using System.Numerics;

namespace DerpySimulation.Render.Cameras
{
    /// <summary>Keeps the camera pointed at a certain position.</summary>
    internal sealed class LockOnCameraMovement : ICameraMovement
    {
        private const float PITCH_SPEED = 0.45f;
        private const float YAW_SPEED = 0.45f;
        private const float ZOOM_SPEED = 0.1f;

        private const float PITCH_MIN = -45f;
        private const float PITCH_MAX = 90f;
        private const float X_SIZE = 0.1f;
        private const float Y_OFFSET = 5f; // Extra height above the target
        private const float Z_SIZE = 0.1f;
        private const float ZOOM_MIN = 1f;
        private const float ZOOM_MAX = 100f;

        public Vector3 Target;

        private SmoothFloat _pitch = new(10, 10);
        private SmoothFloat _angleAroundTarget = new(0, 10);
        private SmoothFloat _distanceFromTarget = new(10, 5);

        public void Update(float delta, Simulation sim, ref PositionRotation pr)
        {
            Mouse.LockMouseIfJustClicked();

            UpdatePitch(delta);
            UpdateYaw(delta);
            UpdateZoom(delta);

            float pitchRadians = _pitch.Current * Utils.DegToRad;
            float horizontalDistance = _distanceFromTarget.Current * MathF.Cos(pitchRadians);
            float verticalDistance = _distanceFromTarget.Current * MathF.Sin(pitchRadians);
            float horizontalTheta = _angleAroundTarget.Current * Utils.DegToRad;

            ref Vector3 pos = ref pr.Position;
            pos.X = Target.X + (horizontalDistance * MathF.Sin(horizontalTheta));
            pos.Y = Target.Y + verticalDistance;
            pos.Z = Target.Z + (horizontalDistance * MathF.Cos(horizontalTheta));

            // Don't allow the camera to leave the terrain or go into the ground
            sim.ClampToBordersAndFloor(ref pos, X_SIZE, Y_OFFSET, Z_SIZE);

            float finalYaw = (360 - _angleAroundTarget.Current) % 360;
            pr.Rotation.Set(finalYaw, _pitch.Current, 0f);
        }

        private void UpdatePitch(float delta)
        {
            if (Mouse.IsDown(MouseButton.Left))
            {
                float target = _pitch.Target;
                float change = Mouse.DeltaY * PITCH_SPEED;
                target += change;
                // Clamp
                if (target < PITCH_MIN)
                {
                    target = PITCH_MIN;
                }
                else if (target > PITCH_MAX)
                {
                    target = PITCH_MAX;
                }
                _pitch.Target = target;
            }
            _pitch.Update(delta);
        }
        private void UpdateYaw(float delta)
        {
            if (Mouse.IsDown(MouseButton.Left))
            {
                float change = Mouse.DeltaX * YAW_SPEED;
                _angleAroundTarget.Target -= change;
            }
            _angleAroundTarget.Update(delta);
        }
        private void UpdateZoom(float delta)
        {
            int scroll = Mouse.Scroll;
            if (scroll != 0)
            {
                float target = _distanceFromTarget.Target;
                float change = scroll * ZOOM_SPEED * target; // Multiply by target so we zoom out really fast
                target -= change;
                // Clamp
                if (target < ZOOM_MIN)
                {
                    target = ZOOM_MIN;
                }
                else if (target > ZOOM_MAX)
                {
                    target = ZOOM_MAX;
                }
                _distanceFromTarget.Target = target;
            }
            _distanceFromTarget.Update(delta);
        }
    }
}
