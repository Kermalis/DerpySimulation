﻿using System.Numerics;
using DerpySimulation.Core;
#if DEBUG
using System;
#endif

namespace DerpySimulation.Render
{
    internal struct PositionRotation
    {
        public static PositionRotation Default { get; } = new(Vector3.Zero, Quaternion.Identity);

        public Vector3 Position;
        public Quaternion Rotation;

        private float _rollDegrees;
        private float _rollRadians;
        private float _pitchDegrees;
        private float _pitchRadians;
        private float _yawDegrees;
        private float _yawRadians;

        public PositionRotation(in Vector3 pos, in Quaternion rot)
            : this()
        {
            Position = pos;
            SetRotation(rot);
        }
        public PositionRotation(in Vector3 pos, float rollDegrees, float pitchDegrees, float yawDegrees)
            : this()
        {
            Position = pos;
            SetRotation(rollDegrees, pitchDegrees, yawDegrees);
        }

        public void ResetRotation()
        {
            _rollDegrees = 0;
            _rollRadians = 0;
            _pitchDegrees = 0;
            _pitchRadians = 0;
            _yawDegrees = 0;
            _yawRadians = 0;
            Rotation = Quaternion.Identity;
        }
        public void SetRotation(in Quaternion rot)
        {
            Rotation = rot;
            _rollRadians = -rot.GetRollRadiansF();
            _rollDegrees = Utils.RadiansToDegreesF(_rollRadians);
            _pitchRadians = -rot.GetPitchRadiansF();
            _pitchDegrees = Utils.RadiansToDegreesF(_pitchRadians);
            _yawRadians = -rot.GetYawRadiansF();
            _yawDegrees = Utils.RadiansToDegreesF(_yawRadians);
        }
        public void SetRotation(float rollDegrees, float pitchDegrees, float yawDegrees)
        {
            _rollDegrees = rollDegrees;
            _rollRadians = Utils.DegreesToRadiansF(rollDegrees);
            _pitchDegrees = pitchDegrees;
            _pitchRadians = Utils.DegreesToRadiansF(pitchDegrees);
            _yawDegrees = yawDegrees;
            _yawRadians = Utils.DegreesToRadiansF(yawDegrees);
            Rotation = CreateRotation(_yawRadians, _pitchRadians, _rollRadians);
        }
        public void UpdateRollDegrees(float degrees)
        {
            _rollDegrees = degrees;
            _rollRadians = Utils.DegreesToRadiansF(degrees);
            UpdateRotation();
        }
        public void UpdatePitchDegrees(float degrees)
        {
            _pitchDegrees = degrees;
            _pitchRadians = Utils.DegreesToRadiansF(degrees);
            UpdateRotation();
        }
        public void UpdateYawDegrees(float degrees)
        {
            _yawDegrees = degrees;
            _yawRadians = Utils.DegreesToRadiansF(degrees);
            UpdateRotation();
        }

        public static Quaternion CreateRotation(float yawRadians, float pitchRadians, float rollRadians)
        {
            return Quaternion.CreateFromYawPitchRoll(-yawRadians, -pitchRadians, -rollRadians);
        }
        private void UpdateRotation()
        {
            Rotation = Quaternion.CreateFromYawPitchRoll(-_yawRadians, -_pitchRadians, -_rollRadians);
        }

        public void LerpPosition(in Vector3 from, in Vector3 to, float progress)
        {
            Position = Vector3.Lerp(from, to, progress);
        }
        public void SlerpRotation(in Quaternion from, in Quaternion to, float progress)
        {
            SetRotation(Quaternion.Slerp(from, to, progress));
        }
        public void Slerp(in PositionRotation from, in PositionRotation to, float progress)
        {
            LerpPosition(from.Position, to.Position, progress);
            SlerpRotation(from.Rotation, to.Rotation, progress);
        }

        /// <summary>For water</summary>
        public Quaternion CreateReflectionQuaternion()
        {
            return CreateRotation(_yawRadians, -_pitchRadians, _rollRadians);
        }

        #region Movement

        public void MoveForward(float value)
        {
            Position += Vector3.Transform(new Vector3(0, 0, -1), Rotation) * value;
        }
        public void MoveForwardZ(float value)
        {
            Position.Z += value;
        }
        public void MoveBackward(float value)
        {
            Position += Vector3.Transform(new Vector3(0, 0, 1), Rotation) * value;
        }
        public void MoveBackwardZ(float value)
        {
            Position.Z -= value;
        }
        public void MoveLeft(float value)
        {
            Position += Vector3.Transform(new Vector3(-1, 0, 0), Rotation) * value;
        }
        public void MoveLeftX(float value)
        {
            Position.X += value;
        }
        public void MoveRightX(float value)
        {
            Position.X -= value;
        }
        public void MoveRight(float value)
        {
            Position += Vector3.Transform(new Vector3(1, 0, 0), Rotation) * value;
        }
        public void MoveUpY(float value)
        {
            Position.Y += value;
        }
        public void MoveDownY(float value)
        {
            Position.Y -= value;
        }

        #endregion

#if DEBUG
        public override string ToString()
        {
            return string.Format("X: {0}\nY: {1}\nZ: {2}\nRoll: {3}\nPitch: {4}\nYaw: {5}",
                MathF.Round(Position.X, 2), MathF.Round(Position.Y, 2), MathF.Round(Position.Z, 2),
                _rollDegrees, _pitchDegrees, _yawDegrees);
        }
#endif
    }
}
