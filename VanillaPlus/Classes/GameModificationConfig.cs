using System;
using System.Text.Json.Serialization;
using VanillaPlus.Utilities;

namespace VanillaPlus.Classes;

public abstract class GameModificationConfig<T> : ISavable where T : GameModificationConfig<T>, new() {
    protected abstract string FileName { get; }
    public static T Load() {
        var configFileName = new T().FileName;
        
        Services.PluginLog.Debug($"正在加载配置 {configFileName}");
        return Config.LoadConfig<T>(configFileName);
    } 
    
    public void Save() {
        Services.PluginLog.Debug($"正在保存配置 {FileName}");
        Config.SaveConfig(this, FileName);
        OnSave?.Invoke();
    }

    [JsonIgnore] public Action? OnSave { get; set; }
}
