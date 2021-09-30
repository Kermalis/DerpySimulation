using System;
using System.IO;

namespace DerpySimulation.Core
{
    internal static class AssetLoader
    {
        private const string AssetPath = "Assets";

        public static string GetPath(string asset)
        {
            asset = Path.Combine(AssetPath, asset);
            if (!File.Exists(asset))
            {
                throw new ArgumentOutOfRangeException(nameof(asset), "Asset not found: " + asset);
            }
            return asset;
        }
        public static FileStream GetAssetStream(string asset)
        {
            return File.OpenRead(GetPath(asset));
        }
        public static StreamReader GetAssetStreamText(string asset)
        {
            return File.OpenText(GetPath(asset));
        }
    }
}
