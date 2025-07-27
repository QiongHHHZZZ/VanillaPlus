using System.Linq;

namespace VanillaPlus.Core;

public abstract class GameModification {
    public abstract ModificationInfo ModificationInfo { get; }

    public abstract void OnEnable();
    public abstract void OnDisable();

    public virtual bool HasConfigWindow => false;
    public virtual void OpenConfigWindow() { }

    /// <summary>
    /// Indicates this modification is experimental in nature and may cause issues.
    /// </summary>
    public virtual bool IsExperimental => false;
    
    /// <summary>
    /// Set this to the filename of an image in the Assets folder, the image must be square or it will render very weirdly.
    /// </summary>
    public virtual string? ImageName => null;

    public string Name => LongName.Split(".").Last();
    private string LongName => GetType().ToString();
}
