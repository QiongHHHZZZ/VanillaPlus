using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace VanillaPlus.Features.TargetCastBarCountdown;

public class TargetCastBarCountdownConfigWindow : Window {
    private readonly TargetCastBarCountdownConfig config;
    private readonly Action drawNodeConfigs;
    private readonly Action saveNodeStyle;

    public TargetCastBarCountdownConfigWindow(TargetCastBarCountdownConfig config, Action drawNodeConfigs, Action saveNodeStyle) : base("目标读条倒计时设置") {
        this.config = config;
        this.drawNodeConfigs = drawNodeConfigs;
        this.saveNodeStyle = saveNodeStyle;
        
        SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(650.0f, 800.0f),
        };
    }

    public override void Draw() {
        if (ImGui.Checkbox("显示在主目标读条", ref config.PrimaryTarget)) config.Save();
        if (ImGui.Checkbox("显示在焦点目标读条", ref config.FocusTarget)) config.Save();
        if (ImGui.Checkbox("显示在敌对名牌读条", ref config.NamePlateTargets)) config.Save();
        
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
        
        drawNodeConfigs();
    }

    public override void OnClose() {
        config.Save();
        saveNodeStyle();
    }
}
