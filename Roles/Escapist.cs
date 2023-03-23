using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;
using TownOfHost.PrivateExtensions;
using TownOfHost.RoleHelpers;
using UnityEngine;

namespace TownOfHost;

public static class Escapist
{
    public static readonly int ID = 132746701;
    public static List<byte> playerIdlist;

    public static CustomOption MarkCooldown;
    public static CustomOption RecallCooldown;
    public static CustomOption EscapistCanVent;

    public static Dictionary<byte, EscapistState> CurrentEscapistState;
    public static Dictionary<byte, string> EscapistStateString;

    public static Dictionary<byte, Vector2> MarkedArea;
    public static Dictionary<byte, bool> InCooldown;

    public static void SetupCustomOption()
    {
        Options.SetupRoleOptions(ID, CustomRoles.Escapist, AmongUsExtensions.OptionType.Impostor);
        MarkCooldown = CustomOption.Create(ID + 10, Color.white, "MarkCooldown", AmongUsExtensions.OptionType.Impostor, 25f, 2.5f, 60f, 2.5f, Options.CustomRoleSpawnChances[CustomRoles.Escapist]);
        RecallCooldown = CustomOption.Create(ID + 11, Color.white, "RecallCooldown", AmongUsExtensions.OptionType.Impostor, 25f, 2.5f, 60f, 2.5f, Options.CustomRoleSpawnChances[CustomRoles.Escapist]);
        EscapistCanVent = CustomOption.Create(ID + 12, Color.white, "EscapistCanVent", AmongUsExtensions.OptionType.Impostor, true, Options.CustomRoleSpawnChances[CustomRoles.Escapist]);
    }

    public static void Init()
    {
        CurrentEscapistState = new Dictionary<byte, EscapistState>();
        EscapistStateString = new Dictionary<byte, string>();
        MarkedArea = new Dictionary<byte, Vector2>();
        InCooldown = new Dictionary<byte, bool>();

        playerIdlist = new List<byte>();
    }

    public static void Add(PlayerControl player)
    {
        playerIdlist.Add(player.PlayerId);
        CurrentEscapistState.Add(player.PlayerId, EscapistState.Default);
        EscapistStateString.Add(player.PlayerId, "gameStart");
        SendRpc(player.PlayerId);
    }

    public static void StartCooldown(PlayerControl player)
    {
        if (!CurrentEscapistState.ContainsKey(player.PlayerId)) return;
        if (!EscapistStateString.ContainsKey(player.PlayerId)) return;

        switch (CurrentEscapistState[player.PlayerId])
        {
            case EscapistState.OnMark:
                InCooldown[player.PlayerId] = true;
                EscapistStateString[player.PlayerId] = "recall-cooldown";
                SendRpc(player.PlayerId);
                _ = new LateTask(() =>
                {
                    InCooldown[player.PlayerId] = false;
                    EscapistStateString[player.PlayerId] = "recall-ready";
                    CurrentEscapistState[player.PlayerId] = EscapistState.OnRecall;
                    SendRpc(player.PlayerId);
                }, RecallCooldown.GetFloat(), "Recall Cooldown");
                break;
            case EscapistState.OnRecall:
                InCooldown[player.PlayerId] = true;
                EscapistStateString[player.PlayerId] = "mark-cooldown";
                SendRpc(player.PlayerId);
                _ = new LateTask(() =>
                {
                    InCooldown[player.PlayerId] = false;
                    EscapistStateString[player.PlayerId] = "mark-ready";
                    CurrentEscapistState[player.PlayerId] = EscapistState.OnMark;
                    SendRpc(player.PlayerId);
                }, MarkCooldown.GetFloat(), "Mark Cooldown");
                break;
        }
    }

    public static float GetCooldown(PlayerControl player)
    {
        if (!EscapistStateString.ContainsKey(player.PlayerId)) return 99999f;

        switch (EscapistStateString[player.PlayerId])
        {
            case "recall-cooldown":
                return MarkCooldown.GetFloat();
            case "mark-cooldown":
                return RecallCooldown.GetFloat();
            case "recall-ready":
            case "mark-ready":

                return 0f;
            default:
                return 10f;
        }
    }

    public static void OnPet(PlayerControl player)
    {
        if (!CurrentEscapistState.ContainsKey(player.PlayerId)) return;
        if (!EscapistStateString.ContainsKey(player.PlayerId)) return;

        string currentState = EscapistStateString[player.PlayerId];

        switch (CurrentEscapistState[player.PlayerId])
        {
            case EscapistState.Default:
                if (currentState == "gameStart")
                {
                    CurrentEscapistState[player.PlayerId] = EscapistState.OnMark;
                    MarkedArea.Add(player.PlayerId, player.GetTruePosition());
                    StartCooldown(player);
                }
                break;
            case EscapistState.OnMark:
                if (currentState == "mark-ready")
                {
                    //CurrentEscapistState[player.PlayerId] = EscapistState.OnRecall;
                    MarkedArea.Add(player.PlayerId, player.GetTruePosition());
                    StartCooldown(player);
                }
                break;
            case EscapistState.OnRecall:
                if (currentState == "recall-ready")
                {
                    //CurrentEscapistState[player.PlayerId] = EscapistState.OnMark;
                    var position = MarkedArea[player.PlayerId];
                    MarkedArea.Remove(player.PlayerId);
                    Utils.TP(player.NetTransform, new Vector2(position.x, position.y));
                    StartCooldown(player);
                }
                break;
        }
    }

    public static string GetEscapistState(PlayerControl player)
    {
        if (!EscapistStateString.ContainsKey(player.PlayerId)) return "No State Found.";
        return EscapistStateString[player.PlayerId];
    }

    public static string GetAbilityButtonText(PlayerControl player)
    {
        string currentState = EscapistStateString[player.PlayerId];
        switch (currentState)
        {
            case "recall-ready":
            case "recall-cooldown":
                return "Recall";
            case "mark-ready":
            case "mark-cooldown":
                return "Mark";
            default:
                return Translator.GetString("DefaultShapeshiftText");
        }
    }

    public static bool CanVent() => EscapistCanVent.GetBool();

    public static void ApplyGameOptions(PlayerControl player,NormalGameOptionsV07 options)
    {
        options.GetShapeshifterOptions().ShapeshifterCooldown = GetCooldown(player);
        options.GetShapeshifterOptions().ShapeshifterDuration = 1f;
        options.GetShapeshifterOptions().ShapeshifterLeaveSkin = false;
    }
    public static void SendRpc(byte playerId)
    {
        if (!EscapistStateString.ContainsKey(playerId)) return;
        if (!InCooldown.ContainsKey(playerId)) return;
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetEscapistState, SendOption.Reliable, -1);
        writer.Write(playerId);
        writer.Write(EscapistStateString[playerId]);
        writer.Write(InCooldown[playerId]);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    public static void HandleRpc(MessageReader reader)
    {
        byte playerId = reader.ReadByte();
        string escapistState = reader.ReadString();
        bool inCooldown = reader.ReadBoolean();
        EscapistStateString[playerId] = escapistState;
        InCooldown[playerId] = inCooldown;
    }
}