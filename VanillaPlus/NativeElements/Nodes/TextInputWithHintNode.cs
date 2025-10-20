﻿using System;
using System.Numerics;
using Dalamud.Utility;
using KamiToolKit.Nodes;
using Lumina.Text;
using SeString = Dalamud.Game.Text.SeStringHandling.SeString;

namespace VanillaPlus.NativeElements.Nodes;

public class TextInputWithHintNode : SimpleComponentNode {
    private readonly TextInputNode textInputNode;
    private readonly ImageNode helpNode;

    public TextInputWithHintNode() {
        textInputNode = new TextInputNode {
            PlaceholderString = "搜索……",
            IsVisible = true,
        };
        System.NativeController.AttachNode(textInputNode, this);

        helpNode = new SimpleImageNode {
            TexturePath = "ui/uld/CircleButtons.tex",
            TextureCoordinates = new Vector2(112.0f, 84.0f),
            TextureSize = new Vector2(28.0f, 28.0f),
            Tooltip = new SeStringBuilder()
                .Append("[VanillaPlus]: 支持正则表达式搜索")
                .AppendNewLine()
                .Append("在开头输入 '$' 可按描述内容进行搜索")
                .ToReadOnlySeString()
                .ToDalamudString(),
            EventFlagsSet = true,
            IsVisible = true,
        };
        System.NativeController.AttachNode(helpNode, this);
    }

    public required Action<SeString>? OnInputReceived {
        get => textInputNode.OnInputReceived;
        set => textInputNode.OnInputReceived = value;
    }

    protected override void OnSizeChanged() {
        base.OnSizeChanged();

        helpNode.Size = new Vector2(Height, Height);
        helpNode.Position = new Vector2(Width - helpNode.Width - 5.0f, 0.0f);

        textInputNode.Size = new Vector2(Width - helpNode.Width - 5.0f, Height);
        textInputNode.Position = new Vector2(0.0f, 0.0f);
    }

    public SeString SearchString {
        get => textInputNode.SeString;
        set => textInputNode.SeString = value;
    }
}
