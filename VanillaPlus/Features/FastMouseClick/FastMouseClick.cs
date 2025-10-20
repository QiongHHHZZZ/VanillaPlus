using Dalamud.Utility.Signatures;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.FastMouseClick;

public class FastMouseClick : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "快速单击修复",
        Description = "游戏在检测到双击时不会触发单击事件。\n\n" +
                      "该功能会在触发双击的同时强制触发一次普通单击事件。",
        Type = ModificationType.GameBehavior,
        Authors = ["Haselnussbomber"],
        ChangeLog = [
            new ChangeLogInfo(1, "初始版本"),
        ],
        CompatibilityModule = new HaselTweaksCompatibilityModule("FastMouseClickFix"),
    };

    [Signature("EB 3F B8 ?? ?? ?? ?? 48 8B D7")]
    private nint? memoryAddress;

    private MemoryReplacement? memoryPatch;

    public override void OnEnable() {
        Services.GameInteropProvider.InitializeFromAttributes(this);
        
        if (memoryAddress is { } address && memoryAddress != nint.Zero) {
            memoryPatch = new MemoryReplacement(address, [0x90, 0x90]);
            memoryPatch.Enable();
        }
    }

    public override void OnDisable() {
        memoryPatch?.Dispose();
        memoryPatch = null;

        memoryAddress = null;
    }
}

