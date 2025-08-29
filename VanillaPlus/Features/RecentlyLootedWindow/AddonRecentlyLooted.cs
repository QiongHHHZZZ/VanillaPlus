using System.Collections.Generic;
using Dalamud.Game.Inventory.InventoryEventArgTypes;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Addon;
using KamiToolKit.Nodes;
using KamiToolKit.System;
using VanillaPlus.Extensions;

namespace VanillaPlus.Features.RecentlyLootedWindow;

public unsafe class AddonRecentlyLooted : NativeAddon {

    private readonly List<LootedItemInfo> items = [];

    private ScrollingAreaNode<VerticalListNode>? scrollingAreaNode;
    private VerticalListNode? ListNode => scrollingAreaNode?.ContentNode;

    protected override void OnSetup(AtkUnitBase* addon) {
        scrollingAreaNode = new ScrollingAreaNode<VerticalListNode> {
            Position = ContentStartPosition,
            Size = ContentSize,
            IsVisible = true,
            ContentHeight = 100,
        };
        ListNode!.FitContents = true;
        AttachNode(scrollingAreaNode);

        RebuildList();
    }

    protected override void OnFinalize(AtkUnitBase* addon)
        => System.NativeController.DisposeNode(ref scrollingAreaNode);

    public void AddInventoryItem(InventoryEventArgs itemEvent) {
        if (!Services.ClientState.IsLoggedIn) return;
        if (itemEvent is not (InventoryItemAddedArgs or InventoryItemChangedArgs)) return;
        if (itemEvent is InventoryItemChangedArgs changedArgs && changedArgs.OldItemState.Quantity >= changedArgs.Item.Quantity) return;

        var inventoryItem = (InventoryItem*)itemEvent.Item.Address;
        var changeAmount = itemEvent is InventoryItemChangedArgs changed ? changed.Item.Quantity - changed.OldItemState.Quantity : itemEvent.Item.Quantity;
        
        items.Add(new LootedItemInfo(
            items.Count, 
            inventoryItem->GetItemId(), 
            inventoryItem->GetIconId(), 
            inventoryItem->GetItemName(), 
            changeAmount)
        );

        if (IsOpen) {
            RebuildList();
        }
    }

    private void RebuildList() {
        if (scrollingAreaNode is null) return;
        if (ListNode is null) return;

        ListNode.SyncWithListData(items, node => node.Item, data => new LootItemNode {
            Height = 36.0f,
            Width = scrollingAreaNode.ContentNode.Width,
            IsVisible = true,
            Item = data,
        });
        
        ListNode.ReorderNodes(Comparison);
        
        scrollingAreaNode.ContentHeight = ListNode?.Height ?? 100.0f;
    }

    private static int Comparison(NodeBase x, NodeBase y) {
        if (x is not LootItemNode left ||  y is not LootItemNode right) return 0;
        
        return left.Item.Index > right.Item.Index ? -1 : 1;
    }

    public void ClearItems()
        => items.Clear();
}
