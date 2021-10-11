using DerpySimulation.Render.GUIs.Fonts;
using DerpySimulation.Render.Meshes;
using DerpySimulation.Render.Renderers;
using DerpySimulation.Render.Shaders;
using Silk.NET.OpenGL;

namespace DerpySimulation.Render
{
    internal static class RenderManager
    {
        public static void Init(GL gl)
        {
            // Init shader instances
            _ = new FontShader(gl);
            _ = new GUIShader(gl);

            // Init other instances
            _ = new SimpleRectMesh(gl);
            _ = new GUIRenderer();
            Font.Init(gl);
            _ = new FoodRenderer(gl);
            _ = new LightController();

            // Set screen size
            FontShader.Instance.Static_SetScreenSize(gl);

            // Set resize callback
            Display.Resized += Display_Resized;
        }

        private static void Display_Resized()
        {
            GL gl = Display.OpenGL;
            FontShader.Instance.Static_SetScreenSize(gl);
        }

        public static void Quit(GL gl)
        {
            FontShader.Instance.Delete(gl);
            GUIShader.Instance.Delete(gl);

            SimpleRectMesh.Instance.Delete(gl);
            Font.Instance.Delete(gl);
            FoodRenderer.Instance.Delete(gl);

            AssimpLoader.Quit();
        }
    }
}
