using System.Numerics;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Command;
using VanillaPlus.Classes;

namespace VanillaPlus.Features.FateListWindow;

public class FateListWindow : GameModification {
    public override ModificationInfo ModificationInfo => new() {
        DisplayName = "Fate List Window",
        Description = "Displays a list of all fates that are currently active in the current zone",
        Type = ModificationType.NewWindow,
        Authors = ["MidoriKami"],
        ChangeLog = [
            new ChangeLogInfo(1, "Initial Implementation"),
            new ChangeLogInfo(2, "Now sorts by time remaining"),
            new ChangeLogInfo(3, "Added `/fatelist` command to open window"),
        ],
    };

    private AddonFateList? addonFateList;
    private AddonConfig? config;
    private KeybindListener? keybindListener;
    private AddonConfigWindow? addonConfigWindow;
    
    public override string ImageName => "FateListWindow.png";

    public override void OnEnable() {
        config = AddonConfig.Load("FateList.addon.json", [VirtualKey.CONTROL, VirtualKey.F]);

        addonFateList = new AddonFateList {
            NativeController = System.NativeController,
            Size = new Vector2(300.0f, 400.0f),
            InternalName = "FateList",
            Title = "Fate List",
        };
        
        keybindListener = new KeybindListener {
            KeybindCallback = () => {
                if (config.WindowSize != Vector2.Zero) {
                    addonFateList.Size = config.WindowSize;
                }

                addonFateList.Toggle();
            },
            KeyCombo = config.OpenKeyCombo,
        };

        addonConfigWindow = new AddonConfigWindow("Fate List", config, keybind => {
            keybindListener.KeyCombo = keybind;
        });

        OpenConfigAction = addonConfigWindow.Toggle;

        Services.CommandManager.AddHandler("/fatelist", new CommandInfo(OnFateListCommand) {
            DisplayOrder = 3,
            HelpMessage = "Opens the Fate List Window",
        });
    }

    private void OnFateListCommand(string command, string arguments)
        => addonFateList?.Open();

    public override void OnDisable() {
        addonFateList?.Dispose();
        addonFateList = null;
        
        addonConfigWindow?.Dispose();
        addonConfigWindow = null;
        
        keybindListener?.Dispose();
        keybindListener = null;

        config = null;
        
        Services.CommandManager.RemoveHandler("/fatelist");
    }
}
