using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Command;
using Dalamud.Game.Inventory.InventoryEventArgTypes;
using VanillaPlus.Classes;
using VanillaPlus.Utilities;

namespace VanillaPlus.Features.RecentlyLootedWindow;

public class RecentlyLootedWindow : GameModification {
    public override ModificationInfo ModificationInfo => new() {
        DisplayName = "Recently Looted Items Window",
        Description = "Adds a window that shows a scrollable list of all items that you have looted this session.\n\n" +
                      "Can only show items looted after this feature is enabled.",
        Type = ModificationType.NewWindow,
        Authors = ["MidoriKami"],
        ChangeLog = [
            new ChangeLogInfo(1, "Initial Implementation"),
            new ChangeLogInfo(2, "Limit tracking to standard inventories, and armory"),
            new ChangeLogInfo(3, "Displays item quantity as text over icon instead of appended to the end of name"),
        ],
    };

    private AddonRecentlyLooted? recentlyLootedWindow;
    private AddonConfig? config;
    private AddonConfigWindow? addonConfigWindow;
    private KeybindListener? keybindListener;

    private bool enableTracking;

    public override string ImageName => "RecentlyLootedWindow.png";

    public override void OnEnable() {
        config = AddonConfig.Load("RecentlyLooted.addon.json", [VirtualKey.CONTROL, VirtualKey.L]);

        recentlyLootedWindow = new AddonRecentlyLooted() {
            NativeController = System.NativeController,
            Size = new Vector2(250.0f, 350.0f),
            InternalName = "RecentlyLooted",
            Title = "Recently Looted Items",
        };
        
        keybindListener = new KeybindListener {
            KeybindCallback = () => {
                if (config.WindowSize != Vector2.Zero) {
                    recentlyLootedWindow.Size = config.WindowSize;
                }

                recentlyLootedWindow.Toggle();
            },
            KeyCombo = config.OpenKeyCombo,
        };
        
        addonConfigWindow = new AddonConfigWindow("Recently Looted Items", config, keybind => {
            keybindListener.KeyCombo = keybind;
        });

        OpenConfigAction = addonConfigWindow.Toggle;

        enableTracking = Services.ClientState.IsLoggedIn;

        Services.GameInventory.InventoryChanged += OnRawItemAdded;
        Services.ClientState.Login += OnLogin;
        Services.ClientState.Logout += OnLogout;

        Services.CommandManager.AddHandler("/recentloot", new CommandInfo(OnListInventoryCommand) {
            DisplayOrder = 3,
            HelpMessage = "Open Recently Looted Window",
        });
    }

    public override void OnDisable() {
        recentlyLootedWindow?.Dispose();
        recentlyLootedWindow = null;
        
        addonConfigWindow?.Dispose();
        addonConfigWindow = null;
        
        keybindListener?.Dispose();
        keybindListener = null;

        Services.GameInventory.InventoryChanged -= OnRawItemAdded;
        Services.ClientState.Login -= OnLogin;
        Services.ClientState.Logout -= OnLogout;
        
        Services.CommandManager.RemoveHandler("/recentloot");
    }

    private void OnLogin() {
        enableTracking = true;
        recentlyLootedWindow?.ClearItems();
    }

    private void OnLogout(int type, int code)
        => enableTracking = false;

    private void OnListInventoryCommand(string command, string arguments)
        => recentlyLootedWindow?.Toggle();

    private void OnRawItemAdded(IReadOnlyCollection<InventoryEventArgs> events) {
        if (!enableTracking) return;
        
        foreach (var eventData in events) {
            if (!Inventory.StandardInventories.Contains(eventData.Item.ContainerType)) continue;

            recentlyLootedWindow?.AddInventoryItem(eventData);
        }
    }
}
