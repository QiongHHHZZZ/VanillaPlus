using System.Collections.Generic;
using VanillaPlus.Core;

namespace VanillaPlus.HideUnwantedBanners;

public class HideUnwantedBannersConfig : GameModificationConfig<HideUnwantedBannersConfig> {
    protected override string FileName =>  "HideUnwantedBanners.config.json";

    public HashSet<int> HiddenBanners = [];
}
