using VanillaPlus.Classes;

namespace VanillaPlus.Features.SampleGameModification;

// Template GameModification for more easily creating your own, can copy this entire folder and rename it.
public class SampleGameModification : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "示例功能模块",
        Description = "用于演示如何创建自定义 GameModification 的模板。",
        Type = ModificationType.Hidden,
        Authors = [ "你的名字" ],
        ChangeLog = [
            new ChangeLogInfo(1, "初始版本"),
        ],
    };

    // public override string ImageName => "SampleGameModification.png";

    // public override bool IsExperimental => true;

    public override void OnEnable() {
    }

    public override void OnDisable() {
    }
}


