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
using System.Linq;
#if DEBUG
using DerpySimulation.Debug;
#endif

namespace DerpySimulation.World
{
    internal sealed class Simulation
    {
        public static Simulation Instance { get; private set; } = null!; // Initialized in constructor

        private readonly SimulationRenderer _renderer;
        private ProgramMain.MainCallbackDelegate _unpauseCB = null!;

        private readonly Camera _camera;
        private readonly FreeRoamCameraMovement _freeRoamMovement;
        private readonly LockOnCameraMovement _lockOnMovement;

        private readonly TerrainTile _terrain;
        private readonly WaterTile _water;

        private readonly List<Entity> _entities;
        private Entity[] _updateEntities;
        private readonly LehmerRand _rand;

        private float _foodTime;
        private const float FoodSpawnSpeed = 75f; // Amount per second
        private int _logLastNumDerps = 0;

#if DEBUG_TEST_SUN
        private int _tempTime;
        private const float TempTimeLimit = 6000f;
#endif

        public Simulation(GL gl, in SimulationCreationSettings settings)
        {
            Instance = this;

            // Load graphics
            _renderer = new SimulationRenderer(gl);
            FoodRenderer.Init(gl);
            BoxRenderer.Init(gl);
            LightController.Init();

            // Create terrain
            _terrain = TerrainGenerator.GenerateTerrain(gl, settings, out Vector3 peak);

            // Create water
#if DEBUG
            Log.WriteLineWithTime("Generating water...");
#endif
            _water = WaterGenerator.Generate(gl, settings, _terrain);
#if DEBUG
            Log.WriteLineWithTime("Done generating! The peak is at " + peak);
#endif

            // Create camera
            peak.Y += 5;
            _freeRoamMovement = new FreeRoamCameraMovement();
            _lockOnMovement = new LockOnCameraMovement();
            _camera = new Camera(_freeRoamMovement);
            _camera.PR.Position = peak;
            Mouse.LockMouseInWindow(true);

            // Test add some lights
            LightController.Instance.Add(new(new Vector3(settings.SizeX - (settings.SizeX / 4), 90f, settings.SizeZ - (settings.SizeZ / 4)), new Vector3(20f, 0f, 0f), new Vector3(1f, 0.01f, 0.002f)));
            LightController.Instance.Add(new(new Vector3(settings.SizeX - (settings.SizeX / 4), 90f, settings.SizeZ / 4), new Vector3(0f, 10f, 10f), new Vector3(1f, 0.01f, 0.002f)));
            LightController.Instance.Add(new(new Vector3(0f, 150f, 0f), new Vector3(0f, 0f, 15f), new Vector3(1f, 0.01f, 0.002f)));

            const int numTestDerps = 1000;

            _rand = new LehmerRand();
            _entities = new List<Entity>((int)FoodEntity.MAX_FOOD + numTestDerps);
            _updateEntities = new Entity[_entities.Capacity];

            // Test add derps
            for (int i = 0; i < numTestDerps; i++)
            {
                RandomPos(out float x, out float z);
                float y = GetHeight(x, z);
                _entities.Add(new LivingEntity(new Vector3(x, y, z), _rand));
            }
        }

        public static void Debug_CreateSimulation(GL gl)
        {
            var sim = new Simulation(gl, SimulationCreationSettings.CreatePreset(2));
            ProgramMain.SetCallbacks(sim.CB_FreeRoam, sim.Delete);
        }

