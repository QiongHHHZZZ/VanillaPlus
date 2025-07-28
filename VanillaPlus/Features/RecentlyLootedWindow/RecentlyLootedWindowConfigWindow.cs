using System;
using System.Linq;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.UI;
using ImGuiNET;
using VanillaPlus.Utilities;

namespace VanillaPlus.RecentlyLootedWindow;

public class RecentlyLootedWindowConfigWindow(RecentlyLootedWindowConfig config) : Window("Recently Looted Window Config Window") {

    private SeVirtualKey newKey = SeVirtualKey.NO_KEY;
    
    public override void Draw() {
        ImGui.Text("Keybind for opening window");
        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(5.0f);
        
        foreach (var keybind in config.OpenKeyCombo.ToList()) {
            if (Drawing.IconButton(FontAwesomeIcon.Trash, keybind.ToString())) {
                config.OpenKeyCombo.Remove(keybind);
                config.Save();
            }
            
            ImGui.SameLine();
            
            ImGui.Text(keybind.ToString());
        }

        if (Drawing.IconButton(FontAwesomeIcon.Plus, "newKey")) {
            config.OpenKeyCombo.Add(newKey);
            config.Save();
        }
        
        ImGui.SameLine();

        DrawKeySelectCombo();
    }

    private void DrawKeySelectCombo() {
        using var dropDown = ImRaii.Combo("##newKeyCombo", newKey.ToString(), ImGuiComboFlags.HeightLarge | ImGuiComboFlags.PopupAlignLeft);
        if (!dropDown) return;

        foreach (var key in Enum.GetValues<SeVirtualKey>()) {
            if (ImGui.Selectable(key.ToString(), key == newKey)) {
                newKey = key;
            }
        }
    }

    public override void OnClose() {
        config.Save();
    }
}
