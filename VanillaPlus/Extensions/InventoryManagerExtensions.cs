using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace VanillaPlus.Extensions;

public static class InventoryManagerExtensions {
    
    /// <summary>
    /// Checks Inventory, Equipped, and Armory for item, does not search all inventories.
    /// </summary>
    public static bool PlayerHasItem(this ref InventoryManager inventoryManager, uint itemId) {
        var inventories = new List<InventoryType> {
            InventoryType.Inventory1,
            InventoryType.Inventory2,
            InventoryType.Inventory3,
            InventoryType.Inventory4,
            InventoryType.EquippedItems,
            InventoryType.ArmoryMainHand,
            InventoryType.ArmoryHead,
            InventoryType.ArmoryBody,
            InventoryType.ArmoryHands,
            InventoryType.ArmoryWaist,
            InventoryType.ArmoryLegs,
            InventoryType.ArmoryFeets,
            InventoryType.ArmoryOffHand,
            InventoryType.ArmoryEar,
            InventoryType.ArmoryNeck,
            InventoryType.ArmoryWrist,
            InventoryType.ArmoryRings,
        };

        foreach (var inventory in inventories) {
            if (inventoryManager.GetItemCountInContainer(itemId, inventory) is not 0) {
                return true;
            }
        }

        return false;
    }
}
