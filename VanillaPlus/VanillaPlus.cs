﻿using System.Diagnostics;
using System.Numerics;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using KamiToolKit;
using VanillaPlus.Classes;
using VanillaPlus.InternalSystem;
using VanillaPlus.Utilities;

namespace VanillaPlus;

public sealed class VanillaPlus : IDalamudPlugin {
    public VanillaPlus(IDalamudPluginInterface pluginInterface) {
        pluginInterface.Create<Services>();
        System.SystemConfig = SystemConfiguration.Load();

        System.NativeController = new NativeController(pluginInterface);

        System.AddonModificationBrowser = new AddonModificationBrowser {
            NativeController = System.NativeController,
            InternalName = "VanillaPlusConfig",
            Title = "Vanilla Plus 功能浏览器",
            Size = new Vector2(836.0f, 650.0f),
        };

        Services.CommandManager.AddHandler("/vanillaplus", new CommandInfo(Handler) {
            DisplayOrder = 1,
            ShowInHelp = true,
            HelpMessage = "打开功能浏览器",
        });

        Services.CommandManager.AddHandler("/plus", new CommandInfo(Handler) {
            DisplayOrder = 2,
            ShowInHelp = true,
            HelpMessage = "打开功能浏览器",
        });

        System.WindowSystem = new WindowSystem("VanillaPlus");
        Services.PluginInterface.UiBuilder.Draw += System.WindowSystem.Draw;
        Services.PluginInterface.UiBuilder.OpenConfigUi += OpenModificationBrowser;

        System.KeyListener = new KeyListener();
        System.ModificationManager = new ModificationManager();

        AutoOpenBrowser(false);
    }

    public void Dispose() {
        System.KeyListener.Dispose();
        System.ModificationManager.Dispose();

        foreach (var asset in Assets.LoadedTextures) {
            asset.Dispose();
        }
        Assets.LoadedTextures.Clear();

        foreach (var (_, agentInfo) in AgentInterfaceExtensions.HookedAgents) {
            agentInfo.ReceiveEventHook?.Dispose();
        }
        AgentInterfaceExtensions.HookedAgents.Clear();

        Services.PluginInterface.UiBuilder.OpenConfigUi -= OpenModificationBrowser;
        Services.PluginInterface.UiBuilder.Draw -= System.WindowSystem.Draw;
        System.WindowSystem.RemoveAllWindows();

        Services.CommandManager.RemoveHandler("/vanillaplus");

        System.AddonModificationBrowser.Dispose();

        System.NativeController.Dispose();
    }

    [Conditional("DEBUG")]
    private static void AutoOpenBrowser(bool enabled) {
        if (!enabled) return;
        
        System.AddonModificationBrowser.Open();
    }

    private static void Handler(string command, string arguments) {
        switch (command, arguments) {
            case { command: "/vanillaplus" or "/plus", arguments: "" }:
                System.AddonModificationBrowser.Open();
                break;
        }
    }

    private void OpenModificationBrowser()
        => System.AddonModificationBrowser.Open();
}
