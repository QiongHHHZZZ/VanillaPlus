using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Addon;
using KamiToolKit.Nodes;
using VanillaPlus.Classes;
using VanillaPlus.Extensions;

namespace VanillaPlus.Basic_Addons;

public record BoolConfigEntry(string Category, string Label, bool InitialState, ISavable ConfigObject, MemberInfo ConfigMemberInfo) {
    public void OnOptionChanged(bool newValue) {
        ConfigMemberInfo.SetValue(ConfigObject, newValue);
        ConfigObject.Save();
    }
}

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
        AttachNode(mainListNode);

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
                var newCheckboxNode = new CheckboxNode {
                    OnClick = option.OnOptionChanged,
                    Height = 24.0f,
                    String = option.Label,
                    IsChecked = option.InitialState,
                    IsVisible = true,
                };
                
                tabbedListNode.AddNode(newCheckboxNode);
            }
            
            tabbedListNode.AddTab(-1);
        }
        
        mainListNode.ContentHeight = mainListNode.ContentNode.Height;
    }

    protected override unsafe void OnHide(AtkUnitBase* addon)
        => OnClose();

    public required Action OnClose { get; init; }

    public void AddConfigEntry(string category, string label, ISavable config, string memberName) {
        var memberInfo = config.GetType().GetMember(memberName).FirstOrDefault();
        if (memberInfo is null) return;
        
        var initialValue = memberInfo.GetValue<bool>(config);
        
        entries.Add(new BoolConfigEntry(category, label, initialValue, config, memberInfo));
    }
}
