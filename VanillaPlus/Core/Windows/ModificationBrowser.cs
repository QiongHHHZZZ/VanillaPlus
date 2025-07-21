using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Addon;
using VanillaPlus.Core.Windows.Parts;

namespace VanillaPlus.Core.Windows;

public class ModificationBrowser : NativeAddon {
    protected override unsafe void OnSetup(AtkUnitBase* addon) {
        AttachNode(new ModificationBrowserNode {
            Position = ContentStartPosition,
            Size = ContentSize,
            IsVisible = true,
        });
    }
}
