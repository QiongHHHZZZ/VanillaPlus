using System.Collections.Generic;
using System.IO;
using Dalamud.Utility;
using Newtonsoft.Json.Linq;

namespace VanillaPlus.Core.Objects;

public class SimpleTweaksCompatabilityModule(string targetModuleName) : CompatabilityModule {
    protected override string TargetModule => targetModuleName;
    public override string TargetPluginInternalName => "SimpleTweaksPlugin";

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
        => Path.Combine(Services.PluginInterface.GetPluginConfigDirectory().Replace("VanillaPlus", "SimpleTweaksPlugin.json"));
    
    private FileInfo GetConfigFileInfo()
        => new(GetConfigFilePath());
}
