#if DEBUG
using DerpySimulation.Debug;
using System.Runtime.InteropServices;
#endif
using SDL2;
using Silk.NET.OpenGL;
using System;
using DerpySimulation.World;
using DerpySimulation.Render;

namespace DerpySimulation.Core
{
    internal static class ProgramMain
    {
        public const int NumTicksPerSecond = 20;

        private const float NumMillisecondsPerTick = 1_000f / NumTicksPerSecond;

        public static DateTime LogicTickTime { get; private set; }
        public static DateTime RenderTickTime { get; private set; }
        public static TimeSpan RenderTimeSinceLastFrame { get; private set; }

        private static readonly IntPtr _window;
        private static readonly IntPtr _gl;
        public static readonly GL OpenGL;

        public delegate void ResizeEventHandler(uint w, uint h);
        public static event ResizeEventHandler Resized;
        public static uint CurrentWidth { get; private set; }
        public static uint CurrentHeight { get; private set; }

        private static Simulation _sim;

        static ProgramMain()
        {
            // SDL 2
            if (SDL.SDL_Init(SDL.SDL_INIT_AUDIO | SDL.SDL_INIT_VIDEO | SDL.SDL_INIT_GAMECONTROLLER) != 0)
            {
                Print_SDL_Error("SDL could not initialize!");
            }

            // Use OpenGL 3.3 core
            if (SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, 3) != 0)
            {
                Print_SDL_Error("Could not set OpenGL's major version!");
            }
            if (SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION, 3) != 0)
            {
                Print_SDL_Error("Could not set OpenGL's minor version!");
            }
            if (SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, SDL.SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_CORE) != 0)
            {
                Print_SDL_Error("Could not set OpenGL's profile!");
            }

            _window = SDL.SDL_CreateWindow("Derpy Simulation", SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED, 1280, 720,
                SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE
#if FULLSCREEN
                | SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP
#endif
                );
            if (_window == IntPtr.Zero)
            {
                Print_SDL_Error("Could not create the window!");
            }

            _gl = SDL.SDL_GL_CreateContext(_window);
            if (_gl == IntPtr.Zero)
            {
                Print_SDL_Error("Could not create the OpenGL context!");
            }
            if (SDL.SDL_GL_SetSwapInterval(1) != 0)
            {
                Print_SDL_Error("Could not enable VSync!");
            }
            if (SDL.SDL_GL_MakeCurrent(_window, _gl) != 0)
            {
                Print_SDL_Error("Could not start OpenGL on the window!");
            }
            OpenGL = GL.GetApi(SDL.SDL_GL_GetProcAddress);
#if DEBUG
            unsafe
            {
                OpenGL.Enable(EnableCap.DebugOutput);
                OpenGL.DebugMessageCallback(HandleGLError, null);
            }
#endif

            // The rest inits in Start()
        }

        private static void Start()
        {
            // Init in the proper thread
            RenderTickTime = LogicTickTime = DateTime.Now;
            UpdateSize();
            _ = new LightController();
            _sim = new Simulation(OpenGL, SimulationCreationSettings.CreatePreset(2));
        }
        private static void UpdateSize()
        {
            SDL.SDL_GetWindowSize(_window, out int w, out int h);
            CurrentWidth = (uint)w;
            CurrentHeight = (uint)h;
        }

        private static void Main()
        {
            Start();

            // Main loop
            while (true)
            {
                if (HandleEvents())
                {
                    break;
                }

                DateTime now = DateTime.Now;
                DateTime prev = LogicTickTime;
                bool doTick;
                if (now <= prev)
                {
#if DEBUG
                    Log.WriteLineWithTime("Time went back!");
#endif
                    doTick = true;
                }
                else
                {
                    doTick = (now - prev).TotalMilliseconds >= NumMillisecondsPerTick;
                }
                if (doTick)
                {
                    LogicTickTime = now;
                    DoLogicTick();
                }

                now = DateTime.Now;
                prev = RenderTickTime;
                RenderTimeSinceLastFrame = now <= prev ? TimeSpan.Zero : now - prev;
                RenderTickTime = now;
                DoRenderTick();
            }

            // Quitting
            GameExit();
        }

        private static bool HandleEvents()
        {
            while (SDL.SDL_PollEvent(out SDL.SDL_Event e) != 0)
            {
                switch (e.type)
                {
                    case SDL.SDL_EventType.SDL_QUIT:
                    {
                        return true;
                    }
                    case SDL.SDL_EventType.SDL_WINDOWEVENT:
                    {
                        SDL.SDL_WindowEventID ev = e.window.windowEvent;
                        switch (ev)
                        {
                            case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                            {
                                UpdateSize();
                                Resized?.Invoke(CurrentWidth, CurrentHeight);
                                break;
                            }
                        }
                        break;
                    }
                    case SDL.SDL_EventType.SDL_KEYDOWN:
                    {
                        SDL.SDL_Keycode sym = e.key.keysym.sym;
                        InputManager.OnKeyDown(sym, true);
                        break;
                    }
                    case SDL.SDL_EventType.SDL_KEYUP:
                    {
                        SDL.SDL_Keycode sym = e.key.keysym.sym;
                        InputManager.OnKeyDown(sym, false);
                        break;
                    }
                }
            }
            return false;
        }

        private static void DoLogicTick()
        {
            InputManager.LogicTick();
            _sim.LogicTick();
        }
        private static void DoRenderTick()
        {
            GL gl = OpenGL;
            gl.Viewport(0, 0, CurrentWidth, CurrentHeight);

            // Render
            _sim.Render(gl);

            // Present to window
            SDL.SDL_GL_SwapWindow(_window);
        }

        private static void Print_SDL_Error(string error)
        {
            error = string.Format("{2}{0}SDL Error: \"{1}\"", Environment.NewLine, SDL.SDL_GetError(), error);
#if DEBUG
            Log.WriteLineWithTime(error);
#endif
            throw new Exception(error);
        }
#if DEBUG
        private static void HandleGLError(GLEnum _, GLEnum type, int id, GLEnum severity, int length, IntPtr message, IntPtr __)
        {
            if (severity == GLEnum.DebugSeverityNotification)
            {
                return;
            }
            string msg = Marshal.PtrToStringAnsi(message, length);
            Log.WriteLineWithTime("GL Error:");
            Log.ModifyIndent(+1);
            Log.WriteLine(string.Format("Message: \"{0}\"", msg));
            Log.WriteLine(string.Format("Type: \"{0}\"", type));
            Log.WriteLine(string.Format("Id: \"{0}\"", id));
            Log.WriteLine(string.Format("Severity: \"{0}\"", severity));
            Log.ModifyIndent(-1);
            ;
        }
#endif

        private static void GameExit()
        {
            SDL.SDL_GL_DeleteContext(_gl);
            SDL.SDL_DestroyWindow(_window);
            SDL.SDL_Quit();
        }
    }
}
