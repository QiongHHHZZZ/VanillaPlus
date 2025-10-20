using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Keys;
using KamiToolKit.Nodes;
using KamiToolKit.System;
using Lumina.Excel.Sheets;
using VanillaPlus.Classes;
using VanillaPlus.NativeElements.Addons;
using Map = FFXIVClientStructs.FFXIV.Client.Game.UI.Map;

namespace VanillaPlus.Features.QuestListWindow;

public unsafe class QuestListWindow : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "任务列表窗口",
        Description = "列出当前区域可接的所有任务，方便快速定位目标。",
        Type = ModificationType.NewWindow,
        Authors = [ "MidoriKami" ],
        ChangeLog = [
            new ChangeLogInfo(1, "初始版本"),
        ],
    };

    private SearchableNodeListAddon? addonQuestList;
    private string filterString = string.Empty;
    private string searchString = string.Empty;
    private bool filterReversed;
    private bool updateRequested;

    public override string ImageName => "QuestList.png";

    public override void OnEnable() {
        addonQuestList = new SearchableNodeListAddon {
            NativeController = System.NativeController,
            Size = new Vector2(300.0f, 400.0f),
            InternalName = "QuestList",
            Title = "任务列表",
            UpdateListFunction = UpdateList,
            DropDownOptions = [ "按类型", "按名称", "按等级", "按距离", "按发布者" ],
            OnFilterUpdated = OnFilterUpdated,
            OnSearchUpdated = OnSearchUpdated,
            OpenCommand = "/questlist",
        };
        
        addonQuestList.Initialize([VirtualKey.MENU, VirtualKey.CONTROL, VirtualKey.J]);
        OnFilterUpdated("按类型", false);

        OpenConfigAction = addonQuestList.OpenAddonConfig;
    }

    public override void OnDisable() {
        addonQuestList?.Dispose();
        addonQuestList = null;
    }

    private void OnFilterUpdated(string newFilterString, bool reversed) {
        updateRequested = true;
        filterString = newFilterString;
        filterReversed = reversed;
        addonQuestList?.DoListUpdate();
    }

    private void OnSearchUpdated(string newSearchString) {
        updateRequested = true;
        searchString = newSearchString;
        addonQuestList?.DoListUpdate();
    }

    private bool UpdateList(VerticalListNode listNode, bool isOpening) {
        var filteredInventoryItems = GetQuests()
            .Where(item => item.IsRegexMatch(searchString))
            .ToList();

        var listUpdated = listNode.SyncWithListData(filteredInventoryItems, node => node.QuestInfo, data => new QuestEntryNode {
            Size = new Vector2(listNode.Width, 48.0f),
            QuestInfo = data,
            IsVisible = true,
        });

        if (listUpdated || updateRequested || filterString is "Distance") {
            listNode.ReorderNodes(Comparison);
        }

        foreach (var questNode in listNode.GetNodes<QuestEntryNode>()) {
            questNode.Update();
        }
        
        updateRequested = false;
        return listUpdated;
    }

    private int Comparison(NodeBase x, NodeBase y) {
        if (x is not QuestEntryNode left || y is not QuestEntryNode right) return 0;

        var leftQuest = left.QuestInfo;
        var rightQuest = right.QuestInfo;

        var result = filterString switch {
            "按名称" => string.CompareOrdinal(leftQuest.Name.ToString(), rightQuest.Name.ToString()),
            "按类型" => rightQuest.IconId.CompareTo(leftQuest.IconId),
            "按等级" => rightQuest.Level.CompareTo(leftQuest.Level),
            "按距离" => leftQuest.Distance.CompareTo(rightQuest.Distance),
            "按发布者" => string.CompareOrdinal(leftQuest.IssuerName.ToString(), rightQuest.IssuerName.ToString()),
            _ => string.CompareOrdinal(leftQuest.Name.ToString(), rightQuest.Name.ToString()),
        };

        var reverseModifier = filterReversed ? -1 : 1;
        
        return ( result is 0 ? string.CompareOrdinal(leftQuest.Name.ToString(), rightQuest.Name.ToString()) : result ) * reverseModifier;
    }
    
    private static List<QuestInfo> GetQuests() {

        List<QuestInfo> quests = [];
        foreach (var questMarker in Map.Instance()->UnacceptedQuestMarkers) {
            if (questMarker.ObjectiveId is 0) continue;
            
            var questInfo = Services.DataManager.GetExcelSheet<Quest>().GetRow(questMarker.ObjectiveId + ushort.MaxValue + 1);

            var newQuestInfo = new QuestInfo{
                ObjectiveId = questMarker.ObjectiveId,
                IconId = questMarker.MarkerData.First->IconId, 
                Name = questMarker.Label.AsSpan(),
                Level = questInfo.ClassJobLevel.First(),
                Position = questMarker.MarkerData.First->Position,
                IssuerName = questInfo.IssuerStart.GetValueOrDefault<ENpcResident>()?.Singular ?? string.Empty,
            };

            quests.Add(newQuestInfo);
        }

        return quests;
    }
}


