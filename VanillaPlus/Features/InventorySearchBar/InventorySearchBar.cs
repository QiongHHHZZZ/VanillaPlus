using VanillaPlus.Classes;

namespace VanillaPlus.Features.InventorySearchBar;

public class InventorySearchBar : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "背包搜索栏",
        Description = "为背包窗口添加搜索栏，快速定位物品。",
        Type = ModificationType.UserInterface,
        SubType = ModificationSubType.Inventory,
        Authors = [ "MidoriKami" ],
        ChangeLog = [
            new ChangeLogInfo(1, "初始版本"),
        ],
    };

    public override string ImageName => "InventorySearchBar.png";

    private InventorySearchAddonController? inventoryController;

    public override void OnEnable() {
        inventoryController = new InventorySearchAddonController("InventoryExpansion", "InventoryLarge", "Inventory");
    }

    public override void OnDisable() {
        inventoryController?.Dispose();
        inventoryController = null;
    }
}


