using System.Numerics;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using KamiToolKit;
using VanillaPlus.Core;
using VanillaPlus.Core.Objects;
using VanillaPlus.Core.Windows;

namespace VanillaPlus;

public sealed class VanillaPlus : IDalamudPlugin {
    public VanillaPlus(IDalamudPluginInterface pluginInterface) {
        pluginInterface.Create<Services>();
        System.SystemConfig = SystemConfiguration.Load();

        System.NativeController = new NativeController(pluginInterface);

        System.AddonModificationBrowser = new ModificationBrowser {
            NativeController = System.NativeController,
            InternalName = "VanillaPlusConfig",
            Title = "Vanilla Plus Modification Browser",
            Size = new Vector2(836.0f, 560.0f),
        };

        if (System.SystemConfig.BrowserWindowPosition is { } browserPosition) {
            System.AddonModificationBrowser.Position = browserPosition;
        }

        Services.CommandManager.AddHandler("/vanillaplus", new CommandInfo(Handler) {
            ShowInHelp = true,
            HelpMessage = "Open Game Modification Browser",
        });

        System.WindowSystem = new WindowSystem("VanillaPlus");
        Services.PluginInterface.UiBuilder.Draw += System.WindowSystem.Draw;
        Services.PluginInterface.UiBuilder.OpenConfigUi += OpenModificationBrowser;

        System.ModificationManager = new ModificationManager();
        
        #if DEBUG
        Services.Framework.RunOnTick(OpenModificationBrowser, delayTicks: 5);
        #endif
    }

    private void Handler(string command, string arguments) {
        switch (command, arguments) {
            case { command: "/vanillaplus", arguments: "" }:
                System.AddonModificationBrowser.Open();
                break;
        }
    }

    private void OpenModificationBrowser()
        => System.AddonModificationBrowser.Open();

    public void Dispose() {
        System.ModificationManager.Dispose();

        Services.PluginInterface.UiBuilder.OpenConfigUi -= OpenModificationBrowser;
        Services.PluginInterface.UiBuilder.Draw -= System.WindowSystem.Draw;
        System.WindowSystem.RemoveAllWindows();

        Services.CommandManager.RemoveHandler("/vanillaplus");

        System.AddonModificationBrowser.Dispose();

        System.NativeController.Dispose();
    }
}
