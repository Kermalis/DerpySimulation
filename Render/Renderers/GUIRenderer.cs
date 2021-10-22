using DerpySimulation.Core;
using DerpySimulation.Render.GUIs;
using Silk.NET.OpenGL;
using System;

namespace DerpySimulation.Render.Renderers
{
    internal sealed class GUIRenderer
    {
        public static GUIRenderer Instance { get; private set; } = null!; // Initialized in RenderManager

        public GUIComponent? TopComponent;

        public GUIRenderer()
        {
            Instance = this;
            Display.Resized += OnResized;
        }

        private void OnResized()
        {
            TopComponent?.OnDisplayResized();
        }

        public void SetTopComponentCallbacks()
        {
            if (TopComponent is null)
            {
                throw new NullReferenceException("No GUI is set to render");
            }
            ProgramMain.SetCallbacks(CB_UpdateAndRenderTopComponent, QCB_DeleteTopComponent);
        }
        private void CB_UpdateAndRenderTopComponent(GL gl, float delta)
        {
            TopComponent!.UpdateAllVisible(gl, delta);

            gl.Disable(EnableCap.DepthTest);
            gl.Disable(EnableCap.CullFace);
            gl.Disable(EnableCap.Multisample);
            gl.ClearColor(0f, 0f, 0f, 1f); // Black
            gl.Clear(ClearBufferMask.ColorBufferBit);
            gl.Enable(EnableCap.Blend);
            gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            foreach (IGUIVisual v in TopComponent.GetVisualsCheckVisible())
            {
                v.Render(gl, delta);
            }

            gl.Disable(EnableCap.Blend);
        }
        private void QCB_DeleteTopComponent(GL gl)
        {
            TopComponent!.DeleteAll(gl);
        }
    }
}
