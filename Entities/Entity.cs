using DerpySimulation.Core;
using System.Numerics;

namespace DerpySimulation.Entities
{
    internal abstract class Entity
    {
        /// <summary>The transform matrix after being updated this frame.</summary>
        public Matrix4x4 UpdatedTransform;

        public Vector3 Scale = Vector3.One;
        public PositionRotation PR;

        protected void UpdateTransform()
        {
            UpdateTransform(PR.Rotation.Value, Scale, PR.Position);
        }
        protected void UpdateTransform(in Vector3 visualScale, in Vector3 visualOffset)
        {
            UpdateTransform(PR.Rotation.Value, Scale * visualScale, PR.Position + visualOffset);
        }
        private void UpdateTransform(in Quaternion rotation, in Vector3 scale, in Vector3 position)
        {
            UpdatedTransform = Matrix4x4.CreateFromQuaternion(rotation)
                * Matrix4x4.CreateScale(scale)
                * Matrix4x4.CreateTranslation(position);
        }
    }
}
