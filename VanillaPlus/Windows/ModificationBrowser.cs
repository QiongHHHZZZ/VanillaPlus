using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Addon;
using VanillaPlus.Windows.Parts;

namespace VanillaPlus.Windows;

public class ModificationBrowser : NativeAddon {
    protected override unsafe void OnSetup(AtkUnitBase* addon) {
        AttachNode(new ModificationBrowserNode {
            Position = ContentStartPosition,
            Size = ContentSize,
            IsVisible = true,
        });
    }

    protected override unsafe void OnHide(AtkUnitBase* addon) {
        System.SystemConfig.BrowserWindowPosition = Position;
        System.SystemConfig.Save();
    }
}
