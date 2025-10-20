using System.Numerics;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.DebugCustomAddon;

#if DEBUG
/// <summary>
/// Debug Game Modification with a Custom Addon for use with playing around with ideas, DO NOT commit changes to this file
/// </summary>
public class DebugCustomAddon : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "调试自定义界面",
        Description = "用于测试与尝试 VanillaPlus 功能的调试模块",
        Type = ModificationType.Debug,
        Authors = [ "开发者" ],
        ChangeLog = [
            new ChangeLogInfo(1, "初始实现"),
        ],
    };

    private DebugAddon? debugAddon;

    public override void OnEnable() {
        debugAddon = new DebugAddon {
            NativeController = System.NativeController,
            InternalName = "DebugAddon",
            Title = "调试插件窗口",
            Size = new Vector2(500.0f, 500.0f),
        };

        debugAddon.Open();

        OpenConfigAction = debugAddon.Toggle;
    }

    public override void OnDisable() {
        debugAddon?.Dispose();
        debugAddon = null;
    }
}
#endif


