using DerpySimulation.Core;
using DerpySimulation.Debug;
using DerpySimulation.Entities;
using DerpySimulation.Render.Shaders;
using DerpySimulation.World.Terrain;
using DerpySimulation.World.Water;
using Silk.NET.OpenGL;
using System.Collections.Generic;
using System.Numerics;

namespace DerpySimulation.World
{
    internal sealed class SimulationCreator
    {
        private enum State : byte
        {
            TerrainGen_Init,
            TerrainGen_Wait,
            WaterGen_Init,
            WaterGen_Wait,
            SpawnInitialPopulation,
            Finish
        }

        private readonly SimulationCreationSettings _settings;

        private State _state;

        // All of the attributes are set in CB_Create()
        private TerrainGenerator? _terrainGen;
        public TerrainTile Terrain = null!;
        public Vector3 TerrainPeak;

        private WaterGenerator? _waterGen;
        public WaterTile Water = null!;
        public LehmerRand Rand = null!;
        public List<Entity> InitialEntities = null!;

        public SimulationCreator(in SimulationCreationSettings settings)
        {
            _settings = settings;
            ProgramMain.SetCallbacks(CB_Create, QCB_Quit);
        }

        private void TerrainGen_Init()
        {
            _terrainGen = new TerrainGenerator(_settings);
        }
        private bool TerrainGen_Wait(GL gl)
        {
            if (_terrainGen!.IsDone(gl, out TerrainTile? terrain))
            {
                Terrain = terrain;
                TerrainPeak = _terrainGen.Peak;
                _terrainGen = null;
                return true;
            }
            return false;
        }
        private void WaterGen_Init()
        {
#if DEBUG
            Log.WriteLineWithTime("Generating water...");
#endif
            _waterGen = new WaterGenerator(_settings, Terrain);
        }
        private bool WaterGen_Wait(GL gl)
        {
            if (_waterGen!.IsDone(gl, out WaterTile? water))
            {
                Water = water;
                _waterGen = null;
                return true;
            }
            return false;
        }
        private void SpawnInitialPopulation()
        {
            Rand = new LehmerRand();
            int derps = _settings.InitialPopulation;
            InitialEntities = new List<Entity>((int)FoodEntity.MAX_FOOD + derps);
            for (int i = 0; i < derps; i++)
            {
                Simulation.RandomPos(Rand, Terrain, out float x, out float z);
                float y = Terrain.GetHeight(x, z);
                InitialEntities.Add(new LivingEntity(new Vector3(x, y, z), Rand, true));
            }
            for (int i = 0; i < FoodEntity.MAX_FOOD; i++)
            {
                Simulation.RandomPos(Rand, Terrain, out float x, out float z);
                float y = Terrain.GetHeight(x, z);
                InitialEntities.Add(new FoodEntity(new Vector3(x, y, z), Rand));
            }
        }
        private void Finish(GL gl)
        {
            _ = new Simulation(gl, this); // Callbacks set in constructor
        }

        private void CB_Create(GL gl, float delta)
        {
            StarNestShader.Instance.Render(gl, delta);

            switch (_state)
            {
                case State.TerrainGen_Init:
                {
                    TerrainGen_Init();
                    _state = State.TerrainGen_Wait;
                    break;
                }
                case State.TerrainGen_Wait:
                {
                    if (TerrainGen_Wait(gl))
                    {
                        _state = State.WaterGen_Init;
                    }
                    break;
                }
                case State.WaterGen_Init:
                {
                    WaterGen_Init();
                    _state = State.WaterGen_Wait;
                    break;
                }
                case State.WaterGen_Wait:
                {
                    if (WaterGen_Wait(gl))
                    {
                        _state = State.SpawnInitialPopulation;
                    }
                    break;
                }
                case State.SpawnInitialPopulation:
                {
                    SpawnInitialPopulation();
                    _state = State.Finish;
                    break;
                }
                case State.Finish:
                {
                    Finish(gl);
                    break;
                }
            }
        }
        private void QCB_Quit(GL gl)
        {
            _terrainGen?.Cancel();
            Terrain?.Delete(gl);
            _waterGen?.Cancel();
            Water?.Delete(gl);
        }
    }
}
