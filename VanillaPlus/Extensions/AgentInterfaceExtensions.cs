using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace VanillaPlus.Extensions;

public static class AgentInterfaceExtensions {
    public static unsafe void SendCommand(this ref AgentInterface agent, uint eventKind, int[] commandValues) {
        using var returnValue = new AtkValue();
        var command= stackalloc AtkValue[commandValues.Length];

        for (var index = 0; index < commandValues.Length; index++) {
            command[index].SetInt(commandValues[index]);
        }
        
        agent.ReceiveEvent(&returnValue, command, (uint) commandValues.Length, eventKind);
    }
}
