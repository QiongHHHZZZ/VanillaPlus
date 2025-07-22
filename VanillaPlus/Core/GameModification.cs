using System;
using System.Linq;
using Dalamud.Interface.Textures.TextureWraps;
using VanillaPlus.Core.Objects;

namespace VanillaPlus.Core;

public abstract class GameModification {
    public abstract ModificationInfo ModificationInfo { get; }

    public abstract void OnEnable();
    public abstract void OnDisable();

    /// <summary>
    /// If implemented, this will add a button that when clicked will call
    /// this to open whatever kind of config window you set up.
    /// </summary>
    public Action? OpenConfig;

    public IDalamudTextureWrap? GetDescriptionImage() => null;
    
    public string Name => LongName.Split(".").Last();
    private string LongName => GetType().ToString();
}
