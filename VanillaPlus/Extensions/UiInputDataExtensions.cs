using FFXIVClientStructs.FFXIV.Client.UI;

namespace VanillaPlus.Extensions;

public static class UiInputDataExtensions {
    public static bool IsComboPressed(this ref UIInputData uiInputData, params SeVirtualKey[] keys) {
        if (keys.Length is 0) return false;
        
        foreach (var key in keys) {
            if (!uiInputData.IsKeyDown(key)) {
                return false;
            }
        }

        return true;
    }
}
