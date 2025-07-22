using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Addon.Events;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using VanillaPlus.Core.Objects;
using VanillaPlus.Extensions;

namespace VanillaPlus.Core.Windows.Parts;

public class ModificationBrowserNode : SimpleComponentNode {

    private readonly HorizontalFlexNode SearchContainer;
    private readonly TextInputNode SearchBox;
    private readonly ScrollingAreaNode<TreeListNode> OptionContainer;
    private readonly ResNode DescriptionContainer;
    private readonly List<TreeListCategoryNode> CategoryNodes = [];

    private const float ItemPadding = 5.0f;

    public ModificationBrowserNode() {
        SearchContainer = new HorizontalFlexNode {
            AlignmentFlags = FlexFlags.FitHeight | FlexFlags.FitWidth,
            IsVisible = true,
        };
        System.NativeController.AttachNode(SearchContainer, this);

        SearchBox = new TextInputNode {
            String = "Search . . . ",
            IsVisible = true,
            OnInputReceived = OnSearchBoxInputReceived,
        };
        SearchContainer.AddNode(SearchBox);

        OptionContainer = new ScrollingAreaNode<TreeListNode> {
            IsVisible = true,
            ContentHeight = 1000.0f,
            ScrollSpeed = 100,
        };
        System.NativeController.AttachNode(OptionContainer, this);

        var groupedOptions = System.ModificationManager.LoadedModifications
                               .Select(option => option)
                               .GroupBy(option => option.Modification.ModificationInfo.Type);

        foreach (var category in groupedOptions) {
            var newCategoryNode = new TreeListCategoryNode {
                IsVisible = true,
                Label = category.Key.GetDescription(),
            };

            foreach (var mod in category) {
                var newOptionNode = new GameModificationOptionNode {
                    Height = 64.0f,
                    Modification = mod,
                    IsVisible = true,
                };

                newCategoryNode.AddNode(newOptionNode);
            }
            
            CategoryNodes.Add(newCategoryNode);
            OptionContainer.ContentNode.AddCategoryNode(newCategoryNode);
        }
        
        DescriptionContainer = new ResNode {
            IsVisible = true,
        };
        System.NativeController.AttachNode(DescriptionContainer, this);
    }

    private void OnSearchBoxInputReceived(SeString searchTerms) {
        
    }

    protected override void OnSizeChanged() {
        SearchContainer.Size = new Vector2(Width, 32.0f);
        OptionContainer.Position = new Vector2(0.0f, SearchContainer.Height + ItemPadding);
        OptionContainer.Size = new Vector2(Width * 3.0f / 5.0f - ItemPadding, Height - SearchContainer.Height - ItemPadding);
        DescriptionContainer.Position = new Vector2(Width * 3.0f / 5.0f, SearchContainer.Height + ItemPadding);
        DescriptionContainer.Size = new Vector2(Width * 2.0f / 5.0f, Height - SearchContainer.Height - ItemPadding);

        foreach (var node in CategoryNodes) {
            node.Width = OptionContainer.ContentNode.Width;
        }
    }
}
