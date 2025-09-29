using System.Collections.Generic;
using System.Numerics;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.InventorySearchBar;

public class InventorySearchBar : GameModification {
    public override ModificationInfo ModificationInfo => new() {
        DisplayName = "Inventory Search Bar",
        Description = "Adds a search bar to the inventory window.",
        Type = ModificationType.UserInterface,
        SubType = ModificationSubType.Inventory,
        Authors = [ "MidoriKami" ],
        ChangeLog = [
            new ChangeLogInfo(1, "InitialChangelog"),
        ],
    };

    public override string ImageName => "InventorySearchBar.png";

    private InventorySearchAddonController? inventoryController;

    public override void OnEnable() {
        inventoryController = new InventorySearchAddonController(new Dictionary<string, Vector2> {
            {"InventoryExpansion", new Vector2(275.0f, 28.0f)},
            {"InventoryLarge", new Vector2(250.0f, 28.0f)},
            {"Inventory", new Vector2(150.0f, 28.0f)},
        });
    }

    public override void OnDisable() {
        inventoryController?.Dispose();
        inventoryController = null;
    }
}
