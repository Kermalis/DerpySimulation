﻿using DerpySimulation.Core;
using DerpySimulation.Input;
using DerpySimulation.World;

namespace DerpySimulation.Render.Cameras
{
    internal sealed class FreeRoamCameraMovement : ICameraMovement
    {
        private const float PITCH_SPEED = 0.45f;
        private const float YAW_SPEED = 0.45f;
        private const float FLY_SPEED_MODIFIER = 5f; // How much scrolling changes your speed
        private const float FLY_SPEED_MIN = 5f;
        private const float FLY_SPEED_MAX = 250f;

        private const float PITCH_MIN = -75f;
        private const float PITCH_MAX = 90f;
        private const float X_SIZE = 0.1f;
        private const float Y_OFFSET = 5f; // Offset above the ground
        private const float Z_SIZE = 0.1f;

        private SmoothFloat _pitch = new(10f, 25f);
        private SmoothFloat _yaw = new(0f, 25f);
        private float _flySpeed = 35f;

        public void Continue(in Rotation rot)
        {
            _pitch.Current = rot.Pitch;
            _pitch.Target = _pitch.Current;
            _yaw.Current = -rot.Yaw;
            _yaw.Target = _yaw.Current;
        }

        public void Update(float delta, ref PositionRotation pr)
        {
            UpdatePitch(delta);
            UpdateYaw(delta);
            UpdateScroll();
            UpdatePos(delta, ref pr);

            // Don't allow the camera to leave the terrain or go into the ground
            Simulation.Instance.ClampToBordersAndFloor(ref pr.Position, X_SIZE, Z_SIZE, Y_OFFSET);

            float finalYaw = (360 - _yaw.Current) % 360;
            pr.Rotation.Set(finalYaw, _pitch.Current, 0f);
        }

        private void UpdatePitch(float delta)
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
            _pitch.Update(delta);
        }
        private void UpdateYaw(float delta)
        {
            float change = Mouse.DeltaX * YAW_SPEED;
            _yaw.Target -= change;
            _yaw.Update(delta);
        }
        private void UpdateScroll()
        {
            int scroll = Mouse.Scroll;
            if (scroll == 0)
            {
                return;
            }
            _flySpeed += scroll * FLY_SPEED_MODIFIER;
            // Clamp
            if (_flySpeed < FLY_SPEED_MIN)
            {
                _flySpeed = FLY_SPEED_MIN;
            }
            else if (_flySpeed > FLY_SPEED_MAX)
            {
                _flySpeed = FLY_SPEED_MAX;
            }
        }
        public void UpdatePos(float delta, ref PositionRotation pr)
        {
            bool forward = Keyboard.IsDown(Key.W);
            bool backward = Keyboard.IsDown(Key.S);
            if (forward && !backward)
            {
                pr.MoveForward(_flySpeed * delta);
            }
            else if (backward && !forward)
            {
                pr.MoveBackward(_flySpeed * delta);
            }

            bool left = Keyboard.IsDown(Key.A);
            bool right = Keyboard.IsDown(Key.D);
            if (left && !right)
            {
                pr.MoveLeft(_flySpeed * delta);
            }
            else if (right && !left)
            {
                pr.MoveRight(_flySpeed * delta);
            }

            bool up = Keyboard.IsDown(Key.Space);
            bool down = Keyboard.IsDown(Key.LShift);
            if (up && !down)
            {
                pr.MoveUpY(_flySpeed * delta);
            }
            else if (down && !up)
            {
                pr.MoveDownY(_flySpeed * delta);
            }
        }
    }
}
