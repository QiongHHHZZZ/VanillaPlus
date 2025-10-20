using System.Runtime.InteropServices;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.OpenGlamourDresserToCurrentJob;

public class OpenGlamourDresserToCurrentJob : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "衣柜定位当前职业",
        Description = "打开幻化衣柜时自动切换到当前职业的分页。",
        Type = ModificationType.GameBehavior,
        Authors = ["MidoriKami"],
        ChangeLog = [
            new ChangeLogInfo(1, "初始实现"),
        ],
        CompatibilityModule = new SimpleTweaksCompatibilityModule("UiAdjustments@OpenGlamourDresserToCurrentJob"),
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


