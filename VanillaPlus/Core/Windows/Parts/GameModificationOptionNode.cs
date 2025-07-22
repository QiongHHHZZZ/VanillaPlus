using System;
using System.Numerics;
using Dalamud.Game.Addon.Events;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using VanillaPlus.Core.Objects;

namespace VanillaPlus.Core.Windows.Parts;

public class GameModificationOptionNode : SimpleComponentNode {

    private readonly NineGridNode hoveredBackgroundNode;
    private readonly NineGridNode selectedBackgroundNode;
    private readonly CheckboxNode checkboxNode;
    private readonly TextNode modificationNameNode;
    private readonly TextNode authorNamesNode;
    private readonly CircleButtonNode configButtonNode;

    public GameModificationOptionNode() {
        hoveredBackgroundNode = new SimpleNineGridNode {
            TexturePath = "ui/uld/ListItemA.tex",
            TextureCoordinates = new Vector2(0.0f, 22.0f),
            TextureSize = new Vector2(64.0f, 22.0f),
            TopOffset = 6,
            BottomOffset = 6,
            LeftOffset = 16,
            RightOffset = 1,
            IsVisible = false,
        };
        System.NativeController.AttachNode(hoveredBackgroundNode, this);
        
        selectedBackgroundNode = new SimpleNineGridNode {
            TexturePath = "ui/uld/ListItemA.tex",
            TextureCoordinates = new Vector2(0.0f, 0.0f),
            TextureSize = new Vector2(64.0f, 22.0f),
            TopOffset = 6,
            BottomOffset = 6,
            LeftOffset = 16,
            RightOffset = 1,
            IsVisible = false,
        };
        System.NativeController.AttachNode(selectedBackgroundNode, this);
        
        checkboxNode = new CheckboxNode {
            Origin = new Vector2(8.0f, 8.0f),
            Scale = new Vector2(2.0f, 2.0f),
            IsVisible = true,
            OnClick = ToggleModification,
        };
        System.NativeController.AttachNode(checkboxNode, this);

        modificationNameNode = new TextNode {
            IsVisible = true,
            TextFlags = TextFlags.AutoAdjustNodeSize,
            TextFlags2 = TextFlags2.Ellipsis,
            AlignmentType = AlignmentType.BottomLeft,
        };
        System.NativeController.AttachNode(modificationNameNode, this);

        authorNamesNode = new TextNode {
            IsVisible = true,
            TextFlags = TextFlags.AutoAdjustNodeSize,
            TextFlags2 = TextFlags2.Ellipsis,
            AlignmentType = AlignmentType.TopLeft,
            TextColor = ColorHelper.GetColor(3),
        };
        System.NativeController.AttachNode(authorNamesNode, this);

        configButtonNode = new CircleButtonNode {
            Icon = ButtonIcon.GearCog,
            Tooltip = "Open configuration window",
        };
        System.NativeController.AttachNode(configButtonNode, this);
        
        CollisionNode.AddEvent(AddonEventType.MouseOver, _ => {
            if (!IsSelected) {
                IsHovered = true;
            }
        });
        
        CollisionNode.AddEvent(AddonEventType.MouseDown, _ => {
            OnClick?.Invoke();
        });
        
        CollisionNode.AddEvent(AddonEventType.MouseOut, _ => {
            IsHovered = false;
        });
    }

    public required LoadedModification Modification {
        get;
        set {
            field = value;
            modificationNameNode.Text = value.Modification.ModificationInfo.DisplayName;
            authorNamesNode.Text = $"By {string.Join(", ", value.Modification.ModificationInfo.Authors)}";

            if (value.Modification.OpenConfig is not null) {
                if (value.Modification.OpenConfig.GetInvocationList().Length != 0) {
                    configButtonNode.IsVisible = true;
                }
            }
            
            checkboxNode.IsChecked = value.State is LoadedState.Enabled;
        }
    }
    
    private void ToggleModification(bool shouldEnableModification) {
        if (shouldEnableModification && Modification.State is LoadedState.Disabled) {
            System.ModificationManager.TryEnableModification(Modification);
        }
        else if (!shouldEnableModification && Modification.State is LoadedState.Enabled) {
            System.ModificationManager.TryDisableModification(Modification);
        }
    }

    public Action? OnClick { get; set; }
    
    public bool IsHovered {
        get => hoveredBackgroundNode.IsVisible;
        set => hoveredBackgroundNode.IsVisible = value;
    }
    
    public bool IsSelected {
        get => selectedBackgroundNode.IsVisible;
        set {
            selectedBackgroundNode.IsVisible = value;
            hoveredBackgroundNode.IsVisible = !value;
        }
    }

    protected override void OnSizeChanged() {
        hoveredBackgroundNode.Size = Size;
        selectedBackgroundNode.Size = Size;

        checkboxNode.Size = new Vector2(16.0f, 16.0f);
        checkboxNode.Position = new Vector2(Height / 4.0f + checkboxNode.Width / 2.0f, Height / 2.0f - checkboxNode.Height / 2.0f - 4.0f);

        modificationNameNode.Height = Height / 2.0f;
        modificationNameNode.Position = new Vector2(Height, 0.0f);
        
        authorNamesNode.Height = Height / 2.0f;
        authorNamesNode.Position = new Vector2(Height + Height / 2.0f, Height / 2.0f);

        configButtonNode.Size = new Vector2(Height / 2.0f, Height / 2.0f);
        configButtonNode.Position = new Vector2(Width - Height, Height / 2.0f - configButtonNode.Height / 2.0f);
    }
}
