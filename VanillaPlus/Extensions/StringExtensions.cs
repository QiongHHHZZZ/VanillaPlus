using System.Globalization;

namespace VanillaPlus.Extensions;

public static class StringExtensions {
    public static string ToTitleCase(this string str)
        => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str);
}
