using DerpySimulation.Core;
using DerpySimulation.World;
using System.Numerics;

namespace DerpySimulation.Render.Cameras
{
    internal sealed class Camera
    {
        private const float FOV = 70f;
        public const float NEAR_PLANE = 0.1f;
        public const float RENDER_DISTANCE = 1500f;

        public Matrix4x4 Projection;
        public PositionRotation PR;
        public ICameraMovement Movement;

        public Camera(ICameraMovement movement)
        {
            Movement = movement;
            UpdateProjection(Display.CurrentWidth, Display.CurrentHeight);
            Display.Resized += UpdateProjection;
        }

        private void UpdateProjection(uint w, uint h)
        {
            Projection = Matrix4x4.CreatePerspectiveFieldOfView(FOV * Utils.DegToRad, (float)w / h, NEAR_PLANE, RENDER_DISTANCE);
        }

        public Matrix4x4 CreateViewMatrix()
        {
            return CreateViewMatrix(PR);
        }
        public static Matrix4x4 CreateViewMatrix(in PositionRotation pr)
        {
            return CreateViewMatrix(pr.Position, pr.Rotation.Value);
        }
        public static Matrix4x4 CreateViewMatrix(in Vector3 pos, in Quaternion rot)
        {
            // A camera works by moving the entire world in the opposite direction of the camera
            return Matrix4x4.CreateTranslation(Vector3.Negate(pos)) * Matrix4x4.CreateFromQuaternion(Quaternion.Conjugate(rot));
        }
        /// <summary>For water</summary>
        public Matrix4x4 CreateReflectionViewMatrix(float waterY)
        {
            Vector3 pos = PR.Position;
            pos.Y -= 2 * (pos.Y - waterY);

            ref Rotation rot = ref PR.Rotation;
            Quaternion q = Rotation.CreateQuaternion(rot.Yaw, -rot.Pitch, rot.Roll);

            return CreateViewMatrix(pos, q);
        }

        /// <summary>Updates see <see cref="PR"/></summary>
        public void Update(float delta, Simulation sim)
        {
            Movement.Update(delta, sim, ref PR);
        }
    }
}
