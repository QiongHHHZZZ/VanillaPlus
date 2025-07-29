using VanillaPlus.Utilities;

namespace VanillaPlus.Classes;

public abstract class GameModificationConfig<T> where T : GameModificationConfig<T>, new() {
    protected abstract string FileName { get; }
    public static T Load() {
        var configFileName = new T().FileName;
        
        Services.PluginLog.Debug($"Loading Config {configFileName}");
        return Config.LoadConfig<T>(configFileName);
    } 
    
    public void Save() {
        Services.PluginLog.Debug($"Saving Config {FileName}");
        Config.SaveConfig(this, FileName);
    }
}
