using DerpySimulation.Core;
using System.Numerics;

namespace DerpySimulation.Render
{
    internal sealed class Camera
    {
        private const float FOV = 90f;
        private const float RENDER_DISTANCE = 1500f;

        public Matrix4x4 Projection;
        public PositionRotation PR;

        public Camera(in PositionRotation pr)
        {
            PR = pr;
            ProgramMain.GetWindowSize(out uint w, out uint h);
            UpdateProjection(w, h);
            ProgramMain.Resized += UpdateProjection;
        }

        private void UpdateProjection(uint w, uint h)
        {
            Projection = Matrix4x4.CreatePerspectiveFieldOfView(Utils.DegreesToRadiansF(FOV), (float)w / h, 0.1f, RENDER_DISTANCE);
        }

        public Matrix4x4 CreateViewMatrix()
        {
            return CreateViewMatrix(PR);
        }
        public static Matrix4x4 CreateViewMatrix(in PositionRotation pr)
        {
            return CreateViewMatrix(pr.Position, pr.Rotation);
        }
        public static Matrix4x4 CreateViewMatrix(in Vector3 pos, in Quaternion rot)
        {
            // A camera works by moving the entire world in the opposite direction of the camera
            return Matrix4x4.CreateTranslation(Vector3.Negate(pos)) * Matrix4x4.CreateFromQuaternion(Quaternion.Conjugate(rot));
        }
    }
}
