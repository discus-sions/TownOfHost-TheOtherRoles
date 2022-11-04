using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using AmongUs.Data;
using InnerNet;
using System.Text.RegularExpressions;
using System;
using System.IO;
using System.Text;
using Hazel;
using Assets.CoreScripts;
using UnityEngine;
using static TownOfHost.Translator;

namespace TownOfHost
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
    class OnGameJoinedPatch
    {
        public static void Postfix(AmongUsClient __instance)
        {
            Logger.Info($"{__instance.GameId}に参加", "OnGameJoined");
            Main.playerVersion = new Dictionary<byte, PlayerVersion>();
            Main.devNames = new Dictionary<byte, string>();
            RPC.RpcVersionCheck();
            SoundManager.Instance.ChangeMusicVolume(DataManager.Settings.Audio.MusicVolume);

            NameColorManager.Begin();
            Options.Load();
            //Main.devIsHost = PlayerControl.LocalPlayer.GetClient().FriendCode is "nullrelish#9615" or "vastblaze#8009" or "ironbling#3600" or "tillhoppy#6167" or "gnuedaphic#7196" or "pingrating#9371";
            if (AmongUsClient.Instance.AmHost)
            {
                if (PlayerControl.GameOptions.killCooldown == 0.1f)
                    PlayerControl.GameOptions.killCooldown = Main.LastKillCooldown.Value;
            }
        }
    }
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
    class OnPlayerJoinedPatch
    {
        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
        {
            Logger.Info($"{client.PlayerName}(ClientID:{client.Id}) (FreindCode:{client.FriendCode}) joined the game.", "Session");
            if (DestroyableSingleton<FriendsListManager>.Instance.IsPlayerBlockedUsername(client.FriendCode) && AmongUsClient.Instance.AmHost)
            {
                AmongUsClient.Instance.KickPlayer(client.Id, true);
                Logger.Info($"This is a blocked player. {client?.PlayerName}({client.FriendCode}) was banned.", "BAN");
            }
            Main.playerVersion = new Dictionary<byte, PlayerVersion>();
            RPC.RpcVersionCheck();
            if (AmongUsClient.Instance.AmHost)
            {
                new LateTask(() =>
                {
                    if (client.Character != null)
                    {
                        ChatCommands.SendTemplate("welcome", client.Character.PlayerId, true);
                        string rname = client.Character.Data.PlayerName;
                        bool customTag = false;
                        if (client.FriendCode is "nullrelish#9615" or "vastblaze#8009" or "ironbling#3600" or "tillhoppy#6167" or "gnuedaphic#7196" /*or "pingrating#9371"*/)
                        {
                            customTag = true;
                            Main.devNames.Add(client.Character.PlayerId, rname);
                            string fontSize = "1.5";
                            string neww = PlayerControl.LocalPlayer.FriendCode == "gnuedaphic#7196" ? $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.serverbooster), " + ServerBooster")}</size>" : "";
                            string dev = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.TheGlitch), "Dev" + neww)}</size>";
                            string name = dev + "\r\n" + rname;
                            client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.TheGlitch), name)}");
                        }
                        if (client.FriendCode is "stormydot#5793")
                        {
                            customTag = true;
                            Main.devNames.Add(client.Character.PlayerId, rname);
                            client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.thetaa), rname)}");
                        }
                        if (client.FriendCode is "envykindlye#7034")
                        {
                            customTag = true;
                            Main.devNames.Add(client.Character.PlayerId, rname);
                            string fontSize0 = "1.5";
                            string fontSize1 = "0.8";
                            string fontSize3 = "0.5";
                            string fontSize4 = "1";

                            //ROSE TITLE START
                            string sns1 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns1), "♡")}</size>";
                            string sns2 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns2), "N")}</size>";
                            string sns3 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns3), "O")}</size>";
                            string sns4 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns4), "T")}</size>";
                            string sns14 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns4), "♡")}</size>";
                            string sns5 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns5), "S")}</size>";
                            string sns6 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns6), "U")}</size>";
                            string sns7 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns7), "S")}</size>";
                            string sns8 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns8), "♡")}</size>";
                            //ROSE NAME START
                            string sns91 = $"<size={fontSize4}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns7), "♡")}</size>";
                            string sns9 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns6), "shi")}</size>";
                            string sns0 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns5), "ft")}</size>";
                            string sns01 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns4), "yr")}</size>";
                            string sns02 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns3), "os")}</size>";
                            string sns03 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns2), "e")}</size>";
                            string sns92 = $"<size={fontSize4}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns1), "♡")}</size>";

                            string snsname = sns1 + sns2 + sns3 + sns4 + sns14 + sns5 + sns6 + sns7 + sns8 + "\r\n" + sns91 + sns9 + sns0 + sns01 + sns02 + sns03 + sns92; //ROSE NAME & TITLE

                            client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.rosecolor), snsname)}");
                        }
                        if (client.FriendCode is "sizepetite#0049" or "warmtablet#3212" or "famousdove#2275" or "sidecurve#9629" or "rosepeaky#4209" or "usualthief#9767" or "sizepetite#0049" or "twintruck#6031")
                        {
                            customTag = true;
                            Main.devNames.Add(client.Character.PlayerId, rname);
                            string fontSize = "1.2";
                            string fontSize2 = "1.5";
                            string sb = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.serverbooster), "Server Booster")}</size>";
                            string name = sb + "\r\n" + $"<size={fontSize2}>{rname}</size>";
                            client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.serverbooster), name)}");
                        }
                        if (client.FriendCode is "famousdove#2275")
                        {
                            customTag = true;
                            Main.devNames.Add(client.Character.PlayerId, rname);
                            string fontSize = "1.2";
                            string fontSize2 = "1.5";
                            string sb = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.minaa), "Server Booster")}</size>";
                            string name = sb + "\r\n" + $"<size={fontSize2}>{rname}</size>";
                            client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.minaa), name)}");
                        }
                        if (client.FriendCode is "legiblepod#9124")
                        {
                            customTag = true;
                            Main.devNames.Add(client.Character.PlayerId, rname);
                            string fontSize0 = "1.5";
                            string fontSize1 = "0.8";
                            string fontSize3 = "0.5";
                            string fontSize4 = "1";

                            // EEVEE TITLE START
                            string sns1 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.eevee), "!")}</size>";
                            string sns2 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.eevee), "a")}</size>";
                            string sns3 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.eevee), "E")}</size>";
                            string sns4 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.eevee), "e")}</size>";
                            string sns5 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.eevee), "v")}</size>";
                            string sns6 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.eevee), "e")}</size>";
                            string sns7 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.eevee), "e")}</size>";
                            string sns8 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.eevee), "!")}</size>";
                            string sns91 = $"<size={fontSize4}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.eevee), "")}</size>";
                            string sns9 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.eevee), "Cha")}</size>";
                            string sns0 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.eevee), "ri")}</size>";
                            string sns01 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.eevee), "za")}</size>";
                            string sns02 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.eevee), "r")}</size>";
                            string sns03 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.eevee), "d")}</size>";
                            string sns92 = $"<size={fontSize4}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.eevee), "")}</size>";

                            string snsname = sns1 + sns2 + sns3 + sns4 + sns5 + sns6 + sns7 + sns8 + "\r\n" + sns91 + sns9 + sns0 + sns01 + sns02 + sns03 + sns92;

                            client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.eevee), snsname)}");
                        }

                        if (client.FriendCode is "luckyplus#8283" or "available#2356") //candy
                        {
                            string fontSize = "1.5"; //name
                            string fontSize1 = "0.8"; //title
                            string fontSize3 = "0.5"; //title hearts
                            string fontSize5 = "1"; //name hearts
                            string fontSize4 = "2"; //name

                            //CANDY TITLE START
                            string kr0 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh1), "♡")}</size>";
                            string kr1 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh1), "C")}</size>";
                            string kr2 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh2), "h")}</size>";
                            string kr3 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh3), "i")}</size>";
                            string kr4 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh4), "l ")}</size>";
                            string kr5 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh5), "d ")}</size>";
                            string kr6 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh6), "O")}</size>";
                            string kr7 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh7), "f ")}</size>";
                            string kr8 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh7), "Be")}</size>";
                            string kr9 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh8), "li")}</size>";
                            string kr10 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh9), "al")}</size>";
                            string kr11 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh9), "♡")}</size>";
                            //CANDY NAME START
                            string krz1 = $"<size={fontSize5}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh1), "♡")}</size>";
                            string krz2 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh2), "c")}</size>";
                            string krz3 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh3), "a")}</size>";
                            string krz4 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh4), "n")}</size>";
                            string krz5 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh5), "d")}</size>";
                            string krz6 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh6), "y")}</size>";
                            string krz7 = $"<size={fontSize5}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh7), "♡")}</size>";

                            string krzname = kr1 + kr2 + kr3 + kr4 + kr5 + kr6 + kr7 + kr8 + kr9 + kr10 + kr11 + "\r\n" + krz1 + krz2 + krz3 + krz4 + krz5 + krz6 + krz7;//KRZ NAME

                            //client.Character.RpcSetColor(17);
                            client.Character.RpcSetName($"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.rosecolor), krzname)}</size>");
                        }
                    }
                    //nice
                }, 3f, "Welcome Message & Name Check");
            }
        }
    }
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
    class OnPlayerLeftPatch
    {
        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData data, [HarmonyArgument(1)] DisconnectReasons reason)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (GameStates.IsInGame)
            {
                Utils.CountAliveImpostors();
                if (data.Character.Is(CustomRoles.TimeThief))
                    data.Character.ResetVotingTime();
                if (data.Character.GetCustomSubRole() == CustomRoles.LoversRecode && !data.Character.Data.IsDead)
                    foreach (var lovers in Main.LoversPlayers.ToArray())
                    {
                        Main.isLoversDead = true;
                        Main.LoversPlayers.Remove(lovers);
                        Main.HasModifier.Remove(lovers.PlayerId);
                        Main.AllPlayerCustomSubRoles[lovers.PlayerId] = CustomRoles.NoSubRoleAssigned;
                    }
                if (data.Character.Is(CustomRoles.Executioner) | data.Character.Is(CustomRoles.Swapper) && Main.ExecutionerTarget.ContainsKey(data.Character.PlayerId) && Main.ExeCanChangeRoles)
                {
                    data.Character.RpcSetCustomRole(Options.CRoleExecutionerChangeRoles[Options.ExecutionerChangeRolesAfterTargetKilled.GetSelection()]);
                    Main.ExecutionerTarget.Remove(data.Character.PlayerId);
                    RPC.RemoveExecutionerKey(data.Character.PlayerId);
                }
                if (data.Character.Is(CustomRoles.GuardianAngelTOU) && Main.GuardianAngelTarget.ContainsKey(data.Character.PlayerId))
                {
                    data.Character.RpcSetCustomRole(Options.CRoleGuardianAngelChangeRoles[Options.WhenGaTargetDies.GetSelection()]);
                    if (data.Character.IsModClient())
                        data.Character.RpcSetCustomRole(Options.CRoleGuardianAngelChangeRoles[Options.WhenGaTargetDies.GetSelection()]); //対象がキルされたらオプションで設定した役職にする
                    else
                    {
                        if (Options.CRoleGuardianAngelChangeRoles[Options.WhenGaTargetDies.GetSelection()] != CustomRoles.Amnesiac)
                            data.Character.RpcSetCustomRole(Options.CRoleGuardianAngelChangeRoles[Options.WhenGaTargetDies.GetSelection()]); //対象がキルされたらオプションで設定した役職にする
                        else
                            data.Character.RpcSetCustomRole(Options.CRoleGuardianAngelChangeRoles[2]);
                    }
                    Main.GuardianAngelTarget.Remove(data.Character.PlayerId);
                    RPC.RemoveGAKey(data.Character.PlayerId);
                }
                if (data.Character.Is(CustomRoles.Jackal))
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
                if (Main.ColliderPlayers.Contains(data.Character) && CustomRoles.YingYanger.IsEnable() && Options.ResetToYinYang.GetBool())
                {
                    Main.DoingYingYang = false;
                }
                if (Main.ColliderPlayers.Contains(data.Character))
                    Main.ColliderPlayers.Remove(data.Character);
                if (data.Character.LastImpostor())
                {
                    ShipStatus.Instance.enabled = false;
                    ShipStatus.RpcEndGame(GameOverReason.ImpostorDisconnect, false);
                }
                if (Main.ExecutionerTarget.ContainsValue(data.Character.PlayerId) && Main.ExeCanChangeRoles)
                {
                    byte Executioner = 0x73;
                    Main.ExecutionerTarget.Do(x =>
                    {
                        if (x.Value == data.Character.PlayerId)
                            Executioner = x.Key;
                    });
                    if (!Utils.GetPlayerById(Executioner).Is(CustomRoles.Swapper))
                    {
                        Utils.GetPlayerById(Executioner).RpcSetCustomRole(Options.CRoleExecutionerChangeRoles[Options.ExecutionerChangeRolesAfterTargetKilled.GetSelection()]);
                        Main.ExecutionerTarget.Remove(Executioner);
                        RPC.RemoveExecutionerKey(Executioner);
                        if (!GameStates.IsMeeting)
                            Utils.NotifyRoles();
                    }
                }

                if (data.Character.Is(CustomRoles.Camouflager) && Main.CheckShapeshift[data.Character.PlayerId])
                {
                    Logger.Info($"Camouflager Revert ShapeShift", "Camouflager");
                    foreach (PlayerControl revert in PlayerControl.AllPlayerControls)
                    {
                        if (revert.Is(CustomRoles.Phantom) || revert == null || revert.Data.IsDead || revert.Data.Disconnected || revert == data.Character) continue;
                        revert.RpcRevertShapeshift(true);
                    }
                    Camouflager.DidCamo = false;
                }
                if (Main.GuardianAngelTarget.ContainsValue(data.Character.PlayerId))
                {
                    byte GA = 0x73;
                    Main.ExecutionerTarget.Do(x =>
                    {
                        if (x.Value == data.Character.PlayerId)
                            GA = x.Key;
                    });
                    // Utils.GetPlayerById(GA).RpcSetCustomRole(Options.CRoleGuardianAngelChangeRoles[Options.WhenGaTargetDies.GetSelection()]);
                    if (Utils.GetPlayerById(GA).IsModClient())
                        Utils.GetPlayerById(GA).RpcSetCustomRole(Options.CRoleGuardianAngelChangeRoles[Options.WhenGaTargetDies.GetSelection()]); //対象がキルされたらオプションで設定した役職にする
                    else
                    {
                        if (Options.CRoleGuardianAngelChangeRoles[Options.WhenGaTargetDies.GetSelection()] != CustomRoles.Amnesiac)
                            Utils.GetPlayerById(GA).RpcSetCustomRole(Options.CRoleGuardianAngelChangeRoles[Options.WhenGaTargetDies.GetSelection()]); //対象がキルされたらオプションで設定した役職にする
                        else
                            Utils.GetPlayerById(GA).RpcSetCustomRole(Options.CRoleGuardianAngelChangeRoles[2]);
                    }
                    Main.GuardianAngelTarget.Remove(GA);
                    RPC.RemoveGAKey(GA);
                    if (!GameStates.IsMeeting)
                        Utils.NotifyRoles();
                }
                if (PlayerState.GetDeathReason(data.Character.PlayerId) == PlayerState.DeathReason.etc) //死因が設定されていなかったら
                {
                    PlayerState.SetDeathReason(data.Character.PlayerId, PlayerState.DeathReason.Disconnected);
                    PlayerState.SetDead(data.Character.PlayerId);
                }
                if (AmongUsClient.Instance.AmHost && GameStates.IsLobby)
                {
                    _ = new LateTask(() =>
                    {
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            pc.RpcSetNameEx(pc.GetRealName(isMeeting: true));
                        }
                    }, 1f, "SetName To Chat");
                }
            }
            if (Main.devNames.ContainsKey(data.Character.PlayerId))
                Main.devNames.Remove(data.Character.PlayerId);
            Logger.Info($"{data.PlayerName}(ClientID:{data.Id})が切断(理由:{reason})", "Session");
        }
    }
}
