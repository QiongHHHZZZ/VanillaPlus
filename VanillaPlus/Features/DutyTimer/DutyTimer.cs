using System;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.DutyTimer;

public class DutyTimer : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "副本计时器",
        Description = "完成副本时，将耗时输出到聊天栏，便于记录。",
        Authors = [ "MidoriKami" ],
        Type = ModificationType.GameBehavior,
        ChangeLog = [
            new ChangeLogInfo(1, "初始实现"),
        ],
        CompatibilityModule = new SimpleTweaksCompatibilityModule("DutyTimer"),
    };

    private DateTime startTimestamp;

    public override void OnEnable() {
        Services.DutyState.DutyStarted += OnDutyStarted;
        Services.DutyState.DutyCompleted += OnDutyCompleted;
        Services.ClientState.TerritoryChanged += OnTerritoryChanged;
    }

    public override void OnDisable() {
        Services.DutyState.DutyStarted -= OnDutyStarted;
        Services.DutyState.DutyCompleted -= OnDutyCompleted;
        Services.ClientState.TerritoryChanged -= OnTerritoryChanged;
    }

    private void OnDutyStarted(object? sender, ushort e)
        => startTimestamp = DateTime.UtcNow;

    private void OnDutyCompleted(object? sender, ushort e)
        => Services.ChatGui.Print($@"副本完成耗时：{DateTime.UtcNow - startTimestamp:hh\:mm\:ss\.ffff}");

    private void OnTerritoryChanged(ushort obj)
        => startTimestamp = DateTime.UtcNow;
}


