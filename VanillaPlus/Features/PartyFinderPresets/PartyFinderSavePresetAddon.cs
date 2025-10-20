using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Addon;
using KamiToolKit.Nodes;
using KamiToolKit.Nodes.TabBar;

namespace VanillaPlus.Features.PartyFinderPresets;

public class PartyFinderSavePresetAddon : NativeAddon {

    private TabBarNode tabBarNode = null!;
    
    private TextInputNode textInputNode = null!;
    private TextDropDownNode textDropDownNode = null!;
    private TextNode noPresetsTextNode = null!;

    private SimpleComponentNode warningContainerNode = null!;
    private IconImageNode warningIconNode = null!;
    private TextNode warningTextNode = null!;
    
    private TextButtonNode confirmButtonNode = null!;
    private TextButtonNode cancelButtonNode = null!;

    private bool anyPresets;
    private int selectedTab;
    
    protected override unsafe void OnSetup(AtkUnitBase* addon) {
        WindowNode.SubtitleNode.X = 130.0f;

        tabBarNode = new TabBarNode {
            Position = ContentStartPosition,
            Size = new Vector2(ContentSize.X, 24.0f),
            IsVisible = true,
        };
        
        tabBarNode.AddTab("新建预设", OnShowCreateNew);
        tabBarNode.AddTab("覆盖预设", OnShowOverwrite);
        
        AttachNode(tabBarNode);

        var extrasPosition = new Vector2(ContentPadding.X * 2.0f, tabBarNode.Y + tabBarNode.Height + 8.0f * 3.0f);
        var extrasSize = new Vector2(Size.X - ContentPadding.X * 4.0f, 28.0f);

        textInputNode = new TextInputNode {
            Position = extrasPosition,
            Size = extrasSize,
            IsVisible = true,
            PlaceholderString = "请输入预设名称",
            OnInputReceived = OnTextInput,
        };
        AttachNode(textInputNode);

        warningContainerNode = new SimpleComponentNode {
            Position = extrasPosition + new Vector2(0.0f, extrasSize.Y),
            Size = extrasSize,
        };
        AttachNode(warningContainerNode);

        warningIconNode = new IconImageNode {
            Position = new Vector2(25.0f, 0.0f),
            Size = new Vector2(16.0f, 16.0f),
            IconId = 60073,
            FitTexture = true,
            IsVisible = true,
        };
        System.NativeController.AttachNode(warningIconNode, warningContainerNode);

        warningTextNode = new TextNode {
            Position = new Vector2(50.0f, 0.0f),
            Size = new Vector2(200.0f, 16.0f),
            IsVisible = true,
            String = "名称中包含非法字符",
            AlignmentType = AlignmentType.Left,
        };
        System.NativeController.AttachNode(warningTextNode, warningContainerNode);

        var presetNames = PresetManager.GetPresetNames().Where(preset => preset != PresetManager.DontUseString).ToList();
        anyPresets = presetNames.All(preset => preset != PresetManager.DefaultString);
        
        noPresetsTextNode = new TextNode {
            Position = extrasPosition,
            Size = extrasSize,
            AlignmentType = AlignmentType.Bottom,
            String = PresetManager.DefaultString,
            IsVisible = false,
        };
        AttachNode(noPresetsTextNode);
        
        textDropDownNode = new TextDropDownNode {
            Position = extrasPosition,
            Size = extrasSize,
            MaxListOptions = 10,
            Options = presetNames,
            IsVisible = false,
        };
        AttachNode(textDropDownNode);

        var buttonSize = new Vector2(125.0f, 28.0f);

        confirmButtonNode = new TextButtonNode {
            Position = new Vector2(ContentPadding.X, Size.Y - buttonSize.Y - 8.0f * 2.0f),
            Size = buttonSize,
            IsVisible = true,
            String = "确定",
            OnClick = OnConfirm,
        };
        AttachNode(confirmButtonNode);

        cancelButtonNode = new TextButtonNode {
            Position = new Vector2(Size.X - ContentPadding.X - buttonSize.X, Size.Y - buttonSize.Y -8.0f * 2.0f),
            Size = buttonSize,
            IsVisible = true,
            String = "取消",
            OnClick = OnCancel,
        };
        AttachNode(cancelButtonNode);
    }

    private void OnTextInput(SeString fileName) {
        var isEmptyFileName = fileName.ToString().IsNullOrEmpty();
        var isInvalidFileName = !PresetManager.IsValidFileName(fileName.ToString());
        
        warningContainerNode.IsVisible = !isEmptyFileName && isInvalidFileName;
    }

    private void OnCancel()
        => Close();

    private void OnConfirm() {
        var fileName = selectedTab switch {
            0 => textInputNode.String,
            1 => textDropDownNode.SelectedOption,
            _ => throw new Exception("无效的标签页"),
        };

        if (fileName is null) return;
        
        PresetManager.SavePreset(fileName);
        Close();
    }

    private void OnShowCreateNew() {
        textInputNode.IsVisible = true;
        textDropDownNode.IsVisible = false;
        noPresetsTextNode.IsVisible = false;
        selectedTab = 0;
    }
    
    private void OnShowOverwrite() {
        textInputNode.IsVisible = false;
        textDropDownNode.IsVisible = anyPresets;
        noPresetsTextNode.IsVisible = !anyPresets;
        selectedTab = 1;
    }
}
