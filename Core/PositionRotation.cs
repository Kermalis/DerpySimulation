using System.Numerics;
#if DEBUG
using System;
#endif

namespace DerpySimulation.Core
{
    internal struct PositionRotation
    {
        public static PositionRotation Default { get; } = new(Vector3.Zero, Rotation.Default);

        public Vector3 Position;
        public Rotation Rotation;

        public PositionRotation(in Vector3 pos, in Rotation rot)
        {
            Position = pos;
            Rotation = rot;
        }

        public void Slerp(in PositionRotation from, in PositionRotation to, float progress)
        {
            Position = Vector3.Lerp(from.Position, to.Position, progress);
            Rotation.Set(Quaternion.Slerp(from.Rotation.Value, to.Rotation.Value, progress));
        }

        #region Movement

        public void MoveForward(float value)
        {
            Position += Vector3.Transform(new Vector3(0, 0, -1), Rotation.Value) * value;
        }
        public void MoveForwardZ(float value)
        {
            Position.Z -= value;
        }
        public void MoveBackward(float value)
        {
            Position += Vector3.Transform(new Vector3(0, 0, +1), Rotation.Value) * value;
        }
        public void MoveBackwardZ(float value)
        {
            Position.Z += value;
        }
        public void MoveLeft(float value)
        {
            Position += Vector3.Transform(new Vector3(-1, 0, 0), Rotation.Value) * value;
        }
        public void MoveLeftX(float value)
        {
            Position.X -= value;
        }
        public void MoveRight(float value)
        {
            Position += Vector3.Transform(new Vector3(+1, 0, 0), Rotation.Value) * value;
        }
        public void MoveRightX(float value)
        {
            Position.X += value;
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
            return string.Format("X: {0}\nY: {1}\nZ: {2}\n{3}",
                MathF.Round(Position.X, 2), MathF.Round(Position.Y, 2), MathF.Round(Position.Z, 2),
                Rotation);
        }
#endif
    }
}
