using DerpySimulation.Input;
using DerpySimulation.Render;
using DerpySimulation.Render.GUIs.Menus;
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

        public static MainCallbackDelegate Callback { get; private set; } = null!; // Initialized in Init()
        public static QuitCallbackDelegate QuitCallback { get; private set; } = null!; // Initialized in Init()

        public static bool QuitRequested;

        // Initializes the first callback, the window, and instances
        private static void Init()
        {
            Display.Init();
            GL gl = Display.OpenGL; // Get gl after display is created

            RenderManager.Init(gl); // Init shader/renderer/mesh instances and their resize callbacks

            // Initial callback
            _ = new TestMenu(gl);
        }
        // Entry point of the program and main loop
        private static void Main()
        {
            Init();

            // Main loop
            DateTime time = DateTime.Now;
            while (!QuitRequested) // Break if quit was requested by program
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
                    if (deltaTime > 1f)
                    {
                        deltaTime = 1f;
#if DEBUG
                        Log.WriteLineWithTime("Time between frames was longer than 1 second!");
#endif
                    }
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

            RenderManager.Quit(gl);

            Display.Quit();
        }

        public static void SetCallbacks(MainCallbackDelegate main, QuitCallbackDelegate quit)
        {
            Callback = main;
            QuitCallback = quit;
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
