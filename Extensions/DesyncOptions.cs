using AmongUs.GameOptions;
using Hazel;
using HarmonyLib;

namespace TownOfHost.PrivateExtensions;
/*
 * code by 단풍잎
*/

public class DesyncOptions
{
    public static IGameOptions originalHostOptions;

    public static void SyncToPlayer(IGameOptions options, PlayerControl player)
    {
        if (AmongUsClient.Instance.AmHost && player.PlayerId == PlayerControl.LocalPlayer.PlayerId)
        {
            GameManager.Instance.LogicOptions.Cast<LogicOptionsNormal>().GameOptions = options.AsNormalOptions();
        }
        else
        {
            SyncToClient(options, AmongUsClient.Instance.GetClientFromCharacter(player).Id);
        }
    }

    public static void SyncToClient(IGameOptions options, int clientId)
    {
        GameOptionsFactory optionsFactory = GameOptionsManager.Instance.gameOptionsFactory;

        MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable); // Start message writer
        messageWriter.StartMessage(6); // Initial control-flow path for packet receival (Line 1352 InnerNetClient.cs) || This can be changed to "5" and remove line 20 to sync options to everybody
        messageWriter.Write(AmongUsClient.Instance.GameId); // Write 4 byte GameId
        messageWriter.WritePacked(clientId); // Target player ID

        messageWriter.StartMessage(1); // Second control-flow path specifically for changing game options
        messageWriter.WritePacked(GetManagerClientId()); // Packed ID for game manager

        messageWriter.StartMessage(4); // Index of logic component in GameManager (4 is current LogicOptionsNormal)
        optionsFactory.ToNetworkMessageWithSize(messageWriter, options); // Write options to message

        messageWriter.EndMessage(); // Finish message 1
        messageWriter.EndMessage(); // Finish message 2
        messageWriter.EndMessage(); // Finish message 3
        AmongUsClient.Instance.SendOrDisconnect(messageWriter); // Wrap up send
        messageWriter.Recycle(); // Recycle
    }

    // This method is used to find the "GameManager" client which is now needed for synchronizing options
    public static int GetManagerClientId()
    {
        int clientId = -1;
        var allClients = AmongUsClient.Instance.allObjectsFast;
        var allClientIds = allClients.Keys;

        foreach (uint id in allClientIds)
            if (clientId == -1 && allClients[id].name.Contains("Manager"))
                clientId = (int)id;
        return clientId;
    }
}
[HarmonyPatch(typeof(LogicOptionsNormal), nameof(LogicOptions.SetGameOptions))]
public static class TestPatch
{
    // stop the host client from setting their options again
    public static bool Prefix([HarmonyArgument(index: 0)] IGameOptions newOptions)
    {
        if (AmongUsClient.Instance.AmHost) return false;
        return true;
    }
}