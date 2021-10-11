using DerpySimulation.Core;
using Kermalis.EndianBinaryIO;
using Silk.NET.OpenGL;
using System.Collections.Generic;
using System.IO;

namespace DerpySimulation.Render.GUIs.Fonts
{
    internal sealed class Font
    {
        private const string FONT_PATH = @"Fonts\Font.kermfont";
        // The tolerance places extra space between chars in the texture atlas
        // This prevents edges of neighboring characters being visible when rendering with specific float fractions
        private const int TOLERANCE_X = 1;
        private const int TOLERANCE_Y = 1;

        public static Font Instance { get; private set; } = null!; // Set in Init()

        public readonly byte FontHeight;
        public readonly uint Texture;

        private readonly Dictionary<ushort, Glyph> _glyphs;
        private readonly (string OldKey, ushort NewKey)[] _overrides;

        public static void Init(GL gl)
        {
            Instance = new Font(gl, 1024, 1024, new (string, ushort)[]
            {
                ("♂", 0x246D),
                ("♀", 0x246E)
            });
        }

        // Atlas size must be a power of 2
        private unsafe Font(GL gl, uint atlasW, uint atlasH, (string, ushort)[] overrides)
        {
            using (var r = new EndianBinaryReader(AssetLoader.GetAssetStream(FONT_PATH), Endianness.LittleEndian))
            {
                FontHeight = r.ReadByte();
                if (FontHeight > atlasH)
                {
                    throw new InvalidDataException();
                }
                byte bitsPerPixel = r.ReadByte();
                int numGlyphs = r.ReadInt32();
                var packed = new Dictionary<ushort, PackedGlyph>(numGlyphs);
                for (int i = 0; i < numGlyphs; i++)
                {
                    packed.Add(r.ReadUInt16(), new PackedGlyph(r, FontHeight, bitsPerPixel));
                }
                _overrides = overrides;

                // Make texture atlas. Atlas must be sized by powers of 2
                byte[] dest = new byte[atlasW * atlasH];
                _glyphs = new Dictionary<ushort, Glyph>(numGlyphs);
                int posX = 0;
                int posY = 0;
                foreach (KeyValuePair<ushort, PackedGlyph> k in packed)
                {
                    ushort key = k.Key;
                    PackedGlyph pg = k.Value;
                    if (pg.CharWidth > atlasW)
                    {
                        throw new InvalidDataException();
                    }
                    if (posX >= atlasW || posX + pg.CharWidth > atlasW)
                    {
                        posX = 0;
                        posY += FontHeight + TOLERANCE_Y;
                        if (posY + FontHeight > atlasH)
                        {
                            throw new InvalidDataException();
                        }
                    }
                    var g = new Glyph(dest, posX, posY, atlasW, atlasH, FontHeight, bitsPerPixel, pg);
                    _glyphs.Add(key, g);
                    posX += g.CharWidth + TOLERANCE_X;
                }
                // Create the texture
                fixed (byte* dst = dest)
                {
                    gl.ActiveTexture(TextureUnit.Texture0);
                    Texture = gl.GenTexture();
                    gl.BindTexture(TextureTarget.Texture2D, Texture);
                    gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.R8ui, atlasW, atlasH, 0, PixelFormat.RedInteger, PixelType.UnsignedByte, dst);
                    gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                    gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                }
            }
        }

        public Glyph? GetGlyph(string str, ref int index, ref uint xOffset, ref uint yOffset, out string? readStr)
        {
            char c = str[index];
            if (c == '\r') // Completely ignore CR
            {
                index++;
                readStr = null;
                return null;
            }
            if (c == '\n' || c == '\v')
            {
                index++;
                xOffset = 0;
                yOffset += (uint)FontHeight + 1;
                readStr = c.ToString();
                return null;
            }
            if (c == '\f')
            {
                index++;
                xOffset = 0;
                yOffset = 0;
                readStr = "\f";
                return null;
            }
            Glyph? ret;
            for (int i = 0; i < _overrides.Length; i++)
            {
                (string oldKey, ushort newKey) = _overrides[i];
                int ol = oldKey.Length;
                if (index + ol <= str.Length && str.Substring(index, ol) == oldKey)
                {
                    index += ol;
                    ret = _glyphs[newKey];
                    readStr = oldKey;
                    goto bottom;
                }
            }
            // ret was not found in the loop
            index++;
            if (!_glyphs.TryGetValue(c, out ret))
            {
                ret = _glyphs['?']; // Will crash if there is no '?' in this font
            }
            readStr = c.ToString();
        bottom:
            xOffset += (uint)ret.CharWidth + ret.CharSpace;
            return ret;
        }

        public void MeasureString(string str, out uint w, out uint h)
        {
            if (string.IsNullOrEmpty(str))
            {
                w = 0;
                h = 0;
                return;
            }
            w = 0;
            h = FontHeight;
            int index = 0;
            uint xOffset = 0;
            while (index < str.Length)
            {
                GetGlyph(str, ref index, ref xOffset, ref h, out _);
                if (xOffset > w)
                {
                    w = xOffset;
                }
            }
        }

        public void Delete(GL gl)
        {
            gl.DeleteTexture(Texture);
        }
    }
}
