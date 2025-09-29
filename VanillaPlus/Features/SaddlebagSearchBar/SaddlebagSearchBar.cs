using System.Collections.Generic;
using System.Numerics;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.SaddlebagSearchBar;

public class SaddlebagSearchBar : GameModification {
    public override ModificationInfo ModificationInfo => new() {
        DisplayName = "Saddlebag Search Bar",
        Description = "Adds a search bar to the saddlebag window.",
        Type = ModificationType.UserInterface,
        SubType = ModificationSubType.Inventory,
        Authors = [ "MidoriKami" ],
        ChangeLog = [
            new ChangeLogInfo(1, "InitialChangelog"),
        ],
    };

    private InventorySearchAddonController? saddlebagInventoryController;

    public override string ImageName => "SaddlebagSearchBar.png";

    public override void OnEnable() {
        saddlebagInventoryController = new InventorySearchAddonController(new Dictionary<string, Vector2> {
            { "InventoryBuddy", new Vector2(275.0f, 28.0f) },
        });
    }

    public override void OnDisable() {
        saddlebagInventoryController?.Dispose();
        saddlebagInventoryController = null;
    }
}
