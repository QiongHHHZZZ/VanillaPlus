using System.Linq;
using System.Numerics;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit;
using KamiToolKit.Nodes;
using KamiToolKit.System;
using VanillaPlus.Classes;
using VanillaPlus.NativeElements.Addons;

namespace VanillaPlus.Features.PartyFinderPresets;

public unsafe class PartyFinderPresets : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "组队搜索预设",
        Description = "为招募信息窗口提供可保存、可复用的预设配置",
        Type = ModificationType.GameBehavior,
        Authors = [ "MidoriKami" ],
        ChangeLog = [
            new ChangeLogInfo(1, "初始版本"),
        ],
    };

    private AddonController<AtkUnitBase>? recruitmentCriteriaController;
    private AddonController<AtkUnitBase>? lookingForGroupController;

    private TextButtonNode? savePresetButton;
    private TextDropDownNode? presetDropDown;
    
    private PartyFinderSavePresetAddon? savePresetWindow;
    private RenameAddon? renameWindow;

    private NodeListAddon? presetEditorAddon;

    public override string ImageName => "PartyFinderPresets.png";

    public override void OnEnable() {
        savePresetWindow = new PartyFinderSavePresetAddon {
            NativeController = System.NativeController,
            Size = new Vector2(300.0f, 215.0f),
            InternalName = "LookingForGroupPreset",
            Title = "组队搜索预设",
            Subtitle = "Vanilla Plus",
            DepthLayer = 5,
        };

        presetEditorAddon = new NodeListAddon {
            NativeController = System.NativeController,
            Size = new Vector2(300.0f, 400.0f),
            InternalName = "PresetEditorAddon",
            Title = "预设管理器",
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
                String = "保存为预设",
                Tooltip = "[VanillaPlus]: 将当前招募设置保存为预设",
                OnClick = SavePreset,
            };
            System.NativeController.AttachNode(savePresetButton, addon->RootNode);
        };

        recruitmentCriteriaController.OnDetach += _ => {
            System.NativeController.DisposeNode(ref savePresetButton);
        };

        recruitmentCriteriaController.Enable();

        lookingForGroupController = new AddonController<AtkUnitBase>("LookingForGroup");

        lookingForGroupController.OnAttach += addon => {
            if (presetDropDown is not null) {
                System.NativeController.DisposeNode(ref presetDropDown);
            }

            presetDropDown = new TextDropDownNode {
                Position = new Vector2(185.0f, 636.0f),
                Size = new Vector2(200.0f, 25.0f),
                MaxListOptions = 10,
                Options = PresetManager.GetPresetNames(),
                IsVisible = true,
            };

            UpdateDropDownOptions();

            System.NativeController.AttachNode(presetDropDown, addon->RootNode);
        };
        
        lookingForGroupController.OnDetach += _ => {
            System.NativeController.DetachNode(presetDropDown);
        };
        
        lookingForGroupController.Enable();
    }

    public override void OnDisable() {
        Services.AddonLifecycle.UnregisterListener(OnLookingForGroupEvent, OnLookingForGroupConditionFinalize);

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

        System.NativeController.DisposeNode(ref presetDropDown);
        presetDropDown = null;
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
                    Title = "重命名预设",
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

    private void OnLookingForGroupConditionFinalize(AddonEvent type, AddonArgs args)
        => UpdateDropDownOptions();

    private void OnLookingForGroupEvent(AddonEvent type, AddonArgs args) {
        if (args is not AddonReceiveEventArgs eventArgs) return;
        if ((AtkEventType)eventArgs.AtkEventType is not AtkEventType.ButtonClick) return;
        if (eventArgs.EventParam is not 2) return;
        if (presetDropDown?.SelectedOption is not { } selectedOption) return;
        if (selectedOption is PresetManager.DefaultString) return;
        if (selectedOption is PresetManager.DontUseString) return;
        
        PresetManager.LoadPreset(selectedOption);
    }

    private void SavePreset() => savePresetWindow?.Open();

    private void UpdateDropDownOptions() {
        if (presetDropDown is not null) {
            var presets = PresetManager.GetPresetNames();
            var anyPresets = presets.All(presetName => presetName != PresetManager.DefaultString);
            
            presetDropDown.Options = presets;
            presetDropDown.IsEnabled = anyPresets;

            if (anyPresets) {
                presetDropDown.Tooltip = "[VanillaPlus]: 请选择要应用的预设";
            }
            else {
                presetDropDown.Tooltip = "[VanillaPlus]: 当前没有保存的预设";
            }
        }
    }
}


