using System;
using System.Numerics;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Component.GUI;
using VanillaPlus.Classes;
using VanillaPlus.NativeElements.Config;

namespace VanillaPlus.Features.FasterScroll;

public unsafe class FasterScroll : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "滚动条加速",
        Description = "提高所有滚动条的滚动速度。",
        Authors = ["MidoriKami"],
        Type = ModificationType.GameBehavior,
        ChangeLog = [
            new ChangeLogInfo(1, "初始实现"),
        ],
    };

    private Hook<AtkComponentScrollBar.Delegates.ReceiveEvent>? scrollBarReceiveEventHook;

    private FasterScrollConfig? config;
    private ConfigAddon? configWindow;

    public override void OnEnable() {
        config = FasterScrollConfig.Load();
        configWindow = new ConfigAddon {
            NativeController = System.NativeController,
            Size = new Vector2(400.0f, 125.0f),
            InternalName = "FasterScrollConfig",
            Title = "滚动条加速设置",
            Config = config,
        };

        configWindow.AddCategory("设置")
            .AddFloatSlider("速度倍率", 0.5f, 4.0f, 2, 0.05f, nameof(config.SpeedMultiplier));
        
        OpenConfigAction = configWindow.Toggle;

        scrollBarReceiveEventHook = Services.Hooker.HookFromAddress<AtkComponentScrollBar.Delegates.ReceiveEvent>(AtkComponentScrollBar.StaticVirtualTablePointer->ReceiveEvent, AtkComponentScrollBarReceiveEvent);
        scrollBarReceiveEventHook?.Enable();
    }

    public override void OnDisable() {
        scrollBarReceiveEventHook?.Dispose();
        scrollBarReceiveEventHook = null;
        
        configWindow?.Dispose();
        configWindow = null;

        config = null;
    }

    private void AtkComponentScrollBarReceiveEvent(AtkComponentScrollBar* thisPtr, AtkEventType type, int param, AtkEvent* eventPointer, AtkEventData* dataPointer) {
        try {
            if (config is null) {
                scrollBarReceiveEventHook!.Original(thisPtr, type, param, eventPointer, dataPointer);
                return;
            }
        
            thisPtr->MouseWheelSpeed = (short) ( config.SpeedMultiplier * thisPtr->MouseWheelSpeed );
            scrollBarReceiveEventHook!.Original(thisPtr, type, param, eventPointer, dataPointer);
            thisPtr->MouseWheelSpeed = (short) ( thisPtr->MouseWheelSpeed / config.SpeedMultiplier );
        }
        catch (Exception e) {
            Services.PluginLog.Error(e, "处理 AtkComponentScrollBarReceiveEvent 时出错");
        }
    }
}

