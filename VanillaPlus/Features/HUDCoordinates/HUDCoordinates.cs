using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit;
using KamiToolKit.Nodes;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.HUDCoordinates;

public unsafe class HUDCoordinates : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "HUD 坐标显示",
        Description = "在 HUD 布局中显示节点坐标，帮助更精确地摆放界面元素。\n\n" +
                      "会显示元素中心点的坐标。",
        Type = ModificationType.UserInterface,
        Authors = [ "MidoriKami" ],
        ChangeLog = [
            new ChangeLogInfo(1, "初始版本"),
        ],
    };

    public override string ImageName => "HUDCoordinates.png";

    private AddonController? hudLayoutScreenController;

    private List<TextNode>? textNodes;

    public override void OnEnable() {
        textNodes = [];
        
        hudLayoutScreenController = new AddonController("_HudLayoutScreen");

        hudLayoutScreenController.OnAttach += addon => {
            foreach (var node in addon->UldManager.Nodes) {
                if (node.Value is null) continue;
                if (node.Value->GetNodeType() is not NodeType.Component) continue;

                var newTextNode = new TextNode {
                    NodeId = 100,
                    Size = new Vector2(90.0f, 22.0f),
                    Position = new Vector2(node.Value->Width / 2.0f, node.Value->Height / 2.0f) - new Vector2(90.0f, 22.0f) / 2.0f,
                    IsVisible = true,
                    String = new Vector2(node.Value->X, node.Value->Y).ToString(),
                };
                
                textNodes.Add(newTextNode);
                System.NativeController.AttachNode(newTextNode, (AtkComponentNode*)node.Value);
            }
        };

        hudLayoutScreenController.OnUpdate += addon => {
            foreach (var node in addon->UldManager.Nodes) {
                if (node.Value is null) continue;
                if (node.Value->GetNodeType() is not NodeType.Component) continue;
                var componentNode = (AtkComponentNode*)node.Value;
                
                var textNode = componentNode->Component->GetTextNodeById(100);
                if (textNode is null) continue;
                
                var textNodeSizeOffset = new Vector2(node.Value->Width, node.Value->Height) / 2.0f - new Vector2(90.0f, 22.0f) / 2.0f;
                var textNodeCenter = new Vector2(node.Value->X, node.Value->Y) + new Vector2(node.Value->Width, node.Value->Height) / 2.0f;
                
                textNode->SetPositionFloat(textNodeSizeOffset.X, textNodeSizeOffset.Y);
                textNode->SetText(textNodeCenter.ToString());
            }
        };

        hudLayoutScreenController.OnDetach += _ => {
            foreach (var node in textNodes) {
                System.NativeController.DetachNode(node);
                node.Dispose();
            }
            
            textNodes.Clear();
        };
        
        hudLayoutScreenController.Enable();
    }

    public override void OnDisable() {
        hudLayoutScreenController?.Dispose();
        hudLayoutScreenController = null;
        
        textNodes?.Clear();
        textNodes = null;
    }
}


