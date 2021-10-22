using System.Numerics;

namespace DerpySimulation.Core
{
    internal struct Rotation
    {
        public static Rotation Default { get; } = new(Quaternion.Identity);

        /// <summary>Yaw degrees, where positive turns your head to the right.</summary>
        public float Yaw { get; private set; }
        /// <summary>Pitch degrees, where positive pitches your head downwards.</summary>
        public float Pitch { get; private set; }
        /// <summary>Roll degrees, where positive rolls your head to the right.</summary>
        public float Roll { get; private set; }

        /// <summary>The final Quaternion to be used in a matrix.</summary>
        public Quaternion Value { get; private set; }
        /// <summary>The normalized forward vector for this rotation.</summary>
        public Vector3 ForwardDirection { get; private set; }

        public Rotation(in Quaternion rot)
            : this()
        {
            Set(rot);
        }
        public Rotation(float yaw, float pitch, float roll)
            : this()
        {
            Set(yaw, pitch, roll);
        }

        public void Reset()
        {
            Yaw = 0f;
            Pitch = 0f;
            Roll = 0f;
            Value = Quaternion.Identity;
            ForwardDirection = new Vector3(0, 0, -1); // Negative z is north
        }
        public void Set(in Quaternion value)
        {
            Yaw = -value.GetYawRadians() * Utils.RadToDeg;
            Pitch = -value.GetPitchRadians() * Utils.RadToDeg;
            Roll = -value.GetRollRadians() * Utils.RadToDeg;
            Value = value;
            ForwardDirection = Vector3.Transform(new Vector3(0, 0, -1), value);
        }
        public void Set(float yaw, float pitch, float roll)
        {
            Yaw = yaw;
            Pitch = pitch;
            Roll = roll;
            Value = CreateQuaternion(yaw, pitch, roll);
            ForwardDirection = Vector3.Transform(new Vector3(0, 0, -1), Value);
        }
        /// <summary><paramref name="direction"/> is calculated as (from - to) and must be normalized.</summary>
        public void FaceDirection(in Vector3 direction)
        {
            Value = Utils.CreateLookRotation(direction, Vector3.UnitY);
            Yaw = -Value.GetYawRadians() * Utils.RadToDeg;
            Pitch = -Value.GetPitchRadians() * Utils.RadToDeg;
            Roll = -Value.GetRollRadians() * Utils.RadToDeg;
            ForwardDirection = Vector3.Transform(new Vector3(0, 0, -1), Value); // Calculate this way (instead of just -direction) due to quirks and floating points
        }

        public static Quaternion CreateQuaternion(float yaw, float pitch, float roll)
        {
            return Quaternion.CreateFromYawPitchRoll(-yaw * Utils.DegToRad, -pitch * Utils.DegToRad, -roll * Utils.DegToRad);
        }

#if DEBUG
        public override string ToString()
        {
            return string.Format("Yaw: {0}\nPitch: {1}\nRoll: {2}", Yaw, Pitch, Roll);
        }
#endif
    }
}
