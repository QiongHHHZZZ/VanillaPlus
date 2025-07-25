using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Component.GUI;
using VanillaPlus.Core;
using VanillaPlus.Extensions;

namespace VanillaPlus.FasterScroll;

public class FasterScroll : GameModification {
    public override ModificationInfo ModificationInfo => new() {
        DisplayName = "Faster Scrollbars",
        Description = "Increases the speed of all scrollbars",
        Authors = ["MidoriKami"],
        Type = ModificationType.GameBehavior,
        ChangeLog = [
            new ChangeLogInfo(1, "Initial Implementation"),
        ],
    };

    [Signature("40 55 53 56 41 54 41 55 41 56 41 57 48 8B EC 48 81 EC ?? ?? ?? ??", DetourName = nameof(AtkComponentScrollBarReceiveEvent))]
    private readonly Hook<AtkComponentScrollBar.Delegates.ReceiveEvent>? scrollbarInitializeHook = null;

    private FasterScrollConfig config = null!;
    private FasterScrollConfigWindow configWindow = null!;

    public override bool HasConfigWindow => true;

    public override void OpenConfigWindow()
        => configWindow.Toggle();

    public override void OnEnable() {
        config = FasterScrollConfig.Load();
        configWindow = new FasterScrollConfigWindow(config);
        configWindow.AddToWindowSystem();
        
        Services.GameInteropProvider.InitializeFromAttributes(this);
        scrollbarInitializeHook?.Enable();
    }

    public override void OnDisable() {
        scrollbarInitializeHook?.Dispose();
        configWindow.RemoveFromWindowSystem();
    }

    private unsafe void AtkComponentScrollBarReceiveEvent(AtkComponentScrollBar* thisPtr, AtkEventType type, int param, AtkEvent* eventPointer, AtkEventData* dataPointer) {
        thisPtr->MouseWheelSpeed = (short) ( config.SpeedMultiplier * thisPtr->MouseWheelSpeed );
        scrollbarInitializeHook!.Original(thisPtr, type, param, eventPointer, dataPointer);
        thisPtr->MouseWheelSpeed = (short) ( thisPtr->MouseWheelSpeed / config.SpeedMultiplier );
    }
}
