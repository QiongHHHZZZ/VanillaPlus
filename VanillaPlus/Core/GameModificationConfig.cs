using VanillaPlus.Utilities;

namespace VanillaPlus.Core;

public abstract class GameModificationConfig<T> where T : GameModificationConfig<T>, new() {
    protected abstract string FileName { get; }
    public static T Load() => Config.LoadConfig<T>(new T().FileName);
    public void Save() => Config.SaveConfig(this, FileName);
}
