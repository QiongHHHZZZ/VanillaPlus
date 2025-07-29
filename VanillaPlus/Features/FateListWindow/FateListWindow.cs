using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using VanillaPlus.Core;
using VanillaPlus.Extensions;

namespace VanillaPlus.FateListWindow;

public class FateListWindow : GameModification {
    public override ModificationInfo ModificationInfo => new() {
        DisplayName = "Fate List Window",
        Description = "Displays a list of all fates that are currently active in the current zone",
        Type = ModificationType.NewWindow,
        Authors = ["MidoriKami"],
        ChangeLog = [
            new ChangeLogInfo(1, "Initial Implementation"),
        ],
    };

    private AddonFateList addonFateList = null!;
    private FateListWindowConfig config = null!;
    private FateListWindowConfigWindow configWindow = null!;
    
    private readonly Stopwatch stopwatch = Stopwatch.StartNew();

    public override string ImageName => "FateListWindow.png";

    public override void OnEnable() {
        config = FateListWindowConfig.Load();
        configWindow = new FateListWindowConfigWindow(config);
        configWindow.AddToWindowSystem();
        OpenConfigAction = configWindow.Toggle;

        addonFateList = new AddonFateList(config) {
            NativeController = System.NativeController,
            Size = new Vector2(300.0f, 400.0f),
            InternalName = "FateList",
            Title = "Fate List",
        };
        
        if (config.WindowPosition is { } windowPosition) {
            addonFateList.Position = windowPosition;
        }
        
        Services.Framework.Update += OnFrameworkUpdate;
    }

    public override void OnDisable() {
        addonFateList.Dispose();
        configWindow.RemoveFromWindowSystem();
        
        Services.Framework.Update -= OnFrameworkUpdate;
    }
    
    private unsafe void OnFrameworkUpdate(IFramework framework) {
        if (UIInputData.Instance()->IsComboPressed(config.OpenKeyCombo.ToArray()) && stopwatch.ElapsedMilliseconds >= 250) {
            if (addonFateList.IsOpen) {
                addonFateList.Close();
            }
            else {
                addonFateList.Open();
            }
            
            stopwatch.Restart();
        }
    }
}
