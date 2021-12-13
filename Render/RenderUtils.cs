using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DerpySimulation.Render
{
    internal static class RenderUtils
    {
        public static Matrix4x4 CreateTransform_ScaleRotPos(in Vector3 scale, in Quaternion rotation, in Vector3 position)
        {
            return Matrix4x4.CreateScale(scale)
                * Matrix4x4.CreateFromQuaternion(rotation)
                * Matrix4x4.CreateTranslation(position);
        }

        #region Textures

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TextureUnit ToTextureUnit(this int unit)
        {
            return (TextureUnit)((int)TextureUnit.Texture0 + unit);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetPixelIndex(uint srcW, int x, int y)
        {
            return (int)(x + (y * srcW));
        }

        public static unsafe void LoadTextureFromFile(GL gl, string path)
        {
            using (var img = Image.Load<Rgba32>(path))
            {
                fixed (void* imgdata = &MemoryMarshal.GetReference(img.GetPixelRowSpan(0)))
                {
                    LoadTextureData(gl, imgdata, (uint)img.Width, (uint)img.Height);
                }
            }
        }
        public static unsafe void LoadTextureData(GL gl, void* data, uint width, uint height)
        {
            gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba8, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);

            gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }

        #endregion

        #region FBOs

        public static void BindFBO(GL gl, uint fbo, DrawBufferMode buf, uint width, uint height)
        {
            gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, fbo);
            gl.DrawBuffer(buf);
            gl.Viewport(0, 0, width, height);
        }
        /// <summary>Sets the current FBO and viewport back to default.</summary>
        public static void UnbindFBO(GL gl)
        {
            gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            gl.DrawBuffer(DrawBufferMode.Back);
            gl.Viewport(0, 0, Display.CurrentWidth, Display.CurrentHeight);
        }

        public static unsafe uint CreateReflectionFBO(GL gl, uint width, uint height, out uint colorTexture, out uint depthBuffer)
        {
            uint fbo = gl.GenFramebuffer();
            gl.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);

            // Color attachment
            colorTexture = gl.GenTexture();
            gl.BindTexture(TextureTarget.Texture2D, colorTexture);
            gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
            gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, colorTexture, 0);

            // Depth attachment
            depthBuffer = gl.GenRenderbuffer();
            gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthBuffer);
            gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.DepthComponent24, width, height);
            gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthBuffer);

            gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            gl.Viewport(0, 0, Display.CurrentWidth, Display.CurrentHeight);

            return fbo;
        }
        public static unsafe uint CreateRefractionFBO(GL gl, uint width, uint height, out uint colorTexture, out uint depthTexture)
        {
            uint fbo = gl.GenFramebuffer();
            gl.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);

            // Color attachment
            colorTexture = gl.GenTexture();
            gl.BindTexture(TextureTarget.Texture2D, colorTexture);
            gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
            gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, colorTexture, 0);

            // Depth attachment
            depthTexture = gl.GenTexture();
            gl.BindTexture(TextureTarget.Texture2D, depthTexture);
            gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.DepthComponent24, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, null);
            gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthTexture, 0);

            gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            gl.Viewport(0, 0, Display.CurrentWidth, Display.CurrentHeight);

            return fbo;
        }

        #endregion

        #region VBOs

        /// <summary>The bound vbo must be a STREAM_DRAW array buffer.</summary>
        public static unsafe void AddInstancedAttribute(GL gl, uint attribIndex, int dataSize, uint stride, uint offset)
        {
            gl.EnableVertexAttribArray(attribIndex);
            gl.VertexAttribPointer(attribIndex, dataSize, VertexAttribPointerType.Float, false, stride, (void*)offset);
            gl.VertexAttribDivisor(attribIndex, 1);
        }
        public static void AddInstancedAttribute_Matrix4x4(GL gl, uint firstAttribIndex, uint stride, uint offset)
        {
            AddInstancedAttribute(gl, firstAttribIndex, 4, stride, offset);
            AddInstancedAttribute(gl, firstAttribIndex + 1, 4, stride, offset + (sizeof(float) * 4));
            AddInstancedAttribute(gl, firstAttribIndex + 2, 4, stride, offset + (sizeof(float) * 8));
            AddInstancedAttribute(gl, firstAttribIndex + 3, 4, stride, offset + (sizeof(float) * 12));
        }

        #endregion
    }
}
