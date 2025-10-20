using System;
using System.Linq;
using VanillaPlus.Localization;

namespace VanillaPlus.Classes;

public abstract class GameModification {
    private ModificationInfo? localizedModificationInfo;

    protected abstract ModificationInfo CreateModificationInfo { get; }

    public ModificationInfo ModificationInfo {
        get {
            localizedModificationInfo ??= LocalizationProvider.Localize(GetType(), CreateModificationInfo);
            return localizedModificationInfo;
        }
    }

    public abstract void OnEnable();
    public abstract void OnDisable();

    public Action? OpenConfigAction { get; set; }

    /// <summary>
    /// Indicates this modification is experimental in nature and may cause issues.
    /// </summary>
    public virtual bool IsExperimental => false;
    
    /// <summary>
    /// Set this to the filename of an image in the Assets folder, the image must be square or it will render very weirdly.
    /// </summary>
    public virtual string? ImageName => null;

    protected void RefreshLocalizedInfo()
        => localizedModificationInfo = LocalizationProvider.Localize(GetType(), CreateModificationInfo);

    public string Name => LongName.Split(".").Last();
    private string LongName => GetType().ToString();
}
