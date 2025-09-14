namespace VanillaPlus.Utilities;

/// <summary>
/// Configuration File Utilities
/// </summary>
public static class Config {
    public static string ConfigPath => FileHelpers.GetFileInfo("Configs").FullName;
    public static string CharacterConfigPath => FileHelpers.GetFileInfo("Configs", FileHelpers.GetCharacterPath()).FullName;

    /// <summary>
    /// Loads a configuration file from PluginConfigs\VanillaPlus\Configs\{FileName}
    /// Creates a `new T()` if the file can't be loaded
    /// </summary>
    public static T LoadConfig<T>(string fileName) where T : new()
        => FileHelpers.LoadFile<T>(FileHelpers.GetFileInfo("Configs", fileName).FullName);
    
    /// <summary>
    /// Loads a character specific config file from PluginConfigs\VanillaPlus\Configs\{ContentId}\{FileName}
    /// Creates a `new T` if the file can't be loaded
    /// </summary>
    /// <remarks>Requires the character to be logged in</remarks>
    public static T LoadCharacterConfig<T>(string fileName) where T : new()
        => FileHelpers.LoadFile<T>(FileHelpers.GetFileInfo("Configs", FileHelpers.GetCharacterPath(), fileName).FullName);
    
    /// <summary>
    /// Saves a configuration file to PluginConfigs\VanillaPlus\Configs\{FileName}
    /// </summary>
    public static void SaveConfig<T>(T modificationConfig, string fileName)
        => FileHelpers.SaveFile(modificationConfig, FileHelpers.GetFileInfo("Configs", fileName).FullName);
    
    /// <summary>
    /// Saves a character specific config file to PluginConfigs\VanillaPlus\Configs\{ContentId}\{FileName}
    /// </summary>
    /// <remarks>Requires the character to be logged in</remarks>
    public static void SaveCharacterConfig<T>(T modificationConfig, string fileName)
        => FileHelpers.SaveFile(modificationConfig, FileHelpers.GetFileInfo("Configs", FileHelpers.GetCharacterPath(), fileName).FullName);
}

