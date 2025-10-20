﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace VanillaPlus.Extensions;

public class LoggedAgentEvents {
    public List<ulong> SuppressedEventKinds { get; set; } = [];
    public List<int> SuppressedValues { get; set; } = [];

    public bool IsDisallowedValue(ref AtkValue value) {
        if (value.Type is not ValueType.Int) return false;
        var intValue = value.Int;
        
        return SuppressedValues.Contains(intValue);
    }

    public bool ContainsDisallowedValue(Span<AtkValue> valueSpan)
        => valueSpan.Length > 0 && IsDisallowedValue(ref valueSpan[0]);
}

public class AgentInfo {
    public required Hook<AgentInterface.Delegates.ReceiveEvent>? ReceiveEventHook { get; set; }
    public LoggedAgentEvents Events { get; set; } = new();
}

public static unsafe class AgentInterfaceExtensions {
    public static Dictionary<nint, AgentInfo> HookedAgents = [];
    
    public static void SendCommand(this ref AgentInterface agent, uint eventKind, int[] commandValues) {
        using var returnValue = new AtkValue();
        var command= stackalloc AtkValue[commandValues.Length];

        for (var index = 0; index < commandValues.Length; index++) {
            command[index].SetInt(commandValues[index]);
        }
        
        agent.ReceiveEvent(&returnValue, command, (uint) commandValues.Length, eventKind);
    }

    public static void LogAgent(this ref AgentInterface agent, LoggedAgentEvents? events = null) {
        var newHook = Services.Hooker.HookFromAddress<AgentInterface.Delegates.ReceiveEvent>(agent.VirtualTable->ReceiveEvent, AgentReceiveEvent);
        if (newHook.Address != nint.Zero) {
            HookedAgents.TryAdd((nint)Unsafe.AsPointer(ref agent), new AgentInfo {
                ReceiveEventHook = newHook,
                Events = events ?? new LoggedAgentEvents(),
            });
            newHook.Enable();
        }
    }

    public static void UnLogAgent(this ref AgentInterface agent) {
        if (HookedAgents.TryGetValue((nint)Unsafe.AsPointer(ref agent), out var agentInfo)) {
            agentInfo.ReceiveEventHook?.Dispose();
            HookedAgents.Remove((nint)Unsafe.AsPointer(ref agent));
        }
    }

    private static AtkValue* AgentReceiveEvent(AgentInterface* thisPtr, AtkValue* returnValue, AtkValue* values, uint valueCount, ulong eventKind) {
        var agentInfo = HookedAgents[(nint)thisPtr];
        if (agentInfo.ReceiveEventHook is null) {
            Services.PluginLog.Fatal("严重异常：尝试调用未追踪的代理。");
            return null;
        }
        
        try {
            var valueSpan = new Span<AtkValue>(values, (int)valueCount);
            if (!agentInfo.Events.ContainsDisallowedValue(valueSpan) && !agentInfo.Events.SuppressedEventKinds.Contains(eventKind)) {
                Services.PluginLog.Debug($"[{(nint)thisPtr:X}]: {eventKind}");
                valueSpan.PrintValues(2);
            }
        }
        catch (Exception e) {
            Services.PluginLog.Error(e, "AgentReceiveEvent 日志方法中发生异常");
        }

        return agentInfo.ReceiveEventHook.Original(thisPtr, returnValue, values, valueCount, eventKind);
    }
}
