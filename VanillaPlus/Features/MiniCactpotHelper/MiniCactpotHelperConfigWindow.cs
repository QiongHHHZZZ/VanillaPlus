using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;

namespace VanillaPlus.Features.MiniCactpotHelper;

public class MiniCactpotHelperConfigWindow(MiniCactpotHelperConfig config, Action onConfigChanged) : Window("仙人微彩助手设置", ImGuiWindowFlags.AlwaysAutoResize) {
    public override void Draw() {
        DrawAnimationConfig();
        DrawIconConfig();
        DrawColorConfig();
    }
    
    private void DrawAnimationConfig() {
        DrawHeader("动画");
        
        if (ImGui.Checkbox("启用动画", ref config.EnableAnimations)) {
            onConfigChanged();
            config.Save();
        }
    }
	
    private void DrawIconConfig() {
        DrawHeader("图标");
        
        if (GameIconButton(61332)) {
            config.IconId = 61332;
            onConfigChanged();
            config.Save();
        }

        ImGui.SameLine();
		
        if (GameIconButton(90452)) {
            config.IconId = 90452;
            onConfigChanged();
            config.Save();
        }
		
        ImGui.SameLine();
		
        if (GameIconButton(234008)) {
            config.IconId = 234008;
            onConfigChanged();
            config.Save();
        }
		
        ImGui.Spacing();
		
        ImGui.AlignTextToFramePadding();
        ImGui.Text("图标 ID：");
		
        ImGui.SameLine();
		
        var iconId = (int) config.IconId;
        if (ImGui.InputInt("##IconId", ref iconId)) {
            config.IconId = (uint) iconId;
            onConfigChanged();
            config.Save();
        }
    }
	
    private void DrawColorConfig() {
        DrawHeader("颜色");

        if (ImGui.ColorEdit4("按钮颜色", ref config.ButtonColor, ImGuiColorEditFlags.AlphaPreviewHalf)) {
            onConfigChanged();
            config.Save();
        }

        if (ImGui.ColorEdit4("路线颜色", ref config.LaneColor, ImGuiColorEditFlags.AlphaPreviewHalf)) {
            onConfigChanged();
            config.Save();
        }
    }

    private void DrawHeader(string text) {
        ImGuiHelpers.ScaledDummy(10.0f);
        ImGui.Text(text);
        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(5.0f);
    }
    
    private bool GameIconButton(uint iconId) {
        var iconTexture = Services.TextureProvider.GetFromGameIcon(iconId);
        
        return ImGui.ImageButton(iconTexture.GetWrapOrEmpty().Handle, new Vector2(48.0f, 48.0f));
    }
        
    public override void OnClose()
        => config.Save();
}
