using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Inventory.InventoryEventArgTypes;
using FFXIVClientStructs.FFXIV.Client.Game;
using KamiToolKit.Nodes;
using KamiToolKit.System;
using VanillaPlus.Classes;
using VanillaPlus.NativeElements.Addons;
using VanillaPlus.Utilities;

namespace VanillaPlus.Features.RecentlyLootedWindow;

public unsafe class RecentlyLootedWindow : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "最近拾取物品窗口",
        Description = "显示本次游戏过程中获得的物品记录，可滚动查看。\n\n" +
                      "仅会记录启用功能后的新增物品。",
        Type = ModificationType.NewWindow,
        Authors = ["MidoriKami"],
        ChangeLog = [
            new ChangeLogInfo(1, "初始实现"),
            new ChangeLogInfo(2, "仅追踪常规背包与军械库"),
            new ChangeLogInfo(3, "物品数量改为显示在图标上方"),
        ],
    };

    private NodeListAddon? addonRecentlyLooted;

    private bool enableTracking;
    private List<LootedItemInfo>? items;
    private bool updateRequested;

    public override string ImageName => "RecentlyLootedWindow.png";

    public override void OnEnable() {
        items = [];

        addonRecentlyLooted = new NodeListAddon {
            NativeController = System.NativeController,
            Size = new Vector2(250.0f, 350.0f),
            InternalName = "RecentlyLooted",
            Title = "最近拾取物品",
            OpenCommand = "/recentloot",
            UpdateListFunction = UpdateList,
        };
        
        addonRecentlyLooted.Initialize([VirtualKey.CONTROL, VirtualKey.L]);

        OpenConfigAction = addonRecentlyLooted.OpenAddonConfig;

        enableTracking = Services.ClientState.IsLoggedIn;

        Services.GameInventory.InventoryChanged += OnRawItemAdded;
        Services.ClientState.Login += OnLogin;
        Services.ClientState.Logout += OnLogout;

        updateRequested = true;
    }

    public override void OnDisable() {
        addonRecentlyLooted?.Dispose();
        addonRecentlyLooted = null;

        items?.Clear();
        items = null;

        Services.GameInventory.InventoryChanged -= OnRawItemAdded;
        Services.ClientState.Login -= OnLogin;
        Services.ClientState.Logout -= OnLogout;
    }

    private void OnLogin() {
        enableTracking = true;
        items?.Clear();
    }

    private void OnLogout(int type, int code)
        => enableTracking = false;

    private void OnRawItemAdded(IReadOnlyCollection<InventoryEventArgs> events) {
        if (!enableTracking) return;
        
        foreach (var eventData in events) {
            if (!Inventory.StandardInventories.Contains(eventData.Item.ContainerType)) continue;
            
            if (!Services.ClientState.IsLoggedIn) return;
            if (eventData is not (InventoryItemAddedArgs or InventoryItemChangedArgs)) return;
            if (eventData is InventoryItemChangedArgs changedArgs && changedArgs.OldItemState.Quantity >= changedArgs.Item.Quantity) return;

            var inventoryItem = (InventoryItem*)eventData.Item.Address;
            var changeAmount = eventData is InventoryItemChangedArgs changed ? changed.Item.Quantity - changed.OldItemState.Quantity : eventData.Item.Quantity;
        
            items?.Add(new LootedItemInfo(
                items.Count, 
                inventoryItem->GetItemId(), 
                inventoryItem->GetIconId(), 
                inventoryItem->GetItemName(), 
                changeAmount)
            );

            updateRequested = true;
        }
    }
    
    private bool UpdateList(VerticalListNode listNode, bool isOpening) {
        if (!updateRequested && !isOpening) return false;
        if (items is null) return false;
        
        var listUpdated = listNode.SyncWithListData(items, node => node.Item, data => new LootItemNode {
            Size = new Vector2(listNode.Width, 36.0f),
            IsVisible = true,
            Item = data,
        });

        listNode.ReorderNodes(Comparison);

        updateRequested = false;
        return listUpdated;
    }
    
    private static int Comparison(NodeBase x, NodeBase y) {
        if (x is not LootItemNode left ||  y is not LootItemNode right) return 0;
        
        return left.Item.Index > right.Item.Index ? -1 : 1;
    }
}


