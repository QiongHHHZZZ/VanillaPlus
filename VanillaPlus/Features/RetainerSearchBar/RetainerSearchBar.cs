using VanillaPlus.Classes;

namespace VanillaPlus.Features.RetainerSearchBar;

public class RetainerSearchBar : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "雇员搜索栏",
        Description = "为雇员仓库窗口添加搜索栏，快速定位物品。",
        Type = ModificationType.UserInterface,
        SubType = ModificationSubType.Inventory,
        Authors = [ "MidoriKami" ],
        ChangeLog = [
            new ChangeLogInfo(1, "初始版本"),
        ],
    };

    private InventorySearchAddonController? retainerInventoryController;

    public override string ImageName => "RetainerSearchBar.png";

    public override void OnEnable() {
        retainerInventoryController = new InventorySearchAddonController("InventoryRetainerLarge", "InventoryRetainer");
    }

    public override void OnDisable() {
        retainerInventoryController?.Dispose();
        retainerInventoryController = null;
    }
}


