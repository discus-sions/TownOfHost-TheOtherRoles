using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using Hazel;
using static TownOfHost.Translator;

namespace TownOfHost
{
    enum CustomRPC
    {
        VersionCheck = 60,
        SyncCustomSettings = 80,
        SetDeathReason,
        EndGame,
        PlaySound,
        SetCustomRole,
        SetBountyTarget,
        SetKillOrSpell,
        SetKillOrSilence,
        SetSheriffShotLimit,
        SetTimeThiefKillCount,
        SetDousedPlayer,
        AddNameColorData,
        RemoveNameColorData,
        ResetNameColorData,
        DoSpell,
        DoSilence,
        SniperSync,
        SetLoversPlayers,
        SetExecutionerTarget,
        RemoveExecutionerTarget,
        SetGATarget,
        RemoveGATarget,
        SendFireWorksState,
        SetCurrentDousingTarget,
        SetCurrentInfectingTarget,
        SetCurrentHexingTarget,
        ToggleCamouflagueActive,
        // FIX SOME CLIENT ISSUES //
        SendBadId,
        SendGoodId,
        SetVeteranAlert,
        SetMedusaInfo,
        SetHackerProgress,
        SetPirateProgress,
        SeeredPlayer,
        SetVultureAmount,
        UpdateGA,
        NotifyDemoKill,
        SendSurvivorInfo,
        SetInfectedPlayer,
        SetHexedPlayer,
    }
    public enum Sounds
    {
        KillSound,
        TaskComplete
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    class RPCHandlerPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            var rpcType = (RpcCalls)callId;
            Logger.Info($"{__instance?.Data?.PlayerId}({__instance?.Data?.PlayerName}):{callId}({RPC.GetRpcName(callId)})", "ReceiveRPC");
            MessageReader subReader = MessageReader.Get(reader);
            switch (rpcType)
            {
                case RpcCalls.SetName: //SetNameRPC
                    string name = subReader.ReadString();
                    if (subReader.BytesRemaining > 0 && subReader.ReadBoolean()) return false;
                    Logger.Info("名前変更:" + __instance.GetNameWithRole() + " => " + name, "SetName");
                    break;
                case RpcCalls.SendChat:
                    var text = subReader.ReadString();
                    /*if (Main.SilencedPlayer.Count != 0)
                    {
                        //someone is silenced
                        foreach (var player in Main.SilencedPlayer)
                        {
                            if (player == __instance) continue;
                            text = "Silenced.";
                            Logger.Info($"{__instance.GetNameWithRole()}:{text}", "TriedToSendChatButSilenced");
                            if (!player.Data.IsDead)
                            {
                                Utils.SendMessage("You are currently Silenced. Try talking again when you aren't silenced.", __instance.PlayerId);
                            }
                        }
                    }
                    else
                    {
                        Logger.Info($"{__instance.GetNameWithRole()}:{text}", "SendChat");
                        ChatCommands.OnReceiveChat(__instance, text);
                    }*/
                    ChatCommands.OnReceiveChat(__instance, text);
                    break;
                case RpcCalls.StartMeeting:
                    var p = Utils.GetPlayerById(subReader.ReadByte());
                    Logger.Info($"{__instance.GetNameWithRole()} => {p?.GetNameWithRole() ?? "null"}", "StartMeeting");
                    break;
            }
            if (__instance.PlayerId != 0 && Enum.IsDefined(typeof(CustomRPC), (int)callId) && callId != (byte)CustomRPC.VersionCheck) //ホストではなく、CustomRPCで、VersionCheckではない
            {
                Logger.Warn($"{__instance?.Data?.PlayerName}:{callId}({RPC.GetRpcName(callId)}) ホスト以外から送信されたためキャンセルしました。", "CustomRPC");
                if (AmongUsClient.Instance.AmHost)
                {
                    AmongUsClient.Instance.KickPlayer(__instance.GetClientId(), false);
                    Logger.Warn($"不正なRPCを受信したため{__instance?.Data?.PlayerName}をキックしました。", "Kick");
                    Logger.SendInGame(string.Format(GetString("Warning.InvalidRpc"), __instance?.Data?.PlayerName));
                }
                return false;
            }
            return true;
        }
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            //if (!AmongUsClient.Instance.AmHost) return;
            var rpcType = (CustomRPC)callId;
            switch (rpcType)
            {
                case CustomRPC.VersionCheck:
                    try
                    {
                        string version = reader.ReadString();
                        string tag = reader.ReadString();
                        Main.playerVersion[__instance.PlayerId] = new PlayerVersion(version, tag);
                    }
                    catch
                    {
                        Logger.Warn($"{__instance?.Data?.PlayerName}({__instance.PlayerId}): バージョン情報が無効です", "RpcVersionCheck");
                        if (AmongUsClient.Instance.AmHost)
                        {
                            AmongUsClient.Instance.KickPlayer(__instance.GetClientId(), false);
                            Logger.Info($"不正なRPCを受信したため{__instance?.Data?.PlayerName}をキックしました。", "Kick");
                            Logger.SendInGame(string.Format(GetString("Warning.InvalidRpc"), __instance?.Data?.PlayerName));
                        }
                    }
                    break;
                case CustomRPC.SyncCustomSettings:
                    foreach (var co in CustomOption.Options)
                    {
                        //すべてのカスタムオプションについてインデックス値で受信
                        co.Selection = reader.ReadInt32();
                    }
                    break;
                case CustomRPC.SetDeathReason:
                    RPC.GetDeathReason(reader);
                    break;
                case CustomRPC.EndGame:
                    RPC.EndGame(reader);
                    break;
                case CustomRPC.PlaySound:
                    byte playerID = reader.ReadByte();
                    Sounds sound = (Sounds)reader.ReadByte();
                    RPC.PlaySound(playerID, sound);
                    break;
                case CustomRPC.SetCustomRole:
                    byte CustomRoleTargetId = reader.ReadByte();
                    CustomRoles role = (CustomRoles)reader.ReadPackedInt32();
                    RPC.SetCustomRole(CustomRoleTargetId, role);
                    break;
                case CustomRPC.SetBountyTarget:
                    BountyHunter.ReceiveRPC(reader);
                    break;
                case CustomRPC.SetKillOrSpell:
                    byte playerId = reader.ReadByte();
                    bool KoS = reader.ReadBoolean();
                    Main.KillOrSpell[playerId] = KoS;
                    break;
                case CustomRPC.SetKillOrSilence:
                    byte playerIdd = reader.ReadByte();
                    bool KoSd = reader.ReadBoolean();
                    Main.KillOrSilence[playerIdd] = KoSd;
                    break;
                case CustomRPC.SetSheriffShotLimit:
                    Sheriff.ReceiveRPC(reader);
                    break;
                case CustomRPC.SetTimeThiefKillCount:
                    TimeThief.ReceiveRPC(reader);
                    break;
                case CustomRPC.SetDousedPlayer:
                    byte ArsonistId = reader.ReadByte();
                    byte DousedId = reader.ReadByte();
                    bool doused = reader.ReadBoolean();
                    Main.isDoused[(ArsonistId, DousedId)] = doused;
                    break;
                case CustomRPC.SetInfectedPlayer:
                    byte PbId = reader.ReadByte();
                    byte InfectId = reader.ReadByte();
                    bool infected = reader.ReadBoolean();
                    Main.isInfected[(PbId, InfectId)] = infected;
                    break;
                case CustomRPC.SetHexedPlayer:
                    byte hmId = reader.ReadByte();
                    byte hexId = reader.ReadByte();
                    bool hexed = reader.ReadBoolean();
                    Main.isHexed[(hmId, hexId)] = hexed;
                    break;
                case CustomRPC.AddNameColorData:
                    byte addSeerId = reader.ReadByte();
                    byte addTargetId = reader.ReadByte();
                    string color = reader.ReadString();
                    RPC.AddNameColorData(addSeerId, addTargetId, color);
                    break;
                case CustomRPC.RemoveNameColorData:
                    byte removeSeerId = reader.ReadByte();
                    byte removeTargetId = reader.ReadByte();
                    RPC.RemoveNameColorData(removeSeerId, removeTargetId);
                    break;
                case CustomRPC.ResetNameColorData:
                    RPC.ResetNameColorData();
                    break;
                case CustomRPC.DoSpell:
                    Main.SpelledPlayer.Add(Utils.GetPlayerById(reader.ReadByte()));
                    break;
                case CustomRPC.DoSilence:
                    Main.SilencedPlayer.Add(Utils.GetPlayerById(reader.ReadByte()));
                    break;
                case CustomRPC.SniperSync:
                    Sniper.ReceiveRPC(reader);
                    break;
                case CustomRPC.SetLoversPlayers:
                    Main.LoversPlayers.Clear();
                    int count = reader.ReadInt32();
                    for (int i = 0; i < count; i++)
                        Main.LoversPlayers.Add(Utils.GetPlayerById(reader.ReadByte()));
                    break;
                case CustomRPC.SetExecutionerTarget:
                    byte executionerId = reader.ReadByte();
                    byte targetId = reader.ReadByte();
                    Main.ExecutionerTarget[executionerId] = targetId;
                    break;
                case CustomRPC.RemoveExecutionerTarget:
                    byte Key = reader.ReadByte();
                    Main.ExecutionerTarget.Remove(Key);
                    break;
                case CustomRPC.SetGATarget:
                    byte gaId = reader.ReadByte();
                    byte targetIds = reader.ReadByte();
                    Main.GuardianAngelTarget[gaId] = targetIds;
                    break;
                case CustomRPC.RemoveGATarget:
                    byte Keys = reader.ReadByte();
                    Main.GuardianAngelTarget.Remove(Keys);
                    break;
                case CustomRPC.SendFireWorksState:
                    FireWorks.ReceiveRPC(reader);
                    break;
                case CustomRPC.SetCurrentDousingTarget:
                    byte arsonistId = reader.ReadByte();
                    byte dousingTargetId = reader.ReadByte();
                    if (PlayerControl.LocalPlayer.PlayerId == arsonistId)
                        Main.currentDousingTarget = dousingTargetId;
                    break;
                case CustomRPC.SetCurrentInfectingTarget:
                    byte pbId = reader.ReadByte();
                    byte infectingTargetId = reader.ReadByte();
                    if (PlayerControl.LocalPlayer.PlayerId == pbId)
                        Main.currentInfectingTarget = infectingTargetId;
                    break;
                case CustomRPC.SetCurrentHexingTarget:
                    break;
                case CustomRPC.ToggleCamouflagueActive:
                    Camouflague.IsActive = reader.ReadBoolean();
                    break;
                case CustomRPC.SetVeteranAlert: // DONE
                    int vet = reader.ReadInt32();
                    Main.VetAlerts = vet;
                    break;
                case CustomRPC.SetPirateProgress: // DONE
                    Guesser.PirateGuess[reader.ReadByte()] = reader.ReadInt32();
                    break;
                case CustomRPC.SetMedusaInfo: // DONE
                    Main.IsGazing = reader.ReadBoolean();
                    Main.GazeReady = reader.ReadBoolean();
                    break;
                case CustomRPC.SetHackerProgress: // DONE
                    byte hackerid = reader.ReadByte();
                    int hcount = reader.ReadInt32();
                    Main.HackerFixedSaboCount[hackerid] = hcount;
                    break;
                case CustomRPC.SeeredPlayer: // DONE
                    byte targetid = reader.ReadByte();
                    Investigator.hasSeered[targetid] = true;
                    break;
                case CustomRPC.SetVultureAmount: // DONE
                    Main.AteBodies = reader.ReadInt32();
                    break;
                case CustomRPC.UpdateGA: // DONE
                    Main.ProtectsSoFar = reader.ReadInt32();
                    break;
                case CustomRPC.NotifyDemoKill: // DONE
                    var remove = reader.ReadBoolean();
                    if (!remove)
                        Main.KilledDemo.Add(reader.ReadByte());
                    else
                        Main.KilledDemo.Remove(reader.ReadByte());
                    break;
                case CustomRPC.SendSurvivorInfo: // done
                    var survivor = reader.ReadByte();
                    var stuff = Main.SurvivorStuff[survivor];
                    stuff.Item1++;
                    Main.SurvivorStuff[survivor] = stuff;
                    break;
            }
        }
    }
    static class RPC
    {
        //SyncCustomSettingsRPC Sender
        public static void SyncCustomSettingsRPC()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, 80, Hazel.SendOption.Reliable, -1);
            foreach (var co in CustomOption.Options)
            {
                //すべてのカスタムオプションについてインデックス値で送信
                writer.Write(co.GetSelection());
            }
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void PlaySoundRPC(byte PlayerID, Sounds sound)
        {
            if (AmongUsClient.Instance.AmHost)
                RPC.PlaySound(PlayerID, sound);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.PlaySound, Hazel.SendOption.Reliable, -1);
            writer.Write(PlayerID);
            writer.Write((byte)sound);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void RpcSetRole(PlayerControl targetPlayer, PlayerControl sendTo, RoleTypes role)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(targetPlayer.NetId, (byte)RpcCalls.SetRole, Hazel.SendOption.Reliable, sendTo.GetClientId());
            writer.Write((byte)role);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void ExileAsync(PlayerControl player)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.Exiled, Hazel.SendOption.Reliable, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            player.Exiled();
        }
        public static async void RpcVersionCheck()
        {
            while (PlayerControl.LocalPlayer == null) await Task.Delay(500);
            MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.VersionCheck, SendOption.Reliable);
            writer.Write(Main.PluginVersion);
            writer.Write($"{ThisAssembly.Git.Commit}({ThisAssembly.Git.Branch})");
            writer.EndMessage();
            Main.playerVersion[PlayerControl.LocalPlayer.PlayerId] = new PlayerVersion(Main.PluginVersion, $"{ThisAssembly.Git.Commit}({ThisAssembly.Git.Branch})");
        }
        public static void SendDeathReason(byte playerId, PlayerState.DeathReason deathReason)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetDeathReason, Hazel.SendOption.Reliable, -1);
            writer.Write(playerId);
            writer.Write((int)deathReason);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void GetDeathReason(MessageReader reader)
        {
            var playerId = reader.ReadByte();
            var deathReason = (PlayerState.DeathReason)reader.ReadInt32();
            PlayerState.deathReasons[playerId] = deathReason;
            PlayerState.isDead[playerId] = true;
        }

        public static void EndGame(MessageReader reader)
        {
            try
            {
                List<byte> winner = new();
                Main.currentWinner = (CustomWinner)reader.ReadInt32();
                while (reader.BytesRemaining > 0) winner.Add(reader.ReadByte());
                switch (Main.currentWinner)
                {
                    case CustomWinner.Draw:
                        ForceEndGame();
                        break;
                    case CustomWinner.None:
                        EveryoneDied();
                        break;
                    case CustomWinner.Jester:
                        JesterExiled(winner[0]);
                        break;
                    case CustomWinner.Terrorist:
                        TerroristWin(winner[0]);
                        break;
                    case CustomWinner.Child:
                        ChildWin(winner[0]);
                        break;
                    case CustomWinner.Executioner:
                        ExecutionerWin(winner[0]);
                        break;
                    case CustomWinner.Hacker:
                        HackerWin(winner[0]);
                        break;
                    case CustomWinner.Arsonist:
                        SingleArsonistWin();
                        break;
                    case CustomWinner.Phantom:
                        PhantomWin(winner[0]);
                        break;
                    case CustomWinner.HASTroll:
                        TrollWin(winner[0]);
                        break;
                    case CustomWinner.Jackal:
                        JackalWin();
                        break;
                    case CustomWinner.Lovers:
                        LoversWin();
                        break;
                    case CustomWinner.Marksman:
                        MarksmanWin();
                        break;
                    case CustomWinner.Painter:
                        PainterWin();
                        break;
                    case CustomWinner.TheGlitch:
                        GlitchWin();
                        break;
                    case CustomWinner.Werewolf:
                        WolfWin();
                        break;
                    case CustomWinner.BloodKnight:
                        KnightWin();
                        break;
                    case CustomWinner.Pirate:
                        PirateWin(winner[0]);
                        break;
                    case CustomWinner.Vulture:
                        VultureWin();
                        break;
                    case CustomWinner.Juggernaut:
                        JugWin();
                        break;
                    case CustomWinner.Coven:
                        CovenWin();
                        break;
                    case CustomWinner.Pestilence:
                        PestiWin();
                        break;

                    default:
                        if (Main.currentWinner != CustomWinner.Default)
                            Logger.Warn($"{Main.currentWinner}は無効なCustomWinnerです", "EndGame");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"正常にEndGameを行えませんでした。{ex}", "EndGame");
            }
        }
        public static void TrollWin(byte trollID)
        {
            Main.WonTrollID = trollID;
            Main.currentWinner = CustomWinner.HASTroll;
            CustomWinTrigger(trollID);
        }
        public static void JesterExiled(byte jesterID)
        {
            Main.ExiledJesterID = jesterID;
            Main.currentWinner = CustomWinner.Jester;
            CustomWinTrigger(jesterID);
        }
        public static void TerroristWin(byte terroristID)
        {
            Main.WonTerroristID = terroristID;
            Main.currentWinner = CustomWinner.Terrorist;
            CustomWinTrigger(terroristID);
        }
        public static void ChildWin(byte childID)
        {
            Main.WonChildID = childID;
            Main.currentWinner = CustomWinner.Child;
            CustomWinTrigger(childID);
            if (ShipStatus.Instance == null) return;
            if (AmongUsClient.Instance.AmHost)
            {
                ShipStatus.Instance.enabled = false;
                Main.currentWinner = CustomWinner.Child;
                ShipStatus.RpcEndGame(GameOverReason.HumansByTask, false);
            }
        }
        public static void ExecutionerWin(byte executionerID)
        {
            Main.WonExecutionerID = executionerID;
            Main.currentWinner = CustomWinner.Executioner;
            CustomWinTrigger(executionerID);
        }
        public static void HackerWin(byte hackerID)
        {
            Main.WonHackerID = hackerID;
            Main.currentWinner = CustomWinner.Hacker;
            CustomWinTrigger(hackerID);
        }
        public static void FFAwin(byte ffaID)
        {
            Main.WonFFAid = ffaID;
            Main.currentWinner = CustomWinner.Jackal;
            CustomWinTrigger(ffaID);
            if (AmongUsClient.Instance.AmHost)
            {
                ShipStatus.Instance.enabled = false;
                Main.currentWinner = CustomWinner.Jackal;
                ShipStatus.RpcEndGame(GameOverReason.ImpostorByKill, false);
            }
        }
        public static void TeamFFAwin(int colorid)
        {
            Main.WonFFATeam = colorid;
            Main.WonFFAid = 254;
            Main.currentWinner = CustomWinner.Jackal;
            CustomWinTrigger(0);
            if (AmongUsClient.Instance.AmHost)
            {
                ShipStatus.Instance.enabled = false;
                Main.currentWinner = CustomWinner.Jackal;
                ShipStatus.RpcEndGame(GameOverReason.ImpostorByKill, false);
            }
        }
        public static void ArsonistWin(byte arsonistID)
        {
            Main.WonArsonistID = arsonistID;
            Main.currentWinner = CustomWinner.Arsonist;
            CustomWinTrigger(arsonistID);
        }
        public static void SingleArsonistWin()
        {
            // /Main.WonArsonistID = arsonistID;
            Main.currentWinner = CustomWinner.Arsonist;
            CustomWinTrigger(0);
            if (ShipStatus.Instance == null) return;
            if (AmongUsClient.Instance.AmHost)
            {
                ShipStatus.Instance.enabled = false;
                Main.currentWinner = CustomWinner.Arsonist;
                ShipStatus.RpcEndGame(GameOverReason.ImpostorByKill, false);
            }
        }
        public static void JackalWin()
        {
            Main.currentWinner = CustomWinner.Jackal;
            CustomWinTrigger(0);
        }
        public static void LoversWin()
        {
            Main.currentWinner = CustomWinner.Lovers;
            CustomWinTrigger(0);
        }
        public static void GlitchWin()
        {
            Main.currentWinner = CustomWinner.TheGlitch;
            CustomWinTrigger(0);
        }
        public static void PainterWin()
        {
            Main.currentWinner = CustomWinner.Painter;
            CustomWinTrigger(0);
        }
        public static void MarksmanWin()
        {
            Main.currentWinner = CustomWinner.Marksman;
            CustomWinTrigger(0);
        }
        public static void PhantomWin(byte phantomID)
        {
            Main.WonTrollID = phantomID;
            Main.currentWinner = CustomWinner.Phantom;
            CustomWinTrigger(0);
        }
        public static void WolfWin()
        {
            Main.currentWinner = CustomWinner.Werewolf;
            CustomWinTrigger(0);
        }
        public static void KnightWin()
        {
            Main.currentWinner = CustomWinner.BloodKnight;
            CustomWinTrigger(0);
            if (AmongUsClient.Instance.AmHost)
            {
                ShipStatus.Instance.enabled = false;
                Main.currentWinner = CustomWinner.BloodKnight;
                ShipStatus.RpcEndGame(GameOverReason.ImpostorByKill, false);
            }
        }
        public static void PirateWin(byte hackerID)
        {
            Main.WonPirateID = hackerID;
            Main.currentWinner = CustomWinner.Pirate;
            CustomWinTrigger(hackerID);
        }
        public static void JugWin()
        {
            Main.currentWinner = CustomWinner.Juggernaut;
            CustomWinTrigger(0);
        }
        public static void CovenWin()
        {
            Main.currentWinner = CustomWinner.Coven;
            CustomWinTrigger(0);
        }
        public static void VultureWin()
        {
            Main.currentWinner = CustomWinner.Vulture;
            CustomWinTrigger(0);
        }
        public static void PestiWin()
        {
            Main.currentWinner = CustomWinner.Pestilence;
            CustomWinTrigger(0);
        }
        public static void EveryoneDied()
        {
            Main.currentWinner = CustomWinner.None;
            CustomWinTrigger(0);
        }
        public static void ForceEndGame()
        {
            if (ShipStatus.Instance == null) return;
            Main.currentWinner = CustomWinner.Draw;
            if (AmongUsClient.Instance.AmHost)
            {
                ShipStatus.Instance.enabled = false;
                ShipStatus.RpcEndGame(GameOverReason.ImpostorByKill, false);
            }
        }
        public static void PlaySound(byte playerID, Sounds sound)
        {
            if (PlayerControl.LocalPlayer.PlayerId == playerID)
            {
                switch (sound)
                {
                    case Sounds.KillSound:
                        SoundManager.Instance.PlaySound(PlayerControl.LocalPlayer.KillSfx, false, 0.8f);
                        break;
                    case Sounds.TaskComplete:
                        SoundManager.Instance.PlaySound(DestroyableSingleton<HudManager>.Instance.TaskCompleteSound, false, 0.8f);
                        break;
                }
            }
        }
        public static void SetCustomRole(byte targetId, CustomRoles role)
        {
            if (role < CustomRoles.NoSubRoleAssigned)
            {
                Main.AllPlayerCustomRoles[targetId] = role;
            }
            else if (role >= CustomRoles.NoSubRoleAssigned)   //500:NoSubRole 501~:SubRole
            {
                Main.AllPlayerCustomSubRoles[targetId] = role;
            }
            switch (role)
            {
                case CustomRoles.FireWorks:
                    FireWorks.Add(targetId);
                    break;
                case CustomRoles.Sniper:
                    Sniper.Add(targetId);
                    break;
                case CustomRoles.Sheriff:
                    Sheriff.Add(targetId);
                    break;
                case CustomRoles.Investigator:
                    Investigator.Add(targetId);
                    break;
            }
            HudManager.Instance.SetHudActive(true);
        }
        public static void AddNameColorData(byte seerId, byte targetId, string color)
        {
            NameColorManager.Instance.Add(seerId, targetId, color);
        }
        public static void RemoveNameColorData(byte seerId, byte targetId)
        {
            NameColorManager.Instance.Remove(seerId, targetId);
        }
        public static void ResetNameColorData()
        {
            NameColorManager.Begin();
        }
        public static void RpcDoSpell(byte player)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DoSpell, Hazel.SendOption.Reliable, -1);
            writer.Write(player);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void RpcDoSilence(byte player)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.DoSilence, Hazel.SendOption.Reliable, -1);
            writer.Write(player);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void SyncLoversPlayers()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetLoversPlayers, Hazel.SendOption.Reliable, -1);
            writer.Write(Main.LoversPlayers.Count);
            foreach (var lp in Main.LoversPlayers)
            {
                writer.Write(lp.PlayerId);
            }
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void SendExecutionerTarget(byte executionerId, byte targetId)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetExecutionerTarget, Hazel.SendOption.Reliable, -1);
            writer.Write(executionerId);
            writer.Write(targetId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void SendGATarget(byte gaId, byte targetId)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetGATarget, Hazel.SendOption.Reliable, -1);
            writer.Write(gaId);
            writer.Write(targetId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void CustomWinTrigger(byte winnerID)
        {
            List<PlayerControl> Impostors = new();
            foreach (var p in PlayerControl.AllPlayerControls)
            {
                PlayerControl Winner = null;
                if (p.PlayerId == winnerID) Winner = p;
                if (p.Data.Role.IsImpostor)
                {
                    Impostors.Add(p);
                }
            }
            if (AmongUsClient.Instance.AmHost)
            {
                foreach (var imp in Impostors)
                {
                    imp.RpcSetRole(RoleTypes.GuardianAngel);
                }
                new LateTask(() => Main.CustomWinTrigger = true,
                0.2f, "Custom Win Trigger Task");
            }
        }
        public static void SendRpcLogger(uint targetNetId, byte callId, int targetClientId = -1)
        {
            if (!Main.AmDebugger.Value) return;
            string rpcName = GetRpcName(callId);
            string from = targetNetId.ToString();
            string target = targetClientId.ToString();
            try
            {
                target = targetClientId < 0 ? "All" : AmongUsClient.Instance.GetClient(targetClientId).PlayerName;
                from = PlayerControl.AllPlayerControls.ToArray().Where(c => c.NetId == targetNetId).FirstOrDefault()?.Data?.PlayerName;
            }
            catch { }
            Logger.Info($"FromNetID:{targetNetId}({from}) TargetClientID:{targetClientId}({target}) CallID:{callId}({rpcName})", "SendRPC");
        }
        public static string GetRpcName(byte callId)
        {
            string rpcName;
            if ((rpcName = Enum.GetName(typeof(RpcCalls), callId)) != null) { }
            else if ((rpcName = Enum.GetName(typeof(CustomRPC), callId)) != null) { }
            else rpcName = callId.ToString();
            return rpcName;
        }
        public static void RemoveExecutionerKey(byte Key)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RemoveExecutionerTarget, Hazel.SendOption.Reliable, -1);
            writer.Write(Key);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void RemoveGAKey(byte Key)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RemoveGATarget, Hazel.SendOption.Reliable, -1);
            writer.Write(Key);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void SetCurrentDousingTarget(byte arsonistId, byte targetId)
        {
            if (PlayerControl.LocalPlayer.PlayerId == arsonistId)
            {
                Main.currentDousingTarget = targetId;
            }
            else
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetCurrentDousingTarget, Hazel.SendOption.Reliable, -1);
                writer.Write(arsonistId);
                writer.Write(targetId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
        }
        public static void SetCurrentInfectingTarget(byte pbId, byte targetId)
        {
            if (PlayerControl.LocalPlayer.PlayerId == pbId)
            {
                Main.currentInfectingTarget = targetId;
            }
            else
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetCurrentInfectingTarget, Hazel.SendOption.Reliable, -1);
                writer.Write(pbId);
                writer.Write(targetId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
        }
        public static void ResetCurrentInfectingTarget(byte pbId) => SetCurrentInfectingTarget(pbId, 255);
        public static void ResetCurrentDousingTarget(byte arsonistId) => SetCurrentDousingTarget(arsonistId, 255);
    }
    [HarmonyPatch(typeof(InnerNet.InnerNetClient), nameof(InnerNet.InnerNetClient.StartRpc))]
    class StartRpcPatch
    {
        public static void Prefix(InnerNet.InnerNetClient __instance, [HarmonyArgument(0)] uint targetNetId, [HarmonyArgument(1)] byte callId)
        {
            RPC.SendRpcLogger(targetNetId, callId);
        }
    }
    [HarmonyPatch(typeof(InnerNet.InnerNetClient), nameof(InnerNet.InnerNetClient.StartRpcImmediately))]
    class StartRpcImmediatelyPatch
    {
        public static void Prefix(InnerNet.InnerNetClient __instance, [HarmonyArgument(0)] uint targetNetId, [HarmonyArgument(1)] byte callId, [HarmonyArgument(3)] int targetClientId = -1)
        {
            RPC.SendRpcLogger(targetNetId, callId, targetClientId);
        }
    }

}
