using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.Interop;
using KamiToolKit.Classes;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.ClearTextInputs;

public unsafe class ClearTextInputs : GameModification {
    public override ModificationInfo ModificationInfo => new() {
        DisplayName = "Clear Text Inputs",
        Description = "Allows you to clear the text in a text input, by right clicking the text input.",
        Type = ModificationType.GameBehavior,
        Authors = [ "MidoriKami" ],
        ChangeLog = [
            new ChangeLogInfo(1, "InitialChangelog"),
        ],
    };
    
    [Signature("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 60 48 8B D9", DetourName = nameof(OnTextComponentSetup))]
    private Hook<AtkComponentTextInput.Delegates.Setup>? onTextComponentSetupHook;

    public delegate bool SetFocusDelegate(AtkInputManager* inputManager, AtkResNode* resNode, AtkUnitBase* addon, int focusParam);

    [Signature("E8 ?? ?? ?? ?? 49 8B 84 FF ?? ?? ?? ??")]
    public SetFocusDelegate? SetFocus = null;

    private CustomEventListener? customEventListener;

    private List<Pointer<AtkCollisionNode>>? registeredEventNodes;

    public override bool IsExperimental => true;

    public override void OnEnable() {
        registeredEventNodes = [];
        
        customEventListener = new CustomEventListener(HandleEvents);
        
        Services.Hooker.InitializeFromAttributes(this);
        onTextComponentSetupHook?.Enable();
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
            Services.PluginLog.Error(e, "Failed to add custom input events");
        }
    }
    
    private void HandleEvents(AtkEventListener* thisPtr, AtkEventType eventType, int eventParam, AtkEvent* atkEvent, AtkEventData* atkEventData) {
        if (eventParam is not 0x80000) return;
        if (eventType is not AtkEventType.MouseClick) return;
        if (atkEventData->MouseData.ButtonId is not 1) return;
        
        Services.PluginLog.Debug("Right clicked!");

        var collisionNode = (AtkCollisionNode*)atkEvent->Target;
        if (collisionNode is null) return;

        var parent = (AtkComponentNode*)collisionNode->ParentNode;
        if (parent is null) return;

        var component = (AtkComponentTextInput*)parent->Component;
        if (component is null) return;

        var addon = component->ContainingAddon;
        if (addon is null) return;

        // Little hacky, have to unfocus else it will remember its last input string when you press another key
        SetFocus?.Invoke(AtkStage.Instance()->AtkInputManager, null, addon, 0);
        component->SetText(string.Empty);
        SetFocus?.Invoke(AtkStage.Instance()->AtkInputManager, (AtkResNode*)collisionNode, addon, 0);
    }
}
