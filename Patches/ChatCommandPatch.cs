using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Assets.CoreScripts;
using HarmonyLib;
using Hazel;
using static TownOfHost.Translator;

namespace TownOfHost
{
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    class ChatCommands
    {
        public static List<string> ChatHistory = new();
        public static bool Prefix(ChatController __instance)
        {
            if (__instance.TextArea.text == "") return false;
            __instance.TimeSinceLastMessage = 3f;
            var text = __instance.TextArea.text;
            if (ChatHistory.Count == 0 || ChatHistory[^1] != text) ChatHistory.Add(text);
            ChatControllerUpdatePatch.CurrentHistorySelection = ChatHistory.Count;
            string[] args = text.Split(' ');
            string subArgs = "";
            var canceled = false;
            var cancelVal = "";
            Main.isChatCommand = true;
            Logger.Info(text, "SendChat");
            switch (args[0])
            {
                case "/dump":
                    canceled = true;
                    Utils.DumpLog();
                    break;
                case "/v":
                case "/version":
                    canceled = true;
                    string version_text = "";
                    foreach (var kvp in Main.playerVersion.OrderBy(pair => pair.Key))
                    {
                        version_text += $"{kvp.Key}:{Utils.GetPlayerById(kvp.Key)?.Data?.PlayerName}:{kvp.Value.version}({kvp.Value.tag})\n";
                    }
                    if (version_text != "") HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, version_text);
                    break;
                /*
            case "/hat":
                var betterArgs = String.Compare("-", "_", true);
                PlayerControl.LocalPlayer.RpcSetHat(betterArgs);
                break;
            case "/pet":
                var betterArgsd = String.Compare("-", "_", true);
                PlayerControl.LocalPlayer.RpcSetPet(betterArgsd);
                break;
            case "/visor":
                var betterArgss = String.Compare("-", "_", true);
                PlayerControl.LocalPlayer.RpcSetVisor(betterArgss);
                break;
            case "/skin":
                var betterArgzs = String.Compare("-", "_", true);
                PlayerControl.LocalPlayer.RpcSetSkin(betterArgzs);
                break;
                */
                default:
                    Main.isChatCommand = false;
                    break;
            }
            if (AmongUsClient.Instance.AmHost)
            {
                Main.isChatCommand = true;
                switch (args[0])
                {
                    case "/win":
                    case "/winner":
                        canceled = true;
                        Utils.SendMessage("Winner: " + string.Join(",", Main.winnerList.Select(b => Main.AllPlayerNames[b])));
                        break;

                    case "/l":
                    case "/lastresult":
                        canceled = true;
                        Utils.ShowLastResult();
                        break;

                    case "/setplayers":
                        canceled = true;
                        subArgs = args.Length < 2 ? "" : args[1];
                        Utils.SendMessage("Max Players set to " + subArgs);
                        var numbereer = System.Convert.ToByte(subArgs);
                        Main.RealOptionsData.MaxPlayers = numbereer;
                        break;
                    case "/guess":
                    case "/shoot":
                        subArgs = args.Length < 2 ? "" : args[1];
                        string subArgs1 = args.Length < 3 ? "" : args[2];
                        Guesser.GuesserShootByID(PlayerControl.LocalPlayer, subArgs, subArgs1);
                        break;

                    case "/r":
                    case "/rename":
                        canceled = true;
                        Main.nickName = args.Length > 1 ? Main.nickName = args[1] : "";
                        break;
                    case "/allids":
                        canceled = true;
                        string senttext = "";
                        List<PlayerControl> AllPlayers = new();
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            AllPlayers.Add(pc);
                        }
                        senttext += "All Players and their IDs:";
                        foreach (var player in AllPlayers)
                        {
                            senttext += $"\n{player.GetRealName(true)} : {player.PlayerId}";
                        }
                        if (senttext != "") HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, senttext);
                        break;
                    case "/setimp":
                        canceled = true;
                        subArgs = args.Length < 2 ? "" : args[1];
                        Utils.SendMessage("Impostors set to " + subArgs);
                        var numberee = System.Convert.ToByte(subArgs);
                        Main.RealOptionsData.numImpostors = numberee;
                        break;
                    case "/m":
                    case "/myrole":
                        canceled = true;
                        var role = PlayerControl.LocalPlayer.GetCustomRole();
                        var subrole = PlayerControl.LocalPlayer.GetCustomSubRole();
                        if (GameStates.IsInGame)
                        {
                            if (role.IsVanilla()) HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, "Vanilla roles currently have no description.");
                            else HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, GetString(role.ToString()) + GetString($"{role}InfoLong"));
                            if (subrole != CustomRoles.NoSubRoleAssigned)
                                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, GetString(role.ToString()) + GetString($"{subrole}InfoLong"));
                        }
                        else { HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, "Sorry, you can only use this command inside the game."); }
                        break;
                    case "/meeting":
                        canceled = true;
                        PlayerControl.LocalPlayer.CmdReportDeadBody(null);
                        break;
                    case "/colour":
                    case "/color":
                        canceled = true;
                        subArgs = args.Length < 2 ? "" : args[1];
                        Utils.SendMessage("Color ID set to " + subArgs);
                        var numbere = System.Convert.ToByte(subArgs);
                        PlayerControl.LocalPlayer.RpcSetColor(numbere);
                        break;
                    case "/level":
                        canceled = true;
                        subArgs = args.Length < 2 ? "" : args[1];
                        Utils.SendMessage("Current AU Level Set to " + subArgs, PlayerControl.LocalPlayer.PlayerId);
                        //nt32.Parse("-105");
                        var number = System.Convert.ToUInt32(subArgs);
                        PlayerControl.LocalPlayer.RpcSetLevel(number - 1);
                        break;
                    case "/n":
                    case "/now":
                        canceled = true;
                        subArgs = args.Length < 2 ? "" : args[1];
                        switch (subArgs)
                        {
                            case "r":
                            case "roles":
                                Utils.ShowActiveRoles();
                                break;
                            default:
                                Utils.ShowActiveSettings();
                                break;
                        }
                        break;
                    case "/perc":
                    case "/percentages":
                        canceled = true;
                        Utils.ShowPercentages();
                        break;
                    case "/dis":
                        canceled = true;
                        subArgs = args.Length < 2 ? "" : args[1];
                        switch (subArgs)
                        {
                            case "crewmate":
                                ShipStatus.Instance.enabled = false;
                                ShipStatus.RpcEndGame(GameOverReason.HumansDisconnect, false);
                                break;

                            case "impostor":
                                ShipStatus.Instance.enabled = false;
                                ShipStatus.RpcEndGame(GameOverReason.ImpostorDisconnect, false);
                                break;

                            default:
                                __instance.AddChat(PlayerControl.LocalPlayer, "crewmate | impostor");
                                cancelVal = "/dis";
                                break;
                        }
                        ShipStatus.Instance.RpcRepairSystem(SystemTypes.Admin, 0);
                        break;

                    case "/h":
                    case "/help":
                        canceled = true;
                        subArgs = args.Length < 2 ? "" : args[1];
                        switch (subArgs)
                        {
                            case "r":
                            case "roles":
                                subArgs = args.Length < 3 ? "" : args[2];
                                GetRolesInfo(subArgs);
                                break;

                            case "att":
                            case "attributes":
                                subArgs = args.Length < 3 ? "" : args[2];
                                switch (subArgs)
                                {
                                    case "lastimpostor":
                                    case "limp":
                                        Utils.SendMessage(Utils.GetRoleName(CustomRoles.LastImpostor) + GetString("LastImpostorInfoLong"));
                                        break;

                                    default:
                                        Utils.SendMessage($"{GetString("Command.h_args")}:\n lastimpostor(limp)");
                                        break;
                                }
                                break;

                            case "m":
                            case "modes":
                                subArgs = args.Length < 3 ? "" : args[2];
                                switch (subArgs)
                                {
                                    case "hideandseek":
                                    case "has":
                                        Utils.SendMessage(GetString("HideAndSeekInfo"));
                                        break;

                                    case "nogameend":
                                    case "nge":
                                        Utils.SendMessage(GetString("NoGameEndInfo"));
                                        break;

                                    case "syncbuttonmode":
                                    case "sbm":
                                        Utils.SendMessage(GetString("SyncButtonModeInfo"));
                                        break;

                                    case "randommapsmode":
                                    case "rmm":
                                        Utils.SendMessage(GetString("RandomMapsModeInfo"));
                                        break;
                                    case "cc":
                                    case "camocomms":
                                        Utils.SendMessage(GetString("CamoCommsInfo"));
                                        break;

                                    default:
                                        Utils.SendMessage($"{GetString("Command.h_args")}:\n hideandseek(has), nogameend(nge), syncbuttonmode(sbm), randommapsmode(rmm)");
                                        break;
                                }
                                break;


                            case "n":
                            case "now":
                                Utils.ShowActiveSettingsHelp();
                                break;

                            default:
                                Utils.ShowHelp();
                                break;
                        }
                        break;

                    case "/t":
                    case "/template":
                        canceled = true;
                        if (args.Length > 1) SendTemplate(args[1]);
                        else HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"{GetString("ForExample")}:\n{args[0]} test");
                        break;

                    case "/mw":
                    case "/messagewait":
                        canceled = true;
                        if (args.Length > 1 && int.TryParse(args[1], out int sec))
                        {
                            Main.MessageWait.Value = sec;
                            Utils.SendMessage(string.Format(GetString("Message.SetToSeconds"), sec), 0);
                        }
                        else Utils.SendMessage($"{GetString("Message.MessageWaitHelp")}\n{GetString("ForExample")}:\n{args[0]} 3", 0);
                        break;

                    case "/exile":
                        canceled = true;
                        if (args.Length < 2 || !int.TryParse(args[1], out int id)) break;
                        Utils.GetPlayerById(id)?.RpcExileV2();
                        break;

                    case "/kill":
                        canceled = true;
                        if (args.Length < 2 || !int.TryParse(args[1], out int id2)) break;
                        Utils.GetPlayerById(id2)?.RpcMurderPlayer(Utils.GetPlayerById(id2));
                        break;

                    case "/changerole":
                        canceled = true;
                        subArgs = args.Length < 2 ? "" : args[1];
                        switch (subArgs)
                        {
                            case "crewmate":
                                PlayerControl.LocalPlayer.RpcSetCustomRole(CustomRoles.Crewmate);
                                PlayerControl.LocalPlayer.RpcSetRole(RoleTypes.Crewmate);
                                break;

                            case "impostor":
                                PlayerControl.LocalPlayer.RpcSetCustomRole(CustomRoles.Impostor);
                                PlayerControl.LocalPlayer.RpcSetRole(RoleTypes.Impostor);
                                break;

                            case "engineer":
                                PlayerControl.LocalPlayer.RpcSetCustomRole(CustomRoles.Engineer);
                                PlayerControl.LocalPlayer.RpcSetRole(RoleTypes.Engineer);
                                break;
                            case "shapeshifter":
                                PlayerControl.LocalPlayer.RpcSetCustomRole(CustomRoles.Shapeshifter);
                                PlayerControl.LocalPlayer.RpcSetRole(RoleTypes.Shapeshifter);
                                break;

                            default:
                                PlayerControl.LocalPlayer.RpcSetCustomRole(CustomRoles.Crewmate);
                                PlayerControl.LocalPlayer.RpcSetRole(RoleTypes.Crewmate);
                                break;
                        }
                        break;
                    default:
                        Main.isChatCommand = false;
                        break;
                }
            }
            if (canceled)
            {
                Logger.Info("Command Canceled", "ChatCommand");
                __instance.TextArea.Clear();
                __instance.TextArea.SetText(cancelVal);
                __instance.quickChatMenu.ResetGlyphs();
            }
            return !canceled;
        }

        public static void GetRolesInfo(string role)
        {
            var roleList = new Dictionary<CustomRoles, string>
            {
                //GM
                { CustomRoles.GM, "gm" },
                //Impostor役職
                { (CustomRoles)(-1), $"== {GetString("Impostor")} ==" }, //区切り用
                { CustomRoles.BountyHunter, "bo" },
                { CustomRoles.FireWorks, "fw" },
                { CustomRoles.Mare, "ma" },
                { CustomRoles.Mafia, "mf" },
                { CustomRoles.SerialKiller, "sk" },
                //{ CustomRoles.ShapeMaster, "sha" },
                { CustomRoles.TimeThief, "tt"},
                { CustomRoles.VoteStealer, "vs"},
                { CustomRoles.Sniper, "snp" },
                { CustomRoles.Puppeteer, "pup" },
                { CustomRoles.Vampire, "va" },
                { CustomRoles.Warlock, "wa" },
                { CustomRoles.Witch, "wi" },
                { CustomRoles.Silencer, "si" },
                { CustomRoles.Ninja,"ni"},
                { CustomRoles.Miner,"mi"},
                { CustomRoles.YingYanger,"yy"},
                { CustomRoles.Camouflager,"cf"},
                { CustomRoles.Grenadier,"gr"},
                { CustomRoles.CorruptedSheriff, "csh" },
                {CustomRoles.EvilGuesser, "eg"},
                //Madmate役職
                { (CustomRoles)(-2), $"== {GetString("Madmate")} ==" }, //区切り用
                { CustomRoles.MadGuardian, "mg" },
                { CustomRoles.Madmate, "mm" },
                { CustomRoles.MadSnitch, "msn" },
                { CustomRoles.SKMadmate, "sm" },
                { CustomRoles.Parasite, "pa" },
                //両陣営役職
                { (CustomRoles)(-3), $"== {GetString("Impostor")} or {GetString("Crewmate")} ==" }, //区切り用
                { CustomRoles.Watcher, "wat" },
                {CustomRoles.Guesser, "gue"},
                { CustomRoles.CrewPostor, "cp" },
                //Crewmate役職
                { (CustomRoles)(-4), $"== {GetString("Crewmate")} ==" }, //区切り用
                { CustomRoles.Dictator, "dic" },
                { CustomRoles.Child, "cd" },
                { CustomRoles.Medium, "med" },
                { CustomRoles.Psychic, "psy" },
                { CustomRoles.Doctor, "doc" },
                { CustomRoles.Lighter, "li" },
                { CustomRoles.Mayor, "my" },
                { CustomRoles.Veteran, "vet" },
                { CustomRoles.SabotageMaster, "sa" },
                {CustomRoles.NiceGuesser, "ng"},
                { CustomRoles.Sheriff, "sh" },
                { CustomRoles.Investigator, "inve" },
                { CustomRoles.Mystic,"my"},
                { CustomRoles.Snitch, "sn" },
                { CustomRoles.SpeedBooster, "sb" },
                { CustomRoles.Trapper, "tra" },
                { CustomRoles.Bastion, "bas"},
                { CustomRoles.Demolitionist, "demo"},
                //Neutral役職
                { (CustomRoles)(-5), $"== {GetString("Neutral")} ==" }, //区切り用
                { CustomRoles.Arsonist, "ar" },
                { CustomRoles.BloodKnight,"bk"},
                { CustomRoles.Egoist, "eg" },
                { CustomRoles.Executioner, "exe" },
                { CustomRoles.Jester, "je" },
                { CustomRoles.Phantom, "ph" },
                { CustomRoles.Opportunist, "op" },
                { CustomRoles.Survivor, "sur" },
                { CustomRoles.SchrodingerCat, "sc" },
                {CustomRoles.Pirate, "pi"},
                { CustomRoles.Marksman, "mar" },
                { CustomRoles.Terrorist, "te" },
                { CustomRoles.Jackal, "jac" },
                { CustomRoles.Sidekick, "jacsk" },
                //{ CustomRoles.Juggernaut, "jn"},
                { CustomRoles.PlagueBearer, "pb" },
                { CustomRoles.Pestilence, "pesti" },
                { CustomRoles.Juggernaut, "jug"},
                { CustomRoles.Vulture, "vu"},
                { CustomRoles.Coven, "co" },
                { CustomRoles.CovenWitch, "cw" },
                { CustomRoles.Poisoner, "poison" },
                { CustomRoles.HexMaster, "hm" },
                { CustomRoles.Medusa, "med" },
                { CustomRoles.TheGlitch, "gl" },
                { CustomRoles.Werewolf, "ww" },
                { CustomRoles.Amnesiac, "amne" },
                { CustomRoles.GuardianAngelTOU, "ga" },
                { CustomRoles.Hacker, "hac" },
                //Sub役職
                { (CustomRoles)(-6), $"== {GetString("SubRole")} ==" }, //区切り用
                {CustomRoles.Lovers, "lo" },
                { CustomRoles.Sleuth, "sl" },
                { CustomRoles.Bait, "ba" },
                { CustomRoles.Oblivious, "obl" },
                { CustomRoles.Torch, "to" },
                { CustomRoles.Flash, "fl" },
                { CustomRoles.Bewilder, "be" },
                { CustomRoles.TieBreaker, "tb" },
                { CustomRoles.Diseased, "di" },
                //HAS
                { (CustomRoles)(-7), $"== {GetString("HideAndSeek")} ==" }, //区切り用
                { CustomRoles.HASFox, "hfo" },
                { CustomRoles.HASTroll, "htr" },
                { CustomRoles.Supporter, "wor" },
                { CustomRoles.Janitor, "jan" },
                { CustomRoles.Painter, "pan" },

            };
            var msg = "";
            var rolemsg = $"{GetString("Command.h_args")}";
            foreach (var r in roleList)
            {
                var roleName = r.Key.ToString();
                var roleShort = r.Value;

                if (String.Compare(role, roleName, true) == 0 || String.Compare(role, roleShort, true) == 0)
                {
                    Utils.SendMessage(GetString(roleName) + GetString($"{roleName}InfoLong"));
                    return;
                }

                var roleText = $"{roleName.ToLower()}({roleShort.ToLower()}), ";
                if ((int)r.Key < 0)
                {
                    msg += rolemsg + "\n" + roleShort + "\n";
                    rolemsg = "";
                }
                else if ((rolemsg.Length + roleText.Length) > 40)
                {
                    msg += rolemsg + "\n";
                    rolemsg = roleText;
                }
                else
                {
                    rolemsg += roleText;
                }
            }
            msg += rolemsg;
            Utils.SendMessage(msg);
        }
        public static void PublicGetRolesInfo(string role, byte playerId = 0xff)
        {
            var roleList = new Dictionary<CustomRoles, string>
            {
                //GM
                { CustomRoles.GM, "gm" },
                //Impostor役職
                { (CustomRoles)(-1), $"== {GetString("Impostor")} ==" }, //区切り用
                { CustomRoles.BountyHunter, "bo" },
                { CustomRoles.FireWorks, "fw" },
                { CustomRoles.Mare, "ma" },
                { CustomRoles.Mafia, "mf" },
                { CustomRoles.SerialKiller, "sk" },
                //{ CustomRoles.ShapeMaster, "sha" },
                { CustomRoles.TimeThief, "tt"},
                { CustomRoles.VoteStealer, "vs"},
                { CustomRoles.Sniper, "snp" },
                { CustomRoles.Puppeteer, "pup" },
                { CustomRoles.Vampire, "va" },
                { CustomRoles.Warlock, "wa" },
                { CustomRoles.Witch, "wi" },
                { CustomRoles.Silencer, "si" },
                { CustomRoles.Camouflager,"cf"},
                { CustomRoles.Ninja,"ni"},
                { CustomRoles.Grenadier,"gr"},
                { CustomRoles.Miner,"mi"},
                { CustomRoles.YingYanger,"yy"},
                { CustomRoles.CorruptedSheriff, "csh" },
                {CustomRoles.EvilGuesser, "eg"},
                //Madmate役職
                { (CustomRoles)(-2), $"== {GetString("Madmate")} ==" }, //区切り用
                { CustomRoles.MadGuardian, "mg" },
                { CustomRoles.Madmate, "mm" },
                { CustomRoles.MadSnitch, "msn" },
                { CustomRoles.SKMadmate, "sm" },
                { CustomRoles.Parasite, "pa" },
                //両陣営役職
                { (CustomRoles)(-3), $"== {GetString("Impostor")} or {GetString("Crewmate")} ==" }, //区切り用
                { CustomRoles.Watcher, "wat" },
                {CustomRoles.Guesser, "gue"},
                { CustomRoles.CrewPostor, "cp" },
                //Crewmate役職
                { (CustomRoles)(-4), $"== {GetString("Crewmate")} ==" }, //区切り用
                { CustomRoles.Dictator, "dic" },
                { CustomRoles.Child, "cd" },
                { CustomRoles.Medium, "med" },
                { CustomRoles.Psychic, "psy" },
                { CustomRoles.Doctor, "doc" },
                { CustomRoles.Lighter, "li" },
                { CustomRoles.Mayor, "my" },
                { CustomRoles.Veteran, "vet" },
                { CustomRoles.SabotageMaster, "sa" },
                { CustomRoles.Sheriff, "sh" },
                {CustomRoles.NiceGuesser, "ng"},
                { CustomRoles.Investigator, "inve" },
                { CustomRoles.Mystic,"my"},
               // { CustomRoles.CorruptedSheriff, "csh" },
                { CustomRoles.Snitch, "sn" },
                { CustomRoles.SpeedBooster, "sb" },
                { CustomRoles.Trapper, "tra" },
                { CustomRoles.Bastion, "bas"},
                { CustomRoles.Demolitionist, "demo"},
                //Neutral役職
                { (CustomRoles)(-5), $"== {GetString("Neutral")} ==" }, //区切り用
                { CustomRoles.Arsonist, "ar" },
                { CustomRoles.BloodKnight,"bk"},
                { CustomRoles.Egoist, "eg" },
                { CustomRoles.Executioner, "exe" },
                { CustomRoles.Jester, "je" },
                { CustomRoles.Phantom, "ph" },
                { CustomRoles.Opportunist, "op" },
                { CustomRoles.Survivor, "sur" },
                { CustomRoles.SchrodingerCat, "sc" },
                { CustomRoles.Terrorist, "te" },
                { CustomRoles.Marksman, "mar" },
                { CustomRoles.Jackal, "jac" },
                { CustomRoles.Sidekick, "jacsk" },
                //{ CustomRoles.Juggernaut, "jn"},
                { CustomRoles.PlagueBearer, "pb" },
                { CustomRoles.Pestilence, "pesti" },
                { CustomRoles.Juggernaut, "jug"},
                { CustomRoles.Vulture, "vu"},
                { CustomRoles.Coven, "co" },
                { CustomRoles.CovenWitch, "cw" },
                { CustomRoles.Poisoner, "poison" },
                { CustomRoles.HexMaster, "hm" },
                { CustomRoles.Medusa, "med" },
                { CustomRoles.TheGlitch, "gl" },
                { CustomRoles.Werewolf, "ww" },
                {CustomRoles.Pirate, "pi"},
                { CustomRoles.Amnesiac, "amne" },
                { CustomRoles.GuardianAngelTOU, "ga" },
                { CustomRoles.Hacker, "hac" },
                //Sub役職
                { (CustomRoles)(-6), $"== {GetString("SubRole")} ==" }, //区切り用
                {CustomRoles.Lovers, "lo" },
                { CustomRoles.Sleuth, "sl" },
                { CustomRoles.Bait, "ba" },
                { CustomRoles.Oblivious, "obl" },
                { CustomRoles.Torch, "to" },
                { CustomRoles.Flash, "fl" },
                { CustomRoles.Bewilder, "be" },
                { CustomRoles.TieBreaker, "tb" },
                { CustomRoles.Diseased, "di" },
                //HAS
                { (CustomRoles)(-7), $"== {GetString("HideAndSeek")} ==" }, //区切り用
                { CustomRoles.HASFox, "hfo" },
                { CustomRoles.HASTroll, "htr" },
                { CustomRoles.Supporter, "wor" },
                { CustomRoles.Janitor, "jan" },
                { CustomRoles.Painter, "pan" },

            };
            var msg = "";
            var rolemsg = $"{GetString("Command.h_args")}";
            foreach (var r in roleList)
            {
                var roleName = r.Key.ToString();
                var roleShort = r.Value;

                if (String.Compare(role, roleName, true) == 0 || String.Compare(role, roleShort, true) == 0)
                {
                    Utils.SendMessage(GetString(roleName) + GetString($"{roleName}InfoLong"), playerId);
                    return;
                }

                //Utils.SendMessage("Sorry, the current role you tried to search up was not inside our databse. Either you misspelled it, or its not there.", playerId);
            }
            //msg += rolemsg;
            //Utils.SendMessage(msg);
        }
        public static void myRole(byte playerId = 0xff)
        {
            if (GameStates.IsInGame || GameStates.IsMeeting)
            {
                var roleList = new Dictionary<CustomRoles, string>
            {
                //GM
                { CustomRoles.GM, "gm" },
                //Impostor役職
                { (CustomRoles)(-1), $"== {GetString("Impostor")} ==" }, //区切り用
                { CustomRoles.BountyHunter, "bo" },
                { CustomRoles.FireWorks, "fw" },
                { CustomRoles.Mare, "ma" },
                { CustomRoles.Mafia, "mf" },
                { CustomRoles.SerialKiller, "sk" },
                //{ CustomRoles.ShapeMaster, "sha" },
                { CustomRoles.TimeThief, "tt"},
                { CustomRoles.VoteStealer, "vs"},
                { CustomRoles.Sniper, "snp" },
                { CustomRoles.Puppeteer, "pup" },
                { CustomRoles.Vampire, "va" },
                { CustomRoles.Warlock, "wa" },
                { CustomRoles.Witch, "wi" },
                { CustomRoles.Silencer, "si" },
                { CustomRoles.Ninja,"ni"},
                { CustomRoles.Miner,"mi"},
                { CustomRoles.YingYanger,"yy"},
                { CustomRoles.Camouflager,"cf"},
                { CustomRoles.CorruptedSheriff, "csh" },
                { CustomRoles.Grenadier,"gr"},
                //Madmate役職
                { (CustomRoles)(-2), $"== {GetString("Madmate")} ==" }, //区切り用
                { CustomRoles.MadGuardian, "mg" },
                { CustomRoles.Madmate, "mm" },
                { CustomRoles.MadSnitch, "msn" },
                { CustomRoles.SKMadmate, "sm" },
                { CustomRoles.Parasite, "pa" },
                //両陣営役職
                { (CustomRoles)(-3), $"== {GetString("Impostor")} or {GetString("Crewmate")} ==" }, //区切り用
                { CustomRoles.Watcher, "wat" },
                {CustomRoles.Guesser, "gue"},
                { CustomRoles.CrewPostor, "cp" },
                {CustomRoles.NiceGuesser, "ng"},
                {CustomRoles.EvilGuesser, "eg"},
                {CustomRoles.Pirate, "pi"},
                //Crewmate役職
                { (CustomRoles)(-4), $"== {GetString("Crewmate")} ==" }, //区切り用
                { CustomRoles.Dictator, "dic" },
                { CustomRoles.Child, "cd" },
                { CustomRoles.Medium, "med" },
                { CustomRoles.Psychic, "psy" },
                { CustomRoles.Doctor, "doc" },
                { CustomRoles.Lighter, "li" },
                { CustomRoles.Mayor, "my" },
                { CustomRoles.Veteran, "vet" },
                { CustomRoles.SabotageMaster, "sa" },
                { CustomRoles.Sheriff, "sh" },
                { CustomRoles.Investigator, "inve" },
                { CustomRoles.Mystic,"my"},
                { CustomRoles.Snitch, "sn" },
                { CustomRoles.SpeedBooster, "sb" },
                { CustomRoles.Trapper, "tra" },
                { CustomRoles.Bastion, "bas"},
                { CustomRoles.Demolitionist, "demo"},
                //Neutral役職
                { (CustomRoles)(-5), $"== {GetString("Neutral")} ==" }, //区切り用
                { CustomRoles.Arsonist, "ar" },
                { CustomRoles.BloodKnight,"bk"},
                { CustomRoles.Egoist, "eg" },
                { CustomRoles.Executioner, "exe" },
                { CustomRoles.Jester, "je" },
                { CustomRoles.Phantom, "ph" },
                { CustomRoles.Opportunist, "op" },
                { CustomRoles.Survivor, "sur" },
                { CustomRoles.SchrodingerCat, "sc" },
                { CustomRoles.Terrorist, "te" },
                { CustomRoles.Jackal, "jac" },
                { CustomRoles.Marksman, "mar" },
                { CustomRoles.Sidekick, "jacsk" },
                //{ CustomRoles.Juggernaut, "jn"},
                { CustomRoles.PlagueBearer, "pb" },
                { CustomRoles.Pestilence, "pesti" },
                { CustomRoles.Juggernaut, "jug"},
                { CustomRoles.Vulture, "vu"},
                { CustomRoles.Coven, "co" },
                { CustomRoles.CovenWitch, "cw" },
                { CustomRoles.Poisoner, "poison" },
                { CustomRoles.HexMaster, "hm" },
                { CustomRoles.Medusa, "med" },
                { CustomRoles.TheGlitch, "gl" },
                { CustomRoles.Werewolf, "ww" },
                { CustomRoles.Amnesiac, "amne" },
                { CustomRoles.GuardianAngelTOU, "ga" },
                { CustomRoles.Hacker, "hac" },
                //Sub役職
                { (CustomRoles)(-6), $"== {GetString("SubRole")} ==" }, //区切り用
                { CustomRoles.Lovers, "lo" },
                { CustomRoles.Sleuth, "sl" },
                { CustomRoles.Bait, "ba" },
                { CustomRoles.Oblivious, "obl" },
                { CustomRoles.Torch, "to" },
                { CustomRoles.Flash, "fl" },
                { CustomRoles.Bewilder, "be" },
                { CustomRoles.TieBreaker, "tb" },
                { CustomRoles.Diseased, "di" },
                //HAS
                { (CustomRoles)(-7), $"== {GetString("HideAndSeek")} ==" }, //区切り用
                { CustomRoles.HASFox, "hfo" },
                { CustomRoles.HASTroll, "htr" },
                { CustomRoles.Supporter, "wor" },
                { CustomRoles.Janitor, "jan" },
                { CustomRoles.Painter, "pan" },

            };
                var msg = "";
                var rolemsg = $"{GetString("Command.h_args")}";
                var role = Utils.GetPlayerById(playerId).GetCustomRole().ToString();
                foreach (var r in roleList)
                {
                    var roleName = r.Key.ToString();
                    var roleShort = r.Value;

                    if (String.Compare(role, roleName, true) == 0 || String.Compare(role, roleShort, true) == 0)
                    {
                        Utils.SendMessage(GetString(roleName) + GetString($"{roleName}InfoLong"), playerId);
                        return;
                    }

                    // Utils.SendMessage("Sorry, your role was not inside our database currently.", playerId);
                }
            }
            else Utils.SendMessage("Sorry, you can only use this command inside the game.", playerId);
            //msg += rolemsg;
            //Utils.SendMessage(msg);
        }
        public static void SendTemplate(string str = "", byte playerId = 0xff, bool noErr = false)
        {
            if (!File.Exists("template.txt"))
            {
                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, "Among Us.exeと同じフォルダにtemplate.txtが見つかりませんでした。\n新規作成します。");
                File.WriteAllText(@"template.txt", "test:This is template text.\\nLine breaks are also possible.\ntest:これは定型文です。\\n改行も可能です。");
                return;
            }
            using StreamReader sr = new(@"template.txt", Encoding.GetEncoding("UTF-8"));
            string text;
            string[] tmp = { };
            List<string> sendList = new();
            HashSet<string> tags = new();
            while ((text = sr.ReadLine()) != null)
            {
                tmp = text.Split(":");
                if (tmp.Length > 1 && tmp[1] != "")
                {
                    tags.Add(tmp[0]);
                    if (tmp[0] == str) sendList.Add(tmp.Skip(1).Join(delimiter: "").Replace("\\n", "\n"));
                }
            }
            if (sendList.Count == 0 && !noErr)
            {
                if (playerId == 0xff)
                    HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, string.Format(GetString("Message.TemplateNotFoundHost"), str, tags.Join(delimiter: ", ")));
                else Utils.SendMessage(string.Format(GetString("Message.TemplateNotFoundClient"), str), playerId);
            }
            else for (int i = 0; i < sendList.Count; i++) Utils.SendMessage(sendList[i], playerId);
        }
        public static void OnReceiveChat(PlayerControl player, string text)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (Main.SilencedPlayer.Count != 0)
            {
                //someone is silenced
                foreach (var p in Main.SilencedPlayer)
                {
                    if (player.PlayerId == p.PlayerId) continue;
                    if (!player.Data.IsDead)
                    {
                        text = "Silenced.";
                        Logger.Info($"{p.GetNameWithRole()}:{text}", "TriedToSendChatButSilenced");
                        Utils.SendMessage("You are currently Silenced. Try talking again when you aren't silenced.", player.PlayerId);
                    }
                }
            }
            string[] args = text.Split(' ');
            string subArgs = "";
            switch (args[0])
            {
                case "/l":
                case "/lastresult":
                    Utils.ShowLastResult(player.PlayerId);
                    break;

                case "/name":
                    if (Options.Customise.GetBool())
                    {
                        var canRename = true;
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc.Data.PlayerName == args[1])
                                canRename = false;
                        }
                        if (canRename)
                            player.Data.PlayerName = args.Length > 1 ? player.Data.PlayerName = args[1] : "";
                    }
                    else { Utils.SendMessage("The host has currently disabled access to this command.\nTry again when this command is enabled.", player.PlayerId); }
                    break;
                case "/n":
                case "/now":
                    var name = args.Length > 20 ? "Test" : subArgs;
                    subArgs = args.Length < 2 ? "Test" : name;
                    switch (subArgs)
                    {
                        case "r":
                        case "roles":
                            Utils.ShowActiveRoles(player.PlayerId);
                            break;

                        default:
                            Utils.ShowActiveSettings(player.PlayerId);
                            break;
                    }
                    break;
                case "/guess":
                case "/shoot":
                    subArgs = args.Length < 2 ? "" : args[1];
                    string subArgs1 = args.Length < 3 ? "" : args[2];
                    Guesser.GuesserShootByID(player, subArgs, subArgs1);
                    break;
                case "/perc":
                case "/percentages":
                    Utils.ShowPercentages(player.PlayerId);
                    break;
                case "/myrole":
                    var role = player.GetCustomRole();
                    var subrole = player.GetCustomSubRole();
                    if (GameStates.IsInGame)
                    {
                        if (role.IsVanilla()) Utils.SendMessage("Vanilla roles currently have no description.", player.PlayerId);
                        else Utils.SendMessage(GetString(role.ToString()) + GetString($"{role}InfoLong"), player.PlayerId);
                        if (subrole != CustomRoles.NoSubRoleAssigned)
                            Utils.SendMessage(GetString(role.ToString()) + GetString($"{subrole}InfoLong"), player.PlayerId);
                    }
                    else { Utils.SendMessage("Sorry, you can only use this command inside the game.", player.PlayerId); }
                    break;
                case "/level":
                    if (Options.Customise.GetBool())
                    {
                        /*subArgs = args.Length < 2 ? "" : args[1];
                        Utils.SendMessage("Current AU Level Set to " + subArgs + ". AU auto adds 1 to your current level. Starting players are at level 0, so AU adds 1 to make you level 1. So no one is level 100, we are all just at level 99.", player.PlayerId);
                        //nt32.Parse("-105");
                        var number = System.Convert.ToUInt32(subArgs);
                        player.RpcSetLevel(number);*/
                        Utils.SendMessage("This command currently does not work as intended for non-host players.\nIn order to prevent kicks, we have disabled this command.", player.PlayerId);
                    }
                    else { Utils.SendMessage("The host has currently disabled access to this command.\nTry again when this command is enabled.", player.PlayerId); }
                    break;
                case "/colour":
                case "/color":
                    if (Options.Customise.GetBool())
                    {
                        subArgs = args.Length < 2 ? "" : args[1];
                        Utils.SendMessage("Color ID set to " + subArgs, player.PlayerId);
                        var numbere = System.Convert.ToByte(subArgs);
                        player.RpcSetColor(numbere);
                    }
                    else { Utils.SendMessage("The host has currently disabled access to this command.\nTry again when this command is enabled.", player.PlayerId); }
                    break;
                /*case "/hat":
                    subArgs = args.Length < 3 ? "" : args[1];
                    var betterArgs = String.Compare("-", "_", true);
                    player.RpcSetHat(betterArgs);
                    break;
                case "/pet":
                    subArgs = args.Length < 3 ? "" : args[1];
                    var betterArgss = String.Compare("-", "_", true);
                    player.RpcSetPet(betterArgss);
                    break;
                case "/visor":
                    subArgs = args.Length < 3 ? "" : args[1];
                    var betterArgsd = String.Compare("-", "_", true);
                    player.RpcSetVisor(betterArgsd);
                    break;
                case "/skin":
                    subArgs = args.Length < 3 ? "" : args[1];
                    var betterArgsb = String.Compare("-", "_", true);
                    player.RpcSetSkin(betterArgsb);
                    break;
                    */
                case "/roleinfo":
                    subArgs = args.Length < 3 ? "" : args[1];
                    PublicGetRolesInfo(subArgs, player.PlayerId);
                    //PublicGetRolesInfo();
                    break;
                case "/t":
                case "/template":
                    if (args.Length > 1) SendTemplate(args[1], player.PlayerId);
                    else Utils.SendMessage($"{GetString("ForExample")}:\n{args[0]} test", player.PlayerId);
                    break;

                default:
                    break;
            }
        }
    }
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
    class ChatUpdatePatch
    {
        public static void Postfix(ChatController __instance)
        {
            if (!AmongUsClient.Instance.AmHost || Main.MessagesToSend.Count < 1 || (Main.MessagesToSend[0].Item2 == byte.MaxValue && Main.MessageWait.Value > __instance.TimeSinceLastMessage)) return;
            var player = PlayerControl.AllPlayerControls.ToArray().OrderBy(x => x.PlayerId).Where(x => !x.Data.IsDead).FirstOrDefault();
            if (player == null) return;
            (string msg, byte sendTo) = Main.MessagesToSend[0];
            Main.MessagesToSend.RemoveAt(0);
            int clientId = sendTo == byte.MaxValue ? -1 : Utils.GetPlayerById(sendTo).GetClientId();
            if (clientId == -1) DestroyableSingleton<HudManager>.Instance.Chat.AddChat(player, msg);
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SendChat, SendOption.None, clientId);
            writer.Write(msg);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            __instance.TimeSinceLastMessage = 0f;
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
    class AddChatPatch
    {
        public static void Postfix(string chatText)
        {
            switch (chatText)
            {
                default:
                    break;
            }
            if (!AmongUsClient.Instance.AmHost) return;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSendChat))]
    class RpcSendChatPatch
    {
        public static bool Prefix(PlayerControl __instance, string chatText, ref bool __result)
        {
            if (string.IsNullOrWhiteSpace(chatText))
            {
                __result = false;
                return false;
            }
            if (AmongUsClient.Instance.AmClient && DestroyableSingleton<HudManager>.Instance)
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(__instance, chatText);
            if (chatText.IndexOf("who", StringComparison.OrdinalIgnoreCase) >= 0)
                DestroyableSingleton<Telemetry>.Instance.SendWho();
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(__instance.NetId, (byte)RpcCalls.SendChat, SendOption.None);
            messageWriter.Write(chatText);
            messageWriter.EndMessage();
            __result = true;
            return false;
        }
    }
}
