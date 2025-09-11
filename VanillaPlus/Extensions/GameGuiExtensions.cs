using System;
using Dalamud.Plugin.Services;

namespace VanillaPlus.Extensions;

public static class GameGuiExtensions {
    [Obsolete("Replace with Dalamud.IGameGui.GetAddonByName<T> after next dalamud stable release.")]
    public static unsafe T* InternalGetAddonByName<T>(this IGameGui gameGui, string name) where T : unmanaged
        => (T*) gameGui.GetAddonByName(name).Address;
}
