using DerpySimulation.Entities;
using DerpySimulation.Render.Data;
using DerpySimulation.Render.Meshes;
using DerpySimulation.Render.Shaders;
using Silk.NET.OpenGL;
using System.Collections.Generic;
using System.Numerics;

namespace DerpySimulation.Render.Renderers
{
    internal sealed class FoodRenderer
    {
        private const string MODEL_PATH = @"Models\Food.obj";

        public static FoodRenderer Instance { get; private set; } = null!; // Initialized in ProgramMain

        private readonly FoodShader _shader;
        private readonly Model _model;
        private readonly uint _instanceVBO;

        public unsafe FoodRenderer(GL gl)
        {
            Instance = this;

            _shader = new FoodShader(gl);
            _model = AssimpLoader.ImportModel(gl, MODEL_PATH);

            // Add instancing stuff to each mesh
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

            gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            gl.BindVertexArray(0);
        }

        public unsafe void UpdateVisuals(GL gl, float delta, List<FoodEntity> food)
        {
            FoodEntity.UpdateSpin(delta);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, _instanceVBO);
            for (int i = 0; i < food.Count; i++)
            {
                FoodEntity f = food[i];
                f.UpdateVisual(delta);
                var vboData = new VBOData_InstancedFood(f.Color, f.UpdatedTransform);
                gl.BufferSubData(BufferTargetARB.ArrayBuffer, i * (int)VBOData_InstancedFood.SizeOf, VBOData_InstancedFood.SizeOf, &vboData);
            }
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        }
        public void Render(GL gl, List<FoodEntity> food, in Matrix4x4 projectionView, in Vector3 camPos, in Vector4 clippingPlane)
        {
            _shader.Use(gl);

            // Global uniforms
            _shader.SetCamera(gl, projectionView, camPos);
            _shader.SetClippingPlane(gl, clippingPlane);
            _shader.SetLights(gl);

            _model.RenderInstanced(gl, (uint)food.Count);

            gl.UseProgram(0);
        }

        public void Delete(GL gl)
        {
            _shader.Delete(gl);
            gl.DeleteBuffer(_instanceVBO);
            _model.Delete(gl);
        }
    }
}
