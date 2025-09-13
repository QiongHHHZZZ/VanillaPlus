using KamiToolKit.System;

namespace VanillaPlus.BasicAddons.Config.EntryTypes;

public interface IConfigEntry {
    NodeBase BuildNode();
}
