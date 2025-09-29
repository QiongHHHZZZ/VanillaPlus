using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit;
using VanillaPlus.NativeElements.Nodes;

namespace VanillaPlus.Classes;

public unsafe class InventorySearchAddonController : IDisposable {

    private MultiAddonController? inventoryController;
    private Dictionary<string, TextInputWithHintNode>? inputTextNodes;
    private Dictionary<string, int>? selectedTabs;

    public InventorySearchAddonController(Dictionary<string, Vector2> entries) {
        inputTextNodes = [];
        selectedTabs = [];

        inventoryController = new MultiAddonController(entries.Keys.ToArray());

        inventoryController.OnAttach += addon => {
            var size = entries[addon->NameString];

            var headerSize = new Vector2(addon->WindowHeaderCollisionNode->Width, addon->WindowHeaderCollisionNode->Height);
            var newInputNode = new TextInputWithHintNode {
                Position = headerSize / 2.0f - size / 2.0f + new Vector2(25.0f, 10.0f),
                Size = size,
                OnInputReceived = searchString => PerformSearch(addon, searchString.ToString()),
                IsVisible = true,
            };

            System.NativeController.AttachNode(newInputNode, addon->WindowNode);
            inputTextNodes.TryAdd(addon->NameString, newInputNode);
        };

        inventoryController.OnUpdate += addon => {
            if (!addon->IsReady) return;

            var currentTab = InventorySearchController.GetTabForInventory(addon);
            
            selectedTabs.TryAdd(addon->NameString, currentTab);
            if (selectedTabs[addon->NameString] != currentTab && inputTextNodes.TryGetValue(addon->NameString, out var inputTextNode)) {
                PerformSearch(addon, inputTextNode.SearchString.ToString());
            }

            selectedTabs[addon->NameString] = currentTab;
        };

        inventoryController.OnDetach += addon => {
            if (inputTextNodes.TryGetValue(addon->NameString, out var node)) {
                System.NativeController.DetachNode(node);
                node.Dispose();
                inputTextNodes.Remove(addon->NameString);
            }
        };

        inventoryController.Enable();
    }
    
    public void Dispose() {
        inputTextNodes?.Clear();
        inputTextNodes = null;
        
        selectedTabs?.Clear();
        selectedTabs = null;
        
        inventoryController?.Dispose();
        inventoryController = null;
    }

    public Action<string>? PreSearch { get; set; }
    public Action<string>? PostSearch { get; set; }

    private void PerformSearch(AtkUnitBase* addon, string searchString) {
        PreSearch?.Invoke(searchString);
        InventorySearchController.FadeInventoryNodes(addon, searchString);
        PostSearch?.Invoke(searchString);
    }
}
