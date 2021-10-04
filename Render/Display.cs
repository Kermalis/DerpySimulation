using SDL2;
using Silk.NET.OpenGL;
using System;
#if DEBUG
using DerpySimulation.Debug;
using System.Runtime.InteropServices;
#endif

namespace DerpySimulation.Render
{
    internal static class Display
    {
        private static readonly IntPtr _window;
        private static readonly IntPtr _gl;
        public static readonly GL OpenGL;

        public static uint CurrentWidth { get; private set; }
        public static uint CurrentHeight { get; private set; }

        public delegate void ResizeEventHandler(uint w, uint h);
        public static event ResizeEventHandler Resized;

        static Display()
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
        }

        public static void Init()
        {
            UpdateSize();
        }
        public static void PrepareFrame()
        {
            OpenGL.Viewport(0, 0, CurrentWidth, CurrentHeight);
        }
        public static void PresentFrame()
        {
            SDL.SDL_GL_SwapWindow(_window);
        }

        public static void OnResized()
        {
            UpdateSize();
            Resized?.Invoke(CurrentWidth, CurrentHeight);
        }
        private static void UpdateSize()
        {
            SDL.SDL_GetWindowSize(_window, out int w, out int h);
            CurrentWidth = (uint)w;
            CurrentHeight = (uint)h;
        }

        public static void LockMouseInWindow(bool locked)
        {
            if (SDL.SDL_SetRelativeMouseMode(locked ? SDL.SDL_bool.SDL_TRUE : SDL.SDL_bool.SDL_FALSE) != 0)
            {
                Print_SDL_Error("Could not lock mouse!");
            }
        }
        public static void SetSDLMousePosition(int x, int y)
        {
            SDL.SDL_WarpMouseInWindow(_window, x, y);
        }

        public static void Print_SDL_Error(string error)
        {
            error = string.Format("{2}{0}SDL Error: \"{1}\"", Environment.NewLine, SDL.SDL_GetError(), error);
#if DEBUG
            Log.WriteLineWithTime(error);
#endif
            throw new Exception(error);
        }
#if DEBUG
        // TODO: GL error when Discord starts streaming the application, but the program continues on after
        // Also throws the error when resizing while Discord streaming
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

        public static void Quit()
        {
            SDL.SDL_GL_DeleteContext(_gl);
            SDL.SDL_DestroyWindow(_window);
            SDL.SDL_Quit();
        }
    }
}
