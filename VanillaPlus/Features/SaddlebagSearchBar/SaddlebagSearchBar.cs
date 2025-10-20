using VanillaPlus.Classes;

namespace VanillaPlus.Features.SaddlebagSearchBar;

public class SaddlebagSearchBar : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "鞍囊搜索栏",
        Description = "为陆行鸟鞍囊窗口添加搜索栏，快捷查找物品。",
        Type = ModificationType.UserInterface,
        SubType = ModificationSubType.Inventory,
        Authors = [ "MidoriKami" ],
        ChangeLog = [
            new ChangeLogInfo(1, "初始版本"),
        ],
    };

    private InventorySearchAddonController? saddlebagInventoryController;

    public override string ImageName => "SaddlebagSearchBar.png";

    public override void OnEnable() {
        saddlebagInventoryController = new InventorySearchAddonController("InventoryBuddy");
    }

    public override void OnDisable() {
        saddlebagInventoryController?.Dispose();
        saddlebagInventoryController = null;
    }
}


