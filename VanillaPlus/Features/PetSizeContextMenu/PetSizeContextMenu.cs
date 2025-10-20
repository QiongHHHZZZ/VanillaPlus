using System.Linq;
using Dalamud.Game.Config;
using Dalamud.Game.Gui.ContextMenu;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.PetSizeContextMenu;

public class PetSizeContextMenu : GameModification {
    protected override ModificationInfo CreateModificationInfo => new() {
        DisplayName = "宠物体型菜单",
        Description = "在右键点击召唤兽或拥有召唤兽的玩家时，增加调整宠物体型的选项",
        Authors = [ "MidoriKami" ],
        Type = ModificationType.GameBehavior,
        ChangeLog = [
            new ChangeLogInfo(1, "初始实现"),
        ],
    };

    private readonly UiConfigOption[] configEntries = [
        UiConfigOption.BahamutSize, UiConfigOption.PhoenixSize, UiConfigOption.GarudaSize,
        UiConfigOption.TitanSize, UiConfigOption.IfritSize, UiConfigOption.SolBahamutSize,
    ];

    public override string ImageName => "PetSizeContextMenu.png";

    public override void OnEnable()
        => Services.ContextMenu.OnMenuOpened += OnMenuOpened;

    public override void OnDisable()
        => Services.ContextMenu.OnMenuOpened -= OnMenuOpened;

    private void OnMenuOpened(IMenuOpenedArgs args) {
        if (args is not { MenuType: ContextMenuType.Default }) return;
        if (args.Target is not MenuTargetDefault targetInfo) return;
        if (!targetInfo.TargetObject.IsPetOrOwner()) return;

        var currentPetSize = GetPetSize();
        
        args.AddMenuItem(new MenuItem {
            IsSubmenu = true,
            UseDefaultPrefix = true,
            Name = "召唤兽体型",
            OnClicked = clickedArgs => {
                clickedArgs.OpenSubmenu([
                    new MenuItem {
                        IsEnabled = currentPetSize is not 0,
                        UseDefaultPrefix = true, 
                        Name = "小型", 
                        OnClicked = _ => SetPetSize(0),
                    },
                    new MenuItem {
                        IsEnabled = currentPetSize is not 1,
                        UseDefaultPrefix = true, 
                        Name = "中型", 
                        OnClicked = _ => SetPetSize(1),
                    },
                    new MenuItem {
                        IsEnabled = currentPetSize is not 2,
                        UseDefaultPrefix = true, 
                        Name = "大型", 
                        OnClicked = _ => SetPetSize(2),
                    },
                ]);
            },
        });
    }

    private void SetPetSize(uint size) {
        foreach(var configEntry in configEntries) {
            Services.GameConfig.Set(configEntry, size);
        }
    }

    private uint? GetPetSize()
        => configEntries
           .Select(configKey => Services.GameConfig.TryGet(configKey, out uint value) ? value : 0)
           .GroupBy(configValue => configValue)
           .MaxBy(group => group.Count())?
           .Key;
}


