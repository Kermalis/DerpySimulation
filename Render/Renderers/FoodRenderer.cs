using DerpySimulation.Entities;
using DerpySimulation.Render.Data;
using DerpySimulation.Render.Meshes;
using DerpySimulation.Render.Shaders;
using Silk.NET.OpenGL;
using System;
using System.Numerics;

namespace DerpySimulation.Render.Renderers
{
    internal sealed class FoodRenderer
    {
        private const string MODEL_PATH = @"Models\Food.obj";

        public static FoodRenderer Instance { get; private set; } = null!; // Initialized in Init()

        private readonly FoodShader _shader;
        private readonly Model _model;
        private readonly uint _instanceVBO;

        private uint _numToRender;

        private unsafe FoodRenderer(GL gl)
        {
            _shader = new FoodShader(gl);
            _model = AssimpLoader.ImportModel(gl, MODEL_PATH);

            // Add instancing stuff to each mesh vao
            _instanceVBO = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, _instanceVBO);
            gl.BufferData(BufferTargetARB.ArrayBuffer, VBOData_InstancedFood.SizeOf * FoodEntity.MAX_FOOD, null, BufferUsageARB.StreamDraw); // Create empty vbo
            for (int i = 0; i < _model.Count; i++)
            {
                Mesh m = _model[i];
                gl.BindVertexArray(m.VAO);

                // Color
                RenderUtils.AddInstancedAttribute(gl, 2, 3, VBOData_InstancedFood.SizeOf, VBOData_InstancedFood.OffsetOfColor);
                // Transform
                RenderUtils.AddInstancedAttribute_Matrix4x4(gl, 3, VBOData_InstancedFood.SizeOf, VBOData_InstancedFood.OffsetOfTransform);
            }
        }
        public static void Init(GL gl)
        {
            if (Instance is not null)
            {
                throw new InvalidOperationException();
            }
            Instance = new FoodRenderer(gl);
        }

        public void NewFrame(float delta)
        {
            _numToRender = 0;
            FoodEntity.UpdateSpin(delta);
        }
        public unsafe void Add(GL gl, in Vector3 color, in Matrix4x4 transform)
        {
            int i = (int)_numToRender;
            if (i >= FoodEntity.MAX_FOOD)
            {
                return;
            }
            _numToRender++;
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, _instanceVBO);
            var vboData = new VBOData_InstancedFood(color, transform);
            gl.BufferSubData(BufferTargetARB.ArrayBuffer, i * (int)VBOData_InstancedFood.SizeOf, VBOData_InstancedFood.SizeOf, &vboData);
        }
        public void Render(GL gl, in Matrix4x4 projectionView, in Vector3 camPos, in Vector4 clippingPlane)
        {
            _shader.Use(gl);

            // Global uniforms
            _shader.SetCamera(gl, projectionView, camPos);
            _shader.SetClippingPlane(gl, clippingPlane);
            _shader.SetLights(gl);

            _model.RenderInstanced(gl, _numToRender);
        }

        public void Delete(GL gl)
        {
            Instance = null!;
            _shader.Delete(gl);
            gl.DeleteBuffer(_instanceVBO);
            _model.Delete(gl);
        }
    }
}
