using System.Collections.Generic;
using System.Linq;

namespace VanillaPlus.Core;

public abstract class CompatabilityModule {
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
}
