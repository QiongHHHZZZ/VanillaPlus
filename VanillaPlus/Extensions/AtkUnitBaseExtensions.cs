using FFXIVClientStructs.FFXIV.Component.GUI;

namespace VanillaPlus.Extensions;

public static unsafe class AtkUnitBaseExtensions {
    public static T* GetNodeById<T>(this ref AtkUnitBase addon, uint nodeId) where T : unmanaged
        => addon.UldManager.SearchNodeById<T>(nodeId);
}
