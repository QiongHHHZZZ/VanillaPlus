using System.Collections.Generic;
using VanillaPlus.Utilities;

namespace VanillaPlus.Core.Objects;

public class SystemConfiguration {
    public int Version = 1;

    public HashSet<string> EnabledModifications = [];

    public static SystemConfiguration Load()
        => Configuration.LoadConfig<SystemConfiguration>("system.config.json");

    public void Save()
        => Configuration.SaveConfig(this, "system.config.json");
}
