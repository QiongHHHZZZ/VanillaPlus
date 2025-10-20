using System.Linq;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.SelectNextLootItem;

public unsafe class SelectNextLootItem : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "自动选中下一个掉落",
        Description = "在选择需求、贪婪或放弃后自动跳转至下一件掉落物品。\n\n" +
                      "注意：此功能不会自动掷骰。",
        Type = ModificationType.GameBehavior,
        Authors = [ "MidoriKami" ],
        ChangeLog = [
            new ChangeLogInfo(1, "初始实现"),
        ],
        CompatibilityModule = new SimpleTweaksCompatibilityModule("UiAdjustments@LootWindowSelectNext"),
    };
    
    public override void OnEnable() {
        Services.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "NeedGreed", OnNeedGreedSetup);
        Services.AddonLifecycle.RegisterListener(AddonEvent.PostReceiveEvent, "NeedGreed", OnNeedGreedEvent);
    }
    
    public override void OnDisable()
        => Services.AddonLifecycle.UnregisterListener(OnNeedGreedSetup, OnNeedGreedEvent);

    private void OnNeedGreedSetup(AddonEvent type, AddonArgs args) {
        // Find first item that hasn't been rolled on, and select it.
        var addonNeedGreed = args.GetAddon<AddonNeedGreed>();
        foreach (var index in Enumerable.Range(0, addonNeedGreed->NumItems)) {
            if (addonNeedGreed->Items[index] is { Roll: 0, ItemId: not 0 }) {
                SelectItem(addonNeedGreed, index);
                break;
            }
        }
    }
    
    private void OnNeedGreedEvent(AddonEvent type, AddonArgs args) {
        if (args is not AddonReceiveEventArgs eventArgs) return;
        
        var eventType = (AtkEventType) eventArgs.AtkEventType;
        var buttonType = (ButtonType) eventArgs.EventParam;
        var addon = eventArgs.GetAddon<AddonNeedGreed>();
        
        if (eventType is not AtkEventType.ButtonClick) return;
        
        switch (buttonType) {
            // Fall through unconditionally
            case ButtonType.Need:
            case ButtonType.Greed:
            
            // Don't select next item if we are passing on an item that we already rolled on
            case ButtonType.Pass when addon->Items[addon->SelectedItemIndex] is { Roll: 0, ItemId: not 0 }: 
                var currentItemCount = addon->NumItems;
                var nextIndex = addon->SelectedItemIndex + 1;

                if (nextIndex < currentItemCount) {
                    SelectItem(addon, nextIndex);
                }
                break;
        }
    }
    
    private static void SelectItem(AddonNeedGreed* addon, int index) {
        var eventData = new AtkEventData();
        eventData.ListItemData.SelectedIndex = index;
        addon->ReceiveEvent(AtkEventType.ListItemClick, 0, null, &eventData);
    }
    
    // There are other button types such as "Greed Only" and "Loot Recipient"
    private enum ButtonType : uint {
        Need = 0,
        Greed = 1,
        Pass = 2,
    }
}


