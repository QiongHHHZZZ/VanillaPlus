using System.Linq;
using System.Numerics;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit;
using KamiToolKit.Nodes;
using KamiToolKit.System;
using VanillaPlus.Classes;
using VanillaPlus.NativeElements.Addons;

namespace VanillaPlus.Features.PartyFinderPresets;

public unsafe class PartyFinderPresets : GameModification {
    public override ModificationInfo ModificationInfo => new() {
        DisplayName = "Party Finder Presets",
        Description = "Allows you to save an use presets for the Party Finder Recruitment window",
        Type = ModificationType.GameBehavior,
        Authors = [ "MidoriKami" ],
        ChangeLog = [
            new ChangeLogInfo(1, "InitialChangelog"),
        ],
    };

    private AddonController<AtkUnitBase>? recruitmentCriteriaController;
    private AddonController<AtkUnitBase>? lookingForGroupController;

    private TextButtonNode? savePresetButton;
    private TextDropDownNode? presetDropDown;
    
    private AddonSavePreset? savePresetWindow;
    private RenameAddon? renameWindow;

    private NodeListAddon? presetEditorAddon;

    public override bool IsExperimental => true;

    public override string ImageName => "PartyFinderPresets.png";

    public override void OnEnable() {
        savePresetWindow = new AddonSavePreset {
            NativeController = System.NativeController,
            Size = new Vector2(300.0f, 215.0f),
            InternalName = "LookingForGroupPreset",
            Title = "Party Finder Preset",
            Subtitle = "Vanilla Plus",
        };

        presetEditorAddon = new NodeListAddon {
            NativeController = System.NativeController,
            Size = new Vector2(300.0f, 400.0f),
            InternalName = "PresetEditorAddon",
            Title = "Preset Editor",
            UpdateListFunction = UpdateList,
        };

        OpenConfigAction = presetEditorAddon.Toggle;
        
        Services.AddonLifecycle.RegisterListener(AddonEvent.PreReceiveEvent, "LookingForGroup", OnLookingForGroupEvent);
        Services.AddonLifecycle.RegisterListener(AddonEvent.PreFinalize, "LookingForGroupCondition", OnLookingForGroupConditionFinalize);

        recruitmentCriteriaController = new AddonController<AtkUnitBase>("LookingForGroupCondition");
        recruitmentCriteriaController.OnAttach += addon => {
            savePresetButton = new TextButtonNode {
                Position = new Vector2(406.0f, 605.0f),
                Size = new Vector2(160.0f, 28.0f),
                IsVisible = true,
                String = "Save Preset",
                Tooltip = "[VanillaPlus]: Save current settings to a preset",
                OnClick = SavePreset,
            };
            System.NativeController.AttachNode(savePresetButton, addon->RootNode);
        };

        recruitmentCriteriaController.OnDetach += _ => {
            System.NativeController.DisposeNode(ref savePresetButton);
        };

        recruitmentCriteriaController.Enable();

        lookingForGroupController = new AddonController<AtkUnitBase>("LookingForGroup");

        lookingForGroupController.OnAttach += BuildDropDownNode;

        lookingForGroupController.OnDetach += _ => {
            System.NativeController.DisposeNode(ref presetDropDown);
        };
        
        lookingForGroupController.Enable();
    }

    private bool UpdateList(VerticalListNode listNode, bool isOpening) {
        var filteredPresetNames = PresetManager.GetPresetNames()
            .Where(name => name is not (PresetManager.DefaultString or PresetManager.DontUseString));

        var listChanged = listNode.SyncWithListData(filteredPresetNames, node => node.PresetName, data => new PresetEditNode {
            Size = new Vector2(listNode.Width, 32.0f),
            PresetName = data,
            OnDeletePreset = _ => PresetManager.DeletePreset(data),
            OnEditPreset = presetName => {

                renameWindow ??= new RenameAddon {
                    NativeController = System.NativeController,
                    InternalName = "PresetRenameWindow",
                    Title = "Preset Rename Window",
                    Size = new Vector2(300.0f, 200.0f),
                };
                
                renameWindow.DefaultString = presetName;
                renameWindow.ResultCallback = newName => {
                    PresetManager.LoadPreset(presetName);
                    PresetManager.SavePreset(newName);
                    PresetManager.DeletePreset(presetName);
                };
                
                renameWindow.Toggle();
            },
            IsVisible = true,
        });

        if (listChanged) {
            listNode.ReorderNodes(Comparison);
        }

        return false;
    }

    private static int Comparison(NodeBase x, NodeBase y) {
        if (x is not PresetEditNode left || y is not PresetEditNode right) return 0;
        
        return string.CompareOrdinal(left.PresetName, right.PresetName);
    }

    public override void OnDisable() {
        Services.AddonLifecycle.UnregisterListener(OnLookingForGroupEvent);
        
        recruitmentCriteriaController?.Dispose();
        recruitmentCriteriaController = null;
        
        lookingForGroupController?.Dispose();
        lookingForGroupController = null;
        
        savePresetWindow?.Dispose();
        savePresetWindow = null;
        
        presetEditorAddon?.Dispose();
        presetEditorAddon = null;
        
        renameWindow?.Dispose();
        renameWindow = null;
        
        Services.AddonLifecycle.UnLogAddon("LookingForGroup");
    }

    private void OnLookingForGroupConditionFinalize(AddonEvent type, AddonArgs args) {
        if (presetDropDown is null) return;

        var parentAddon = RaptureAtkUnitManager.Instance()->GetAddonByNode(presetDropDown);
        if (parentAddon is null) return;
        
        System.NativeController.DisposeNode(ref presetDropDown);
        BuildDropDownNode(parentAddon);
    }

    private void OnLookingForGroupEvent(AddonEvent type, AddonArgs args) {
        if (args is not AddonReceiveEventArgs eventArgs) return;
        if ((AtkEventType)eventArgs.AtkEventType is not AtkEventType.ButtonClick) return;
        if (eventArgs.EventParam is not 2) return;
        if (presetDropDown?.SelectedOption is not { } selectedOption) return;
        if (selectedOption is PresetManager.DefaultString) return;
        if (selectedOption is PresetManager.DontUseString) return;
        
        PresetManager.LoadPreset(selectedOption);
    }
    
    private void SavePreset()
        => savePresetWindow?.Open(5);

    private void BuildDropDownNode(AtkUnitBase* addon) {
        var presets = PresetManager.GetPresetNames();
        var anyPresets = presets.All(presetName => presetName != PresetManager.DefaultString);
        
        presetDropDown = new TextDropDownNode {
            Position = new Vector2(185.0f, 636.0f),
            Size = new Vector2(200.0f, 25.0f),
            MaxListOptions = 10,
            Options = presets,
            IsEnabled = anyPresets,
            IsVisible = true,
        };

        if (anyPresets) {
            presetDropDown.Tooltip = "[VanillaPlus]: Select a preset";
        }
        else {
            presetDropDown.Tooltip = "[VanillaPlus]: No presets saved";
        }
        System.NativeController.AttachNode(presetDropDown, addon->RootNode);
    }
}
