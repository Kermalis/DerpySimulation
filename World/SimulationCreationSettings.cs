using DerpySimulation.World.Terrain;
using System.Numerics;

namespace DerpySimulation.World
{
    internal struct SimulationCreationSettings
    {
        public float WaterLevel;
        public uint SizeX;
        public uint SizeZ;
        public ColorStep[] Colors;

        public float HeightGenAmplitude;
        public int HeightGenNumOctaves;
        public float HeightGenRoughness;
        public int? HeightGenSeed;

        public static SimulationCreationSettings CreatePreset(int num)
        {
            switch (num)
            {
                case 0: // Suitable for smaller worlds
                {
                    SimulationCreationSettings settings;

                    settings.WaterLevel = 0f;
                    settings.SizeX = 1000;
                    settings.SizeZ = 1000;

                    settings.HeightGenAmplitude = 100f;
                    settings.HeightGenNumOctaves = 7;
                    settings.HeightGenRoughness = 0.5f;
                    settings.HeightGenSeed = null;

                    settings.Colors = new ColorStep[]
                    {
                        new(-100, new Vector3( 50/255f,  50/255f, 130/255f)), // Deep ocean
                        new(- 80, new Vector3( 50/255f,  50/255f, 180/255f)), // Ocean
                        new(- 10, new Vector3(100/255f, 100/255f, 110/255f)), // Gravelly ocean
                        new(   0, new Vector3(180/255f, 175/255f, 120/255f)), // Sandy
                        new(   2, new Vector3( 80/255f, 170/255f, 120/255f)), // Grass
                        new(  40, new Vector3( 80/255f, 190/255f, 120/255f)), // Grass brighter
                        new(  50, new Vector3(100/255f, 100/255f, 100/255f)), // Grayish mountain
                        new(  70, new Vector3(120/255f, 120/255f, 120/255f)), // Brighter gray
                        new(  85, new Vector3(220/255f, 210/255f, 200/255f)), // Redish peak (almost white)
                        new(  90, new Vector3(205/255f, 235/255f, 255/255f)), // White peaks
                    };

                    return settings;
                }
                case 1: // Suitable for massive worlds
                {
                    SimulationCreationSettings settings;

                    settings.WaterLevel = 0f;
                    settings.SizeX = 1000;
                    settings.SizeZ = 1000;

                    settings.HeightGenAmplitude = 500f;
                    settings.HeightGenNumOctaves = 9;
                    settings.HeightGenRoughness = 0.5f;
                    settings.HeightGenSeed = null;

                    settings.Colors = new ColorStep[]
                    {
                        new(-500, new Vector3( 50/255f,  50/255f, 130/255f)), // Deep ocean
                        new(-420, new Vector3( 50/255f,  50/255f, 180/255f)), // Ocean
                        new(- 50, new Vector3(100/255f, 100/255f, 110/255f)), // Gravelly ocean
                        new(   0, new Vector3(180/255f, 175/255f, 120/255f)), // Sandy
                        new(  25, new Vector3( 80/255f, 170/255f, 120/255f)), // Grass
                        new( 140, new Vector3( 80/255f, 190/255f, 120/255f)), // Grass brighter
                        new( 190, new Vector3(100/255f, 100/255f, 100/255f)), // Grayish mountain
                        new( 250, new Vector3(120/255f, 120/255f, 120/255f)), // Brighter gray
                        new( 285, new Vector3(220/255f, 210/255f, 200/255f)), // Redish peak (almost white)
                        new( 300, new Vector3(205/255f, 235/255f, 255/255f)), // White peaks
                    };

                    return settings;
                }
                default:// Suitable for medium size worlds
                {
                    SimulationCreationSettings settings;

                    settings.WaterLevel = 0f;
                    settings.SizeX = 1500;
                    settings.SizeZ = 1500;

                    settings.HeightGenAmplitude = 300f;
                    settings.HeightGenNumOctaves = 8;
                    settings.HeightGenRoughness = 0.425f;
                    settings.HeightGenSeed = null;

                    settings.Colors = new ColorStep[]
                    {
                        new(-300, new Vector3( 50/255f,  50/255f, 130/255f)), // Deep ocean
                        new(-220, new Vector3( 50/255f,  50/255f, 180/255f)), // Ocean
                        new(- 50, new Vector3(100/255f, 100/255f, 110/255f)), // Gravelly ocean
                        new(   0, new Vector3(180/255f, 175/255f, 120/255f)), // Sandy
                        new(  15, new Vector3( 80/255f, 170/255f, 120/255f)), // Grass
                        new( 110, new Vector3( 80/255f, 190/255f, 120/255f)), // Grass brighter
                        new( 175, new Vector3(100/255f, 100/255f, 100/255f)), // Grayish mountain
                        new( 200, new Vector3(120/255f, 120/255f, 120/255f)), // Brighter gray
                        new( 225, new Vector3(220/255f, 210/255f, 200/255f)), // Redish peak (almost white)
                        new( 250, new Vector3(205/255f, 235/255f, 255/255f)), // White peaks
                    };

                    return settings;
                }
            }
        }
    }
}
