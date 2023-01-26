using System.Collections.Generic;
using Hazel;
using UnityEngine;
using static TownOfHost.Translator;
using System.Linq;
using System.Threading.Tasks;
using System;
using HarmonyLib;
//using Object = UnityEngine.Object;

namespace TownOfHost
{
    public static class AgiTater
    {
        private static readonly int Id = 32434954;
        public static List<byte> playerIdList = new();

        public static bool BombedThisRound = false;

        public static CustomOption BombCooldown;
        public static CustomOption PassCooldown;
        public static CustomOption ReportBait;

        public static byte CurrentBombedPlayer = 255;
        public static byte LastBombedPlayer = 255;
        public static bool CanPass = true;

        public static void SetupCustomOption()
        {
            Options.SetupSingleRoleOptions(Id, CustomRoles.AgiTater, 1, AmongUsExtensions.OptionType.Neutral);
            BombCooldown = CustomOption.Create(Id + 232, Color.white, "BombCooldown", AmongUsExtensions.OptionType.Neutral, 20f, 10f, 60f, 2.5f, Options.CustomRoleSpawnChances[CustomRoles.AgiTater]);
            PassCooldown = CustomOption.Create(Id + 233, Color.white, "PassCooldown", AmongUsExtensions.OptionType.Neutral, 1f, 0f, 5f, 0.25f, Options.CustomRoleSpawnChances[CustomRoles.AgiTater]);
            ReportBait = CustomOption.Create(Id + 234, Color.white, "ReportBaitAgi", AmongUsExtensions.OptionType.Neutral, true, Options.CustomRoleSpawnChances[CustomRoles.AgiTater]);
        }

        public static bool IsEnable() => playerIdList.Count != 0;

        public static void Reset()
        {
            playerIdList = new();
            BombedThisRound = false;
            CurrentBombedPlayer = 255;
            LastBombedPlayer = 255;
            CanPass = true;
        }

        public static void Add(PlayerControl agi)
        {
            playerIdList.Add(agi.PlayerId);
        }

        public static void ResetBomb(bool bombAgain = true)
        {
            BombedThisRound = !bombAgain;
            CurrentBombedPlayer = 255;
            LastBombedPlayer = 255;
            SendRPC(255, 255);
        }

        public static void PassBomb(PlayerControl player, PlayerControl target)
        {
            if (PassCooldown.GetFloat() != 0f)
                CanPass = false;
            if (target.Is(CustomRoles.Pestilence) || (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted))
                target.RpcMurderPlayer(player);
            else
            {
                LastBombedPlayer = CurrentBombedPlayer;
                CurrentBombedPlayer = target.PlayerId;
            }
            Utils.CustomSyncAllSettings();
            Utils.NotifyRoles(GameStates.IsMeeting, player);
            Utils.NotifyRoles(GameStates.IsMeeting, target);
            SendRPC(CurrentBombedPlayer, LastBombedPlayer);
            new LateTask(() =>
            {
                CanPass = true;
            }, PassCooldown.GetFloat(), "Agitater Can Pass");
        }
        public static void SendRPC(byte newbomb, byte oldbomb)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RpcPassBomb, SendOption.Reliable, -1);
            writer.Write(newbomb);
            writer.Write(oldbomb);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
    }
}