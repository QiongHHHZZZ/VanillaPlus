using System;
using System.Drawing;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;

namespace VanillaPlus.Classes;

public class AddonConfigWindow : Window, IDisposable {
    private readonly AddonConfig config;
    private KeybindModal? keybindModal;

    public AddonConfigWindow(string windowName, AddonConfig config) : base($"{windowName} 窗口设置", ImGuiWindowFlags.AlwaysAutoResize) {
        this.config = config;
        
        keybindModal = new KeybindModal(windowName) {
            KeybindSetCallback = keyBind => {
                config.OpenKeyCombo = keyBind;
                config.Save();
            },
        };
        
        this.AddToWindowSystem();
    }

    public override void Draw() {
        ImGui.TextColored(KnownColor.Gray.Vector(), "变更将在重新打开窗口后生效");
        
        ImGui.Spacing();
        if (ImGui.DragFloat2("窗口大小", ref config.WindowSize)) {
            var xPos = Math.Max(50.0f, config.WindowSize.X);
            var yPos = Math.Max(50.0f, config.WindowSize.Y);
            
            config.WindowSize = new Vector2(xPos, yPos);
            config.Save();
        }
        
        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.Text("快捷键");
        ImGui.Separator();
        
        ImGuiHelpers.CenteredText(string.Join(" + ", config.OpenKeyCombo));
        
        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.Spacing();

        if (ImGui.Checkbox("启用快捷键", ref config.KeybindEnabled)) {
            config.Save();
        }
        
        ImGui.SameLine(ImGui.GetContentRegionMax().X - 100.0f * ImGuiHelpers.GlobalScale);

        if (ImGui.Button("编辑快捷键", ImGuiHelpers.ScaledVector2(100.0f, 24.0f))) {
            keybindModal?.Open();
        }
    }

    public override void OnClose() {
        config.Save();
    }

    public void Dispose() {
        this.RemoveFromWindowSystem();
        
        keybindModal?.Dispose();
        keybindModal = null;
    }
}
