using System.Collections.Generic;
using System.IO;
using Dalamud.Utility;
using Newtonsoft.Json.Linq;

namespace VanillaPlus.Classes;

public class HaselTweaksCompatabilityModule(string moduleName, IncompatibilityType type = IncompatibilityType.OldVersion) : CompatabilityModule(type) {
    public override string TargetModule => moduleName;
    public override string TargetPluginInternalName => "HaselTweaks";

    protected override List<string> GetTargetPluginLoadedModules() {
        var configFileInfo = GetConfigFileInfo();
        if (configFileInfo.Exists) {
            var fileText = File.ReadAllText(configFileInfo.FullName);

            if (fileText.IsNullOrEmpty()) return [];
            
            var jObject = JObject.Parse(fileText);
            if (!jObject.HasValues) return [];

            var enabledTweaksToken = jObject.GetValue("EnabledTweaks");
            if (enabledTweaksToken is null) return [];

            if (enabledTweaksToken.Type is JTokenType.Array) {
                var enabledTweaksList = enabledTweaksToken.ToObject<List<string>>();
                
                return enabledTweaksList ?? [];
            }
        }

        return [];
    }
    
    private string GetConfigFilePath()
        => Path.Combine(Services.PluginInterface.GetPluginConfigDirectory().Replace("VanillaPlus", "HaselTweaks.json"));
    
    private FileInfo GetConfigFileInfo()
        => new(GetConfigFilePath());
}
