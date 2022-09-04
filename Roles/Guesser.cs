using System.Collections.Generic;
using System.Linq;
using Hazel;
using UnityEngine;
using System;
using InnerNet;
using static TownOfHost.Translator;

namespace TownOfHost
{
    public static class Guesser
    {
        static readonly int Id = 301000;
        static CustomOption EvilGuesserChance;
        static CustomOption NeutralGuesserChance;
        static CustomOption ConfirmedEvilGuesser;
        static CustomOption CanShootAsNormalCrewmate;
        static CustomOption GuesserCanKillCount;
        static CustomOption CanKillMultipleTimes;
        public static CustomOption PirateGuessAmount;
        static CustomOption PirateMisguessAmount;
        static List<byte> playerIdList = new();
        static Dictionary<byte, int> GuesserShootLimit;
        public static Dictionary<byte, bool> isEvilGuesserExiled;
        static Dictionary<int, CustomRoles> RoleAndNumber;
        static Dictionary<int, CustomRoles> RoleAndNumberPirate;
        static Dictionary<int, CustomRoles> RoleAndNumberAss;
        public static Dictionary<byte, bool> IsSkillUsed;
        static Dictionary<byte, bool> IsEvilGuesser;
        static Dictionary<byte, bool> IsNeutralGuesser;
        public static bool IsEvilGuesserMeeting;
        public static bool canGuess;
        public static Dictionary<byte, int> PirateGuess;
        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id + 21, CustomRoles.EvilGuesser);
            CanShootAsNormalCrewmate = CustomOption.Create(Id + 30130, Color.white, "CanShootAsNormalCrewmate", true, Options.CustomRoleSpawnChances[CustomRoles.EvilGuesser]);
            GuesserCanKillCount = CustomOption.Create(Id + 30140, Color.white, "GuesserShootLimit", 1, 1, 15, 1, Options.CustomRoleSpawnChances[CustomRoles.EvilGuesser]);
            CanKillMultipleTimes = CustomOption.Create(Id + 30150, Color.white, "CanKillMultipleTimes", false, Options.CustomRoleSpawnChances[CustomRoles.EvilGuesser]);
            Options.SetupRoleOptions(Id + 20, CustomRoles.NiceGuesser);
            Options.SetupRoleOptions(Id + 51, CustomRoles.Pirate);
            PirateGuessAmount = CustomOption.Create(Id + 30170, Color.white, "PirateGuessAmount", 3, 1, 10, 1, Options.CustomRoleSpawnChances[CustomRoles.Pirate]);
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
            IsSkillUsed = new();
            IsEvilGuesserMeeting = false;
            canGuess = true;
            PirateGuess = new();
            IsEvilGuesser = new();
            IsNeutralGuesser = new();
        }
        public static void Add(byte PlayerId)
        {
            playerIdList.Add(PlayerId);
            if (Utils.GetPlayerById(PlayerId).Is(CustomRoles.Pirate))
                GuesserShootLimit[PlayerId] = 99;
            else
                GuesserShootLimit[PlayerId] = GuesserCanKillCount.GetInt();
            isEvilGuesserExiled[PlayerId] = false;
            IsSkillUsed[PlayerId] = false;
            IsEvilGuesserMeeting = false;
        }
        public static bool IsEnable()
        {
            return playerIdList.Count > 0;
        }
        public static void SetRoleToGuesser(PlayerControl player)//ゲッサーをイビルとナイスに振り分ける
        {
            if (!player.Is(CustomRoles.Guesser)) return;
            if (IsEvilGuesser[player.PlayerId]) Main.AllPlayerCustomRoles[player.PlayerId] = CustomRoles.EvilGuesser;
            else if (IsNeutralGuesser[player.PlayerId]) Main.AllPlayerCustomRoles[player.PlayerId] = CustomRoles.Pirate;
            else Main.AllPlayerCustomRoles[player.PlayerId] = CustomRoles.NiceGuesser;
        }
        public static void GuesserShoot(PlayerControl killer, string targetname, string targetrolenum)//ゲッサーが撃てるかどうかのチェック
        {
            if ((!killer.Is(CustomRoles.NiceGuesser) && !killer.Is(CustomRoles.EvilGuesser) && !killer.Is(CustomRoles.Pirate)) || killer.Data.IsDead || !AmongUsClient.Instance.IsGameStarted) return;
            if (killer.Is(CustomRoles.Pirate) && !canGuess) return;
            //死んでるやつとゲッサーじゃないやつ、ゲームが始まってない場合は引き返す
            if (killer.Is(CustomRoles.NiceGuesser) && IsEvilGuesserMeeting) return;//イビルゲッサー会議の最中はナイスゲッサーは打つな
            if (!CanKillMultipleTimes.GetBool() && IsSkillUsed[killer.PlayerId] && !IsEvilGuesserMeeting) if (!killer.Is(CustomRoles.Pirate)) return;
            if (targetname == "show")
            {
                SendShootChoices(killer.PlayerId);
                return;
            }
            foreach (var target in PlayerControl.AllPlayerControls)
            {
                if (targetname == $"{target.name}" && GuesserShootLimit[killer.PlayerId] != 0)//targetnameが人の名前で弾数が０じゃないなら続行
                {
                    //if (target.Data.IsDead) return;
                    var r = GetGuessingType(killer.GetCustomRole(), targetrolenum);
                    if (target.Data.IsDead) return;
                    if (target.GetCustomRole() == r)//当たっていた場合
                    {
                        if (killer.Is(CustomRoles.Pirate))
                            PirateGuess[killer.PlayerId]++;
                        if (!killer.Is(CustomRoles.Pirate))
                            if ((target.GetCustomRole() == CustomRoles.Crewmate && !CanShootAsNormalCrewmate.GetBool()) || (target.GetCustomRole() == CustomRoles.Egoist && killer.Is(CustomRoles.EvilGuesser))) return;
                        //クルー打ちが許可されていない場合とイビルゲッサーがエゴイストを打とうとしている場合はここで帰る
                        GuesserShootLimit[killer.PlayerId]--;
                        IsSkillUsed[killer.PlayerId] = true;
                        PlayerState.SetDeathReason(target.PlayerId, PlayerState.DeathReason.Kill);
                        target.RpcGuesserMurderPlayer(0f);//専用の殺し方
                        PlayerState.SetDeathReason(target.PlayerId, PlayerState.DeathReason.Kill);
                        if (PirateGuess[killer.PlayerId] == PirateGuessAmount.GetInt())
                        {
                            // pirate wins.
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EndGame, Hazel.SendOption.Reliable, -1);
                            writer.Write((byte)CustomWinner.Jester);
                            writer.Write(killer.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPC.PirateWin(killer.PlayerId);
                            //CheckAndEndGamePatch.ResetRoleAndEndGame(endReason, false);
                        }
                        return;
                    }
                    if (target.GetCustomRole() != r)//外していた場合
                    {
                        if (!killer.Is(CustomRoles.Pirate))
                        {
                            PlayerState.SetDeathReason(target.PlayerId, PlayerState.DeathReason.Misfire);
                            killer.RpcGuesserMurderPlayer(0f);
                            PlayerState.SetDeathReason(target.PlayerId, PlayerState.DeathReason.Misfire);
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
        public static CustomRoles GetGuessingType(CustomRoles role, string targetrolenum)
        {
            switch (role)
            {
                case CustomRoles.EvilGuesser:
                    RoleAndNumberAss.TryGetValue(int.Parse(targetrolenum), out var r);
                    return r;
                    break;
                case CustomRoles.NiceGuesser:
                    RoleAndNumber.TryGetValue(int.Parse(targetrolenum), out var re);
                    return re;
                    break;
                case CustomRoles.Pirate:
                    RoleAndNumberPirate.TryGetValue(int.Parse(targetrolenum), out var ree);
                    return ree;
                    break;
            }
            return CustomRoles.Amnesiac;
        }
        public static void GuesserShootByID(PlayerControl killer, string playerId, string targetrolenum)//ゲッサーが撃てるかどうかのチェック
        {
            if ((!killer.Is(CustomRoles.NiceGuesser) && !killer.Is(CustomRoles.EvilGuesser) && !killer.Is(CustomRoles.Pirate)) || killer.Data.IsDead || !AmongUsClient.Instance.IsGameStarted) if (killer.GetCustomRole().IsCoven() && !Main.HasNecronomicon) return;
            if (killer.Is(CustomRoles.Pirate) && !canGuess) return;
            //死んでるやつとゲッサーじゃないやつ、ゲームが始まってない場合は引き返す
            if (killer.Is(CustomRoles.NiceGuesser) && IsEvilGuesserMeeting) return;//イビルゲッサー会議の最中はナイスゲッサーは打つな
            if (!CanKillMultipleTimes.GetBool() && IsSkillUsed[killer.PlayerId] && !IsEvilGuesserMeeting) if (!killer.Is(CustomRoles.Pirate)) return;
            if (playerId == "show")
            {
                SendShootChoices(killer.PlayerId);
                SendShootID(killer.PlayerId);
                return;
            }
            foreach (var target in PlayerControl.AllPlayerControls)
            {
                if (playerId == $"{target.PlayerId}" && GuesserShootLimit[killer.PlayerId] != 0)//targetnameが人の名前で弾数が０じゃないなら続行
                {
                    var r = GetShootChoices(killer.GetCustomRole(), targetrolenum);
                    if (target.Data.IsDead) return;
                    if (target.GetCustomRole() == r)//当たっていた場合
                    {
                        if (killer.Is(CustomRoles.Pirate))
                            PirateGuess[killer.PlayerId]++;
                        if (!killer.Is(CustomRoles.Pirate))
                            if ((target.GetCustomRole() == CustomRoles.Crewmate && !CanShootAsNormalCrewmate.GetBool()) || (target.GetCustomRole() == CustomRoles.Egoist && killer.Is(CustomRoles.EvilGuesser))) return;
                        //クルー打ちが許可されていない場合とイビルゲッサーがエゴイストを打とうとしている場合はここで帰る
                        GuesserShootLimit[killer.PlayerId]--;
                        IsSkillUsed[killer.PlayerId] = true;
                        PlayerState.SetDeathReason(target.PlayerId, PlayerState.DeathReason.Kill);
                        target.RpcGuesserMurderPlayer(0f);//専用の殺し方
                        if (PirateGuess[killer.PlayerId] == PirateGuessAmount.GetInt())
                        {
                            // pirate wins.
                        }
                        return;
                    }
                    if (target.GetCustomRole() != r)//外していた場合
                    {
                        if (!killer.Is(CustomRoles.Pirate))
                        {
                            PlayerState.SetDeathReason(target.PlayerId, PlayerState.DeathReason.Misfire);
                            killer.RpcGuesserMurderPlayer(0f);
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
            if (RoleAndNumber.Count() == 0) return;
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
        public static void RpcGuesserMurderPlayer(this PlayerControl pc, float delay = 0f)//ゲッサー用の殺し方
        {
            string text = "";
            new LateTask(() =>
            {
                MessageWriter MurderWriter = AmongUsClient.Instance.StartRpcImmediately(pc.NetId, (byte)RpcCalls.MurderPlayer, SendOption.Reliable, pc.GetClientId());
                MessageExtensions.WriteNetObject(MurderWriter, pc);
                AmongUsClient.Instance.FinishRpcImmediately(MurderWriter);
            }, 0.2f + delay, "Guesser Murder");//ここまでの処理でターゲットで視点キルを発生させる
            pc.Data.IsDead = true;//それ以外のやつ視点で勝手に死んだことにする
            text += string.Format(GetString("KilledByGuesser"), pc.name);//ホスト以外死んだのがわからないのでチャットで送信
            Utils.SendMessage(text, byte.MaxValue);

        }
        public static void SetRoleAndNumber()//役職を番号で管理
        {
            List<CustomRoles> vigiList = new();
            List<CustomRoles> pirateList = new();
            List<CustomRoles> assassinList = new();
            var i = 1;
            var ie = 1;
            var iee = 1;
            foreach (var pc in PlayerControl.AllPlayerControls)//とりあえずアサインされた役職をすべて取りだす
            {
                var role = pc.GetCustomRole();
                if (!assassinList.Contains(pc.GetCustomRole()) && !role.IsImpostorTeam() && role != CustomRoles.Egoist) assassinList.Add(pc.GetCustomRole());
                if (!pirateList.Contains(pc.GetCustomRole()) && role != CustomRoles.Pirate) pirateList.Add(pc.GetCustomRole());
                if (!vigiList.Contains(pc.GetCustomRole()) && !role.IsCrewmate()) vigiList.Add(pc.GetCustomRole());
            }
            if (Options.CanMakeMadmateCount.GetInt() != 0) vigiList.Add(CustomRoles.SKMadmate);//SKMadmateがいる際にはサイドキック前から候補に入れておく。
            if (Options.CanMakeMadmateCount.GetInt() != 0) pirateList.Add(CustomRoles.SKMadmate);//SKMadmateがいる際にはサイドキック前から候補に入れておく。
            if (CustomRoles.SchrodingerCat.IsEnable())//シュレネコがいる場合も役職変化前から候補に入れておく。
            {
                vigiList.Add(CustomRoles.MSchrodingerCat);
                pirateList.Add(CustomRoles.MSchrodingerCat);
                assassinList.Add(CustomRoles.MSchrodingerCat);
                if (Sheriff.IsEnable) assassinList.Add(CustomRoles.CSchrodingerCat);
                if (Sheriff.IsEnable) pirateList.Add(CustomRoles.CSchrodingerCat);
                if (CustomRoles.Egoist.IsEnable()) vigiList.Add(CustomRoles.EgoSchrodingerCat);
                if (CustomRoles.Jackal.IsEnable()) vigiList.Add(CustomRoles.JSchrodingerCat);
                if (CustomRoles.Egoist.IsEnable()) pirateList.Add(CustomRoles.EgoSchrodingerCat);
                if (CustomRoles.Jackal.IsEnable()) pirateList.Add(CustomRoles.JSchrodingerCat);
                //if (CustomRoles.Egoist.IsEnable()) roles.Add(CustomRoles.EgoSchrodingerCat);
                if (CustomRoles.Jackal.IsEnable()) assassinList.Add(CustomRoles.JSchrodingerCat);
            }
            vigiList = vigiList.OrderBy(a => Guid.NewGuid()).ToList();//会議画面で見たときに役職と順番が一緒で、役バレしたのでシャッフル
            assassinList = assassinList.OrderBy(a => Guid.NewGuid()).ToList();//会議画面で見たときに役職と順番が一緒で、役バレしたのでシャッフル
            pirateList = pirateList.OrderBy(a => Guid.NewGuid()).ToList();//会議画面で見たときに役職と順番が一緒で、役バレしたのでシャッフル
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
    }
}