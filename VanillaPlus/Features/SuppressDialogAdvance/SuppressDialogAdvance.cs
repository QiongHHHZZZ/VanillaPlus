using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.SuppressDialogAdvance;

public unsafe class SuppressDialogueAdvance : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "防止对话误跳过",
        Description = "禁止过场对白被误跳过，只有点击对话框本体才会推进。",
        Type = ModificationType.GameBehavior,
        Authors = [ "MidoriKami" ],
        ChangeLog = [
            new ChangeLogInfo(1, "初始版本"),
        ],
    };

    public override void OnEnable()
        => Services.AddonLifecycle.RegisterListener(AddonEvent.PreReceiveEvent, "Talk", OnTalkReceiveEvent);

    public override void OnDisable()
        => Services.AddonLifecycle.UnregisterListener(OnTalkReceiveEvent);

    private static void OnTalkReceiveEvent(AddonEvent type, AddonArgs args) {
        if (args is not AddonReceiveEventArgs eventArgs) return;

        if ((AtkEventType)eventArgs.AtkEventType is AtkEventType.MouseClick) {
            var addon = args.GetAddon<AddonTalk>();
            var inputData = (AtkEventData*)eventArgs.Data;
            var mouseData = inputData->MouseData;

            if (!addon->RootNode->CheckCollisionAtCoords(mouseData.PosX, mouseData.PosY, true)) {
                eventArgs.AtkEventType = 0;
            }
        }
    }
}


