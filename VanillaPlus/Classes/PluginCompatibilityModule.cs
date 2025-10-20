namespace VanillaPlus.Classes;

public class PluginCompatibilityModule(params string[] pluginNames) : CompatibilityModule {

    private string erroringPluginName = string.Empty;
    
    public override bool ShouldLoadGameModification() {
        foreach (var pluginName in pluginNames) {
            if (IsPluginLoaded(pluginName)) {
                erroringPluginName = pluginName;
                return false;
            }
        }

        return true;
    }

    public override string GetErrorMessage()
        => $"已有插件提供了该功能的原版实现：{erroringPluginName}（目前处于启用状态）。";
}
