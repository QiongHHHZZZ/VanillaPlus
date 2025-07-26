using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VanillaPlus.Core;

public class ModificationManager : IDisposable {

    public readonly List<LoadedModification> LoadedModifications = [];
    private readonly List<GameModification> gameModifications;

    public ModificationManager() {
        gameModifications = GetGameModifications();

        foreach (var gameMod in gameModifications) {
            Services.PluginInterface.Inject(gameMod);

            var newLoadedModification = new LoadedModification(gameMod, LoadedState.Disabled);
            
            LoadedModifications.Add(newLoadedModification);

            if (System.SystemConfig.EnabledModifications.Contains(gameMod.Name)) {
                TryEnableModification(newLoadedModification);
            }
        }
    }

    public void Dispose() {
        foreach (var loadedMod in LoadedModifications) {
            if (loadedMod.State is LoadedState.Enabled) {
                loadedMod.Modification.OnDisable();
            }
        }
    }

    public void TryEnableModification(LoadedModification modification) {
        if (modification.State is LoadedState.Errored) {
            Services.PluginLog.Error("Attempted to enable errored modification");
            return;
        }

        try {
            if (modification.Modification.ModificationInfo.CompatabilityModule is { } compatabilityModule) {
                if (!compatabilityModule.ShouldLoadGameModification()) {
                    modification.State = LoadedState.Errored;
                    modification.ErrorMessage = $"The original version of this feature is already active in {compatabilityModule.TargetPluginInternalName}\n\n" +
                                                $"ID: {compatabilityModule.TargetModule}";
                    Services.PluginLog.Warning($"Attempted to load {modification.Name}, but it's already enabled in {compatabilityModule.TargetPluginInternalName}");
                    return;
                }
            }
            
            Services.PluginLog.Info($"Enabling {modification.Name}");
            modification.Modification.OnEnable();
            modification.State = LoadedState.Enabled;
            Services.PluginLog.Info($"{modification.Name} has been enabled");
            System.SystemConfig.EnabledModifications.Add(modification.Name);
            System.SystemConfig.Save();
        }
        catch (Exception e) {
            modification.State = LoadedState.Errored;
            modification.ErrorMessage = "Failed to load, this module has been disabled";
            Services.PluginLog.Error(e, $"Error while enabling {modification.Name}, attempting to disable");
            
            try {
                modification.Modification.OnDisable();
                Services.PluginLog.Information($"Successfully disabled erroring modification {modification.Name}");
            }
            catch (Exception fatal) {
                modification.ErrorMessage = "Critical Error: Module failed to load, and errored again while unloading";
                Services.PluginLog.Error(fatal, $"Critical Error while trying to unload erroring modification: {modification.Name}");
            }
        }
    }

    public void TryDisableModification(LoadedModification modification) {
        if (modification.State is LoadedState.Errored) {
            Services.PluginLog.Error("Attempted to disable errored modification");
            return;
        }

        try {
            Services.PluginLog.Info($"Disabling {modification.Name}");
            modification.Modification.OnDisable();
        }
        catch (Exception e) {
            modification.State = LoadedState.Errored;
            Services.PluginLog.Error(e, $"Failed to disable modification: {modification.Name}");
        } finally {
            modification.State = LoadedState.Disabled;
            Services.PluginLog.Info($"{modification.Name} has been disabled");
            System.SystemConfig.EnabledModifications.Remove(modification.Name);
            System.SystemConfig.Save();
        }
    }

    private List<GameModification> GetGameModifications() 
        => Assembly
           .GetCallingAssembly()
           .GetTypes()
           .Where(type => type.IsSubclassOf(typeof(GameModification)))
           .Where(type => !type.IsAbstract)
           .Select(type => (GameModification?) Activator.CreateInstance(type))
           .OfType<GameModification>()
           .ToList();
}
