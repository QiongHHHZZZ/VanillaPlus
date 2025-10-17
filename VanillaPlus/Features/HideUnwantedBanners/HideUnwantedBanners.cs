using System;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.HideUnwantedBanners;

public unsafe class HideUnwantedBanners : GameModification {
    public override ModificationInfo ModificationInfo => new() {
        DisplayName = "Hide Unwanted Banners",
        Description = "Prevents large text banners from appearing and playing their sound effect.",
        Authors = ["MidoriKami"],
        Type = ModificationType.GameBehavior,
        ChangeLog = [
            new ChangeLogInfo(1, "Initial Implementation"),
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
            Services.PluginLog.Error(e, "Exception in OnSetImageTexture");
        }

        setImageTextureHook!.Original(addon, skipOriginal ? 0 : bannerId, language, skipOriginal ? 0 : soundEffectId);
    }
}
