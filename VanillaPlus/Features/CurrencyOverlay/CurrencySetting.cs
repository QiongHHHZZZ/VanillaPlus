using System;
using System.Numerics;
using Dalamud.Utility;
using KamiToolKit.Addons.Interfaces;

namespace VanillaPlus.Features.CurrencyOverlay;

public class CurrencySetting : IInfoNodeData {
    public uint ItemId;
    public Vector2 Position = Vector2.Zero;
    public bool EnableLowLimit;
    public bool EnableHighLimit;
    public int LowLimit;
    public int HighLimit;
    public bool IconReversed;
    public float Scale = 1.0f;

    [NonSerialized]
    public bool IsNodeMoveable;

    public string GetLabel()
        => ItemId is 0 ? "未选择货币" : Services.DataManager.GetItem(ItemId).Name.ToString();

    public string GetSubLabel()
        => Services.DataManager.GetItem(ItemId).ItemSearchCategory.Value.Name.ToString().FirstCharToUpper();

    public uint? GetId()
        => ItemId;

    public uint? GetIconId()
        => ItemId is 0 ? (uint) 5 : Services.DataManager.GetItem(ItemId).Icon;

    public int Compare(IInfoNodeData other, string sortingMode) {
        return sortingMode switch {
            "按名称排序" => string.CompareOrdinal(GetLabel(), other.GetLabel()),
            _ => 0,
        };
    }
}
