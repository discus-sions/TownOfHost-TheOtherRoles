using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Hazel;
using UnityEngine;
using static TownOfHost.Translator;

namespace TownOfHost
{
    public static class Utils
    {
        public static bool IsActive(SystemTypes type)
        {
            var SwitchSystem = ShipStatus.Instance.Systems[type].Cast<SwitchSystem>();
            Logger.Info($"SystemTypes:{type}", "SwitchSystem");
            return SwitchSystem != null && SwitchSystem.IsActive;
        }
        public static void SetVision(this GameOptionsData opt, PlayerControl player, bool HasImpVision)
        {
            if (HasImpVision)
            {
                opt.CrewLightMod = opt.ImpostorLightMod;
                if (IsActive(SystemTypes.Electrical))
                    opt.CrewLightMod *= 5;
                return;
            }
            else
            {
                opt.ImpostorLightMod = opt.CrewLightMod;
                if (IsActive(SystemTypes.Electrical))
                    opt.ImpostorLightMod /= 5;
                return;
            }
        }
        public static string GetOnOff(bool value) => value ? "ON" : "OFF";
        public static int SetRoleCountToggle(int currentCount) => currentCount > 0 ? 0 : 1;
        public static void SetRoleCountToggle(CustomRoles role)
        {
            int count = Options.GetRoleCount(role);
            count = SetRoleCountToggle(count);
            Options.SetRoleCount(role, count);
        }
        public static string GetRoleName(CustomRoles role)
        {
            var CurrentLanguage = TranslationController.Instance.currentLanguage.languageID;
            var lang = CurrentLanguage;
            if (Main.ForceJapanese.Value && Main.JapaneseRoleName.Value)
                lang = SupportedLangs.Japanese;
            else if (CurrentLanguage == SupportedLangs.Japanese && !Main.JapaneseRoleName.Value)
                lang = SupportedLangs.English;
            return GetRoleName(role, lang);
        }
        public static string GetRoleName(CustomRoles role, SupportedLangs lang)
        {
            return GetString(Enum.GetName(typeof(CustomRoles), role), lang);
        }
        public static string GetDeathReason(PlayerState.DeathReason status)
        {
            return GetString("DeathReason." + Enum.GetName(typeof(PlayerState.DeathReason), status));
        }
        public static Color GetRoleColor(CustomRoles role)
        {
            if (!Main.roleColors.TryGetValue(role, out var hexColor)) hexColor = "#ffffff";
            ColorUtility.TryParseHtmlString(hexColor, out Color c);
            return c;
        }
        public static string GetRoleColorCode(CustomRoles role)
        {
            if (!Main.roleColors.TryGetValue(role, out var hexColor)) hexColor = "#ffffff";
            return hexColor;
        }
        public static (string, Color) GetRoleText(PlayerControl player)
        {
            string RoleText = "Invalid Role";
            Color TextColor = Color.red;

            var cRole = player.GetCustomRole();
            /*if (player.isLastImpostor())
            {
                RoleText = $"{getRoleName(cRole)} ({getString("Last")})";
            }
            else*/
            RoleText = GetRoleName(cRole);

            return (RoleText, GetRoleColor(cRole));
        }

        public static string GetVitalText(byte player) =>
            PlayerState.isDead[player] ? GetString("DeathReason." + PlayerState.GetDeathReason(player)) : GetString("Alive");
        public static (string, Color) GetRoleTextHideAndSeek(RoleTypes oRole, CustomRoles hRole)
        {
            string text = "Invalid";
            Color color = Color.red;
            switch (oRole)
            {
                case RoleTypes.Impostor:
                case RoleTypes.Shapeshifter:
                    text = "Impostor";
                    color = Palette.ImpostorRed;
                    break;
                default:
                    switch (hRole)
                    {
                        case CustomRoles.Crewmate:
                            text = "Crewmate";
                            color = Color.white;
                            break;
                        case CustomRoles.HASFox:
                            text = "Fox";
                            color = Color.magenta;
                            break;
                        case CustomRoles.HASTroll:
                            text = "Troll";
                            color = Color.green;
                            break;
                    }
                    break;
            }
            return (text, color);
        }

        public static bool HasTasks(GameData.PlayerInfo p, bool ForRecompute = true)
        {
            //Tasksがnullの場合があるのでその場合タスク無しとする
            if (p.Tasks == null) return false;
            if (p.Role == null) return false;

            var hasTasks = true;
            if (p.Disconnected) hasTasks = false;
            if (p.Role.IsImpostor)
                hasTasks = false; //タスクはCustomRoleを元に判定する
            if (Options.CurrentGameMode() == CustomGameMode.HideAndSeek)
            {
                if (p.IsDead && !Options.SplatoonOn.GetBool()) hasTasks = false;
                var hasRole = Main.AllPlayerCustomRoles.TryGetValue(p.PlayerId, out var role);
                if (hasRole)
                {
                    if (role is CustomRoles.HASFox or CustomRoles.HASTroll or CustomRoles.Painter or CustomRoles.Janitor) hasTasks = false;
                }
            }
            else
            {
                var cRoleFound = Main.AllPlayerCustomRoles.TryGetValue(p.PlayerId, out var cRole);
                if (CustomRolesHelper.IsCoven(p.GetCustomRole())) hasTasks = false;
                if (cRoleFound)
                {
                    if (cRole == CustomRoles.GM) hasTasks = false;
                    if (cRole == CustomRoles.Jester) hasTasks = false;
                    if (cRole == CustomRoles.MadGuardian && ForRecompute) hasTasks = false;
                    if (cRole == CustomRoles.MadSnitch && ForRecompute) hasTasks = false;
                    if (cRole == CustomRoles.Opportunist) hasTasks = false;
                    if (cRole == CustomRoles.Survivor && ForRecompute) hasTasks = false;
                    if (cRole == CustomRoles.Sheriff) hasTasks = false;
                    if (cRole == CustomRoles.CorruptedSheriff) hasTasks = false;
                    if (cRole == CustomRoles.Investigator) hasTasks = false;
                    if (cRole == CustomRoles.Amnesiac && ForRecompute) hasTasks = false;
                    if (cRole == CustomRoles.Madmate) hasTasks = false;
                    if (cRole == CustomRoles.SKMadmate) hasTasks = false;
                    if (cRole == CustomRoles.Terrorist && ForRecompute) hasTasks = false;
                    if (cRole == CustomRoles.Executioner) hasTasks = false;
                    if (cRole == CustomRoles.Impostor) hasTasks = false;
                    if (cRole == CustomRoles.Shapeshifter) hasTasks = false;
                    if (cRole == CustomRoles.Arsonist) hasTasks = false;
                    if (cRole == CustomRoles.Parasite) hasTasks = false;
                    if (cRole == CustomRoles.SchrodingerCat) hasTasks = false;
                    if (cRole == CustomRoles.CSchrodingerCat) hasTasks = false;
                    if (cRole == CustomRoles.MSchrodingerCat) hasTasks = false;
                    if (cRole == CustomRoles.EgoSchrodingerCat) hasTasks = false;
                    if (cRole == CustomRoles.JSchrodingerCat) hasTasks = false;
                    if (cRole == CustomRoles.Egoist) hasTasks = false;
                    if (cRole == CustomRoles.Jackal) hasTasks = false;
                    if (cRole == CustomRoles.Sidekick) hasTasks = false;
                    if (cRole == CustomRoles.Juggernaut) hasTasks = false;
                    if (cRole == CustomRoles.PlagueBearer) hasTasks = false;
                    if (cRole == CustomRoles.Pestilence) hasTasks = false;
                    if (cRole == CustomRoles.Coven) hasTasks = false;
                    if (cRole == CustomRoles.Vulture) hasTasks = false;
                    if (cRole == CustomRoles.GuardianAngelTOU) hasTasks = false;
                    if (cRole == CustomRoles.Werewolf) hasTasks = false;
                    if (cRole == CustomRoles.TheGlitch) hasTasks = false;
                    if (cRole == CustomRoles.Hacker) hasTasks = false;
                    if (cRole == CustomRoles.BloodKnight) hasTasks = false;
                    if (cRole == CustomRoles.Marksman) hasTasks = false;
                    if (cRole == CustomRoles.Pirate) hasTasks = false;

                    if (cRole == CustomRoles.CrewPostor && ForRecompute) hasTasks = false;
                    if (cRole == CustomRoles.Phantom && ForRecompute) hasTasks = false;

                    if (cRole == CustomRoles.CovenWitch) hasTasks = false;
                    if (cRole == CustomRoles.HexMaster) hasTasks = false;
                    if (cRole == CustomRoles.PotionMaster) hasTasks = false;
                    if (cRole == CustomRoles.Medusa) hasTasks = false;
                    if (cRole == CustomRoles.Mimic) hasTasks = false;
                    if (cRole == CustomRoles.Conjuror) hasTasks = false;
                    if (cRole == CustomRoles.Necromancer) hasTasks = false;
                    if (cRole == CustomRoles.Poisoner) hasTasks = false;
                }
                var cSubRoleFound = Main.AllPlayerCustomSubRoles.TryGetValue(p.PlayerId, out var cSubRole);
                if (cSubRoleFound)
                {

                }
            }
            return hasTasks;
        }

