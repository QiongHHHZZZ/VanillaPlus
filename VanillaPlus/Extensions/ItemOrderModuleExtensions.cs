using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace VanillaPlus.Extensions;

public static unsafe class ItemOrderModuleExtensions {
    public static ItemOrderModuleSorter* GetCurrentRetainerSorter(ref this ItemOrderModule instance)
        => instance.RetainerSorter[instance.ActiveRetainerId];
}
