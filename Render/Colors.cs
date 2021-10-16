using Silk.NET.OpenGL;
using System.Numerics;

namespace DerpySimulation.Render
{
    internal static class Colors
    {
        public static Vector4 Transparent { get; } = new Vector4(0, 0, 0, 0);
        public static Vector3 Black { get; } = new Vector3(0, 0, 0);
        public static Vector3 White { get; } = new Vector3(1, 1, 1);
        public static Vector3 Red { get; } = new Vector3(1, 0, 0);
        public static Vector3 Green { get; } = new Vector3(0, 1, 0);
        public static Vector3 Blue { get; } = new Vector3(0, 0, 1);

        public static Vector3 FromRGB(uint r, uint g, uint b)
        {
            return new Vector3(r / 255f, g / 255f, b / 255f);
        }
        public static Vector4 V4FromRGB(uint r, uint g, uint b)
        {
            return new Vector4(r / 255f, g / 255f, b / 255f, 1f);
        }
        public static Vector4 FromRGBA(in Vector3 rgb, uint a)
        {
            return new Vector4(rgb, a / 255f);
        }
        public static Vector4 FromRGBA(uint r, uint g, uint b, uint a)
        {
            return new Vector4(r / 255f, g / 255f, b / 255f, a / 255f);
        }

        public static unsafe void PutInShader(GL gl, int loc, Vector3 c)
        {
            gl.Uniform3(loc, 1, (float*)&c);
        }
        public static unsafe void PutInShader(GL gl, int loc, Vector4 c)
        {
            gl.Uniform4(loc, 1, (float*)&c);
        }
    }
}
