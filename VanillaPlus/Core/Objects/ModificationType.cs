using System.ComponentModel;

namespace VanillaPlus.Core.Objects;

public enum ModificationType {
    
    /// <summary>
    /// Modifies some type of base game functionality to make it behave differently
    /// </summary>
    [Description("Game Behavior Modification")]
    GameBehavior,
    
    /// <summary>
    /// Modifies some aspect of the base games user interface
    /// </summary>
    [Description("UI Modification")]
    UserInterface,
    
    /// <summary>
    /// Adds a new native window to the game
    /// </summary>
    [Description("Custom Native Windows")]
    NewWindow,
    
    /// <summary>
    /// Adds a new native overlay to the game
    /// </summary>
    [Description("Custom Native Overlay")]
    NewOverlay,
}
