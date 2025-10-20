using System;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.SkipTeleportConfirm;

public unsafe class SkipTeleportConfirm : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "跳过传送确认",
        Description = "使用地图传送时自动跳过“是否花费××金币传送？”的确认弹窗。",
        Type = ModificationType.GameBehavior,
        Authors = [ "MidoriKami" ],
        ChangeLog = [
            new ChangeLogInfo(1, "初始版本"),
        ],
    };

    private Hook<AgentInterface.Delegates.ReceiveEvent>? mapReceiveEventHook;

    public override void OnEnable() {
        mapReceiveEventHook = Services.Hooker.HookFromAddress<AgentInterface.Delegates.ReceiveEvent>(AgentMap.Instance()->VirtualTable->ReceiveEvent, OnAgentMapReceiveEvent);
        mapReceiveEventHook?.Enable();
    }

    public override void OnDisable() {
        mapReceiveEventHook?.Dispose();
        mapReceiveEventHook = null;
    }

    private AtkValue* OnAgentMapReceiveEvent(AgentInterface* thisPtr, AtkValue* returnValue, AtkValue* values, uint valueCount, ulong eventKind) {
        var result = mapReceiveEventHook!.Original(thisPtr, returnValue, values, valueCount, eventKind);

        try {
            if (valueCount is 2 && values[0].Int is 7) {
                var addon = Services.GameGui.GetAddonByName<AddonSelectYesno>("SelectYesno");
                if (addon is not null) {
                    var newValues = stackalloc AtkValue[1];
                    newValues->SetInt(0);

                    addon->FireCallback(1, newValues, true);
                }
            }
        }
        catch (Exception e) {
            Services.PluginLog.Error(e, "处理 OnAgentMapReceiveEvent 时出现异常");
        }

        return result;
    }
}


