using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.HideGuildhestObjectivePopup;

public class HideGuildhestObjectivePopup : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "隐藏行会令提示窗口",
        Description = "进入行会令时阻止弹出的教学提示窗口。\n\n首次参与行会令时，建议暂时禁用该功能。",
        Authors = ["MidoriKami"],
        Type = ModificationType.GameBehavior,
        ChangeLog = [
            new ChangeLogInfo(1, "初始实现"),
        ],
        CompatibilityModule = new SimpleTweaksCompatibilityModule("UiAdjustments@HideGuildhestObjectivePopup"),
    };

    public override string ImageName => "HideGuildhestObjective.png";

    public override void OnEnable()
        => Services.AddonLifecycle.RegisterListener(AddonEvent.PreSetup, "JournalAccept", OnJournalAcceptOpen);

    public override void OnDisable()
        => Services.AddonLifecycle.UnregisterListener(OnJournalAcceptOpen);

    private unsafe void OnJournalAcceptOpen(AddonEvent type, AddonArgs args) {
        if (Services.DataManager.GetExcelSheet<TerritoryType>().GetRow(Services.ClientState.TerritoryType) is not { TerritoryIntendedUse.RowId: 3 }) return;

        args.GetAddon<AtkUnitBase>()->Hide(false, false, 1);
    }
}


