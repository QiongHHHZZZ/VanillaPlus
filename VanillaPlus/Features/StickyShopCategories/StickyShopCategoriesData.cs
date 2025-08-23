using System.Collections.Generic;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.StickyShopCategories;

public class StickyShopCategoriesData : GameModificationData<StickyShopCategoriesData> {
    protected override string FileName => "StickyShopCategories.data.json";

    public Dictionary<uint, ShopConfig> ShopConfigs = [];
}

public class ShopConfig {
    public int Category;
    public int SubCategory;
}
