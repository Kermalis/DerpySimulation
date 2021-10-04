using DerpySimulation.Core;
using DerpySimulation.Input;
using DerpySimulation.Render;
using DerpySimulation.World.Terrain;
using DerpySimulation.World.Water;
using Silk.NET.OpenGL;
using System.Numerics;
#if DEBUG
using DerpySimulation.Debug;
#endif
#if DEBUG_TEST_SUN
using System;
#endif

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
            /*var camMove = new LockOnCameraMovement
            {
                //Target = new Vector3(settings.SizeX / 2f, GetHeight(settings.SizeX / 2f, settings.SizeZ / 2f), settings.SizeZ / 2f)
                //Target = peak
                Target = LightController.Instance[1].Pos
            };*/
            var camMove = new FreeRoamCameraMovement();
            _camera = new Camera(camMove, this);
            _camera.PR.Position = peak;
            Mouse.LockMouseInWindow(true);
        }

        public static void CB_Debug_CreateSimulation(GL gl, float _)
        {
            var sim = new Simulation(gl, SimulationCreationSettings.CreatePreset(0));
            ProgramMain.Callback = sim.CB_RunSimulation;
        }

        public float GetHeight(float x, float z)
        {
            return _terrain.GetHeight(x, z);
        }
        public void ClampToBorders(ref float x, ref float z)
        {
            if (x < 0)
            {
                x = 0;
            }
            else if (x >= _terrain.SizeX)
            {
                x = _terrain.SizeX - float.Epsilon;
            }
            if (z < 0)
            {
                z = 0;
            }
            else if (z >= _terrain.SizeZ)
            {
                z = _terrain.SizeZ - float.Epsilon;
            }
        }
        public void ClampToBordersAndFloor(ref Vector3 pos, float yOffset)
        {
            ClampToBorders(ref pos.X, ref pos.Z);
            float floor = GetHeight(pos.X, pos.Z) + yOffset;
            if (pos.Y < floor)
            {
                pos.Y = floor;
            }
        }

        public void CB_RunSimulation(GL gl, float delta)
        {
            // Check for pause input
            if (Keyboard.JustPressed(Key.Escape))
            {
                Mouse.LockMouseInWindow(false);
                Mouse.CenterMouseInWindow();
                ProgramMain.Callback = CB_Paused;
                Render(gl); // Still render this frame before returning
                return;
            }

            _camera.Update(delta);

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

            Render(gl);
        }
        private void CB_Paused(GL gl, float _)
        {
            // Check for unpause input
            if (Keyboard.JustPressed(Key.Escape))
            {
                Mouse.LockMouseInWindow(true);
                ProgramMain.Callback = CB_RunSimulation;
            }

            Render(gl);
        }

        public void Render(GL gl)
        {
            _renderer.Render(gl, _camera, _terrain, _water);
        }
    }
}
