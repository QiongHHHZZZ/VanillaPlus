using System.Collections.Generic;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace VanillaPlus.Extensions;

public static class KeyStateExtensions {
    public static bool IsKeybindPressed(this IKeyState keyState, IEnumerable<SeVirtualKey> keys) {
        foreach (var key in keys) {
            if (!keyState[(int)key]) {
                return false;
            }
        }

        return true;
    }
    
    public static void ResetKeyCombo(this IKeyState keyState, IEnumerable<SeVirtualKey> keys) {
        foreach(var key in keys){
            if (key is SeVirtualKey.CONTROL or SeVirtualKey.MENU or SeVirtualKey.SHIFT) continue;
            
            keyState[(int)key] = false;
        }
    }
}
