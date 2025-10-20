using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using VanillaPlus.Classes;
using VanillaPlus.NativeElements.Config;
using Action = Lumina.Excel.Sheets.Action;
using ActionBarSlotNumberArray = FFXIVClientStructs.FFXIV.Client.UI.Arrays.ActionBarNumberArray.ActionBarBarNumberArray.ActionBarSlotNumberArray;

namespace VanillaPlus.Features.FadeUnavailableActions;

public unsafe class FadeUnavailableActions : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "不可用技能淡化",
        Description = "当技能因资源不足、距离或冷却无法施放时，自动淡化对应热键。\n\n" +
                      "对于因降同步而不可用的技能也会进行淡化处理。",
        Authors = ["MidoriKami"],
        Type = ModificationType.UserInterface,
        ChangeLog = [
            new ChangeLogInfo(1, "初始实现"),
        ],
        CompatibilityModule = new SimpleTweaksCompatibilityModule("UiAdjustments@FadeUnavailableActions"),
    };

    private Hook<AddonActionBarBase.Delegates.UpdateHotbarSlot>? onHotBarSlotUpdateHook;

    private Dictionary<uint, Action?>? actionCache;

    private FadeUnavailableActionsConfig? config;
    private ConfigAddon? configWindow;

    public override string ImageName => "FadeUnavailableActions.png";

    public override void OnEnable() {
        actionCache = [];

        config = FadeUnavailableActionsConfig.Load();
        configWindow = new ConfigAddon {
            NativeController = System.NativeController,
            Size = new Vector2(400.0f, 250.0f),
            InternalName = "FadeUnavailableConfig",
            Title = "技能淡化设置",
            Config = config,
        };

        configWindow.AddCategory("样式设置")
            .AddIntSlider("淡化百分比", 0, 90, nameof(config.FadePercentage))
            .AddIntSlider("变红程度", 5, 100,  nameof(config.ReddenPercentage));

        configWindow.AddCategory("功能开关")
            .AddCheckbox("对技能框体也应用透明度", nameof(config.ApplyToFrame))
            .AddCheckbox("仅影响降同步技能", nameof(config.ApplyToSyncActions))
            .AddCheckbox("距离不足时转为红色", nameof(config.ReddenOutOfRange));
        
        OpenConfigAction = configWindow.Toggle;

        onHotBarSlotUpdateHook = Services.Hooker.HookFromAddress<AddonActionBarBase.Delegates.UpdateHotbarSlot>(AddonActionBarBase.MemberFunctionPointers.UpdateHotbarSlot, OnHotBarSlotUpdate);
        onHotBarSlotUpdateHook?.Enable();
    }

    public override void OnDisable() {
        onHotBarSlotUpdateHook?.Dispose();
        onHotBarSlotUpdateHook = null;
        
        configWindow?.Dispose();
        configWindow = null;

        actionCache = null;
        
        ResetAllHotbars();
    }
    
    private void OnHotBarSlotUpdate(AddonActionBarBase* addon, ActionBarSlot* hotBarSlotData, NumberArrayData* numberArray, StringArrayData* stringArray, int numberArrayIndex, int stringArrayIndex) {
        try {
            ProcessHotBarSlot(hotBarSlotData, numberArray, numberArrayIndex);
        }
        catch (Exception e) {
            Services.PluginLog.Error(e, "处理 FadeUnavailableActions 时发生异常，请联系 MidoriKami。");
        } finally {
            onHotBarSlotUpdateHook!.Original(addon, hotBarSlotData, numberArray, stringArray, numberArrayIndex, stringArrayIndex);
        }
    }
    
    private void ProcessHotBarSlot(ActionBarSlot* hotBarSlotData, NumberArrayData* numberArray, int numberArrayIndex) {
        if (config is null) return;
        if (Services.ClientState.LocalPlayer is { IsCasting: true } ) return;

        var numberArrayData = (ActionBarSlotNumberArray*) (&numberArray->IntArray[numberArrayIndex]);

        if ((NumberArrayActionType)numberArrayData->ActionType is not (NumberArrayActionType.Action or NumberArrayActionType.CraftAction)) {
            ApplyColoring(hotBarSlotData, false, false);
            return;
        }

        if (config.ApplyToSyncActions) {
            var action = GetAction(numberArrayData->ActionId);

            var actionLevel = action?.ClassJobLevel ?? 0;
            var playerLevel = Services.ClientState.LocalPlayer?.Level ?? 0;

            switch (action) {
                case null:
                    ApplyColoring(hotBarSlotData, false, false);
                    break;
                
                case { IsRoleAction: false } when actionLevel > playerLevel:
                    ApplyColoring(hotBarSlotData, false, true);
                    break;
                
                default:
                    ApplyColoring(hotBarSlotData, false, false);
                    break;
            }
        }
        else {
            ApplyColoring(hotBarSlotData, !numberArrayData->InRange, ShouldFadeAction(numberArrayData));
        }
    }

    private Action? GetAction(uint actionId) {
        var adjustedActionId = ActionManager.Instance()->GetAdjustedActionId(actionId);

        if (actionCache?.TryGetValue(adjustedActionId, out var action) ?? false) return action;

        action = Services.DataManager.GetExcelSheet<Action>().GetRowOrDefault(adjustedActionId);
        actionCache?.Add(adjustedActionId, action);
        return action;
    }

    private bool ShouldFadeAction(ActionBarSlotNumberArray* numberArrayData) 
        => !(numberArrayData->Executable && numberArrayData->Executable2);

    private void ApplyColoring(ActionBarSlot* hotBarSlotData, bool redden, bool fade) {
        if (config is null) return;
        if (hotBarSlotData is null) return;

        var icon = hotBarSlotData->GetImageNode();
        var frame = hotBarSlotData->GetFrameNode();
        
        if (icon is null || frame is null) return;

        icon->Color.R = 0xFF;
        icon->Color.G = config.ReddenOutOfRange && redden ? (byte)(0xFF * ((100 - config.ReddenPercentage) / 100.0f)) : (byte)0xFF;
        icon->Color.B = config.ReddenOutOfRange && redden ? (byte)(0xFF * ((100 - config.ReddenPercentage) / 100.0f)) : (byte)0xFF;
        icon->Color.A = fade ? (byte)(0xFF * ((100 - config.FadePercentage) / 100.0f)) : (byte)0xFF;

        frame->Color.A = fade ? config.ApplyToFrame ? (byte)(0xFF * ((100 - config.FadePercentage) / 100.0f)) : (byte) 0xFF : (byte)0xFF;
    }

    private static void ResetAllHotbars() {
        foreach (var addon in RaptureAtkUnitManager.Instance()->AllLoadedUnitsList.Entries) {
            if (addon.Value is null) continue;
            if (addon.Value->NameString.Contains("_Action") && !addon.Value->NameString.Contains("Contents")) {
                var actionBar = (AddonActionBarBase*)addon.Value;
                if (actionBar is null) continue;
                if (actionBar->ActionBarSlotVector.First is null) continue;

                foreach (var slot in actionBar->ActionBarSlotVector) {
                    if (slot.Icon is not null) {
                        var iconComponent = (AtkComponentIcon*) slot.Icon->Component;
                        if (iconComponent is null) continue;

                        iconComponent->IconImage->Color = Vector4.One.ToByteColor();
                        iconComponent->Frame->Color = Vector4.One.ToByteColor();
                    }
                }
            }
        }
    }

    private enum NumberArrayActionType : uint {
        Action = 0x2E,
        CraftAction = 0x36,
    }
}


