using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;

namespace VanillaPlus.Extensions;

public static class GameObjectExtensions {
    public static bool IsPet(this IGameObject gameObject)
        => gameObject is { ObjectKind: ObjectKind.BattleNpc, SubKind: (byte)BattleNpcSubKind.Pet };
    
    public static bool IsPetOrOwner(this IGameObject? gameObject) {
        if (gameObject is null) return false;
        if (gameObject.IsPet()) return true;
        if (Services.ObjectTable.ClientObjects.Any(obj => obj.IsPet() && obj.OwnerId == gameObject.EntityId)) return true;

        return false;
    }
}
