using Kermalis.EndianBinaryIO;

namespace DerpySimulation.Render.GUIs.Fonts
{
    internal sealed class PackedGlyph
    {
        public readonly byte CharWidth;
        public readonly byte CharSpace;
        public readonly byte[] PackedBitmap;

        public PackedGlyph(EndianBinaryReader r, byte fontHeight, byte bpp)
        {
            CharWidth = r.ReadByte();
            CharSpace = r.ReadByte();
            int numBitsToRead = fontHeight * CharWidth * bpp;
            PackedBitmap = r.ReadBytes((numBitsToRead / 8) + ((numBitsToRead % 8) != 0 ? 1 : 0));
        }
    }
    internal sealed class Glyph
    {
        public readonly byte CharWidth;
        public readonly byte CharSpace;
        public readonly AtlasPos AtlasPos;

        public Glyph(byte[] dst, int posX, int posY, uint atlasW, uint atlasH, byte fontHeight, byte bpp, PackedGlyph g)
        {
            CharWidth = g.CharWidth;
            CharSpace = g.CharSpace;
            if (CharWidth == 0)
            {
                return;
            }
            AtlasPos = new AtlasPos(posX, posY, CharWidth, fontHeight, atlasW, atlasH);

            // Draw to texture atlas
            byte[] packed = g.PackedBitmap;

            int curBit = 0;
            int curByte = 0;
            for (int py = 0; py < fontHeight; py++)
            {
                for (int px = 0; px < CharWidth; px++)
                {
                    int colorIndex = (packed[curByte] >> (8 - bpp - curBit)) % (1 << bpp);
                    dst[RenderUtils.GetPixelIndex(atlasW, px + posX, py + posY)] = (byte)colorIndex; // Only set the R component
                    curBit = (curBit + bpp) % 8;
                    if (curBit == 0)
                    {
                        curByte++;
                    }
                }
            }
        }
    }
}
