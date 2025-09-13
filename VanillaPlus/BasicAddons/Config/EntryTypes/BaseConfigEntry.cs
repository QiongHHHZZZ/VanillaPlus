using System.Reflection;
using KamiToolKit.System;
using VanillaPlus.Classes;

namespace VanillaPlus.BasicAddons.Config.EntryTypes;

public abstract class BaseConfigEntry : IConfigEntry {
    public required string Label { get; init; }
    public required MemberInfo MemberInfo { get; init; }
    public required ISavable Config { get; init; }

    public abstract NodeBase BuildNode();
}
