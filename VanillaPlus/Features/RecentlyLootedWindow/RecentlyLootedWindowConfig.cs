using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI;
using VanillaPlus.Core;

namespace VanillaPlus.RecentlyLootedWindow;

public class RecentlyLootedWindowConfig : GameModificationConfig<RecentlyLootedWindowConfig> {
    protected override string FileName => "RecentlyLootedWindow.config.json";

    public HashSet<SeVirtualKey> OpenKeyCombo = [SeVirtualKey.MENU, SeVirtualKey.L];
    public Vector2? WindowPosition = null;
}
