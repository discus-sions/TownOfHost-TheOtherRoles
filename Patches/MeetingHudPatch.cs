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
                    if (pc.Is(CustomRoles.Dictator) && pva.DidVote && pc.PlayerId != pva.VotedFor && pva.VotedFor < 253 && !pc.Data.IsDead && !IsPhantom(pva.VotedFor))
                    {
                        var voteTarget = Utils.GetPlayerById(pva.VotedFor);
                        if (!Main.AfterMeetingDeathPlayers.ContainsKey(pc.PlayerId))
                            Main.AfterMeetingDeathPlayers.Add(pc.PlayerId, PlayerState.DeathReason.Suicide);
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
                    bool skipVote = false;
                    if (Options.VoteMode.GetBool())
                    {
                        if (ps.VotedFor == 253 && !voter.Data.IsDead)//スキップ
                        {
                            switch (Options.GetWhenSkipVote())
                            {
                                case VoteMode.Suicide:
                                    if (!Main.AfterMeetingDeathPlayers.ContainsKey(ps.TargetPlayerId))
                                        Main.AfterMeetingDeathPlayers.Add(ps.TargetPlayerId, PlayerState.DeathReason.Suicide);
                                    Logger.Info($"{voter.GetNameWithRole()} Suicided because they skipped. (Skip Vote Option)", "Vote");
                                    break;
                                case VoteMode.SelfVote:
                                    ps.VotedFor = ps.TargetPlayerId;
                                    Logger.Info($"{voter.GetNameWithRole()} Self voted because they skipped. (Skip Vote Option)", "Vote");
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
                                    if (!Main.AfterMeetingDeathPlayers.ContainsKey(ps.TargetPlayerId))
                                        Main.AfterMeetingDeathPlayers.Add(ps.TargetPlayerId, PlayerState.DeathReason.Suicide);
                                    Logger.Info($"{voter.GetNameWithRole()} Suicided because they didnt vote. (Non Vote Option)", "Vote");
                                    break;
                                case VoteMode.SelfVote:
                                    ps.VotedFor = ps.TargetPlayerId;
                                    Logger.Info($"{voter.GetNameWithRole()} Self voted because they didn't vote. (Non Vote Option)", "Vote");
                                    break;
                                case VoteMode.Skip:
                                    ps.VotedFor = 253;
                                    Logger.Info($"{voter.GetNameWithRole()} Skipped because they didn't vote. (Non Vote Option)", "Vote");
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    if (voter.GetCustomRole() is CustomRoles.Oracle or CustomRoles.Bodyguard or CustomRoles.Medic)
                    {
                        if (Main.CurrentTarget.ContainsKey(ps.TargetPlayerId))
                            if (Main.CurrentTarget[ps.TargetPlayerId] == 255 && ps.VotedFor != ps.TargetPlayerId && ps.VotedFor != 253 && ps.VotedFor != 254 && ps.VotedFor != 255 && !Main.HasTarget[ps.TargetPlayerId])
                            {
                                skipVote = true;
                                Main.CurrentTarget[ps.TargetPlayerId] = ps.VotedFor;
                                Main.HasTarget[ps.TargetPlayerId] = true;
                                Utils.SendMessage($"You have locked in your target. Your target is: {Utils.GetPlayerById(Main.CurrentTarget[ps.TargetPlayerId]).GetRealName(true)}", ps.TargetPlayerId);
                            }
                    }
                    var votedFor = Utils.GetPlayerById(ps.VotedFor);
                    if (!voter.Is(CustomRoles.Phantom) && !votedFor.Is(CustomRoles.Phantom) && !skipVote)
                        statesList.Add(new MeetingHud.VoterState()
                        {
                            VoterId = ps.TargetPlayerId,
                            VotedForId = ps.VotedFor
                        });
                    if (IsMayor(ps.TargetPlayerId))
                    {
                        for (var i2 = 0; i2 < Options.MayorAdditionalVote.GetFloat(); i2++)
                        {
                            statesList.Add(new MeetingHud.VoterState()
                            {
                                VoterId = Options.MayorVotesAppearBlack.GetBool() ? PlayerControl.LocalPlayer.PlayerId : ps.TargetPlayerId,
                                VotedForId = ps.VotedFor
                            });
                        }
                    }
                    if (IsEvilMayor(ps.TargetPlayerId))
                    {
                        for (var i2 = 0; i2 < Main.MayorUsedButtonCount[ps.TargetPlayerId]; i2++)
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
                int max = 0;
                Logger.Info("===追放者確認処理開始===", "Vote");
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

                Logger.Info($"Voted Person: {exileId}({Utils.GetVoteName(exileId)})", "Vote");
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
                        if (!Main.AfterMeetingDeathPlayers.ContainsKey(p.PlayerId))
                            Main.AfterMeetingDeathPlayers.Add(p.PlayerId, PlayerState.DeathReason.Spell);
                        if (Main.ExecutionerTarget.ContainsValue(p.PlayerId) && exileId != p.PlayerId && Main.ExeCanChangeRoles)
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
                            Utils.NotifyRoles();
                        }
                        if (CustomRoles.LoversRecode.IsEnable() && Main.isLoversDead == false && Main.LoversPlayers.Find(lp => lp.PlayerId == p.PlayerId) != null)
                        {
                            PlayerControl lover = Main.LoversPlayers.ToArray().Where(pc => pc.PlayerId == p.PlayerId).FirstOrDefault();
                            Main.LoversPlayers.Remove(lover);
                            Main.isLoversDead = true;
                            if (Options.LoversDieTogether.GetBool())
                            {
                                foreach (var lp in Main.LoversPlayers)
                                {
                                    if (!lp.Is(CustomRoles.Pestilence))
                                        if (!Main.AfterMeetingDeathPlayers.ContainsKey(lp.PlayerId))
                                            Main.AfterMeetingDeathPlayers.Add(lp.PlayerId, PlayerState.DeathReason.LoversSuicide);
                                    Main.LoversPlayers.Remove(lp);
                                }
                            }
                        }
                    }
                }
                var realName = exiledPlayer.Object.GetRealName(isMeeting: true);
                Main.LastVotedPlayer = realName;
                if (Main.showEjections)
                {
                    if (exiledPlayer.PlayerId == exileId)
                    {
                        var player = Utils.GetPlayerById(exiledPlayer.PlayerId);
                        var role = GetString(exiledPlayer.GetCustomRole().ToString());
                        var crole = exiledPlayer.GetCustomRole();
                        var coloredRole = Helpers.ColorString(Utils.GetRoleColor(exiledPlayer.GetCustomRole()), $"{role}");
                        var name = "";
                        int impnum = 0;
                        int covennum = 0;
                        int neutralnum = 0;
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc == null || pc.Data.IsDead || pc.Data.Disconnected) continue;
                            var pc_role = pc.GetCustomRole();
                            if (pc_role.IsImpostor() && pc != exiledPlayer.Object)
                                impnum++;
                            else if (pc_role.IsNeutralKilling() && pc != exiledPlayer.Object)
                                neutralnum++;
                            else if (pc_role.IsCoven() && pc != exiledPlayer.Object)
                                covennum++;
                        }
                        name = $"{realName} was The {coloredRole}.";
                        if (crole == CustomRoles.Jester)
                            name = $"You feel a sense of dread while voting out {realName}.\nThey turn out to be the {coloredRole}.<size=0>";
                        if (crole != CustomRoles.Jester)
                        {
                            name += "\n";
                            string icomma = covennum + neutralnum != 0 ? ", " : "";
                            string ncomma = covennum + impnum != 0 ? ", " : "";
                            string ccomma = impnum + neutralnum != 0 ? ", " : "";
                            //if (impnum != 0)
                            name += $"{impnum} Impostor(s) remain{icomma}";
                            if (neutralnum != 0)
                                name += $"{neutralnum} Neutral(s) remain{neutralnum}";
                            if (CustomRoles.Coven.IsEnable())
                                name += $"{covennum} Coven remain{ccomma}";
                            name += ".<size=0>";
                        }
                        player.RpcSetName(name);
                    }
                }
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (!AmongUsClient.Instance.AmHost) continue;
                    if (pc.Data.Disconnected || pc == null || pc.Data.IsDead || !pc.Is(CustomRoles.Survivor)) continue;
                    var stuff = Main.SurvivorStuff[pc.PlayerId];
                    stuff.Item2 = false;
                    stuff.Item3 = false;
                    stuff.Item4 = false;
                    stuff.Item5 = false;
                    Main.SurvivorStuff[pc.PlayerId] = stuff;
                }
                Main.SpelledPlayer.Clear();
                Main.SilencedPlayer.Clear();
                Main.firstKill.Clear();
                Main.VettedThisRound = false;
                Main.VetIsAlerted = false;
                Main.IsRampaged = false;
                Main.RampageReady = false;
                List<byte> change = new();
                foreach (var key in Main.HasTarget)
                {
                    if (Utils.GetPlayerById(key.Key).Is(CustomRoles.Crusader))
                        change.Add(key.Key);
                }
                foreach (var id in change)
                {
                    Main.HasTarget[id] = false;
                }

                if (CustomRoles.LoversRecode.IsEnable() && Main.isLoversDead == false && Main.LoversPlayers.Find(lp => lp.PlayerId == exileId) != null)
                {
                    PlayerControl lover = Main.LoversPlayers.ToArray().Where(pc => pc.PlayerId == exileId).FirstOrDefault();
                    Main.LoversPlayers.Remove(lover);
                    Main.isLoversDead = true;
                    if (Options.LoversDieTogether.GetBool())
                    {
                        foreach (var lp in Main.LoversPlayers)
                        {
                            if (!lp.Is(CustomRoles.Pestilence))
                                if (!Main.AfterMeetingDeathPlayers.ContainsKey(lp.PlayerId))
                                    Main.AfterMeetingDeathPlayers.Add(lp.PlayerId, PlayerState.DeathReason.LoversSuicide);
                            Main.LoversPlayers.Remove(lp);
                        }
                    }
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
        public static bool IsPhantom(byte id)
        {
            var player = PlayerControl.AllPlayerControls.ToArray().Where(pc => pc.PlayerId == id).FirstOrDefault();
            return player != null && player.Is(CustomRoles.Phantom);
        }
        public static bool IsMayor(byte id)
        {
            var player = PlayerControl.AllPlayerControls.ToArray().Where(pc => pc.PlayerId == id).FirstOrDefault();
            return player != null && player.Is(CustomRoles.Mayor);
        }
        public static bool IsEvilMayor(byte id)
        {
            var player = PlayerControl.AllPlayerControls.ToArray().Where(pc => pc.PlayerId == id).FirstOrDefault();
            return player != null && player.Is(CustomRoles.VoteStealer);
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
                    if (CheckForEndVotingPatch.IsEvilMayor(ps.TargetPlayerId)) VoteNum += Main.MayorUsedButtonCount[ps.TargetPlayerId];
                    if (CheckForEndVotingPatch.IsPhantom(ps.VotedFor)) VoteNum = -1;
                    if (CheckForEndVotingPatch.IsPhantom(ps.TargetPlayerId) && !CheckForEndVotingPatch.IsPhantom(ps.VotedFor)) VoteNum = -1;
                    // if (VoteNum != 1) Utils.GetPlayerById(ps.TargetPlayerId).SetColor(1);
                    //投票を1追加 キーが定義されていない場合は1で上書きして定義
                    dic[ps.VotedFor] = !dic.TryGetValue(ps.VotedFor, out int num) ? VoteNum : num + VoteNum;
                }
            }
            bool tie = false;
            byte exileId = byte.MaxValue;
            int max = 0;
            foreach (var data in dic)
            {
                if (data.Value > max)
                {
                    exileId = data.Key;
                    max = data.Value;
                    tie = false;
                }
                else if (data.Value == max)
                {
                    exileId = byte.MaxValue;
                    tie = true;
                }
            }
            foreach (var player in __instance.playerStates)
            {
                if (!AmongUsClient.Instance.AmHost) continue;
                if (!player.DidVote
                    || player.AmDead
                    || !tie
                    || player.VotedFor == PlayerVoteArea.MissedVote
                    || player.VotedFor == PlayerVoteArea.DeadVote) continue;

                var modifier = Utils.GetPlayerById(player.TargetPlayerId).GetCustomSubRole();
                if (modifier == CustomRoles.NoSubRoleAssigned) continue;
                if (modifier == CustomRoles.TieBreaker)
                {
                    if (dic.TryGetValue(player.VotedFor, out var num))
                    {
                        dic[player.VotedFor] = num + 1;
                    }
                    else
                        dic[player.VotedFor] = num;
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
            Utils.NotifyRoles(isMeeting: true, ForceLoop: true, startOfMeeting: true);
            Main.witchMeeting = false;
            Ninja.NewNinjaKillTarget();
        }
        public static void Postfix(MeetingHud __instance)
        {
            foreach (var pva in __instance.playerStates)
            {
                if (!Options.RolesLikeToU.GetBool())
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
                        pva.TargetPlayerId == PlayerControl.LocalPlayer.PlayerId || Main.rolesRevealedNextMeeting.Contains(pva.TargetPlayerId) || (PlayerControl.LocalPlayer.GetCustomRole().IsImpostor() && Options.ImpostorKnowsRolesOfTeam.GetBool() && pc.GetCustomRole().IsImpostor()) ||
                        (Main.VisibleTasksCount && PlayerControl.LocalPlayer.Data.IsDead && Options.GhostCanSeeOtherRoles.GetBool()) || (PlayerControl.LocalPlayer.GetCustomRole().IsCoven() && Options.CovenKnowsRolesOfTeam.GetBool() && pc.GetCustomRole().IsCoven());
                }
                else
                {
                    var pc = Utils.GetPlayerById(pva.TargetPlayerId);
                    if (pc == null) continue;
                    bool continues = pva.TargetPlayerId == PlayerControl.LocalPlayer.PlayerId || Main.rolesRevealedNextMeeting.Contains(pva.TargetPlayerId) || (PlayerControl.LocalPlayer.GetCustomRole().IsImpostor() && Options.ImpostorKnowsRolesOfTeam.GetBool() && pc.GetCustomRole().IsImpostor()) ||
                        (Main.VisibleTasksCount && PlayerControl.LocalPlayer.Data.IsDead && Options.GhostCanSeeOtherRoles.GetBool()) || (PlayerControl.LocalPlayer.GetCustomRole().IsCoven() && Options.CovenKnowsRolesOfTeam.GetBool() && pc.GetCustomRole().IsCoven());
                    if (!continues) continue;
                    string name = pva.NameText.text + " ";
                    if (Main.VisibleTasksCount) name += Utils.GetProgressText(pc);
                    name += "\r\n";
                    name += Utils.GetRoleName(pc.GetCustomRole());
                    pva.NameText.text = name;
                    pva.NameText.color = Utils.GetRoleColor(pc.GetCustomRole());
                }
            }
            int numOfPsychicBad = UnityEngine.Random.RandomRange(0, 3);
            numOfPsychicBad = Mathf.RoundToInt(numOfPsychicBad);
            if (numOfPsychicBad > 3) // failsafe
                numOfPsychicBad = 3;
            List<byte> goodids = new();
            List<byte> badids = new();
            Dictionary<byte, bool> isGood = new();
            if (!PlayerControl.LocalPlayer.Data.IsDead)
            {
                if (PlayerControl.LocalPlayer.Is(CustomRoles.Psychic))
                {
                    List<PlayerControl> badPlayers = new();
                    List<PlayerControl> goodPlayers = new();
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc.Data.IsDead || pc.Data.Disconnected || pc.PlayerId == PlayerControl.LocalPlayer.PlayerId || pc == null) continue;
                        isGood.Add(pc.PlayerId, true);
                        var role = pc.GetCustomRole();
                        if (Options.ExeTargetShowsEvil.GetBool())
                            if (Main.ExecutionerTarget.ContainsValue(pc.PlayerId))
                            {
                                badPlayers.Add(pc);
                                isGood[pc.PlayerId] = false;
                                continue;
                            }
                        switch (role)
                        {
                            case CustomRoles.GuardianAngelTOU:
                                if (!Options.GAdependsOnTaregtRole.GetBool()) break;
                                Main.GuardianAngelTarget.TryGetValue(pc.PlayerId, out var protectId);
                                if (!Utils.GetPlayerById(protectId).GetCustomRole().IsCrewmate())
                                    badPlayers.Add(pc);
                                break;
                        }
                        switch (role.GetRoleType())
                        {
                            case RoleType.Crewmate:
                                if (!Options.CkshowEvil.GetBool()) break;
                                if (role is CustomRoles.Sheriff or CustomRoles.Veteran or CustomRoles.Bodyguard or CustomRoles.Crusader or CustomRoles.Child or CustomRoles.Bastion or CustomRoles.Demolitionist or CustomRoles.NiceGuesser) badPlayers.Add(pc);
                                break;
                            case RoleType.Impostor:
                                badPlayers.Add(pc);
                                isGood[pc.PlayerId] = false;
                                break;
                            case RoleType.Neutral:
                                if (role.IsNeutralKilling()) badPlayers.Add(pc);
                                if (Options.NBshowEvil.GetBool())
                                    if (role is CustomRoles.Opportunist or CustomRoles.Survivor or CustomRoles.GuardianAngelTOU or CustomRoles.Amnesiac or CustomRoles.SchrodingerCat) badPlayers.Add(pc);
                                if (Options.NEshowEvil.GetBool())
                                    if (role is CustomRoles.Jester or CustomRoles.Terrorist or CustomRoles.Executioner or CustomRoles.Swapper or CustomRoles.Hacker or CustomRoles.Vulture) badPlayers.Add(pc);
                                break;
                            case RoleType.Madmate:
                                if (!Options.MadmatesAreEvil.GetBool()) break;
                                badPlayers.Add(pc);
                                isGood[pc.PlayerId] = false;
                                break;
                        }
                        if (isGood[pc.PlayerId]) goodPlayers.Add(pc);
                    }
                    List<byte> badpcids = new();
                    foreach (var p in badPlayers)
                    {
                        badpcids.Add(p.PlayerId);
                    }
                    if (numOfPsychicBad > badPlayers.Count) numOfPsychicBad = badPlayers.Count;
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
                    HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, "Your list of names are:");
                }
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
                        if (pc == null || pc.Data.Disconnected) continue;
                        pc.RpcSetNameEx(pc.GetRealName(isMeeting: true));
                    }
                    _ = new LateTask(() =>
                    {
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc == null || pc.Data.Disconnected) continue;
                            pc.RpcSetNamePrivate(Helpers.ColorString(Utils.GetRoleColor(pc.GetCustomRole()), pc.GetRealName(isMeeting: true)), true, pc);
                        }
                    }, 3f, "Make Name Colored and Check for Camouflage");
                }, 3f, "SetName To Chat");
            }

            if (AmongUsClient.Instance.AmHost)
                foreach (var protect in Main.GuardianAngelTarget)
                {
                    if (!AmongUsClient.Instance.AmHost) continue;
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
            List<CustomRoles> loversRoles = new();
            // GetString(lp.GetSubRoleName() + "Info")
            foreach (var lp in Main.LoversPlayers)
            {
                if (!AmongUsClient.Instance.AmHost) continue;
                if (!Options.LoversKnowRoleOfOtherLover.GetBool()) continue;
                loversRoles.Add(lp.GetCustomRole());
            }
            foreach (var lp in Main.LoversPlayers)
            {
                if (!AmongUsClient.Instance.AmHost) continue;
                if (!Options.LoversKnowRoleOfOtherLover.GetBool()) continue;
                foreach (var role in loversRoles)
                {
                    if (lp.GetCustomRole() == role) continue;
                    Utils.SendMessage($"Your lover has direct messaged you their role! Their role is {GetString(role.ToString())}.", lp.PlayerId);
                }
            }

            if (AmongUsClient.Instance.AmHost)
                foreach (var ar in PlayerControl.AllPlayerControls)
                {
                    if (ar == null || ar.Data.IsDead || ar.Data.Disconnected) continue;
                    if (ar.GetCustomRole() != CustomRoles.HexMaster) continue;
                    if (ar.IsHexedDone())
                        Utils.SendMessage("The Hex Master is done hexing people. If they survive the current meeting, they will kill everyone with a hex bomb!");
                    else
                    {
                        foreach (var hex in PlayerControl.AllPlayerControls)
                        {
                            if (Main.isHexed.TryGetValue((ar.PlayerId, hex.PlayerId), out var isHexed) && isHexed)
                                Utils.SendMessage("You have been hexed by the Hex Master!", hex.PlayerId);
                        }
                    }
                }

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
                            pva.NameText.text += $"({Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Doctor), target.GetDeathReason())})";
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
                    case CustomRoles.Psychic:
                        foreach (var id in goodids)
                        {
                            if (target.PlayerId == id)
                            {
                                pva.NameText.text = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Impostor), pva.NameText.text);
                                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, target.GetRealName(isMeeting: true));
                            }
                        }
                        foreach (var id in badids)
                        {
                            if (target.PlayerId == id)
                            {
                                pva.NameText.text = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Impostor), pva.NameText.text);
                                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, target.GetRealName(isMeeting: true));
                            }
                        }
                        break;
                    case CustomRoles.Swapper:
                    case CustomRoles.Executioner:
                        if (Main.ExecutionerTarget.TryGetValue(seer.PlayerId, out var targetId) && target.PlayerId == targetId) //targetがValue
                            pva.NameText.text = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Target), pva.NameText.text);
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
                                    pva.NameText.color = Utils.GetRoleColor(CustomRoles.Impostor);
                                }
                                else
                                {
                                    if (Investigator.SeeredCSheriff)
                                        pva.NameText.color = Utils.GetRoleColor(CustomRoles.Impostor);
                                    else
                                        pva.NameText.color = Utils.GetRoleColor(CustomRoles.TheGlitch);
                                }
                            }
                            else
                            {
                                if (Investigator.IsRed(target))
                                {
                                    if (target.GetCustomRole().IsCoven())
                                    {
                                        if (Investigator.CovenIsPurple.GetBool())
                                            pva.NameText.color = Utils.GetRoleColor(CustomRoles.Coven);
                                        else
                                            pva.NameText.color = Utils.GetRoleColor(CustomRoles.Impostor);
                                    }
                                    else
                                    {
                                        pva.NameText.color = Utils.GetRoleColor(CustomRoles.Impostor);
                                    }
                                }
                                else
                                {
                                    pva.NameText.color = Utils.GetRoleColor(CustomRoles.TheGlitch);
                                }
                            }
                        }
                        break;
                }

                switch (target.GetCustomSubRole())
                {
                    case CustomRoles.LoversRecode:
                        if (seer.Is(CustomRoles.LoversRecode) || seer.Data.IsDead)
                            pva.NameText.text += Helpers.ColorString(Utils.GetRoleColor(CustomRoles.LoversRecode), "♡");
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
                    case CustomRoles.Child:
                        if (Options.ChildKnown.GetBool() == true)
                            pva.NameText.text += Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Jackal), " (C)");
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
                    //Utils.SendMessage(string.Format(GetString("Message.Executed"), player.Data.PlayerName));
                    Logger.Info($"{player.GetNameWithRole()}を処刑しました", "Execution");
                });
            }
            if (Input.GetMouseButtonUp(1) && Input.GetKey(KeyCode.RightControl))
            {
                __instance.playerStates.DoIf(x => x.HighlightedFX.enabled, x =>
                {
                    var player = Utils.GetPlayerById(x.TargetPlayerId);
                    PlayerState.SetDeathReason(player.PlayerId, PlayerState.DeathReason.Execution);
                    PlayerState.SetDead(player.PlayerId);
                    player.RpcGuesserMurderPlayer();
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
