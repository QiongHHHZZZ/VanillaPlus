using System;
using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Addon;
using KamiToolKit.Nodes;

namespace VanillaPlus.Basic_Addons;

public record BoolConfigEntry(string Category, string Label, bool InitialState, Action<bool> Callback);

public class BoolConfigAddon : NativeAddon {

    private ScrollingAreaNode<TabbedVerticalListNode>? mainListNode;
    
    private readonly List<BoolConfigEntry> entries = [];

    protected override unsafe void OnSetup(AtkUnitBase* addon) {
        mainListNode = new ScrollingAreaNode<TabbedVerticalListNode> {
            Size = ContentSize,
            Position = ContentStartPosition,
            IsVisible = true,
            ContentHeight = ContentSize.Y,
        };

        var tabbedListNode = mainListNode.ContentNode;

        var isFirstNode = true;

        foreach (var categoryGroups in entries.GroupBy(entry => entry.Category)) {
            if (isFirstNode) {
                isFirstNode = false;
            }
            else {
                tabbedListNode.AddNode(new ResNode {
                    Height = 8.0f,
                    IsVisible = true,
                });
            }
            
            tabbedListNode.AddNode(new SimpleLabelNode {
                String = categoryGroups.Key,
            });
            
            tabbedListNode.AddTab(1);
            
            foreach (var option in categoryGroups) {
                tabbedListNode.AddNode(new CheckboxNode {
                    OnClick = option.Callback,
                    Height = 24.0f,
                    String = option.Label,
                    IsChecked = option.InitialState,
                    IsVisible = true,
                });
            }
            
            tabbedListNode.AddTab(-1);
        }
        
        mainListNode.ContentHeight = mainListNode.ContentNode.Height;
        AttachNode(mainListNode);
    }

    protected override unsafe void OnHide(AtkUnitBase* addon)
        => OnClose();

    public void AddConfigEntry(BoolConfigEntry entry)
        => entries.Add(entry);

    public required Action OnClose { get; init; }
}
