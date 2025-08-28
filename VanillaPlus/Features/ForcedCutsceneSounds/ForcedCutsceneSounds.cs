using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.System.Scheduler;
using FFXIVClientStructs.FFXIV.Client.System.Scheduler.Base;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using VanillaPlus.Basic_Addons;
using VanillaPlus.Classes;
using VanillaPlus.Extensions;

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
    private AddonBoolConfig? configWindow;

    public override void OnEnable() {
        wasMuted = [];
        
        config = ForcedCutsceneSoundsConfig.Load();
        configWindow = new AddonBoolConfig {
            NativeController = System.NativeController,
            Size = new Vector2(325.0f, 375.0f),
            InternalName = "ForcedCutsceneConfig",
            Title = "Forced Cutscene Sounds Config",
            OnClose = () => config.Save(),
        };
        
        configWindow.AddConfigEntry(new BoolConfigEntry("General", "Restore mute state after cutscene", config.Restore, b => {
            config.Restore = b;
            config.Save();
        }));
        
        configWindow.AddConfigEntry(new BoolConfigEntry("Toggles", "Unmute Master Volume", config.HandleMaster, b => {
            config.HandleMaster = b;
            config.Save();
        }));
        configWindow.AddConfigEntry(new BoolConfigEntry("Toggles", "Unmute BGM", config.HandleBgm, b => {
            config.HandleBgm = b;
            config.Save();
        }));
        configWindow.AddConfigEntry(new BoolConfigEntry("Toggles", "Unmute Sound Effects", config.HandleSe, b => {
            config.HandleSe = b;
            config.Save();
        }));
        configWindow.AddConfigEntry(new BoolConfigEntry("Toggles", "Unmute Voice", config.HandleVoice, b => {
            config.HandleVoice = b;
            config.Save();
        }));
        configWindow.AddConfigEntry(new BoolConfigEntry("Toggles", "Unmute Ambient Sounds", config.HandleEnv, b => {
            config.HandleEnv = b;
            config.Save();
        }));
        configWindow.AddConfigEntry(new BoolConfigEntry("Toggles", "Unmute System Sounds", config.HandleSystem, b => {
            config.HandleSystem = b;
            config.Save();
        }));
        configWindow.AddConfigEntry(new BoolConfigEntry("Toggles", "Unmute Performance", config.HandlePerform, b => {
            config.HandlePerform = b;
            config.Save();
        }));
        
        configWindow.AddConfigEntry(new BoolConfigEntry("Special", "Disable in MSQ Roulette", config.DisableInMsqRoulette, b => {
            config.Restore = b;
            config.Save();
        }));
        
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
