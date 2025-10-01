using Dalamud.Game.Gui.ContextMenu;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace VanillaPlus.Extensions;

public static unsafe class MenuOpenedArgsExtensions {
    public static AgentContext* GetContext(this IMenuOpenedArgs args)
        => (AgentContext*)args.AgentPtr;
}
