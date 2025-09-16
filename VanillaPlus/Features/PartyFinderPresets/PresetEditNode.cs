using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;

namespace VanillaPlus.Features.PartyFinderPresets;

public class PresetEditNode : SimpleComponentNode {
    private readonly TextNode textNode;
    private readonly CircleButtonNode editButton;
    private readonly CircleButtonNode deleteButton;

    public PresetEditNode() {
        textNode = new TextNode {
            IsVisible = true,
            AlignmentType = AlignmentType.Left,
        };
        System.NativeController.AttachNode(textNode, this);

        editButton = new CircleButtonNode {
            IsVisible = true,
            Icon = ButtonIcon.Edit,
        };
        System.NativeController.AttachNode(editButton, this);

        deleteButton = new CircleButtonNode {
            IsVisible = true,
            Icon = ButtonIcon.Cross,
        };
        System.NativeController.AttachNode(deleteButton, this);
    }

    protected override void OnSizeChanged() {
        base.OnSizeChanged();

        const float padding = 4.0f;
        
        deleteButton.Size = new Vector2(32.0f, 32.0f);
        deleteButton.Position = new Vector2(Width - deleteButton.Width - padding, Height / 2.0f - deleteButton.Height / 2.0f);
        
        editButton.Size = new Vector2(32.0f, 32.0f);
        editButton.Position = new Vector2(deleteButton.X - editButton.Width - padding, Height / 2.0f - editButton.Height / 2.0f);

        textNode.Size = new Vector2(Width - editButton.X, Height);
        textNode.Position = new Vector2(0.0f, 0.0f);
    }

    public required Action<string> OnDeletePreset {
        init => deleteButton.OnClick = () => value(PresetName);
    }

    public required Action<string> OnEditPreset {
        init => editButton.OnClick = () => value(PresetName);
    }

    public required string PresetName {
        get => textNode.String;
        set => textNode.String = value;
    }
}
