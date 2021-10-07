using DerpySimulation.Core;
using DerpySimulation.World;

namespace DerpySimulation.Render.Cameras
{
    internal interface ICameraMovement
    {
        void Update(float delta, Simulation sim, ref PositionRotation pr);
    }
}
