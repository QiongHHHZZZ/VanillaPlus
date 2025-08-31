using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;

namespace VanillaPlus.Basic_Addons;

public unsafe class SearchableNodeListAddon : NodeListAddon {

    private TextInputNode? textInputNode;
    private TextNode? searchLabelNode;
    private TextDropDownNode? sortDropdownNode;
    
    private VerticalListNode? mainContainerNode;
    private HorizontalFlexNode? searchContainerNode;
    private HorizontalListNode? widgetsContainerNode;
    private CircleButtonNode? reverseButtonNode;
    
    private bool reverseSort;
    private string searchText = string.Empty;
    private string filterOption = string.Empty;
    
    public required List<string> DropDownOptions { get; init; }
    
    protected override void OnSetup(AtkUnitBase* addon) {
        const float dropDownWidth = 175.0f;

        mainContainerNode = new VerticalListNode {
            Position = ContentStartPosition,
            Size = ContentSize,
            IsVisible = true,
        };

        searchContainerNode = new HorizontalFlexNode {
            Size = new Vector2(ContentSize.X, 28.0f),
            AlignmentFlags = FlexFlags.FitHeight | FlexFlags.FitWidth,
            IsVisible = true,
        };

        widgetsContainerNode = new HorizontalListNode {
            Size = new Vector2(ContentSize.X, 28.0f),
            Alignment = HorizontalListAnchor.Right,
            IsVisible = true,
        };

        sortDropdownNode = new TextDropDownNode {
            Size = new Vector2(dropDownWidth, 28.0f),
            MaxListOptions = DropDownOptions.Count,
            Options = DropDownOptions,
            IsVisible = true,
            OnOptionSelected = newOption => {
                filterOption = newOption;
                OnFilterUpdated(newOption, reverseSort);
            },
        };
        sortDropdownNode.SelectedOption = DropDownOptions.First();

        reverseButtonNode = new CircleButtonNode {
            Size = new Vector2(28.0f, 28.0f),
            Icon = ButtonIcon.Sort,
            IsVisible = true,
            OnClick = () => {
                reverseSort = !reverseSort;
                OnFilterUpdated(filterOption, reverseSort);
            },
            Tooltip = "Reverse Sort Direction",
        };

        textInputNode = new TextInputNode {
            IsVisible = true,
        };
        textInputNode.SeString = searchText;

        searchLabelNode = new TextNode {
            Position = new Vector2(10.0f, 6.0f),
            TextColor = ColorHelper.GetColor(3),
            String = "Search . . .",
            IsVisible = searchText.IsNullOrEmpty(),
        };

        textInputNode.OnFocused += () => {
            searchLabelNode.IsVisible = false;
        };

        textInputNode.OnUnfocused += () => {
            searchLabelNode.IsVisible = searchText.IsNullOrEmpty();
        };

        textInputNode.OnInputReceived += newSearchString => {
            searchText = newSearchString.ToString();
            OnSearchUpdated(searchText);
        };
        
        const float listPadding = 4.0f;
        
        ScrollingAreaNode = new ScrollingAreaNode<VerticalListNode> {
            Size = ContentSize - new Vector2(0.0f, searchContainerNode.Height + widgetsContainerNode.Height + listPadding),
            Position = new Vector2(0.0f, listPadding),
            ContentHeight = 1000.0f,
            IsVisible = true,
        };
        
        AttachNode(mainContainerNode);
        mainContainerNode.AddNode(searchContainerNode);
        searchContainerNode.AddNode(textInputNode);
        mainContainerNode.AddNode(widgetsContainerNode);
        widgetsContainerNode.AddNode(reverseButtonNode);

        sortDropdownNode.Width = widgetsContainerNode.AreaRemaining;
        widgetsContainerNode.AddNode(sortDropdownNode);

        AttachNode(searchLabelNode, textInputNode);

        mainContainerNode.AddDummy(4.0f);
        mainContainerNode.AddNode(ScrollingAreaNode);
    }
    
    protected override void OnFinalize(AtkUnitBase* addon) {
        System.NativeController.DisposeNode(ref sortDropdownNode);
        System.NativeController.DisposeNode(ref ScrollingAreaNode);
        System.NativeController.DisposeNode(ref searchLabelNode);
        System.NativeController.DisposeNode(ref textInputNode);
    }

    public delegate void SearchUpdated(string searchString);
    public delegate void FilterUpdated(string filterString, bool reversed);

    public required SearchUpdated OnSearchUpdated { get; init; }
    public required FilterUpdated OnFilterUpdated { get; init; }
}
