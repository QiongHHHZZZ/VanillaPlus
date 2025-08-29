using System.Diagnostics.CodeAnalysis;
using Dalamud.Game.Text;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

namespace VanillaPlus.Extensions;

public static class InventoryItemExtensions {
    public static uint GetIconId(ref this InventoryItem item) {
        uint baseIconId = 0;
        
        if (ItemUtil.IsEventItem(item.ItemId)) {
            if (item.TryGetEventItem(out var eventItem)) {
                baseIconId = eventItem.Value.Icon;
            }
        }
        else if (ItemUtil.IsNormalItem(item.ItemId)) {
            if (item.TryGetItem(out var baseItem)) {
                baseIconId = baseItem.Value.Icon;
            }
        }

        if (item.IsHighQuality()) {
            baseIconId += 1_000_000;
        }

        Services.PluginLog.Debug($"Resolved {item.ItemId}'s iconId to: {baseIconId}");
        return baseIconId;
    }

    public static ReadOnlySeString GetItemName(ref this InventoryItem item) {
        var itemName = ItemUtil.IsEventItem(item.ItemId)
                           ? Services.DataManager.GetExcelSheet<EventItem>().TryGetRow(item.ItemId, out var eventItem) ? eventItem.Name : default
                           : Services.DataManager.GetExcelSheet<Item>().TryGetRow(ItemUtil.GetBaseId(item.ItemId).ItemId, out var baseItem) ? baseItem.Name : default;

        if (item.IsHighQuality())
            itemName += " " + SeIconChar.HighQuality.ToIconString();
        else if (item.IsCollectable())
            itemName += " " + SeIconChar.Collectible.ToIconString();

        Services.PluginLog.Debug($"Resolved {item.ItemId}'s name to to: {itemName}");
        
        return new Lumina.Text.SeStringBuilder()
            .PushColorType(ItemUtil.GetItemRarityColorType(item.ItemId))
            .Append(itemName)
            .PopColorType()
            .ToReadOnlySeString();
    }

    public static bool TryGetItem(ref this InventoryItem inventoryItem, [NotNullWhen(returnValue: true)] out Item? item) {
        item = null;
        
        if (ItemUtil.IsNormalItem(inventoryItem.ItemId)) {
            if (Services.DataManager.GetExcelSheet<Item>().TryGetRow(ItemUtil.GetBaseId(inventoryItem.ItemId).ItemId, out var baseItem)) {
                item = baseItem;
                return true;
            }
        }

        return false;
    }

    public static bool TryGetEventItem(ref this InventoryItem inventoryItem, [NotNullWhen(returnValue: true)] out EventItem? item) {
        item = null;
        
        if (ItemUtil.IsEventItem(inventoryItem.ItemId)) {
            if (Services.DataManager.GetExcelSheet<EventItem>().TryGetRow(ItemUtil.GetBaseId(inventoryItem.ItemId).ItemId, out var eventItem)) {
                item = eventItem;
                return true;
            }
        }

        return false;
    }
}
