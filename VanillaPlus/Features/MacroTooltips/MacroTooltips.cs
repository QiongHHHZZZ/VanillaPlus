using System;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.GUI;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.MacroTooltips;

/// <summary>
/// Debug Game Modification for use with playing around with ideas, DO NOT commit changes to this file
/// </summary>
public unsafe class MacroTooltips : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "宏命令技能提示",
        Description = "当宏使用“/macroicon”指定了某个技能时，鼠标悬停即可显示对应技能的提示。",
        Type = ModificationType.UserInterface,
        Authors = [ "MidoriKami" ],
        ChangeLog = [
            new ChangeLogInfo(1, "初始实现"),
        ],
    };

    private Hook<AddonActionBarBase.Delegates.ShowTooltip>? showTooltipHook;
    
    public override string ImageName => "MacroTooltips.png";

    public override void OnEnable() {
        showTooltipHook = Services.Hooker.HookFromAddress<AddonActionBarBase.Delegates.ShowTooltip>(AddonActionBarBase.MemberFunctionPointers.ShowTooltip, OnShowMacroTooltip);
        showTooltipHook?.Enable();
    }

    public override void OnDisable() {
        showTooltipHook?.Dispose();
        showTooltipHook = null;
    }

    private void OnShowMacroTooltip(AddonActionBarBase* a1, AtkResNode* macroResNode, NumberArrayData* numberArray, StringArrayData* stringArray, int numberArrayIndex, int stringArrayIndex) {
        showTooltipHook!.Original(a1, macroResNode, numberArray, stringArray, numberArrayIndex, stringArrayIndex);

        try {
            // In ActionBarNumberArray, the first hotbar starts at index 15
            var realSlotId = (numberArrayIndex - 15) % 16;
            var realHotbarId = (numberArrayIndex - 15) / 272;
            var originalTooltip = stringArray->StringArray[stringArrayIndex];

            // When using a shared pet/accessory hotbar, the hotbar id will be out of range
            // These slots can't have macros, so we will ignore them entirely
            if (realHotbarId >= RaptureHotbarModule.Instance()->Hotbars.Length) return;
            var hotbarSlot = RaptureHotbarModule.Instance()->Hotbars[realHotbarId].Slots[realSlotId];

            if (hotbarSlot is { CommandType: RaptureHotbarModule.HotbarSlotType.Macro, ApparentSlotType: RaptureHotbarModule.HotbarSlotType.Action }) {

                var isShared = (hotbarSlot.CommandId & 0x100) > 0;
                var macroIndex = hotbarSlot.CommandId & 0xFF;
                var slotType = stackalloc RaptureHotbarModule.HotbarSlotType[1];
                var rowId = stackalloc uint[1];
                var itemId = stackalloc uint[1];

                RaptureMacroModule.Instance()->TryResolveMacroIcon(UIModule.Instance(), slotType, rowId, isShared ? 1 : 0, macroIndex, itemId);

                macroResNode->ShowActionTooltip(*rowId, originalTooltip);
            }
        }
        catch (Exception e) {
            Services.PluginLog.Error(e, "显示宏提示时出现异常");
        }
    }
}


