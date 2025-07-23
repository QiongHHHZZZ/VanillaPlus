using System.Linq;
using Dalamud.Interface.Textures.TextureWraps;
using VanillaPlus.Core.Objects;

namespace VanillaPlus.Core;

public abstract class GameModification {
    public abstract ModificationInfo ModificationInfo { get; }

    public abstract void OnEnable();
    public abstract void OnDisable();

    public virtual bool HasConfigWindow => false;
    public virtual void OpenConfigWindow() { }

    public IDalamudTextureWrap? GetDescriptionImage() => null;
    
    public string Name => LongName.Split(".").Last();
    private string LongName => GetType().ToString();
}
