using DerpySimulation.Input;
using DerpySimulation.Render;
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
        private static DateTime _renderTickTime;

        public delegate void MainCallback(GL gl, float delta);
        public static MainCallback Callback;

        private static void Init()
        {
            _renderTickTime = DateTime.Now;
            Display.Init();
            _ = new LightController(); // Init instance of LightController

            // Initial callback for now is already the simulation
            Callback = Simulation.CB_Debug_CreateSimulation;
        }
        private static void Main()
        {
            Init();

            // Main loop
            while (true)
            {
                // Clear per-frame button states
                Keyboard.Prepare();
                Mouse.Prepare();

                // Grab all OS events
                if (HandleEvents())
                {
                    break; // Break if quit was requested
                }

                // Calculate delta time
                DateTime now = DateTime.Now;
                DateTime prev = _renderTickTime;
                _renderTickTime = now;
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
        private static void Quit()
        {
            Display.Quit();
        }

        private static bool HandleEvents()
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
                            case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED: Display.OnResized(); break;
                        }
                        break;
                    }
                }
            }
            return false;
        }
    }
}
