using DerpySimulation.Render.Data;
using DerpySimulation.Render.GUIs.Positioning;
using DerpySimulation.Render.Shaders;
using Silk.NET.OpenGL;
using System.Numerics;

namespace DerpySimulation.Render.GUIs.Fonts
{
    internal sealed class GUIString : GUIComponent, IGUIVisual
    {
        public readonly string Str;
        public readonly Font Font;
        public Color4[] Colors;
        public readonly uint Scale;

        private uint _vao;
        private uint _vbo;
        private uint _ebo;
        private uint _totalVisible;

        public GUIString(string str, Font font, Color4[] colors, uint scale = 1)
        {
            Str = str;
            Font = font;
            Colors = colors;
            Scale = scale;
        }

        protected override unsafe void Init(GL gl)
        {
            // Write glyph vertices
            // May not necessarily use this many, because some glyphs don't have visual results
            var vertices = new VBOData_Font[Str.Length * 4];
            uint[] indices = new uint[Str.Length * 6];
            _totalVisible = 0;
            uint nextXOffset = 0;
            uint nextYOffset = 0;
            int index = 0;
            uint w;
            uint h = Font.FontHeight * Scale;
            while (index < Str.Length)
            {
                int curX = (int)(nextXOffset * Scale);
                int curY = (int)(nextYOffset * Scale);
                Glyph? g = Font.GetGlyph(Str, ref index, ref nextXOffset, ref nextYOffset, out _);
                if (g is null)
                {
                    continue;
                }
                w = g.CharWidth * Scale;
                // Can't use triangle strips
                uint vIndex = _totalVisible * 4;
                uint iIndex = _totalVisible * 6;
                int exclusiveRight = curX + (int)w;
                int exclusiveBottom = curY + (int)h;
                vertices[vIndex + 0] = new VBOData_Font(curX, curY, g.AtlasPos.Start);
                vertices[vIndex + 1] = new VBOData_Font(curX, exclusiveBottom, g.AtlasPos.GetBottomLeft());
                vertices[vIndex + 2] = new VBOData_Font(exclusiveRight, curY, g.AtlasPos.GetTopRight());
                vertices[vIndex + 3] = new VBOData_Font(exclusiveRight, exclusiveBottom, g.AtlasPos.GetBottomRight());
                indices[iIndex + 0] = vIndex + 0;
                indices[iIndex + 1] = vIndex + 1;
                indices[iIndex + 2] = vIndex + 2;
                indices[iIndex + 3] = vIndex + 2;
                indices[iIndex + 4] = vIndex + 1;
                indices[iIndex + 5] = vIndex + 3;
                _totalVisible++;
            }

            // Create vao
            _vao = gl.GenVertexArray();
            gl.BindVertexArray(_vao);

            // Store in vbo
            _vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            fixed (void* data = vertices)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, VBOData_Font.SizeOf * _totalVisible * 4, data, BufferUsageARB.StaticDraw);
            }
            // Store in ebo
            _ebo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
            fixed (void* data = indices)
            {
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, sizeof(uint) * _totalVisible * 6, data, BufferUsageARB.StaticDraw);
            }

            // Now set attribs
            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, VBOData_Font.SizeOf, (void*)VBOData_Font.OffsetOfPos);
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, VBOData_Font.SizeOf, (void*)VBOData_Font.OffsetOfUV);

            gl.BindVertexArray(0);
        }

        protected override void Update(GL gl, float delta)
        {
            //
        }

        public GUIConstraints CreateCenterConstraints()
        {
            Font.MeasureString(Str, out uint w, out uint h);
            return new GUIConstraints(
                new CenterConstraint(),
                new CenterConstraint(),
                new PixelConstraint((int)(w * Scale)),
                new PixelConstraint((int)(h * Scale))
                );
        }

        public unsafe void Render(GL gl, float delta)
        {
            if (_totalVisible == 0)
            {
                return;
            }

            gl.ActiveTexture(TextureUnit.Texture0);
            gl.BindTexture(TextureTarget.Texture2D, Font.Texture);
            gl.BindVertexArray(_vao);
            FontShader shader = FontShader.Instance;
            shader.Use(gl);
            shader.SetTranslation(gl, new Vector2(RelPos.X, RelPos.Y));
            shader.SetColors(gl, Colors);

            gl.DrawElements(PrimitiveType.Triangles, _totalVisible * 6, DrawElementsType.UnsignedInt, (void*)0);

            gl.UseProgram(0);
            gl.BindTexture(TextureTarget.Texture2D, 0);
            gl.BindVertexArray(0);
        }

        protected override void Delete(GL gl)
        {
            gl.DeleteVertexArray(_vao);
            gl.DeleteBuffer(_vbo);
            gl.DeleteBuffer(_ebo);
        }
    }
}
