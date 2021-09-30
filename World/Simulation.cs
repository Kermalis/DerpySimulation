#if DEBUG
using DerpySimulation.Debug;
#endif
#if DEBUG_TEST_SUN
using System;
#endif
using DerpySimulation.Core;
using DerpySimulation.Render;
using Silk.NET.OpenGL;
using System.Numerics;

namespace DerpySimulation.World
{
    internal sealed class Simulation
    {
        private readonly TerrainShader _terrainShader;

        private readonly Camera _camera;
        private readonly Terrain _terrain;

#if DEBUG_TEST_SUN
        private int _tempTime;
        private const float TempTimeLimit = 2000f;
#endif

        public Simulation(GL gl)
        {
            // Create terrain
            //const float amplitude = 100f;
            //const float waterLevel = 0f;
            var heightGen = new HeightGenerator(100f, 7, 0.5f);
            var colors = new ColorStep[]
            {
                new(-100, new Vector3( 50/255f,  50/255f, 130/255f)), // Deep ocean
                new(- 80, new Vector3( 50/255f,  50/255f, 180/255f)), // Ocean
                new(- 10, new Vector3(100/255f, 100/255f, 110/255f)), // Gravelly ocean
                new(   0, new Vector3(180/255f, 175/255f, 120/255f)), // Sandy
                new(   2, new Vector3( 80/255f, 170/255f, 120/255f)), // Grass
                new(  40, new Vector3( 80/255f, 190/255f, 120/255f)), // Grass brighter
                new(  50, new Vector3(100/255f, 100/255f, 100/255f)), // Grayish mountain
                new(  70, new Vector3(120/255f, 120/255f, 120/255f)), // Brighter gray
                new(  85, new Vector3(210/255f, 200/255f, 190/255f)), // Redish peak (almost white)
                new( 100, new Vector3(205/255f, 235/255f, 255/255f)), // White peaks
            };
            _terrain = TerrainGenerator.GenerateTerrain(gl, heightGen, colors, Terrain.SIZE, out Vector3 peak);

            // Test add some lights
            LightController.Instance.Add(new(new Vector3(Terrain.SIZE - (Terrain.SIZE / 4), 90, Terrain.SIZE - (Terrain.SIZE / 4)), new Vector3(10, 134f / 255, 5), new Vector3(1f, 0.01f, 0.002f)));
            LightController.Instance.Add(new(new Vector3(Terrain.SIZE - (Terrain.SIZE / 4), 90, Terrain.SIZE / 4), new Vector3(218f / 255, 134f / 255, 226f / 255), new Vector3(1f, 0.01f, 0.002f)));
            LightController.Instance.Add(new(new Vector3(Terrain.SIZE / 4, 90, Terrain.SIZE / 4), new Vector3(218f / 255, 134f / 255, 226f / 255), new Vector3(1f, 0.01f, 0.002f)));

            // Create camera
#if DEBUG
            Log.WriteLineWithTime("Done generating! The peak is at " + peak);
#endif
            peak.Y += 5;
            var pr = new PositionRotation(peak, 0f, 30f, 0f);
            _camera = new Camera(pr);

            // Create shader
            _terrainShader = new(ProgramMain.OpenGL);
        }

        public void LogicTick()
        {
            _camera.PR.Debug_Move(50f);
            //System.Console.WriteLine(_camera.PR);
        }

        public void Render(GL gl)
        {
            gl.Enable(EnableCap.DepthTest);
            gl.Enable(EnableCap.CullFace);
            gl.CullFace(CullFaceMode.Back);
            gl.Enable(EnableCap.Multisample);
            gl.ProvokingVertex(VertexProvokingMode.FirstVertexConvention);
            gl.ClearColor(0.49f, 0.89f, 0.98f, 1f); // Sky color
            gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
#if DEBUG_WIREFRAME
            gl.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
#endif

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

            // Terrain
            _terrainShader.Use(gl);

            _terrainShader.SetCamera(gl, _camera);
            _terrainShader.SetLights(gl, LightController.Instance);

            _terrain.Render(gl);

            gl.UseProgram(0);

#if DEBUG_WIREFRAME
            gl.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill); // Reset
#endif
        }
    }
}
