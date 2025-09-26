using Dalamud.Game.Addon.Events;
using Dalamud.Plugin.Services;

namespace VanillaPlus.Extensions;

public static class AddonEventManagerExtensions {
    public static void RemoveEventNullable(this IAddonEventManager manager, IAddonEventHandle? handle) {
        if (handle is not null) {
            manager.RemoveEvent(handle);
        }
    }
}
