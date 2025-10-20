using System.Numerics;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.ClientState.Keys;
using KamiToolKit.Nodes;
using KamiToolKit.System;
using VanillaPlus.Classes;
using VanillaPlus.NativeElements.Addons;
using VanillaPlus.Utilities;

namespace VanillaPlus.Features.ListInventory;

public class ListInventory : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "背包列表窗口",
        Description = "以列表形式展示背包内容，并提供可切换的筛选与排序。",
        Type = ModificationType.NewWindow,
        Authors = [ "MidoriKami" ],
        ChangeLog = [
            new ChangeLogInfo(1, "初始版本"),
            new ChangeLogInfo(2, "新增按数量排序"),
            new ChangeLogInfo(3, "新增 `/listinventory` 指令用于打开窗口"),
            new ChangeLogInfo(4, "排序下拉框移动至单独一行，并新增排序方向按钮"),
            new ChangeLogInfo(5, "名称调整，与其它功能保持一致"),
        ],
    };
    
    private SearchableNodeListAddon? addonListInventory;

    private string filterString = string.Empty;
    private string searchString = string.Empty;
    private bool filterReversed;
    private bool updateRequested;

    public override string ImageName => "ListInventory.png";

    public override void OnEnable() {
        addonListInventory = new SearchableNodeListAddon {
            NativeController = System.NativeController,
            InternalName = "ListInventory",
            Title = "背包列表",
            Size = new Vector2(450.0f, 700.0f),
            OnFilterUpdated = OnFilterUpdated,
            OnSearchUpdated = OnSearchUpdated,
            UpdateListFunction = OnListUpdated,
            DropDownOptions = ["按名称", "按数量", "按等级", "按物品品级", "按稀有度", "按物品 ID", "按类别"],
            OpenCommand = "/listinventory",
        };

        addonListInventory.Initialize([VirtualKey.SHIFT, VirtualKey.CONTROL, VirtualKey.I]);

        OnFilterUpdated("按名称", false);

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

        var filteredInventoryItems = Inventory.GetInventoryItems(searchString);

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
            "按名称" => string.CompareOrdinal(leftItem.Name, rightItem.Name),
            "按等级" => rightItem.Level.CompareTo(leftItem.Level),
            "按物品品级" => rightItem.ItemLevel.CompareTo(leftItem.ItemLevel),
            "按稀有度" => rightItem.Rarity.CompareTo(leftItem.Rarity),
            "按物品 ID" => rightItem.Item.ItemId.CompareTo(leftItem.Item.ItemId),
            "按类别" => rightItem.UiCategory.CompareTo(leftItem.UiCategory),
            "按数量" => rightItem.ItemCount.CompareTo(leftItem.ItemCount),
            _ => string.CompareOrdinal(leftItem.Name, rightItem.Name),
        };

        var reverseModifier = filterReversed ? -1 : 1;
        
        return ( result is 0 ? string.CompareOrdinal(leftItem.Name, rightItem.Name) : result ) * reverseModifier;
    }
}


