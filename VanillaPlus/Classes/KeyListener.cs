using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace VanillaPlus.Classes;

public unsafe class KeyListener : IDisposable {

    private readonly Dictionary<VirtualKey, bool> virtualKeyMap = [];
    private readonly Dictionary<SeVirtualKey, bool> seVirtualKeyMap = [];
    
    public KeyListener() {
        foreach (var value in Services.KeyState.GetValidVirtualKeys()) {
            virtualKeyMap.TryAdd(value, false);
        }

        foreach (var value in Enum.GetValues<SeVirtualKey>()) {
            seVirtualKeyMap.TryAdd(value, false);
        }
        
        Services.Framework.Update += OnFrameworkUpdate;
    }

    public void Dispose()
        => Services.Framework.Update -= OnFrameworkUpdate;

    private void OnFrameworkUpdate(IFramework framework) {
        foreach (var pair in virtualKeyMap) {
            var newState = Services.KeyState[(int)pair.Key];

            if (virtualKeyMap[pair.Key] != newState) {
                OnKeyPressed?.Invoke(pair.Key, newState);
            }
            
            virtualKeyMap[pair.Key] = newState;
        }

        foreach (var pair in seVirtualKeyMap) {
            var newState = UIInputData.Instance()->IsKeyPressed(pair.Key);

            if (seVirtualKeyMap[pair.Key] != newState) {
                OnSeKeyPressed?.Invoke(pair.Key, newState);
            }
            
            seVirtualKeyMap[pair.Key] = newState;
        }
    }
    
    public Action<VirtualKey, bool>? OnKeyPressed { get; set; }
    public Action<SeVirtualKey, bool>? OnSeKeyPressed { get; set; }
}
