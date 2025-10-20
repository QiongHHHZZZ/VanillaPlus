using System;
using System.Collections.Generic;
using System.Linq;
using VanillaPlus.Classes;

namespace VanillaPlus.Localization;

public static class LocalizationProvider {
    private sealed class LocalizationEntry {
        public string? DisplayName { get; init; }
        public string? Description { get; init; }
        public string[]? Authors { get; init; }
        public Dictionary<int, string>? ChangeLog { get; init; }
        public List<string>? Tags { get; init; }
    }

    private static readonly Dictionary<string, LocalizationEntry> Translations = new() {
        {
            "VanillaPlus.Features.ClearTextInputs.ClearTextInputs",
            new LocalizationEntry {
                DisplayName = "清除文本输入",
                Description = "允许你在文本输入框上点击右键以快速清除内容。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始版本" },
                },
            }
        },
        {
            "VanillaPlus.Features.BetterQuestMapLink.BetterQuestMapLink",
            new LocalizationEntry {
                DisplayName = "改进任务地图链接",
                Description = "点击任务链接时，打开对应任务所在区域的实际地图，而非通用世界地图。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始版本" },
                },
            }
        },
        {
            "VanillaPlus.Features.FasterScroll.FasterScroll",
            new LocalizationEntry {
                DisplayName = "滚动条加速",
                Description = "提高所有滚动条的滚动速度。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始实现" },
                },
            }
        },
        {
            "VanillaPlus.Features.ArmourySearchBar.ArmourySearchBar",
            new LocalizationEntry {
                DisplayName = "武器库搜索栏",
                Description = "为军械库窗口新增一个搜索栏。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始版本" },
                },
            }
        },
        {
            "VanillaPlus.Features.BetterCursor.BetterCursor",
            new LocalizationEntry {
                DisplayName = "光标强化",
                Description = "在光标位置绘制醒目的环形效果，帮助你更快找到鼠标。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始实现" },
                    { 2, "将动画速度降低至 1Hz" },
                    { 3, "新增仅在副本或战斗中显示的选项" },
                },
            }
        },
        {
            "VanillaPlus.Features.BetterInterruptableCastBars.BetterInterruptableCastBars",
            new LocalizationEntry {
                DisplayName = "强化打断读条",
                Description = "让敌对可打断的读条条更加醒目，并在热键栏上标注可用于打断的技能。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始版本" },
                },
            }
        },
        {
            "VanillaPlus.Features.ClearFlag.ClearFlag",
            new LocalizationEntry {
                DisplayName = "清除旗标",
                Description = "允许你在小地图上点击右键来移除当前设置的旗标。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始版本" },
                },
            }
        },
        {
            "VanillaPlus.Features.ClearSelectedDuties.ClearSelectedDuties",
            new LocalizationEntry {
                DisplayName = "清除已选任务",
                Description = "打开职责查找器时，自动取消所有已勾选的任务。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始实现" },
                },
            }
        },
        {
            "VanillaPlus.Features.CurrencyOverlay.CurrencyOverlay",
            new LocalizationEntry {
                DisplayName = "货币覆盖层",
                Description = "在界面覆盖层中添加额外的货币显示，并可设定警示阈值。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始版本" },
                    { 2, "重写配置系统并加入缩放调节" },
                },
            }
        },
        {
            "VanillaPlus.Features.CastBarAetheryteNames.CastBarAetheryteNames",
            new LocalizationEntry {
                DisplayName = "传送读条显示目的地",
                Description = "将“传送”动作名称替换为目的地的以太之地名，方便快速确认。"
            }
        },
        {
            "VanillaPlus.Features.DebugCustomAddon.DebugCustomAddon",
            new LocalizationEntry {
                DisplayName = "调试自定义界面",
                Description = "用于测试与实验 VanillaPlus 的自定义界面功能模块。"
            }
        },
        {
            "VanillaPlus.Features.DebugGameModification.DebugGameModification",
            new LocalizationEntry {
                DisplayName = "调试功能模块",
                Description = "用于测试与实验 VanillaPlus 提供的各类功能修改。"
            }
        },
        {
            "VanillaPlus.Features.DraggableWindowDeadSpace.DraggableWindowDeadSpace",
            new LocalizationEntry {
                DisplayName = "拖拽窗口空白区域",
                Description = "允许在窗口的空白区域按住拖动，轻松移动界面位置。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始版本" }
                }
            }
        },
        {
            "VanillaPlus.Features.DutyTimer.DutyTimer",
            new LocalizationEntry {
                DisplayName = "副本计时器",
                Description = "在完成副本时，将本次副本耗时输出到聊天栏，便于记录。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始实现" }
                }
            }
        },
        {
            "VanillaPlus.Features.EnhancedLootWindow.EnhancedLootWindow",
            new LocalizationEntry {
                DisplayName = "强化拾取窗口",
                Description = "在拾取窗口内显示物品是否已解锁或仍可获取的指示图标，避免重复需求。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始实现" }
                }
            }
        },
        {
            "VanillaPlus.Features.FadeLootButton.FadeLootButton",
            new LocalizationEntry {
                DisplayName = "拾取按钮淡出",
                Description = "当所有可拾取物品都已选择时淡化拾取按钮，提醒无需再次点击。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始实现" }
                }
            }
        },
        {
            "VanillaPlus.Features.FadeUnavailableActions.FadeUnavailableActions",
            new LocalizationEntry {
                DisplayName = "不可用技能淡化",
                Description = "当技能因距离、资源或冷却无法施放时自动淡化热键，并可对降同步技能单独处理。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始实现" }
                }
            }
        },
        {
            "VanillaPlus.Features.FastMouseClick.FastMouseClick",
            new LocalizationEntry {
                DisplayName = "快速鼠标点击",
                Description = "修复因检测到双击而导致单击事件不触发的问题，确保菜单响应稳定。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始实现" }
                }
            }
        },
        {
            "VanillaPlus.Features.FateListWindow.FateListWindow",
            new LocalizationEntry {
                DisplayName = "Fate列表",
                Description = "在区域内显示当前激活的 F.A.T.E. 列表，便于快速查看并前往。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始实现" }
                }
            }
        },
        {
            "VanillaPlus.Features.FocusTargetLock.FocusTargetLock",
            new LocalizationEntry {
                DisplayName = "焦点目标恢复",
                Description = "当副本重新开始时自动恢复上一次设置的焦点目标。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始版本" }
                }
            }
        },
        {
            "VanillaPlus.Features.ForcedCutsceneSounds.ForcedCutsceneSounds",
            new LocalizationEntry {
                DisplayName = "过场音效强制开启",
                Description = "在过场动画中自动开启指定的声音频道，避免听不到剧情语音。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始实现" },
                    { 2, "新增可在主线随机任务中禁用的选项" }
                }
            }
        },
        {
            "VanillaPlus.Features.GearsetRedirect.GearsetRedirect",
            new LocalizationEntry {
                DisplayName = "装备套装重定向",
                Description = "根据所在区域自动切换至备用套装，实现进本前的快速换装。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始版本" }
                }
            }
        },
        {
            "VanillaPlus.Features.HideGuildhestObjectivePopup.HideGuildhestObjectivePopup",
            new LocalizationEntry {
                DisplayName = "隐藏行会令提示窗口",
                Description = "进入行会令时阻止弹出教学提示窗口，减少打断。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始实现" }
                }
            }
        },
        {
            "VanillaPlus.Features.HideMpBars.HideMpBars",
            new LocalizationEntry {
                DisplayName = "隐藏无用 MP 条",
                Description = "对于不使用 MP 的职业，在队伍列表中隐藏其 MP 条以腾出空间。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始实现" }
                }
            }
        },
        {
            "VanillaPlus.Features.HideUnwantedBanners.HideUnwantedBanners",
            new LocalizationEntry {
                DisplayName = "隐藏不需要的横幅",
                Description = "阻止特定大型横幅及音效出现，减少屏幕干扰。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始实现" }
                }
            }
        },
        {
            "VanillaPlus.Features.HUDCoordinates.HUDCoordinates",
            new LocalizationEntry {
                DisplayName = "HUD 坐标显示",
                Description = "在 HUD 布局界面中显示各元素的坐标，方便精准对齐位置。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始实现" }
                }
            }
        },
        {
            "VanillaPlus.Features.HUDPresets.HUDPreset",
            new LocalizationEntry {
                DisplayName = "HUD 预设管理",
                Description = "保存并应用无限数量的 HUD 布局方案，随时切换。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始版本" }
                }
            }
        },
        {
            "VanillaPlus.Features.InstancedWaymarks.InstancedWaymarks",
            new LocalizationEntry {
                DisplayName = "副本独立场景标记",
                Description = "为每个副本提供独立的标记保存槽，并可自定义命名。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始版本" }
                }
            }
        },
        {
            "VanillaPlus.Features.InventorySearchBar.InventorySearchBar",
            new LocalizationEntry {
                DisplayName = "背包搜索栏",
                Description = "为背包窗口添加搜索栏，快速定位所需物品。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始版本" }
                }
            }
        },
        {
            "VanillaPlus.Features.ListInventory.ListInventory",
            new LocalizationEntry {
                DisplayName = "背包列表窗口",
                Description = "新增列表形式的背包查看窗口，并提供多种过滤选项。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始版本" }
                }
            }
        },
        {
            "VanillaPlus.Features.LocationDisplay.LocationDisplay",
            new LocalizationEntry {
                DisplayName = "位置栏信息扩展",
                Description = "在服务器信息栏显示当前所在区域及附加详细位置。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始实现" }
                }
            }
        },
        {
            "VanillaPlus.Features.MacroLineNumbers.MacroLineNumbers",
            new LocalizationEntry {
                DisplayName = "宏命令行号",
                Description = "在自定义宏界面左侧显示行号，便于编写与排错。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始版本" }
                }
            }
        },
        {
            "VanillaPlus.Features.MacroTooltips.MacroTooltips",
            new LocalizationEntry {
                DisplayName = "宏命令技能提示",
                Description = "当宏使用 /macroicon action 时，在悬停时显示对应技能提示信息。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始实现" }
                }
            }
        },
        {
            "VanillaPlus.Features.MiniCactpotHelper.MiniCactpotHelper",
            new LocalizationEntry {
                DisplayName = "仙人微彩助手",
                Description = "提示在仙人微彩游戏中下一步推荐翻开的格子，提高中奖概率。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始实现" }
                }
            }
        },
        {
            "VanillaPlus.Features.MissingJobStoneLockout.MissingJobStoneLockout",
            new LocalizationEntry {
                DisplayName = "缺失魂晶阻止排本",
                Description = "在缺少职业魂晶时阻止进入匹配，提醒先装备完整套装。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始版本" }
                }
            }
        },
        {
            "VanillaPlus.Features.OpenGlamourDresserToCurrentJob.OpenGlamourDresserToCurrentJob",
            new LocalizationEntry {
                DisplayName = "衣柜定位当前职业",
                Description = "打开幻化衣柜时自动切换到当前职业对应的分组。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始版本" }
                }
            }
        },
        {
            "VanillaPlus.Features.PartyFinderPresets.PartyFinderPresets",
            new LocalizationEntry {
                DisplayName = "组队搜索预设",
                Description = "为招募板窗口提供可保存、切换的招募条件预设。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始版本" }
                }
            }
        },
        {
            "VanillaPlus.Features.PetSizeContextMenu.PetSizeContextMenu",
            new LocalizationEntry {
                DisplayName = "宠物体型菜单",
                Description = "在右键菜单中添加选项，可快速调整召唤兽或宠物的体型大小。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始版本" }
                }
            }
        },
        {
            "VanillaPlus.Features.QuestListWindow.QuestListWindow",
            new LocalizationEntry {
                DisplayName = "任务列表窗口",
                Description = "显示当前地图内可接任务列表，帮助规划路线。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始版本" }
                }
            }
        },
        {
            "VanillaPlus.Features.RecentlyLootedWindow.RecentlyLootedWindow",
            new LocalizationEntry {
                DisplayName = "最近拾取窗口",
                Description = "提供滚动列表显示本次登录期间获得的物品记录。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始实现" }
                }
            }
        },
        {
            "VanillaPlus.Features.ResetInventoryTab.ResetInventoryTab",
            new LocalizationEntry {
                DisplayName = "重置背包分页",
                Description = "打开背包时自动返回到第一页，保持视图统一。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始实现" }
                }
            }
        },
        {
            "VanillaPlus.Features.ResourceBarPercentages.ResourceBarPercentages",
            new LocalizationEntry {
                DisplayName = "资源条百分比显示",
                Description = "将 HP、MP、GP 与 CP 条改为显示百分比，更直观掌握剩余资源。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始更新" },
                    { 2, "新增队友资源覆盖与信赖支援支持" }
                }
            }
        },
        {
            "VanillaPlus.Features.RetainerSearchBar.RetainerSearchBar",
            new LocalizationEntry {
                DisplayName = "雇员搜索栏",
                Description = "为雇员仓库窗口增加搜索栏，快速查找交给雇员的物品。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始版本" }
                }
            }
        },
        {
            "VanillaPlus.Features.SaddlebagSearchBar.SaddlebagSearchBar",
            new LocalizationEntry {
                DisplayName = "鞍囊搜索栏",
                Description = "为陆行鸟鞍囊界面添加搜索功能，便于整理。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始版本" }
                }
            }
        },
        {
            "VanillaPlus.Features.SelectNextLootItem.SelectNextLootItem",
            new LocalizationEntry {
                DisplayName = "自动选中下一个掉落",
                Description = "在对战利品进行需求/贪婪/放弃操作后自动跳转到下一项。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始实现" }
                }
            }
        },
        {
            "VanillaPlus.Features.SkipTeleportConfirm.SkipTeleportConfirm",
            new LocalizationEntry {
                DisplayName = "跳过传送确认",
                Description = "使用地图传送时跳过“是否花费××金币传送”的确认弹窗。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始实现" }
                }
            }
        },
        {
            "VanillaPlus.Features.StickyShopCategories.StickyShopCategories",
            new LocalizationEntry {
                DisplayName = "记住商店分类",
                Description = "记忆特定店家的分类与子分类选择，下次打开自动恢复。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始实现" }
                }
            }
        },
        {
            "VanillaPlus.Features.SuppressDialogAdvance.SuppressDialogAdvance",
            new LocalizationEntry {
                DisplayName = "防止对话误跳过",
                Description = "除非点击对话框本体，否则禁止过场文本自动推进，避免错过剧情。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始版本" }
                }
            }
        },
        {
            "VanillaPlus.Features.TargetCastBarCountdown.TargetCastBarCountdown",
            new LocalizationEntry {
                DisplayName = "目标读条倒计时",
                Description = "在目标、焦点及敌对名牌的读条上显示剩余时间，并提供多种样式配置。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始实现" },
                    { 2, "新增支持最多 10 个敌对名牌读条节点" },
                }
            }
        },
        {
            "VanillaPlus.Features.WindowBackground.WindowBackground",
            new LocalizationEntry {
                DisplayName = "原生窗口背景",
                Description = "为任意原生窗口添加可自定义的背景、边距与样式，让界面更统一。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始实现" }
                }
            }
        },
        {
            "VanillaPlus.Features.WondrousTailsProbabilities.WondrousTailsProbabilities",
            new LocalizationEntry {
                DisplayName = "天书奇谈助手",
                Description = "在天书奇谈手册中显示当前连线概率与平均重抽概率，辅助决策。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始实现" }
                }
            }
        },
        {
            "VanillaPlus.Features.SampleGameModification.SampleGameModification",
            new LocalizationEntry {
                DisplayName = "示例功能模块",
                Description = "模板示例，用于演示如何编写自定义功能；复制本目录后即可快速开始开发。",
                ChangeLog = new Dictionary<int, string> {
                    { 1, "初始示例" }
                }
            }
        },
    };

    public static ModificationInfo Localize(Type type, ModificationInfo original) {
        var key = type.FullName ?? type.Name;
        if (!Translations.TryGetValue(key, out var entry)) return original;

        var localizedChangeLog = entry.ChangeLog is { Count: > 0 }
            ? original.ChangeLog
                .Select(changeLog => entry.ChangeLog.TryGetValue(changeLog.Version, out var text)
                    ? new ChangeLogInfo(changeLog.Version, text)
                    : changeLog)
                .ToList()
            : new List<ChangeLogInfo>(original.ChangeLog);

        return new ModificationInfo {
            DisplayName = entry.DisplayName ?? original.DisplayName,
            Description = entry.Description ?? original.Description,
            Authors = entry.Authors ?? original.Authors,
            Type = original.Type,
            SubType = original.SubType,
            ChangeLog = localizedChangeLog,
            Tags = entry.Tags ?? new List<string>(original.Tags),
            CompatibilityModule = original.CompatibilityModule,
        };
    }
}
