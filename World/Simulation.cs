using DerpySimulation.Core;
using DerpySimulation.Entities;
using DerpySimulation.Input;
using DerpySimulation.Render;
using DerpySimulation.Render.Cameras;
using DerpySimulation.Render.Renderers;
using DerpySimulation.World.Terrain;
using DerpySimulation.World.Water;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Numerics;
#if DEBUG
using DerpySimulation.Debug;
#endif

namespace DerpySimulation.World
{
    internal sealed class Simulation
    {
        private readonly SimulationRenderer _renderer;

        private readonly Camera _camera;
        private readonly TerrainTile _terrain;
        private readonly WaterTile _water;

        private readonly List<FoodEntity> _foodPieces;

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
            LightController.Instance.Add(new(new Vector3(settings.SizeX - (settings.SizeX / 4), 90f, settings.SizeZ - (settings.SizeZ / 4)), new Color3(20f, 0f, 0f), new Vector3(1f, 0.01f, 0.002f)));
            LightController.Instance.Add(new(new Vector3(settings.SizeX - (settings.SizeX / 4), 90f, settings.SizeZ / 4), new Color3(0f, 10f, 10f), new Vector3(1f, 0.01f, 0.002f)));
            LightController.Instance.Add(new(new Vector3(0f, 150f, 0f), new Color3(0f, 0f, 15f), new Vector3(1f, 0.01f, 0.002f)));
            // Test add food
            const int num = (int)FoodEntity.MAX_FOOD - 1;
            float f = 2f;
            uint randState = (uint)Environment.TickCount;
            _foodPieces = new List<FoodEntity>(num + 1)
            {
                new(new Vector3(0f, 101f, 0f), new Vector3(0.5f, 0.1f, 1f))
                //new(new Vector3(1f, 100f, 1f), new Vector3(0.5f, 0.1f, 1f))
            };
            for (int i = 0; i < num; i++)
            {
                float x = Utils.LehmerRandomizerFloat(ref randState) * settings.SizeX;
                float z = Utils.LehmerRandomizerFloat(ref randState) * settings.SizeZ;
                float y = GetHeight(x, z);
                //float y = 100f;
                _foodPieces.Add(new FoodEntity(new Vector3(x, y, z), Utils.RandomVector3(ref randState)));
                f += 1f;
            }

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
            _camera = new Camera(camMove);
            _camera.PR.Position = peak;
            Mouse.LockMouseInWindow(true);
        }

        public static void Debug_CreateSimulation(GL gl)
        {
            var sim = new Simulation(gl, SimulationCreationSettings.CreatePreset(0));
            ProgramMain.Callback = sim.CB_RunSimulation;
            ProgramMain.QuitCallback = sim.Delete;
        }

        public float GetHeight(float x, float z)
        {
            return _terrain.GetHeight(x, z);
        }
        public void ClampToBorders(ref float x, ref float z, float sizeX, float sizeZ)
        {
            float xd2 = sizeX / 2f;
            float zd2 = sizeZ / 2f;
            if (x - xd2 < 0f)
            {
                x = xd2;
            }
            else if (x + xd2 >= _terrain.SizeX)
            {
                x = _terrain.SizeX - xd2;
            }
            if (z - zd2 < 0f)
            {
                z = zd2;
            }
            else if (z + zd2 >= _terrain.SizeZ)
            {
                z = _terrain.SizeZ - zd2;
            }
        }
        public void ClampToBordersAndFloor(ref Vector3 pos, float xSize, float yOffset, float zSize)
        {
            ClampToBorders(ref pos.X, ref pos.Z, xSize, zSize);
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
                // QuitCallback is Delete

                Render(gl, delta); // Still render this frame before returning
                return;
            }

            _camera.Update(delta, this);

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
            sun.Pos.X = MathF.Sin((timeF + 270) * Utils.DegToRad) * 20000; // 0 -> -20000, 0.25 -> 0, 0.5 -> 20000, 0.75 -> 0
            sun.Pos.Y = MathF.Sin(timeF * Utils.DegToRad) * 20000; // 0 -> 0, 0.25 -> 20000, 0.5 -> 0, 0.75 -> -20000
#endif

            Render(gl, delta);
        }
        private void CB_Paused(GL gl, float delta)
        {
            // Check for unpause input
            if (Keyboard.JustPressed(Key.Escape))
            {
                Mouse.LockMouseInWindow(true);

                ProgramMain.Callback = CB_RunSimulation;
                // QuitCallback is Delete
            }

            Render(gl, delta);
        }

        public void Render(GL gl, float delta)
        {
            FoodRenderer.Instance.UpdateVisuals(gl, delta, _foodPieces);
            _renderer.Render(gl, _camera, _terrain, _water, _foodPieces);
        }

        public void Delete(GL gl)
        {
            _terrain.Delete(gl);
            _water.Delete(gl);
            _renderer.Delete(gl);
        }
    }
}
