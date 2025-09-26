using FFXIVClientStructs.FFXIV.Component.GUI;

namespace VanillaPlus.Extensions;

public static unsafe class AtkUldManagerExtensions {
    public static T* SearchNodeById<T>(this AtkUldManager atkUldManager, uint nodeId) where T : unmanaged {
        foreach (var node in atkUldManager.Nodes) {
            if (node.Value is not null) {
                if (node.Value->NodeId == nodeId)
                    return (T*) node.Value;
            }
        }

        return null;
    }

    public static AtkResNode* SearchNodeById(this AtkUldManager atkUldManager, uint nodeId)
        => atkUldManager.SearchNodeById<AtkResNode>(nodeId);
}
