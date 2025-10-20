using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.System.Scheduler;
using FFXIVClientStructs.FFXIV.Client.System.Scheduler.Base;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using VanillaPlus.Classes;
using VanillaPlus.NativeElements.Config;

namespace VanillaPlus.Features.ForcedCutsceneSounds;

public unsafe class ForcedCutsceneSounds : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "过场音效强制开启",
        Description = "在过场动画中自动开启指定的声音频道。",
        Authors = ["Haselnussbomber"],
        Type = ModificationType.GameBehavior,
        ChangeLog = [
            new ChangeLogInfo(1, "初始实现"),
            new ChangeLogInfo(2, "新增主线随机副本中禁用的选项"),
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
    private ConfigAddon? configWindow;

    public override void OnEnable() {
        wasMuted = [];
        
        config = ForcedCutsceneSoundsConfig.Load();
        configWindow = new ConfigAddon {
            NativeController = System.NativeController,
            Size = new Vector2(330.0f, 385.0f),
            InternalName = "ForcedCutsceneConfig",
            Title = "过场音效设置",
            Config = config,
        };

        configWindow.AddCategory("通用")
            .AddCheckbox("过场结束后恢复原静音状态", nameof(config.Restore));

        configWindow.AddCategory("音量渠道")
            .AddCheckbox("取消静音：总音量", nameof(config.HandleMaster))
            .AddCheckbox("取消静音：背景音乐", nameof(config.HandleBgm))
            .AddCheckbox("取消静音：音效", nameof(config.HandleSe))
            .AddCheckbox("取消静音：语音", nameof(config.HandleVoice))
            .AddCheckbox("取消静音：环境音", nameof(config.HandleEnv))
            .AddCheckbox("取消静音：系统音", nameof(config.HandleSystem))
            .AddCheckbox("取消静音：演奏音", nameof(config.HandlePerform));

        configWindow.AddCategory("特殊选项")
            .AddCheckbox("主线随机任务中禁用该功能", nameof(config.DisableInMsqRoulette));

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
            Services.PluginLog.Error(e, "创建过场控制器时出错");
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
            Services.PluginLog.Error(e, "过场控制器析构处理时出错");
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


