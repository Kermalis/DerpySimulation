using DerpySimulation.Render.GUIs.Positioning;
using DerpySimulation.Render.Meshes;
using DerpySimulation.Render.Shaders;
using Silk.NET.OpenGL;
using System.Numerics;

namespace DerpySimulation.Render.GUIs
{
    internal sealed class GUIBlock : GUIComponent, IGUIVisual
    {
        private readonly Vector4 _color;

        public GUIBlock(in Vector4 color)
        {
            _color = color;
        }

        protected override void Init(GL gl)
        {
            //
        }
        protected override void Update(GL gl, float delta)
        {
            //
        }

        public void Render(GL gl, float delta)
        {
            GUIShader shader = GUIShader.Instance;
            shader.Use(gl);
            ref GUIRect pos = ref RelPos;
            gl.Uniform2(shader.LRelPos, pos.X, pos.Y);
            gl.Uniform2(shader.LRelSize, pos.W, pos.H);
            Colors.PutInShader(gl, shader.LColor, _color);

            SimpleRectMesh.Instance.Render(gl);
        }
    }
}
