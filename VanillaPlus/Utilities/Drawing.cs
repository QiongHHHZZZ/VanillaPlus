using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace VanillaPlus.Utilities;

public static class Drawing {
    public static bool IconButton(FontAwesomeIcon icon, string id = "") {
        using var pushedId = ImRaii.PushId(id);
        using var fixedFont = Services.PluginInterface.UiBuilder.IconFontFixedWidthHandle.Push();
        return ImGui.Button(icon.ToIconString(), ImGuiHelpers.ScaledVector2(25.0f, 25.0f));
    }
}
