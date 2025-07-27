using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using VanillaPlus.Core;
using VanillaPlus.Extensions;

namespace VanillaPlus.ClearSelectedDuties;

public class ClearSelectedDuties : GameModification {
    public override ModificationInfo ModificationInfo => new() {
        DisplayName = "Clear Selected Duties",
        Description = "When opening the Duty Finder, deselects any selected duties.",
        Authors = [ "MidoriKami" ],
        Type = ModificationType.GameBehavior,
        ChangeLog = [
            new ChangeLogInfo(1, "Initial Implementation"),
        ],
    };

    public override void OnEnable()
        => Services.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "ContentsFinder", OnContentsFinderSetup);

    public override void OnDisable()
        => Services.AddonLifecycle.UnregisterListener(OnContentsFinderSetup);

    private unsafe void OnContentsFinderSetup(AddonEvent type, AddonArgs args) {
        if (ContentsFinder.Instance()->QueueInfo.QueueState is not ContentsFinderQueueInfo.QueueStates.None)
            return;

        AgentContentsFinder.Instance()->AgentInterface.SendCommand(0, [ 12, 1 ]);
    }
}
