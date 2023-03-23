using System.Collections.Generic;
using System.Linq;
using Hazel;
using UnityEngine;
using System;
using InnerNet;
using static TownOfHost.Translator;
using AmongUs.GameOptions;

namespace TownOfHost
{
    public static class Guesser
    {
        static readonly int Id = 3428702;
        static List<byte> playerIdList = new();
        static Dictionary<byte, int> GuesserShootLimit;
        public static Dictionary<byte, bool> isEvilGuesserExiled;
        public static Dictionary<byte, int> GuessesThisRound;
        static Dictionary<int, CustomRoles> RoleAndNumber;
        static Dictionary<int, CustomRoles> RoleAndNumberPirate;
        static Dictionary<int, CustomRoles> RoleAndNumberAss;
        static Dictionary<int, CustomRoles> RoleAndNumberCoven;
        public static Dictionary<byte, bool> IsSkillUsed;
        static Dictionary<byte, bool> IsEvilGuesser;
        static Dictionary<byte, bool> IsNeutralGuesser;
        public static bool IsEvilGuesserMeeting;
        public static bool canGuess;
        public static Dictionary<byte, int> PirateGuess;

        // GUESSER OPTIONS //

        // ASSASSIN //
        public static CustomOption AssKillCount;
        public static CustomOption AssCanKillMultiplePerMeeting;
        public static CustomOption AssCanGuessCrewmate;
        public static CustomOption AssCanGuessNeutralBenign;
        public static CustomOption AssCanGuessNeutralEvil;
        public static CustomOption AssCanGuessNeutralKilling;
        public static CustomOption AssCanGuessCrewModifers;
        public static CustomOption AssCanGuessLovers;
        public static CustomOption AssCanGuessAfterVoting;
        public static CustomOption AssHideCommand;
        // VIGILANTE //
        public static CustomOption VigiKillCount;
        public static CustomOption VigiCanKillMultiplePerMeeting;
        public static CustomOption VigiCanGuessNeutralBenign;
        public static CustomOption VigiCanGuessNeutralEvil;
        public static CustomOption VigiCanGuessNeutralKilling;
        public static CustomOption VigiCanGuessLovers;
        public static CustomOption VigiCanGuessAfterVoting;
        public static CustomOption VigiHideCommand;
        // PIRATE //
        public static CustomOption PirateGuessAmount;
        public static CustomOption PirateCanGuessCrewmate;
        public static CustomOption PirateCanKillMultiplePerMeeting;
        public static CustomOption PirateCanGuessNeutralBenign;
        public static CustomOption PirateCanGuessNeutralEvil;
        public static CustomOption PirateCanGuessNeutralKilling;
        public static CustomOption PirateCanGuessCrewModifers;
        public static CustomOption PirateCanGuessImpostorRoles;
        public static CustomOption PirateCanGuessLovers;
        public static CustomOption PirateCanGuessAfterVoting;
        public static CustomOption PirateHideCommand;
        // GUESSER OPTIONS //