        public static string GetProgressText(PlayerControl pc)
        {
            if (!Main.playerVersion.ContainsKey(0)) return ""; //ホストがMODを入れていなければ未記入を返す
            var taskState = pc.GetPlayerTaskState();
            var Comms = false;
            if (taskState.hasTasks)
            {
                foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
                    if (task.TaskType == TaskTypes.FixComms)
                    {
                        Comms = true;
                        break;
                    }
            }
            return GetProgressText(pc.PlayerId, Comms);
        }
        public static string GetProgressText(byte playerId, bool comms = false)
        {
            if (!Main.playerVersion.ContainsKey(0)) return ""; //ホストがMODを入れていなければ未記入を返す
            if (!Main.AllPlayerCustomRoles.TryGetValue(playerId, out var role)) return Helpers.ColorString(Color.yellow, "Invalid");
            string ProgressText = "";
            bool checkTasks = false;
            switch (role)
            {
                case CustomRoles.Jackal:
                case CustomRoles.Amnesiac:
                    ProgressText = "";
                    break;
                case CustomRoles.Arsonist:
                    var doused = GetDousedPlayerCount(playerId);
                    ProgressText = Helpers.ColorString(GetRoleColor(CustomRoles.Arsonist), $"({doused.Item1}/{doused.Item2})");
                    break;
                case CustomRoles.HexMaster:
                    var hexed = GetHexedPlayerCount(playerId);
                    ProgressText = Helpers.ColorString(GetRoleColor(CustomRoles.Coven), $"({hexed.Item1}/{hexed.Item2})");
                    break;
                case CustomRoles.PlagueBearer:
                    var infected = GetInfectedPlayerCount(playerId);
                    ProgressText = Helpers.ColorString(GetRoleColor(CustomRoles.Pestilence), $"({infected.Item1}/{infected.Item2})");
                    break;
                case CustomRoles.Sheriff:
                    ProgressText += Sheriff.GetShotLimit(playerId);
                    break;
                case CustomRoles.Survivor:
                    var stuff = Main.SurvivorStuff[playerId];
                    ProgressText = Helpers.ColorString(GetRoleColor(CustomRoles.Survivor), $"({stuff.Item1}/{Options.NumOfVests.GetInt()})");
                    break;
                case CustomRoles.Pirate:
                    ProgressText = Helpers.ColorString(GetRoleColor(CustomRoles.Pirate), $"({Guesser.PirateGuess[playerId]}/{Guesser.PirateGuessAmount.GetInt()})");
                    break;
                case CustomRoles.Veteran:
                    ProgressText += Helpers.ColorString(GetRoleColor(CustomRoles.Veteran), $"({Main.VetAlerts}/{Options.NumOfVets.GetInt()})");
                    checkTasks = true;
                    break;
                case CustomRoles.GuardianAngelTOU:
                    ProgressText += Helpers.ColorString(GetRoleColor(CustomRoles.GuardianAngelTOU), $"({Main.ProtectsSoFar}/{Options.NumOfProtects.GetInt()})");
                    break;
                case CustomRoles.Sniper:
                    ProgressText += $" {Sniper.GetBulletCount(playerId)}";
                    break;
                case CustomRoles.Vulture:
                    ProgressText = Helpers.ColorString(GetRoleColor(CustomRoles.Vulture), $"({Main.AteBodies}/{Options.BodiesAmount.GetInt()})");
                    break;
                case CustomRoles.Hacker:
                    ProgressText = Helpers.ColorString(GetRoleColor(CustomRoles.Hacker), $"({Main.HackerFixedSaboCount[playerId]}/{Options.SaboAmount.GetInt()})");
                    break;
                default:
                    //タスクテキスト
                    checkTasks = true;
                    break;
            }
            if (checkTasks)
            {
                var taskState = PlayerState.taskState?[playerId];
                if (taskState.hasTasks)
                {
                    string Completed = comms ? "?" : $"{taskState.CompletedTasksCount}";
                    ProgressText += Helpers.ColorString(Color.yellow, $"({Completed}/{taskState.AllTasksCount})");
                    if (role == CustomRoles.CrewPostor)
                    {
                        int amount = Main.lastAmountOfTasks[playerId];

                        if (taskState.CompletedTasksCount != amount) // new task completed //
                        {
                            Main.lastAmountOfTasks[playerId] = taskState.CompletedTasksCount;
                            var cp = GetPlayerById(playerId);
                            if (!cp.Data.IsDead)
                            {
                                Vector2 cppos = cp.transform.position;//呪われた人の位置
                                Dictionary<PlayerControl, float> cpdistance = new();
                                float dis;
                                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                                {
                                    if (!p.Data.IsDead && p != cp)
                                    {
                                        dis = Vector2.Distance(cppos, p.transform.position);
                                        cpdistance.Add(p, dis);
                                        Logger.Info($"{p?.Data?.PlayerName}の位置{dis}", "CrewPostor");
                                    }
                                }
                                var min = cpdistance.OrderBy(c => c.Value).FirstOrDefault();//一番小さい値を取り出す
                                PlayerControl targetw = min.Key;
                                Logger.Info($"{targetw.GetNameWithRole()}was killed", "CrewPostor");
                                if (targetw.Is(CustomRoles.Pestilence))
                                    targetw.RpcMurderPlayerV2(cp);
                                else
                                    cp.RpcMurderPlayerV2(targetw);//殺す
                                cp.RpcGuardAndKill(cp);
                            }
                        }
                    }
                    else if (role == CustomRoles.Phantom && !GetPlayerById(playerId).Data.IsDead)
                    {
                        int amount = Main.lastAmountOfTasks[playerId];
                        int remaining = taskState.AllTasksCount - taskState.CompletedTasksCount;

                        if (taskState.CompletedTasksCount != amount) // new task completed //
                        {
                            Main.lastAmountOfTasks[playerId] = taskState.CompletedTasksCount;
                            if (taskState.CompletedTasksCount == taskState.AllTasksCount)
                            {
                                // PHANTOM WINS //
                                var phantom = GetPlayerById(playerId);
                                phantom.RpcMurderPlayer(phantom);
                                PlayerState.SetDeathReason(playerId, PlayerState.DeathReason.Alive);
                                var endReason = TempData.LastDeathReason switch
                                {
                                    DeathReason.Exile => GameOverReason.ImpostorByVote,
                                    DeathReason.Kill => GameOverReason.ImpostorByKill,
                                    _ => GameOverReason.ImpostorByVote,
                                };
                                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EndGame, Hazel.SendOption.Reliable, -1);
                                writer.Write((byte)CustomWinner.Phantom);
                                AmongUsClient.Instance.FinishRpcImmediately(writer);
                                RPC.PhantomWin(playerId);
                                foreach (var pc in PlayerControl.AllPlayerControls)
                                {
                                    pc.RpcSetRole(RoleTypes.GuardianAngel);
                                }
                                new LateTask(() =>
                                {
                                    ShipStatus.RpcEndGame(endReason, false);
                                }, 0.5f, "EndGameTaskForPhantom");
                            }
                            if (remaining == Options.TasksRemainingForPhantomClicked.GetInt())
                            {
                                Main.PhantomCanBeKilled = true;
                            }
                            if (remaining == Options.TasksRemaningForPhantomAlert.GetInt())
                            {
                                Main.PhantomAlert = true;
                            }
                        }
                    }
                }
            }
            if (role.IsImpostor() && role != CustomRoles.LastImpostor && GetPlayerById(playerId).IsLastImpostor())
            {
                ProgressText += $" <color={GetRoleColorCode(CustomRoles.Impostor)}>(Last)</color>";
            }
            if (GetPlayerById(playerId).CanMakeMadmate()) ProgressText += $" [{Options.CanMakeMadmateCount.GetInt() - Main.SKMadmateNowCount}]";

            foreach (var TargetGA in Main.GuardianAngelTarget)
            {
                if (Options.TargetKnowsGA.GetBool())
                {
                    if (playerId == TargetGA.Value)
                        ProgressText += $"<color={Utils.GetRoleColorCode(CustomRoles.GuardianAngelTOU)}>♦</color>";
                }
            }

