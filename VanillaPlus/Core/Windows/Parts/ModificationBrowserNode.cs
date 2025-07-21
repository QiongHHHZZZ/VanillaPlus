using System.Numerics;
using Dalamud.Game.Text.SeStringHandling;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;

namespace VanillaPlus.Core.Windows.Parts;

public class ModificationBrowserNode : SimpleComponentNode {

    public readonly HorizontalFlexNode SearchContainer;
    public readonly TextInputNode SearchBox;
    public readonly ResNode OptionContainer;
    public readonly ResNode DescriptionContainer;

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

        OptionContainer = new ResNode {
            IsVisible = true,
        };
        System.NativeController.AttachNode(OptionContainer, this);

        DescriptionContainer = new ResNode {
            IsVisible = true,
        };
        System.NativeController.AttachNode(DescriptionContainer, this);
    }

    private void OnSearchBoxInputReceived(SeString searchTerms) {
        
    }

    public override float Height {
        get => base.Height;
        set {
            base.Height = value;
            RecalculateLayout();
        }
    }

    public override float Width {
        get => base.Width;
        set {
            base.Width = value;
            RecalculateLayout();
        }
    }

    private void RecalculateLayout() {
        SearchContainer.Size = new Vector2(Width, 32.0f);
        OptionContainer.Position = new Vector2(0.0f, SearchContainer.Height + ItemPadding);
        OptionContainer.Size = new Vector2(Width * 3.0f / 5.0f - ItemPadding, Height - SearchContainer.Height - ItemPadding);
        DescriptionContainer.Position = new Vector2(Width * 3.0f / 5.0f, SearchContainer.Height + ItemPadding);
        DescriptionContainer.Size = new Vector2(Width * 2.0f / 5.0f, Height - SearchContainer.Height - ItemPadding);
    }
}
