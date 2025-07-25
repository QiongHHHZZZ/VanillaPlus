using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

namespace VanillaPlus.Extensions;

public static class DataManagerExtensions {
    public static ReadOnlySeString GetAddonText(this IDataManager dataManager, uint id)
        => dataManager.GetExcelSheet<Addon>().GetRow(id).Text;
}
