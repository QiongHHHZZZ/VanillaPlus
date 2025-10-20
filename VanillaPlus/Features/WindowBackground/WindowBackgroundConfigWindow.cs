using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.UI;
using VanillaPlus.Utilities;

namespace VanillaPlus.Features.WindowBackground;

public unsafe class WindowBackgroundConfigWindow : Window {
    private readonly WindowBackgroundConfig config;
    private readonly Action styleChanged;
    private readonly Action<string> addonRemoved;
    private readonly Action<string> addonAdded;
    private string searchString = string.Empty;

    public WindowBackgroundConfigWindow(WindowBackgroundConfig config, Action styleChanged, Action<string> addonRemoved, Action<string> addonAdded) : base("窗口背景设置") {
        this.config = config;
        this.styleChanged = styleChanged;
        this.addonRemoved = addonRemoved;
        this.addonAdded = addonAdded;

        SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(350.0f, 500.0f),
        };
    }

    public override void Draw() {
        ImGui.Text("颜色");
        
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.ColorEdit4("##Color", ref config.Color, ImGuiColorEditFlags.AlphaPreviewHalf | ImGuiColorEditFlags.AlphaBar)) styleChanged();
        if (ImGui.IsItemDeactivatedAfterEdit()) config.Save();

        ImGui.Spacing();
        ImGui.Text("边距");
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.DragFloat2("##Padding", ref config.Padding, 0.1f, -50.0f, 50.0f)) styleChanged();
        if (ImGui.IsItemDeactivatedAfterEdit()) config.Save();
        
        ImGui.Spacing();
        ImGui.Separator();

        DrawEnabledAddons();
        DrawAllAddons();
    }

    private void DrawEnabledAddons() {
        ImGui.Text("已启用的窗口");
        using var child = ImRaii.Child("Enabled_Addons", new Vector2(ImGui.GetContentRegionAvail().X, 100.0f * ImGuiHelpers.GlobalScale));
        if (!child) return;
        ImGui.Spacing();
        
        ImGuiClip.ClippedDraw(config.Addons, option => DrawAddonOption(KnownColor.LimeGreen.Vector(), option), 25.0f * ImGuiHelpers.GlobalScale);
    }
    
    private void DrawAllAddons() {
        ImGui.Text("所有窗口");

        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        ImGui.InputTextWithHint("##Search", "搜索...", ref searchString);
        ImGui.Spacing();
        
        using var child = ImRaii.Child("All_Addons_Select", ImGui.GetContentRegionAvail());
        if (!child) return;
        ImGui.Spacing();
        
        var addonList = GetAddonNames().Where(addonInfo => addonInfo.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
        ImGuiClip.ClippedDraw(addonList, DrawAddonOption, 25.0f * ImGuiHelpers.GlobalScale);
    }

    private record AddonInfo(bool IsVisible, string Name);

    private static List<AddonInfo> GetAddonNames() {
        List<AddonInfo> addonNames = [];

        foreach (var addon in RaptureAtkUnitManager.Instance()->AllLoadedUnitsList.Entries) {
            if (addon.Value is not null) {
                addonNames.Add(new AddonInfo(addon.Value->IsVisible, addon.Value->NameString));
            }
        }

        return addonNames;
    }

    private void DrawAddonOption(AddonInfo addonInfo) {
        var color = KnownColor.White.Vector();
        if (!addonInfo.IsVisible) {
            color = KnownColor.Gray.Vector();
        }

        if (config.Addons.Contains(addonInfo.Name)) {
            color = KnownColor.LimeGreen.Vector();
        }
        
        DrawAddonOption(color, addonInfo.Name);
    }

    private void DrawAddonOption(Vector4 color, string name) {
        using var id = ImRaii.PushId(name);
        var isTracked = config.Addons.Contains(name);
        
        if (isTracked) {
            if (Drawing.IconButton(FontAwesomeIcon.Trash)) {
                config.Addons.Remove(name);
                addonRemoved(name);
                config.Save();
            }
        }
        else {
            if (Drawing.IconButton(FontAwesomeIcon.Plus)) {
                config.Addons.Add(name);
                addonAdded(name);
                config.Save();
            }
        }
        
        ImGui.SameLine();
        ImGui.TextColored(color, name);
    }
}