            return ProgressText;
        }
        public static void ShowActiveSettingsHelp()
        {
            SendMessage(GetString("CurrentActiveSettingsHelp") + ":");
            if (Options.CurrentGameMode() == CustomGameMode.HideAndSeek)
            {
                if (!Options.SplatoonOn.GetBool())
                {
                    SendMessage(GetString("HideAndSeekInfo"));
                    if (CustomRoles.HASFox.IsEnable()) { SendMessage(GetRoleName(CustomRoles.HASFox) + GetString("HASFoxInfoLong")); }
                    if (CustomRoles.HASTroll.IsEnable()) { SendMessage(GetRoleName(CustomRoles.HASTroll) + GetString("HASTrollInfoLong")); }
                }
                else
                {
                    //SendMessage(GetString("HideAndSeekInfo"));
                    if (CustomRoles.Supporter.IsEnable()) { SendMessage(GetRoleName(CustomRoles.Supporter) + GetString("SupporterInfoLong")); }
                    if (CustomRoles.Janitor.IsEnable()) { SendMessage(GetRoleName(CustomRoles.Janitor) + GetString("JanitorInfoLong")); }
                }
            }
            else
            {
                if (Options.DisableDevices.GetBool()) { SendMessage(GetString("DisableDevicesInfo")); }
                if (Options.SyncButtonMode.GetBool()) { SendMessage(GetString("SyncButtonModeInfo")); }
                if (Options.SabotageTimeControl.GetBool()) { SendMessage(GetString("SabotageTimeControlInfo")); }
                if (Options.RandomMapsMode.GetBool()) { SendMessage(GetString("RandomMapsModeInfo")); }
                if (Options.IsStandardHAS) { SendMessage(GetString("StandardHASInfo")); }
                if (Options.CamoComms.GetBool()) { SendMessage(GetString("CamoCommsInfo")); }
                // if (Options.EnableGM.GetBool()) { SendMessage(GetRoleName(CustomRoles.GM) + GetString("GMInfoLong")); }
                foreach (var role in Enum.GetValues(typeof(CustomRoles)).Cast<CustomRoles>())
                {
                    if (role is CustomRoles.HASFox or CustomRoles.HASTroll) continue;
                    if (role.IsEnable() && !role.IsVanilla()) SendMessage(GetRoleName(role) + GetString(Enum.GetName(typeof(CustomRoles), role) + "InfoLong"));
                }
                if (Options.EnableLastImpostor.GetBool()) { SendMessage(GetRoleName(CustomRoles.LastImpostor) + GetString("LastImpostorInfoLong")); }
            }
            if (Options.NoGameEnd.GetBool()) { SendMessage(GetString("NoGameEndInfo")); }
        }
        public static void ShowActiveSettings(byte PlayerId = byte.MaxValue)
        {
            if (Options.HideGameSettings.GetBool() && PlayerId != byte.MaxValue)
            {
                SendMessage(GetString("Message.HideGameSettings"), PlayerId);
                return;
            }
            var text = "";
            if (Options.CurrentGameMode() == CustomGameMode.HideAndSeek)
            {
                text = GetString("Roles") + ":";
                if (CustomRoles.HASFox.IsEnable()) text += String.Format("\n{0}:{1}", GetRoleName(CustomRoles.HASFox), CustomRoles.HASFox.GetCount());
                if (CustomRoles.HASTroll.IsEnable()) text += String.Format("\n{0}:{1}", GetRoleName(CustomRoles.HASTroll), CustomRoles.HASTroll.GetCount());
                SendMessage(text, PlayerId);
                text = GetString("Settings") + ":";
                text += Main.versionText;
                text += GetString("HideAndSeek");
            }
            else if (Options.CurrentGameMode() == CustomGameMode.ColorWars)
            {
                text = GetString("Roles") + ":";
                if (CustomRoles.HASFox.IsEnable()) text += String.Format("\n{0}:{1}", GetRoleName(CustomRoles.HASFox), CustomRoles.HASFox.GetCount());
                if (CustomRoles.HASTroll.IsEnable()) text += String.Format("\n{0}:{1}", GetRoleName(CustomRoles.HASTroll), CustomRoles.HASTroll.GetCount());
                SendMessage(text, PlayerId);
                text = GetString("Settings") + ":";
                text += String.Format("\n\n{0}:{1}", "Current Game Mode", Options.GameMode.GetString());
                text += Main.versionText;
                text += GetString("ColorWars");
            }
            else if (Options.CurrentGameMode() == CustomGameMode.Splatoon)
            {
                text = GetString("Roles") + ":";
                if (CustomRoles.HASFox.IsEnable()) text += String.Format("\n{0}:{1}", GetRoleName(CustomRoles.HASFox), CustomRoles.HASFox.GetCount());
                if (CustomRoles.HASTroll.IsEnable()) text += String.Format("\n{0}:{1}", GetRoleName(CustomRoles.HASTroll), CustomRoles.HASTroll.GetCount());
                SendMessage(text, PlayerId);
                text = GetString("Settings") + ":";
                text += String.Format("\n\n{0}:{1}", "Current Game Mode", Options.GameMode.GetString());
                text += Main.versionText;
                text += GetString("Splatoon");
            }
            else
            {
                ShowActiveRoles(PlayerId);
                text = GetString("Attributes") + ":";
                if (Options.EnableLastImpostor.GetBool())
                {
                    text += String.Format("\n{0}:{1}", GetRoleName(CustomRoles.LastImpostor), Options.EnableLastImpostor.GetString());
                }
                if (Options.CamoComms.GetBool()) text += String.Format("\n{0}:{1}", GetString("CamoComms"), Options.CamoComms.GetString());
                if (Options.CurrentGameMode() == CustomGameMode.Standard)
                {
                    text += String.Format("\n{0}:{1}", "Min Neutral Killings", Options.MinNK.GetString());
                    text += String.Format("\n{0}:{1}", "Max Neutral Killings", Options.MaxNK.GetString());
                    text += String.Format("\n{0}:{1}", "Min Non-Neutral Killings", Options.MinNonNK.GetString());
                    text += String.Format("\n{0}:{1}", "Max Nin-Neutral Killings", Options.MaxNonNK.GetString());
                    text += String.Format("\n{0}:{1}", "Impostors know the Roles of their Team", Options.ImpostorKnowsRolesOfTeam.GetString());
                    text += String.Format("\n{0}:{1}", "Coven knows the Roles of their Team", Options.CovenKnowsRolesOfTeam.GetString());
                }
                text += String.Format("\n\n{0}:{1}", "Current Game Mode", Options.GameMode.GetString());
                text += String.Format("\n{0}:{1}", "Players have Access to /color,/name, and /level", Options.Customise.GetString());
                text += String.Format("\n{0}:{1}", "Roles look Similar to ToU", Options.RolesLikeToU.GetString());
                text += Main.versionText;
                //Roles look Similar to ToU
                SendMessage(text, PlayerId);
                text = GetString("Settings") + ":";
                foreach (var role in Options.CustomRoleCounts)
                {
                    if (!role.Key.IsEnable()) continue;
                    bool isFirst = true;
                    foreach (var c in Options.CustomRoleSpawnChances[role.Key].Children)
                    {
                        if (isFirst) { isFirst = false; continue; }
                        text += $"\n{c.GetName(disableColor: true)}:{c.GetString()}";

                        //タスク上書き設定用の処理
                        if (c.Name == "doOverride" && c.GetBool() == true)
                        {
                            foreach (var d in c.Children)
                            {
                                text += $"\n{d.GetName(disableColor: true)}:{d.GetString()}";
                            }
                        }
                        //メイヤーのポータブルボタン使用可能回数
                        if (c.Name == "MayorHasPortableButton" && c.GetBool() == true)
                        {
                            foreach (var d in c.Children)
                            {
                                text += $"\n{d.GetName(disableColor: true)}:{d.GetString()}";
                            }
                        }
                        text = text.RemoveHtmlTags();
                    }
                }
                if (Options.EnableLastImpostor.GetBool()) text += String.Format("\n{0}:{1}", GetString("LastImpostorKillCooldown"), Options.LastImpostorKillCooldown.GetString());
                if (Options.DisableDevices.GetBool())
                {
                    if (Options.DisableDevices.GetBool()) text += String.Format("\n{0}:{1}", Options.DisableAdmin.GetName(disableColor: true), Options.WhichDisableAdmin.GetString());
                }
                if (Options.SyncButtonMode.GetBool()) text += String.Format("\n{0}:{1}", GetString("SyncedButtonCount"), Options.SyncedButtonCount.GetInt());
                if (Options.SabotageTimeControl.GetBool())
                {
                    if (PlayerControl.GameOptions.MapId == 2) text += String.Format("\n{0}:{1}", GetString("PolusReactorTimeLimit"), Options.PolusReactorTimeLimit.GetString());
                    if (PlayerControl.GameOptions.MapId == 4) text += String.Format("\n{0}:{1}", GetString("AirshipReactorTimeLimit"), Options.AirshipReactorTimeLimit.GetString());
                }
                if (Options.VoteMode.GetBool())
                {
                    if (Options.GetWhenSkipVote() != VoteMode.Default) text += String.Format("\n{0}:{1}", GetString("WhenSkipVote"), Options.WhenSkipVote.GetString());
                    if (Options.GetWhenNonVote() != VoteMode.Default) text += String.Format("\n{0}:{1}", GetString("WhenNonVote"), Options.WhenNonVote.GetString());
                    if ((Options.GetWhenNonVote() == VoteMode.Suicide || Options.GetWhenSkipVote() == VoteMode.Suicide) && CustomRoles.Terrorist.IsEnable()) text += String.Format("\n{0}:{1}", GetString("CanTerroristSuicideWin"), Options.CanTerroristSuicideWin.GetBool());
                }
            }
            if (Options.LadderDeath.GetBool())
            {
                text += String.Format("\n{0}:{1}", GetString("LadderDeath"), GetOnOff(Options.LadderDeath.GetBool()));
                text += String.Format("\n{0}:{1}", GetString("LadderDeathChance"), Options.LadderDeathChance.GetString());
            }
            if (Options.IsStandardHAS) text += String.Format("\n{0}:{1}", GetString("StandardHAS"), GetOnOff(Options.StandardHAS.GetBool()));
            if (Options.NoGameEnd.GetBool()) text += String.Format("\n{0}:{1}", GetString("NoGameEnd"), GetOnOff(Options.NoGameEnd.GetBool()));
            SendMessage(text, PlayerId);
        }
        public static void ShowActiveRoles(byte PlayerId = byte.MaxValue)
        {
            if (Options.HideGameSettings.GetBool() && PlayerId != byte.MaxValue)
            {
                SendMessage(GetString("Message.HideGameSettings"), PlayerId);
                return;
            }
            var text = GetString("Roles") + ":";
            text += "\nFor Percentages, \nPlease type /percentages.";
            // text += string.Format("\n{0}:{1}", GetRoleName(CustomRoles.GM), GetOnOff(Options.EnableGM.GetBool()));
            foreach (CustomRoles role in Enum.GetValues(typeof(CustomRoles)))
            {
                if (role is CustomRoles.HASFox or CustomRoles.HASTroll) continue;
                if (role.IsEnable()) text += string.Format("\n{0}x{1}", GetRoleName(role), role.GetCount());
            }
            SendMessage(text, PlayerId);
        }
        public static void ShowPercentages(byte PlayerId = byte.MaxValue)
        {
            if (Options.HideGameSettings.GetBool() && PlayerId != byte.MaxValue)
            {
                SendMessage(GetString("Message.HideGameSettings"), PlayerId);
                return;
            }
            var text = GetString("Percentages") + ":";
            foreach (CustomRoles role in Enum.GetValues(typeof(CustomRoles)))
            {
                // if (role.RoleCannotBeInList()) continue;
                if (role.IsEnable()) text += string.Format("\n{0}:{1}x{2}", GetRoleName(role), $"{PercentageChecker.CheckPercentage(role.ToString(), PlayerId, role: role)}%", role.GetCount());
            }
            SendMessage(text, PlayerId);
        }
        public static void ShowLastResult(byte PlayerId = byte.MaxValue)
        {
            if (AmongUsClient.Instance.IsGameStarted)
            {
                SendMessage(GetString("CantUse.lastroles"), PlayerId);
                return;
            }
            var text = GetString("LastResult") + ":";
            Dictionary<byte, CustomRoles> cloneRoles = new(Main.AllPlayerCustomRoles);
            text += $"\n{SetEverythingUpPatch.LastWinsText}\n";
            foreach (var id in Main.winnerList)
            {
                text += $"\n★ " + SummaryTexts(id);
                cloneRoles.Remove(id);
            }
            foreach (var kvp in cloneRoles)
            {
                var id = kvp.Key;
                text += $"\n　 " + SummaryTexts(id);
            }
            text += "\n　 Last Voted Player: " + Main.LastVotedPlayer;
            SendMessage(text, PlayerId);
        }

        public static void LastResult(byte PlayerId = byte.MaxValue)
        {
            var text = GetString("LastResult") + ":";
            Dictionary<byte, CustomRoles> cloneRoles = new(Main.AllPlayerCustomRoles);
            text += $"\n{SetEverythingUpPatch.LastWinsText}\n";
            foreach (var kvp in cloneRoles)
            {
                var id = kvp.Key;
                text += $"\n　 " + SummaryTexts(id);
            }
            SendMessage(text, PlayerId);
        }


