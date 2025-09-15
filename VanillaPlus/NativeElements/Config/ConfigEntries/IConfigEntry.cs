using KamiToolKit.System;

namespace VanillaPlus.NativeElements.Config.ConfigEntries;

public interface IConfigEntry {
    NodeBase BuildNode();
}
