using Silk.NET.OpenGL;

namespace DerpySimulation.Render
{
    internal struct Color3
    {
        public float R;
        public float G;
        public float B;

        public Color3(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
        }

        public static Color3 FromRGB(uint r, uint g, uint b)
        {
            return new Color3(r / 255f, g / 255f, b / 255f);
        }

        public static Color3 Lerp(in Color3 c1, in Color3 c2, float progress)
        {
            return new Color3(
                c1.R + (c2.R - c1.R) * progress,
                c1.G + (c2.G - c1.G) * progress,
                c1.B + (c2.B - c1.B) * progress
                );
        }

        public static unsafe void PutInShader(GL gl, int loc, Color3 c)
        {
            gl.Uniform3(loc, 1, (float*)&c);
        }
    }
    internal struct Color4
    {
        public Color3 RGB;
        public float A;

        public Color4(in Color3 rgb, float a)
        {
            RGB = rgb;
            A = a;
        }
        public Color4(float r, float g, float b, float a)
        {
            RGB = new Color3(r, g, b);
            A = a;
        }

        public static Color4 FromRGB(uint r, uint g, uint b)
        {
            return new Color4(Color3.FromRGB(r, g, b), 1f);
        }
        public static Color4 FromRGBA(in Color3 rgb, uint a)
        {
            return new Color4(rgb, a / 255f);
        }
        public static Color4 FromRGBA(uint r, uint g, uint b, uint a)
        {
            return new Color4(Color3.FromRGB(r, g, b), a / 255f);
        }

        public static Color4 Lerp(in Color4 c1, in Color4 c2, float progress)
        {
            return new Color4(
                Color3.Lerp(c1.RGB, c2.RGB, progress),
                c1.A + (c2.A - c1.A) * progress
                );
        }

        public static unsafe void PutInShader(GL gl, int loc, Color4 c)
        {
            gl.Uniform4(loc, 1, (float*)&c);
        }
    }

    internal static class Colors
    {
        public static Color4 Transparent { get; } = new Color4(0, 0, 0, 0);
        public static Color3 Black { get; } = new Color3(0, 0, 0);
        public static Color3 White { get; } = new Color3(1, 1, 1);
        public static Color3 Red { get; } = new Color3(1, 0, 0);
        public static Color3 Green { get; } = new Color3(0, 1, 0);
        public static Color3 Blue { get; } = new Color3(0, 0, 1);
    }
}
