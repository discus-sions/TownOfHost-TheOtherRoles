using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Assets.CoreScripts;
using HarmonyLib;
using Hazel;
using static TownOfHost.Translator;
using AmongUs.GameOptions;

namespace TownOfHost
{
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    class ChatCommands
    {
        public static List<string> ChatHistory = new();
        public static Dictionary<byte, ChatController> allControllers = new();
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
            var player = PlayerControl.LocalPlayer;
            Main.isChatCommand = true;
            Logger.Info(text, "SendChat");
            switch (args[0])
            {
                case "/dump":
                    if (GameStates.IsLobby)
                    {
                        canceled = true;
                        Utils.DumpLog();
                    }
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
                        Utils.SendMessage($"Winning Team: {SetEverythingUpPatch.LastWinsText}\n\nWinner(s): " + string.Join(",", Main.winnerList.Select(b => Main.AllPlayerNames[b])));
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
                        GameOptionsManager.Instance.currentNormalGameOptions.MaxPlayers = numbereer;
                        break;
                    case "/guess":
                    case "/shoot":
                        subArgs = args.Length < 2 ? "" : args[1];
                        string subArgs1 = args.Length < 3 ? "" : args[2];
                        Guesser.GuesserShootByID(PlayerControl.LocalPlayer, subArgs, subArgs1);
                        break;

                    case "/tag":
                        string type = args.Length < 2 ? "" : args[1];
                        switch (type)
                        {
                            case "create":
                                subArgs = args.Length < 2 ? "" : args[2];
                                switch (subArgs)
                                {
                                    case "gradient":
                                        HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"Gradients are currently not supported. While we do plan to add support eventually, it is not planned. Please use static or sforce for the time being.");
                                        break;
                                    case "sforce":
                                        string argsfr = args.Length < 2 ? "" : args[3];
                                        string[] friendCodep = argsfr.Split(".");
                                        string friendcode = $"{friendCodep[0]}#{friendCodep[1]}";
                                        // tag create sforce friend-code hex toptext bottom text
                                        string shex = args.Length < 2 ? "" : args[4];
                                        string toptext = Utils.ReplaceCharWithSpace(args.Length < 2 ? "" : args[5], ".");
                                        string name = Utils.ReplaceCharWithSpace(args.Length < 2 ? "" : args[6], ".");
                                        if (!File.Exists(CustomTags.GetFilePath(friendcode)))
                                        {
                                            File.WriteAllText(CustomTags.GetFilePath(friendcode), $"type:{subArgs}\ncode:{friendcode}\ncolor:#{shex}\ntoptext:{toptext}\nname:{name}");
                                            HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"Your tag was successfully created! Info:\nType: {subArgs}\nFriend Code: {friendcode}\nColor: #{shex}\nAbove Tag Text: {toptext}\nTag Name: {name}");
                                        }
                                        else HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"A tag with that friend code already exists! Try another friend code! You looked for: {friendcode}");
                                        break;
                                    case "static":
                                        string argsfr2 = args.Length < 2 ? "" : args[3];
                                        string[] friendCodee = argsfr2.Split(".");
                                        string friendCodewe = $"{friendCodee[0]}#{friendCodee[1]}";
                                        // tag create sforce friend-code hex toptext
                                        string hex = args.Length < 2 ? "" : args[4];
                                        string abovename = args.Length < 2 ? "" : args[5];
                                        if (abovename.Contains("."))
                                            abovename = Utils.ReplaceCharWithSpace(abovename, ".");
                                        if (!File.Exists(CustomTags.GetFilePath(friendCodewe)))
                                        {
                                            File.WriteAllText(CustomTags.GetFilePath(friendCodewe), $"type:{subArgs}\ncode:{friendCodewe}\ncolor:#{hex}\ntext:{abovename}");
                                            HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"Your tag was successfully created! Info:\nType: {subArgs}\nFriend Code: {friendCodewe}\nColor: #{hex}\nTag Text: {abovename}");
                                        }
                                        else HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"A tag with that friend code already exists! Try another friend code! You looked for: {friendCodewe}");
                                        break;
                                    default:
                                        HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"Your specified type is incorrect. The only types are static, sforce, and gradient. You tried to search for: {subArgs}");
                                        break;
                                }
                                break;
                            case "remove":
                                subArgs = args.Length < 2 ? "" : args[2];
                                string[] friendCodeP = subArgs.Split(".");
                                string friendCode = $"{friendCodeP[0]}#{friendCodeP[1]}";
                                // tag remove friend-code
                                if (File.Exists(CustomTags.GetFilePath(friendCode)))
                                {
                                    HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"The tag of the friend code you specified was deleted! You removed the tag of {friendCode}.");
                                    File.Delete(CustomTags.GetFilePath(friendCode));
                                }
                                else
                                {
                                    HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"Your specified friend code is incorrect. There is no current tag with that friend code. You tried to search for: {friendCode}");
                                }
                                break;
                        }
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
                        foreach (var pc in AllPlayers)
                        {
                            string name = Main.devNames.ContainsKey(pc.PlayerId) ? Main.devNames[pc.PlayerId] : pc.GetRealName(true);
                            senttext += $"\n{name} : {pc.PlayerId}";
                        }
                        if (senttext != "") HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, senttext);
                        break;
                    case "/setimp":
                        canceled = true;
                        subArgs = args.Length < 2 ? "" : args[1];
                        Utils.SendMessage("Impostors set to " + subArgs);
                        var numberee = System.Convert.ToByte(subArgs);
                        GameOptionsManager.Instance.currentNormalGameOptions.NumImpostors = numberee;
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
                                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, GetString(subrole.ToString()) + GetString($"{subrole}InfoLong"));
                            if (Options.TosOptions.GetBool() && Options.AttackDefenseValues.GetBool())
                                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"Attack Value: {Utils.GetAttackEnum(role)}\nDefense Value: {Utils.GetDefenseEnum(role)}\nIf your Attack Value is higher than your target's defense value, you can kill them. If its the same or lower, then you can't kill them.");
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
                    case "/kick":
                        subArgs = args.Length < 2 ? "show" : args[1];
                        canceled = true;
                        if (subArgs == "show")
                        {
                            string sentttext = "";
                            List<PlayerControl> AlllPlayers = new();
                            sentttext += "All Players and their IDs (pick one):";
                            foreach (var pc in PlayerControl.AllPlayerControls)
                            {
                                if (pc == null || pc.Data.Disconnected) continue;
                                string name = Main.devNames.ContainsKey(pc.PlayerId) ? Main.devNames[pc.PlayerId] : pc.GetRealName(true);
                                sentttext += $"\n{name} : {pc.PlayerId}";
                            }
                            if (sentttext != "") HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, sentttext);
                        }
                        else
                        {
                            var kickplayerid = Convert.ToByte(subArgs);
                            AmongUsClient.Instance.KickPlayer(Utils.GetPlayerById(kickplayerid).GetClientId(), false);
                            string name = Main.devNames.ContainsKey(kickplayerid) ? Main.devNames[kickplayerid] : Utils.GetPlayerById(kickplayerid).GetRealName(true);
                            string texttosend = $"{name} was kicked.";
                            if (GameStates.IsInGame)
                            {
                                texttosend += $" Their role was {GetString(Utils.GetPlayerById(kickplayerid).GetCustomRole().ToString())}";
                            }
                            Utils.SendMessage(texttosend);
                        }
                        break;

                    case "/ban":
                        subArgs = args.Length < 2 ? "show" : args[1];
                        canceled = true;
                        if (subArgs == "show")
                        {
                            string sentttext = "";
                            List<PlayerControl> AlllPlayers = new();
                            sentttext += "All Players and their IDs (pick one):";
                            foreach (var pc in PlayerControl.AllPlayerControls)
                            {
                                if (pc == null || pc.Data.Disconnected) continue;
                                string name = Main.devNames.ContainsKey(pc.PlayerId) ? Main.devNames[pc.PlayerId] : pc.GetRealName(true);
                                sentttext += $"\n{name} : {pc.PlayerId}";
                            }
                            if (sentttext != "") HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, sentttext);
                        }
                        else
                        {
                            var banplayerid = Convert.ToByte(subArgs);
                            AmongUsClient.Instance.KickPlayer(Utils.GetPlayerById(banplayerid).GetClientId(), true);
                            string name = Main.devNames.ContainsKey(banplayerid) ? Main.devNames[banplayerid] : Utils.GetPlayerById(banplayerid).GetRealName(true);
                            string texttosend = $"{name} was kicked.";
                            if (GameStates.IsInGame)
                            {
                                texttosend += $" Their role was {GetString(Utils.GetPlayerById(banplayerid).GetCustomRole().ToString())}";
                            }
                            Utils.SendMessage(texttosend);
                        }
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
                        var gameManager = new GameManager();
                        switch (subArgs)
                        {
                            case "crewmate":
                                ShipStatus.Instance.enabled = false;
                                gameManager.RpcEndGame(GameOverReason.HumansDisconnect, false);
                                break;

                            case "impostor":
                                ShipStatus.Instance.enabled = false;
                                gameManager.RpcEndGame(GameOverReason.ImpostorDisconnect, false);
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
                            case "ak":
                                Utils.SendMessage("Auto Kick does the following features:\n1) Kicks Silenced people who Talk\n2) Kick anyone in Lobby who says \"Start\" and some other Variations\n3) Kicks People who use /color to kill themselves\n\nIf you want any more features to come here or having any other variations of start, suggest something over at discord!");
                                break;
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

                                    case "speedrun":
                                    case "sr":
                                        Utils.SendMessage(GetString("SpeedrunInfo"));
                                        break;

                                    default:
                                        Utils.SendMessage($"{GetString("Command.h_args")}:\n hideandseek(has), nogameend(nge), syncbuttonmode(sbm), randommapsmode(rmm), speedrun(sr)");
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
                        subArgs = args.Length < 2 ? "" : args[1];
                        if (!Main.CachedDevMode) break;
                        switch (subArgs)
                        {
                            case "crewmate":
                                Utils.SendMessage($"Host switched to role: {subArgs}");
                                PlayerControl.LocalPlayer.RpcSetCustomRole(CustomRoles.Crewmate);
                                PlayerControl.LocalPlayer.RpcSetRole(RoleTypes.Crewmate);
                                RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Crewmate);
                                break;
                            case "impostor":
                                Utils.SendMessage($"Host switched to role: {subArgs}");
                                PlayerControl.LocalPlayer.RpcSetCustomRole(CustomRoles.Impostor);
                                PlayerControl.LocalPlayer.RpcSetRole(RoleTypes.Impostor);
                                RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Impostor);
                                break;
                            case "engineer":
                                Utils.SendMessage($"Host switched to role: {subArgs}");
                                PlayerControl.LocalPlayer.RpcSetCustomRole(CustomRoles.Engineer);
                                PlayerControl.LocalPlayer.RpcSetRole(RoleTypes.Engineer);
                                RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Engineer);
                                break;
                            case "scientist":
                                Utils.SendMessage($"Host switched to role: {subArgs}");
                                PlayerControl.LocalPlayer.RpcSetCustomRole(CustomRoles.Scientist);
                                PlayerControl.LocalPlayer.RpcSetRole(RoleTypes.Scientist);
                                RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Scientist);
                                break;
                            case "shapeshifter":
                                Utils.SendMessage($"Host switched to role: {subArgs}");
                                PlayerControl.LocalPlayer.RpcSetCustomRole(CustomRoles.Shapeshifter);
                                PlayerControl.LocalPlayer.RpcSetRole(RoleTypes.Shapeshifter);
                                RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Shapeshifter);
                                break;
                            case "crewghost":
                                Utils.SendMessage($"Host switched to role: {subArgs}");
                                PlayerControl.LocalPlayer.RpcSetCustomRole(CustomRoles.CrewmateGhost);
                                PlayerControl.LocalPlayer.RpcSetRole(RoleTypes.CrewmateGhost);
                                RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.CrewmateGhost);
                                break;
                            case "impghost":
                                Utils.SendMessage($"Host switched to role: {subArgs}");
                                PlayerControl.LocalPlayer.RpcSetCustomRole(CustomRoles.ImpostorGhost);
                                PlayerControl.LocalPlayer.RpcSetRole(RoleTypes.ImpostorGhost);
                                RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.ImpostorGhost);
                                break;
                            case "ga":
                                Utils.SendMessage($"Host switched to role: {subArgs}");
                                PlayerControl.LocalPlayer.RpcSetCustomRole(CustomRoles.GuardianAngel);
                                PlayerControl.LocalPlayer.RpcSetRole(RoleTypes.GuardianAngel);
                                RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.GuardianAngel);
                                break;
                            default:
                                Utils.SendMessage($"Host switched to role: {subArgs}/crewmates");
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
                { CustomRoles.SerialKiller, "mc" },
                //{ CustomRoles.ShapeMaster, "sha" },
                { CustomRoles.TimeThief, "tt"},
                { CustomRoles.VoteStealer, "pi"},
                { CustomRoles.Sniper, "snp" },
                { CustomRoles.Puppeteer, "pup" },
                { CustomRoles.Escapist, "esc" },
                { CustomRoles.Disperser, "dis" },
                { CustomRoles.Vampire, "va" },
                { CustomRoles.Warlock, "wa" },
                { CustomRoles.Witch, "wi" },
                  { CustomRoles.Consort, "con" },
                { CustomRoles.Freezer, "fre" },
                { CustomRoles.Bomber, "bb" },
                { CustomRoles.Cleaner, "cle" },
                { CustomRoles.Silencer, "si" },
                { CustomRoles.IdentityTheft, "idth"},
                { CustomRoles.Ninja,"ni"},
                { CustomRoles.Miner,"mi"},
                { CustomRoles.Manipulator, "mani"},
                { CustomRoles.YingYanger,"yy"},
                { CustomRoles.Camouflager,"cf"},
                { CustomRoles.Swooper,"sp"},
                { CustomRoles.Morphling, "mor" },
                { CustomRoles.Grenadier,"gr"},
                { CustomRoles.CorruptedSheriff, "tra" },
                { CustomRoles.EvilGuesser, "eg"},
                //Madmate役職
                { (CustomRoles)(-2), $"== {GetString("Madmate")} ==" }, //区切り用
                { CustomRoles.MadGuardian, "mg" },
                { CustomRoles.Madmate, "mm" },
                { CustomRoles.MadSnitch, "msn" },
                { CustomRoles.SKMadmate, "sm" },
                { CustomRoles.Parasite, "pa" },
                //両陣営役職
                { (CustomRoles)(-3), $"== {GetString("Impostor")} or {GetString("Crewmate")} ==" }, //区切り用
                { CustomRoles.CrewPostor, "cp" },
                //Crewmate役職
                { (CustomRoles)(-4), $"== {GetString("Crewmate")} ==" }, //区切り用
                { CustomRoles.Dictator, "dic" },
                { CustomRoles.Child, "cd" },
                { CustomRoles.Medium, "med" },
                { CustomRoles.Psychic, "psy" },
                { CustomRoles.Doctor, "doc" },
                { CustomRoles.Mechanic, "mec" },
                { CustomRoles.Physicist, "phy" },
                { CustomRoles.Lighter, "li" },
                { CustomRoles.Mayor, "my" },
                { CustomRoles.Bodyguard, "bd" },
                { CustomRoles.Oracle, "or" },
                { CustomRoles.Medic, "me" },
                { CustomRoles.Crusader, "cru" },
                { CustomRoles.Escort, "esc" },
                { CustomRoles.Veteran, "vet" },
                { CustomRoles.Transporter, "tr" },
                { CustomRoles.SabotageMaster, "sa" },
                { CustomRoles.NiceGuesser, "vigi"},
                { CustomRoles.Sheriff, "sh" },
                { CustomRoles.Investigator, "inve" },
                { CustomRoles.Mystic,"ms"},
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
                { CustomRoles.Swapper, "sw" },
                { CustomRoles.Jester, "je" },
                { CustomRoles.Phantom, "ph" },
                { CustomRoles.Opportunist, "op" },
                { CustomRoles.Hitman, "hn" },
                { CustomRoles.Survivor, "sur" },
                { CustomRoles.SchrodingerCat, "sc" },
                { CustomRoles.Postman, "ptm" },
                { CustomRoles.Pirate, "pi"},
                { CustomRoles.Marksman, "mar" },
                { CustomRoles.Terrorist, "te" },
                { CustomRoles.Jackal, "jac" },
                { CustomRoles.Sidekick, "jacsk" },
                { CustomRoles.NeutWitch, "nwi" },
                //{ CustomRoles.Juggernaut, "jn"},
                { CustomRoles.PlagueBearer, "pb" },
                { CustomRoles.AgiTater, "agt" },
                { CustomRoles.Pestilence, "pesti" },
                { CustomRoles.Juggernaut, "jug"},
                { CustomRoles.Vulture, "vu"},
                { CustomRoles.Coven, "co" },
                { CustomRoles.CovenWitch, "cw" },
                { CustomRoles.Poisoner, "poison" },
                { CustomRoles.HexMaster, "hm" },
                { CustomRoles.Medusa, "medu" },
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
                { CustomRoles.DoubleShot, "ds" },
                { CustomRoles.Obvious, "obv" },
                { CustomRoles.Torch, "to" },
                { CustomRoles.Flash, "fl" },
                { CustomRoles.Bewilder, "be" },
                { CustomRoles.TieBreaker, "tb" },
                { CustomRoles.Watcher, "wat" },
                { CustomRoles.Diseased, "di" },
                //HAS
                { (CustomRoles)(-7), $"== {GetString("HideAndSeek")} ==" }, //区切り用
                { CustomRoles.HASFox, "hfo" },
                { CustomRoles.HASTroll, "htr" },
                { CustomRoles.Supporter, "wor" },
                { CustomRoles.Janitor, "jan" },
                { CustomRoles.Painter, "pan" },
                { CustomRoles.Tasker, "tas" },

            };
            var msg = "";
            var rolemsg = $"{GetString("Command.h_args")}";
            foreach (var r in roleList)
            {
                //var roleName = Utils.ReplaceCharWithSpace(Utils.GetRoleName(r.Key).ToLower(), "");
                var roleName = GetString(r.Key.ToString()).ToLower();
                //roleName = roleName.Replace(" ", "-");
                var roleShort = r.Value;

                if (String.Compare(role, roleName, true) == 0 || String.Compare(role, roleShort, true) == 0 ||
                    role == "vampiress" || role == "escalation" || role == "creeper")
                {
                    roleName = r.Key.ToString();
                    if (role == "vampiress")
                        roleName = CustomRoles.Vampress.ToString();
                    if (role == "escalation")
                        roleName = CustomRoles.Escalation.ToString();
                    if (role == "creeper")
                        roleName = CustomRoles.Creeper.ToString();
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
                { CustomRoles.Escapist, "esc" },
                { CustomRoles.Disperser, "dis" },
                { CustomRoles.Vampire, "va" },
                { CustomRoles.Warlock, "wa" },
                { CustomRoles.Consort, "con" },
                { CustomRoles.Witch, "wi" },
                { CustomRoles.Freezer, "fre" },
                { CustomRoles.Bomber, "bb" },
                { CustomRoles.Cleaner, "cle" },
                { CustomRoles.Silencer, "si" },
                { CustomRoles.Camouflager,"cf"},
                { CustomRoles.Swooper,"sp"},
                { CustomRoles.Ninja,"ni"},
                { CustomRoles.Grenadier,"gr"},
                { CustomRoles.Miner,"mi"},
                { CustomRoles.Morphling, "mor" },
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
                { CustomRoles.CrewPostor, "cp" },
                //Crewmate役職
                { (CustomRoles)(-4), $"== {GetString("Crewmate")} ==" }, //区切り用
                { CustomRoles.Dictator, "dic" },
                { CustomRoles.Child, "cd" },
                { CustomRoles.Medium, "med" },
                { CustomRoles.Psychic, "psy" },
                { CustomRoles.Doctor, "doc" },
                { CustomRoles.Mechanic, "mec" },
                { CustomRoles.Physicist, "phy" },
                { CustomRoles.Lighter, "li" },
                { CustomRoles.Mayor, "my" },
                { CustomRoles.Veteran, "vet" },
                { CustomRoles.Bodyguard, "bd" },
                { CustomRoles.Oracle, "or" },
                { CustomRoles.Medic, "me" },
                { CustomRoles.Crusader, "cru" },
                { CustomRoles.Escort, "esc" },
                { CustomRoles.Veteran, "vet" },
                { CustomRoles.Transporter, "tr" },
                { CustomRoles.SabotageMaster, "sa" },
                { CustomRoles.Sheriff, "sh" },
                {CustomRoles.NiceGuesser, "ng"},
                { CustomRoles.Investigator, "inve" },
                { CustomRoles.Mystic,"ms"},
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
                { CustomRoles.Swapper, "sw" },
                { CustomRoles.Jester, "je" },
                { CustomRoles.Phantom, "ph" },
                { CustomRoles.Hitman, "hn" },
                { CustomRoles.Opportunist, "op" },
                { CustomRoles.Survivor, "sur" },
                { CustomRoles.SchrodingerCat, "sc" },
                { CustomRoles.Postman, "ptm" },
                { CustomRoles.Terrorist, "te" },
                { CustomRoles.Marksman, "mar" },
                { CustomRoles.Jackal, "jac" },
                { CustomRoles.Sidekick, "jacsk" },
                //{ CustomRoles.Juggernaut, "jn"},
                { CustomRoles.NeutWitch, "nwi" },
                { CustomRoles.PlagueBearer, "pb" },
                { CustomRoles.Pestilence, "pesti" },
                { CustomRoles.Juggernaut, "jug"},
                { CustomRoles.Vulture, "vu"},
                { CustomRoles.Coven, "co" },
                { CustomRoles.CovenWitch, "cw" },
                { CustomRoles.Poisoner, "poison" },
                { CustomRoles.HexMaster, "hm" },
                { CustomRoles.Medusa, "medu" },
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
                { CustomRoles.DoubleShot, "ds" },
                { CustomRoles.Obvious, "obv" },
                { CustomRoles.Torch, "to" },
                { CustomRoles.Flash, "fl" },
                { CustomRoles.Bewilder, "be" },
                { CustomRoles.TieBreaker, "tb" },
                { CustomRoles.Watcher, "wat" },
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
        public static void SendTemplate(string str = "", byte playerId = 0xff, bool noErr = false)
        {
            if (!File.Exists(Main.TEMPLATE_FILE_PATH))
            {
                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, "No template file found.");
                File.WriteAllText(Main.TEMPLATE_FILE_PATH, "test:This is template text.\\nLine breaks are also possible.\ntest:これは定型文です。\\n改行も可能です。");
                return;
            }
            using StreamReader sr = new(Main.TEMPLATE_FILE_PATH, Encoding.GetEncoding("UTF-8"));
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
        public static List<string> ReturnAllNewLinesInFile(string filename, byte playerId = 0xff, bool noErr = false)
        {
            // Logger.Info($"Checking lines in directory {filename}.", "ReturnAllNewLinesInFile (ChatCommands)");
            if (!File.Exists(filename))
            {
                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"No {filename} file found.");
                File.WriteAllText(filename, "Enter the desired stuff here.");
                return new List<string>();
            }
            using StreamReader sr = new(filename, Encoding.GetEncoding("UTF-8"));
            string text;
            string[] tmp = { };
            List<string> sendList = new();
            HashSet<string> tags = new();
            while ((text = sr.ReadLine()) != null)
            {
                if (text.Length > 1 && text != "")
                {
                    tags.Add(text.ToLower());
                    sendList.Add(text.Join(delimiter: "").Replace("\\n", "\n").ToLower());
                }
            }
            if (sendList.Count == 0 && !noErr)
            {
                if (playerId == 0xff)
                    HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, string.Format(GetString("Message.TemplateNotFoundHost"), Main.BANNEDWORDS_FILE_PATH, tags.Join(delimiter: ", ")));
                else Utils.SendMessage(string.Format(GetString("Message.TemplateNotFoundClient"), Main.BANNEDWORDS_FILE_PATH), playerId);
                return new List<string>();
            }
            else
            {
                return sendList;
            }
        }
        public static void OnReceiveChat(PlayerControl player, string text)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (Main.SilencedPlayer.Count != 0)
            {
                //someone is silenced
                foreach (var p in Main.SilencedPlayer)
                {
                    if (player.PlayerId != p.PlayerId) continue;
                    if (!player.Data.IsDead)
                    {
                        text = "Silenced.";
                        Logger.Info($"{p.GetNameWithRole()}:{text}", "Tried To Send Chat But Silenced");
                        Utils.SendMessage("You are currently Silenced. Try talking again when you aren't silenced.", player.PlayerId);
                        if (Options.AutoKick.GetBool())
                        {
                            AmongUsClient.Instance.KickPlayer(player.GetClientId(), Options.BanInsteadOfKick.GetBool());
                            Utils.BlockCommand(18);
                        }
                    }
                }
            }
            var message = text.ToLower();
            if (message.ContainsStart() && GameStates.IsLobby)
            {
                if (Options.AutoKick.GetBool())
                {
                    string name = Main.devNames.ContainsKey(player.PlayerId) ? Main.devNames[player.PlayerId] : player.GetRealName(true);
                    Utils.SendMessage($"{name} was kicked for saying start.");
                    AmongUsClient.Instance.KickPlayer(player.GetClientId(), Options.BanInsteadOfKick.GetBool());
                }
            }
            string[] args = text.Split(' ');
            var list = ReturnAllNewLinesInFile(Main.BANNEDWORDS_FILE_PATH, noErr: true);
            bool banned = false;
            foreach (var area in args)
            {
                if (list.Contains(area) && AmongUsClient.Instance.AmHost && !banned)
                {
                    banned = true;
                    var msg = "(Could not find specific message.)";
                    foreach (var txt in list)
                    {
                        if (txt.ToLower() == area.ToLower())
                            msg = txt;
                    }
                    string name = Main.devNames.ContainsKey(player.PlayerId) ? Main.devNames[player.PlayerId] : player.GetRealName(true);
                    Utils.SendMessage($"{name} said a word blocked by this host. The host may add, remove or change blocked words at will. The blocked word was {msg}.");
                    AmongUsClient.Instance.KickPlayer(player.GetClientId(), Options.BanInsteadOfKick.GetBool());
                    Logger.Msg($"{name} said a word blocked by this host. The host may add, remove or change blocked words at will. The blocked word was {msg}.", "Blocked Word");
                }
            }
            string subArgs = "";
            switch (args[0])
            {
                case "/l":
                case "/lastresult":
                    Utils.ShowLastResult(player.PlayerId);
                    break;

                case "/name":
                    if (Options.Customise.GetBool() | Main.devNames.ContainsKey(player.PlayerId))
                    {
                        var canRename = true;
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc.Data.PlayerName == args[1])
                                canRename = false;
                        }
                        if (canRename)
                            player.Data.PlayerName = (args.Length > 1 && args.Length <= 20) ? player.Data.PlayerName = args[1] : "";
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
                case "/m":
                case "/myrole":
                    var role = player.GetCustomRole();
                    var subrole = player.GetCustomSubRole();
                    if (GameStates.IsInGame)
                    {
                        if (role.IsVanilla()) Utils.SendMessage("Vanilla roles currently have no description.", player.PlayerId);
                        else Utils.SendMessage(GetString(role.ToString()) + GetString($"{role}InfoLong"), player.PlayerId);
                        if (subrole != CustomRoles.NoSubRoleAssigned)
                            Utils.SendMessage(GetString(subrole.ToString()) + GetString($"{subrole}InfoLong"), player.PlayerId);
                        if (Options.TosOptions.GetBool() && Options.AttackDefenseValues.GetBool())
                            Utils.SendMessage($"Attack Value: {Utils.GetAttackEnum(role)}\nDefense Value: {Utils.GetDefenseEnum(role)}If your Attack Value is higher than your target's defense value, you can kill them. If its the same or lower, then you can't kill them.", player.PlayerId);
                    }
                    else { Utils.SendMessage("Sorry, you can only use this command inside the game.", player.PlayerId); }
                    break;
                case "/level":
                    if (Options.Customise.GetBool() | Main.devNames.ContainsKey(player.PlayerId))
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
                    if (Options.Customise.GetBool() | Main.devNames.ContainsKey(player.PlayerId))
                    {
                        subArgs = args.Length < 2 ? "" : args[1];
                        Utils.SendMessage("Color ID set to " + subArgs, player.PlayerId);
                        var numbere = System.Convert.ToByte(subArgs);
                        player.RpcSetColor(numbere);
                    }
                    else { Utils.SendMessage("The host has currently disabled access to this command.\nTry again when this command is enabled.", player.PlayerId); }
                    break;
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
            if (Main.SilencedPlayer.Contains(player)) return;
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
