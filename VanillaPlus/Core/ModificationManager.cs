using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VanillaPlus.Core.Objects;

namespace VanillaPlus.Core;

public class ModificationManager : IDisposable {

    public readonly List<LoadedModification> LoadedModifications = [];
    private readonly List<GameModification> gameModifications;

    public ModificationManager() {
        System.SystemConfig = SystemConfiguration.Load();
        gameModifications = GetGameModifications();

        foreach (var gameMod in gameModifications) {
            Services.PluginInterface.Inject(gameMod);
            Services.Hooker.InitializeFromAttributes(gameMod);

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
            modification.Modification.OnEnable();
            modification.State = LoadedState.Enabled;
            Services.PluginLog.Info($"{modification.Name} has been enabled.");
            System.SystemConfig.EnabledModifications.Add(modification.Name);
            System.SystemConfig.Save();
        }
        catch (Exception e) {
            modification.State = LoadedState.Errored;
            Services.PluginLog.Error(e, $"Error while enabling {modification.Name}, attempting to disable.");
            
            try {
                modification.Modification.OnDisable();
                Services.PluginLog.Information($"Successfully disabled erroring modification {modification.Name}.");
            }
            catch (Exception fatal) {
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
            modification.Modification.OnDisable();
        }
        catch (Exception e) {
            modification.State = LoadedState.Errored;
            Services.PluginLog.Error(e, $"Failed to disable modification: {modification.Name}");
        } finally {
            modification.State = LoadedState.Disabled;
            Services.PluginLog.Info($"{modification.Name} has been disabled.");
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
