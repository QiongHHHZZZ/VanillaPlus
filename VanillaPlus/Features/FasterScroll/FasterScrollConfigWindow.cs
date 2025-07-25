using System.Drawing;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace VanillaPlus.FasterScroll;

public class FasterScrollConfigWindow(FasterScrollConfig config) : Window("Faster Scroll Config", ImGuiWindowFlags.AlwaysAutoResize) {
    public override void Draw() {
        ImGui.TextColored(KnownColor.Gray.Vector(), "Tip: double click to input exact value");
        
        ImGui.DragFloat("Speed Multiplier", ref config.SpeedMultiplier, 0.01f, 0.25f, 5.0f);
        if (ImGui.IsItemDeactivatedAfterEdit()) {
            config.Save();
        }
    }
}
