using Dalamud.Game.Addon.Events;
using Dalamud.Game.Addon.Events.EventDataTypes;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit;
using KamiToolKit.Classes;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.ClearFlag;

public unsafe class ClearFlag : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "清除旗标",
        Description = "允许你在小地图上点击右键，快速移除当前设置的旗标。",
        Type = ModificationType.GameBehavior,
        Authors = [ "MidoriKami" ],
        ChangeLog = [
            new ChangeLogInfo(1, "初始版本"),
        ],
    };

    private AddonController? minimapController;
    private IAddonEventHandle? minimapMouseClick;
    
    public override void OnEnable() {
        minimapController = new AddonController("_NaviMap");

        minimapController.OnAttach += addon => {
            var collisionNode = addon->GetNodeById<AtkCollisionNode>(19);
            if (collisionNode is null) return;

            collisionNode->DrawFlags |= (uint)DrawFlags.ClickableCursor;

            minimapMouseClick = Services.AddonEventManager.AddEvent((nint)addon, (nint)collisionNode, AddonEventType.MouseClick, OnMiniMapMouseClick);
        };

        minimapController.OnDetach += addon => {
            Services.AddonEventManager.RemoveEventNullable(minimapMouseClick);
            
            var collisionNode = addon->GetNodeById<AtkCollisionNode>(19);
            if (collisionNode is null) return;

            collisionNode->DrawFlags &= ~(uint)DrawFlags.ClickableCursor;
            
        };

        minimapController.Enable();
    }

    public override void OnDisable() {
        minimapController?.Dispose();
        minimapController = null;
    }

    private static void OnMiniMapMouseClick(AddonEventType atkEventType, AddonEventData data) {
        if (data.IsRightClick() && AgentMap.Instance()->FlagMarkerCount is not 0) {
            AgentMap.Instance()->FlagMarkerCount = 0;
        }
    }
}


