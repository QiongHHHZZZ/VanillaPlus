using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using VanillaPlus.Core.Objects;
using VanillaPlus.Extensions;

namespace VanillaPlus.Core.Windows.Parts;

public class ModificationBrowserNode : SimpleComponentNode {

    private readonly HorizontalFlexNode searchContainerNode;
    private readonly TextInputNode searchBoxNode;
    private readonly TextNode searchLabelNode;
    private readonly ScrollingAreaNode<TreeListNode> optionContainerNode;
    private readonly ResNode descriptionContainerNode;
    private readonly ImGuiImageNode descriptionImageNode;
    private readonly TextNode descriptionImageTextNode;
    private readonly TextNode descriptionTextNode;
    private readonly TextNode descriptionVersionTextNode;
    private readonly TextButtonNode changelogButtonNode;

    private const float ItemPadding = 5.0f;

    private GameModificationOptionNode? selectedOption;
    
    private ChangelogBrowser? changelogBrowser;

    private readonly List<TreeListCategoryNode> categoryNodes = [];
    private readonly List<GameModificationOptionNode> modificationOptionNodes = [];

    public ModificationBrowserNode() {
        changelogBrowser = new ChangelogBrowser {
            InternalName = "VPChangelog",
            Title = "Vanilla Plus Changelog Browser",
            NativeController = System.NativeController,
            Size = new Vector2(450.0f, 400.0f),
        };
        
        optionContainerNode = new ScrollingAreaNode<TreeListNode> {
            IsVisible = true,
            ContentHeight = 1000.0f,
            ScrollSpeed = 100,
        };
        System.NativeController.AttachNode(optionContainerNode, this);

        descriptionContainerNode = new ResNode {
            IsVisible = true,
        };
        System.NativeController.AttachNode(descriptionContainerNode, this);
        
        descriptionImageNode = new ImGuiImageNode();
        System.NativeController.AttachNode(descriptionImageNode, descriptionContainerNode);

        descriptionTextNode = new TextNode {
            AlignmentType = AlignmentType.Center,
            TextFlags = TextFlags.WordWrap | TextFlags.MultiLine,
            FontSize = 14,
            LineSpacing = 22,
            FontType = FontType.Axis,
            IsVisible = true,
            Text = "Please select an option on the left",
        };
        System.NativeController.AttachNode(descriptionTextNode, descriptionContainerNode);
        
        descriptionImageTextNode = new TextNode();
        System.NativeController.AttachNode(descriptionImageTextNode, descriptionContainerNode);

        changelogButtonNode = new TextButtonNode {
            Label = "Changelog",
            OnClick = OnChangelogButtonClicked,
        };
        System.NativeController.AttachNode(changelogButtonNode, descriptionContainerNode);
        
        descriptionVersionTextNode = new TextNode {
            IsVisible = true,
            AlignmentType = AlignmentType.BottomRight,
            TextColor = ColorHelper.GetColor(3),
        };
        System.NativeController.AttachNode(descriptionVersionTextNode, descriptionContainerNode);
                
        searchContainerNode = new HorizontalFlexNode {
            AlignmentFlags = FlexFlags.FitHeight | FlexFlags.FitWidth,
            IsVisible = true,
        };
        System.NativeController.AttachNode(searchContainerNode, this);
        
        searchBoxNode = new TextInputNode {
            IsVisible = true,
            OnInputReceived = OnSearchBoxInputReceived,
        };
        searchContainerNode.AddNode(searchBoxNode);

        searchLabelNode = new TextNode {
            Position = new Vector2(10.0f, 6.0f),
            IsVisible = true,
            TextColor = ColorHelper.GetColor(3),
            Text = "Search . . .",
        };
        System.NativeController.AttachNode(searchLabelNode, searchBoxNode);

        searchBoxNode.OnFocused += () => {
            searchLabelNode.IsVisible = false;
        };

        searchBoxNode.OnUnfocused += () => {
            if (searchBoxNode.String.ToString() is "") {
                searchLabelNode.IsVisible = true;
            }
        };

        var groupedOptions = System.ModificationManager.LoadedModifications
                               .Select(option => option)
                               .GroupBy(option => option.Modification.ModificationInfo.Type);

        foreach (var category in groupedOptions) {
            var newCategoryNode = new TreeListCategoryNode {
                IsVisible = true,
                Label = category.Key.GetDescription(),
                OnToggle = isVisible => OnCategoryToggled(isVisible, category.Key),
            };

            foreach (var mod in category) {
                var newOptionNode = new GameModificationOptionNode {
                    Height = 42.0f,
                    Modification = mod,
                    IsVisible = true,
                };

                newOptionNode.OnClick = () => OnOptionClicked(newOptionNode);

                newCategoryNode.AddNode(newOptionNode);
                modificationOptionNodes.Add(newOptionNode);
            }
            
            categoryNodes.Add(newCategoryNode);
            optionContainerNode.ContentNode.AddCategoryNode(newCategoryNode);
        }

        RecalculateScrollableAreaSize();
    }

