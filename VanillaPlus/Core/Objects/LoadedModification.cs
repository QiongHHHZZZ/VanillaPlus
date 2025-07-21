namespace VanillaPlus.Core.Objects;

public record LoadedModification(GameModification Modification, LoadedState State = LoadedState.Unknown);
