using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using VanillaPlus.Core;

namespace VanillaPlus;

public class WindowBackgroundConfig : GameModificationConfig<WindowBackgroundConfig> {
    protected override string FileName => "WindowBackground.config.json";

    public List<string> Addons = [
        "_ToDoList",
    ];

    public Vector4 Color = KnownColor.Black.Vector() with { W = 0.33f };
    public Vector2 Padding = new(30.0f, 30.0f);
}
