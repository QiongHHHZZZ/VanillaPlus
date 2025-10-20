using System;
using System.Linq;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace VanillaPlus.Extensions;

public static unsafe class AddonArgsExtensions {
    public static T* GetAddon<T>(this AddonArgs args) where T : unmanaged
        => (T*)args.Addon.Address;

    public static void PrintAtkValues(this AddonArgs args) {
        var atkValues = args switch {
            AddonRefreshArgs refreshArgs => refreshArgs.AtkValueSpan,
            AddonSetupArgs setupArgs => setupArgs.AtkValueSpan,
            _ => throw new Exception("参数类型无效。"),
        };

        foreach (var index in Enumerable.Range(0, atkValues.Length)) {
            ref var value = ref atkValues[index];
            if (value.Type is 0) continue;
            
            Services.PluginLog.Debug($"[{index,4}]{value.GetValueAsString()}");
        }
    }

    public static Span<AtkValue> GetAtkValues(this AddonArgs args)
        => args.GetAddon<AtkUnitBase>()->AtkValuesSpan;
}
