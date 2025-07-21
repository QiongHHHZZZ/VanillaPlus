namespace VanillaPlus.Core.Objects;

public enum ModificationType {
    
    /// <summary>
    /// Modifies some type of base game functionality to make it behave differently
    /// </summary>
    GameBehavior,
    
    /// <summary>
    /// Modifies some aspect of the base games user interface
    /// </summary>
    UserInterface,
    
    /// <summary>
    /// Adds a new native window to the game
    /// </summary>
    NewWindow,
    
    /// <summary>
    /// Adds a new native overlay to the game
    /// </summary>
    NewOverlay,
}
