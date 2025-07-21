using System.Runtime.InteropServices;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using VanillaPlus.Core;
using VanillaPlus.Core.Objects;

namespace VanillaPlus.GameModifications.OpenGlamourDresserToCurrentJob;

public class OpenGlamourDresserToCurrentJob : GameModification {
    public override ModificationInfo ModificationInfo => new() {
        DisplayName = "Open GlamourDresser To Current Job",
        Description = "When opening the glamour dresser, the tab for your current job" +
                      "will be automatically selected",
        Type = ModificationType.GameBehavior,
        Authors = ["MidoriKami"],
        ChangeLog = [
            new ChangeLogInfo(1, "Initial Implementation"),
        ]
    };
    
    public override void OnEnable()
        => Services.AddonLifecycle.RegisterListener(AddonEvent.PreSetup, "MiragePrismPrismBox", OnGlamourDresserSetup);

    public override void OnDisable()
        => Services.AddonLifecycle.UnregisterListener(OnGlamourDresserSetup);

    private void OnGlamourDresserSetup(AddonEvent type, AddonArgs args) {
        if (Services.ClientState is { LocalPlayer.ClassJob.RowId: var playerJob }) {
            Marshal.WriteByte(args.Addon, 0x1A8, (byte)playerJob);
        }
    }
}
