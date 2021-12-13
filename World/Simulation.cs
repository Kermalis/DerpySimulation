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
using System.Linq;
using System.Numerics;

namespace DerpySimulation.World
{
    internal sealed class Simulation
    {
        private const float FOOD_SPAWN_Y = 500f;
        private const float FOOD_SPAWN_SPEED = 100f; // Amount per second
        private const int SIM_SPEED = 1;

        public static Simulation Instance { get; private set; } = null!; // Initialized in constructor

        private readonly SimulationRenderer _renderer;
        private ProgramMain.MainCallbackDelegate _unpauseCB = null!;

        private readonly Camera _camera;
        private readonly FreeRoamCameraMovement _freeRoamMovement;
        private readonly LockOnCameraMovement _lockOnMovement;

        private readonly TerrainTile _terrain;
        public readonly WaterTile Water; // public for now (to get Y)

        private readonly List<Entity> _entities;
        private Entity[] _updateEntities;
        private readonly LehmerRand _rand;

        private float _foodTime;
        private int _logLastNumDerps = 0;

#if DEBUG_TEST_SUN
        private float _tempTime;
        private const float TempDayLength = 60f; // Seconds per day
#endif

        public Simulation(GL gl, SimulationCreator creator)
        {
            Instance = this;

            // Load graphics
            _renderer = new SimulationRenderer(gl);
            FoodRenderer.Init(gl);
            BoxRenderer.Init(gl);
            LightController.Init();

            _terrain = creator.Terrain;
            Water = creator.Water;

            // Test add some lights
            LightController.Instance.Add(new PointLight(Vector3.Zero, Vector3.Zero, new Vector3(1f, 0.01f, 0.002f)));
            LightController.Instance.Add(new PointLight(Vector3.Zero, Vector3.Zero, new Vector3(1f, 0.01f, 0.002f)));
            LightController.Instance.Add(new PointLight(Vector3.Zero, Vector3.Zero, new Vector3(1f, 0.01f, 0.002f)));

            _rand = creator.Rand;
            _entities = creator.InitialEntities;
            _updateEntities = new Entity[_entities.Capacity];
            // Start callbacks on our living entities
            for (int i = 0; i < _entities.Count; i++)
            {
                Entity e = _entities[i];
                if (e is LivingEntity le)
                {
                    le.DecideCallbacks();
                }
            }

            // Create camera
            Vector3 camPos = creator.TerrainPeak;
            camPos.Y += 5;
            _freeRoamMovement = new FreeRoamCameraMovement();
            _lockOnMovement = new LockOnCameraMovement();
            _camera = new Camera(_freeRoamMovement);
            _camera.PR.Position = camPos;
            Mouse.LockMouseInWindow(true);

            ProgramMain.SetCallbacks(CB_FreeRoam, Delete);
        }

