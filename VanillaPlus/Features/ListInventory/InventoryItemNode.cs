﻿using System.Numerics;
using Dalamud.Game.Addon.Events;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;
using VanillaPlus.NativeElements.Nodes;

namespace VanillaPlus.Features.ListInventory;

public unsafe class InventoryItemNode : SimpleComponentNode {
    
    private readonly NineGridNode hoveredBackgroundNode;

    private readonly IconWithCountNode iconNode;
    private readonly TextNode itemNameTextNode;
    private readonly TextNode levelTextNode;
    private readonly TextNode itemLevelTextNode;

    public InventoryItemNode() {
        hoveredBackgroundNode = new SimpleNineGridNode {
            NodeId = 2,
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

        iconNode = new IconWithCountNode {
            NodeId = 3,
            IsVisible = true,
        };
        System.NativeController.AttachNode(iconNode, this);

        itemNameTextNode = new TextNode {
            NodeId = 5,
            IsVisible = true,
            AlignmentType = AlignmentType.Left,
            TextFlags = TextFlags.Ellipsis,
        };
        System.NativeController.AttachNode(itemNameTextNode, this);
        
        levelTextNode = new TextNode {
            NodeId = 6,
            IsVisible = true,
            AlignmentType = AlignmentType.Left,
        };
        System.NativeController.AttachNode(levelTextNode, this);

        itemLevelTextNode = new TextNode {
            NodeId = 7,
            IsVisible = true,
            AlignmentType = AlignmentType.Left,
        };
        System.NativeController.AttachNode(itemLevelTextNode, this);
        
        CollisionNode.AddEvent(AddonEventType.MouseOver, _ => {
            IsHovered = true;

            if (Item is null) return;
            var node = (AtkResNode*)CollisionNode;

            node->ShowInventoryItemTooltip(Item.Item.Container, Item.Item.Slot);
        });
        
        CollisionNode.AddEvent(AddonEventType.MouseOut, _ => {
            IsHovered = false;
            CollisionNode.HideTooltip();
        });
    }
    
    public bool IsHovered {
        get => hoveredBackgroundNode.IsVisible;
        private set => hoveredBackgroundNode.IsVisible = value;
    }

    public ItemInfo? Item {
        get;
        set {
            if (value is null) {
                field = null;
                return;
            }

            field = value;
            var item = value.Item.GetLinkedItem();

            iconNode.IconId = item->GetIconId();
            itemNameTextNode.ReadOnlySeString = item->GetItemName();
            iconNode.Count = value.ItemCount;

            if (value.Level > 1) {
                levelTextNode.String = $"等级 {value.Level, 3}";
            }
            else {
                levelTextNode.String = string.Empty;
            }
            
            if (value.ItemLevel > 1) {
                itemLevelTextNode.String = $"装等 {value.ItemLevel, 3}";
            }
            else {
                itemLevelTextNode.String = string.Empty;
            }
        }
    }
    
    protected override void OnSizeChanged() {
        base.OnSizeChanged();

        iconNode.Size = new Vector2(Height, Height);
        iconNode.Position = Vector2.Zero;

        itemLevelTextNode.Size = new Vector2(64.0f, Height);
        itemLevelTextNode.Position = new Vector2(Width - itemLevelTextNode.Width, 0.0f);
        
        levelTextNode.Size = new Vector2(64.0f, Height);
        levelTextNode.Position = new Vector2(Width - levelTextNode.Width - itemLevelTextNode.Width, 0.0f);

        itemNameTextNode.Size = new Vector2(Width - iconNode.Width - itemLevelTextNode.Width - levelTextNode.Width - 8.0f, Height);
        itemNameTextNode.Position = new Vector2(iconNode.Width + 4.0f, 0.0f);

        hoveredBackgroundNode.Size = Size + new Vector2(6.0f, 6.0f);
        hoveredBackgroundNode.Position = new Vector2(-3.0f, -3.0f);
    }
}
