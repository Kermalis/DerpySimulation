using DerpySimulation.Core;

namespace DerpySimulation.Render.Cameras
{
    internal interface ICameraMovement
    {
        void Update(float delta, ref PositionRotation pr);
    }
}
