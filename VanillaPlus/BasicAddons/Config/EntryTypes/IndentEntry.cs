using System;
using KamiToolKit.System;

namespace VanillaPlus.BasicAddons.Config.EntryTypes;

public class IndentEntry : IConfigEntry {
    public NodeBase BuildNode()
        => throw new InvalidOperationException();
}