        public float GetHeight(float x, float z)
        {
            return _terrain.GetHeight(x, z);
        }
        public void ClampToBorders(ref float x, ref float z, float sizeX, float sizeZ)
        {
            float xd2 = sizeX * 0.5f;
            float zd2 = sizeZ * 0.5f;
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
        public bool ClampToBordersAndFloor(ref Vector3 pos, float xSize, float zSize, float floorOffset)
        {
            ClampToBorders(ref pos.X, ref pos.Z, xSize, zSize);
            return ClampToFloor(ref pos, floorOffset);
        }
        public bool ClampToFloor(ref Vector3 pos, float floorOffset)
        {
            float floor = GetHeight(pos.X, pos.Z) + floorOffset;
            if (pos.Y < floor)
            {
                pos.Y = floor;
                return true;
            }
            return pos.Y == floor;
        }
        public bool IsUnderwater(float y)
        {
            return y <= _water.Y;
        }
        public void RandomPos(out float x, out float z)
        {
            x = _rand.NextFloatNo1() * _terrain.SizeX;
            z = _rand.NextFloatNo1() * _terrain.SizeZ;
        }

        // Finds closest food instead of first food detected
        public FoodEntity? FindFood(in Vector3 pos, float senseDistSqr)
        {
            float best = float.PositiveInfinity;
            FoodEntity? bestE = null;
            for (int i = 0; i < _entities.Count; i++)
            {
                Entity e = _entities[i];
                if (e is FoodEntity f)
                {
                    float distSqr = Vector3.DistanceSquared(pos, f.PR.Position);
                    if (distSqr <= senseDistSqr && (bestE is null || distSqr < best))
                    {
                        best = distSqr;
                        bestE = f;
                    }
                }
            }
            return bestE;
        }
        public void SomethingDied(Entity e)
        {
            _entities.Remove(e);
        }

        private void SpawnFood(float delta)
        {
            _foodTime = (FoodSpawnSpeed * delta) + _foodTime;
            while (_foodTime >= 1f)
            {
                _foodTime--;
                if (FoodEntity.CanSpawnFood())
                {
                    RandomPos(out float x, out float z);
                    float y = 500f; // GetHeight(x, z);
                    // Giving a random velocity doesn't change much
                    _entities.Add(new FoodEntity(new Vector3(x, y, z), _rand));
                }
            }
        }
        private void RunSimulation(GL gl, float delta)
        {
            FoodRenderer.Instance.NewFrame(delta);
            BoxRenderer.Instance.NewFrame();

            SpawnFood(delta);

            // Update entities unless they're dead
            int numEntities = _entities.Count;
            if (_updateEntities.Length < numEntities)
            {
                Array.Resize(ref _updateEntities, numEntities);
            }
            _entities.CopyTo(_updateEntities);
            for (int i = 0; i < numEntities; i++)
            {
                Entity e = _updateEntities[i];
                if (!e.IsDead)
                {
                    e.Update(gl, delta);
                }
            }

            // Update camera
            _camera.Update(delta);
            if (LivingEntity.NumAliveDerps != _logLastNumDerps)
            {
                _logLastNumDerps = LivingEntity.NumAliveDerps;
                Console.WriteLine("Derps alive: " + LivingEntity.NumAliveDerps);
                if (LivingEntity.NumAliveDerps != 0)
                {
                    IEnumerable<LivingEntity> derps = _entities.OfType<LivingEntity>();
                    Console.WriteLine("Average lunge speed: " + derps.Average(d => d.LungeSpeed));
                    Console.WriteLine("Average sense distance: " + MathF.Sqrt(derps.Average(d => d.SenseDistSquared)));
                    Console.WriteLine("Average size: " + derps.Average(d => d.Scale.X));
                }
            }

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
        }

        private bool CheckForPause(GL gl, ProgramMain.MainCallbackDelegate unpause)
        {
            if (Keyboard.JustPressed(Key.Escape))
            {
                Mouse.LockMouseInWindow(false);
                Mouse.CenterMouseInWindow();

                _unpauseCB = unpause;
                ProgramMain.SetCallbacks(CB_Paused, Delete);

                Render(gl); // Still render this frame before returning
                return true;
            }
            return false;
        }
        private void CB_FreeRoam(GL gl, float delta)
        {
            if (CheckForPause(gl, CB_FreeRoam))
            {
                return;
            }

            if (Mouse.JustPressed(MouseButton.X1))
            {
                _lockOnMovement.Target = _entities[0].PR.Position;
                _camera.Movement = _lockOnMovement;
                Mouse.LockMouseInWindow(false);
                Mouse.CenterMouseInWindow();

                ProgramMain.SetCallbacks(CB_FollowEntity, Delete);
            }
            // Food thrower
            if ((Mouse.JustPressed(MouseButton.Left) || Mouse.IsDown(MouseButton.Right)) && FoodEntity.CanSpawnFood())
            {
                const float THROW_STRENGTH = 200f;
                var e = new FoodEntity(_camera.PR.Position, _rand);
                e.Velocity = _camera.PR.Rotation.ForwardDirection * THROW_STRENGTH;
                _entities.Add(e);
            }
            // Food explosion
            if (Mouse.JustPressed(MouseButton.X2))
            {
                for (int i = 0; i < _entities.Count; i++)
                {
                    Entity e = _entities[i];
                    if (e is FoodEntity)
                    {
                        e.PR = _camera.PR;
                        e.Velocity = new Vector3(_rand.NextFloatRange(-50f, 50f), 0f, _rand.NextFloatRange(-50f, 50f));
                    }
                }
            }

            RunSimulation(gl, delta);
            Render(gl);
        }
        private void CB_FollowEntity(GL gl, float delta)
        {
            if (CheckForPause(gl, CB_FollowEntity))
            {
                return;
            }

            if (Mouse.JustPressed(MouseButton.X1))
            {
                _freeRoamMovement.Continue(_camera.PR.Rotation);
                _camera.Movement = _freeRoamMovement;
                Mouse.LockMouseInWindow(true);

                ProgramMain.SetCallbacks(CB_FreeRoam, Delete);
            }
            else
            {
                _lockOnMovement.Target = _entities[0].PR.Position;
            }

            RunSimulation(gl, delta);
            Render(gl);
        }
        private void CB_Paused(GL gl, float delta)
        {
            // Check for unpause input
            if (Keyboard.JustPressed(Key.Escape))
            {
                if (_unpauseCB == CB_FreeRoam)
                {
                    Mouse.LockMouseInWindow(true);
                }
                ProgramMain.SetCallbacks(_unpauseCB, Delete);
            }

            Render(gl);
        }

        public void Render(GL gl)
        {
            _renderer.Render(gl, _camera, _terrain, _water);
        }

        public void Delete(GL gl)
        {
            Instance = null!;
            _terrain.Delete(gl);
            _water.Delete(gl);
            _renderer.Delete(gl);
            FoodRenderer.Instance.Delete(gl);
            BoxRenderer.Instance.Delete(gl);
            LightController.Delete();
        }
    }
}
