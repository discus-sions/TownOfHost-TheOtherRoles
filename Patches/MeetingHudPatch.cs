using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;
using static TownOfHost.Translator;

namespace TownOfHost
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
    class CheckForEndVotingPatch
    {
        public static bool Prefix(MeetingHud __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            try
            {
                foreach (var pva in __instance.playerStates)
                {
                    if (pva == null) continue;
                    PlayerControl pc = Utils.GetPlayerById(pva.TargetPlayerId);
                    if (pc == null) continue;
                    //死んでいないディクテーターが投票済み
                    if (pc.Is(CustomRoles.Dictator) && pva.DidVote && pc.PlayerId != pva.VotedFor && pva.VotedFor < 253 && !pc.Data.IsDead)
                    {
                        var voteTarget = Utils.GetPlayerById(pva.VotedFor);
                        Main.AfterMeetingDeathPlayers.TryAdd(pc.PlayerId, PlayerState.DeathReason.Suicide);
                        __instance.RpcVotingComplete(new MeetingHud.VoterState[]{ new ()
                        {
                            VoterId = pva.TargetPlayerId,
                            VotedForId = pva.VotedFor
                        }}, voteTarget.Data, false); //RPC
                        Logger.Info($"{voteTarget.GetNameWithRole()}を追放", "Dictator");
                        Logger.Info("ディクテーターによる強制会議終了", "Special Phase");
                        return true;
                    }
                }
                foreach (var ps in __instance.playerStates)
                {
                    //死んでいないプレイヤーが投票していない
                    if (!(ps.AmDead || ps.DidVote)) return false;
                }

                MeetingHud.VoterState[] states;
                GameData.PlayerInfo exiledPlayer = PlayerControl.LocalPlayer.Data;
                bool tie = false;

                List<MeetingHud.VoterState> statesList = new();
                for (var i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea ps = __instance.playerStates[i];
                    if (ps == null) continue;
                    Logger.Info(string.Format("{0,-2}{1}:{2,-3}{3}", ps.TargetPlayerId, Utils.PadRightV2($"({Utils.GetVoteName(ps.TargetPlayerId)})", 40), ps.VotedFor, $"({Utils.GetVoteName(ps.VotedFor)})"), "Vote");
                    var voter = Utils.GetPlayerById(ps.TargetPlayerId);
                    if (voter == null || voter.Data == null || voter.Data.Disconnected) continue;
                    if (Options.VoteMode.GetBool())
                    {
                        if (ps.VotedFor == 253 && !voter.Data.IsDead)//スキップ
                        {
                            switch (Options.GetWhenSkipVote())
                            {
                                case VoteMode.Suicide:
                                    Main.AfterMeetingDeathPlayers.TryAdd(ps.TargetPlayerId, PlayerState.DeathReason.Suicide);
                                    Logger.Info($"スキップしたため{voter.GetNameWithRole()}を自殺させました", "Vote");
                                    break;
                                case VoteMode.SelfVote:
                                    ps.VotedFor = ps.TargetPlayerId;
                                    Logger.Info($"スキップしたため{voter.GetNameWithRole()}に自投票させました", "Vote");
                                    break;
                                default:
                                    break;
                            }
                        }
                        if (ps.VotedFor == 254 && !voter.Data.IsDead)//無投票
                        {
                            switch (Options.GetWhenNonVote())
                            {
                                case VoteMode.Suicide:
                                    Main.AfterMeetingDeathPlayers.TryAdd(ps.TargetPlayerId, PlayerState.DeathReason.Suicide);
                                    Logger.Info($"無投票のため{voter.GetNameWithRole()}を自殺させました", "Vote");
                                    break;
                                case VoteMode.SelfVote:
                                    ps.VotedFor = ps.TargetPlayerId;
                                    Logger.Info($"無投票のため{voter.GetNameWithRole()}に自投票させました", "Vote");
                                    break;
                                case VoteMode.Skip:
                                    ps.VotedFor = 253;
                                    Logger.Info($"無投票のため{voter.GetNameWithRole()}にスキップさせました", "Vote");
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    statesList.Add(new MeetingHud.VoterState()
                    {
                        VoterId = ps.TargetPlayerId,
                        VotedForId = ps.VotedFor
                    });
                    if (IsMayor(ps.TargetPlayerId))//Mayorの投票数
                    {
                        for (var i2 = 0; i2 < Options.MayorAdditionalVote.GetFloat(); i2++)
                        {
                            statesList.Add(new MeetingHud.VoterState()
                            {
                                VoterId = ps.TargetPlayerId,
                                VotedForId = ps.VotedFor
                            });
                        }
                    }
                }
                states = statesList.ToArray();

                var VotingData = __instance.CustomCalculateVotes();
                byte exileId = byte.MaxValue;
                /*var TieBreakerData = __instance.GetTieBreakerInfo();
                byte exileId = byte.MaxValue;
                byte tiebreakerID = 0;
                byte tiebreakerVoted = 0;
                byte tiebreaker = 0;
                if (CustomRoles.TieBreaker.IsEnable())
                {
                    foreach (var role in Main.AllPlayerCustomSubRoles)
                    {
                        if (role.Value == CustomRoles.TieBreaker)
                            tiebreakerID = role.Key;
                    }
                    foreach (var data in TieBreakerData)
                    {
                        if (Utils.GetPlayerById(data.Key).Is(CustomRoles.TieBreaker))
                        {
                            tiebreakerVoted = __instance.GetTieBreakerVote();
                        }
                    }
                }*/
                int max = 0;
                Logger.Info("===追放者確認処理開始===", "Vote");
                byte left = 0;
                int right = 0;
                foreach (var data in VotingData)
                {
                    Logger.Info($"{data.Key}({Utils.GetVoteName(data.Key)}):{data.Value}票", "Vote");
                    if (data.Value > max)
                    {
                        Logger.Info(data.Key + "番が最高値を更新(" + data.Value + ")", "Vote");
                        exileId = data.Key;
                        max = data.Value;
                        tie = false;
                    }
                    else if (data.Value == max)
                    {
                        Logger.Info(data.Key + "番が" + exileId + "番と同数(" + data.Value + ")", "Vote");
                        exileId = byte.MaxValue;
                        tie = true;
                    }
                }

                if (Utils.GetPlayerById(exileId).GetCustomRole() == CustomRoles.EvilGuesser)
                {
                    if (!Guesser.IsEvilGuesserMeeting)
                    {
                        Guesser.isEvilGuesserExiled[exileId] = true;
                        MeetingHud.Instance.RpcClose();
                        return false;
                    }
                    if (Guesser.IsEvilGuesserMeeting)
                    {
                        Guesser.IsEvilGuesserMeeting = false;
                        Guesser.isEvilGuesserExiled[exileId] = false;
                    }
                }

                Logger.Info($"追放者決定: {exileId}({Utils.GetVoteName(exileId)})", "Vote");
                exiledPlayer = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(info => !tie && info.PlayerId == exileId);

                //RPC
                if (AntiBlackout.OverrideExiledPlayer)
                {
                    __instance.RpcVotingComplete(states, null, true);
                    ExileControllerWrapUpPatch.AntiBlackout_LastExiled = exiledPlayer;
                }
                else __instance.RpcVotingComplete(states, exiledPlayer, tie); //通常処理
                if (!Utils.GetPlayerById(exileId).Is(CustomRoles.Witch))
                {
                    foreach (var p in Main.SpelledPlayer)
                    {
                        Main.AfterMeetingDeathPlayers.TryAdd(p.PlayerId, PlayerState.DeathReason.Spell);
                        if (Main.ExecutionerTarget.ContainsValue(p.PlayerId) && exileId != p.PlayerId)
                        {
                            byte Executioner = 0x73;
                            Main.ExecutionerTarget.Do(x =>
                            {
                                if (x.Value == p.PlayerId)
                                    Executioner = x.Key;
                            });
                            Utils.GetPlayerById(Executioner).RpcSetCustomRole(Options.CRoleExecutionerChangeRoles[Options.ExecutionerChangeRolesAfterTargetKilled.GetSelection()]);
                            Main.ExecutionerTarget.Remove(Executioner);
                            RPC.RemoveExecutionerKey(Executioner);
                            Utils.NotifyRoles();
                        }
                        if (Main.GuardianAngelTarget.ContainsValue(p.PlayerId) && exileId != p.PlayerId)
                        {
                            byte GA = 0x73;
                            Main.GuardianAngelTarget.Do(x =>
                            {
                                if (x.Value == p.PlayerId)
                                    GA = x.Key;
                            });
                            Utils.GetPlayerById(GA).RpcSetCustomRole(Options.CRoleGuardianAngelChangeRoles[Options.WhenGaTargetDies.GetSelection()]);
                            Main.GuardianAngelTarget.Remove(GA);
                            RPC.RemoveGAKey(GA);
                            Utils.NotifyRoles();
                        }
                    }
                }
                var realName = exiledPlayer.Object.GetRealName(isMeeting: true);
                Main.LastVotedPlayer = realName;
                if (Main.RealOptionsData.ConfirmImpostor)
                {
                    if (exiledPlayer.PlayerId == exileId)
                    {
                        var player = Utils.GetPlayerById(exiledPlayer.PlayerId);
                        var role = GetString(exiledPlayer.GetCustomRole().ToString());
                        var coloredRole = Helpers.ColorString(Utils.GetRoleColor(exiledPlayer.GetCustomRole()), $"{role}");
                        var name = "";
                        //player?.Data?.PlayerName = $"{realName} was The {coloredRole}.<size=0>";
                        name = realName + " was The " + coloredRole + ".<size=0>";
                        player.Data.PlayerName = name;
                    }
                }
                if (exiledPlayer.Object.CurrentlyLastImpostor())
                {
                    //impostor was voted.
                    PlayerControl votedOut = Utils.GetPlayerById(exiledPlayer.Object.PlayerId);
                    //bool LocalPlayerKnowsImpostor = false;
                    if (Sheriff.SheriffCorrupted.GetBool())
                    {
                        int IsAlive = 0;
                        int numCovenAlive = 0;
                        int numNKalive = 0;
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (!pc.Data.IsDead)
                            {
                                IsAlive++;
                                if (pc.GetCustomRole().IsNeutralKilling() && !Sheriff.TraitorCanSpawnIfNK.GetBool())
                                    numNKalive++;
                                if (pc.GetCustomRole().IsCoven() && !Sheriff.TraitorCanSpawnIfCoven.GetBool())
                                    numCovenAlive++;
                                if (pc.Is(CustomRoles.Sheriff))
                                    Sheriff.seer = pc;
                            }
                        }

                        //foreach (var pva in __instance.playerStates)
                        if (IsAlive >= Sheriff.PlayersForTraitor.GetFloat())
                        {
                            if (numCovenAlive == 0 && numNKalive == 0)
                            {
                                Sheriff.seer.RpcSetCustomRole(CustomRoles.CorruptedSheriff);
                            }
                        }
                    }
                }
                else
                {
                    //still check anyway
                    if (Sheriff.SheriffCorrupted.GetBool())
                    {
                        int IsAlive = 0;
                        int numCovenAlive = 0;
                        int numNKalive = 0;
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (!pc.Data.IsDead)
                            {
                                IsAlive++;
                                if (pc.GetCustomRole().IsNeutralKilling() && !Sheriff.TraitorCanSpawnIfNK.GetBool())
                                    numNKalive++;
                                if (pc.GetCustomRole().IsCoven() && !Sheriff.TraitorCanSpawnIfCoven.GetBool())
                                    numCovenAlive++;
                                if (pc.Is(CustomRoles.Sheriff))
                                    Sheriff.seer = pc;
                            }
                        }

                        //foreach (var pva in __instance.playerStates)
                        if (IsAlive >= Sheriff.PlayersForTraitor.GetFloat())
                        {
                            if (numCovenAlive == 0 && numNKalive == 0)
                            {
                                Sheriff.seer.RpcSetCustomRole(CustomRoles.CorruptedSheriff);
                            }
                        }
                    }
                }
                Main.SpelledPlayer.Clear();
                Main.SilencedPlayer.Clear();
                Main.firstKill.Clear();
                Main.VettedThisRound = false;
                Main.VetIsAlerted = false;
                Main.IsRampaged = false;
                Main.RampageReady = false;

                if (CustomRoles.Lovers.IsEnable() && Main.isLoversDead == false && Main.LoversPlayers.Find(lp => lp.PlayerId == exileId) != null)
                {
                    FixedUpdatePatch.LoversSuicide(exiledPlayer.PlayerId, true);
                }

                //霊界用暗転バグ対処
                if (!AntiBlackout.OverrideExiledPlayer && exiledPlayer != null && Main.ResetCamPlayerList.Contains(exiledPlayer.PlayerId))
                    exiledPlayer.Object?.ResetPlayerCam(19f);

                return false;
            }
            catch (Exception ex)
            {
                Logger.SendInGame(string.Format(GetString("Error.MeetingException"), ex.Message), true);
                throw;
            }
        }
        public static bool IsMayor(byte id)
        {
            var player = PlayerControl.AllPlayerControls.ToArray().Where(pc => pc.PlayerId == id).FirstOrDefault();
            return player != null && player.Is(CustomRoles.Mayor);
        }
        public static bool IsTieBreaker(byte id)
        {
            var player = PlayerControl.AllPlayerControls.ToArray().Where(pc => pc.PlayerId == id).FirstOrDefault();
            return player != null && player.Is(CustomRoles.TieBreaker);
        }
    }

    static class ExtendedMeetingHud
    {
        public static Dictionary<byte, int> CustomCalculateVotes(this MeetingHud __instance)
        {
            Logger.Info("CustomCalculateVotes開始", "Vote");
            Dictionary<byte, int> dic = new();
            //| 投票された人 | 投票された回数 |
            for (int i = 0; i < __instance.playerStates.Length; i++)
            {
                PlayerVoteArea ps = __instance.playerStates[i];
                if (ps == null) continue;
                if (ps.VotedFor is not ((byte)252) and not byte.MaxValue and not ((byte)254))
                {
                    int VoteNum = 1;
                    if (CheckForEndVotingPatch.IsMayor(ps.TargetPlayerId)) VoteNum += Options.MayorAdditionalVote.GetInt();
                    //投票を1追加 キーが定義されていない場合は1で上書きして定義
                    dic[ps.VotedFor] = !dic.TryGetValue(ps.VotedFor, out int num) ? VoteNum : num + VoteNum;
                }
            }
            return dic;
        }

        public static Dictionary<byte, int> GetTieBreakerInfo(this MeetingHud __instance)
        {
            Logger.Info("CustomCalculateVotes開始", "Vote");
            Dictionary<byte, int> dic = new();
            //| 投票された人 | 投票された回数 |
            for (int i = 0; i < __instance.playerStates.Length; i++)
            {
                PlayerVoteArea ps = __instance.playerStates[i];
                if (ps == null) continue;
                if (CheckForEndVotingPatch.IsTieBreaker(ps.TargetPlayerId)) continue;
                if (ps.VotedFor is not ((byte)252) and not byte.MaxValue and not ((byte)254))
                {
                    int VoteNum = 1;
                    dic[ps.VotedFor] = !dic.TryGetValue(ps.VotedFor, out int num) ? VoteNum : num + VoteNum;
                }
            }
            return dic;
        }
        public static byte GetTieBreakerVote(this MeetingHud __instance)
        {
            if (CustomRoles.TieBreaker.IsEnable())
            {
                Logger.Info("CustomCalculateVotes開始", "Vote");
                byte dic = new();
                //| 投票された人 | 投票された回数 |
                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea ps = __instance.playerStates[i];
                    if (ps == null) continue;
                    if (CheckForEndVotingPatch.IsTieBreaker(ps.TargetPlayerId)) continue;
                    if (ps.VotedFor is not ((byte)252) and not byte.MaxValue and not ((byte)254))
                    {
                        dic = Utils.GetVoteID(ps.VotedFor);
                    }
                }
                return dic;
            }
            else
            {
                return 0;
            }
        }
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    class MeetingHudStartPatch
    {
        public static void Prefix(MeetingHud __instance)
        {
            Logger.Info("------------会議開始------------", "Phase");
            Main.witchMeeting = true;
            Utils.NotifyRoles(isMeeting: true, ForceLoop: true);
            Main.witchMeeting = false;
            Ninja.NewNinjaKillTarget();
        }
        public static void Postfix(MeetingHud __instance)
        {
            foreach (var pva in __instance.playerStates)
            {
                var pc = Utils.GetPlayerById(pva.TargetPlayerId);
                if (pc == null) continue;
                var RoleTextData = Utils.GetRoleText(pc);
                var roleTextMeeting = UnityEngine.Object.Instantiate(pva.NameText);
                roleTextMeeting.transform.SetParent(pva.NameText.transform);
                roleTextMeeting.transform.localPosition = new Vector3(0f, -0.18f, 0f);
                roleTextMeeting.fontSize = 1.5f;
                roleTextMeeting.text = RoleTextData.Item1;
                if (Main.VisibleTasksCount) roleTextMeeting.text += Utils.GetProgressText(pc);
                roleTextMeeting.color = RoleTextData.Item2;
                roleTextMeeting.gameObject.name = "RoleTextMeeting";
                roleTextMeeting.enableWordWrapping = false;
                roleTextMeeting.enabled =
                    pva.TargetPlayerId == PlayerControl.LocalPlayer.PlayerId ||
                    (Main.VisibleTasksCount && PlayerControl.LocalPlayer.Data.IsDead && Options.GhostCanSeeOtherRoles.GetBool()) ||
                    Options.EnableGM.GetBool();
            }
            if (Options.SyncButtonMode.GetBool())
            {
                if (AmongUsClient.Instance.AmHost) PlayerControl.LocalPlayer.RpcSetName("test");
                Utils.SendMessage(string.Format(GetString("Message.SyncButtonLeft"), Options.SyncedButtonCount.GetFloat() - Options.UsedButtonCount));
                Logger.Info("緊急会議ボタンはあと" + (Options.SyncedButtonCount.GetFloat() - Options.UsedButtonCount) + "回使用可能です。", "SyncButtonMode");
            }
            if (AntiBlackout.OverrideExiledPlayer)
            {
                Utils.SendMessage(Translator.GetString("Warning.OverrideExiledPlayer"));
            }

            if (AmongUsClient.Instance.AmHost)
            {
                _ = new LateTask(() =>
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        pc.RpcSetNameEx(pc.GetRealName(isMeeting: true));
                    }
                }, 3f, "SetName To Chat");
            }

            foreach (var protect in Main.GuardianAngelTarget)
            {
                PlayerControl ga = Utils.GetPlayerById(protect.Key);
                PlayerControl protecting = Utils.GetPlayerById(protect.Value);
                if (!ga.Data.IsDead)
                {
                    if (Options.GAknowsRole.GetBool())
                        Utils.SendMessage("You are a Guardian Angel. Your Job is to protect your target from Death. Your target's role is: " + Utils.GetRoleName(protecting.GetCustomRole()), ga.PlayerId);
                }
                if (Options.TargetKnowsGA.GetBool() && !protecting.Data.IsDead)
                    Utils.SendMessage("You have a Guardian Angel. Find out who they are and keep them to protect you.", protecting.PlayerId);
            }

            foreach (var ar in PlayerControl.AllPlayerControls)
                if (ar.IsHexedDone())
                    Utils.SendMessage("The Hex Master is done hexing people. If they survive the current meeting, they will kill everyone with a hex bomb!");


            foreach (var pva in __instance.playerStates)
            {
                if (pva == null) continue;
                PlayerControl seer = PlayerControl.LocalPlayer;
                PlayerControl target = Utils.GetPlayerById(pva.TargetPlayerId);
                if (target == null) continue;

                //会議画面での名前変更
                //自分自身の名前の色を変更
                if (target != null && target.AmOwner && AmongUsClient.Instance.IsGameStarted) //変更先が自分自身
                    pva.NameText.color = seer.GetRoleColor();//名前の色を変更

                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc == null ||
                        pc.Data.IsDead ||
                        pc.Data.Disconnected ||
                        pc.PlayerId == seer.PlayerId
                    ) continue; //塗れない人は除外 (死んでたり切断済みだったり あとアーソニスト自身も)

                    if (Main.isDoused.TryGetValue((seer.PlayerId, pc.PlayerId), out var isDoused) && isDoused)
                        Utils.SendMessage("You have been hexed by the Hex Master!", pc.PlayerId);
                }

                //とりあえずSnitchは会議中にもインポスターを確認することができる仕様にしていますが、変更する可能性があります。

                //インポスター表示
                bool LocalPlayerKnowsImpostor = false; //203行目のif文で使う trueの時にインポスターの名前を赤くする
                bool LocalPlayerKnowsCoven = false;

                switch (seer.GetCustomRole().GetRoleType())
                {
                    case RoleType.Impostor:
                        if (target.Is(CustomRoles.Snitch) && //変更対象がSnitch
                        target.GetPlayerTaskState().DoExpose) //変更対象のタスクが終わりそう)
                            pva.NameText.text += Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Snitch), "★"); //変更対象にSnitchマークをつける
                        break;
                    case RoleType.Coven:
                        if (target.Is(CustomRoles.Snitch) && //変更対象がSnitch
                            target.GetPlayerTaskState().DoExpose && Options.SnitchCanFindCoven.GetBool()) //変更対象のタスクが終わりそう)
                            pva.NameText.text += Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Snitch), "★"); //変更対象にSnitchマークをつける
                        LocalPlayerKnowsCoven = true;
                        break;
                }
                switch (seer.GetCustomRole())
                {
                    case CustomRoles.MadSnitch:
                    case CustomRoles.Snitch:
                        if (seer.GetPlayerTaskState().IsTaskFinished) //seerがタスクを終えている
                            LocalPlayerKnowsImpostor = true;
                        if (seer.GetPlayerTaskState().IsTaskFinished && Options.SnitchCanFindCoven.GetBool()) //seerがタスクを終えている
                            LocalPlayerKnowsCoven = true;
                        break;
                    case CustomRoles.CorruptedSheriff:
                        LocalPlayerKnowsImpostor = true;
                        break;
                    case CustomRoles.Doctor:
                        if (target.Data.IsDead) //変更対象が死人
                            pva.NameText.text += $"({Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Doctor), Utils.GetVitalText(target.PlayerId))})";
                        break;
                    case CustomRoles.Arsonist:
                        if (seer.IsDousedPlayer(target)) //seerがtargetに既にオイルを塗っている(完了)
                            pva.NameText.text += Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Arsonist), "▲");
                        break;
                    case CustomRoles.HexMaster:
                        if (seer.IsHexedPlayer(target)) //seerがtargetに既にオイルを塗っている(完了)
                            pva.NameText.text += Helpers.ColorString(Utils.GetRoleColor(CustomRoles.HexMaster), "†");
                        break;
                    case CustomRoles.PlagueBearer:
                        if (seer.IsInfectedPlayer(target)) //seerがtargetに既にオイルを塗っている(完了)
                            pva.NameText.text += Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Pestilence), "▲");
                        break;
                    case CustomRoles.Executioner:
                        if (Main.ExecutionerTarget.TryGetValue(seer.PlayerId, out var targetId) && target.PlayerId == targetId) //targetがValue
                            pva.NameText.text += Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Executioner), "♦");
                        break;
                    case CustomRoles.GuardianAngelTOU:
                        if (Main.GuardianAngelTarget.TryGetValue(seer.PlayerId, out var protectId) && target.PlayerId == protectId) //targetがValue
                            pva.NameText.text += Helpers.ColorString(Utils.GetRoleColor(CustomRoles.GuardianAngelTOU), "♦");
                        break;
                    case CustomRoles.Investigator:
                        if (Investigator.hasSeered[target.PlayerId] == true)
                        {
                            // Investigator has Seered Player.
                            if (target.Is(CustomRoles.CorruptedSheriff))
                            {
                                if (Investigator.CSheriffSwitches.GetBool())
                                {
                                    pva.NameText.text = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Impostor), pva.NameText.text);
                                }
                                else
                                {
                                    if (Investigator.SeeredCSheriff)
                                        pva.NameText.text = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Impostor), pva.NameText.text);
                                    else
                                        pva.NameText.text = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.TheGlitch), pva.NameText.text);
                                }
                            }
                            else
                            {
                                if (Investigator.IsRed(target))
                                {
                                    if (target.GetCustomRole().IsCoven())
                                    {
                                        if (Investigator.CovenIsPurple.GetBool())
                                            pva.NameText.text = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Coven), pva.NameText.text); //targetの名前をエゴイスト色で表示
                                        else
                                            pva.NameText.text = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Impostor), pva.NameText.text); //targetの名前をエゴイスト色で表示
                                    }
                                    else
                                    {
                                        pva.NameText.text = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Impostor), pva.NameText.text);
                                    }
                                }
                                else
                                {
                                    pva.NameText.text = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.TheGlitch), pva.NameText.text); //targetの名前をエゴイスト色で表示
                                }
                            }
                        }
                        break;
                }

                switch (target.GetCustomRole())
                {
                    case CustomRoles.Egoist:
                        if (seer.GetCustomRole().IsImpostor() || //seerがImpostor
                        seer.Is(CustomRoles.EgoSchrodingerCat)) //またはEgoSchrodingerCat
                            pva.NameText.color = Utils.GetRoleColor(CustomRoles.Egoist);//変更対象の名前をエゴイスト色にする
                        break;
                    case CustomRoles.Jackal:
                        if (seer.GetCustomRole().IsJackalTeam())
                            pva.NameText.color = Utils.GetRoleColor(CustomRoles.Jackal);//変更対象の名前をジャッカル色にする
                        break;
                    case CustomRoles.Lovers:
                        if (seer.Is(CustomRoles.Lovers) || seer.Data.IsDead)
                            pva.NameText.text += Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Lovers), "♡");
                        break;
                    case CustomRoles.Child:
                        if (Options.ChildKnown.GetBool() == true)
                            pva.NameText.text += Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Jackal), " (C)");
                        break;
                    case CustomRoles.CorruptedSheriff:
                        LocalPlayerKnowsImpostor = true;
                        break;
                    case CustomRoles.Sleuth:
                        //pva.NameText.text += Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Impostor), " (S)");
                        /*if (Options.SleuthReport.GetBool() == false)
                        {
                            //Utils.SendMessage("", target.PlayerId);
                            //Utils.LastResult(target.PlayerId);
                            foreach (var pc in PlayerControl.AllPlayerControls)
                            {
                                if (pc.Data.IsDead)
                                {
                                    Utils.GetRoleName(pc.GetCustomRole());
                                }
                            }
                        }
                        else
                        {
                            Utils.SendMessage("There hasn't been a way for Sleuth to find roles by reporting. For now, you just see this message.", target.PlayerId);
                        }*/
                        break;
                }

                if (LocalPlayerKnowsImpostor)
                {
                    if (target != null && target.GetCustomRole().IsImpostor()) //変更先がインポスター
                        pva.NameText.color = Palette.ImpostorRed; //変更対象の名前を赤くする
                }
                if (LocalPlayerKnowsCoven)
                {
                    if (target != null && target.GetCustomRole().IsCoven()) //変更先がインポスター
                        pva.NameText.color = Utils.GetRoleColor(CustomRoles.Coven); //変更対象の名前を赤くする
                }
                //呪われている場合
                if (Main.SpelledPlayer.Find(x => x.PlayerId == target.PlayerId) != null)
                    pva.NameText.text += Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Impostor), "†");

                if (Main.SilencedPlayer.Count != 0)
                {
                    Utils.SendMessage("Some people are Silenced! While they may have 2 crosses next to their name, they are silenced. Being silenced means you cannot talk.", target.PlayerId);
                }
                //if (target.GetCustomSubRole().GetModifierType() != ModifierType.None)
                //    {
                //      Utils.SendMessage("You have a modifier. Your modifier is: " + target.GetSubRoleName() + ".", target.PlayerId);
                //  }
                if (Main.SilencedPlayer.Find(x => x.PlayerId == target.PlayerId) != null)
                {
                    if (!pva.AmDead)
                    {
                        pva.NameText.text += Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Impostor), " (S)");
                        Utils.SendMessage("You are currently Silenced. Try talking again when you aren't silenced. Even though you may be able to talk, please don't and wait until you are no longer silenced.", target.PlayerId);
                    }
                }


                //会議画面ではインポスター自身の名前にSnitchマークはつけません。
            }
        }
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    class MeetingHudUpdatePatch
    {
        public static void Postfix(MeetingHud __instance)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (Input.GetMouseButtonUp(1) && Input.GetKey(KeyCode.LeftControl))
            {
                __instance.playerStates.DoIf(x => x.HighlightedFX.enabled, x =>
                {
                    var player = Utils.GetPlayerById(x.TargetPlayerId);
                    player.RpcExileV2();
                    PlayerState.SetDeathReason(player.PlayerId, PlayerState.DeathReason.Execution);
                    PlayerState.SetDead(player.PlayerId);
                    Main.unreportableBodies.Add(player.PlayerId);
                    Utils.SendMessage(string.Format(GetString("Message.Executed"), player.Data.PlayerName));
                    Logger.Info($"{player.GetNameWithRole()}を処刑しました", "Execution");
                });
            }
            if (Input.GetMouseButtonUp(1) && Input.GetKey(KeyCode.RightControl))
            {
                __instance.playerStates.DoIf(x => x.HighlightedFX.enabled, x =>
                {
                    var player = Utils.GetPlayerById(x.TargetPlayerId);
                    player.RpcExileV2();
                    PlayerState.SetDeathReason(player.PlayerId, PlayerState.DeathReason.Execution);
                    PlayerState.SetDead(player.PlayerId);
                    player.RpcMurderPlayer(player);
                    Main.unreportableBodies.Add(player.PlayerId);
                    Utils.SendMessage(string.Format(GetString("Message.Executed"), player.Data.PlayerName));
                    Logger.Info($"{player.GetNameWithRole()}を処刑しました", "Execution");
                });
            }
            if (Input.GetMouseButtonUp(1) && Input.GetKey(KeyCode.RightAlt))
            {
                __instance.playerStates.DoIf(x => x.HighlightedFX.enabled, x =>
                {
                    var player = Utils.GetPlayerById(x.TargetPlayerId);
                    player.RpcExileV2();
                    PlayerState.SetDeathReason(player.PlayerId, PlayerState.DeathReason.Alive);
                    Main.unreportableBodies.Add(player.PlayerId);
                    Utils.SendMessage(string.Format(GetString("Message.Executed"), player.Data.PlayerName));
                    Logger.Info($"{player.GetNameWithRole()}を処刑しました", "Execution");
                });
            }
        }
    }
    [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetHighlighted))]
    class SetHighlightedPatch
    {
        public static bool Prefix(PlayerVoteArea __instance, bool value)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            if (!__instance.HighlightedFX) return false;
            __instance.HighlightedFX.enabled = value;
            return false;
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.OnDestroy))]
    class MeetingHudOnDestroyPatch
    {
        public static void Postfix()
        {
            Logger.Info("------------会議終了------------", "Phase");
            if (AmongUsClient.Instance.AmHost)
                AntiBlackout.SetIsDead();
            if (Camouflague.IsActive)
            {
                Camouflague.MeetingCause();
                Camouflague.did = false;
            }
        }
    }
}