using DerpySimulation.Core;
using System;
using System.Numerics;

namespace DerpySimulation.World
{
    internal static class ColorGenerator
    {
        /// <summary>Calculates the color for every vertex of the terrain by linearly interpolating between the colors depending on the vertex's height.</summary>
        public static Vector3[,] GenerateColors(ColorStep[] colors, float[,] gridHeights)
        {
            var gridColors = new Vector3[gridHeights.GetLength(0), gridHeights.GetLength(1)];
            for (int z = 0; z < gridHeights.GetLength(0); z++)
            {
                for (int x = 0; x < gridHeights.GetLength(1); x++)
                {
                    gridColors[z, x] = CalcColor(colors, gridHeights[z, x]);
                }
            }
            return gridColors;
        }

        /// <summary>Determines the color of the vertex based on the provided height.</summary>
        private static Vector3 CalcColor(ColorStep[] colors, float height)
        {
            int firstColorIdx = GetColorIdxForHeight(colors, height);
            Vector3 firstColor = colors[firstColorIdx].Color;
            float firstColorHeight = colors[firstColorIdx].Height;
            // If it's the last color, should rarely happen
            // Or if there should be no blending
            if (firstColorIdx == colors.Length - 1 || firstColorHeight == height)
            {
                return firstColor;
            }

            // From now on we assume there is a next color
            int nextColorIdx = firstColorIdx + 1;
            Vector3 nextColor = colors[nextColorIdx].Color;
            float nextColorHeight = colors[nextColorIdx].Height;

            float newHeight = height - firstColorHeight;
            float heightDifference = nextColorHeight - firstColorHeight; // The amount between the two heights
            float nextColorAmt = newHeight / heightDifference; // Convert newHeight to a value between 0 and 1 exclusive
            return Utils.InterpolateColors(firstColor, nextColor, nextColorAmt);
        }

        private static int GetColorIdxForHeight(ColorStep[] colors, float height)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                float iHeight = colors[i].Height;
                if (iHeight == i || i == colors.Length - 1)
                {
                    return i; // If the heights are the same or this is the last height in the table
                }
                float nextIHeight = colors[i + 1].Height;
                if (height > iHeight && height < nextIHeight)
                {
                    return i; // If between two values
                }
            }
            throw new Exception(); // Should never reach here
        }
    }
}
