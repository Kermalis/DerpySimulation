using DerpySimulation.World;

namespace DerpySimulation.Render
{
    internal interface ICameraMovement
    {
        void Update(float delta, Simulation sim, ref PositionRotation pr);
    }
}
