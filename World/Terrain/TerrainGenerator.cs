using DerpySimulation.Core;
using DerpySimulation.Render.Data;
using DerpySimulation.Render.Meshes;
using Silk.NET.OpenGL;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Threading;
#if DEBUG
using DerpySimulation.Debug;
#endif

namespace DerpySimulation.World.Terrain
{
    internal sealed class TerrainGenerator
    {
        private readonly SimulationCreationSettings _settings;
        private readonly Thread _thread;
        private readonly CancellationTokenSource _cts;

        private bool _done;
        public Vector3 Peak;
        private float[,] _gridHeights = null!;
        private Vector3[,] _gridColors = null!;
        private VBOData_PosNormalColor[] _vertices = null!;
        private uint _numVertices;
        private uint[] _indices = null!;

        public TerrainGenerator(in SimulationCreationSettings settings)
        {
            _settings = settings;
            _thread = new Thread(Generate)
            {
                IsBackground = true,
                Name = "Terrain Generator"
            };
            _thread.Start();
            _cts = new CancellationTokenSource();
        }

        private void Generate()
        {
            try
            {
                GenerateHeights();
                _cts.Token.ThrowIfCancellationRequested();
                GenerateColors();
                _cts.Token.ThrowIfCancellationRequested();
                GenerateMesh();
#if DEBUG
                Log.WriteLineWithTime("Done generating! The peak is at " + Peak);
#endif
                _done = true;
            }
            catch (OperationCanceledException)
            {
#if DEBUG
                Log.SetIndent(0);
                Log.WriteLineWithTime("Terrain generation cancelled!");
#endif
            }
            _cts.Dispose();
        }
        private void GenerateHeights()
        {
            int seed = _settings.HeightGenSeed ?? (int)LehmerRand.CreateRandomSeed();
#if DEBUG
            Log.WriteLineWithTime("Generating terrain heights...");
            Log.ModifyIndent(+1);
            Log.WriteLineWithTime("World generation seed: " + seed.ToString("X8"));
            Log.ModifyIndent(-1);
#endif
            _gridHeights = TerrainHeightGenerator.GenerateArea(_cts,
                _settings.SizeX, _settings.SizeZ, seed, _settings.HeightGenAmplitude, _settings.HeightGenNumOctaves, _settings.HeightGenRoughness,
                out Peak);
        }
        private void GenerateColors()
        {
#if DEBUG
            Log.WriteLineWithTime("Generating terrain colors...");
#endif
            _gridColors = TerrainColorGenerator.Generate(_cts, _settings.ColorSteps, _gridHeights);
        }
        private void GenerateMesh()
        {
#if DEBUG
            Log.WriteLineWithTime("Generating terrain mesh...");
#endif
            _vertices = TerrainMeshGenerator.Generate(_cts, _settings.SizeX, _settings.SizeZ, _gridHeights, _gridColors, out _numVertices);
            _gridColors = null!; // Don't need anymore. Need heights for terrain tile though
            _cts.Token.ThrowIfCancellationRequested();
            _indices = TerrainIndexGenerator.Generate(_cts, _settings.SizeX, _settings.SizeZ);
        }

        /// <summary>Checks if the mesh creation thread is done each frame.</summary>
        public bool IsDone(GL gl, [NotNullWhen(true)] out TerrainTile? terrain)
        {
            if (_done)
            {
                terrain = new TerrainTile(CreateMesh(gl), _gridHeights);
                return true;
            }
            terrain = null;
            return false;
        }
        private unsafe Mesh CreateMesh(GL gl)
        {
            uint elementCount = (uint)_indices.Length;

            // Create vao
            uint vao = gl.CreateVertexArray();
            gl.BindVertexArray(vao);

            // Create ebo
            uint ebo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
            fixed (void* data = _indices)
            {
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, sizeof(uint) * elementCount, data, BufferUsageARB.StaticDraw);
            }

            // Create vbo
            uint vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            fixed (void* data = _vertices)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, VBOData_PosNormalColor.SizeOf * _numVertices, data, BufferUsageARB.StaticDraw);
            }

            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, VBOData_PosNormalColor.SizeOf, (void*)VBOData_PosNormalColor.OffsetOfPos);
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, VBOData_PosNormalColor.SizeOf, (void*)VBOData_PosNormalColor.OffsetOfNormal);
            gl.EnableVertexAttribArray(2);
            gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, VBOData_PosNormalColor.SizeOf, (void*)VBOData_PosNormalColor.OffsetOfColor);

            return new Mesh(vao, elementCount, false, ebo, vbo);
        }

        public void Cancel()
        {
            if (!_done)
            {
                _cts.Cancel();
            }
        }
    }
}