        public static string GetShowLastSubRolesText(byte id, bool disableColor = false)
        {
            var cSubRoleFound = Main.AllPlayerCustomSubRoles.TryGetValue(id, out var cSubRole);
            if (!cSubRoleFound || cSubRole == CustomRoles.NoSubRoleAssigned) return "";
            return disableColor ? " + " + GetRoleName(cSubRole) : Helpers.ColorString(Color.white, " (") + Helpers.ColorString(GetRoleColor(cSubRole), GetRoleName(cSubRole) + Helpers.ColorString(Color.white, ")"));
        }
        public static string SubRoleIntro(byte id, bool disableColor = false)
        {
            var cSubRoleFound = Main.AllPlayerCustomSubRoles.TryGetValue(id, out var cSubRole);
            if (!cSubRoleFound || cSubRole == CustomRoles.NoSubRoleAssigned) return "";
            return disableColor ? " + " + GetRoleName(cSubRole) : Helpers.ColorString(Color.white, " (") + Helpers.ColorString(GetRoleColor(cSubRole), GetRoleName(cSubRole) + Helpers.ColorString(Color.white, ")"));
        }

        public static void ShowHelp()
        {
            SendMessage(
                GetString("CommandList")
                + $"\n/winner - {GetString("Command.winner")}"
                + $"\n/lastresult - {GetString("Command.lastresult")}"
                + $"\n/rename - {GetString("Command.rename")}"
                + $"\n/now - {GetString("Command.now")}"
                + $"\n/h now - {GetString("Command.h_now")}"
                + $"\n/h roles {GetString("Command.h_roles")}"
                + $"\n/h attributes {GetString("Command.h_attributes")}"
                + $"\n/h modes {GetString("Command.h_modes")}"
                + $"\n/dump - {GetString("Command.dump")}"
                );

        }
        public static void CheckTerroristWin(GameData.PlayerInfo Terrorist)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            var taskState = GetPlayerById(Terrorist.PlayerId).GetPlayerTaskState();
            if (taskState.IsTaskFinished && Main.DeadPlayersThisRound.Contains(Terrorist.PlayerId) && (!PlayerState.IsSuicide(Terrorist.PlayerId) || Options.CanTerroristSuicideWin.GetBool())) //タスクが完了で（自殺じゃない OR 自殺勝ちが許可）されていれば
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.Is(CustomRoles.Terrorist))
                    {
                        if (PlayerState.GetDeathReason(pc.PlayerId) == PlayerState.DeathReason.Vote)
                        {
                            //追放された場合は生存扱い
                            PlayerState.SetDeathReason(pc.PlayerId, PlayerState.DeathReason.etc);
                            //生存扱いのためSetDeadは必要なし
                        }
                        else
                        {
                            //キルされた場合は自爆扱い
                            PlayerState.SetDeathReason(pc.PlayerId, PlayerState.DeathReason.Suicide);
                        }
                    }
                    else if (!pc.Data.IsDead && !pc.Is(CustomRoles.Pestilence))
                    {
                        //生存者は爆死
                        pc.RpcMurderPlayer(pc);
                        PlayerState.SetDeathReason(pc.PlayerId, PlayerState.DeathReason.Bombed);
                        PlayerState.SetDead(pc.PlayerId);
                    }
                }
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EndGame, Hazel.SendOption.Reliable, -1);
                writer.Write((byte)CustomWinner.Terrorist);
                writer.Write(Terrorist.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPC.TerroristWin(Terrorist.PlayerId);
            }
        }
        public static void ChildWin(GameData.PlayerInfo Child)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.Is(CustomRoles.Child))
                {
                    if (PlayerState.GetDeathReason(pc.PlayerId) == PlayerState.DeathReason.Vote)
                    {
                        PlayerState.SetDeathReason(pc.PlayerId, PlayerState.DeathReason.Screamed);
                    }
                    else
                    {
                        //キルされた場合は自爆扱い
                        PlayerState.SetDeathReason(pc.PlayerId, PlayerState.DeathReason.Screamed);
                    }
                }
                else if (!pc.Data.IsDead)
                {
                    if (!pc.Is(CustomRoles.Pestilence))
                    {
                        pc.RpcMurderPlayer(pc);
                        PlayerState.SetDeathReason(pc.PlayerId, PlayerState.DeathReason.EarDamage);
                        PlayerState.SetDead(pc.PlayerId);
                    }
                }
            }
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EndGame, Hazel.SendOption.Reliable, -1);
            writer.Write((byte)CustomWinner.Child);
            writer.Write(Child.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPC.ChildWin(Child.PlayerId);
        }
        public static void SendMessage(string text, byte sendTo = byte.MaxValue)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            Main.MessagesToSend.Add((text, sendTo));
        }
        public static void ApplySuffix()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            string name = SaveManager.PlayerName;
            string rname = PlayerControl.LocalPlayer.Data.PlayerName;
            string fontSize = "1";
            string fontSize1 = "0.8";
            string fontSize2 = "1.5";
            string fontSize3 = "2";
            string fontSize4 = "13";
            string none1 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Amnesiac), "Krampus")}</size>";
            string none2 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Jackal), "TOR")}</size>";
            string disc = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.PlagueBearer), "/t discord")}</size>";
            string disc1 = $"<size={fontSize2}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.thirdcolor), "Krampus")}</size>";
            string host = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.tancolor), "Best Host")}</size>";
            string test = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.fourthcolor), "Testing ToH:ToR")}</size>";
            string test1 = $"<size={fontSize2}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.tancolor2), "Krampus")}</size>";
            string toh = $"<size={fontSize2}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.TheGlitch), "TOR")}</size>";
            string simping = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns5), "♡")}</size>";
            string simping2 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns6), "Star")}</size>";
            string simping3 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns7), "Struck")}</size>";
            string simping4 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns7), "♡")}</size>";
            string simp1 = $"<size={fontSize2}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.aug6), "Krampus")}</size>";
            string simp2 = $"<size={fontSize2}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.aug5), "TOR")}</size>";
            string troll  = $"<size={fontSize4}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.PlagueBearer), "/t discord")}</size>";
            string dname = disc + "\r\n" + disc1 + toh;
            string hname = host + "\r\n" + disc1 + toh;
            string tname = test + "\r\n" + test1 + toh;
            string sname = simping + simping2 + simping3 + simping4 + "\r\n" + simp1 + simp2;
            string none = none1 + none2;
            string trname = troll + "\r\n" + none1 + none2;
            if (AmongUsClient.Instance.IsGameStarted)
            {
                if (Options.ColorNameMode.GetBool() && Main.nickName == "") name = Palette.GetColorName(PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId);
            }
            else
            {
                switch (Options.GetSuffixMode())
                {
                    case SuffixModes.None:
                        name = none;
                        break;
                    case SuffixModes.TOH:
                        name += "\r\n<color=" + Main.modColor + ">TOH: TORv" + Main.PluginVersion + "</color>";
                        break;
                    case SuffixModes.Discord:
                        name = dname;
                        break;
                    case SuffixModes.Hosting:
                        name = hname;
                        break;
                    case SuffixModes.Testing:
                        name = tname;
                        break;
                    case SuffixModes.Simping:
                        name = ($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.tancolor2), sname)}");
                        break;
                    case SuffixModes.Trolling:
                        name = ($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.tancolor2), trname)}");
                        break;
                }
            }
            if (name != PlayerControl.LocalPlayer.name && PlayerControl.LocalPlayer.CurrentOutfitType == PlayerOutfitType.Default) PlayerControl.LocalPlayer.RpcSetName(name);
        }
        public static PlayerControl GetPlayerById(int PlayerId)
        {
            return PlayerControl.AllPlayerControls.ToArray().Where(pc => pc.PlayerId == PlayerId).FirstOrDefault();
        }
        public static DeadBody GetDeadBodyById(int PlayerId)
        {
            var deadBodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            foreach (var body in deadBodies)
                if (body.ParentId == PlayerId)
                    return body;
            return null;
        }
        public static void NotifyRoles(bool isMeeting = false, PlayerControl SpecifySeer = null, bool NoCache = false, bool ForceLoop = false)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (PlayerControl.AllPlayerControls == null) return;

            var caller = new System.Diagnostics.StackFrame(1, false);
            var callerMethod = caller.GetMethod();
            string callerMethodName = callerMethod.Name;
            string callerClassName = callerMethod.DeclaringType.FullName;
            TownOfHost.Logger.Info("NotifyRolesが" + callerClassName + "." + callerMethodName + "から呼び出されました", "NotifyRoles");
            HudManagerPatch.NowCallNotifyRolesCount++;
            HudManagerPatch.LastSetNameDesyncCount = 0;

            //Snitch警告表示のON/OFF
            bool ShowSnitchWarning = false;
            if (CustomRoles.Snitch.IsEnable())
            {
                foreach (var snitch in PlayerControl.AllPlayerControls)
                {
                    if (snitch.Is(CustomRoles.Snitch) && !snitch.Data.IsDead && !snitch.Data.Disconnected)
                    {
                        var taskState = snitch.GetPlayerTaskState();
                        if (taskState.DoExpose)
                        {
                            ShowSnitchWarning = true;
                            break;
                        }
                    }
                }
            }

            var seerList = PlayerControl.AllPlayerControls;
            if (SpecifySeer != null)
            {
                seerList = new();
                seerList.Add(SpecifySeer);
            }
            //seer:ここで行われた変更を見ることができるプレイヤー
            //target:seerが見ることができる変更の対象となるプレイヤー
            foreach (var seer in seerList)
            {
                if (seer.IsModClient()) continue;
                if (seer.Data.Disconnected) continue;
                if (seer == null) return;
                string fontSize = "1.5";
                if (isMeeting && (seer.GetClient().PlatformData.Platform.ToString() == "Playstation" || seer.GetClient().PlatformData.Platform.ToString() == "Switch")) fontSize = "70%";
                TownOfHost.Logger.Info("NotifyRoles-Loop1-" + seer.GetNameWithRole() + ":START", "NotifyRoles");
                //Loop1-bottleのSTART-END間でKeyNotFoundException
                //seerが落ちているときに何もしない

                //タスクなど進行状況を含むテキスト
                string SelfTaskText = GetProgressText(seer);

                //名前の後ろに付けるマーカー
                string SelfMark = "";

                //インポスター/キル可能な第三陣営に対するSnitch警告
                var canFindSnitchRole = seer.GetCustomRole().IsImpostor() || //LocalPlayerがインポスター
                    (Options.SnitchCanFindNeutralKiller.GetBool() && seer.IsNeutralKiller());//or エゴイスト

                if (canFindSnitchRole && ShowSnitchWarning && !isMeeting)
                {
                    var arrows = "";
                    foreach (var arrow in Main.targetArrows)
                    {
                        if (arrow.Key.Item1 == seer.PlayerId && !PlayerState.isDead[arrow.Key.Item2])
                        {
                            //自分用の矢印で対象が死んでない時
                            arrows += arrow.Value;
                        }
                    }
                    SelfMark += $"<color={GetRoleColorCode(CustomRoles.Snitch)}>★{arrows}</color>";
                }

                if (seer.Is(CustomRoles.Phantom))
                {
                    if (Main.PhantomAlert) SelfMark += $"<color={GetRoleColorCode(CustomRoles.Phantom)}>★★</color>";
                    else if (Main.PhantomCanBeKilled) SelfMark += $"<color={GetRoleColorCode(CustomRoles.Phantom)}>★</color>";
                }

                //ハートマークを付ける(自分に)
                if (seer.Is(CustomRoles.LoversRecode)) SelfMark += $"<color={GetRoleColorCode(CustomRoles.LoversRecode)}>♡</color>";

                //呪われている場合
                if (Main.SpelledPlayer.Find(x => x.PlayerId == seer.PlayerId) != null && isMeeting)
                    SelfMark += "<color=#ff0000>†</color>";
                if (Main.SilencedPlayer.Find(x => x.PlayerId == seer.PlayerId) != null && isMeeting)
                    SelfMark += "<color=#ff0000> (S)</color>";

                if (seer.Is(CustomRoles.Survivor) && !Main.SurvivorStuff.ContainsKey(seer.PlayerId))
                {
                    Main.SurvivorStuff.Add(seer.PlayerId, (0, false, false, false, false));
                }
                if (Sniper.IsEnable())
                {
                    //銃声が聞こえるかチェック
                    SelfMark += Sniper.GetShotNotify(seer.PlayerId);
                }
                //Markとは違い、改行してから追記されます。
                string SelfSuffix = "";

                if (seer.Is(CustomRoles.BountyHunter) && BountyHunter.GetTarget(seer) != null)
                {
                    string BountyTargetName = BountyHunter.GetTarget(seer).GetRealName(isMeeting);
                    SelfSuffix = $"<size={fontSize}>Target:{BountyTargetName}</size>";
                }
                if (seer.Is(CustomRoles.FireWorks))
                {
                    string stateText = FireWorks.GetStateText(seer);
                    SelfSuffix = $"{stateText}";
                }
                if (seer.Is(CustomRoles.Witch))
                {
                    SelfSuffix = seer.IsSpellMode() ? "Mode:" + GetString("WitchModeSpell") : "Mode:" + GetString("WitchModeKill");
                }
                if (seer.Is(CustomRoles.HexMaster))
                {
                    SelfSuffix = seer.IsHexMode() ? "Mode:" + "Hexing" : "Mode:" + "Killing";
                }
                if (seer.Is(CustomRoles.Werewolf))
                {
                    var ModeLang = Main.IsRampaged ? "True" : "False";
                    var ReadyLang = Main.RampageReady ? "True" : "False";
                    SelfSuffix = "Is Rampaging: " + ModeLang;
                    SelfSuffix += "\nRampage Ready: " + ReadyLang;
                }
                if (seer.Is(CustomRoles.Medusa))
                {
                    var ModeLang = Main.IsGazing ? "True" : "False";
                    var ReadyLang = Main.GazeReady ? "True" : "False";
                    SelfSuffix = "Gazing: " + ModeLang;
                    SelfSuffix += "\nGaze Ready: " + ReadyLang;
                }
                if (seer.Is(CustomRoles.TheGlitch))
                {
                    var ModeLang = Main.IsHackMode ? "Hack" : "Kill";
                    SelfSuffix = "Glitch Current Mode: " + ModeLang;
                }

                //他人用の変数定義
                bool SeerKnowsImpostors = false; //trueの時、インポスターの名前が赤色に見える
                bool SeerKnowsCoven = false; //trueの時、インポスターの名前が赤色に見える

                //タスクを終えたSnitchがインポスター/キル可能な第三陣営の方角を確認できる
                if (seer.Is(CustomRoles.Snitch))
                {
                    var TaskState = seer.GetPlayerTaskState();
                    if (TaskState.IsTaskFinished)
                    {
                        SeerKnowsImpostors = true;
                        if (Options.SnitchCanFindCoven.GetBool())
                            SeerKnowsCoven = true;
                        //ミーティング以外では矢印表示
                        if (!isMeeting)
                        {
                            foreach (var arrow in Main.targetArrows)
                            {
                                //自分用の矢印で対象が死んでない時
                                if (arrow.Key.Item1 == seer.PlayerId && !PlayerState.isDead[arrow.Key.Item2])
                                    SelfSuffix += arrow.Value;
                            }
                        }
                    }
                }

                if (seer.Is(CustomRoles.Vulture))
                {
                    var TaskState = Options.VultureArrow.GetBool();
                    if (TaskState)
                    {
                        if (!isMeeting)
                        {
                            foreach (var arrow in Main.targetArrows)
                            {
                                //自分用の矢印で対象が死んでない時
                                if (Main.DeadPlayersThisRound.Contains(arrow.Key.Item2))
                                    if (arrow.Key.Item1 == seer.PlayerId && PlayerState.isDead[arrow.Key.Item2])
                                        SelfSuffix += arrow.Value;
                            }
                        }
                    }
                }

                if (seer.GetCustomRole().IsCoven())
                {
                    SeerKnowsCoven = true;
                }
                if (seer.Is(CustomRoles.MadSnitch))
                {
                    var TaskState = seer.GetPlayerTaskState();
                    if (TaskState.IsTaskFinished)
                        SeerKnowsImpostors = true;
                }
                if (seer.Is(CustomRoles.CorruptedSheriff))
                    SeerKnowsImpostors = true;

                foreach (var target in PlayerControl.AllPlayerControls)
                {
                    //targetがseer自身の場合は何もしない
                    if (target == seer || target.Data.Disconnected) continue;
                    if (target == null) continue;
                    if (target.Is(CustomRoles.Phantom)) continue;
                    if (target.Is(CustomRoles.Phantom) && Main.PhantomAlert)
                    {
                        if (!isMeeting)
                        {
                            foreach (var arrow in Main.targetArrows)
                            {
                                if (arrow.Key.Item1 == seer.PlayerId && !PlayerState.isDead[arrow.Key.Item2])
                                    SelfSuffix += arrow.Value;
                            }
                        }
                    }
                }

                //RealNameを取得 なければ現在の名前をRealNamesに書き込む
                if (SelfSuffix != "")
                    SelfSuffix = Helpers.ColorString(Utils.GetRoleColor(seer.GetCustomRole()), SelfSuffix);
                if (isMeeting) SelfSuffix = "";
                if (Options.RolesLikeToU.GetBool() && !isMeeting)
                {
                    string SeerRealName = seer.GetRealName(isMeeting);

                    string SelfRoleName = $"{Helpers.ColorString(seer.GetRoleColor(), seer.GetRoleName())}{SelfTaskText}";
                    string SelfName = $"{Helpers.ColorString(seer.GetRoleColor(), SeerRealName)}{SelfMark}";
                    if (Main.KilledDemo.Contains(seer.PlayerId))
                        SelfName += $"<size={fontSize}>\r\n{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Demolitionist), "You killed Demolitionist!")}</size>";
                    if (seer.Is(CustomRoles.Arsonist) && seer.IsDouseDone())
                        SelfName = $"</size>\r\n{Helpers.ColorString(seer.GetRoleColor(), GetString("EnterVentToWin"))}";
                    SelfName = SelfName + "\r\n" + SelfRoleName;
                    SelfName += SelfSuffix == "" ? "" : "\r\n " + SelfSuffix;
                    if (!isMeeting) SelfName += "\r\n";

                    //適用
                    seer.RpcSetNamePrivate(SelfName, true, force: NoCache);
                }
                else
                {
                    string SeerRealName = seer.GetRealName(isMeeting);

                    //seerの役職名とSelfTaskTextとseerのプレイヤー名とSelfMarkを合成
                    string SelfRoleName = $"<size={fontSize}>{Helpers.ColorString(seer.GetRoleColor(), seer.GetRoleName())}{SelfTaskText}</size>";
                    string SelfName = $"{Helpers.ColorString(seer.GetRoleColor(), SeerRealName)}{SelfMark}";
                    if (Main.KilledDemo.Contains(seer.PlayerId))
                        SelfName += $"</size>\r\n{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Demolitionist), "You killed Demolitionist!")}";
                    // else SelfName = $"{Helpers.ColorString(seer.GetRoleColor(), SeerRealName)}{SelfMark}";
                    if (seer.Is(CustomRoles.Arsonist) && seer.IsDouseDone())
                        SelfName = $"</size>\r\n{Helpers.ColorString(seer.GetRoleColor(), GetString("EnterVentToWin"))}";
                    SelfName = SelfRoleName + "\r\n" + SelfName;
                    SelfName += SelfSuffix == "" ? "" : "\r\n " + SelfSuffix;
                    if (!isMeeting) SelfName += "\r\n";

                    //適用
                    seer.RpcSetNamePrivate(SelfName, true, force: NoCache);
                }

                if (seer.Is(CustomRoles.Survivor) && !Main.SurvivorStuff.ContainsKey(seer.PlayerId))
                {
                    Main.SurvivorStuff.Add(seer.PlayerId, (0, false, false, false, false));
                }

                //seerが死んでいる場合など、必要なときのみ第二ループを実行する
                if (seer.Data.IsDead //seerが死んでいる
                    || SeerKnowsImpostors //seerがインポスターを知っている状態
                    || SeerKnowsCoven
                    || seer.GetCustomRole().IsImpostor() //seerがインポスター
                    || seer.Is(CustomRoles.EgoSchrodingerCat) //seerがエゴイストのシュレディンガーの猫
                    || seer.Is(CustomRoles.JSchrodingerCat) //seerがJackal陣営のシュレディンガーの猫
                    || seer.GetCustomRole().IsJackalTeam()
                    || NameColorManager.Instance.GetDataBySeer(seer.PlayerId).Count > 0 //seer視点用の名前色データが一つ以上ある
                    || seer.Is(CustomRoles.Arsonist)
                    || seer.Is(CustomRoles.LoversRecode)
                    || Main.SpelledPlayer.Count > 0
                    || Main.SilencedPlayer.Count > 0
                    || seer.Is(CustomRoles.GuardianAngelTOU)
                    || seer.Is(CustomRoles.Executioner)
                    || seer.Is(CustomRoles.Doctor) //seerがドクター
                    || seer.Is(CustomRoles.Puppeteer)
                    || seer.Is(CustomRoles.HexMaster)
                    || seer.Is(CustomRoles.BountyHunter)
                    || seer.Is(CustomRoles.Investigator)
                    || Main.PhantomAlert
                    // || (IsActive(SystemTypes.Comms) && Options.CamoComms.GetBool())
                    //|| Main.KilledDemo.Contains(seer.PlayerId)
                    || seer.Is(CustomRoles.PlagueBearer)
                    || seer.Is(CustomRoles.YingYanger)
                    //|| seer.GetCustomSubRole().GetModifierType() != ModifierType.None
                    || IsActive(SystemTypes.Electrical)
                    || Camouflague.IsActive
                    || NoCache
                    || ForceLoop
                )
                {
                    /*if (Camouflague.IsActive && !Camouflague.InMeeting && !Camouflague.did && Options.CamoComms.GetBool())
                    {
                        Camouflague.did = true;
                        Camouflague.MeetingCause();
                    }*/

                    foreach (var target in PlayerControl.AllPlayerControls)
                    {
                        //targetがseer自身の場合は何もしない
                        if (target == seer || target.Data.Disconnected) continue;
                        if (target == null) continue;
                        TownOfHost.Logger.Info("NotifyRoles-Loop2-" + target.GetNameWithRole() + ":START", "NotifyRoles");

                        //他人のタスクはtargetがタスクを持っているかつ、seerが死んでいる場合のみ表示されます。それ以外の場合は空になります。
                        string TargetTaskText = "";
                        if (seer.Data.IsDead && Options.GhostCanSeeOtherRoles.GetBool())
                            TargetTaskText = $"{GetProgressText(target)}";

                        //名前の後ろに付けるマーカー
                        string TargetMark = "";
                        //呪われている人
                        if (Main.SpelledPlayer.Find(x => x.PlayerId == target.PlayerId) != null && isMeeting)
                            TargetMark += "<color=#ff0000>†</color>";
                        if (Main.SilencedPlayer.Find(x => x.PlayerId == target.PlayerId) != null && isMeeting)
                            TargetMark += "<color=#ff0000> (S)</color>";
                        if (target.Is(CustomRoles.Phantom) && Main.PhantomAlert)
                        {
                            TargetMark += $"<color={GetRoleColorCode(CustomRoles.Phantom)}>★</color>";
                        }
                        //タスク完了直前のSnitchにマークを表示
                        canFindSnitchRole = seer.GetCustomRole().IsImpostor() || //Seerがインポスター
                            (Options.SnitchCanFindNeutralKiller.GetBool() && seer.IsNeutralKiller());//or エゴイスト

                        if (target.Is(CustomRoles.Snitch) && canFindSnitchRole)
                        {
                            var taskState = target.GetPlayerTaskState();
                            if (taskState.DoExpose)
                                TargetMark += $"<color={GetRoleColorCode(CustomRoles.Snitch)}>★</color>";
                        }
                        string TeamText = "";

                        if (seer.GetCustomRole().IsImpostor() && !isMeeting)
                        {
                            if (Options.ImpostorKnowsRolesOfTeam.GetBool())
                            {
                                if (!CustomRoles.Egoist.IsEnable())
                                {
                                    //so we gotta make it so they can see the team. of their impostor
                                    if (target.GetCustomRole().IsImpostor())
                                    {
                                        if (!seer.Data.IsDead && !seer.Data.Disconnected)
                                        {
                                            // TeamText += "\r\n";
                                            if (!Options.RolesLikeToU.GetBool())
                                                TeamText += $"<size={fontSize}>{Helpers.ColorString(target.GetRoleColor(), target.GetRoleName())}</size>\r\n";
                                            else
                                                TeamText = $"\r\n{Helpers.ColorString(target.GetRoleColor(), target.GetRoleName())}";
                                        }
                                    }
                                }
                                else
                                {
                                    if (!Egoist.ImpostorsKnowEgo.GetBool())
                                    {
                                        //so we gotta make it so they can see the team. of their impostor
                                        if (target.GetCustomRole().IsImpostor())
                                        {
                                            if (!seer.Data.IsDead && !seer.Data.Disconnected)
                                            {
                                                // TeamText += "\r\n";
                                                if (!Options.RolesLikeToU.GetBool())
                                                    TeamText += $"<size={fontSize}>{Helpers.ColorString(target.GetRoleColor(), target.GetRoleName())}</size>\r\n";
                                                else
                                                    TeamText = $"\r\n{Helpers.ColorString(target.GetRoleColor(), target.GetRoleName())}";
                                            }
                                        }
                                    }
                                }
                            }

                        }
                        if (seer.GetCustomRole().IsCoven() && !isMeeting)
                        {
                            if (Options.CovenKnowsRolesOfTeam.GetBool())
                            {
                                if (target.GetCustomRole().IsCoven())
                                {
                                    if (!seer.Data.IsDead)
                                    {
                                        // TeamText += "\r\n";
                                        if (!Options.RolesLikeToU.GetBool())
                                            TeamText += $"<size={fontSize}>{Helpers.ColorString(target.GetRoleColor(), target.GetRoleName())}</size>\r\n";
                                        else
                                            TeamText = $"\r\n{Helpers.ColorString(target.GetRoleColor(), target.GetRoleName())}";
                                    }
                                }
                            }
                        }
                        //ハートマークを付ける(相手に)
                        if (seer.Is(CustomRoles.LoversRecode) && target.Is(CustomRoles.LoversRecode))
                        {
                            TargetMark += $"<color={GetRoleColorCode(CustomRoles.LoversRecode)}>♡</color>";
                        }
                        //霊界からラバーズ視認
                        else if (seer.Data.IsDead && !seer.Is(CustomRoles.LoversRecode) && target.Is(CustomRoles.LoversRecode))
                        {
                            TargetMark += $"<color={GetRoleColorCode(CustomRoles.LoversRecode)}>♡</color>";
                        }

                        /*if (!seer.Is(CustomRoles.LoversRecode) && seer.GetCustomSubRole().GetModifierType() != ModifierType.None)
                        {
                            TargetMark += $"<color={GetRoleColorCode(CustomRoles.Yellow)}> " + seer.GetSubRoleName() + "</color>";
                        }*/

                        if (seer.Is(CustomRoles.Arsonist))//seerがアーソニストの時
                        {
                            if (seer.IsDousedPlayer(target)) //seerがtargetに既にオイルを塗っている(完了)
                            {
                                TargetMark += $"<color={GetRoleColorCode(CustomRoles.Arsonist)}>▲</color>";
                            }
                            if (
                                Main.ArsonistTimer.TryGetValue(seer.PlayerId, out var ar_kvp) && //seerがオイルを塗っている途中(現在進行)
                                ar_kvp.Item1 == target //オイルを塗っている対象がtarget
                            )
                            {
                                TargetMark += $"<color={GetRoleColorCode(CustomRoles.Arsonist)}>△</color>";
                            }
                        }
                        if (seer.Is(CustomRoles.HexMaster))
                        {
                            if (seer.IsHexedPlayer(target))
                                TargetMark += $"<color={GetRoleColorCode(CustomRoles.Coven)}>†</color>";
                        }
                        if (seer.Is(CustomRoles.PlagueBearer))//seerがアーソニストの時
                        {
                            if (seer.IsInfectedPlayer(target)) //seerがtargetに既にオイルを塗っている(完了)
                            {
                                TargetMark += $"<color={GetRoleColorCode(CustomRoles.Pestilence)}>◆</color>";
                            }
                            if (
                                Main.PlagueBearerTimer.TryGetValue(seer.PlayerId, out var ar_kvp) && //seerがオイルを塗っている途中(現在進行)
                                ar_kvp.Item1 == target //オイルを塗っている対象がtarget
                            )
                            {
                                TargetMark += $"<color={GetRoleColorCode(CustomRoles.Pestilence)}>△</color>";
                            }
                        }
                        if (seer.Is(CustomRoles.Puppeteer) &&
                        Main.PuppeteerList.ContainsValue(seer.PlayerId) &&
                        Main.PuppeteerList.ContainsKey(target.PlayerId))
                            TargetMark += $"<color={Utils.GetRoleColorCode(CustomRoles.Impostor)}>◆</color>";

                        if (seer.Is(CustomRoles.CovenWitch) &&
                        Main.WitchedList.ContainsValue(seer.PlayerId) &&
                        Main.WitchedList.ContainsKey(target.PlayerId))
                            TargetMark += $"<color={Utils.GetRoleColorCode(CustomRoles.CovenWitch)}>◆</color>";

                        //他人の役職とタスクは幽霊が他人の役職を見れるようになっていてかつ、seerが死んでいる場合のみ表示されます。それ以外の場合は空になります。
                        string TargetRoleText = "";
                        if (seer.Data.IsDead && Options.GhostCanSeeOtherRoles.GetBool())
                            if (!Options.RolesLikeToU.GetBool())
                                TargetRoleText = $"<size={fontSize}>{Helpers.ColorString(target.GetRoleColor(), target.GetRoleName())}{TargetTaskText}</size>\r\n";
                            else
                                TargetRoleText = $"\r\n{Helpers.ColorString(target.GetRoleColor(), target.GetRoleName())}{TargetTaskText}";

                        if (target.Is(CustomRoles.GM))
                            TargetRoleText = $"<size={fontSize}>{Helpers.ColorString(target.GetRoleColor(), target.GetRoleName())}</size>\r\n";

                        //RealNameを取得 なければ現在の名前をRealNamesに書き込む
                        string TargetPlayerName = target.GetRealName(isMeeting);

                        if (seer.Is(CustomRoles.Psychic) && isMeeting)
                        {
                            int numOfPsychicBad = UnityEngine.Random.RandomRange(0, 3);
                            numOfPsychicBad = (int)Math.Round(numOfPsychicBad + 0.1);
                            if (numOfPsychicBad > 3) // failsafe
                                numOfPsychicBad = 3;
                            List<byte> goodids = new();
                            List<byte> badids = new();
                            if (!seer.Data.IsDead)
                            {
                                List<PlayerControl> badPlayers = new();
                                List<PlayerControl> goodPlayers = new();
                                foreach (var pc in PlayerControl.AllPlayerControls)
                                {
                                    if (pc.Data.IsDead || pc.Data.Disconnected || pc.PlayerId == seer.PlayerId) continue;
                                    bool isGood = true;
                                    var role = pc.GetCustomRole();
                                    if (Options.ExeTargetShowsEvil.GetBool())
                                        if (Main.ExecutionerTarget.ContainsValue(pc.PlayerId))
                                        {
                                            badPlayers.Add(pc);
                                            isGood = false;
                                            continue;
                                        }
                                    switch (role)
                                    {
                                        case CustomRoles.GuardianAngelTOU:
                                            if (!Options.GAdependsOnTaregtRole.GetBool()) break;
                                            Main.GuardianAngelTarget.TryGetValue(pc.PlayerId, out var protectId);
                                            if (!GetPlayerById(protectId).GetCustomRole().IsCrewmate())
                                                badPlayers.Add(pc);
                                            break;
                                    }
                                    switch (role.GetRoleType())
                                    {
                                        case RoleType.Crewmate:
                                            if (!Options.CkshowEvil.GetBool()) break;
                                            if (role is CustomRoles.Sheriff or CustomRoles.Veteran or CustomRoles.Child or CustomRoles.Bastion or CustomRoles.Demolitionist or CustomRoles.NiceGuesser) badPlayers.Add(pc);
                                            break;
                                        case RoleType.Impostor:
                                            badPlayers.Add(pc);
                                            isGood = false;
                                            break;
                                        case RoleType.Neutral:
                                            if (role.IsNeutralKilling()) badPlayers.Add(pc);
                                            if (Options.NBshowEvil.GetBool())
                                                if (role is CustomRoles.Opportunist or CustomRoles.Survivor or CustomRoles.GuardianAngelTOU or CustomRoles.Amnesiac or CustomRoles.SchrodingerCat) badPlayers.Add(pc);
                                            if (Options.NEshowEvil.GetBool())
                                                if (role is CustomRoles.Jester or CustomRoles.Terrorist or CustomRoles.Executioner or CustomRoles.Hacker or CustomRoles.Vulture) badPlayers.Add(pc);
                                            break;
                                        case RoleType.Madmate:
                                            if (!Options.MadmatesAreEvil.GetBool()) break;
                                            badPlayers.Add(pc);
                                            isGood = false;
                                            break;
                                    }
                                    List<byte> badpcids = new();
                                    foreach (var p in badPlayers)
                                    {
                                        badpcids.Add(p.PlayerId);
                                    }
                                    if (badids.Contains(pc.PlayerId)) isGood = false;
                                    if (isGood)
                                        goodPlayers.Add(pc);
                                }
                                if (numOfPsychicBad < badPlayers.Count) numOfPsychicBad = badPlayers.Count;
                                int goodPeople = 3 - numOfPsychicBad;
                                for (var i = 0; i < numOfPsychicBad; i++)
                                {
                                    var rando = new System.Random();
                                    var player = badPlayers[rando.Next(0, badPlayers.Count)];
                                    badPlayers.Remove(player);
                                    badids.Add(player.PlayerId);
                                }
                                if (goodPeople != 0)
                                    for (var i = 0; i < goodPeople; i++)
                                    {
                                        var rando = new System.Random();
                                        var player = goodPlayers[rando.Next(0, goodPlayers.Count)];
                                        goodPlayers.Remove(player);
                                        goodids.Add(player.PlayerId);
                                    }
                            }
                            foreach (var id in goodids)
                            {
                                if (target.PlayerId == id)
                                    TargetPlayerName = Helpers.ColorString(GetRoleColor(CustomRoles.Impostor), TargetPlayerName);
                            }
                            foreach (var id in badids)
                            {
                                if (target.PlayerId == id)
                                    TargetPlayerName = Helpers.ColorString(GetRoleColor(CustomRoles.Impostor), TargetPlayerName);
                            }
                            // TargetPlayerName = Helpers.ColorString(GetRoleColor(CustomRoles.Impostor), TargetPlayerName)
                        }

                        //ターゲットのプレイヤー名の色を書き換えます。
                        if (SeerKnowsImpostors) //Seerがインポスターが誰かわかる状態
                        {
                            //スニッチはオプション有効なら第三陣営のキル可能役職も見れる
                            if (seer.Is(CustomRoles.CorruptedSheriff))
                            {
                                var foundCheck = target.GetCustomRole().IsImpostor();
                                if (foundCheck)
                                    TargetPlayerName = Helpers.ColorString(target.GetRoleColor(), TargetPlayerName);
                            }
                            else
                            {
                                var snitchOption = seer.Is(CustomRoles.Snitch) && Options.SnitchCanFindNeutralKiller.GetBool();
                                var foundCheck = target.GetCustomRole().IsImpostor() || (snitchOption && target.IsNeutralKiller());
                                if (foundCheck)
                                    TargetPlayerName = Helpers.ColorString(target.GetRoleColor(), TargetPlayerName);
                            }
                        }
                        else if (SeerKnowsCoven)
                        {
                            var isCoven = seer.GetCustomRole().IsCoven();
                            var foundCheck = target.GetCustomRole().IsCoven();
                            if (isCoven)
                                if (foundCheck)
                                    TargetPlayerName = Helpers.ColorString(target.GetRoleColor(), TargetPlayerName);
                        }
                        else if (seer.GetCustomRole().IsImpostor() && target.Is(CustomRoles.Egoist) && Egoist.ImpostorsKnowEgo.GetBool())
                            TargetPlayerName = Helpers.ColorString(GetRoleColor(CustomRoles.Egoist), TargetPlayerName);
                        else if (seer.GetCustomRole().IsImpostor() && target.Is(CustomRoles.CorruptedSheriff))
                            TargetPlayerName = Helpers.ColorString(GetRoleColor(CustomRoles.Impostor), TargetPlayerName);
                        else if ((seer.Is(CustomRoles.EgoSchrodingerCat) && target.Is(CustomRoles.Egoist)) || //エゴ猫 --> エゴイスト
                                 (seer.GetCustomRole().IsJackalTeam() && target.GetCustomRole().IsJackalTeam())) // J猫 --> ジャッカル
                            TargetPlayerName = Helpers.ColorString(target.GetRoleColor(), TargetPlayerName);
                        else if (Utils.IsActive(SystemTypes.Electrical) && target.Is(CustomRoles.Mare) && !isMeeting)
                            TargetPlayerName = Helpers.ColorString(GetRoleColor(CustomRoles.Impostor), TargetPlayerName); //targetの赤色で表示
                        else
                        {
                            //NameColorManager準拠の処理
                            var ncd = NameColorManager.Instance.GetData(seer.PlayerId, target.PlayerId);
                            TargetPlayerName = ncd.OpenTag + TargetPlayerName + ncd.CloseTag;
                        }
                        foreach (var ExecutionerTarget in Main.ExecutionerTarget)
                        {
                            if ((seer.PlayerId == ExecutionerTarget.Key || seer.Data.IsDead) && //seerがKey or Dead
                            target.PlayerId == ExecutionerTarget.Value)
                                TargetPlayerName = Helpers.ColorString(GetRoleColor(CustomRoles.Target), TargetPlayerName);
                        }

                        foreach (var GATarget in Main.GuardianAngelTarget)
                        {
                            if ((seer.PlayerId == GATarget.Key || seer.Data.IsDead) && //seerがKey or Dead
                            target.PlayerId == GATarget.Value) //targetがValue
                                TargetMark += $"<color={Utils.GetRoleColorCode(CustomRoles.GuardianAngel)}>♦</color>";
                        }
                        if (seer.Data.IsDead && Options.GhostCanSeeOtherRoles.GetBool())
                            TargetPlayerName = Helpers.ColorString(Utils.GetRoleColor(target.GetCustomRole()), TargetPlayerName);
                        if (seer.Is(CustomRoles.HexMaster) && isMeeting)
                        {
                            foreach (var pc in PlayerControl.AllPlayerControls)
                            {
                                if (pc == null ||
                                    pc.Data.IsDead ||
                                    pc.Data.Disconnected ||
                                    pc.PlayerId == seer.PlayerId
                                ) continue; //塗れない人は除外 (死んでたり切断済みだったり あとアーソニスト自身も)

                                if (Main.isHexed.TryGetValue((seer.PlayerId, pc.PlayerId), out var isDoused) && isDoused)
                                    Utils.SendMessage("You have been hexed by the Hex Master!", pc.PlayerId);
                            }
                        }
                        if (seer.Is(CustomRoles.BountyHunter) && BountyHunter.GetTarget(seer) != null)
                        {
                            var bounty = BountyHunter.GetTarget(seer);
                            if (target == bounty) TargetPlayerName = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Target), TargetPlayerName);
                        }
                        if (seer.Is(CustomRoles.Investigator))
                        {
                            if (Investigator.hasSeered[target.PlayerId] == true)
                            {
                                // Investigator has Seered Player.
                                if (target.Is(CustomRoles.CorruptedSheriff))
                                {
                                    if (Investigator.CSheriffSwitches.GetBool())
                                    {
                                        TargetPlayerName = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Impostor), TargetPlayerName);
                                    }
                                    else
                                    {
                                        if (Investigator.SeeredCSheriff)
                                            TargetPlayerName = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Impostor), TargetPlayerName);
                                        else
                                            TargetPlayerName = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.TheGlitch), TargetPlayerName);
                                    }
                                }
                                else
                                {
                                    if (Investigator.IsRed(target))
                                    {
                                        if (target.GetCustomRole().IsCoven())
                                        {
                                            if (Investigator.CovenIsPurple.GetBool())
                                                TargetPlayerName = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Coven), TargetPlayerName); //targetの名前をエゴイスト色で表示
                                            else
                                                TargetPlayerName = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Impostor), TargetPlayerName); //targetの名前をエゴイスト色で表示
                                        }
                                        else TargetPlayerName = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Impostor), TargetPlayerName);
                                    }
                                    else
                                    {
                                        TargetPlayerName = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.TheGlitch), TargetPlayerName); //targetの名前をエゴイスト色で表示
                                    }
                                }
                            }
                        }
                        if (seer.Is(CustomRoles.YingYanger) && Main.ColliderPlayers.Contains(target))
                        {
                            TargetPlayerName = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Target), TargetPlayerName);
                        }

                        string TargetDeathReason = "";
                        if (seer.Is(CustomRoles.Doctor) && target.Data.IsDead)
                            TargetDeathReason = $"({Helpers.ColorString(GetRoleColor(CustomRoles.Doctor), GetVitalText(target.PlayerId))})";

                        //全てのテキストを合成します。
                        string TargetName = "";
                        if (!Options.RolesLikeToU.GetBool())
                            TargetName = $"{TargetRoleText}{TeamText}{TargetPlayerName}{TargetDeathReason}{TargetMark}";
                        else
                            TargetName = $"{TargetPlayerName}{TeamText}{TargetRoleText}{TargetDeathReason}{TargetMark}";

                        //適用
                        target.RpcSetNamePrivate(TargetName, true, seer, force: NoCache);
                        //target.RpcSetNamePlatePrivate("");

                        TownOfHost.Logger.Info("NotifyRoles-Loop2-" + target.GetNameWithRole() + ":END", "NotifyRoles");
                    }
                }
                TownOfHost.Logger.Info("NotifyRoles-Loop1-" + seer.GetNameWithRole() + ":END", "NotifyRoles");
            }
            Main.witchMeeting = false;
        }
        public static void CheckSurvivorVest(PlayerControl survivor, PlayerControl killer, bool suicide = true)
        {
            foreach (var ar in Main.SurvivorStuff)
            {
                if (ar.Key != survivor.PlayerId) break;
                var stuff = Main.SurvivorStuff[survivor.PlayerId];
                if (stuff.Item2)
                {
                    //killer.RpcGuardAndKill(killer);
                    killer.RpcGuardAndKill(survivor);
                }
                else
                {
                    if (!suicide)
                        killer.RpcMurderPlayerV2(survivor);
                    else
                        survivor.RpcMurderPlayerV2(survivor);
                }
            }
        }
        public static void CustomSyncAllSettings()
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                pc.CustomSyncSettings();
            }
        }
        public static void AfterMeetingTasks()
        {
            BountyHunter.AfterMeetingTasks();
            SerialKiller.AfterMeetingTasks();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                var role = pc.GetCustomRole();
                if (!pc.Data.IsDead)
                    if (role.IsImpostor() || role.IsCoven() || role.IsNeutralKilling() || role == CustomRoles.Investigator)
                        pc.RpcGuardAndKill(pc);
                if (PlayerControl.GameOptions.MapId != 4) // other than Airship
                    if (pc.Is(CustomRoles.Camouflager))
                    {
                        //main.AirshipMeetingTimer.Add(pc.PlayerId , 0f);
                        Main.AllPlayerKillCooldown[pc.PlayerId] *= 2;
                    }
                if (pc.Is(CustomRoles.EvilGuesser) || pc.Is(CustomRoles.NiceGuesser) || pc.Is(CustomRoles.Pirate))
                {
                    Guesser.IsSkillUsed[pc.PlayerId] = false;
                }
            }
        }

        public static void ChangeInt(ref int ChangeTo, int input, int max)
        {
            var tmp = ChangeTo * 10;
            tmp += input;
            ChangeTo = Math.Clamp(tmp, 0, max);
        }
        public static void CountAliveImpostors()
        {
            int AliveImpostorCount = 0;
            int AllImpostorCount = 0;
            List<PlayerControl> AllImpostors = new();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                CustomRoles pc_role = pc.GetCustomRole();
                if (pc_role.IsImpostor() && !pc.Data.IsDead) AliveImpostorCount++;
                if (pc_role.IsImpostor()) AllImpostors.Add(pc);
                if (pc_role.IsImpostor() || pc_role == CustomRoles.Egoist) AllImpostorCount++;
            }
            TownOfHost.Logger.Info("生存しているインポスター:" + AliveImpostorCount + "人", "CountAliveImpostors");
            Main.AliveImpostorCount = AliveImpostorCount;
            Main.AllImpostorCount = AllImpostorCount;
            Main.Impostors = new();
            Main.Impostors = AllImpostors;
            if (Options.EnableLastImpostor.GetBool() && AliveImpostorCount == 1)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.IsLastImpostor() && pc.Is(CustomRoles.Impostor))
                    {
                        pc.RpcSetCustomRole(CustomRoles.LastImpostor);
                        break;
                    }
                }
                NotifyRoles(isMeeting: GameStates.IsMeeting);
                CustomSyncAllSettings();
            }
        }
        public static string GetAllRoleName(byte playerId)
        {
            return GetPlayerById(playerId)?.GetAllRoleName() ?? "";
        }
        public static string GetNameWithRole(byte playerId)
        {
            return GetPlayerById(playerId)?.GetNameWithRole() ?? "";
        }
        public static string GetNameWithRole(this GameData.PlayerInfo player)
        {
            return GetPlayerById(player.PlayerId)?.GetNameWithRole() ?? "";
        }
        public static string GetVoteName(byte num)
        {
            string name = "invalid";
            var player = GetPlayerById(num);
            if (num < 15 && player != null) name = player?.GetNameWithRole();
            if (num == 253) name = "Skip";
            if (num == 254) name = "None";
            if (num == 255) name = "Dead";
            return name;
        }
        public static byte GetVoteID(byte num)
        {
            byte name = 0;
            var player = GetPlayerById(num);
            if (num < 15 && player != null) name = player.PlayerId;
            return name;
        }
        public static string PadRightV2(this object text, int num)
        {
            int bc = 0;
            var t = text.ToString();
            foreach (char c in t) bc += Encoding.GetEncoding("UTF-8").GetByteCount(c.ToString()) == 1 ? 1 : 2;
            return t?.PadRight(Mathf.Max(num - (bc - t.Length), 0));
        }
        public static void DumpLog()
        {
            string t = DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss");
            string filename = $"{System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)}/TownOfHost-v{Main.PluginVersion}-{t}.log";
            FileInfo file = new(@$"{System.Environment.CurrentDirectory}/BepInEx/LogOutput.log");
            file.CopyTo(@filename);
            System.Diagnostics.Process.Start(@$"{System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)}");
            if (PlayerControl.LocalPlayer != null)
                HudManager.Instance?.Chat?.AddChat(PlayerControl.LocalPlayer, "デスクトップにログを保存しました。バグ報告チケットを作成してこのファイルを添付してください。");
        }
        public static (int, int) GetDousedPlayerCount(byte playerId)
        {
            int doused = 0, all = 0; //学校で習った書き方
                                     //多分この方がMain.isDousedでforeachするより他のアーソニストの分ループ数少なくて済む
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc == null ||
                    pc.Data.IsDead ||
                    pc.Data.Disconnected ||
                    pc.Is(CustomRoles.Phantom) ||
                    pc.PlayerId == playerId
                ) continue; //塗れない人は除外 (死んでたり切断済みだったり あとアーソニスト自身も)

                all++;
                if (Main.isDoused.TryGetValue((playerId, pc.PlayerId), out var isDoused) && isDoused)
                    //塗れている場合
                    doused++;
            }

            return (doused, all);
        }
        public static (int, int) GetHexedPlayerCount(byte playerId)
        {
            int hexed = 0, all = 0; //学校で習った書き方
                                    //多分この方がMain.isDousedでforeachするより他のアーソニストの分ループ数少なくて済む
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc == null ||
                    pc.Data.IsDead ||
                    pc.Data.Disconnected ||
                    pc.Is(CustomRoles.Phantom) ||
                    //!pc.GetCustomRole().IsCoven() ||
                    pc.PlayerId == playerId
                ) continue; //塗れない人は除外 (死んでたり切断済みだったり あとアーソニスト自身も)

                if (!pc.GetCustomRole().IsCoven())
                    all++;
                if (Main.isHexed.TryGetValue((playerId, pc.PlayerId), out var isHexed) && isHexed)
                    //塗れている場合
                    hexed++;
            }

            return (hexed, all);
        }
        public static List<PlayerControl> GetDousedPlayer(byte playerId)
        {
            List<PlayerControl> doused = null;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc == null ||
                    pc.Data.IsDead ||
                    pc.Is(CustomRoles.Phantom) ||
                    pc.Data.Disconnected ||
                    pc.PlayerId == playerId
                ) continue; //塗れない人は除外 (死んでたり切断済みだったり あとアーソニスト自身も)

                //all++;
                if (Main.isDoused.TryGetValue((playerId, pc.PlayerId), out var isDoused) && isDoused)
                    doused.Add(GetPlayerById(pc.PlayerId));
            }

            return doused;
        }
        public static (int, int) GetInfectedPlayerCount(byte playerId)
        {
            int infected = 0, all = 0;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc == null ||
                    pc.Data.IsDead ||
                    pc.Is(CustomRoles.Phantom) ||
                    pc.Data.Disconnected ||
                    pc.PlayerId == playerId
                ) continue;

                all++;
                if (Main.isInfected.TryGetValue((playerId, pc.PlayerId), out var isInfected) && isInfected)
                    infected++;
            }

            return (infected, all);
        }
        public static string SummaryTexts(byte id, bool disableColor = true)
        {
            string summary = $"{Helpers.ColorString(Main.PlayerColors[id], Main.AllPlayerNames[id])}<pos=25%> {Helpers.ColorString(GetRoleColor(Main.AllPlayerCustomRoles[id]), GetRoleName(Main.AllPlayerCustomRoles[id]))}{GetShowLastSubRolesText(id)}</pos><pos=44%> {GetProgressText(id)}</pos><pos=51%> {GetVitalText(id)}</pos>";
            return disableColor ? summary.RemoveHtmlTags() : Regex.Replace(summary, " ", "");
        }
        public static string RemoveHtmlTags(this string str) => Regex.Replace(str, "<[^>]*?>", "");
        public static bool CanMafiaKill()
        {
            if (Main.AllPlayerCustomRoles == null) return false;
            //マフィアを除いた生きているインポスターの人数  Number of Living Impostors excluding mafia
            int LivingImpostorsNum = 0;
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                var role = pc.GetCustomRole();
                if (!pc.Data.IsDead && role != CustomRoles.Mafia && role.IsImpostor()) LivingImpostorsNum++;
            }

            return LivingImpostorsNum <= 0;
        }
    }
}
