using Dalamud.Interface.Windowing;
using KamiToolKit;
using VanillaPlus.Core;
using VanillaPlus.Core.Objects;
using VanillaPlus.Core.Windows;

namespace VanillaPlus;

public static class System {
    public static SystemConfiguration SystemConfig { get; set; } = null!;
    public static NativeController NativeController { get; set; } = null!;
    public static WindowSystem WindowSystem { get; set; } = null!;
    public static ModificationBrowser AddonModificationBrowser { get; set; } = null!;
    public static ModificationManager ModificationManager { get; set; } = null!;
}
