using System.Collections.Generic;
using System.Linq;

namespace VanillaPlus.Classes;

public abstract class CompatabilityModule(IncompatibilityType incompatibilityType) {
    public abstract string TargetModule { get; }
    public abstract string TargetPluginInternalName { get; }
    
    public bool ShouldLoadGameModification() {
        var plugin = Services.PluginInterface.InstalledPlugins.FirstOrDefault(installed => installed.InternalName == TargetPluginInternalName);
        
        // If the targetPlugin is not installed, we can enable our module
        if (plugin is null) return true;
        
        // If the targetPlugin is installed, but isn't loaded, we can enable our module
        if (!plugin.IsLoaded) return true;
        
        // If the targetPlugin is installed and enabled, then we need to check if out targetModule is enabled
        var modules = GetTargetPluginLoadedModules();

        // If the targetPlugin has our targetModule enabled, we cannot enable our version.
        if (modules.Contains(TargetModule)) return false;

        // Else, it is not enabled in target plugin, we are good to go.
        return true;
    }
    
    protected abstract List<string> GetTargetPluginLoadedModules();

    public string GetErrorMessage() {
        return incompatibilityType switch {
            IncompatibilityType.OldVersion => $"The original version of this feature is already active in {TargetPluginInternalName}.\n\n" +
                                              $"ID: {TargetModule}",
            
            IncompatibilityType.Crash => $"There is a known crash when {TargetModule} from {TargetPluginInternalName} is also enabled.",
            
            IncompatibilityType.Plugin => $"The original version of this feature is from a plugin that is currently active: {TargetPluginInternalName}.",

            _ => "ERROR: Type not set!",
        };
    }
}
