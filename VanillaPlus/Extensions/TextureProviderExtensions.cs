using Dalamud.Interface.Textures;
using Dalamud.Plugin.Services;

namespace VanillaPlus.Extensions;

public static class TextureProviderExtensions {
    public static ISharedImmediateTexture GetPlaceholderTexture(this ITextureProvider textureProvider)
        => textureProvider.GetFromGameIcon(60042);
}
