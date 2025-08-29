using FFXIVClientStructs.FFXIV.Component.GUI;

namespace VanillaPlus.Extensions;

public static unsafe class AtkComponentNodeExtensions {
    public static T* SearchNodeById<T>(ref this AtkComponentNode node, uint id) where T : unmanaged
        => node.Component->UldManager.SearchNodeById<T>(id);
}
