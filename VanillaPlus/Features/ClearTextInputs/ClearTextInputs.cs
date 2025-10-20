using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.Interop;
using KamiToolKit.Classes;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.ClearTextInputs;

public unsafe class ClearTextInputs : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "清除文本输入",
        Description = "允许你在文本输入框上点击右键以快速清除内容。",
        Type = ModificationType.GameBehavior,
        Authors = [ "MidoriKami" ],
        ChangeLog = [
            new ChangeLogInfo(1, "初始版本"),
        ],
    };

    private Hook<AtkComponentTextInput.Delegates.Setup>? onTextComponentSetupHook;

    private CustomEventListener? customEventListener;

    private List<Pointer<AtkCollisionNode>>? registeredEventNodes;

    public override bool IsExperimental => true;

    public override void OnEnable() {
        Services.PluginLog.Warning("清除文本输入功能已暂时禁用以避免客户端崩溃。");
        Services.ChatGui.PrintError("[VanillaPlus] 清除文本输入功能已暂时禁用，该功能可能导致客户端崩溃。");
    }

    public override void OnDisable() {
        onTextComponentSetupHook?.Dispose();
        onTextComponentSetupHook = null;

        if (customEventListener is not null) {
            foreach (var node in registeredEventNodes ?? []) {
                if (node.Value is null) continue;
                if (node.Value->VirtualTable == AtkEventTarget.StaticVirtualTablePointer) continue; // Node has been disposed already

                node.Value->AtkEventManager.UnregisterEvent(AtkEventType.MouseClick, 0x80000, customEventListener.EventListener, false);
            }
        }
        
        registeredEventNodes?.Clear();
        registeredEventNodes = null;
        
        customEventListener?.Dispose();
        customEventListener = null;
    }

    private void OnTextComponentSetup(AtkComponentTextInput* textInput) {
        onTextComponentSetupHook!.Original(textInput);

        try {
            if (customEventListener is null) return;
            
            var collisionNode = (AtkCollisionNode*) Marshal.ReadIntPtr((nint)textInput, 0xD8);
            if (collisionNode is null) return;

            var atkEventManager = collisionNode->AtkEventManager;

            atkEventManager.RegisterEvent(
                AtkEventType.MouseClick, 
                0x80000, 
                null, 
                (AtkEventTarget*)collisionNode, 
                customEventListener.EventListener, 
                false
            );
            
            registeredEventNodes?.Add(collisionNode);
        }
        catch (Exception e) {
            Services.PluginLog.Error(e, "无法为文本输入添加自定义事件");
        }
    }
    
    private static void HandleEvents(AtkEventListener* thisPtr, AtkEventType eventType, int eventParam, AtkEvent* atkEvent, AtkEventData* atkEventData) {
        if (eventParam is not 0x80000) return;
        if (eventType is not AtkEventType.MouseClick) return;
        if (atkEventData->MouseData.ButtonId is not 1) return;
        
        Services.PluginLog.Debug("检测到右键点击");

        var collisionNode = (AtkCollisionNode*)atkEvent->Target;
        if (collisionNode is null) return;

        var parent = (AtkComponentNode*)collisionNode->ParentNode;
        if (parent is null) return;

        var component = (AtkComponentTextInput*)parent->Component;
        if (component is null) return;

        var addon = component->ContainingAddon;
        if (addon is null) return;

        // Little hacky, have to unfocus else it will remember its last input string when you press another key
        AtkStage.Instance()->AtkInputManager->SetFocus(null, addon, 0);
        component->SetText(string.Empty);
        AtkStage.Instance()->AtkInputManager->SetFocus((AtkResNode*)collisionNode, addon, 0);
    }
}

