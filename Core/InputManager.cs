using SDL2;
using System;
using System.Collections.Generic;

namespace DerpySimulation.Core
{
    // TODO: This is from PokemonGameEngine and should be changed
    internal static class InputManager
    {
        private sealed class KeyDownData
        {
            // Updated in real time
            public bool Pressed;
            // Updated every logic tick
            public bool IsNew; // True if this key was not active the previous tick but now is
            public ulong PressTime; // The amount of ticks this key has been active
            public bool IsActive; // True if the key is active
        }

        private static readonly Dictionary<Key, KeyDownData> _pressed = new();
        static InputManager()
        {
            foreach (Key k in Enum.GetValues<Key>())
            {
                _pressed.Add(k, new KeyDownData());
            }
        }

        // Updating the real time presses
        public static void OnKeyDown(SDL.SDL_Keycode sym, bool down)
        {
            Key key;
            switch (sym)
            {
                case SDL.SDL_Keycode.SDLK_q: key = Key.L; break;
                case SDL.SDL_Keycode.SDLK_w: key = Key.R; break;
                case SDL.SDL_Keycode.SDLK_a: key = Key.X; break;
                case SDL.SDL_Keycode.SDLK_s: key = Key.Y; break;
                case SDL.SDL_Keycode.SDLK_z: key = Key.B; break;
                case SDL.SDL_Keycode.SDLK_x: key = Key.A; break;
                case SDL.SDL_Keycode.SDLK_LEFT: key = Key.Left; break;
                case SDL.SDL_Keycode.SDLK_RIGHT: key = Key.Right; break;
                case SDL.SDL_Keycode.SDLK_DOWN: key = Key.Down; break;
                case SDL.SDL_Keycode.SDLK_UP: key = Key.Up; break;
                case SDL.SDL_Keycode.SDLK_RETURN: key = Key.Start; break;
                case SDL.SDL_Keycode.SDLK_RSHIFT: key = Key.Select; break;
                default: return;
            }

            KeyDownData p = _pressed[key];
            p.Pressed = down;
        }

        public static void LogicTick()
        {
            foreach (KeyValuePair<Key, KeyDownData> kvp in _pressed)
            {
                KeyDownData p = kvp.Value;
                bool active = p.Pressed;
                if (p.IsActive) // Was active last tick
                {
                    p.IsNew = false;
                    if (active)
                    {
                        p.PressTime++;
                    }
                    else
                    {
                        p.IsActive = false;
                        p.PressTime = 0;
                    }
                }
                else // Not active last tick
                {
                    if (active)
                    {
                        p.IsNew = true;
                        p.IsActive = true;
                        p.PressTime = 0;
                    }
                }
            }
        }

        public static bool IsPressed(Key key)
        {
            KeyDownData p = _pressed[key];
            return p.IsActive && p.IsNew;
        }
        public static bool IsDown(Key key, uint downTime = 0)
        {
            KeyDownData p = _pressed[key];
            return p.IsActive && p.PressTime >= downTime;
        }

#if DEBUG
        public static string Debug_GetKeys()
        {
            string s = string.Empty;
            foreach (KeyValuePair<Key, KeyDownData> kvp in _pressed)
            {
                KeyDownData p = kvp.Value;
                s += string.Format("{0,-15}{1}{2}", kvp.Key, p.IsActive ? p.PressTime : null, Environment.NewLine);
            }
            return s;
        }
#endif
    }
}
