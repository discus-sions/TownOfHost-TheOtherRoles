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
                if (GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown == 0.1f)
                    GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown = Main.LastKillCooldown.Value;
            }
            if (AmongUsClient.Instance.AmHost)
            {
                new LateTask(() =>
                {
                    if (PlayerControl.LocalPlayer != null)
                    {
                        bool customTag = false;
                        string rname = PlayerControl.LocalPlayer.Data.PlayerName;
                        if (PlayerControl.LocalPlayer.FriendCode is "nullrelish#9615" or "pingrating#9371")
                        {
                            //    customTag = true;
                            string rtag = "type:sforce\ncode:name\ncolor:#00A700\ntoptext:<color=#00A700><size=1.0>【</size>D</color><color=#00B800>E</color><color=#00CC00>V</color><color=#00E000>E</color><color=#2BF32B>L</color><color=#1FFF1F>O</color><color=#33FF33>P</color><color=#46FF46>E</color><color=#57FF57>R<size=1.0>】</size></color>\nname:<color=#57FF57><size=1.1>《</size>Di</color><color=#46FF46>s</color><color=#33FF33>c</color><color=#1FFF1F>u</color><color=#2BF32B>s</color><color=#00E000>s</color><color=#00CC00>i</color><color=#00B800>o</color><color=#00A700>ns<size=1.1>》</size></color>";
                            List<string> response = CustomTags.ReturnTagInfoFromString(rtag);
                            Main.devNames.Add(PlayerControl.LocalPlayer.PlayerId, rname);
                            string fontSizee = "1.2";
                            string fontSizee2 = "1.5";
                            string tag = $"<size={fontSizee}>{Helpers.ColorString(Utils.GetHexColor(response[1]), $"{response[2]}")}</size>";
                            string realname = tag + "\r\n" + $"<size={fontSizee2}>{response[3]}</size>";
                            PlayerControl.LocalPlayer.RpcSetName($"{Helpers.ColorString(Utils.GetHexColor(response[1]), realname)}");
                        }
                        if (PlayerControl.LocalPlayer.FriendCode is "tillhoppy#6167")
                        {
                            //    customTag = true;
                            string rtag = "type:sforce\ncode:stuff\ncolor:#00A700\ntoptext:<color=#00A700>D</color><color=#00B800>E</color><color=#00CC00>V</color><color=#00E000>E</color><color=#2BF32B>L</color><color=#1FFF1F>O</color><color=#33FF33>P</color><color=#46FF46>E</color><color=#57FF57>R</color>\nname:<color=#57FF57>D</color><color=#46FF46>e</color><color=#33FF33>t</color><color=#1FFF1F>e</color><color=#2BF32B>c</color><color=#00E000>t</color><color=#00CC00>i</color><color=#00B800>v</color><color=#00A700>e</color>";
                            List<string> response = CustomTags.ReturnTagInfoFromString(rtag);
                            Main.devNames.Add(PlayerControl.LocalPlayer.PlayerId, rname);
                            string fontSizee = "1.2";
                            string fontSizee2 = "1.5";
                            string tag = $"<size={fontSizee}>{Helpers.ColorString(Utils.GetHexColor(response[1]), $"{response[2]}")}</size>";
                            string realname = tag + "\r\n" + $"<size={fontSizee2}>{response[3]}</size>";
                            PlayerControl.LocalPlayer.RpcSetName($"{Helpers.ColorString(Utils.GetHexColor(response[1]), realname)}");
                        }
                        if (PlayerControl.LocalPlayer.FriendCode is /*"minishelf#4561" or*/ "onionforty#4833" or "pagersane#4064" or "coralcode#0731" or "basketsane#0222" or "gameshrimp#5509" or "irongrace#5957" or "rosecoven#1786" or "sizepetite#0049" or "casualclod#9221" or "warmtablet#3212" or "sidecurvee#9629" or "usualthief#9767" or "sizepetite#0049" or "twintruck#6031")
                        {
                            //    customTag = true;
                            Main.devNames.Add(PlayerControl.LocalPlayer.PlayerId, rname);
                            string fontSize = "1.2";
                            string fontSize2 = "1.5";
                            string sb = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.serverbooster), "Server Booster")}</size>";
                            string name = sb + "\r\n" + $"<size={fontSize2}>{rname}</size>";
                            PlayerControl.LocalPlayer.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.serverbooster), name)}");
                        }
                        if (PlayerControl.LocalPlayer.FriendCode is "famousdove#2275")
                        {
                            customTag = true;
                            Main.devNames.Add(PlayerControl.LocalPlayer.PlayerId, rname);
                            string fontSize = "1.2";
                            string fontSize2 = "1.5";
                            string sb = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.minaa), "Server Booster")}</size>";
                            string name = sb + "\r\n" + $"<size={fontSize2}>{rname}</size>";
                            PlayerControl.LocalPlayer.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.minaa), name)}");
                        }
                        if (PlayerControl.LocalPlayer.FriendCode is "rosepeaky#4209")
                        {
                            customTag = true;
                            string rtag = "type:sforce\ncode:rosepeaky#4209\ncolor:#5E99EC\ntoptext:<color=#5E99EC><size=0.9>☆</size>E</color><color=#7F9FF1>S</color><color=#9FA5F6>S</color><color=#E0B0FF><size=0.9>☆</size>B</color><color=#D786EB>E</color><color=#CE5CD7>A</color><color=#BB07AE>N<size=0.9>☆</size></color>\nname:<color=#BB07AE><size=1.1>☆</size>e</color><color=#CE5CD7>s</color><color=#D786EB>s</color><color=#E0B0FF>e</color><color=#9FA5F6>n</color><color=#7F9FF1>c</color><color=#5E99EC>e<size=1.1>☆</size></color>";
                            List<string> response = CustomTags.ReturnTagInfoFromString(rtag);
                            Main.devNames.Add(PlayerControl.LocalPlayer.PlayerId, rname);
                            string fontSizee = "1.2";
                            string fontSizee2 = "1.5";
                            string tag = $"<size={fontSizee}>{Helpers.ColorString(Utils.GetHexColor(response[1]), $"{response[2]}")}</size>";
                            string realname = tag + "\r\n" + $"<size={fontSizee2}>{response[3]}</size>";
                            PlayerControl.LocalPlayer.RpcSetName($"{Helpers.ColorString(Utils.GetHexColor(response[1]), realname)}");
                        }
                        if (PlayerControl.LocalPlayer.FriendCode is "stormydott#5793") // THETAA
                        {
                            //    customTag = true;
                            Main.devNames.Add(PlayerControl.LocalPlayer.PlayerId, rname);
                            string fontSize = "1.2";
                            string fontSize2 = "1.5";
                            string sb = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.thetaa), "Server Booster")}</size>";
                            string name = sb + "\r\n" + $"<size={fontSize2}>{rname}</size>";
                            PlayerControl.LocalPlayer.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.thetaa), name)}");
                        }
                        if (PlayerControl.LocalPlayer.FriendCode is "envykindly#7034")
                        {
                            //   customTag = true;
                            Main.devNames.Add(PlayerControl.LocalPlayer.PlayerId, rname);
                            string fontSize0 = "1.2";
                            string fontSize1 = "0.5";
                            string fontSize3 = "0.8";
                            string fontSize4 = "1";

                            //ROSE TITLE START
                            string sns1 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns10), "♡")}</size>";
                            string sns2 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns9), "T")}</size>";
                            string sns3 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns8), "H")}</size>";
                            string sns4 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns7), "E")}</size>";
                            string sns14 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns6), "♡")}</size>";
                            string sns5 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns6), "A")}</size>";
                            string sns6 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns5), "M")}</size>";
                            string sns7 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns4), "A")}</size>";
                            string shi1 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns4), "Z")}</size>";
                            string shi2 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns3), "I")}</size>";
                            string shi3 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns2), "N")}</size>";
                            string shi4 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns1), "G")}</size>";
                            string sns8 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns1), "♡")}</size>";
                            //ROSE NAME START
                            string sns91 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns1), "♡")}</size>";
                            string sns9 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns1), "S")}</size>";
                            string sns0 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns2), "h")}</size>";
                            string sns01 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns3), "i")}</size>";
                            string sns02 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns4), "f")}</size>";
                            string sns03 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns4), "t")}</size>";
                            string sns11 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns5), "y")}</size>";
                            string sns12 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns6), "R")}</size>";
                            string sns13 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns7), "o")}</size>";
                            string sns16 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns8), "s")}</size>";
                            string sns15 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns9), "e")}</size>";
                            string sns92 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns10), "♡")}</size>";
                            //client.Character.RpcSetColor(13);
                            string snsname = sns1 + sns2 + sns3 + sns4 + sns14 + sns5 + sns6 + sns7 + shi1 + shi2 + shi3 + shi4 + sns8 + "\r\n" + sns91 + sns9 + sns0 + sns01 + sns02 + sns03 + sns11 + sns12 + sns13 + sns16 + sns15 + sns92; //ROSE NAME & TITLE

                            PlayerControl.LocalPlayer.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.rosecolor), snsname)}");
                        }
                        if (PlayerControl.LocalPlayer.FriendCode is "legiblepod#9124")
                        {
                            //   customTag = true;
                            Main.devNames.Add(PlayerControl.LocalPlayer.PlayerId, rname);
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

                            PlayerControl.LocalPlayer.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.eevee), snsname)}");
                        }
                        if (PlayerControl.LocalPlayer.FriendCode is "luckyplus#8283" or "available#2356") //candy
                        {
                            //    customTag = true;
                            Main.devNames.Add(PlayerControl.LocalPlayer.PlayerId, rname);
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
                            string kr4 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh4), "l")}</size>";
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

                            string krzname = kr0 + kr1 + kr2 + kr3 + kr4 + kr5 + kr6 + kr7 + kr8 + kr9 + kr10 + kr11 + "\r\n" + krz1 + krz2 + krz3 + krz4 + krz5 + krz6 + krz7;//KRZ NAME

                            //PlayerControl.LocalPlayer.RpcSetColor(17);
                            PlayerControl.LocalPlayer.RpcSetName($"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.rosecolor), krzname)}</size>");
                        }
                        if (!customTag && AmongUsClient.Instance.AmHost)
                            if (File.Exists(CustomTags.GetFilePath(PlayerControl.LocalPlayer.FriendCode)))
                            {
                                List<string> response = CustomTags.ReturnTagInfo(PlayerControl.LocalPlayer.FriendCode);
                                switch (response[0])
                                {
                                    case "sforce":
                                        customTag = true;
                                        Main.devNames.Add(PlayerControl.LocalPlayer.PlayerId, rname);
                                        string fontSizee = "1.2";
                                        string fontSizee2 = "1.5";
                                        string tag = $"<size={fontSizee}>{Helpers.ColorString(Utils.GetHexColor(response[1]), $"{response[2]}" /*+ " (Custom)"*/)}</size>";
                                        string realname = tag + "\r\n" + $"<size={fontSizee2}>{response[3]}</size>";
                                        PlayerControl.LocalPlayer.RpcSetName($"{Helpers.ColorString(Utils.GetHexColor(response[1]), realname)}");
                                        break;
                                    case "static":
                                        customTag = true;
                                        Main.devNames.Add(PlayerControl.LocalPlayer.PlayerId, rname);
                                        string fontSize = "1.2";
                                        string fontSize2 = "1.5";
                                        string sb = $"<size={fontSize}>{Helpers.ColorString(Utils.GetHexColor(response[1]), $"{response[2]}")}</size>";
                                        string name = sb + "\r\n" + $"<size={fontSize2}>{rname}</size>";
                                        PlayerControl.LocalPlayer.RpcSetName($"{Helpers.ColorString(Utils.GetHexColor(response[1]), name)}");
                                        break;
                                    default:
                                    case "gradient":
                                        break;
                                }
                            }
                    }
                    //nice
                }, 3f, "Welcome Message & Name Check");
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
            if (client.FriendCode is "nullrelish#9615" or "tillhoppy#6167" or "pingrating#9371") { }
            else
            {
                var list = ChatCommands.ReturnAllNewLinesInFile(Main.BANNEDFRIENDCODES_FILE_PATH, noErr: true);
                if (list.Contains(client.FriendCode) && AmongUsClient.Instance.AmHost)
                {
                    AmongUsClient.Instance.KickPlayer(client.Id, true);
                    Logger.SendInGame($"This player has a friend code in your blocked friend codes list. {client?.PlayerName}({client.FriendCode}) was banned.");
                    Logger.Msg($"This player has a friend code in your blocked friend codes list. {client?.PlayerName}({client.FriendCode}) was banned.", "BAN");
                }
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
                        if (client.FriendCode is "nullrelish#9615" or "pingrating#9371")
                        {
                            //    customTag = true;
                            string rtag = "type:sforce\ncode:name\ncolor:#00A700\ntoptext:<color=#00A700><size=1.0>【</size>D</color><color=#00B800>E</color><color=#00CC00>V</color><color=#00E000>E</color><color=#2BF32B>L</color><color=#1FFF1F>O</color><color=#33FF33>P</color><color=#46FF46>E</color><color=#57FF57>R<size=1.0>】</size></color>\nname:<color=#57FF57><size=1.1>《</size>Di</color><color=#46FF46>s</color><color=#33FF33>c</color><color=#1FFF1F>u</color><color=#2BF32B>s</color><color=#00E000>s</color><color=#00CC00>i</color><color=#00B800>o</color><color=#00A700>ns<size=1.1>》</size></color>";
                            List<string> response = CustomTags.ReturnTagInfoFromString(rtag);
                            Main.devNames.Add(client.Character.PlayerId, rname);
                            string fontSizee = "1.2";
                            string fontSizee2 = "1.5";
                            string tag = $"<size={fontSizee}>{Helpers.ColorString(Utils.GetHexColor(response[1]), $"{response[2]}")}</size>";
                            string realname = tag + "\r\n" + $"<size={fontSizee2}>{response[3]}</size>";
                            client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetHexColor(response[1]), realname)}");
                        }
                        if (client.FriendCode is "tillhoppy#6167")
                        {
                            //    customTag = true;
                            string rtag = "type:sforce\ncode:stuff\ncolor:#00A700\ntoptext:<color=#00A700>D</color><color=#00B800>E</color><color=#00CC00>V</color><color=#00E000>E</color><color=#2BF32B>L</color><color=#1FFF1F>O</color><color=#33FF33>P</color><color=#46FF46>E</color><color=#57FF57>R</color>\nname:<color=#57FF57>D</color><color=#46FF46>e</color><color=#33FF33>t</color><color=#1FFF1F>e</color><color=#2BF32B>c</color><color=#00E000>t</color><color=#00CC00>i</color><color=#00B800>v</color><color=#00A700>e</color>";
                            List<string> response = CustomTags.ReturnTagInfoFromString(rtag);
                            Main.devNames.Add(client.Character.PlayerId, rname);
                            string fontSizee = "1.2";
                            string fontSizee2 = "1.5";
                            string tag = $"<size={fontSizee}>{Helpers.ColorString(Utils.GetHexColor(response[1]), $"{response[2]}")}</size>";
                            string realname = tag + "\r\n" + $"<size={fontSizee2}>{response[3]}</size>";
                            client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetHexColor(response[1]), realname)}");
                        }
                        if (client.FriendCode is "stormydott#5793") // THETAA
                        {
                            customTag = true;
                            Main.devNames.Add(client.Character.PlayerId, rname);
                            string fontSize = "1.2";
                            string fontSize2 = "1.5";
                            string sb = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.thetaa), "Server Booster")}</size>";
                            string name = sb + "\r\n" + $"<size={fontSize2}>{rname}</size>";
                            client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.thetaa), name)}");
                        }
                        if (client.FriendCode is "envykindly#7034")
                        {
                            customTag = true;
                            Main.devNames.Add(client.Character.PlayerId, rname);
                            string fontSize0 = "1.2";
                            string fontSize1 = "0.5";
                            string fontSize3 = "0.8";
                            string fontSize4 = "1";

                            //ROSE TITLE START
                            string sns1 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns10), "♡")}</size>";
                            string sns2 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns9), "T")}</size>";
                            string sns3 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns8), "H")}</size>";
                            string sns4 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns7), "E")}</size>";
                            string sns14 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns6), "♡")}</size>";
                            string sns5 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns6), "A")}</size>";
                            string sns6 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns5), "M")}</size>";
                            string sns7 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns4), "A")}</size>";
                            string shi1 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns4), "Z")}</size>";
                            string shi2 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns3), "I")}</size>";
                            string shi3 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns2), "N")}</size>";
                            string shi4 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns1), "G")}</size>";
                            string sns8 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns1), "♡")}</size>";
                            //ROSE NAME START
                            string sns91 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns1), "♡")}</size>";
                            string sns9 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns1), "S")}</size>";
                            string sns0 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns2), "h")}</size>";
                            string sns01 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns3), "i")}</size>";
                            string sns02 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns4), "f")}</size>";
                            string sns03 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns4), "t")}</size>";
                            string sns11 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns5), "y")}</size>";
                            string sns12 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns6), "R")}</size>";
                            string sns13 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns7), "o")}</size>";
                            string sns16 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns8), "s")}</size>";
                            string sns15 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns9), "e")}</size>";
                            string sns92 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns10), "♡")}</size>";
                            //client.Character.RpcSetColor(13);
                            string snsname = sns1 + sns2 + sns3 + sns4 + sns14 + sns5 + sns6 + sns7 + shi1 + shi2 + shi3 + shi4 + sns8 + "\r\n" + sns91 + sns9 + sns0 + sns01 + sns02 + sns03 + sns11 + sns12 + sns13 + sns16 + sns15 + sns92; //ROSE NAME & TITLE

                            client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.rosecolor), snsname)}");
                            Main.devNames.Add(client.Character.PlayerId, sns1 + sns2 + sns3 + sns4 + sns14 + sns5 + sns6 + sns7 + shi1 + shi2 + shi3 + shi4 + sns8 + "\r\n" + $"<size={fontSize0}>{rname}</size>");
                        }
                        if (client.FriendCode is /*"minishelf#4561" or*/ "onionforty#4833" or "pagersane#4064" or "coralcode#0731" or "basketsane#0222" or "gameshrimp#5509" or "irongrace#5957" or "rosecoven#1786" or "sizepetite#0049" or "casualclod#9221" or "warmtablet#3212" or "sidecurvee#9629" or "usualthief#9767" or "sizepetite#0049" or "twintruck#6031")
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
                        if (client.FriendCode is "rosepeaky#4209")
                        {
                            customTag = true;
                            string rtag = "type:sforce\ncode:rosepeaky#4209\ncolor:#5E99EC\ntoptext:<color=#5E99EC><size=0.9>☆</size>E</color><color=#7F9FF1>S</color><color=#9FA5F6>S</color><color=#E0B0FF><size=0.9>☆</size>B</color><color=#D786EB>E</color><color=#CE5CD7>A</color><color=#BB07AE>N<size=0.9>☆</size></color>\nname:<color=#BB07AE><size=1.1>☆</size>e</color><color=#CE5CD7>s</color><color=#D786EB>s</color><color=#E0B0FF>e</color><color=#9FA5F6>n</color><color=#7F9FF1>c</color><color=#5E99EC>e<size=1.1>☆</size></color>";
                            List<string> response = CustomTags.ReturnTagInfoFromString(rtag);
                            Main.devNames.Add(client.Character.PlayerId, rname);
                            string fontSizee = "1.2";
                            string fontSizee2 = "1.5";
                            string tag = $"<size={fontSizee}>{Helpers.ColorString(Utils.GetHexColor(response[1]), $"{response[2]}")}</size>";
                            string realname = tag + "\r\n" + $"<size={fontSizee2}>{response[3]}</size>";
                            client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetHexColor(response[1]), realname)}");
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
                            customTag = true;
                            Main.devNames.Add(client.Character.PlayerId, rname);
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
                            string kr4 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh4), "l")}</size>";
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

                            string krzname = kr0 + kr1 + kr2 + kr3 + kr4 + kr5 + kr6 + kr7 + kr8 + kr9 + kr10 + kr11 + "\r\n" + krz1 + krz2 + krz3 + krz4 + krz5 + krz6 + krz7;//KRZ NAME

                            //client.Character.RpcSetColor(17);
                            client.Character.RpcSetName($"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.rosecolor), krzname)}</size>");
                        }
                        //ck
                        if (client.FriendCode is "totalstork#2439") //feyre
                        {
                            string fontSize0 = "1.3";
                            string fontSize1 = "0.5";
                            string fontSize3 = "0.8";
                            string fontSize4 = "1";

                            //ROSE TITLE START
                            string ben01 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cc1), "T")}</size>";
                            string ben1 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cc2), "h")}</size>";
                            string ben2 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cc3), "e ")}</size>";
                            string ben3 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cc4), "R")}</size>";
                            string ben5 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cc5), "e")}</size>";
                            string ben6 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cc6), "d ")}</size>";
                            string ben7 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cc7), "M")}</size>";
                            string ben8 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cc8), "a")}</size>";
                            string ben9 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cc9), "n")}</size>";

                            //ROSE NAME STAR
                            string ben11 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cc10), "☆")}</size>";
                            string ben12 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cc8), "C")}</size>";
                            string ben13 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cc6), "K")}</size>";
                            string ben14 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cc4), "☆ ")}</size>";


                            string snsname = ben01 + ben1 + ben2 + ben3 + ben5 + ben6 + ben7 + ben8 + ben9 + "\r\n" + ben11 + ben12 + ben13 + ben14; //ROSE NAME & TITLE

                            client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.rosecolor), snsname)}");
                        }
                        if (client.FriendCode is "neatnet#5851") //Gurge44    beespotty#5432
                        {
                            customTag = true;
                            string fontSize0 = "1.3";
                            string fontSize1 = "0.5";
                            string fontSize3 = "0.8";
                            string fontSize4 = "1";

                            //WINNER1 TITLE START
                            string win01 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.gu1), "Th")}</size>";
                            string win1 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.gu2), "e ")}</size>";
                            string win2 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.gu3), "2")}</size>";
                            string win3 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.gu4), "0")}</size>";
                            string win5 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.gu5), "0")}</size>";
                            string win6 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.gu6), "I")}</size>";
                            string win7 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.gu7), "Q ")}</size>";
                            string win8 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.gu8), "G")}</size>";
                            string win9 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.gu9), "u")}</size>";
                            string win10 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.gu10), "y")}</size>";
                            //WINNER1 NAME STAR
                            string win11 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.gu10), "D")}</size>";
                            string win12 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.gu8), "e")}</size>";
                            string win13 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.gu6), "b")}</size>";
                            string win14 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.gu4), "re")}</size>";
                            string win15 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.gu3), "c")}</size>";
                            string win16 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.gu2), "e")}</size>";
                            string win17 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.gu1), "n")}</size>";
                            string snsname = win01 + win1 + win2 + win3 + win5 + win6 + win7 + win8 + win9 + win10 + "\r\n" + win11 + win12 + win13 + win14 + win15 + win16 + win17; //WINNER1 NAME & TITLE

                            client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.rosecolor), snsname)}");
                            Main.devNames.Add(client.Character.PlayerId, rname);
                        }
                        if (client.FriendCode is "soulfulfax#2735") //yoclobo   beespotty#5432
                        {
                            customTag = true;
                            string fontSize0 = "1.3";
                            string fontSize1 = "0.5";
                            string fontSize3 = "0.8";
                            string fontSize4 = "1";

                            //WINNER2 TITLE START
                            string win01 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.yo1), "☆")}</size>";
                            string win1 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.yo2), "C")}</size>";
                            string win3 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.yo4), "I")}</size>";
                            string win5 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.yo5), "T")}</size>";
                            string win6 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.yo6), "RI")}</size>";
                            string win7 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.yo7), "N")}</size>";
                            string win8 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.yo8), "E")}</size>";
                            string win10 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.yo10), "☆")}</size>";
                            //WINNER2 NAME STAR
                            string win11 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.yo10), "☆⁑")}</size>";
                            string win12 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.yo9), "y")}</size>";
                            string win13 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.yo8), "o")}</size>";
                            string win14 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.yo7), "。c")}</size>";
                            string win15 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.yo6), "l")}</size>";
                            string win16 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.yo4), "o。")}</size>";
                            string win17 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.yo4), "b")}</size>";
                            string win18 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.yo3), "o")}</size>";
                            string win19 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.yo2), "⁑")}</size>";
                            string win20 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.yo1), "☆")}</size>";
                            string snsname = win01 + win1 + win3 + win5 + win6 + win7 + win8 + win10 + "\r\n" + win11 + win12 + win13 + win14 + win15 + win16 + win17 + win18 + win19 + win20; //WINNER2 NAME & TITLE

                            client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.rosecolor), snsname)}");
                            Main.devNames.Add(client.Character.PlayerId, rname);

                        }
                        if (client.FriendCode is "slinkysoup#5274") //pineappleman  beespotty#5432
                        {
                            customTag = true;
                            string fontSize0 = "1.3";
                            string fontSize1 = "0.5";
                            string fontSize3 = "0.8";
                            string fontSize4 = "1";

                            //WINNER3 TITLE START
                            string win01 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pi1), "F")}</size>";
                            string win1 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pi2), "r")}</size>";
                            string win3 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pi4), "u")}</size>";
                            string win5 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pi5), "i")}</size>";
                            string win6 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pi6), "t ")}</size>";
                            string win7 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pi7), "M")}</size>";
                            string win8 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pi8), "a")}</size>";
                            string win10 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pi10), "n")}</size>";
                            //WINNER3 NAME STAR
                            string win11 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pi10), "P")}</size>";
                            string win12 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pi8), "i")}</size>";
                            string win13 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pi6), "n")}</size>";
                            string win14 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pi5), "ea")}</size>";
                            string win15 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pi4), "p")}</size>";
                            string win16 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pi3), "pl")}</size>";
                            string win17 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pi1), "e")}</size>";
                            string snsname = win01 + win1 + win3 + win5 + win6 + win7 + win8 + win10 + "\r\n" + win11 + win12 + win13 + win14 + win15 + win16 + win17; //WINNER3 NAME & TITLE

                            client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.rosecolor), snsname)}");
                            Main.devNames.Add(client.Character.PlayerId, rname);
                        }
                        if (client.FriendCode is "elitelike#1704") //Nicky G  beespotty#5432 lvvfsg
                        {
                            customTag = true;
                            string fontSize0 = "1.3";
                            string fontSize1 = "0.5";
                            string fontSize3 = "0.8";
                            string fontSize4 = "1";

                            //WINNER4 TITLE START
                            string win01 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ni1), "C")}</size>";
                            string win1 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ni2), "O")}</size>";
                            string win3 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ni3), "M")}</size>";
                            string win5 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ni4), "R")}</size>";
                            string win6 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ni5), "A")}</size>";
                            string win7 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ni6), "D")}</size>";
                            string win8 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ni7), "E")}</size>";

                            //WINNER4 NAME STAR
                            string win11 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ni7), "N")}</size>";
                            string win12 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ni6), "i")}</size>";
                            string win13 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ni5), "c")}</size>";
                            string win14 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ni4), "o ")}</size>";
                            string win15 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ni3), "T")}</size>";
                            string win16 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ni2), "O")}</size>";
                            string win17 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ni1), "R")}</size>";
                            string snsname = win01 + win1 + win3 + win5 + win6 + win7 + win8 + "\r\n" + win11 + win12 + win13 + win14 + win15 + win16 + win17; //WINNER4 NAME & TITLE

                            client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.rosecolor), snsname)}");
                            Main.devNames.Add(client.Character.PlayerId, rname);


                        }
                        if (client.FriendCode is "eastsnow#9772") //AAron   beespotty#5432
                        {
                            customTag = true;
                            string fontSize0 = "1.3";
                            string fontSize1 = "0.5";
                            string fontSize3 = "0.8";
                            string fontSize4 = "1";

                            //WINNER1 TITLE START  ˚ ♡ ｡˚・Milk・˚ ♡ ｡˚       ˚₊·-͟͟͞➳❥ Cry About it・❥・
                            string win01 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ml1), "•")}</size>";
                            string win1 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ml2), "♥")}</size>";
                            string win2 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ml3), "•")}</size>";
                            string win3 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ml4), "Cry ")}</size>";
                            string win5 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ml5), "Ab")}</size>";
                            string win6 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ml6), "o")}</size>";
                            string win7 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ml7), "ut")}</size>";
                            string win8 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ml8), " I")}</size>";
                            string win9 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ml9), "t •")}</size>";
                            string win10 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ml10), " ♥ •")}</size>";
                            //WINNER1 NAME STAR
                            string win11 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ml10), "°")}</size>";
                            string win12 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ml8), "♡ 。")}</size>";
                            string win13 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ml8), "º・")}</size>";
                            string win14 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ml7), "M")}</size>";
                            string win15 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ml6), "i")}</size>";
                            string win16 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ml5), "l")}</size>";
                            string win17 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ml4), "k")}</size>";
                            string win18 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ml3), "・º")}</size>";
                            string win19 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ml2), "。♡ ")}</size>";
                            string win20 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.ml1), "°")}</size>";
                            string snsname = win01 + win1 + win2 + win3 + win5 + win6 + win7 + win8 + win9 + win10 + "\r\n" + win11 + win12 + win13 + win14 + win15 + win16 + win17 + win18 + win19 + win20; //WINNER1 NAME & TITLE

                            client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.rosecolor), snsname)}");
                            Main.devNames.Add(client.Character.PlayerId, rname);

                        }
                        if (client.FriendCode is "irongrace#5957") //Paige   "beespotty#5432" or "envykindly#7034"
                        {
                            customTag = true;
                            string fontSize0 = "1.3";
                            string fontSize1 = "0.5";
                            string fontSize3 = "0.8";
                            string fontSize4 = "1";

                            //WINNER1 TITLE START  ˚ ♡ ｡˚・Milk・˚ ♡ ｡˚       ˚₊·-͟͟͞➳❥ Cry About it・❥・
                            string win01 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pg1), "《")}</size>";
                            string win1 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pg2), "⁑")}</size>";
                            string win2 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pg3), "S")}</size>";
                            string win3 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pg4), "MI")}</size>";
                            string win5 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pg5), "LE")}</size>";
                            string win6 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pg6), " M")}</size>";
                            string win7 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pg7), "OR")}</size>";
                            string win8 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pg8), "E")}</size>";
                            string win9 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pg9), "⁑")}</size>";
                            string win10 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pg10), "》")}</size>";
                            //WINNER1 NAME STAR
                            string win11 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pg10), "*")}</size>";
                            string win12 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pg8), "☆")}</size>";
                            string win13 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pg7), ".")}</size>";
                            string win14 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pg6), "C")}</size>";
                            string win15 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pg5), "•")}</size>";
                            string win17 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pg4), "C")}</size>";
                            string win18 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pg3), ".")}</size>";
                            string win19 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pg2), "☆")}</size>";
                            string win20 = $"<size={fontSize0}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pg1), "*")}</size>";
                            string snsname = win01 + win1 + win2 + win3 + win5 + win6 + win7 + win8 + win9 + win10 + "\r\n" + win11 + win12 + win13 + win14 + win15 + win17 + win18 + win19 + win20; //WINNER1 NAME & TITLE

                            client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.rosecolor), snsname)}");
                            Main.devNames.Add(client.Character.PlayerId, rname);
                        }
                        if (client.FriendCode is "sassysalon#0701")
                        {/*
                            string rtag = "type:sforce\ncode:sassysalon#0701\ncolor:#FFD2DF\ntoptext:<color=#FFD2DF>♡s</color><color=#FEC5D8>hi</color><color=#FFABCB>ft</color><color=#FF99BF>yr</color><color=#FF84BA>os</color><color=#FF65A9>e♡</color>\nname:<color=#FF65A9>♡E</color><color=#FF84BA>n</color><color=#FF99BF>o</color><color=#FFABCB>l</color><color=#FEC5D8>a</color><color=#FFD2DF>♡</color>";
                            List<string> response = CustomTags.ReturnTagInfoFromString(rtag);
                            Main.devNames.Add(client.Character.PlayerId, rname);
                            string fontSizee = "1.2";
                            string fontSizee2 = "1.5";
                            string tag = $"<size={fontSizee}>{Helpers.ColorString(Utils.GetHexColor(response[1]), $"{response[2]}")}</size>";
                            string realname = tag + "\r\n" + $"<size={fontSizee2}>{response[3]}</size>";
                            client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetHexColor(response[1]), realname)}");
                        */
                        }
                        if (!customTag)
                            if (File.Exists(CustomTags.GetFilePath(client.FriendCode)))
                            {
                                List<string> response = CustomTags.ReturnTagInfo(client.FriendCode);
                                switch (response[0])
                                {
                                    case "sforce":
                                        customTag = true;
                                        Main.devNames.Add(client.Character.PlayerId, rname);
                                        string fontSizee = "1.2";
                                        string fontSizee2 = "1.5";
                                        string tag = $"<size={fontSizee}>{Helpers.ColorString(Utils.GetHexColor(response[1]), $"{response[2]}")}</size>";
                                        string realname = tag + "\r\n" + $"<size={fontSizee2}>{response[3]}</size>";
                                        client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetHexColor(response[1]), realname)}");
                                        break;
                                    case "static":
                                        customTag = true;
                                        Main.devNames.Add(client.Character.PlayerId, rname);
                                        string fontSize = "1.2";
                                        string fontSize2 = "1.5";
                                        string sb = $"<size={fontSize}>{Helpers.ColorString(Utils.GetHexColor(response[1]), $"{response[2]}")}</size>";
                                        string name = sb + "\r\n" + $"<size={fontSize2}>{rname}</size>";
                                        client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetHexColor(response[1]), name)}");
                                        break;
                                    default:
                                    case "gradient":
                                        break;
                                }
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
                if (Main.ColliderPlayers.Contains(data.Character.PlayerId) && CustomRoles.YingYanger.IsEnable() && Options.ResetToYinYang.GetBool())
                {
                    Main.DoingYingYang = false;
                }
                if (Main.ColliderPlayers.Contains(data.Character.PlayerId))
                    Main.ColliderPlayers.Remove(data.Character.PlayerId);
                if (data.Character.LastImpostor())
                {
                    ShipStatus.Instance.enabled = false;
                    GameManager.Instance.RpcEndGame(GameOverReason.ImpostorDisconnect, false);
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
                AntiBlackout.OnDisconnect(data.Character.Data);
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