        public static bool alreadyTried = false;
        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id + 100, CustomRoles.EvilGuesser, AmongUsExtensions.OptionType.Impostor);
            AssKillCount = CustomOption.Create(Id + 110, Color.white, "GuesserShootLimit", AmongUsExtensions.OptionType.Impostor, 1, 1, 15, 1, Options.CustomRoleSpawnChances[CustomRoles.EvilGuesser]);
            AssCanKillMultiplePerMeeting = CustomOption.Create(Id + 111, Color.white, "CanKillMultiplePerMeeting", AmongUsExtensions.OptionType.Impostor, false, Options.CustomRoleSpawnChances[CustomRoles.EvilGuesser]);
            AssCanGuessCrewmate = CustomOption.Create(Id + 112, Color.white, "CanGuessCrewmate", AmongUsExtensions.OptionType.Impostor, false, Options.CustomRoleSpawnChances[CustomRoles.EvilGuesser]);
            AssCanGuessNeutralBenign = CustomOption.Create(Id + 113, Color.white, "CanGuessNeutralBenign", AmongUsExtensions.OptionType.Impostor, false, Options.CustomRoleSpawnChances[CustomRoles.EvilGuesser]);
            AssCanGuessNeutralEvil = CustomOption.Create(Id + 114, Color.white, "CanGuessNeutralEvil", AmongUsExtensions.OptionType.Impostor, false, Options.CustomRoleSpawnChances[CustomRoles.EvilGuesser]);
            AssCanGuessNeutralKilling = CustomOption.Create(Id + 115, Color.white, "CanGuessNeutralKilling", AmongUsExtensions.OptionType.Impostor, false, Options.CustomRoleSpawnChances[CustomRoles.EvilGuesser]);
            AssCanGuessCrewModifers = CustomOption.Create(Id + 116, Color.white, "CanGuessCrewModifers", AmongUsExtensions.OptionType.Impostor, false, Options.CustomRoleSpawnChances[CustomRoles.EvilGuesser]);
            AssCanGuessLovers = CustomOption.Create(Id + 117, Color.white, "CanGuessLovers", AmongUsExtensions.OptionType.Impostor, false, Options.CustomRoleSpawnChances[CustomRoles.EvilGuesser]);
            AssCanGuessAfterVoting = CustomOption.Create(Id + 118, Color.white, "CanGuessAfterVoting", AmongUsExtensions.OptionType.Impostor, false, Options.CustomRoleSpawnChances[CustomRoles.EvilGuesser]);
            AssHideCommand = CustomOption.Create(Id + 119, Color.white, "HideCommand", AmongUsExtensions.OptionType.Impostor, false, Options.CustomRoleSpawnChances[CustomRoles.EvilGuesser]);
            Options.SetupRoleOptions(Id + 200, CustomRoles.NiceGuesser, AmongUsExtensions.OptionType.Crewmate);
            VigiKillCount = CustomOption.Create(Id + 210, Color.white, "GuesserShootLimit", AmongUsExtensions.OptionType.Impostor, 1, 1, 15, 1, Options.CustomRoleSpawnChances[CustomRoles.NiceGuesser]);
            VigiCanKillMultiplePerMeeting = CustomOption.Create(Id + 211, Color.white, "CanKillMultiplePerMeeting", AmongUsExtensions.OptionType.Impostor, false, Options.CustomRoleSpawnChances[CustomRoles.NiceGuesser]);
            VigiCanGuessNeutralBenign = CustomOption.Create(Id + 213, Color.white, "CanGuessNeutralBenign", AmongUsExtensions.OptionType.Impostor, false, Options.CustomRoleSpawnChances[CustomRoles.NiceGuesser]);
            VigiCanGuessNeutralEvil = CustomOption.Create(Id + 214, Color.white, "CanGuessNeutralEvil", AmongUsExtensions.OptionType.Impostor, false, Options.CustomRoleSpawnChances[CustomRoles.NiceGuesser]);
            VigiCanGuessNeutralKilling = CustomOption.Create(Id + 215, Color.white, "CanGuessNeutralKilling", AmongUsExtensions.OptionType.Impostor, false, Options.CustomRoleSpawnChances[CustomRoles.NiceGuesser]);
            VigiCanGuessLovers = CustomOption.Create(Id + 217, Color.white, "CanGuessLovers", AmongUsExtensions.OptionType.Impostor, false, Options.CustomRoleSpawnChances[CustomRoles.NiceGuesser]);
            VigiCanGuessAfterVoting = CustomOption.Create(Id + 218, Color.white, "CanGuessAfterVoting", AmongUsExtensions.OptionType.Impostor, false, Options.CustomRoleSpawnChances[CustomRoles.NiceGuesser]);
            VigiHideCommand = CustomOption.Create(Id + 219, Color.white, "HideCommand", AmongUsExtensions.OptionType.Impostor, false, Options.CustomRoleSpawnChances[CustomRoles.NiceGuesser]);
            Options.SetupRoleOptions(Id + 300, CustomRoles.Pirate, AmongUsExtensions.OptionType.Neutral);
            PirateGuessAmount = CustomOption.Create(Id + 310, Color.white, "PirateGuessAmount", AmongUsExtensions.OptionType.Neutral, 3, 1, 15, 1, Options.CustomRoleSpawnChances[CustomRoles.Pirate]);
            PirateCanKillMultiplePerMeeting = CustomOption.Create(Id + 311, Color.white, "CanKillMultiplePerMeeting", AmongUsExtensions.OptionType.Impostor, false, Options.CustomRoleSpawnChances[CustomRoles.Pirate]);
            PirateCanGuessCrewmate = CustomOption.Create(Id + 312, Color.white, "CanGuessCrewmate", AmongUsExtensions.OptionType.Impostor, false, Options.CustomRoleSpawnChances[CustomRoles.Pirate]);
            PirateCanGuessImpostorRoles = CustomOption.Create(Id + 320, Color.white, "CanGuessImpostorRoles", AmongUsExtensions.OptionType.Impostor, false, Options.CustomRoleSpawnChances[CustomRoles.Pirate]);
            PirateCanGuessNeutralBenign = CustomOption.Create(Id + 313, Color.white, "CanGuessNeutralBenign", AmongUsExtensions.OptionType.Impostor, false, Options.CustomRoleSpawnChances[CustomRoles.Pirate]);
            PirateCanGuessNeutralEvil = CustomOption.Create(Id + 314, Color.white, "CanGuessNeutralEvil", AmongUsExtensions.OptionType.Impostor, false, Options.CustomRoleSpawnChances[CustomRoles.Pirate]);
            PirateCanGuessNeutralKilling = CustomOption.Create(Id + 315, Color.white, "CanGuessNeutralKilling", AmongUsExtensions.OptionType.Impostor, false, Options.CustomRoleSpawnChances[CustomRoles.Pirate]);
            PirateCanGuessCrewModifers = CustomOption.Create(Id + 316, Color.white, "CanGuessCrewModifers", AmongUsExtensions.OptionType.Impostor, false, Options.CustomRoleSpawnChances[CustomRoles.Pirate]);
            PirateCanGuessLovers = CustomOption.Create(Id + 317, Color.white, "CanGuessLovers", AmongUsExtensions.OptionType.Impostor, false, Options.CustomRoleSpawnChances[CustomRoles.Pirate]);
            PirateCanGuessAfterVoting = CustomOption.Create(Id + 318, Color.white, "CanGuessAfterVoting", AmongUsExtensions.OptionType.Impostor, false, Options.CustomRoleSpawnChances[CustomRoles.Pirate]);
            PirateHideCommand = CustomOption.Create(Id + 319, Color.white, "HideCommand", AmongUsExtensions.OptionType.Impostor, false, Options.CustomRoleSpawnChances[CustomRoles.Pirate]);
        }
        /*public static bool SetGuesserTeam(byte PlayerId = byte.MaxValue)//確定イビルゲッサーの人数とは別でイビルゲッサーかナイスゲッサーのどちらかに決める。
        {
            float EvilGuesserRate = EvilGuesserChance.GetFloat();
            IsEvilGuesser[PlayerId] = UnityEngine.Random.Range(1, 100) < EvilGuesserRate;
            return IsEvilGuesser[PlayerId];
        }
        public static bool SetOtherGuesserTeam(byte PlayerId = byte.MaxValue)//確定イビルゲッサーの人数とは別でイビルゲッサーかナイスゲッサーのどちらかに決める。
        {
            float NeutralGuesserRate = NeutralGuesserChance.GetFloat();
            IsNeutralGuesser[PlayerId] = UnityEngine.Random.Range(1, 100) < NeutralGuesserRate;
            return IsNeutralGuesser[PlayerId];
        }*/
        public static void Init()
        {
            playerIdList = new();
            GuesserShootLimit = new();
            isEvilGuesserExiled = new();
            RoleAndNumber = new();
            RoleAndNumberPirate = new();
            RoleAndNumberAss = new();
            RoleAndNumberCoven = new();
            IsSkillUsed = new();
            GuessesThisRound = new();
            IsEvilGuesserMeeting = false;
            alreadyTried = false;
            canGuess = true;
            PirateGuess = new();
            IsEvilGuesser = new();
            IsNeutralGuesser = new();
        }

        public static void AssassinAdd(byte PlayerId)
        {
            playerIdList.Add(PlayerId);
            GuesserShootLimit[PlayerId] = AssKillCount.GetInt();
            GuessesThisRound[PlayerId] = 0;
            isEvilGuesserExiled[PlayerId] = false;
            IsSkillUsed[PlayerId] = false;
            IsEvilGuesserMeeting = false;
        }
        public static void VigilanteAdd(byte PlayerId)
        {
            playerIdList.Add(PlayerId);
            GuesserShootLimit[PlayerId] = VigiKillCount.GetInt();
            GuessesThisRound[PlayerId] = 0;
            isEvilGuesserExiled[PlayerId] = false;
            IsSkillUsed[PlayerId] = false;
            IsEvilGuesserMeeting = false;
        }
        public static void PirateAdd(byte PlayerId)
        {
            playerIdList.Add(PlayerId);
            GuesserShootLimit[PlayerId] = 99;
            GuessesThisRound[PlayerId] = 0;
            PirateGuess[PlayerId] = 0;
            isEvilGuesserExiled[PlayerId] = false;
            IsSkillUsed[PlayerId] = false;
            IsEvilGuesserMeeting = false;
        }

        public static void OnMeeting()
        {
            if (!IsEnable()) return;
            foreach (byte playerId in playerIdList)
            {
                GuessesThisRound[playerId] = 0;
            }
        }
        public static bool IsEnable()
        {
            return playerIdList.Count > 0;
        }
        public static bool CanGuess(this PlayerControl pc)
        {
            if (GameStates.IsLobby || !AmongUsClient.Instance.IsGameStarted) return false;
            switch (pc.GetRoleType())
            {
                case RoleType.Coven:
                    if (pc.Is(CustomRoles.Mimic) | !Main.HasNecronomicon) break;
                    return true;
            }
            switch (pc.GetCustomRole())
            {
                case CustomRoles.EvilGuesser:
                case CustomRoles.NiceGuesser:
                case CustomRoles.Pirate:
                    return true;
                default:
                    return false;
            }
        }
        public static void GuesserShootByID(PlayerControl killer, string playerId, string targetrolenum)//ゲッサーが撃てるかどうかのチェック
        {
            if (killer.Data.IsDead) return;
            if (!killer.CanGuess()) return;
            if (killer.Is(CustomRoles.Pirate) && !canGuess) return;
            if (!CanGuessMoreThanOnce(killer) && !IsEvilGuesserMeeting) return;
            if (playerId == "show")
            {
                if (CanHideCommand(killer.GetCustomRole()))
                    Utils.BlockCommand(19);
                SendShootChoices(killer.PlayerId);
                SendShootID(killer.PlayerId);
                return;
            }
            if (!killer.Data.IsDead)
                foreach (var target in PlayerControl.AllPlayerControls)
                {
                    if (playerId == $"{target.PlayerId}" && GuesserShootLimit[killer.PlayerId] != 0)//targetnameが人の名前で弾数が０じゃないなら続行
                    {
                        var r = GetShootChoices(killer.GetCustomRole(), targetrolenum);
                        if (target.Data.IsDead) return;
                        if (target.GetCustomRole() == r | target.GetCustomSubRole() == r)//当たっていた場合
                        {
                            if (killer.Is(CustomRoles.Pirate))
                            {
                                PirateGuess[killer.PlayerId]++;
                                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetPirateProgress, Hazel.SendOption.Reliable, -1);
                                writer.Write(killer.PlayerId);
                                writer.Write(PirateGuess[killer.PlayerId]);
                                AmongUsClient.Instance.FinishRpcImmediately(writer);
                            }
                            if ((target.GetCustomRole() == CustomRoles.Crewmate && !CanShootRegularCrewmate(killer.GetCustomRole())) || (target.GetCustomRole() == CustomRoles.Egoist && killer.Is(CustomRoles.EvilGuesser))) return;
                            if (!CanGuessAfterVoting(killer)) return;
                            //クルー打ちが許可されていない場合とイビルゲッサーがエゴイストを打とうとしている場合はここで帰る
                            GuesserShootLimit[killer.PlayerId]--;
                            GuessesThisRound[killer.PlayerId]++;
                            Utils.SendMessage("--GUESS CHECKUP--" + (CanGuessMoreThanOnce(killer) ? "" : " (You have used up your guess for this meeting. Guessing from this point onwards in this meeting will not work.)" + $" (You have {GuesserShootLimit[killer.PlayerId]} Guesses Left.)"), killer.PlayerId);
                            PlayerState.SetDeathReason(target.PlayerId, PlayerState.DeathReason.Kill);
                            target.RpcGuesserMurderPlayer(0f);//専用の殺し方
                            if (PirateGuess[killer.PlayerId] == PirateGuessAmount.GetInt())
                            {
                                // pirate wins.
                                var endReason = TempData.LastDeathReason switch
                                {
                                    DeathReason.Exile => GameOverReason.ImpostorByVote,
                                    DeathReason.Kill => GameOverReason.ImpostorByKill,
                                    _ => GameOverReason.ImpostorByVote,
                                };
                                Main.WonPirateID = killer.PlayerId;
                                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EndGame, Hazel.SendOption.Reliable, -1);
                                writer.Write((byte)CustomWinner.Pirate);
                                writer.Write(killer.PlayerId);
                                AmongUsClient.Instance.FinishRpcImmediately(writer);
                                RPC.PirateWin(killer.PlayerId);
                                PirateEndGame(endReason, false);
                            }
                            return;
                        }
                        if (target.GetCustomRole() != r)//外していた場合
                        {
                            if (!killer.Is(CustomRoles.Pirate))
                            {
                                if (killer.GetCustomSubRole() is CustomRoles.DoubleShot && !alreadyTried)
                                {
                                    alreadyTried = true;
                                    Utils.SendMessage("You have misguessed someone's role. However, you have the modifier Double Shot. Because of this, you have another shot at guessing.\nDon't mess it up this time though.", killer.PlayerId);
                                }
                                else
                                {
                                    PlayerState.SetDeathReason(target.PlayerId, PlayerState.DeathReason.Misfire);
                                    killer.RpcGuesserMurderPlayer(0f);
                                }
                            }
                            else { canGuess = false; Utils.SendMessage("You missguessed as Pirate. Because of this, instead of dying, your guessing powers have been removed for the rest of the meeting,.", killer.PlayerId); }
                            if (IsEvilGuesserMeeting)
                            {
                                IsEvilGuesserMeeting = false;
                                isEvilGuesserExiled[killer.PlayerId] = false;
                                MeetingHud.Instance.RpcClose();
                            }
                            return;
                        }
                    }
                }
        }
        public static CustomRoles GetShootChoices(CustomRoles role, string targetrolenum)
        {
            if (role.IsCoven())
            {
                RoleAndNumberCoven.TryGetValue(int.Parse(targetrolenum), out var nvm);
                return nvm;
            }
            switch (role)
            {
                case CustomRoles.EvilGuesser:
                    RoleAndNumberAss.TryGetValue(int.Parse(targetrolenum), out var e);
                    return e;
                case CustomRoles.NiceGuesser:
                    RoleAndNumber.TryGetValue(int.Parse(targetrolenum), out var n);
                    return n;
                case CustomRoles.Pirate:
                    RoleAndNumberPirate.TryGetValue(int.Parse(targetrolenum), out var p);
                    return p;
                default:
                    RoleAndNumberAss.TryGetValue(int.Parse(targetrolenum), out var nvm);
                    return nvm;
            }
        }
        public static void SendShootChoices(byte PlayerId = byte.MaxValue)//番号と役職をチャットに表示
        {
            string text = "";
            var role = Utils.GetPlayerById(PlayerId).GetCustomRole();
            switch (role)
            {
                case CustomRoles.EvilGuesser:
                    for (var n = 1; n <= RoleAndNumberAss.Count(); n++)
                    {
                        text += string.Format("{0}:{1}\n", RoleAndNumberAss[n], n);
                    }
                    break;
                case CustomRoles.NiceGuesser:
                    for (var n = 1; n <= RoleAndNumber.Count(); n++)
                    {
                        text += string.Format("{0}:{1}\n", RoleAndNumber[n], n);
                    }
                    break;
                case CustomRoles.Pirate:
                    for (var n = 1; n <= RoleAndNumberPirate.Count(); n++)
                    {
                        text += string.Format("{0}:{1}\n", RoleAndNumberPirate[n], n);
                    }
                    break;
            }
            Utils.SendMessage(text, PlayerId);
        }
        public static void SendShootID(byte PlayerId = byte.MaxValue)//番号と役職をチャットに表示
        {
            string text = "";
            List<PlayerControl> AllPlayers = new();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                AllPlayers.Add(pc);
            }
            text += "All Players and their IDs:";
            foreach (var player in AllPlayers)
            {
                text += $"\n{player.GetRealName(true)} : {player.PlayerId}";
            }
            Utils.SendMessage(text, PlayerId);
        }
        public static void RpcClientGuess(this PlayerControl pc)
        {
            var amOwner = pc.AmOwner;
            var meetingHud = MeetingHud.Instance;
            var hudManager = DestroyableSingleton<HudManager>.Instance;
            SoundManager.Instance.PlaySound(pc.KillSfx, false, 0.8f);
            hudManager.KillOverlay.ShowKillAnimation(pc.Data, pc.Data);
            if (amOwner)
            {
                hudManager.ShadowQuad.gameObject.SetActive(false);
                pc.nameText().GetComponent<MeshRenderer>().material.SetInt("_Mask", 0);
                pc.RpcSetScanner(false);
                ImportantTextTask importantTextTask = new GameObject("_Player").AddComponent<ImportantTextTask>();
                importantTextTask.transform.SetParent(AmongUsClient.Instance.transform, false);
                meetingHud.SetForegroundForDead();
            }
            PlayerVoteArea voteArea = MeetingHud.Instance.playerStates.First(
                x => x.TargetPlayerId == pc.PlayerId
            );
            //pc.Die(DeathReason.Kill);
            if (voteArea == null) return;
            if (voteArea.DidVote) voteArea.UnsetVote();
            voteArea.AmDead = true;
            voteArea.Overlay.gameObject.SetActive(true);
            voteArea.Overlay.color = Color.white;
            voteArea.XMark.gameObject.SetActive(true);
            voteArea.XMark.transform.localScale = Vector3.one;
            foreach (var playerVoteArea in meetingHud.playerStates)
            {
                if (playerVoteArea.VotedFor != pc.PlayerId) continue;
                playerVoteArea.UnsetVote();
                var voteAreaPlayer = Utils.GetPlayerById(playerVoteArea.TargetPlayerId);
                if (!voteAreaPlayer.AmOwner) continue;
                meetingHud.ClearVote();
            }
        }
        public static void RpcGuesserMurderPlayer(this PlayerControl pc, float delay = 0f)//ゲッサー用の殺し方
        {
            string text = "";
            text += string.Format(GetString("KilledByGuesser"), pc.name);
            Main.unreportableBodies.Add(pc.PlayerId);
            if (CanHideCommand(pc.GetCustomRole()))
                Utils.BlockCommand(19);
            Utils.SendMessage(text, byte.MaxValue);
            if (pc.GetCustomRole() is CustomRoles.LoversRecode)
            {
                PlayerControl? lover = Main.LoversPlayers.ToArray().Where(pc => pc.PlayerId == pc.PlayerId).FirstOrDefault();
                Main.LoversPlayers.Remove(lover);
                Main.isLoversDead = true;
                if (Options.LoversDieTogether.GetBool())
                {
                    foreach (var lp in Main.LoversPlayers)
                    {
                        if (!lp.Is(CustomRoles.Pestilence))
                        {
                            lp.RpcGuesserMurderPlayer();
                            PlayerState.SetDeathReason(lp.PlayerId, PlayerState.DeathReason.LoversSuicide);
                        }
                        Main.LoversPlayers.Remove(lp);
                    }
                }
            }
            // DEATH STUFF //
            var amOwner = pc.AmOwner;
            pc.Data.IsDead = true;
            pc.RpcExileV2();
            //PlayerState.SetDeathReason(pc.PlayerId, PlayerState.DeathReason.Execution);
            PlayerState.SetDead(pc.PlayerId);
            var meetingHud = MeetingHud.Instance;
            var hudManager = DestroyableSingleton<HudManager>.Instance;
            SoundManager.Instance.PlaySound(pc.KillSfx, false, 0.8f);
            hudManager.KillOverlay.ShowKillAnimation(pc.Data, pc.Data);
            if (amOwner)
            {
                hudManager.ShadowQuad.gameObject.SetActive(false);
                pc.nameText().GetComponent<MeshRenderer>().material.SetInt("_Mask", 0);
                pc.RpcSetScanner(false);
                ImportantTextTask importantTextTask = new GameObject("_Player").AddComponent<ImportantTextTask>();
                importantTextTask.transform.SetParent(AmongUsClient.Instance.transform, false);
                meetingHud.SetForegroundForDead();
            }
            PlayerVoteArea voteArea = MeetingHud.Instance.playerStates.First(
                x => x.TargetPlayerId == pc.PlayerId
            );
            if (voteArea == null) return;
            if (voteArea.DidVote) voteArea.UnsetVote();
            voteArea.AmDead = true;
            voteArea.Overlay.gameObject.SetActive(true);
            voteArea.Overlay.color = Color.white;
            voteArea.XMark.gameObject.SetActive(true);
            voteArea.XMark.transform.localScale = Vector3.one;
            foreach (var playerVoteArea in meetingHud.playerStates)
            {
                if (playerVoteArea.VotedFor != pc.PlayerId) continue;
                playerVoteArea.UnsetVote();
                var voteAreaPlayer = Utils.GetPlayerById(playerVoteArea.TargetPlayerId);
                if (!voteAreaPlayer.AmOwner) continue;
                meetingHud.ClearVote();
            }
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.AssassinKill, Hazel.SendOption.Reliable, -1);
            writer.Write(pc.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static bool RoleAgrees(CustomRoles roleToAgree, CustomRoles team)
        {
            switch (team)
            {
                case CustomRoles.EvilGuesser:
                    if (roleToAgree.IsImpostorTeam()) return false;
                    NeutralRoleType roleTypeAss = roleToAgree.GetNeutralRoleType();
                    switch (roleTypeAss)
                    {
                        case NeutralRoleType.Benign:
                            return AssCanGuessNeutralBenign.GetBool();
                        case NeutralRoleType.Evil:
                            return AssCanGuessNeutralEvil.GetBool();
                        case NeutralRoleType.Killing:
                            return AssCanGuessNeutralKilling.GetBool();
                        case NeutralRoleType.None:
                            if (roleToAgree == CustomRoles.Crewmate) return AssCanGuessCrewmate.GetBool();
                            if (roleToAgree.IsCrewmate()) return true;
                            if (roleToAgree.IsCrewModifier()) return AssCanGuessCrewModifers.GetBool();
                            if (roleToAgree == CustomRoles.Lovers) return AssCanGuessLovers.GetBool();
                            break;
                    }
                    break;
                case CustomRoles.NiceGuesser:
                    if (roleToAgree.IsCrewmate()) return false;
                    NeutralRoleType roleTypeVigi = roleToAgree.GetNeutralRoleType();
                    switch (roleTypeVigi)
                    {
                        case NeutralRoleType.Benign:
                            return VigiCanGuessNeutralBenign.GetBool();
                        case NeutralRoleType.Evil:
                            return VigiCanGuessNeutralEvil.GetBool();
                        case NeutralRoleType.Killing:
                            return VigiCanGuessNeutralKilling.GetBool();
                        case NeutralRoleType.None:
                            if (roleToAgree.IsImpostorTeam()) return true;
                            if (roleToAgree == CustomRoles.Lovers) return VigiCanGuessLovers.GetBool();
                            break;
                    }
                    break;
                case CustomRoles.Pirate:
                    NeutralRoleType roleType = roleToAgree.GetNeutralRoleType();
                    switch (roleType)
                    {
                        case NeutralRoleType.Benign:
                            return PirateCanGuessNeutralBenign.GetBool();
                        case NeutralRoleType.Evil:
                            return PirateCanGuessNeutralEvil.GetBool();
                        case NeutralRoleType.Killing:
                            return PirateCanGuessNeutralKilling.GetBool();
                        case NeutralRoleType.None:
                            if (roleToAgree == CustomRoles.Crewmate) return PirateCanGuessCrewmate.GetBool();
                            if (roleToAgree.IsCrewmate()) return true;
                            if (roleToAgree.IsImpostorTeam()) return PirateCanGuessImpostorRoles.GetBool();
                            if (roleToAgree.IsCrewModifier()) return PirateCanGuessCrewModifers.GetBool();
                            if (roleToAgree == CustomRoles.Lovers) return PirateCanGuessLovers.GetBool();
                            break;
                    }
                    break;
            }

            return false;
        }

        public static bool CanHideCommand(CustomRoles team)
        {
            switch (team)
            {
                case CustomRoles.EvilGuesser:
                    return AssHideCommand.GetBool();
                case CustomRoles.NiceGuesser:
                    return VigiHideCommand.GetBool();
                case CustomRoles.Pirate:
                    return PirateHideCommand.GetBool();
                default:
                    return false;
            }
        }
        public static bool CanShootRegularCrewmate(CustomRoles team)
        {
            switch (team)
            {
                case CustomRoles.EvilGuesser:
                    return AssCanGuessCrewmate.GetBool();
                case CustomRoles.NiceGuesser:
                    return false;
                case CustomRoles.Pirate:
                    return PirateCanGuessCrewmate.GetBool();
                default:
                    return false;
            }
        }

        public static bool CanGuessMoreThanOnce(PlayerControl pc)
        {
            CustomRoles team = pc.GetCustomRole();
            switch (team)
            {
                case CustomRoles.EvilGuesser:
                    return AssCanKillMultiplePerMeeting.GetBool() || GuessesThisRound[pc.PlayerId] < 1;
                case CustomRoles.NiceGuesser:
                    return VigiCanKillMultiplePerMeeting.GetBool() || GuessesThisRound[pc.PlayerId] < 1;
                case CustomRoles.Pirate:
                    return PirateCanKillMultiplePerMeeting.GetBool() || GuessesThisRound[pc.PlayerId] < 1;
                default:
                    return false;
            }
        }
        public static bool CanGuessAfterVoting(PlayerControl pc)
        {
            CustomRoles team = pc.GetCustomRole();
            switch (team)
            {
                case CustomRoles.EvilGuesser:
                    PlayerVoteArea voteArea = MeetingHud.Instance.playerStates.First(
                        x => x.TargetPlayerId == pc.PlayerId
                    );
                    if (voteArea == null) return true;
                    if (voteArea.VotedFor <= 15)
                    {
                        return AssCanGuessAfterVoting.GetBool();
                    }
                    else
                    {
                        return true;
                    }
                case CustomRoles.NiceGuesser:
                    PlayerVoteArea voteArea1 = MeetingHud.Instance.playerStates.First(
                        x => x.TargetPlayerId == pc.PlayerId
                    );
                    if (voteArea1 == null) return true;
                    if (voteArea1.VotedFor <= 15)
                    {
                        return VigiCanGuessAfterVoting.GetBool();
                    }
                    else
                    {
                        return true;
                    }
                case CustomRoles.Pirate:
                    PlayerVoteArea voteArea2 = MeetingHud.Instance.playerStates.First(
                        x => x.TargetPlayerId == pc.PlayerId
                    );
                    if (voteArea2 == null) return true;
                    if (voteArea2.VotedFor <= 15)
                    {
                        return PirateCanGuessAfterVoting.GetBool();
                    }
                    else
                    {
                        return true;
                    }
                default:
                    return false;
            }
        }
        public static void SetRoleAndNumber()//役職を番号で管理
        {
            RoleAndNumber = new Dictionary<int, CustomRoles>();
            RoleAndNumberPirate = new Dictionary<int, CustomRoles>();
            RoleAndNumberAss = new Dictionary<int, CustomRoles>();
            RoleAndNumberCoven = new Dictionary<int, CustomRoles>();
            List<CustomRoles> vigiList = new();
            List<CustomRoles> pirateList = new();
            List<CustomRoles> assassinList = new();
            List<CustomRoles> covenList = new();
            List<CustomRoles> revealed = new();
            var i = 1;
            var ie = 1;
            var iee = 1;
            var c = 1;
            foreach (var id in Main.rolesRevealedNextMeeting)
            {
                revealed.Add(Utils.GetPlayerById(id).GetCustomRole());
            }
            foreach (CustomRoles role in Enum.GetValues(typeof(CustomRoles)))
            {
                if (!role.IsEnable() | revealed.Contains(role)) continue;
                if (role == CustomRoles.Phantom) continue;
                if (role == CustomRoles.Child && Options.ChildKnown.GetBool()) continue;
                if (role.IsModifier())
                {
                    if (!role.IsCrewModifier() && role != CustomRoles.LoversRecode) continue;
                }
                if (!role.IsImpostorTeam() && role != CustomRoles.Egoist && RoleAgrees(role, CustomRoles.EvilGuesser)) assassinList.Add(role);
                if (role != CustomRoles.Pirate && RoleAgrees(role, CustomRoles.Pirate)) pirateList.Add(role);
                if (!role.IsCrewmate() && RoleAgrees(role, CustomRoles.NiceGuesser)) vigiList.Add(role);
                if (!role.IsCoven()) covenList.Add(role);
            }
            vigiList = vigiList.OrderBy(a => Guid.NewGuid()).ToList();
            assassinList = assassinList.OrderBy(a => Guid.NewGuid()).ToList();
            pirateList = pirateList.OrderBy(a => Guid.NewGuid()).ToList();
            covenList = covenList.OrderBy(a => Guid.NewGuid()).ToList();
            foreach (var ro in vigiList)
            {
                RoleAndNumber.Add(i, ro);
                i++;
            }//番号とセットにする
            foreach (var ro in pirateList)
            {
                RoleAndNumberPirate.Add(ie, ro);
                ie++;
            }//番号とセットにする
            foreach (var ro in assassinList)
            {
                RoleAndNumberAss.Add(iee, ro);
                iee++;
            }//番号とセットにする
            foreach (var ro in covenList)
            {
                RoleAndNumberCoven.Add(c, ro);
                c++;
            }//番号とセットにする
        }
        public static void OpenGuesserMeeting()
        {
            foreach (var gu in playerIdList)
            {
                if (isEvilGuesserExiled[gu])//ゲッサーの中から吊られた奴がいないかどうかの確認
                {
                    string text = "";
                    Utils.GetPlayerById(gu).CmdReportDeadBody(null);//会議を起こす
                    IsEvilGuesserMeeting = true;
                    text += GetString("EvilGuesserMeeting");
                    Utils.SendMessage(text, byte.MaxValue);
                }
            }
        }
        private static void PirateEndGame(GameOverReason reason, bool showAd)
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                var LoseImpostorRole = Main.AliveImpostorCount == 0 ? pc.Is(RoleType.Impostor) : pc.Is(CustomRoles.Egoist);
                if (pc.Is(CustomRoles.Sheriff) || pc.Is(CustomRoles.Investigator) ||
                    (!(Main.currentWinner == CustomWinner.Arsonist) && pc.Is(CustomRoles.Arsonist)) || (Main.currentWinner != CustomWinner.Vulture && pc.Is(CustomRoles.Vulture)) || (Main.currentWinner != CustomWinner.Marksman && pc.Is(CustomRoles.Marksman)) || (Main.currentWinner != CustomWinner.Pirate && pc.Is(CustomRoles.Pirate)) ||
                    (Main.currentWinner != CustomWinner.Jackal && pc.Is(CustomRoles.Jackal)) || (Main.currentWinner != CustomWinner.BloodKnight && pc.Is(CustomRoles.BloodKnight)) || (Main.currentWinner != CustomWinner.Pestilence && pc.Is(CustomRoles.Pestilence)) || (Main.currentWinner != CustomWinner.Coven && pc.GetRoleType() == RoleType.Coven) ||
                    LoseImpostorRole || (Main.currentWinner != CustomWinner.Werewolf && pc.Is(CustomRoles.Werewolf)) || (Main.currentWinner != CustomWinner.AgiTater && pc.Is(CustomRoles.AgiTater)) || (Main.currentWinner != CustomWinner.TheGlitch && pc.Is(CustomRoles.TheGlitch)))
                {
                    pc.RpcSetRole(RoleTypes.CrewmateGhost);
                }
                if (pc.Is(CustomRoles.Pirate))
                {
                    pc.RpcSetRole(RoleTypes.ImpostorGhost);
                }
            }
            new LateTask(() =>
            {
                GameManager.Instance.RpcEndGame(reason, showAd);
            }, 0.5f, "EndGameTask");
        }
    }
}
