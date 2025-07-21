using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dalamud.Plugin;
using VanillaPlus.Core.Objects;

namespace VanillaPlus.Core;

public class ModificationManager : IDisposable {

    private readonly List<LoadedModification> loadedModifications = [];
    private readonly List<GameModification> gameModifications;

    public ModificationManager(IDalamudPluginInterface pluginInterface) {
        System.SystemConfig = SystemConfiguration.Load();
        gameModifications = GetGameModifications();

        foreach (var gameMod in gameModifications) {
            pluginInterface.Inject(gameMod);
            
            loadedModifications.Add(new LoadedModification(gameMod, LoadedState.Disabled));

            if (System.SystemConfig.EnabledModifications.Contains(gameMod.ModificationInfo.DisplayName)) {
                gameMod.OnEnable();
            }
        }
    }

    public void Dispose() {
        foreach (var loadedMod in loadedModifications) {
            if (loadedMod.State is LoadedState.Enabled) {
                loadedMod.Modification.OnDisable();
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
           .OfType<GameModification>()
           .ToList();
}
