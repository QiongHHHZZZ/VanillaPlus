using Lumina.Text.ReadOnly;

namespace VanillaPlus.Features.RecentlyLootedWindow;

public record LootedItemInfo(int Index, uint ItemId, uint IconId, ReadOnlySeString Name, int Quantity);
