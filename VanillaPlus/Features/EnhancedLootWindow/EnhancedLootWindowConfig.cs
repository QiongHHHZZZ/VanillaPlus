using VanillaPlus.Utilities;

namespace VanillaPlus.Features.EnhancedLootWindow;

public class EnhancedLootWindowConfig {
    public bool MarkUnobtainableItems = true;
    public bool MarkAlreadyObtainedItems = true;

    public static EnhancedLootWindowConfig Load()
        => Configuration.LoadConfig<EnhancedLootWindowConfig>("EnhancedLootWindow.config.json");
    
    public void Save()
        => Configuration.SaveConfig(this, "EnhancedLootWindow.config.json");
}
