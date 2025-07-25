using System;
using System.IO;
using System.Text.Json;
using Dalamud.Utility;

namespace VanillaPlus.Utilities;

/// <summary>
/// Configuration File Utilities
/// </summary>
public static class Configuration {
    public static string ConfigPath => GetFileInfo("Configs").FullName;
    public static string CharacterConfigPath => GetFileInfo("Configs", GetCharacterPath()).FullName;

    public static string DataPath => GetFileInfo("Data").FullName;
    public static string CharacterDataPath => GetFileInfo("Data", GetCharacterPath()).FullName;

    /// <summary>
    /// Loads a configuration file from PluginConfigs\VanillaPlus\Configs\{FileName}
    /// Creates a `new T()` if the file can't be loaded
    /// </summary>
    public static T LoadConfig<T>(string fileName) where T : new()
        => LoadFile<T>(GetFileInfo("Configs", fileName).FullName);
    
    /// <summary>
    /// Loads a character specific config file from PluginConfigs\VanillaPlus\Configs\{ContentId}\{FileName}
    /// Creates a `new T` if the file can't be loaded
    /// </summary>
    /// <remarks>Requires the character to be logged in</remarks>
    public static T LoadCharacterConfig<T>(string fileName) where T : new()
        => LoadFile<T>(GetFileInfo("Configs", GetCharacterPath(), fileName).FullName);
    
    /// <summary>
    /// Saves a configuration file to PluginConfigs\VanillaPlus\Configs\{FileName}
    /// </summary>
    public static void SaveConfig<T>(T modificationConfig, string fileName)
        => SaveFile(modificationConfig, GetFileInfo("Configs", fileName).FullName);
    
    /// <summary>
    /// Saves a character specific config file to PluginConfigs\VanillaPlus\Configs\{ContentId}\{FileName}
    /// </summary>
    /// <remarks>Requires the character to be logged in</remarks>
    public static void SaveCharacterConfig<T>(T modificationConfig, string fileName)
        => SaveFile(modificationConfig, GetFileInfo("Configs", GetCharacterPath(), fileName).FullName);
    
    /// <summary>
    /// Loads a data file from PluginConfigs\VanillaPlus\Data\{FileName}
    /// </summary>
    public static T LoadData<T>(string fileName) where T : new()
        => LoadFile<T>(GetFileInfo("Data", fileName).FullName);
    
    /// <summary>
    /// Loads a character specific data file from PluginConfigs\VanillaPlus\Data\{ContentId}\{FileName}
    /// Creates a `new T` if the file can't be loaded
    /// </summary>
    /// <remarks>Requires the character to be logged in</remarks>
    public static T LoadCharacterData<T>(string fileName) where T : new()
        => LoadFile<T>(GetFileInfo("Data", GetCharacterPath(), fileName).FullName);

    /// <summary>
    /// Saves a data file to PluginConfigs\VanillaPlus\Data\{FileName}
    /// </summary>
    public static void SaveData<T>(T modificationConfig, string fileName)
        => SaveFile(modificationConfig, GetFileInfo("Data", fileName).FullName);
    
    /// <summary>
    /// Saves a character specific data file to PluginConfigs\VanillaPlus\Data\{ContentId}\{FileName}
    /// </summary>
    /// <remarks>Requires the character to be logged in</remarks>
    public static void SaveCharacterData<T>(T modificationConfig, string fileName)
        => SaveFile(modificationConfig, GetFileInfo("Data", GetCharacterPath(), fileName).FullName);

    private static FileInfo GetFileInfo(params string[] path) {
        var directory = Services.PluginInterface.ConfigDirectory;

        for (var index = 0; index < path.Length - 1; index++) {
            directory = new DirectoryInfo(Path.Combine(directory.FullName, path[index]));
            if (!directory.Exists) {
                directory.Create();
            }
        }

        return new FileInfo(Path.Combine(directory.FullName, path[^1]));
    }

    private static string GetCharacterPath() {
        if (!Services.ClientState.IsLoggedIn) {
            throw new Exception("Character is not logged in.");
        }
        
        return Services.ClientState.LocalContentId.ToString("X");
    }

    private static readonly JsonSerializerOptions SerializerOptions = new() {
        WriteIndented = true,
        IncludeFields = true,
    };
    
    private static T LoadFile<T>(string filePath) where T : new() {
        var fileInfo = new FileInfo(filePath);
        if (fileInfo is { Exists: true }) {
            try {
                var fileText = File.ReadAllText(fileInfo.FullName);
                var dataObject = JsonSerializer.Deserialize<T>(fileText, SerializerOptions);

                // If deserialize result is null, create a new instance instead and save it.
                if (dataObject is null) {
                    dataObject = new T();
                    SaveFile(dataObject, filePath);
                }
            
                return dataObject;
            }
            catch (Exception e) {
                // If there is any kind of error loading the file, generate a new one instead and save it.
                Services.PluginLog.Error(e, $"Error trying to load file {filePath}, creating a new one instead.");
            
                SaveFile(new T(), filePath);
            }
        }

        var newFile = new T();
        SaveFile(newFile, filePath);
    
        return newFile;
    }

    private static void SaveFile<T>(T? file, string filePath) {
        try {
            if (file is null) {
                Services.PluginLog.Error("Null file provided.");
                return;
            }
            
            var fileText = JsonSerializer.Serialize(file, file.GetType(), SerializerOptions);
            FilesystemUtil.WriteAllTextSafe(filePath, fileText);
        }
        catch (Exception e) {
            Services.PluginLog.Error(e, $"Error trying to save file {filePath}");
        }
    } 
}

