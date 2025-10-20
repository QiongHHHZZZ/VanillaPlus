using System;
using System.Collections.Generic;
using System.IO;
using Dalamud.Interface.Textures.TextureWraps;

namespace VanillaPlus.Utilities;

public static class Assets {
    public static HashSet<IDalamudTextureWrap> LoadedTextures = [];
    
    public static string GetAssetDirectoryPath()
        => Path.Combine(Services.PluginInterface.AssemblyLocation.DirectoryName ?? throw new Exception("Dalamud 提供的目录无效。"), "Assets");
    
    public static string GetAssetPath(string assetName)
        => Path.Combine(GetAssetDirectoryPath(), assetName);
    
    public static IDalamudTextureWrap? LoadAsset(string assetName)
        => Services.TextureProvider.GetFromFile(GetAssetPath(assetName)).GetWrapOrDefault();
}
