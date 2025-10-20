using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.HideMpBars;

public unsafe class HideMpBars : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "隐藏 MP 条",
        Description = "在队伍列表中隐藏不使用 MP 的职业的 MP 条。",
        Type = ModificationType.UserInterface,
        Authors = [ "MidoriKami" ],
        ChangeLog = [
            new ChangeLogInfo(1, "初始版本"),
        ],
        Tags = [ "队伍列表" ],
    };

    private List<uint>? manaUsingClassJobs;

    public override string ImageName => "HideMpBars.png";

    public override void OnEnable() {
        manaUsingClassJobs = Services.DataManager.GetManaUsingClassJobs().ToList();
        
        Services.AddonLifecycle.RegisterListener(AddonEvent.PostDraw, "_PartyList", OnPartyListDraw);
        Services.AddonLifecycle.RegisterListener(AddonEvent.PostRequestedUpdate, "_PartyList", OnPartyListDraw);
    }

    public override void OnDisable() {
        Services.AddonLifecycle.UnregisterListener(OnPartyListDraw);
        
        manaUsingClassJobs = null;
    }
    
    private void OnPartyListDraw(AddonEvent type, AddonArgs args) {
        if (Services.ClientState.IsPvP) return;
        if (manaUsingClassJobs is null) return;
        if (Services.ClientState.LocalPlayer is not { ClassJob: { IsValid: true, Value: var classJob }, EntityId: var playerId } localPlayer) return;

        var addon = args.GetAddon<AddonPartyList>();

        if (GroupManager.Instance()->MainGroup.MemberCount is 0) {
            if (classJob.IsCrafter() || classJob.IsGatherer()) return;

            var mpGaugeNode = addon->PartyMembers[0].MPGaugeBar->OwnerNode;
            mpGaugeNode->ToggleVisibility(manaUsingClassJobs.Contains(localPlayer.ClassJob.RowId));
        }
        else {
            foreach (var hudMember in AgentHUD.Instance()->GetSizedHudMemberSpan()) {
                if (hudMember.EntityId is 0) continue;
                if (hudMember.EntityId == playerId && ( classJob.IsCrafter() || classJob.IsGatherer() )) continue;
                if (hudMember.Object is null) continue;

                var mpGaugeNode = addon->PartyMembers[hudMember.Index].MPGaugeBar->OwnerNode;
                mpGaugeNode->ToggleVisibility(manaUsingClassJobs.Contains(hudMember.Object->ClassJob));
            }
        }
    }
}


