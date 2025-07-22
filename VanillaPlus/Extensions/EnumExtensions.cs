using System;
using System.ComponentModel;
using Dalamud.Utility;

namespace VanillaPlus.Extensions;

public static class EnumExtensions {
    public static string GetDescription(this Enum enumValue) {
        var attribute = enumValue.GetAttribute<DescriptionAttribute>();
        return attribute == null ? enumValue.ToString() : attribute.Description;
    }
}
