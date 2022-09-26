using System;
using HarmonyLib;
using System.Collections.Generic;
using Hazel;

namespace TownOfHost
{
    class ExileControllerWrapUpPatch
    {
        public static GameData.PlayerInfo AntiBlackout_LastExiled;
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
        class BaseExileControllerPatch
        {
            public static void Postfix(ExileController __instance)
            {
                try
                {
                    WrapUpPostfix(__instance.exiled);
                }
                finally
                {
                    WrapUpFinalizer(__instance.exiled);
                }
            }
        }

        [HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
        class AirshipExileControllerPatch
        {
            public static void Postfix(AirshipExileController __instance)
            {
                try
                {
                    WrapUpPostfix(__instance.exiled);
                }
                finally
                {
                    WrapUpFinalizer(__instance.exiled);
                }
            }
        }
        static void WrapUpPostfix(GameData.PlayerInfo exiled)
        {
            if (AntiBlackout.OverrideExiledPlayer)
            {
                exiled = AntiBlackout_LastExiled;
            }

            Main.witchMeeting = false;
            bool DecidedWinner = false;
            if (!AmongUsClient.Instance.AmHost) return; //ホスト以外はこれ以降の処理を実行しません
            AntiBlackout.RestoreIsDead(doSend: false);
            if (exiled != null)
            {
                exiled.IsDead = true;
                PlayerState.SetDeathReason(exiled.PlayerId, PlayerState.DeathReason.Vote);
                if (Main.showEjections)
                {
                    exiled.Object.Data.PlayerName = Main.LastVotedPlayer;
                    exiled.Object.name = Main.LastVotedPlayer;
                }
                var role = exiled.GetCustomRole();
                Main.DeadPlayersThisRound.Add(exiled.PlayerId);
                /*if (Main.RealOptionsData.ConfirmImpostor)
                {
                    exiled.PlayerName = exiled.GetNameWithRole();
                }*/
                if (role == CustomRoles.Jester && AmongUsClient.Instance.AmHost)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EndGame, Hazel.SendOption.Reliable, -1);
                    writer.Write((byte)CustomWinner.Jester);
                    writer.Write(exiled.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPC.JesterExiled(exiled.PlayerId);
                    DecidedWinner = true;
                }
                if (role == CustomRoles.Child && AmongUsClient.Instance.AmHost)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EndGame, Hazel.SendOption.Reliable, -1);
                    writer.Write((byte)CustomWinner.Child);
                    writer.Write(exiled.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    //RPC.ChildWin(exiled.PlayerId);
                    Utils.ChildWin(exiled);
                    DecidedWinner = true;
                }
                if (role == CustomRoles.Jackal && AmongUsClient.Instance.AmHost)
                {
                    Main.JackalDied = true;
                    if (Options.SidekickGetsPromoted.GetBool())
                    {
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc.Is(CustomRoles.Sidekick))
                                pc.RpcSetCustomRole(CustomRoles.Jackal);
                        }
                    }
                }
                if (role == CustomRoles.Terrorist && AmongUsClient.Instance.AmHost)
                {
                    Utils.CheckTerroristWin(exiled);
                    DecidedWinner = true;
                }
                if (!exiled.Object.Is(CustomRoles.HexMaster) && exiled.Object.IsHexedDone() && AmongUsClient.Instance.AmHost)
                {
                    DecidedWinner = true;
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (!pc.Data.IsDead && !pc.GetCustomRole().IsCoven())
                        {
                            if (!pc.Is(CustomRoles.Pestilence))
                            {
                                pc.RpcMurderPlayer(pc);
                                PlayerState.SetDeathReason(pc.PlayerId, PlayerState.DeathReason.Bombed);
                                PlayerState.SetDead(pc.PlayerId);
                            }
                        }
                    }
                }
            }
            foreach (var kvp in Main.ExecutionerTarget)
            {
                var executioner = Utils.GetPlayerById(kvp.Key);
                if (executioner == null) continue;
                if (executioner.Data.IsDead || executioner.Data.Disconnected) continue; //Keyが死んでいたらor切断していたらこのforeach内の処理を全部スキップ
                if (kvp.Value == exiled.PlayerId && AmongUsClient.Instance.AmHost && !DecidedWinner)
                {
                    //RPC送信開始
                    Main.ExeCanChangeRoles = false;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EndGame, Hazel.SendOption.Reliable, -1);
                    writer.Write((byte)CustomWinner.Executioner);
                    writer.Write(kvp.Key);
                    AmongUsClient.Instance.FinishRpcImmediately(writer); //終了

                    RPC.ExecutionerWin(kvp.Key);
                }
            }
            if (exiled.Object.Is(CustomRoles.TimeThief))
                exiled.Object.ResetVotingTime();
            if (exiled.Object.Is(CustomRoles.SchrodingerCat) && Options.SchrodingerCatExiledTeamChanges.GetBool())
                exiled.Object.ExiledSchrodingerCatTeamChange();

            Main.VetIsAlerted = false;
            Main.HexesThisRound = 0;

            if (Main.currentWinner != CustomWinner.Terrorist) PlayerState.SetDead(exiled.PlayerId);
            {
                if (AmongUsClient.Instance.AmHost && Main.IsFixedCooldown)
                    Main.RefixCooldownDelay = Options.DefaultKillCooldown - 3f;
                Main.SpelledPlayer.RemoveAll(pc => pc == null || pc.Data == null || pc.Data.IsDead || pc.Data.Disconnected);
                Main.SilencedPlayer.RemoveAll(pc => pc == null || pc.Data == null || pc.Data.IsDead || pc.Data.Disconnected);
                Main.IsHackMode = false;
                Main.DoingYingYang = true;
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    pc.ResetKillCooldown();
                    if (Options.MayorHasPortableButton.GetBool() && pc.Is(CustomRoles.Mayor))
                        pc.RpcResetAbilityCooldown();
                    if (pc.Is(CustomRoles.Veteran))
                        pc.RpcResetAbilityCooldown();
                    if (pc.Is(CustomRoles.Warlock))
                    {
                        Main.CursedPlayers[pc.PlayerId] = null;
                        Main.isCurseAndKill[pc.PlayerId] = false;
                    }
                }
            }
            /*Main.AfterMeetingDeathPlayers.Do(x =>
            {
                var player = Utils.GetPlayerById(x.Key);
                Logger.Info($"{player.GetNameWithRole()}を{x.Value}で死亡させました", "AfterMeetingDeath");
                PlayerState.SetDeathReason(x.Key, x.Value);
                PlayerState.SetDead(x.Key);
                player?.RpcExileV2();
                if (player.Is(CustomRoles.TimeThief) && x.Value == PlayerState.DeathReason.LoversSuicide)
                    player?.ResetVotingTime();
            });*/
            FallFromLadder.Reset();
            Utils.CountAliveImpostors();
            Utils.AfterMeetingTasks();
            Utils.CustomSyncAllSettings();
            Utils.NotifyRoles();
        }

        static void WrapUpFinalizer(GameData.PlayerInfo exiled)
        {
            //WrapUpPostfixで例外が発生しても、この部分だけは確実に実行されます。
            if (AmongUsClient.Instance.AmHost)
            {
                new LateTask(() =>
                {
                    AntiBlackout.SendGameData();
                    if (AntiBlackout.OverrideExiledPlayer && // 追放対象が上書きされる状態 (上書きされない状態なら実行不要)
                        exiled != null && //exiledがnullでない
                        exiled.Object != null) //exiled.Objectがnullでない
                    {
                        exiled.Object.RpcExileV2();
                    }
                }, 0.5f, "Restore IsDead Task");
                Main.IsRampaged = false;
                Main.RampageReady = false;
                Main.IsRoundOne = false;
                Main.IsRoundOneGA = false;
                Main.IsGazing = false;
                Main.GazeReady = false;
                Main.bkProtected = false;
                new LateTask(() =>
                {
                    Main.RampageReady = true;
                }, Options.RampageCD.GetFloat(), "Werewolf Rampage Cooldown (After Meeting)");
                new LateTask(() =>
                    {
                        Main.GazeReady = true;
                    }, Options.StoneCD.GetFloat(), "Gaze Cooldown");
                //Guesser.OpenGuesserMeeting();
                foreach (var x in Main.AfterMeetingDeathPlayers)
                {
                    var player = Utils.GetPlayerById(x.Key);
                    Logger.Info($"{player.GetNameWithRole()}を{x.Value}", "AfterMeetingDeath");
                    PlayerState.SetDeathReason(x.Key, x.Value);
                    PlayerState.SetDead(x.Key);
                    player?.RpcExileV2();

                    if (player.Is(CustomRoles.TimeThief) && x.Value == PlayerState.DeathReason.LoversSuicide)
                        player?.ResetVotingTime();
                }
                Main.AfterMeetingDeathPlayers.Clear();
                Main.DeadPlayersThisRound.Clear();
                Main.MercCanSuicide = true;
                if (Options.SheriffCorrupted.GetBool())
                {
                    if (!Sheriff.csheriff)
                    {
                        int IsAlive = 0;
                        int numCovenAlive = 0;
                        int numImpsAlive = 0;
                        int numNKalive = 0;
                        List<PlayerControl> couldBeTraitors = new();
                        List<byte> couldBeTraitorsid = new();
                        var rando = new Random();
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (!pc.Data.Disconnected)
                                if (!pc.Data.IsDead)
                                {
                                    IsAlive++;
                                    if (pc.GetCustomRole().IsNeutralKilling() && !Options.TraitorCanSpawnIfNK.GetBool())
                                        numNKalive++;
                                    if (pc.GetCustomRole().IsCoven() && !Options.TraitorCanSpawnIfCoven.GetBool())
                                        numCovenAlive++;
                                    if (pc.Is(CustomRoles.Sheriff) || pc.Is(CustomRoles.Investigator))
                                        couldBeTraitors.Add(pc);
                                    if (pc.GetCustomRole().IsImpostor())
                                        numImpsAlive++;
                                }
                        }

                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (!pc.Data.Disconnected)
                                if (!pc.Data.IsDead)
                                {
                                    if (!pc.IsModClient()) continue;
                                    if (!pc.GetCustomRole().IsCrewmate()) continue;
                                    if (!couldBeTraitorsid.Contains(pc.PlayerId))
                                    {
                                        couldBeTraitors.Add(pc);
                                        couldBeTraitorsid.Add(pc.PlayerId);
                                    }
                                }
                        }

                        Sheriff.seer = couldBeTraitors[rando.Next(0, couldBeTraitors.Count)];

                        //foreach (var pva in __instance.playerStates)
                        if (IsAlive >= Options.PlayersForTraitor.GetFloat() && Sheriff.seer != null)
                        {
                            if (numCovenAlive == 0 && numNKalive == 0 && numCovenAlive == 0 && numImpsAlive == 0)
                            {
                                Sheriff.seer.RpcSetCustomRole(CustomRoles.CorruptedSheriff);
                                Sheriff.seer.CustomSyncSettings();
                                Sheriff.csheriff = true;
                                if (Sheriff.seer.IsModClient())
                                    RoleManager.Instance.SetRole(Sheriff.seer, RoleTypes.Impostor);
                            }
                        }
                    }
                }
            }
            Logger.Info("タスクフェイズ開始", "Phase");
        }
    }
}
