using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI;
using VanillaPlus.Core;

namespace VanillaPlus.FateListWindow;

public class FateListWindowConfig : GameModificationConfig<FateListWindowConfig> {
    protected override string FileName => "FateListWindowConfig.config.json";
    
    public HashSet<SeVirtualKey> OpenKeyCombo = [SeVirtualKey.MENU, SeVirtualKey.F];
    public Vector2? WindowPosition = null;
}
