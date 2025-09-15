using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Addon;
using KamiToolKit.Nodes;
using VanillaPlus.Classes;

namespace VanillaPlus.NativeElements.Config;

public unsafe class ConfigAddon : NativeAddon {
    private ScrollingAreaNode<VerticalListNode>? configurationListNode;

    private readonly List<ConfigCategory> configCategories = [];
    
    public required ISavable Config { get; init; }

    protected override void OnSetup(AtkUnitBase* addon) { // todo: add option to hide scrollbar
        configurationListNode = new ScrollingAreaNode<VerticalListNode> {
            Size = ContentSize + new Vector2(0.0f, ContentPadding.Y),
            Position = ContentStartPosition - new Vector2(0.0f, ContentPadding.Y),
            IsVisible = true,
            ContentHeight = ContentSize.Y,
            AutoHideScrollBar = true,
        };
        AttachNode(configurationListNode);

        foreach (var category in configCategories) {
            var listNode = category.BuildNode();
            configurationListNode.ContentNode.AddNode(listNode);

            listNode.Width = configurationListNode.ContentNode.Width;
            listNode.RecalculateLayout();
        }
        
        configurationListNode.ContentHeight = configurationListNode.ContentNode.Nodes.Sum(node => node.Height);
    }

    public ConfigCategory AddCategory(string label) {
        var newCategory = new ConfigCategory {
            CategoryLabel = label,
            ConfigObject = Config,
        };
        
        configCategories.Add(newCategory);
        return newCategory;
    }
}
