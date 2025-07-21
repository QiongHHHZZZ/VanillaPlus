using System;
using Dalamud.Interface.Textures.TextureWraps;
using VanillaPlus.Core.Objects;

namespace VanillaPlus.Core;

public abstract class GameModification {
    public abstract ModificationInfo ModificationInfo { get; }

    public abstract void OnEnable();
    public abstract void OnDisable();

    public event Action? OpenConfig;

    public IDalamudTextureWrap? GetDescriptionImage() 
        => null;
}
