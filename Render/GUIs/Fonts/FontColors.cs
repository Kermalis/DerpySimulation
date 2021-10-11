namespace DerpySimulation.Render.GUIs.Fonts
{
    internal static class FontColors
    {
        public static Color4[] Disabled { get; } = new Color4[] { Colors.Transparent, Color4.FromRGB(133, 133, 141), Color4.FromRGB(58, 50, 50) };
        public static Color4[] Black_I { get; } = new Color4[] { Colors.Transparent, Color4.FromRGB(15, 25, 30), Color4.FromRGB(170, 185, 185) };
        public static Color4[] Blue_I { get; } = new Color4[] { Colors.Transparent, Color4.FromRGB(0, 110, 250), Color4.FromRGB(120, 185, 230) };
        public static Color4[] Blue_O { get; } = new Color4[] { Colors.Transparent, Color4.FromRGB(115, 148, 255), Color4.FromRGB(0, 0, 214) };
        public static Color4[] Cyan_O { get; } = new Color4[] { Colors.Transparent, Color4.FromRGB(50, 255, 255), Color4.FromRGB(0, 90, 140) };
        public static Color4[] DarkGray_I { get; } = new Color4[] { Colors.Transparent, Color4.FromRGB(90, 82, 82), Color4.FromRGB(165, 165, 173) };
        public static Color4[] Red_I { get; } = new Color4[] { Colors.Transparent, Color4.FromRGB(230, 30, 15), Color4.FromRGB(250, 170, 185) };
        public static Color4[] Red_O { get; } = new Color4[] { Colors.Transparent, Color4.FromRGB(255, 50, 50), Color4.FromRGB(110, 0, 0) };
        public static Color4[] Red_Lighter_O { get; } = new Color4[] { Colors.Transparent, Color4.FromRGB(255, 115, 115), Color4.FromRGB(198, 0, 0) };
        public static Color4[] Yellow_O { get; } = new Color4[] { Colors.Transparent, Color4.FromRGB(255, 224, 22), Color4.FromRGB(188, 165, 16) };
        public static Color4[] White_I { get; } = new Color4[] { Colors.Transparent, Color4.FromRGB(239, 239, 239), Color4.FromRGB(132, 132, 132) };
        public static Color4[] White_DarkerOutline_I { get; } = new Color4[] { Colors.Transparent, Color4.FromRGB(250, 250, 250), Color4.FromRGB(80, 80, 80) };

#if DEBUG
        public static Color4[] Debug { get; } = new Color4[] { new(Colors.Red, 1), new(Colors.Green, 1), new(Colors.Blue, 1) };
#endif
    }
}
