﻿using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Addon;
using KamiToolKit.Nodes;

namespace VanillaPlus.NativeElements.Addons;

public class RenameAddon : NativeAddon {
    private TextInputNode? inputNode;
    private TextButtonNode? confirmButton;
    private TextButtonNode? cancelButton;

    public Action<string>? ResultCallback { get; set; }
    
    protected override unsafe void OnSetup(AtkUnitBase* addon) {
        inputNode = new TextInputNode {
            Position = ContentStartPosition + new Vector2(0.0f, ContentPadding.Y),
            Size = new Vector2(ContentSize.X, 28.0f),
            IsVisible = true,
            PlaceholderString = PlaceholderString,
            String = DefaultString,
            AutoSelectAll = AutoSelectAll,
        };
        AttachNode(inputNode);

        var buttonSize = new Vector2(100.0f, 24.0f);
        var targetYPos = ContentSize.Y - buttonSize.Y + ContentStartPosition.Y;
        
        confirmButton = new TextButtonNode {
            Position = new Vector2(ContentStartPosition.X, targetYPos),
            Size = buttonSize,
            IsVisible = true,
            String = "确定",
            OnClick = () => {
                ResultCallback?.Invoke(inputNode.String);
                Close();
            },
        };
        AttachNode(confirmButton);

        cancelButton = new TextButtonNode {
            Position = new Vector2(ContentSize.X - buttonSize.X + ContentPadding.X, targetYPos),
            Size = buttonSize,
            IsVisible = true,
            String = "取消",
            OnClick = Close,
        };
        AttachNode(cancelButton);
    }

    public string PlaceholderString { get; set; } = string.Empty;
    public string DefaultString { get; set; } = string.Empty;
    public bool AutoSelectAll { get; set; }
}
