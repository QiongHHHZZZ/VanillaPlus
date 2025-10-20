using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;

namespace VanillaPlus.Features.BetterCursor;

public class BetterCursorConfigWindow(BetterCursorConfig config, Action onConfigChanged) : Window("光标强化设置", ImGuiWindowFlags.AlwaysAutoResize) {
    public override void Draw() {
        if (ImGui.ColorEdit4("颜色", ref config.Color)) {
            onConfigChanged();
            config.Save();
        }

        if (ImGui.DragFloat("尺寸", ref config.Size)) {
            config.Size = Math.Max(0, config.Size);
            
            onConfigChanged();
            config.Save();
        }
        
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (ImGui.Checkbox("启用动画", ref config.Animations)) {
            onConfigChanged();
            config.Save();
        }

        if (ImGui.Checkbox("按住左右键时隐藏", ref config.HideOnCameraMove)) {
            config.Save();
        }
        
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (ImGui.Checkbox("仅在战斗中显示", ref config.OnlyShowInCombat)) {
            config.Save();
        }

        if (ImGui.Checkbox("仅在副本中显示", ref config.OnlyShowInDuties)) {
            config.Save();
        }
        
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (ImGui.InputUInt("图标 ID", ref config.IconId, 1)) {
            onConfigChanged();
            config.Save();
        }

        if (Services.TextureProvider.TryGetFromGameIcon(config.IconId, out var texture)) {
            ImGui.Image(texture.GetWrapOrEmpty().Handle, ImGuiHelpers.ScaledVector2(128.0f, 128.0f));
        }
        else {
            ImGui.Image(Services.TextureProvider.GetPlaceholderTexture().GetWrapOrEmpty().Handle, ImGuiHelpers.ScaledVector2(128.0f, 128.0f));
        }
    }

    public override void OnClose()
        => config.Save();
}
