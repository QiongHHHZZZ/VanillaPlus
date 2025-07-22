namespace VanillaPlus.Core.Objects;

public class LoadedModification(GameModification modification, LoadedState state = LoadedState.Unknown) {
    public GameModification Modification { get; set; } = modification;
    public LoadedState State { get; set; } = state;
    
    public string Name => Modification.Name;
}
