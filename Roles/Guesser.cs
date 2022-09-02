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
        public static Dictionary<byte, bool> IsSkillUsed;
        static bool IsEvilGuesser;
        static bool IsNeutralGuesser;
        public static bool IsEvilGuesserMeeting;
        public static bool canGuess;
        public static int PirateGuess;
        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id, CustomRoles.Guesser);
            EvilGuesserChance = CustomOption.Create(Id + 30110, Color.white, "EvilGuesserChance", 0, 0, 100, 10, Options.CustomRoleSpawnChances[CustomRoles.Guesser]);
            NeutralGuesserChance = CustomOption.Create(Id + 30160, Color.white, "NeutralGuesserChance", 0, 0, 100, 10, Options.CustomRoleSpawnChances[CustomRoles.Guesser]);
            ConfirmedEvilGuesser = CustomOption.Create(Id + 30120, Color.white, "ConfirmedEvilGuesser", 0, 0, 3, 1, Options.CustomRoleSpawnChances[CustomRoles.Guesser]);
            Options.CustomRoleCounts.Add(CustomRoles.EvilGuesser, ConfirmedEvilGuesser);
            Options.CustomRoleSpawnChances.Add(CustomRoles.EvilGuesser, ConfirmedEvilGuesser);
            PirateGuessAmount = CustomOption.Create(Id + 30170, Color.white, "PirateGuessAmount", 3, 1, 10, 1, Options.CustomRoleSpawnChances[CustomRoles.Guesser]);
            CanShootAsNormalCrewmate = CustomOption.Create(Id + 30130, Color.white, "CanShootAsNormalCrewmate", true, Options.CustomRoleSpawnChances[CustomRoles.Guesser]);
            GuesserCanKillCount = CustomOption.Create(Id + 30140, Color.white, "GuesserShootLimit", 1, 1, 15, 1, Options.CustomRoleSpawnChances[CustomRoles.Guesser]);
            CanKillMultipleTimes = CustomOption.Create(Id + 30150, Color.white, "CanKillMultipleTimes", false, Options.CustomRoleSpawnChances[CustomRoles.Guesser]);
        }
        public static bool SetGuesserTeam()//確定イビルゲッサーの人数とは別でイビルゲッサーかナイスゲッサーのどちらかに決める。
        {
            float EvilGuesserRate = EvilGuesserChance.GetFloat();
            IsEvilGuesser = UnityEngine.Random.Range(1, 100) < EvilGuesserRate;
            return IsEvilGuesser;
        }
        public static bool SetOtherGuesserTeam()//確定イビルゲッサーの人数とは別でイビルゲッサーかナイスゲッサーのどちらかに決める。
        {
            float NeutralGuesserRate = NeutralGuesserChance.GetFloat();
            IsNeutralGuesser = UnityEngine.Random.Range(1, 100) < NeutralGuesserRate;
            return IsNeutralGuesser;
        }
        public static void Init()
        {
            playerIdList = new();
            GuesserShootLimit = new();
            isEvilGuesserExiled = new();
            RoleAndNumber = new();
            IsSkillUsed = new();
            IsEvilGuesserMeeting = false;
            canGuess = true;
            PirateGuess = 0;
        }
        public static void Add(byte PlayerId)
        {
            playerIdList.Add(PlayerId);
            if (Utils.GetPlayerById(PlayerId).Is(CustomRoles.Pirate))
                GuesserShootLimit[PlayerId] = 11;
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
            if (IsEvilGuesser) Main.AllPlayerCustomRoles[player.PlayerId] = CustomRoles.EvilGuesser;
            else if (IsNeutralGuesser) Main.AllPlayerCustomRoles[player.PlayerId] = CustomRoles.Pirate;
            else Main.AllPlayerCustomRoles[player.PlayerId] = CustomRoles.NiceGuesser;
        }
        public static void GuesserShoot(PlayerControl killer, string targetname, string targetrolenum)//ゲッサーが撃てるかどうかのチェック
        {
            if ((!killer.Is(CustomRoles.NiceGuesser) && !killer.Is(CustomRoles.EvilGuesser) && !killer.Is(CustomRoles.Pirate)) || killer.Data.IsDead || !AmongUsClient.Instance.IsGameStarted) return;
            if (killer.Is(CustomRoles.Pirate) && !canGuess) return;
            //死んでるやつとゲッサーじゃないやつ、ゲームが始まってない場合は引き返す
            if (killer.Is(CustomRoles.NiceGuesser) && IsEvilGuesserMeeting) return;//イビルゲッサー会議の最中はナイスゲッサーは打つな
            if (!CanKillMultipleTimes.GetBool() && IsSkillUsed[killer.PlayerId] && !IsEvilGuesserMeeting && !killer.Is(CustomRoles.Pirate)) return;
            if (targetname == "show")
            {
                SendShootChoices(killer.PlayerId);
                return;
            }
            foreach (var target in PlayerControl.AllPlayerControls)
            {
                if (targetname == $"{target.name}" && GuesserShootLimit[killer.PlayerId] != 0)//targetnameが人の名前で弾数が０じゃないなら続行
                {
                    RoleAndNumber.TryGetValue(int.Parse(targetrolenum), out var r);//番号から役職を取得
                    if (target.GetCustomRole() == r)//当たっていた場合
                    {
                        if (killer.Is(CustomRoles.Pirate))
                            PirateGuess++;
                        if (!killer.Is(CustomRoles.Pirate))
                            if ((target.GetCustomRole() == CustomRoles.Crewmate && !CanShootAsNormalCrewmate.GetBool()) || (target.GetCustomRole() == CustomRoles.Egoist && killer.Is(CustomRoles.EvilGuesser))) return;
                        //クルー打ちが許可されていない場合とイビルゲッサーがエゴイストを打とうとしている場合はここで帰る
                        GuesserShootLimit[killer.PlayerId]--;
                        IsSkillUsed[killer.PlayerId] = true;
                        PlayerState.SetDeathReason(target.PlayerId, PlayerState.DeathReason.Kill);
                        target.RpcGuesserMurderPlayer(0f);//専用の殺し方
                        if (PirateGuess == PirateGuessAmount.GetInt())
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
                        else canGuess = false;
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
        public static void SendShootChoices(byte PlayerId = byte.MaxValue)//番号と役職をチャットに表示
        {
            string text = "";
            if (RoleAndNumber.Count() == 0) return;
            for (var n = 1; n <= RoleAndNumber.Count(); n++)
            {
                text += string.Format("{0}:{1}\n", RoleAndNumber[n], n);
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
            List<CustomRoles> roles = new();
            var i = 1;
            foreach (var pc in PlayerControl.AllPlayerControls)//とりあえずアサインされた役職をすべて取りだす
            {
                var role = pc.GetCustomRole();
                if (IsEvilGuesser)
                {
                    if (!roles.Contains(pc.GetCustomRole()) && !role.IsImpostorTeam()) roles.Add(pc.GetCustomRole());
                }
                else if (IsNeutralGuesser)
                {
                    if (!roles.Contains(pc.GetCustomRole()) && role != CustomRoles.Pirate) roles.Add(pc.GetCustomRole());
                }
                else
                {
                    if (!roles.Contains(pc.GetCustomRole()) && !role.IsCrewmate()) roles.Add(pc.GetCustomRole());
                }
            }
            if (Options.CanMakeMadmateCount.GetInt() != 0) roles.Add(CustomRoles.SKMadmate);//SKMadmateがいる際にはサイドキック前から候補に入れておく。
            if (CustomRoles.SchrodingerCat.IsEnable())//シュレネコがいる場合も役職変化前から候補に入れておく。
            {
                roles.Add(CustomRoles.MSchrodingerCat);
                if (Sheriff.IsEnable) roles.Add(CustomRoles.CSchrodingerCat);
                if (CustomRoles.Egoist.IsEnable()) roles.Add(CustomRoles.EgoSchrodingerCat);
                if (CustomRoles.Jackal.IsEnable()) roles.Add(CustomRoles.JSchrodingerCat);
            }
            roles = roles.OrderBy(a => Guid.NewGuid()).ToList();//会議画面で見たときに役職と順番が一緒で、役バレしたのでシャッフル
            foreach (var ro in roles)
            {
                RoleAndNumber.Add(i, ro);
                i++;
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