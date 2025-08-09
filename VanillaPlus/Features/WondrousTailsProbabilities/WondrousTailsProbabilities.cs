using System.Linq;
using System.Numerics;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.WondrousTailsProbabilities;

public unsafe class WondrousTailsProbabilities : GameModification {
    public override ModificationInfo ModificationInfo => new() {
        DisplayName = "Wondrous Tails Probabilities",
        Description = "Displays current line probabilities and average reroll probabilities in the Wondrous Tails Book.",
        Authors = [ "MidoriKami" ],
        Type = ModificationType.UserInterface,
        ChangeLog = [
            new ChangeLogInfo(1, "Initial Implementation"),
        ],
        CompatabilityModule = new PluginCompatabilityModule("WondrousTailsSolver"),
    };

    private AddonController<AddonWeeklyBingo>? weeklyBingoController;
    private TextNode? probabilityTextNode;
    private PerfectTails? perfectTails;

    public override string ImageName => "WondrousTailsProbabilities.png";

    public override void OnEnable() {
        perfectTails = new PerfectTails();

        weeklyBingoController = new AddonController<AddonWeeklyBingo>("WeeklyBingo");
        weeklyBingoController.OnAttach += AttachNodes;
        weeklyBingoController.OnDetach += DetachNodes;
        weeklyBingoController.OnRefresh += RefreshNodes;
        weeklyBingoController.Enable();
    }

    public override void OnDisable()
        => weeklyBingoController?.Dispose();

    private void AttachNodes(AddonWeeklyBingo* addon) {
        if (perfectTails is null) return;
        
        var existingTextNode = addon->GetTextNodeById(34);
        if (existingTextNode is null) return;

        // Shrink existing node, the game doesn't need that space anyway.
        existingTextNode->SetHeight((ushort)(existingTextNode->GetHeight() * 2.0f / 3.0f));

        // Add new custom text node to ui
        probabilityTextNode = new TextNode {
            Size = new Vector2(290.0f, 36.0f),
            Position = new Vector2(68.0f, 469.0f),
            TextColor = ColorHelper.GetColor(1),
            LineSpacing = 18,
            TextFlags = TextFlags.MultiLine | TextFlags.Edge | TextFlags.WordWrap,
            Text = perfectTails.SolveAndGetProbabilitySeString(),
            IsVisible = true,
        };
        System.NativeController.AttachNode(probabilityTextNode, (AtkResNode*)existingTextNode, NodePosition.AfterTarget);
    }
    
    private void RefreshNodes(AddonWeeklyBingo* addon) {
        if (perfectTails is null) return;
        var existingTextNode = addon->GetTextNodeById(34);
        if (existingTextNode is not null) {
            var nodeText = SeString.Parse(existingTextNode->NodeText);

            // Get the first index of the double-newline payloads
            var lineBreakIndex = -1;
            for (var index = 0; index < nodeText.Payloads.Count; index++) {
                if (index > 0) {
                    var previousPayload = nodeText.Payloads[index - 1];
                    var payload = nodeText.Payloads[index];

                    if (previousPayload.Type is PayloadType.NewLine && payload.Type is PayloadType.NewLine) {
                        lineBreakIndex = index - 1;
                        break;
                    }
                }
            }

            // Copy all payloads up until the double-newline payload
            if (lineBreakIndex is not -1) {
                var newString = new SeStringBuilder();

                for(var index = 0; index < lineBreakIndex; index++) {
                    newString.Add(nodeText.Payloads[index]);
                }

                existingTextNode->SetText(newString.Encode());
            }
        }
        
        foreach (var index in Enumerable.Range(0, 16)) {
            perfectTails.GameState[index] = PlayerState.Instance()->IsWeeklyBingoStickerPlaced(index);
        }

        if (probabilityTextNode is not null) {
            probabilityTextNode.Text = perfectTails.SolveAndGetProbabilitySeString();
        }
    }
    
    private void DetachNodes(AddonWeeklyBingo* addon) {
        var existingTextNode = addon->GetTextNodeById(34);
        if (existingTextNode is not null) {
            existingTextNode->SetHeight((ushort)(existingTextNode->GetHeight() * 3.0f / 2.0f));
        }

        System.NativeController.DetachNode(probabilityTextNode, () => {
            probabilityTextNode?.Dispose();
            probabilityTextNode = null;
        });
    }
}
