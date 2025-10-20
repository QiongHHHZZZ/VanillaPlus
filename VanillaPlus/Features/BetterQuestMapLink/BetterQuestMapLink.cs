using System;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.BetterQuestMapLink;

public unsafe class BetterQuestMapLink : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "改进任务地图链接",
        Description = "点击任务链接时，直接打开对应区域的实际地图，而非默认的世界地图。",
        Type = ModificationType.GameBehavior,
        Authors = [ "MidoriKami" ],
        ChangeLog = [
            new ChangeLogInfo(1, "初始版本"),
        ],
        CompatibilityModule = new PluginCompatibilityModule("Mappy"),
    };

    private Hook<AgentMap.Delegates.OpenMap>? openMapHook;
    
    public override void OnEnable() {
        openMapHook = Services.Hooker.HookFromAddress<AgentMap.Delegates.OpenMap>(AgentMap.MemberFunctionPointers.OpenMap, OnOpenMap);
        openMapHook?.Enable();
    }

    public override void OnDisable() {
        openMapHook?.Dispose();
        openMapHook = null;
    }

    private void OnOpenMap(AgentMap* agent, OpenMapInfo* data) {
        openMapHook!.Original(agent, data);
        
        try {
            if (data->Type is MapType.QuestLog && agent->CurrentMapId != data->MapId) {
                data->Type = MapType.Centered;
                data->TerritoryId = 0;
                openMapHook!.Original(agent, data);
            }
        }
        catch (Exception e) {
            Services.PluginLog.Error(e, "打开地图时出现异常");
        }
    }
}

