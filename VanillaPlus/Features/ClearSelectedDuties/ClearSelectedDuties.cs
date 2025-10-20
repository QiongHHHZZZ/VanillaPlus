using System.Numerics;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using VanillaPlus.Classes;
using VanillaPlus.NativeElements.Config;

namespace VanillaPlus.Features.ClearSelectedDuties;

public class ClearSelectedDuties : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "清除已选任务",
        Description = "打开职责查找器时，自动取消所有已选中的任务。",
        Authors = [ "MidoriKami" ],
        Type = ModificationType.GameBehavior,
        ChangeLog = [
            new ChangeLogInfo(1, "初始实现"),
        ],
    };

    private ClearSelectedDutiesConfig? config;
    private ConfigAddon? configWindow;

    public override void OnEnable() {
        config = ClearSelectedDutiesConfig.Load();
        configWindow = new ConfigAddon {
            NativeController = System.NativeController,
            Size = new Vector2(300.0f, 135.0f),
            InternalName = "ClearSelectedConfig",
            Title = "清除已选任务设置",
            Config = config,
        };

        configWindow.AddCategory("设置")
            .AddCheckbox("解除人数限制时禁用", nameof(config.DisableWhenUnrestricted));
        
        OpenConfigAction = configWindow.Toggle;
        
        Services.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "ContentsFinder", OnContentsFinderSetup);
    }

    public override void OnDisable() {
        Services.AddonLifecycle.UnregisterListener(OnContentsFinderSetup);
       
        configWindow?.Dispose();
        configWindow = null;
        
        config = null;
    }

    private unsafe void OnContentsFinderSetup(AddonEvent type, AddonArgs args) {
        if (config is null) return;
        
        var contentsFinder = ContentsFinder.Instance();
        var agent = AgentContentsFinder.Instance();
        var addon = args.GetAddon<AddonContentsFinder>();

        if (contentsFinder->QueueInfo.QueueState is not ContentsFinderQueueInfo.QueueStates.None)
            return;

        if (!IsRouletteTab(addon) && config.DisableWhenUnrestricted && contentsFinder->IsUnrestrictedParty) return;

        agent->AgentInterface.SendCommand(0, [ 12, 1 ]);
    }

    private static unsafe bool IsRouletteTab(AddonContentsFinder* addon)
        => addon->SelectedRadioButton is 0;
}

