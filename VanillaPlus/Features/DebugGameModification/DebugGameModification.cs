using VanillaPlus.Classes;

namespace VanillaPlus.Features.DebugGameModification;

#if DEBUG
/// <summary>
/// Debug Game Modification for use with playing around with ideas, DO NOT commit changes to this file
/// </summary>
public class DebugGameModification : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "调试功能模块",
        Description = "用于测试和尝试 VanillaPlus 功能的调试模块",
        Type = ModificationType.Debug,
        Authors = [ "开发者" ],
        ChangeLog = [
            new ChangeLogInfo(1, "初始实现"),
        ],
    };

    public override void OnEnable() {
    }

    public override void OnDisable() {
    }
}
#endif


