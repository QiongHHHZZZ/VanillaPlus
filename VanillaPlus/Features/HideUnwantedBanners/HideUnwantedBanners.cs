using System;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.HideUnwantedBanners;

public unsafe class HideUnwantedBanners : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "隐藏不需要的横幅",
        Description = "阻止特定大型横幅及其音效弹出，减少画面干扰。",
        Authors = ["MidoriKami"],
        Type = ModificationType.GameBehavior,
        ChangeLog = [
            new ChangeLogInfo(1, "初始实现"),
        ],
        CompatibilityModule = new SimpleTweaksCompatibilityModule("UiAdjustments@HideUnwantedBanner"),
    };

    private Hook<AddonImage3.Delegates.SetImage>? setImageTextureHook;

    private HideUnwantedBannersConfig? config;
    private HideUnwantedBannersConfigWindow? configWindow;

    public override void OnEnable() {
        config = HideUnwantedBannersConfig.Load();
        configWindow = new HideUnwantedBannersConfigWindow(config);
        configWindow.AddToWindowSystem();
        OpenConfigAction = configWindow.Toggle;

        setImageTextureHook = Services.Hooker.HookFromAddress<AddonImage3.Delegates.SetImage>(AddonImage3.Addresses.SetImage.Value, OnSetImageTexture);
        setImageTextureHook?.Enable();
    }

    public override void OnDisable() {
        configWindow?.RemoveFromWindowSystem();
        configWindow = null;
        
        setImageTextureHook?.Dispose();
        setImageTextureHook = null;

        config = null;
    }

    private void OnSetImageTexture(AddonImage3* addon, int bannerId, IconSubFolder language, int soundEffectId) {
        var skipOriginal = false;

        try {
            if (config is null) {
                setImageTextureHook!.Original(addon, skipOriginal ? 0 : bannerId, language, skipOriginal ? 0 : soundEffectId);
                return;
            }
            
            skipOriginal = config.HiddenBanners.Contains(bannerId);
        } catch (Exception e) {
            Services.PluginLog.Error(e, "设置横幅图像时出现异常");
        }

        setImageTextureHook!.Original(addon, skipOriginal ? 0 : bannerId, language, skipOriginal ? 0 : soundEffectId);
    }
}


