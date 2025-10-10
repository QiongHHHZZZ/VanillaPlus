using System.Numerics;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace VanillaPlus.Extensions;

public static unsafe class AtkUnitBaseExtensions {
    public static T* GetNodeById<T>(this ref AtkUnitBase addon, uint nodeId) where T : unmanaged
        => addon.UldManager.SearchNodeById<T>(nodeId);

    public static Vector2 GetSize(this ref AtkUnitBase addon) {
        var width = stackalloc short[1];
        var height = stackalloc short[1];

        addon.GetSize(width, height, false);
        return new Vector2(*width, *height);
    }

    public static Vector2 GetPosition(this ref AtkUnitBase addon)
        => new(addon.X, addon.Y);

    /// <summary>
    /// Resizes the target addon to the new size, making sure to adjust various WindowNode properties
    /// to make the window appear and behave normally.
    /// </summary>
    /// <param name="addon">Pointer to the addon you wish to resize</param>
    /// <param name="newSize">The new size of the addon</param>
    public static void Resize(this ref AtkUnitBase addon, Vector2 newSize) {
        var windowNode = addon.WindowNode;
        if (windowNode is null) return;

        addon.WindowNode->SetWidth((ushort)newSize.X);
        addon.WindowNode->SetHeight((ushort)newSize.Y);

        if (addon.WindowHeaderCollisionNode is not null) {
            addon.WindowHeaderCollisionNode->SetWidth((ushort)(newSize.X - 14.0f));
        }

        addon.SetSize((ushort)newSize.X, (ushort)newSize.Y);

        addon.WindowNode->Component->UldManager.UpdateDrawNodeList();
        addon.UpdateCollisionNodeList(false);
    }
}
