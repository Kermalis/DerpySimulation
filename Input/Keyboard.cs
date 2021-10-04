using SDL2;
using System.Collections.Generic;

namespace DerpySimulation.Input
{
    internal static class Keyboard
    {
        private static readonly Dictionary<Key, PressData> _keys;
        static Keyboard()
        {
            _keys = PressData.CreateDict<Key>();
        }

        public static bool IsDown(Key k)
        {
            return _keys[k].IsPressed;
        }
        public static bool JustPressed(Key k)
        {
            return _keys[k].IsNew;
        }
        public static bool JustReleased(Key k)
        {
            return _keys[k].WasReleased;
        }

        public static void OnKeyDown(in SDL.SDL_Event e, bool down)
        {
            Key key;
            switch (e.key.keysym.sym)
            {
                case SDL.SDL_Keycode.SDLK_ESCAPE: key = Key.Escape; break;
                case SDL.SDL_Keycode.SDLK_w: key = Key.W; break;
                case SDL.SDL_Keycode.SDLK_a: key = Key.A; break;
                case SDL.SDL_Keycode.SDLK_s: key = Key.S; break;
                case SDL.SDL_Keycode.SDLK_d: key = Key.D; break;
                case SDL.SDL_Keycode.SDLK_LSHIFT: key = Key.LShift; break;
                case SDL.SDL_Keycode.SDLK_SPACE: key = Key.Space; break;
                default: return;
            }
            _keys[key].OnDown(down);
        }

        public static void Prepare()
        {
            PressData.PrepareMany(_keys.Values);
        }
    }
}