    private void OnCategoryToggled(bool isVisible, ModificationType type) {
        var selectionCategory = selectedOption?.Modification.Modification.ModificationInfo.Type;
        if (selectionCategory is not null) {
            if (!isVisible && selectionCategory == type) {
                ClearSelection();
            }
        }

        RecalculateScrollableAreaSize();
    }
    
    private void OnSearchBoxInputReceived(SeString searchTerm) {
        List<GameModificationOptionNode> validOptions = [];
        
        foreach (var option in modificationOptionNodes) {
            var isTarget = option.ModificationInfo.IsMatch(searchTerm.ToString());
            option.IsVisible = isTarget;

            if (isTarget) {
                validOptions.Add(option);
            }
        }

        foreach (var categoryNode in categoryNodes) {
            categoryNode.IsVisible = validOptions.Any(option => option.ModificationInfo.Type.GetDescription() == categoryNode.Label.ToString());
            categoryNode.RecalculateLayout();
        }
        
        optionContainerNode.ContentNode.RefreshLayout();
        RecalculateScrollableAreaSize();
    }

    private void OnOptionClicked(GameModificationOptionNode option) {
        ClearSelection();
        
        selectedOption = option;
        selectedOption.IsSelected = true;

        if (selectedOption.Modification.Modification.GetDescriptionImage() is { } image) {
            descriptionImageNode.LoadTexture(image);
            descriptionImageNode.IsVisible = true;
            descriptionImageTextNode.IsVisible = true;
            descriptionTextNode.IsVisible = false;
        }
        else {
            descriptionImageNode.IsVisible = false;
            descriptionImageTextNode.IsVisible = false;
            descriptionTextNode.IsVisible = true;
            descriptionTextNode.Text = selectedOption.Modification.Modification.ModificationInfo.Description;
        }

        changelogButtonNode.IsVisible = true;
        descriptionVersionTextNode.IsVisible = true;
        descriptionVersionTextNode.Text = $"Version {selectedOption.Modification.Modification.ModificationInfo.Version}";
    }

    private void ClearSelection() {
        selectedOption = null;
        foreach (var node in modificationOptionNodes) {
            node.IsSelected = false;
            node.IsHovered = false;
        }

        descriptionTextNode.Text = "Please select an option on the left";

        descriptionImageNode.IsVisible = false;
        descriptionImageTextNode.IsVisible = false;
        descriptionVersionTextNode.IsVisible = false;
        changelogButtonNode.IsVisible = false;
    }

    private void OnChangelogButtonClicked() {
        if (changelogBrowser is not null && selectedOption is not null) {
            if (changelogBrowser.IsOpen) {
                changelogBrowser.Close();
            }

            if (System.SystemConfig.ChangelogWindowPosition is { } changelogPosition) {
                changelogBrowser.Position = changelogPosition;
            }
            
            changelogBrowser.Modification = selectedOption.Modification.Modification;
            changelogBrowser.Title = $"{selectedOption.ModificationInfo.DisplayName} Changelog";
            changelogBrowser.Open();
        }
    }

    private void RecalculateScrollableAreaSize() {
        optionContainerNode.ContentHeight = categoryNodes.Sum(node => node.Height) + 5.0f;
    }

    private void ResetSearch() {
        foreach (var category in categoryNodes) {
            category.IsCollapsed = false;
        }

        foreach (var option in modificationOptionNodes) {
            option.IsVisible = true;
        }
        
        optionContainerNode.ContentNode.RefreshLayout();
    }

    protected override void OnSizeChanged() {
        searchContainerNode.Size = new Vector2(Width, 28.0f);
        optionContainerNode.Position = new Vector2(0.0f, searchContainerNode.Height + ItemPadding);
        optionContainerNode.Size = new Vector2(Width * 3.0f / 5.0f - ItemPadding, Height - searchContainerNode.Height - ItemPadding);
        descriptionContainerNode.Position = new Vector2(Width * 3.0f / 5.0f, searchContainerNode.Height + ItemPadding);
        descriptionContainerNode.Size = new Vector2(Width * 2.0f / 5.0f, Height - searchContainerNode.Height - ItemPadding);

        descriptionImageNode.Size = new Vector2(descriptionContainerNode.Width * 0.66f, descriptionContainerNode.Width * 0.66f);
        descriptionImageNode.Position = new Vector2(descriptionContainerNode.Width * 0.33f / 2.0f, descriptionContainerNode.Width * 0.33f / 4.0f);

        changelogButtonNode.Size = new Vector2(125.0f, 28.0f);
        changelogButtonNode.Position = new Vector2(0.0f, descriptionContainerNode.Height - changelogButtonNode.Height - ItemPadding);
        
        descriptionVersionTextNode.Size = new Vector2(200.0f, 28.0f);
        descriptionVersionTextNode.Position = descriptionContainerNode.Size - descriptionVersionTextNode.Size - new Vector2(8.0f, 8.0f);
        
        descriptionTextNode.Size = descriptionContainerNode.Size - new Vector2(16.0f, 16.0f) - new Vector2(0.0f, descriptionVersionTextNode.Height);
        descriptionTextNode.Position = new Vector2(8.0f, 8.0f);
        
        foreach (var node in categoryNodes) {
            node.Width = optionContainerNode.ContentNode.Width;
        }
    }
}
