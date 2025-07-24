using System;
using System.Collections.Generic;
using System.Linq;
using VanillaPlus.Extensions;

namespace VanillaPlus.Core.Objects;

public class ModificationInfo {
    public int Version { get; init; } = 1;
    public required string DisplayName { get; init; }
    public required string Description { get; init; }
    public required string[] Authors { get; init; }
    public required ModificationType Type { get; init; }
    public List<ChangeLogInfo> ChangeLog { get; init; } = [];

    public bool IsMatch(string searchTerm) {
        if (DisplayName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) return true;
        // if (Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) return true; // Probably not a good idea to use this without a fuzzy matcher.
        if (Authors.Any(author => author.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))) return true;
        if (Type.GetDescription().Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) return true;
        
        return false;
    }
}
