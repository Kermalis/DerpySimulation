using DerpySimulation.Input;
using DerpySimulation.Render;
using DerpySimulation.Render.Meshes;
using DerpySimulation.Render.Renderers;
using DerpySimulation.World;
using SDL2;
using Silk.NET.OpenGL;
using System;
#if DEBUG
using DerpySimulation.Debug;
#endif

namespace DerpySimulation.Core
{
    internal static class ProgramMain
    {
        public delegate void MainCallbackDelegate(GL gl, float delta);
        public delegate void QuitCallbackDelegate(GL gl);

        public static MainCallbackDelegate Callback = null!; // Initialized in Init()
        public static QuitCallbackDelegate QuitCallback = null!; // Initialized in Init()

        // Initializes the first callback, the window, and instances
        private static void Init()
        {
            Display.Init();

            GL gl = Display.OpenGL;
            _ = new LightController(); // Init instance of LightController
            _ = new FoodRenderer(gl); // Init instance of FoodRenderer

            // Initial callback for now is already the simulation
            Simulation.Debug_CreateSimulation(gl);
        }
        // Entry point of the program and main loop
        private static void Main()
        {
            Init();

            // Main loop
            DateTime time = DateTime.Now;
            {
                // Clear per-frame button states
                Keyboard.Prepare();
                Mouse.Prepare();

                // Grab all OS events
                if (HandleOSEvents())
                {
                    break; // Break if quit was requested by OS
                }

                // Calculate delta time
                DateTime now = DateTime.Now;
                DateTime prev = time;
                time = now;
                float deltaTime;
                if (now <= prev)
                {
#if DEBUG
                    Log.WriteLineWithTime("Time went back!");
#endif
                    deltaTime = 0;
                    continue; // Skip current frame if time went back
                }
                else
                {
                    deltaTime = (float)(now - prev).TotalSeconds;
                }

                // Run frame
                Display.PrepareFrame();
                Callback(Display.OpenGL, deltaTime);
                Display.PresentFrame();
            }

            // Quitting
            Quit();
        }
        // Handles freeing resources once the program is closing
        private static void Quit()
        {
            GL gl = Display.OpenGL;
            QuitCallback(gl);

            FoodRenderer.Instance.Delete(gl);

            AssimpLoader.Quit();
            Display.Quit();
        }

        private static bool HandleOSEvents()
        {
            while (SDL.SDL_PollEvent(out SDL.SDL_Event e) != 0)
            {
                switch (e.type)
                {
                    case SDL.SDL_EventType.SDL_QUIT: return true;
                    case SDL.SDL_EventType.SDL_KEYUP: Keyboard.OnKeyDown(e, false); break;
                    case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN: Mouse.OnButtonDown(e, true); break;
                    case SDL.SDL_EventType.SDL_MOUSEBUTTONUP: Mouse.OnButtonDown(e, false); break;
                    case SDL.SDL_EventType.SDL_MOUSEWHEEL: Mouse.OnScroll(e); break;
                    case SDL.SDL_EventType.SDL_MOUSEMOTION: Mouse.OnMove(e); break;
                    case SDL.SDL_EventType.SDL_KEYDOWN:
                    {
                        // Don't accept repeat events
                        if (e.key.repeat == 0)
                        {
                            Keyboard.OnKeyDown(e, true);
                        }
                        break;
                    }
                    case SDL.SDL_EventType.SDL_WINDOWEVENT:
                    {
                        SDL.SDL_WindowEventID ev = e.window.windowEvent;
                        switch (ev)
                        {
                            case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED: Display.OnWindowResized(); break;
                        }
                        break;
                    }
                }
            }
            return false;
        }
    }
}
