using DerpySimulation.Render;
using DerpySimulation.World.Terrain;

namespace DerpySimulation.World
{
    internal struct SimulationCreationSettings
    {
        public int InitialPopulation;
        public float WaterLevel;
        public uint SizeX;
        public uint SizeZ;
        public ColorStep[] ColorSteps;

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

                    settings.InitialPopulation = 25;
                    settings.WaterLevel = 0f;
                    settings.SizeX = 1000;
                    settings.SizeZ = 1000;

                    settings.HeightGenAmplitude = 100f;
                    settings.HeightGenNumOctaves = 7;
                    settings.HeightGenRoughness = 0.5f;
                    settings.HeightGenSeed = null;

                    settings.ColorSteps = new ColorStep[]
                    {
                        new(-100, Colors.FromRGB( 50,  50, 130)), // Deep ocean
                        new(- 80, Colors.FromRGB( 50,  50, 180)), // Ocean
                        new(- 10, Colors.FromRGB(100, 100, 110)), // Gravelly ocean
                        new(   0, Colors.FromRGB(180, 175, 120)), // Sandy
                        new(   2, Colors.FromRGB( 80, 170, 120)), // Grass
                        new(  40, Colors.FromRGB( 80, 190, 120)), // Grass brighter
                        new(  50, Colors.FromRGB(100, 100, 100)), // Grayish mountain
                        new(  70, Colors.FromRGB(120, 120, 120)), // Brighter gray
                        new(  85, Colors.FromRGB(220, 210, 200)), // Redish peak (almost white)
                        new(  90, Colors.FromRGB(205, 235, 255)), // White peaks
                    };

                    return settings;
                }
                case 1: // Suitable for massive worlds
                {
                    SimulationCreationSettings settings;

                    settings.InitialPopulation = 50;
                    settings.WaterLevel = 0f;
                    settings.SizeX = 1000;
                    settings.SizeZ = 1000;

                    settings.HeightGenAmplitude = 500f;
                    settings.HeightGenNumOctaves = 9;
                    settings.HeightGenRoughness = 0.5f;
                    settings.HeightGenSeed = null;

                    settings.ColorSteps = new ColorStep[]
                    {
                        new(-500, Colors.FromRGB( 50,  50, 130)), // Deep ocean
                        new(-420, Colors.FromRGB( 50,  50, 180)), // Ocean
                        new(- 50, Colors.FromRGB(100, 100, 110)), // Gravelly ocean
                        new(   0, Colors.FromRGB(180, 175, 120)), // Sandy
                        new(  25, Colors.FromRGB( 80, 170, 120)), // Grass
                        new( 140, Colors.FromRGB( 80, 190, 120)), // Grass brighter
                        new( 190, Colors.FromRGB(100, 100, 100)), // Grayish mountain
                        new( 250, Colors.FromRGB(120, 120, 120)), // Brighter gray
                        new( 285, Colors.FromRGB(220, 210, 200)), // Redish peak (almost white)
                        new( 300, Colors.FromRGB(205, 235, 255)), // White peaks
                    };

                    return settings;
                }
                default:// Suitable for medium size worlds
                {
                    SimulationCreationSettings settings;

                    settings.InitialPopulation = 100;
                    settings.WaterLevel = 0f;
                    settings.SizeX = 1500;
                    settings.SizeZ = 1500;

                    settings.HeightGenAmplitude = 300f;
                    settings.HeightGenNumOctaves = 8;
                    settings.HeightGenRoughness = 0.425f;
                    settings.HeightGenSeed = null;

                    settings.ColorSteps = new ColorStep[]
                    {
                        new(-300, Colors.FromRGB( 50,  50, 130)), // Deep ocean
                        new(-220, Colors.FromRGB( 50,  50, 180)), // Ocean
                        new(- 50, Colors.FromRGB(100, 100, 110)), // Gravelly ocean
                        new(   0, Colors.FromRGB(180, 175, 120)), // Sandy
                        new(  15, Colors.FromRGB( 80, 170, 120)), // Grass
                        new( 110, Colors.FromRGB( 80, 190, 120)), // Grass brighter
                        new( 175, Colors.FromRGB(100, 100, 100)), // Grayish mountain
                        new( 200, Colors.FromRGB(120, 120, 120)), // Brighter gray
                        new( 225, Colors.FromRGB(220, 210, 200)), // Redish peak (almost white)
                        new( 250, Colors.FromRGB(205, 235, 255)), // White peaks
                    };

                    return settings;
                }
            }
        }
    }
}
