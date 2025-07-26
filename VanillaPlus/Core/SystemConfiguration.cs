using System.Collections.Generic;
using System.Numerics;
using VanillaPlus.Utilities;

namespace VanillaPlus.Core;

public class SystemConfiguration {
    public int Version = 1;

    public HashSet<string> EnabledModifications = [];
    public Vector2? ChangelogWindowPosition;
    public Vector2? BrowserWindowPosition;

    public static SystemConfiguration Load()
        => Config.LoadConfig<SystemConfiguration>("system.config.json");

    public void Save()
        => Config.SaveConfig(this, "system.config.json");
}
