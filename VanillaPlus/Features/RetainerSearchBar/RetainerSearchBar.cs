using System.Collections.Generic;
using System.Numerics;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.RetainerSearchBar;

public class RetainerSearchBar : GameModification {
    public override ModificationInfo ModificationInfo => new() {
        DisplayName = "Retainer Search Bar",
        Description = "Adds a search bar to the retainer window.",
        Type = ModificationType.UserInterface,
        SubType = ModificationSubType.Inventory,
        Authors = [ "MidoriKami" ],
        ChangeLog = [
            new ChangeLogInfo(1, "InitialChangelog"),
        ],
    };

    private InventorySearchAddonController? retainerInventoryController;

    public override string ImageName => "RetainerSearchBar.png";

    public override void OnEnable() {
        retainerInventoryController = new InventorySearchAddonController(new Dictionary<string, Vector2> {
            { "InventoryRetainerLarge", new Vector2(250.0f, 28.0f) },
            { "InventoryRetainer", new Vector2(150.0f, 28.0f) },
        });
    }

    public override void OnDisable() {
        retainerInventoryController?.Dispose();
        retainerInventoryController = null;
    }
}
