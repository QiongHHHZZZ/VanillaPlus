using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Game.ClientState.Keys;
using KamiToolKit.Nodes;
using KamiToolKit.System;
using VanillaPlus.Classes;
using VanillaPlus.NativeElements.Addons;

namespace VanillaPlus.Features.FateListWindow;

public class FateListWindow : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "Fate列表",
        Description = "显示当前区域正在进行的所有 FATE，便于快速查看。",
        Type = ModificationType.NewWindow,
        Authors = ["MidoriKami"],
        ChangeLog = [
            new ChangeLogInfo(1, "初始实现"),
            new ChangeLogInfo(2, "按剩余时间排序"),
            new ChangeLogInfo(3, "新增 `/fatelist` 指令以打开窗口"),
        ],
    };

    private NodeListAddon? addonFateList;
    
    public override string ImageName => "FateListWindow.png";

    public override void OnEnable() {
        addonFateList = new NodeListAddon {
            NativeController = System.NativeController,
            Size = new Vector2(300.0f, 400.0f),
            InternalName = "FateList",
            Title = "Fate列表",
            OpenCommand = "/fatelist",
            UpdateListFunction = UpdateList,
        };

        addonFateList.Initialize([VirtualKey.CONTROL, VirtualKey.F] );

        OpenConfigAction = addonFateList.OpenAddonConfig;
    }

    public override void OnDisable() {
        addonFateList?.Dispose();
        addonFateList = null;
    }

    private static bool UpdateList(VerticalListNode listNode, bool isOpening) {
        var validFates = Services.FateTable.Where(fate => fate is { State: FateState.Running or FateState.Preparation }).ToList();
        var listChanged = listNode.SyncWithListData(validFates, node => node.Fate, data => new FateEntryNode {
            Size = new Vector2(listNode.Width, 53.0f),
            IsVisible = true,
            Fate = data,
        });

        if (listChanged) {
            listNode.ReorderNodes(Comparison);
        }

        foreach (var fateNode in listNode.GetNodes<FateEntryNode>()) {
            fateNode.Update();
        }

        return listChanged;
    }

    private static int Comparison(NodeBase x, NodeBase y) {
        if (x is not FateEntryNode left || y is not FateEntryNode right) return 0;
        
        return left.Fate.TimeRemaining.CompareTo(right.Fate.TimeRemaining);
    }
}


