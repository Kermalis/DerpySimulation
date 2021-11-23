using DerpySimulation.Render.Data;
using DerpySimulation.Render.Meshes;
using DerpySimulation.World.Terrain;
using Silk.NET.OpenGL;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Threading;
#if DEBUG
using DerpySimulation.Debug;
#endif

namespace DerpySimulation.World.Water
{
    /// <summary>Creates a flat mesh of water. Indicators are used to know which other vertices the current vertex is paired with to make a triangle.</summary>
    internal sealed class WaterGenerator
    {
        private readonly SimulationCreationSettings _settings;
        private readonly TerrainTile _terrain;
        private readonly Thread _thread;
        private readonly CancellationTokenSource _cts;

        private bool _done;
        private VBOData_Water[] _vertices = null!;
        private uint _numVertices;

        public WaterGenerator(in SimulationCreationSettings settings, TerrainTile terrain)
        {
            _settings = settings;
            _terrain = terrain;
            _thread = new Thread(Generate)
            {
                IsBackground = true,
                Name = "Water Generator"
            };
            _thread.Start();
            _cts = new CancellationTokenSource();
        }

        /// <summary>Checks if the mesh creation thread is done each frame.</summary>
        public bool IsDone(GL gl, [NotNullWhen(true)] out WaterTile? water)
        {
            if (_done)
            {
                water = new WaterTile(CreateMesh(gl), _settings.WaterLevel);
                return true;
            }
            water = null;
            return false;
        }
        private unsafe Mesh CreateMesh(GL gl)
        {
            // Create vao
            uint vao = gl.CreateVertexArray();
            gl.BindVertexArray(vao);

            // Create vbo
            uint vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            fixed (void* data = _vertices)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, VBOData_Water.SizeOf * _numVertices, data, BufferUsageARB.StaticDraw);
            }

            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, VBOData_Water.SizeOf, (void*)VBOData_Water.OffsetOfPos);
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Byte, false, VBOData_Water.SizeOf, (void*)VBOData_Water.OffsetOfPartnerVertex1);
            gl.EnableVertexAttribArray(2);
            gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Byte, false, VBOData_Water.SizeOf, (void*)VBOData_Water.OffsetOfPartnerVertex2);

            return new Mesh(vao, _numVertices, true, vbo);
        }

        /// <summary>Creates the mesh on a separate thread.</summary>
        private void Generate()
        {
            try
            {
                _vertices = new VBOData_Water[6 * _settings.SizeX * _settings.SizeZ];
                _numVertices = 0;
                for (int z = 0; z < _settings.SizeZ; z++)
                {
                    for (int x = 0; x < _settings.SizeX; x++)
                    {
                        _cts.Token.ThrowIfCancellationRequested();
                        StoreGridSquare(x, z);
                    }
                }
                _done = true;
            }
            catch (OperationCanceledException)
            {
#if DEBUG
                Log.SetIndent(0);
                Log.WriteLineWithTime("Water generation cancelled!");
#endif
            }
            _cts.Dispose();
        }

        /// <summary>Stores the two triangles of a square of water.</summary>
        private void StoreGridSquare(int x, int z)
        {
            Vector2[] cornerPos = CalculateCornerPositions(x, z);
            StoreTriangle(cornerPos, true);
            StoreTriangle(cornerPos, false);
        }
        private void StoreTriangle(Vector2[] cornerPos, bool left)
        {
            int i0 = left ? 0 : 2;
            int i1 = 1;
            int i2 = left ? 2 : 3;
            // Check if all 3 vertices are under the terrain, and if so, don't place them
            Vector2 v0 = cornerPos[i0];
            Vector2 v1 = cornerPos[i1];
            Vector2 v2 = cornerPos[i2];
            float water = _settings.WaterLevel;
            if (_terrain.GetHeight(v0.X, v0.Y, outOfBoundsResult: water + 1) > water
                && _terrain.GetHeight(v1.X, v1.Y, outOfBoundsResult: water + 1) > water
                && _terrain.GetHeight(v2.X, v2.Y, outOfBoundsResult: water + 1) > water)
            {
                return; // Catch the edge of the terrain on the two positive sides
            }
            StoreVertex(v0, i0, cornerPos, i1, i2);
            StoreVertex(v1, i1, cornerPos, i2, i0);
            StoreVertex(v2, i2, cornerPos, i0, i1);
        }
        private void StoreVertex(Vector2 cornerPos, int currentVertex, Vector2[] vertexPositions, int vertex1, int vertex2)
        {
            GetIndicators(currentVertex, vertexPositions, vertex1, vertex2, out Vector2 offset1, out Vector2 offset2);
            _vertices[_numVertices++] = new VBOData_Water(cornerPos, offset1, offset2);
        }

        private static Vector2[] CalculateCornerPositions(int x, int z)
        {
            var vertices = new Vector2[4];
            vertices[0] = new Vector2(x, z);
            vertices[1] = new Vector2(x, z + 1);
            vertices[2] = new Vector2(x + 1, z);
            vertices[3] = new Vector2(x + 1, z + 1);
            return vertices;
        }
        private static void GetIndicators(int currentVertex, Vector2[] vertexPositions, int vertex1, int vertex2, out Vector2 offset1, out Vector2 offset2)
        {
            Vector2 currentVertexPos = vertexPositions[currentVertex];
            Vector2 vertex1Pos = vertexPositions[vertex1];
            Vector2 vertex2Pos = vertexPositions[vertex2];
            offset1 = vertex1Pos - currentVertexPos;
            offset2 = vertex2Pos - currentVertexPos;
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
