using VanillaPlus.Utilities;

namespace VanillaPlus.Core;

/// <summary>
/// For use with per-character settings. This will save a file in a character specific directory.
/// </summary>
/// <remarks>You must be logged in to load or save a character config.</remarks>
public abstract class GameModificationCharacterConfig<T> where T : GameModificationCharacterConfig<T>, new() {
    protected abstract string FileName { get; }
    public static T Load() => Config.LoadCharacterConfig<T>(new T().FileName);
    public void Save() => Config.SaveCharacterConfig(this, FileName);
}
