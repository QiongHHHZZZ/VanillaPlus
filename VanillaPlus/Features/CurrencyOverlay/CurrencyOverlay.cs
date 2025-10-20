using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit;
using KamiToolKit.Addons;
using KamiToolKit.Classes;
using KamiToolKit.Nodes;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.CurrencyOverlay;

public unsafe class CurrencyOverlay : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "货币覆盖层",
        Description = "允许你在界面覆盖层中添加额外的货币显示。\n\n同时可以设置最小值与最大值，超出范围时触发警示。",
        Type = ModificationType.NewOverlay,
        Authors = [ "MidoriKami" ],
        ChangeLog = [
            new ChangeLogInfo(1, "初始版本"),
            new ChangeLogInfo(2, "重写配置系统，并支持调整缩放比例"),
        ],
    };

    public override string ImageName => "CurrencyOverlay.png";

    private OverlayAddonController? overlayAddonController;

    private SimpleOverlayNode? overlayRootNode;
    private CurrencyOverlayConfig? config;

    private ListConfigAddon<CurrencySetting, CurrencyOverlayConfigNode>? configAddon;

    private List<CurrencyNode>? currencyNodes;

    private List<CurrencySetting>? addedCurrencySettings;
    private List<CurrencySetting>? removedCurrencySettings;

    public override void OnEnable() {
        currencyNodes = [];
        addedCurrencySettings = [];
        removedCurrencySettings = [];
        
        config = CurrencyOverlayConfig.Load();

        configAddon = new ListConfigAddon<CurrencySetting, CurrencyOverlayConfigNode> {
            NativeController = System.NativeController,
            Size = new Vector2(700.0f, 500.0f),
            InternalName = "CurrencyOverlayConfig",
            Title = "货币覆盖层设置",
            SortOptions = [ "按名称排序" ],

            Options = config.Currencies,

            OnConfigChanged = changedSetting => {
                var nodes = currencyNodes.Where(node => node.Currency == changedSetting);

                foreach (var node in nodes) {
                    node.Currency = changedSetting;
                    node.OnMoveComplete = changedSetting.IsNodeMoveable ? () => config.Save() : null;
                }
                config.Save();
            },

            OnItemAdded = item => {
                addedCurrencySettings.Add(item);
                config.Save();
            },
            
            OnItemRemoved = item => {
                removedCurrencySettings.Add(item);
                config.Save();
            },
        };

        OpenConfigAction = configAddon.Toggle;

        overlayAddonController = new OverlayAddonController();
        overlayAddonController.OnAttach += addon => {
            overlayRootNode = new SimpleOverlayNode {
                Size = addon->AtkUnitBase.Size(),
                IsVisible = true,
            };
            System.NativeController.AttachNode(overlayRootNode, addon->RootNode, NodePosition.AsFirstChild);

            var screenSize = new Vector2(AtkStage.Instance()->ScreenSize.Width, AtkStage.Instance()->ScreenSize.Height);

            foreach (var setting in config.Currencies) {
                var newCurrencyNode = BuildCurrencyNode(setting, screenSize);

                currencyNodes.Add(newCurrencyNode);
                System.NativeController.AttachNode(newCurrencyNode, overlayRootNode);
            }
        };

        overlayAddonController.OnUpdate += _ => {
            if (overlayRootNode is null) return;
            
            var screenSize = new Vector2(AtkStage.Instance()->ScreenSize.Width, AtkStage.Instance()->ScreenSize.Height);
            
            foreach (var toAdd in addedCurrencySettings) {
                var newCurrencyNode = BuildCurrencyNode(toAdd, screenSize);

                currencyNodes.Add(newCurrencyNode);
                System.NativeController.AttachNode(newCurrencyNode, overlayRootNode);
            }
            addedCurrencySettings.Clear();

            foreach (var toRemove in removedCurrencySettings) {
                var node = currencyNodes.FirstOrDefault(node => node.Currency == toRemove);
                if (node is not null) {
                    currencyNodes.Remove(node);
                    System.NativeController.DetachNode(node);
                    node.Dispose();
                }
            }
            removedCurrencySettings.Clear();

            foreach (var currencyNode in currencyNodes) {
                currencyNode.UpdateValues();
            }
        };

        overlayAddonController.OnDetach += _ => {
            System.NativeController.DisposeNode(ref overlayRootNode);

            foreach (var currencyNode in currencyNodes) {
                System.NativeController.DetachNode(currencyNode);
                currencyNode.Dispose();
            }
            
            currencyNodes.Clear();
        };
        
        overlayAddonController.Enable();
    }

    public override void OnDisable() {
        overlayAddonController?.Dispose();
        overlayAddonController = null;
        
        configAddon?.Dispose();
        configAddon = null;

        config = null;
        
        currencyNodes?.Clear();
        currencyNodes = null;
    }

    private CurrencyNode BuildCurrencyNode(CurrencySetting setting, Vector2 screenSize) {
        var newCurrencyNode = new CurrencyNode {
            Size = new Vector2(164.0f, 36.0f),
            IsVisible = true,
            Currency = setting,
        };

        newCurrencyNode.OnEditComplete = () => {
            setting.Position = newCurrencyNode.Position;
            config!.Save();
        };

        if (setting.Position == Vector2.Zero) {
            newCurrencyNode.Position = new Vector2(screenSize.X, screenSize.Y) / 2.0f - new Vector2(164.0f, 36.0f) / 2.0f;
        }
        else {
            newCurrencyNode.Position = setting.Position;
        }

        return newCurrencyNode;
    }
}