        public float GetHeight(float x, float z, float outOfBoundsResult = 0f)
        {
            return _terrain.GetHeight(x, z, outOfBoundsResult: outOfBoundsResult);
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
            return y <= Water.Y;
        }
        public void RandomPos(out float x, out float z)
        {
            RandomPos(_rand, _terrain, out x, out z);
        }
        public static void RandomPos(LehmerRand rand, TerrainTile terrain, out float x, out float z)
        {
            x = rand.NextFloatNo1() * terrain.SizeX;
            z = rand.NextFloatNo1() * terrain.SizeZ;
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
        public void AddEntity(Entity e)
        {
            _entities.Add(e);
        }
        public void SomethingDied(Entity e)
        {
            _entities.Remove(e);
        }

        private void SpawnFood(float delta)
        {
            _foodTime += FOOD_SPAWN_SPEED * delta;
            while (_foodTime >= 1f)
            {
                _foodTime--;
                if (FoodEntity.CanSpawnFood())
                {
                    RandomPos(out float x, out float z);
                    // Giving a random velocity doesn't change much due to friction
                    _entities.Add(new FoodEntity(new Vector3(x, FOOD_SPAWN_Y, z), _rand));
                }
            }
        }
        private void UpdateEntities(GL gl, float delta, bool doVisual)
        {
            // Check for array resize
            int cap = _entities.Capacity;
            if (_updateEntities.Length < cap)
            {
                Array.Resize(ref _updateEntities, cap);
            }
            _entities.CopyTo(_updateEntities);

            int numEntities = _entities.Count; // Don't keep updating count because of entity changes
            for (int i = 0; i < numEntities; i++)
            {
                Entity e = _updateEntities[i];
                if (!e.IsDead)
                {
                    e.Update(delta);
                    if (doVisual)
                    {
                        e.UpdateVisual(gl, delta);
                    }
                }
            }
        }
#if DEBUG_TEST_SUN
        private void UpdateTimeOfDay(float delta)
        {
            _tempTime += delta;
            if (_tempTime >= TempDayLength)
            {
                _tempTime -= TempDayLength;
            }
            float t = _tempTime / TempDayLength * 360; // Map daytime to 0-360
            ref Vector3 sunPos = ref LightController.Instance.Sun.Pos;
            // Temp move the sun west
            // Pretending the sun is rising from the east (negative x)
            sunPos.X = MathF.Sin((t + 270) * Utils.DegToRad) * 20000; // 0 -> -20000, 0.25 -> 0, 0.5 -> 20000, 0.75 -> 0
            sunPos.Y = MathF.Sin(t * Utils.DegToRad) * 20000; // 0 -> 0, 0.25 -> 20000, 0.5 -> 0, 0.75 -> -20000
        }
#endif
#if DEBUG
        private void Debug_LightsOnEntities()
        {
            LightController lights = LightController.Instance;
            for (int i = 0; i < LightController.MAX_LIGHTS - 1; i++)
            {
                if (i >= _entities.Count)
                {
                    break;
                }
                PointLight lit = lights[i + 1];
                Entity e = _entities[i];
                lit.Pos = e.PR.Position;
                lit.Pos.Y += e.Size.Y * 0.5f;
                lit.Color = e.Debug_GetColor() * 2f;
            }
        }
#endif
        private void LogPopulation()
        {
            int numAlive = LivingEntity.NumAliveDerps;
            if (numAlive == _logLastNumDerps)
            {
                return;
            }

            _logLastNumDerps = numAlive;
            Console.WriteLine("({0})", DateTime.Now.ToLongTimeString());
            Console.WriteLine("Derps alive: " + numAlive);
            if (numAlive == 0)
            {
                return;
            }

            IEnumerable<LivingEntity> derps = _entities.OfType<LivingEntity>();
            int numInWater = derps.Count(d => d.IsUnderwater);
            int numOnLand = numAlive - numInWater;
            Console.WriteLine("- Derps on land: {0} ({1:P2})", numOnLand, numOnLand / (float)numAlive);
            Console.WriteLine("- Derps in water: {0} ({1:P2})", numInWater, numInWater / (float)numAlive);
            Console.WriteLine("-- Average lunge speed: " + derps.Average(d => d.LungeSpeed));
            Console.WriteLine("-- Average sense distance: " + derps.Average(d => d.SenseDist));
            Console.WriteLine("-- Average size: " + derps.Average(d => d.Size.X));
        }
        private void RunSimulation(GL gl, float delta)
        {
            FoodRenderer.Instance.NewFrame(delta);
            BoxRenderer.Instance.NewFrame();

            for (int i = 0; i < SIM_SPEED; i++)
            {
#if DEBUG_TEST_SUN
                UpdateTimeOfDay(delta);
#endif
                SpawnFood(delta);
                UpdateEntities(gl, delta, i == SIM_SPEED - 1);
            }

#if DEBUG
            Debug_LightsOnEntities(); // Temp put non-sun lights on oldest entities
#endif

            LogPopulation();

            _camera.Update(delta);
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
            _renderer.Render(gl, _camera, _terrain, Water);
        }

        public void Delete(GL gl)
        {
            Instance = null!;
            _terrain.Delete(gl);
            Water.Delete(gl);
            _renderer.Delete(gl);
            FoodRenderer.Instance.Delete(gl);
            BoxRenderer.Instance.Delete(gl);
            LightController.Delete();
        }
    }
}
