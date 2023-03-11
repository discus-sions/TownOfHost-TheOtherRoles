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

        public static CustomOption AgiTaterGetsAdvantage;
        public static CustomOption AgiTaterBombCooldown;
        public static CustomOption AgiTaterCanBombMoreThanOnce;

        public static byte CurrentBombedPlayer = 255;
        public static byte LastBombedPlayer = 255;
        public static bool CanPass = true;

        public static void SetupCustomOption()
        {
            Options.SetupSingleRoleOptions(Id, CustomRoles.AgiTater, 1, AmongUsExtensions.OptionType.Neutral);
            BombCooldown = CustomOption.Create(Id + 232, Color.white, "BombCooldown", AmongUsExtensions.OptionType.Neutral, 20f, 10f, 60f, 2.5f, Options.CustomRoleSpawnChances[CustomRoles.AgiTater]);
            PassCooldown = CustomOption.Create(Id + 233, Color.white, "PassCooldown", AmongUsExtensions.OptionType.Neutral, 1f, 0f, 5f, 0.25f, Options.CustomRoleSpawnChances[CustomRoles.AgiTater]);
            ReportBait = CustomOption.Create(Id + 234, Color.white, "ReportBaitAgi", AmongUsExtensions.OptionType.Neutral, true, Options.CustomRoleSpawnChances[CustomRoles.AgiTater]);
            AgiTaterGetsAdvantage = CustomOption.Create(Id + 235, Color.white, "AgiTaterGetsAdvantage", AmongUsExtensions.OptionType.Neutral, false, Options.CustomRoleSpawnChances[CustomRoles.AgiTater]);
            AgiTaterBombCooldown = CustomOption.Create(Id + 236, Color.white, "AgiTaterBombCooldown", AmongUsExtensions.OptionType.Neutral, 30f, 10f, 60f, 2.5f, AgiTaterGetsAdvantage);
            AgiTaterCanBombMoreThanOnce = CustomOption.Create(Id + 237, Color.white, "AgiTaterCanBombMoreThanOnce", AmongUsExtensions.OptionType.Neutral, false, AgiTaterGetsAdvantage);
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

        public static void PassBomb(PlayerControl player, PlayerControl target, bool IsAgitater = false)
        {
            if (target.Data.IsDead) return;
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
            Logger.Msg($"{player.GetNameWithRole()} passed bomb to {target.GetNameWithRole()}", "Agitater Pass");
            if (AgiTaterGetsAdvantage.GetBool())
            {
                if (AgiTaterCanBombMoreThanOnce.GetBool() && IsLastKiller())
                {
                    BombedThisRound = false;
                }
                if (IsAgitater && IsLastKiller())
                {
                    new LateTask(() =>
                    {
                        var bombed = Utils.GetPlayerById(CurrentBombedPlayer);
                        if (!bombed.Is(CustomRoles.Pestilence))
                        {
                            if (bombed.Is(CustomRoles.Veteran))
                            {
                                if (!Main.VetIsAlerted)
                                {
                                    bombed.RpcMurderPlayer(bombed);
                                    PlayerState.SetDeathReason(bombed.PlayerId, PlayerState.DeathReason.Bombed);
                                }
                            }
                            else
                            {
                                if (bombed.GetCustomSubRole() is CustomRoles.Bait && ReportBait.GetBool())
                                {
                                    bombed.RpcMurderPlayer(bombed);
                                    PlayerState.SetDeathReason(bombed.PlayerId, PlayerState.DeathReason.Bombed);
                                    foreach (var playerid in playerIdList)
                                    {
                                        var pc = Utils.GetPlayerById(playerid);
                                        Logger.Info(bombed?.Data?.PlayerName + "はBaitだった", "MurderPlayer");
                                        new LateTask(() => pc.CmdReportDeadBody(bombed.Data), 0.15f, "Bait Self Report");
                                    }
                                    ResetBomb();
                                }
                                else
                                {
                                    bombed.RpcMurderPlayer(bombed);
                                    PlayerState.SetDeathReason(bombed.PlayerId, PlayerState.DeathReason.Bombed);
                                }
                            }
                        }
                        CurrentBombedPlayer = 255;
                        LastBombedPlayer = 255;
                        SendRPC(255, 255);
                    }, AgiTaterBombCooldown.GetFloat(), "Agitater Last Killer Bomb", true);
                }
            }
            if (PassCooldown.GetFloat() != 0f)
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

        public static bool IsLastKiller()
        {
            CheckGameEndPatch.PlayerStatistics statistics = new CheckGameEndPatch.PlayerStatistics();
            return statistics.TeamImpostorsAlive <= 0 && statistics.TeamJuggernautAlive <= 0 && statistics.TeamPestiAlive <= 0 && statistics.TeamJackalAlive <= 0
                && statistics.TeamWolfAlive <= 0 && statistics.TeamCovenAlive <= 0 && statistics.TeamKnightAlive <= 0 && statistics.TeamGlitchAlive <= 0 && statistics.TeamArsoAlive <= 0;
        }
    }
}