using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.UI;
using ImGuiNET;
using VanillaPlus.Utilities;

namespace VanillaPlus.FateListWindow;

public class FateListWindowConfigWindow(FateListWindowConfig config) : Window("Fate List Window Config Window") {
    private SeVirtualKey newKey = SeVirtualKey.NO_KEY;

    public override void Draw() {
        ImGui.Text("Keybind for opening window");
        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(5.0f);

        if (Drawing.KeybindConfig(config.OpenKeyCombo, ref newKey)) {
            config.Save();
        }
    }

    public override void OnClose()
        => config.Save();
}
