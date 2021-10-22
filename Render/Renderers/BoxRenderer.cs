using DerpySimulation.Render.Data;
using DerpySimulation.Render.Meshes;
using DerpySimulation.Render.Shaders;
using Silk.NET.OpenGL;
using System;
using System.Numerics;

namespace DerpySimulation.Render.Renderers
{
    internal sealed class BoxRenderer
    {
        private const uint MAX_BOXES = 50_000;

        public static BoxRenderer Instance { get; private set; } = null!; // Initialized in Init()

        private readonly BoxShader _shader;
        private readonly Mesh _mesh;
        private readonly uint _instanceVBO;

        private uint _numToRender;

        private unsafe BoxRenderer(GL gl)
        {
            _shader = new BoxShader(gl);
            _mesh = BoxMesh.Create(gl);

            // Add instancing stuff to the mesh vao
            _instanceVBO = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, _instanceVBO);
            gl.BufferData(BufferTargetARB.ArrayBuffer, VBOData_InstancedBox.SizeOf * MAX_BOXES, null, BufferUsageARB.StreamDraw); // Create empty vbo
            // Colors
            RenderUtils.AddInstancedAttribute(gl, 2, 3, VBOData_InstancedBox.SizeOf, VBOData_InstancedBox.OffsetOfColors + BoxColors.OffsetOfEast);
            RenderUtils.AddInstancedAttribute(gl, 3, 3, VBOData_InstancedBox.SizeOf, VBOData_InstancedBox.OffsetOfColors + BoxColors.OffsetOfWest);
            RenderUtils.AddInstancedAttribute(gl, 4, 3, VBOData_InstancedBox.SizeOf, VBOData_InstancedBox.OffsetOfColors + BoxColors.OffsetOfUp);
            RenderUtils.AddInstancedAttribute(gl, 5, 3, VBOData_InstancedBox.SizeOf, VBOData_InstancedBox.OffsetOfColors + BoxColors.OffsetOfDown);
            RenderUtils.AddInstancedAttribute(gl, 6, 3, VBOData_InstancedBox.SizeOf, VBOData_InstancedBox.OffsetOfColors + BoxColors.OffsetOfSouth);
            RenderUtils.AddInstancedAttribute(gl, 7, 3, VBOData_InstancedBox.SizeOf, VBOData_InstancedBox.OffsetOfColors + BoxColors.OffsetOfNorth);
            // Transform
            RenderUtils.AddInstancedAttribute_Matrix4x4(gl, 8, VBOData_InstancedBox.SizeOf, VBOData_InstancedBox.OffsetOfTransform);
        }
        public static void Init(GL gl)
        {
            if (Instance is not null)
            {
                throw new InvalidOperationException();
            }
            Instance = new BoxRenderer(gl);
        }

        public void NewFrame()
        {
            _numToRender = 0;
        }
        public unsafe void Add(GL gl, in BoxColors colors, in Matrix4x4 transform)
        {
            int i = (int)_numToRender;
            if (i >= MAX_BOXES)
            {
                return;
            }
            _numToRender++;
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, _instanceVBO);
            var vboData = new VBOData_InstancedBox(colors, transform);
            gl.BufferSubData(BufferTargetARB.ArrayBuffer, i * (int)VBOData_InstancedBox.SizeOf, VBOData_InstancedBox.SizeOf, &vboData);
        }
        public void Render(GL gl, in Matrix4x4 projectionView, in Vector3 camPos, in Vector4 clippingPlane)
        {
            _shader.Use(gl);

            // Global uniforms
            _shader.SetCamera(gl, projectionView, camPos);
            _shader.SetClippingPlane(gl, clippingPlane);
            _shader.SetLights(gl);

            _mesh.RenderInstanced(gl, _numToRender);
        }

        public void Delete(GL gl)
        {
            Instance = null!;
            _shader.Delete(gl);
            gl.DeleteBuffer(_instanceVBO);
            _mesh.Delete(gl);
        }
    }
}
