using DerpySimulation.Render;
using SDL2;
using System.Collections.Generic;

namespace DerpySimulation.Input
{
    internal static class Mouse
    {
        private static readonly Dictionary<MouseButton, PressData> _buttons;
        static Mouse()
        {
            _buttons = PressData.CreateDict<MouseButton>();
        }

        /// <summary>-1, 0, or +1</summary>
        public static int Scroll;
        public static int X;
        public static int Y;
        public static int DeltaX;
        public static int DeltaY;

        public static bool IsDown(MouseButton b)
        {
            return _buttons[b].IsPressed;
        }
        public static bool JustPressed(MouseButton b)
        {
            return _buttons[b].IsNew;
        }
        public static bool JustReleased(MouseButton b)
        {
            return _buttons[b].WasReleased;
        }

        public static void LockMouseInWindow(bool locked)
        {
            Display.LockMouseInWindow(locked);
        }
        public static void CenterMouseInWindow()
        {
            uint x = Display.CurrentWidth / 2;
            uint y = Display.CurrentHeight / 2;
            Display.SetSDLMousePosition((int)x, (int)y);
        }
        public static void LockMouseIfJustClicked()
        {
            if (JustPressed(MouseButton.Left))
            {
                LockMouseInWindow(true);
            }
            else if (JustReleased(MouseButton.Left))
            {
                LockMouseInWindow(false);
                CenterMouseInWindow();
            }
        }

        public static void OnButtonDown(in SDL.SDL_Event e, bool down)
        {
            MouseButton button;
            switch ((uint)e.button.button)
            {
                case SDL.SDL_BUTTON_LEFT: button = MouseButton.Left; break;
                case SDL.SDL_BUTTON_MIDDLE: button = MouseButton.Middle; break;
                case SDL.SDL_BUTTON_RIGHT: button = MouseButton.Right; break;
                case SDL.SDL_BUTTON_X1: button = MouseButton.X1; break;
                case SDL.SDL_BUTTON_X2: button = MouseButton.X2; break;
                default: return;
            }
            _buttons[button].OnDown(down);
        }
        public static void OnScroll(in SDL.SDL_Event e)
        {
            Scroll = e.wheel.y;
        }
        public static void OnMove(in SDL.SDL_Event e)
        {
            X = e.motion.x;
            Y = e.motion.y;
            DeltaX = e.motion.xrel;
            DeltaY = e.motion.yrel;
        }

        public static void Prepare()
        {
            PressData.PrepareMany(_buttons.Values);
            DeltaX = 0;
            DeltaY = 0;
            Scroll = 0;
        }
    }
}
