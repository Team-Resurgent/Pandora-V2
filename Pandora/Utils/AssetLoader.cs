using System;
using System.IO;

namespace Pandora.Utils
{
    public static class AssetLoader
    {
        public static byte[] LoadAssetBytes(string assetPath)
        {
            var uri = new Uri($"avares://Pandora/Assets/{assetPath}");
            using var assetStrean = Avalonia.Platform.AssetLoader.Open(uri);
            using MemoryStream memoryStream = new();
            assetStrean.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
