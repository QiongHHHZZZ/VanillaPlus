using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.System.Input;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit;
using KamiToolKit.Classes.TimelineBuilding;
using KamiToolKit.Nodes;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.BetterCursor;

public unsafe class BetterCursor : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "光标强化",
        Description = "在光标周围绘制一个环形效果，让你更容易找到鼠标位置。",
        Authors = ["MidoriKami"],
        Type = ModificationType.UserInterface,
        ChangeLog = [
            new ChangeLogInfo(1, "初始实现"),
            new ChangeLogInfo(2, "将动画速度降低至 1Hz"),
            new ChangeLogInfo(3, "新增仅在副本或战斗中显示的选项"),
        ],
    };

    private ResNode? animationContainer;
    private IconImageNode? imageNode;

    private AddonController<AtkUnitBase>? screenTextController;

    private BetterCursorConfig? config;
    private BetterCursorConfigWindow? configWindow;

    public override string ImageName => "BetterCursor.png";

    public override void OnEnable() {
        config = BetterCursorConfig.Load();
        configWindow = new BetterCursorConfigWindow(config, UpdateNodeConfig);
        configWindow.AddToWindowSystem();
        OpenConfigAction = configWindow.Toggle;

        screenTextController = new AddonController<AtkUnitBase>("_ScreenText");
        screenTextController.OnAttach += AttachNodes;
        screenTextController.OnDetach += DetachNodes;
        screenTextController.OnUpdate += Update;
        screenTextController.Enable();
    }

    public override void OnDisable() {
        screenTextController?.Dispose();
        screenTextController = null;
        
        configWindow?.RemoveFromWindowSystem();
        configWindow = null;
        
        config = null;
    }

    private void UpdateNodeConfig() {
        if (config is null) return;
        
        if (animationContainer is not null) {
            animationContainer.Size = new Vector2(config.Size);
        }

        if (imageNode is not null) {
            imageNode.Size = new Vector2(config.Size);
            imageNode.Origin = new Vector2(config.Size / 2.0f);
            imageNode.Color = config.Color;
            imageNode.IconId = config.IconId;
        }

        animationContainer?.Timeline?.PlayAnimation(config.Animations ? 1 : 2);
    }

    private void Update(AtkUnitBase* addon) {
        if (config is null) return;

        if (animationContainer is not null && imageNode is not null) {
            ref var cursorData = ref UIInputData.Instance()->CursorInputs;
            animationContainer.Position = new Vector2(cursorData.PositionX, cursorData.PositionY) - imageNode.Size / 2.0f;

            var isLeftHeld = (cursorData.MouseButtonHeldFlags & MouseButtonFlags.LBUTTON) != 0;
            var isRightHeld = (cursorData.MouseButtonHeldFlags & MouseButtonFlags.RBUTTON) != 0;

            if (config is { OnlyShowInCombat: true } or { OnlyShowInDuties: true }) {
                var shouldShow = true;
                shouldShow &= !config.OnlyShowInCombat || Services.Condition.IsInCombat();
                shouldShow &= !config.OnlyShowInDuties || Services.Condition.IsBoundByDuty();
                shouldShow &= !config.HideOnCameraMove || (!isLeftHeld && !isRightHeld);

                animationContainer.IsVisible = shouldShow;
            }
            else {
                animationContainer.IsVisible = !isLeftHeld && !isRightHeld || !config.HideOnCameraMove;
            }
        }
    }

    private void AttachNodes(AtkUnitBase* addon) {
        animationContainer = new ResNode();
        System.NativeController.AttachNode(animationContainer, addon->RootNode);

        imageNode = new IconImageNode {
            IconId = 60498,
            FitTexture = true,
            IsVisible = true,
        };
        System.NativeController.AttachNode(imageNode, animationContainer);

        animationContainer.AddTimeline(new TimelineBuilder()
            .BeginFrameSet(1, 120)
            .AddLabel(1, 1, AtkTimelineJumpBehavior.Start, 0)
            .AddLabel(60, 0, AtkTimelineJumpBehavior.LoopForever, 1)
            .AddLabel(61, 2, AtkTimelineJumpBehavior.Start, 0)
            .AddLabel(120, 0, AtkTimelineJumpBehavior.LoopForever, 2)
            .EndFrameSet()
            .Build());

        imageNode.AddTimeline(new TimelineBuilder()
            .BeginFrameSet(1, 60)
            .AddFrame(1, scale: new Vector2(1.0f, 1.0f))
            .AddFrame(30, scale: new Vector2(0.75f, 0.75f))
            .AddFrame(60, scale: new Vector2(1.0f, 1.0f))
            .EndFrameSet()
            .BeginFrameSet(61, 120)
            .AddFrame(61, scale: new Vector2(1.0f, 1.0f))
            .EndFrameSet()
            .Build());

        UpdateNodeConfig();
    }

    private void DetachNodes(AtkUnitBase* addon) {
        System.NativeController.DisposeNode(ref animationContainer);
        System.NativeController.DisposeNode(ref imageNode);
    }
}

