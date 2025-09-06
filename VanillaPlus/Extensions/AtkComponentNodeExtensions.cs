using FFXIVClientStructs.FFXIV.Component.GUI;

namespace VanillaPlus.Extensions;

public static unsafe class AtkComponentNodeExtensions {
    public static T* SearchNodeById<T>(ref this AtkComponentNode node, uint id) where T : unmanaged
        => node.Component->UldManager.SearchNodeById<T>(id);

    public static void FadeNode(ref this AtkComponentNode node, float fadePercentage) {
        node.MultiplyRed = (byte) ((1 - fadePercentage) * 100);
        node.MultiplyGreen = (byte) ((1 - fadePercentage) * 100);
        node.MultiplyBlue = (byte) ((1 - fadePercentage) * 100);
    }
}
