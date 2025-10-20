using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dalamud.Plugin;
using VanillaPlus.Classes;

namespace VanillaPlus.InternalSystem;

public class ModificationManager : IDisposable {

    public readonly List<LoadedModification> LoadedModifications = [];

    public ModificationManager() {
        var allGameModifications = GetGameModifications();

        foreach (var gameMod in allGameModifications) {
            Services.PluginInterface.Inject(gameMod);

            var newLoadedModification = new LoadedModification(gameMod, LoadedState.Disabled);

            LoadedModifications.Add(newLoadedModification);

            if (System.SystemConfig.EnabledModifications.Contains(gameMod.Name)) {
                TryEnableModification(newLoadedModification);
            }
        }

        Services.PluginInterface.ActivePluginsChanged += OnPluginsChanged;
    }

    public void Dispose() {
        Services.PluginInterface.ActivePluginsChanged -= OnPluginsChanged;

        Services.PluginLog.Debug("正在释放修改管理器，当前将禁用所有功能模块");
        
        foreach (var loadedMod in LoadedModifications) {
            if (loadedMod.State is LoadedState.Enabled) {
                try {
                    Services.PluginLog.Debug($"正在禁用 {loadedMod.Name}");
                    loadedMod.Modification.OnDisable();
                    Services.PluginLog.Debug($"{loadedMod.Name} 已禁用");
                }
                catch (Exception e) {
                    Services.PluginLog.Error(e, $"卸载功能 {loadedMod.Name} 时出错");
                }
            }
        }
    }

    // When loaded plugins change, re-evaluate any compat modules
    private void OnPluginsChanged(IActivePluginsChangedEventArgs args)
        => ReloadConflictedModules();

    public void ReloadConflictedModules() {
        foreach (var gameModification in LoadedModifications) {

            // Only evaluate modules that have a compatability module
            if (gameModification.Modification.ModificationInfo.CompatibilityModule is not { } compatibilityModule) continue;

            switch (gameModification.State) {
                // If the module is currently enabled, check that it should stay enabled, if not disable it
                case LoadedState.Enabled:

                    // This module was enabled, but after a refresh it's not allowed, disable it
                    if (!compatibilityModule.ShouldLoadGameModification()) {
                        TryDisableModification(gameModification, false);
                        gameModification.State = LoadedState.CompatError;
                        gameModification.ErrorMessage = compatibilityModule.GetErrorMessage();
                    }
                    break;

                // If the module is disabled due to a compat error, re-evaluate if it can be enabled now
                case LoadedState.CompatError:

                    // This module was disabled due to compat, it is now allowed, load it
                    if (compatibilityModule.ShouldLoadGameModification()) {
                        TryEnableModification(gameModification);
                    }
                    break;
            }
        }

        System.AddonModificationBrowser.UpdateDisabledState();
    }

    public static void TryEnableModification(LoadedModification modification) {
        if (modification.State is LoadedState.Errored) {
            Services.PluginLog.Error("尝试启用已出错的功能模块");
            return;
        }

        try {
            if (modification.Modification.ModificationInfo.CompatibilityModule is { } compatibilityModule) {
                if (!compatibilityModule.ShouldLoadGameModification()) {
                    modification.State = LoadedState.CompatError;
                    modification.ErrorMessage = compatibilityModule.GetErrorMessage();
                    
                    Services.PluginLog.Warning(compatibilityModule.GetErrorMessage());
                    return;
                }
            }
            
            Services.PluginLog.Info($"正在启用 {modification.Name}");
            modification.Modification.OnEnable();
            modification.State = LoadedState.Enabled;
            Services.PluginLog.Info($"{modification.Name} 已启用");
            System.SystemConfig.EnabledModifications.Add(modification.Name);
            System.SystemConfig.Save();
        }
        catch (Exception e) {
            modification.State = LoadedState.Errored;
            modification.ErrorMessage = "加载失败，该模块已被禁用。";
            Services.PluginLog.Error(e, $"启用 {modification.Name} 时出错，正在尝试禁用");
            
            try {
                modification.Modification.OnDisable();
                Services.PluginLog.Information($"已成功禁用出错的功能模块 {modification.Name}");
            }
            catch (Exception fatal) {
                modification.ErrorMessage = "严重错误：模块加载失败，并在卸载时再次出错。";
                Services.PluginLog.Error(fatal, $"卸载出错模块 {modification.Name} 时发生严重错误");
            }
        }
    }

    public static void TryDisableModification(LoadedModification modification, bool removeFromList = true) {
        if (modification.State is LoadedState.Errored) {
            Services.PluginLog.Error("尝试禁用已出错的功能模块");
            return;
        }

        try {
            Services.PluginLog.Info($"正在禁用 {modification.Name}");
            modification.Modification.OnDisable();
            modification.Modification.OpenConfigAction = null;
        }
        catch (Exception e) {
            modification.State = LoadedState.Errored;
            Services.PluginLog.Error(e, $"无法禁用功能：{modification.Name}");
        } finally {
            modification.State = LoadedState.Disabled;
            Services.PluginLog.Info($"{modification.Name} 已禁用");

            if (removeFromList) {
                System.SystemConfig.EnabledModifications.Remove(modification.Name);
                System.SystemConfig.Save();
            }
        }
    }

    private List<GameModification> GetGameModifications() 
        => Assembly
           .GetCallingAssembly()
           .GetTypes()
           .Where(type => type.IsSubclassOf(typeof(GameModification)))
           .Where(type => !type.IsAbstract)
           .Select(type => (GameModification?) Activator.CreateInstance(type))
           .Where(modification => modification?.ModificationInfo.Type is not ModificationType.Hidden)
           .OfType<GameModification>()
           .ToList();
}
