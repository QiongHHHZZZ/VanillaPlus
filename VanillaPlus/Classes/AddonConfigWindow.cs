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

    public AddonConfigWindow(string windowName, AddonConfig config) : base($"{windowName} Window Configuration", ImGuiWindowFlags.AlwaysAutoResize) {
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
        ImGui.TextColored(KnownColor.Gray.Vector(), "Changes won't take effect until the window is reopened");
        
        ImGui.Spacing();
        if (ImGui.DragFloat2("Window Size", ref config.WindowSize)) {
            var xPos = Math.Max(50.0f, config.WindowSize.X);
            var yPos = Math.Max(50.0f, config.WindowSize.Y);
            
            config.WindowSize = new Vector2(xPos, yPos);
            config.Save();
        }
        
        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.Text("Keybind");
        ImGui.Separator();
        
        ImGuiHelpers.CenteredText(string.Join(" + ", config.OpenKeyCombo));
        
        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.Spacing();

        if (ImGui.Checkbox("Enable Keybind", ref config.KeybindEnabled)) {
            config.Save();
        }
        
        ImGui.SameLine(ImGui.GetContentRegionMax().X - 100.0f * ImGuiHelpers.GlobalScale);

        if (ImGui.Button("Edit Keybind", ImGuiHelpers.ScaledVector2(100.0f, 24.0f))) {
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
