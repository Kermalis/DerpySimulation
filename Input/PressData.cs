using System;
using System.Collections.Generic;

namespace DerpySimulation.Input
{
    internal sealed class PressData
    {
        public bool IsPressed;

        /// <summary>True if this button was not pressed the previous frame but now is.</summary>
        public bool IsNew;
        /// <summary>True if the button was released this frame.</summary>
        public bool WasReleased;

        public void OnDown(bool down)
        {
            IsPressed = down;
            if (down)
            {
                IsNew = true;
            }
            else
            {
                WasReleased = true;
            }
        }

        public static Dictionary<T, PressData> CreateDict<T>() where T : struct, Enum
        {
            T[] arr = Enum.GetValues<T>();
            Dictionary<T, PressData> dict = new(arr.Length);
            for (int i = 0; i < arr.Length; i++)
            {
                dict.Add(arr[i], new PressData());
            }
            return dict;
        }
        public static void PrepareMany(IEnumerable<PressData> data)
        {
            foreach (PressData p in data)
            {
                p.IsNew = false;
                p.WasReleased = false;
            }
        }
    }
}
