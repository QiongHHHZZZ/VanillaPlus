using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.System.Scheduler;
using FFXIVClientStructs.FFXIV.Client.System.Scheduler.Base;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using VanillaPlus.BasicAddons;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.ForcedCutsceneSounds;

public unsafe class ForcedCutsceneSounds : GameModification {
    public override ModificationInfo ModificationInfo => new() {
        DisplayName = "Forced Cutscene Sounds",
        Description = "Automatically unmutes selected sound channels in cutscenes.",
        Authors = ["Haselnussbomber"],
        Type = ModificationType.GameBehavior,
        ChangeLog = [
            new ChangeLogInfo(1, "Initial Implementation"),
            new ChangeLogInfo(2, "Added option to disable in MSQ Roulette"),
        ],
        CompatibilityModule = new HaselTweaksCompatibilityModule("ForcedCutsceneMusic"),
    };

    private static readonly string[] ConfigOptions = [
        "IsSndMaster",
        "IsSndBgm",
        "IsSndSe",
        "IsSndVoice",
        "IsSndEnv",
        "IsSndSystem",
        "IsSndPerform",
    ];
    
    private Dictionary<string, bool>? wasMuted;

    private delegate CutSceneController* CutSceneControllerDtorDelegate(CutSceneController* self, byte freeFlags);
    
    private Hook<ScheduleManagement.Delegates.CreateCutSceneController>? createCutSceneControllerHook;
    private Hook<CutSceneControllerDtorDelegate>? cutSceneControllerDtorHook;

    private ForcedCutsceneSoundsConfig? config;
    private BoolConfigAddon? configWindow;

    public override void OnEnable() {
        wasMuted = [];
        
        config = ForcedCutsceneSoundsConfig.Load();
        configWindow = new BoolConfigAddon {
            NativeController = System.NativeController,
            Size = new Vector2(325.0f, 385.0f),
            InternalName = "ForcedCutsceneConfig",
            Title = "Forced Cutscene Sounds Config",
            OnClose = () => config.Save(),
        };
        
        configWindow.AddConfigEntry("General", "Restore mute state after cutscene", config, nameof(config.Restore));

        configWindow.AddConfigEntry("Toggles", "Unmute Master Volume", config, nameof(config.HandleMaster));
        configWindow.AddConfigEntry("Toggles", "Unmute BGM", config, nameof(config.HandleBgm));
        configWindow.AddConfigEntry("Toggles", "Unmute Sound Effects", config, nameof(config.HandleSe));
        configWindow.AddConfigEntry("Toggles", "Unmute Voice", config, nameof(config.HandleVoice));
        configWindow.AddConfigEntry("Toggles", "Unmute Ambient Sounds", config, nameof(config.HandleEnv));
        configWindow.AddConfigEntry("Toggles", "Unmute System Sounds", config, nameof(config.HandleSystem));
        configWindow.AddConfigEntry("Toggles", "Unmute Performance", config, nameof(config.HandlePerform));

        configWindow.AddConfigEntry("Special", "Disable in MSQ Roulette", config, nameof(config.DisableInMsqRoulette));
        
        OpenConfigAction = configWindow.Toggle;
        
        createCutSceneControllerHook = Services.GameInteropProvider.HookFromAddress<ScheduleManagement.Delegates.CreateCutSceneController>(
            ScheduleManagement.MemberFunctionPointers.CreateCutSceneController,
            CreateCutSceneControllerDetour);
        createCutSceneControllerHook.Enable();

        cutSceneControllerDtorHook = Services.GameInteropProvider.HookFromVTable<CutSceneControllerDtorDelegate>(
            CutSceneController.StaticVirtualTablePointer, 0,
            CutSceneControllerDtorDetour);
        cutSceneControllerDtorHook.Enable();
    }

    public override void OnDisable() {
        createCutSceneControllerHook?.Dispose();
        createCutSceneControllerHook = null;
        
        cutSceneControllerDtorHook?.Dispose();
        cutSceneControllerDtorHook = null;
        
        configWindow?.Dispose();
        configWindow = null;
        
        config = null;

        wasMuted = null;
    }
    
    private CutSceneController* CreateCutSceneControllerDetour(ScheduleManagement* thisPtr, byte* path, uint id, byte a4) {
        var result = createCutSceneControllerHook!.Original(thisPtr, path, id, a4);
        
        try {
            if (config is null) return result;
            if (config.DisableInMsqRoulette && AgentContentsFinder.Instance()->SelectedDuty is { ContentType: ContentsId.ContentsType.Roulette, Id: 3 }) return result;
            if (wasMuted is null || id is 0) return result;

            foreach (var optionName in ConfigOptions) {
                var isMuted = Services.GameConfig.System.TryGet(optionName, out bool value) && value;

                wasMuted[optionName] = isMuted;

                if (ShouldHandle(optionName) && isMuted) {
                    Services.GameConfig.System.Set(optionName, false);
                }
            }
            
        }
        catch (Exception e) {
            Services.PluginLog.Error(e, "Error in CreateCutSceneControllerDetour");
        }
        
        return result;
    }
    
    private CutSceneController* CutSceneControllerDtorDetour(CutSceneController* self, byte freeFlags) {
        try {
            if (config is null) {
                return cutSceneControllerDtorHook!.Original(self, freeFlags);
            }
            
            var cutsceneId = self->CutsceneId;
            
            if (config.Restore && cutsceneId is not 0) { // ignore title screen cutscene
                foreach (var optionName in ConfigOptions) {
                    if (ShouldHandle(optionName) && (wasMuted?.TryGetValue(optionName, out var value) ?? false) && value) {
                        Services.GameConfig.System.Set(optionName, value);
                    }
                }
            }
        }
        catch (Exception e) {
            Services.PluginLog.Error(e, "Error in CutSceneControllerDtorDetour");
        }
        
        return cutSceneControllerDtorHook!.Original(self, freeFlags);
    }

    private bool ShouldHandle(string optionName) {
        if (config is null) return false;
        
        return optionName switch {
            "IsSndMaster" => config.HandleMaster,
            "IsSndBgm" => config.HandleBgm,
            "IsSndSe" => config.HandleSe,
            "IsSndVoice" => config.HandleVoice,
            "IsSndEnv" => config.HandleEnv,
            "IsSndSystem" => config.HandleSystem,
            "IsSndPerform" => config.HandlePerform,
            _ => false,
        };
    }
}
