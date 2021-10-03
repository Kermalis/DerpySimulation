#if DEBUG
using DerpySimulation.Debug;
#endif
#if DEBUG_TEST_SUN
using DerpySimulation.Core;
using System;
#endif
using DerpySimulation.Render;
using Silk.NET.OpenGL;
using System.Numerics;
using DerpySimulation.World.Terrain;
using DerpySimulation.World.Water;

namespace DerpySimulation.World
{
    internal sealed class Simulation
    {
        private readonly SimulationRenderer _renderer;

        private readonly Camera _camera;
        private readonly TerrainTile _terrain;
        private readonly WaterTile _water;

#if DEBUG_TEST_SUN
        private int _tempTime;
        private const float TempTimeLimit = 6000f;
#endif

        public Simulation(GL gl, in SimulationCreationSettings settings)
        {
            _renderer = new SimulationRenderer(gl);

            // Create terrain
            _terrain = TerrainGenerator.GenerateTerrain(gl, settings, out Vector3 peak);

            // Create water
#if DEBUG
            Log.WriteLineWithTime("Generating water...");
#endif
            _water = WaterGenerator.Generate(gl, settings, _terrain);

            // Test add some lights
            LightController.Instance.Add(new(new Vector3(settings.SizeX - (settings.SizeX / 4), 90, settings.SizeZ - (settings.SizeZ / 4)), new Vector3(10, 134f / 255, 5), new Vector3(1f, 0.01f, 0.002f)));
            LightController.Instance.Add(new(new Vector3(settings.SizeX - (settings.SizeX / 4), 90, settings.SizeZ / 4), new Vector3(218f / 255, 134f / 255, 226f / 255), new Vector3(1f, 0.01f, 0.002f)));
            LightController.Instance.Add(new(new Vector3(settings.SizeX / 4, 90, settings.SizeZ / 4), new Vector3(218f / 255, 134f / 255, 226f / 255), new Vector3(1f, 0.01f, 0.002f)));

            // Create camera
#if DEBUG
            Log.WriteLineWithTime("Done generating! The peak is at " + peak);
#endif
            peak.Y += 5;
            var pr = new PositionRotation(peak, 0f, 30f, 0f);
            _camera = new Camera(pr);
        }

        public void LogicTick()
        {
            _camera.PR.Debug_Move(50f);
            //System.Console.WriteLine(_camera.PR);
        }

        public void Render(GL gl)
        {
#if DEBUG_TEST_SUN
            // Sun
            _tempTime++;
            if (_tempTime >= TempTimeLimit)
            {
                _tempTime = 0;
            }
            float timeF = _tempTime / TempTimeLimit * 360;
            PointLight sun = LightController.Instance.Sun;
            // Temp move the sun west
            // Pretending the sun is rising from the east (negative x)
            sun.Pos.X = MathF.Sin(Utils.DegreesToRadiansF(timeF + 270)) * 20000; // 0 -> -20000, 0.25 -> 0, 0.5 -> 20000, 0.75 -> 0
            sun.Pos.Y = MathF.Sin(Utils.DegreesToRadiansF(timeF)) * 20000; // 0 -> 0, 0.25 -> 20000, 0.5 -> 0, 0.75 -> -20000
#endif

#if DEBUG_CAM_GRAVITY
            const float gravity = 0.1f;
            const float eyeHeight = 5.5f;
            Vector3 camPos = _camera.PR.Position;
            camPos.Y -= eyeHeight;

            camPos.Y -= gravity;
            float floorHeight = _terrain.GetHeight(camPos.X, camPos.Z);
            if (camPos.Y < floorHeight)
            {
                camPos.Y = floorHeight;
            }

            camPos.Y += eyeHeight;
            _camera.PR.Position = camPos;
#endif

            _renderer.Render(gl, _camera, _terrain, _water);
        }
    }
}
