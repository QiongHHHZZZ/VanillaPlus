using System.Numerics;
using KamiToolKit.Addon;
using VanillaPlus.Classes;

namespace VanillaPlus.Extensions;

public static class NativeAddonExtensions {
    public static void InitializeConfig(this NativeAddon addon, AddonConfig config) {
        if (config.WindowPosition == Vector2.Zero) {
            // Do nothing, the window will automatically appear in the middle of the screen,
            // when it closes it will update the position
        }
        else {
            addon.Position = config.WindowPosition;
        }

        if (config.WindowSize == Vector2.Zero) {
            config.WindowSize = addon.Size;
            config.Save();
        }
        else {
            addon.Size = config.WindowSize;
        }
    }
}
