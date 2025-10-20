using Dalamud.Game.ClientState.Objects.Types;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.FocusTargetLock;

public class FocusTargetLock : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "焦点目标恢复",
        Description = "副本重新开始时自动恢复之前设置的焦点目标。",
        Type = ModificationType.GameBehavior,
        Authors = [ "MidoriKami" ],
        ChangeLog = [
            new ChangeLogInfo(1, "初始版本"),
        ],
    };

    private IGameObject? focusTarget;

    public override void OnEnable() {
        Services.DutyState.DutyWiped += OnDutyWiped;
        Services.DutyState.DutyRecommenced += OnDutyRecommenced;
        Services.ClientState.TerritoryChanged += OnTerritoryChanged;
    }

    public override void OnDisable() {
        Services.DutyState.DutyWiped -= OnDutyWiped;
        Services.DutyState.DutyRecommenced -= OnDutyRecommenced;
        Services.ClientState.TerritoryChanged -= OnTerritoryChanged;
    }

    private void OnDutyRecommenced(object? sender, ushort e)
        => Services.TargetManager.FocusTarget = focusTarget;

    private void OnDutyWiped(object? sender, ushort e)
        => focusTarget = Services.TargetManager.FocusTarget;

    private void OnTerritoryChanged(ushort obj)
        => focusTarget = null;
}


