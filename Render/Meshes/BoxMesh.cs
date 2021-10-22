using DerpySimulation.Render.Data;
using Silk.NET.OpenGL;
using System.Numerics;

namespace DerpySimulation.Render.Meshes
{
    internal static class BoxMesh
    {
        private static readonly Vector3[] _vertices = new Vector3[24]
        {
            new( .5f, .5f, .5f),  new(-.5f, .5f, .5f),  new(-.5f,-.5f, .5f), new( .5f,-.5f, .5f), // v0,v1,v2,v3 (South)
            new( .5f, .5f, .5f),  new( .5f,-.5f, .5f),  new( .5f,-.5f,-.5f), new( .5f, .5f,-.5f), // v0,v3,v4,v5 (Right)
            new( .5f, .5f, .5f),  new( .5f, .5f,-.5f),  new(-.5f, .5f,-.5f), new(-.5f, .5f, .5f), // v0,v5,v6,v1 (Up)
            new(-.5f, .5f, .5f),  new(-.5f, .5f,-.5f),  new(-.5f,-.5f,-.5f), new(-.5f,-.5f, .5f), // v1,v6,v7,v2 (West)
            new(-.5f,-.5f,-.5f),  new( .5f,-.5f,-.5f),  new( .5f,-.5f, .5f), new(-.5f,-.5f, .5f), // v7,v4,v3,v2 (Down)
            new( .5f,-.5f,-.5f),  new(-.5f,-.5f,-.5f),  new(-.5f, .5f,-.5f), new( .5f, .5f,-.5f)  // v4,v7,v6,v5 (North)
        };
        private static readonly Vector3[] _normals = new Vector3[6]
        {
            new( 0, 0, 1), // v0,v1,v2,v3 (South)
            new( 1, 0, 0), // v0,v3,v4,v5 (Right)
            new( 0, 1, 0), // v0,v5,v6,v1 (Up)
            new(-1, 0, 0), // v1,v6,v7,v2 (West)
            new( 0,-1, 0), // v7,v4,v3,v2 (Down)
            new( 0, 0,-1)  // v4,v7,v6,v5 (North)
        };
        private static readonly uint[] _indices = new uint[36]
        {
             0, 1, 2,   2, 3, 0, // v0-v1-v2, v2-v3-v0 (South)
             4, 5, 6,   6, 7, 4, // v0-v3-v4, v4-v5-v0 (Right)
             8, 9,10,  10,11, 8, // v0-v5-v6, v6-v1-v0 (Up)
            12,13,14,  14,15,12, // v1-v6-v7, v7-v2-v1 (West)
            16,17,18,  18,19,16, // v7-v4-v3, v3-v2-v7 (Down)
            20,21,22,  22,23,20  // v4-v7-v6, v6-v5-v4 (North)
        };

        public static unsafe Mesh Create(GL gl)
        {
            // Create vbo data
            var vs = new VBOData_PosNormal[24];
            for (int i = 0; i < 24; i++)
            {
                vs[i] = new VBOData_PosNormal(_vertices[i], _normals[i / 4]);
            }

            // Create vao
            uint vao = gl.CreateVertexArray();
            gl.BindVertexArray(vao);

            // Create ebo
            uint ebo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
            fixed (void* data = _indices)
            {
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, sizeof(uint) * 36, data, BufferUsageARB.StaticDraw);
            }

            // Create vbo
            uint vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            fixed (void* data = vs)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, VBOData_PosNormal.SizeOf * 24, data, BufferUsageARB.StaticDraw);
            }

            // vbo attributes
            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, VBOData_PosNormal.SizeOf, (void*)VBOData_PosNormal.OffsetOfPos);
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, VBOData_PosNormal.SizeOf, (void*)VBOData_PosNormal.OffsetOfNormal);

            return new Mesh(vao, 36, false, ebo, vbo);
        }
    }
}
