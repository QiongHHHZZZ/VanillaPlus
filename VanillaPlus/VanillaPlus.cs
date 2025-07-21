using System.Numerics;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using KamiToolKit;
using VanillaPlus.Core;
using VanillaPlus.Core.Windows;

namespace VanillaPlus;

public sealed class VanillaPlus : IDalamudPlugin {
    private readonly ModificationManager gameModificationManager;

    public VanillaPlus(IDalamudPluginInterface pluginInterface) {
        pluginInterface.Create<Services>();

        System.NativeController = new NativeController(pluginInterface);

        System.ModificationBrowser = new ModificationBrowser {
            NativeController = System.NativeController,
            InternalName = "VanillaPlusConfig",
            Title = "Vanilla Plus Modification Browser",
            Size = new Vector2(836.0f, 560.0f),
        };

        Services.CommandManager.AddHandler("/vanillaplus", new CommandInfo(Handler) {
            ShowInHelp = true,
            HelpMessage = "Open Game Modification Browser",
        });

        System.WindowSystem = new WindowSystem("VanillaPlus");
        Services.PluginInterface.UiBuilder.Draw += System.WindowSystem.Draw;
        Services.PluginInterface.UiBuilder.OpenConfigUi += OpenModificationBrowser;

        gameModificationManager = new ModificationManager(pluginInterface);
        
        #if DEBUG
        Services.Framework.RunOnTick(OpenModificationBrowser);
        #endif
    }

    private void Handler(string command, string arguments) {
        switch (command, arguments) {
            case { command: "/vanillaplus", arguments: "" }:
                System.ModificationBrowser.Open();
                break;
        }
    }

    private void OpenModificationBrowser()
        => System.ModificationBrowser.Open();

    public void Dispose() {
        gameModificationManager.Dispose();

        Services.PluginInterface.UiBuilder.OpenConfigUi -= OpenModificationBrowser;
        Services.PluginInterface.UiBuilder.Draw -= System.WindowSystem.Draw;
        System.WindowSystem.RemoveAllWindows();

        Services.CommandManager.RemoveHandler("/vanillaplus");

        System.ModificationBrowser.Dispose();

        System.NativeController.Dispose();
    }
}
