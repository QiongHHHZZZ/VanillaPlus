using System;
using System.Linq;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.GearsetRedirect;

public unsafe class GearsetRedirect : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "套装重定向",
        Description = "根据所在区域选择要装备的替代套装。",
        Type = ModificationType.GameBehavior,
        Authors = [ "MidoriKami" ],
        ChangeLog = [
            new ChangeLogInfo(1, "初始版本"),
        ],
    };

    private Hook<RaptureGearsetModule.Delegates.EquipGearset>? gearsetChangedHook;
    private GearsetRedirectConfig? config;
    private GearsetRedirectConfigWindow? configWindow;
    
    public override void OnEnable() {
        config = GearsetRedirectConfig.Load();

        configWindow = new GearsetRedirectConfigWindow(config);
        configWindow.AddToWindowSystem();
        OpenConfigAction = () => {
            if (Services.ClientState.IsLoggedIn) {
                configWindow.Toggle();
            }
        };
        
        gearsetChangedHook = Services.Hooker.HookFromAddress<RaptureGearsetModule.Delegates.EquipGearset>(RaptureGearsetModule.Addresses.EquipGearset.Value, OnGearsetChanged);
        gearsetChangedHook?.Enable();
    }
    
    public override void OnDisable() {
        gearsetChangedHook?.Dispose();
        gearsetChangedHook = null;
        
        configWindow?.RemoveFromWindowSystem();
        configWindow = null;

        config = null;
    }

    private int OnGearsetChanged(RaptureGearsetModule* thisPtr, int gearsetId, byte glamourPlateId) {
        try {
            if (config is not null && config.Redirections.TryGetValue(gearsetId, out var redirection)) {
                var targetRedirection = redirection.FirstOrDefault(info => Services.ClientState.TerritoryType == info.TerritoryType);
                if (targetRedirection is not null) {
                    gearsetId = targetRedirection.AlternateGearsetId;
                }
            }
        }
        catch (Exception e) {
            Services.PluginLog.Error(e, "处理套装重定向时出现异常");
        }

        return gearsetChangedHook!.Original(thisPtr, gearsetId, glamourPlateId);
    }
}


