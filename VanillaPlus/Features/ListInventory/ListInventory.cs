using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.ClientState.Keys;
using FFXIVClientStructs.FFXIV.Client.Game;
using KamiToolKit.Nodes;
using KamiToolKit.System;
using VanillaPlus.Basic_Addons;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.ListInventory;

public unsafe class ListInventory : GameModification {
    public override ModificationInfo ModificationInfo => new() {
        DisplayName = "List Inventory Window",
        Description = "Adds a window that displays your inventory as a list, with toggleable filters.",
        Type = ModificationType.NewWindow,
        Authors = [ "MidoriKami" ],
        ChangeLog = [
            new ChangeLogInfo(1, "InitialChangelog"),
            new ChangeLogInfo(2, "Added Sort by Quantity"),
            new ChangeLogInfo(3, "Added `/listinventory` command to open window"),
            new ChangeLogInfo(4, "Sort Dropdown is now on another line, added reverse sort direction button"),
        ],
    };
    
    private SearchableNodeListAddon? addonListInventory;

    private string filterString = string.Empty;
    private string searchString = string.Empty;
    private bool filterReversed;
    private bool updateRequested;
    
    public override void OnEnable() {
        addonListInventory = new SearchableNodeListAddon {
            NativeController = System.NativeController,
            InternalName = "ListInventory",
            Title = "Inventory List",
            Size = new Vector2(450.0f, 700.0f),
            OnFilterUpdated = OnFilterUpdated,
            OnSearchUpdated = OnSearchUpdated,
            UpdateListFunction = OnListUpdated,
            DropDownOptions = ["Alphabetically", "Quantity", "Level", "Item Level", "Rarity", "Item Id", "Item Category"],
            OpenCommand = "/listinventory",
        };

        addonListInventory.Initialize([VirtualKey.SHIFT, VirtualKey.CONTROL, VirtualKey.I]);

        OpenConfigAction = addonListInventory.OpenAddonConfig;

        Services.AddonLifecycle.RegisterListener(AddonEvent.PostRequestedUpdate, "Inventory", OnInventoryUpdate);
        
        updateRequested = true;
    }

    public override void OnDisable() {
        addonListInventory?.Dispose();
        addonListInventory = null;
        
        Services.AddonLifecycle.UnregisterListener(OnInventoryUpdate);
    }
    
    private void OnInventoryUpdate(AddonEvent type, AddonArgs args) {
        updateRequested = true;
        addonListInventory?.DoListUpdate();
    }

    private void OnFilterUpdated(string newFilterString, bool reversed) {
        updateRequested = true;
        filterString = newFilterString;
        filterReversed = reversed;
        addonListInventory?.DoListUpdate();
    }

    private void OnSearchUpdated(string newSearchString) {
        updateRequested = true;
        searchString = newSearchString;
        addonListInventory?.DoListUpdate();
    }

    private bool OnListUpdated(VerticalListNode list, bool isOpening) {
        if (!updateRequested && !isOpening) return false;

        var filteredInventoryItems = GetInventoryItems()
            .Where(item => item.IsRegexMatch(searchString))
            .ToList();

        var listUpdated = list.SyncWithListData(filteredInventoryItems, node => node.Item, data => new InventoryItemNode {
            Size = new Vector2(list.Width, 32.0f),
            Item = data,
            IsVisible = true,
        });

        list.ReorderNodes(Comparison);
        
        updateRequested = false;
        return listUpdated;
    }
    
    private int Comparison(NodeBase x, NodeBase y) {
        if (x is not InventoryItemNode left || y is not InventoryItemNode right) return 0;

        var leftItem = left.Item;
        var rightItem = right.Item;
        if (leftItem is null || rightItem is null) return 0;

        // Note: Compares in opposite direction to be descending instead of ascending, except for alphabetically

        var result = filterString switch {
            "Alphabetically" => string.CompareOrdinal(leftItem.Name, rightItem.Name),
            "Level" => rightItem.Level.CompareTo(leftItem.Level),
            "Item Level" => rightItem.ItemLevel.CompareTo(leftItem.ItemLevel),
            "Rarity" => rightItem.Rarity.CompareTo(leftItem.Rarity),
            "Item Id" => rightItem.Item.ItemId.CompareTo(leftItem.Item.ItemId),
            "Item Category" => rightItem.UiCategory.CompareTo(leftItem.UiCategory),
            "Quantity" => rightItem.ItemCount.CompareTo(leftItem.ItemCount),
            _ => string.CompareOrdinal(leftItem.Name, rightItem.Name),
        };

        var reverseModifier = filterReversed ? -1 : 1;
        
        return ( result is 0 ? string.CompareOrdinal(leftItem.Name, rightItem.Name) : result ) * reverseModifier;
    }
    
    private static List<ItemInfo> GetInventoryItems() {
        List<InventoryType> inventories = [ InventoryType.Inventory1, InventoryType.Inventory2, InventoryType.Inventory3, InventoryType.Inventory4 ];
        List<InventoryItem> items = [];

        foreach (var inventory in inventories) {
            var container = InventoryManager.Instance()->GetInventoryContainer(inventory);

            for (var index = 0; index < container->Size; ++index) {
                ref var item = ref container->Items[index];
                if (item.ItemId is 0) continue;
                
                items.Add(item);
            }
        }

        List<ItemInfo> itemInfos = [];
        itemInfos.AddRange(from itemGroups in items.GroupBy(item => item.ItemId)
                           where itemGroups.Key is not 0
                           let item = itemGroups.First()
                           let itemCount = itemGroups.Sum(duplicateItem => duplicateItem.Quantity)
                           select new ItemInfo {
                               Item = item, ItemCount = itemCount,
                           });

        return itemInfos;
    }
}
