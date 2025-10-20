using System.ComponentModel;

namespace VanillaPlus.Classes;

public enum ModificationType {
    
    /// <summary>
    /// Not intended for actual use, only used to prevent loading SampleGameModification
    /// </summary>
    Hidden,

    /// <summary>
    /// Not intended for actual use
    /// </summary>
    [Description("调试功能")]
    Debug,
    
    /// <summary>
    /// Adds a new native window to the game
    /// </summary>
    [Description("自定义原生窗口")]
    NewWindow,
    
    /// <summary>
    /// Modifies some aspect of the base games user interface
    /// </summary>
    [Description("界面调整")]
    UserInterface,
    
    /// <summary>
    /// Modifies some type of base game functionality to make it behave differently
    /// </summary>
    [Description("游戏行为调整")]
    GameBehavior,

    /// <summary>
    /// Adds a new native overlay to the game
    /// </summary>
    [Description("原生覆盖层")]
    NewOverlay,
}
