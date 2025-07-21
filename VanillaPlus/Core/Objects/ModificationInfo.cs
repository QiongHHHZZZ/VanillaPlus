using System.Collections.Generic;

namespace VanillaPlus.Core.Objects;

public class ModificationInfo {
    public int Version { get; init; } = 1;
    public required string DisplayName { get; init; }
    public required string Description { get; init; }
    public required string[] Authors { get; init; }
    public required ModificationType Type { get; init; }
    public List<ChangeLogInfo> ChangeLog { get; init; } = [];
}
