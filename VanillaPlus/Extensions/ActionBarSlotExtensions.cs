using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace VanillaPlus.Extensions;

public static unsafe class ActionBarSlotExtensions {
    public static AtkImageNode* GetImageNode(this ref ActionBarSlot slot) {
        var component = slot.GetIconComponent();
        if (component is null) return null;

        return component->IconImage;
    }

    public static AtkResNode* GetFrameNode(this ref ActionBarSlot slot) {
        var component = slot.GetIconComponent();
        if (component is null) return null;

        return component->Frame;
    }

    private static AtkComponentIcon* GetIconComponent(this ref ActionBarSlot slot) {
        if (slot.Icon is null) return null;
        return (AtkComponentIcon*) slot.Icon->Component;
    }
}
