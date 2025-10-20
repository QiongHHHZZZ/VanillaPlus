using VanillaPlus.Utilities;

namespace VanillaPlus.Classes;

public abstract class GameModificationData<T> where T : GameModificationData<T>, new() {
    protected abstract string FileName { get; }
    public static T Load() {
        var configFileName = new T().FileName;
        
        Services.PluginLog.Debug($"正在加载数据 {configFileName}");
        return Data.LoadData<T>(configFileName);
    } 
    
    public void Save() {
        Services.PluginLog.Debug($"正在保存数据 {FileName}");
        Data.SaveData(this, FileName);
    }
}
