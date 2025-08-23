using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace VanillaPlus.Extensions;

public record LoggedAddonEvents {
    public bool Setup { get; set; } = true;
    public bool Refresh { get; set; } = true;
    public bool ReceiveEvent { get; set; } = false;
    public bool RequestedUpdate { get; set; } = true;
    public bool Update { get; set; } = false;
    public bool Draw { get; set; } = false;
    public bool Finalize { get; set; } = true;
}

public static class AddonLifecycleExtensions {
    public static void LogAddon(this IAddonLifecycle addonLifecycle, string addonName, LoggedAddonEvents? loggedModules = null) {
        if (loggedModules is not null) {
            if (loggedModules.Setup) addonLifecycle.RegisterListener(AddonEvent.PostSetup, addonName, Logger);
            if (loggedModules.Refresh) addonLifecycle.RegisterListener(AddonEvent.PostRefresh, addonName, Logger);
            if (loggedModules.ReceiveEvent) addonLifecycle.RegisterListener(AddonEvent.PostReceiveEvent, addonName, Logger);
            if (loggedModules.RequestedUpdate) addonLifecycle.RegisterListener(AddonEvent.PostRequestedUpdate, addonName, Logger);
            if (loggedModules.Update) addonLifecycle.RegisterListener(AddonEvent.PostUpdate, addonName, Logger);
            if (loggedModules.Draw) addonLifecycle.RegisterListener(AddonEvent.PostDraw, addonName, Logger);
            if (loggedModules.Finalize) addonLifecycle.RegisterListener(AddonEvent.PreFinalize, addonName, Logger);
        }
        else {
            addonLifecycle.RegisterListener(AddonEvent.PostSetup, addonName, Logger);
            addonLifecycle.RegisterListener(AddonEvent.PostRefresh, addonName, Logger);
            addonLifecycle.RegisterListener(AddonEvent.PostReceiveEvent, addonName, Logger);
            addonLifecycle.RegisterListener(AddonEvent.PostRequestedUpdate, addonName, Logger);
            addonLifecycle.RegisterListener(AddonEvent.PreFinalize, addonName, Logger);
        }
    }

    private static void Logger(AddonEvent type, AddonArgs args) {
        Services.PluginLog.Debug($"{args.AddonName} called {type.ToString().Replace("Post", string.Empty)}");

        if (args is AddonReceiveEventArgs receiveEventArgs) {
            Services.PluginLog.Debug($"\t{(AtkEventType)receiveEventArgs.AtkEventType}: {receiveEventArgs.EventParam}");
        }
    }

    public static void UnLogAddon(this IAddonLifecycle addonLifecycle, string addonName) {
        addonLifecycle.UnregisterListener(AddonEvent.PostSetup, addonName, Logger);
        addonLifecycle.UnregisterListener(AddonEvent.PostRefresh, addonName, Logger);
        addonLifecycle.UnregisterListener(AddonEvent.PostReceiveEvent, addonName, Logger);
        addonLifecycle.UnregisterListener(AddonEvent.PostRequestedUpdate, addonName, Logger);
        addonLifecycle.UnregisterListener(AddonEvent.PreFinalize, addonName, Logger);
        addonLifecycle.UnregisterListener(AddonEvent.PostDraw, addonName, Logger);
        addonLifecycle.UnregisterListener(AddonEvent.PostUpdate, addonName, Logger);
    }
}
