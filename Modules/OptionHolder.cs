using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TownOfHost
{
    [Flags]
    public enum CustomGameMode
    {
        Standard = 0x01,
        HideAndSeek = 0x02,
        ColorWars = 0x03,
        Splatoon = 0x04,
        FFA = 0x05,
        All = int.MaxValue
    }

    public static class Options
    {
        // オプションId
        public const int PresetId = 0;

        // プリセット
        private static readonly string[] presets =
        {
            "Preset_1", "Preset_2", "Preset_3",
            "Preset_4", "Preset_5"
        };

        // ゲームモード
        public static CustomOption GameMode;
        // public static CustomGameMode CurrentGameMode
        //     => GameMode.Selection == 0 ? CustomGameMode.Standard : CustomGameMode.HideAndSeek;

        public static CustomGameMode CurrentGameMode()
        {
            switch (GameMode.Selection)
            {
                case 0:
                    return CustomGameMode.Standard;
                case 1:
                    return CustomGameMode.HideAndSeek;
                case 2:
                    return CustomGameMode.ColorWars;
                case 3:
                    return CustomGameMode.Splatoon;
                case 4:
                    return CustomGameMode.FFA;
                default:
                    return CustomGameMode.Standard;
            }
        }


        public static readonly string[] gameModes =
        {
            "Standard", "HideAndSeek", //"ColorWars", "Splatoon", "FreeForAll"
        };

        public static readonly string[] whichDisableAdmin =
        {
            "All", "Archive",
        };

        // 役職数・確率
        public static Dictionary<CustomRoles, int> roleCounts;
        public static Dictionary<CustomRoles, float> roleSpawnChances;
        public static Dictionary<CustomRoles, CustomOption> CustomRoleCounts;
        public static Dictionary<CustomRoles, CustomOption> CustomRoleSpawnChances;
        public static readonly string[] rates =
        {
            "Rate0", "Rate10", "Rate20", "Rate30", "Rate40", "Rate50",
            "Rate60", "Rate70", "Rate80", "Rate90", "Rate100",
        };
        public static readonly string[] ExecutionerChangeRoles =
        {
            CustomRoles.Crewmate.ToString(), CustomRoles.Jester.ToString(), CustomRoles.Opportunist.ToString(), CustomRoles.SchrodingerCat.ToString(),
        };
        public static readonly CustomRoles[] CRoleExecutionerChangeRoles =
        {
            CustomRoles.Crewmate, CustomRoles.Jester, CustomRoles.Opportunist, CustomRoles.SchrodingerCat,
        };

        public static readonly string[] GAChangeRoles =
        {
            CustomRoles.Crewmate.ToString(), CustomRoles.Engineer.ToString(), CustomRoles.Survivor.ToString(), CustomRoles.Amnesiac.ToString()
        };
        public static readonly CustomRoles[] CRoleGuardianAngelChangeRoles =
        {
            CustomRoles.Crewmate, CustomRoles.Engineer, CustomRoles.Survivor, CustomRoles.Amnesiac,
        };

        public static readonly string[] PestiAttacksVetString =
        {
            "VetKillsPesti", "Trade", "PestiKillsVet"
        };
        public static readonly CustomRoles[] CRolePestiAttacksVet =
        {
            CustomRoles.Crewmate, CustomRoles.Jester, CustomRoles.Opportunist,
        };

        // 各役職の詳細設定
        public static CustomOption EnableGM;
        public static CustomOption EnableLastImpostor;
        public static CustomOption LastImpostorKillCooldown;
        public static float DefaultKillCooldown = PlayerControl.GameOptions.KillCooldown;
        public static CustomOption VampireKillDelay;
        public static CustomOption VampireBuff;
        //public static CustomOption ShapeMasterShapeshiftDuration;
        public static CustomOption DefaultShapeshiftCooldown;
        public static CustomOption CanMakeMadmateCount;
        public static CustomOption MadGuardianCanSeeWhoTriedToKill;
        public static CustomOption MadSnitchCanVent;
        public static CustomOption MadmateCanFixLightsOut; // TODO:mii-47 マッド役職統一
        public static CustomOption MadmateCanFixComms;
        public static CustomOption MadmateHasImpostorVision;
        public static CustomOption MadmateVentCooldown;
        public static CustomOption MadmateVentMaxTime;

        public static CustomOption EvilWatcherChance;
        public static CustomOption LighterTaskCompletedVision;
        public static CustomOption LighterTaskCompletedDisableLightOut;
        public static CustomOption MayorAdditionalVote;
        public static CustomOption MayorHasPortableButton;
        public static CustomOption MayorNumOfUseButton;
        public static CustomOption DoctorTaskCompletedBatteryCharge;
        public static CustomOption SnitchEnableTargetArrow;
        public static CustomOption SnitchCanGetArrowColor;
        public static CustomOption SnitchCanFindNeutralKiller;
        public static CustomOption SnitchCanFindCoven;
        public static CustomOption SpeedBoosterUpSpeed;
        public static CustomOption TrapperBlockMoveTime;
        public static CustomOption ChildKnown;
        public static CustomOption SleuthReport;
        public static CustomOption JesterCanVent;
        public static CustomOption VultureCanVent;
        public static CustomOption VultureArrow;
        public static CustomOption CanTerroristSuicideWin;
        public static CustomOption ArsonistDouseTime;
        public static CustomOption ArsonistCooldown;
        public static CustomOption TOuRArso;
        public static CustomOption BastionVentsRemoveOnBomb;

        public static CustomOption NumOfCoven;

        public static CustomOption InfectCooldown;
        public static CustomOption PestilKillCooldown;
        public static CustomOption PestiCanVent;
        public static CustomOption InfectionSkip;
        public static CustomOption CanBeforeSchrodingerCatWinTheCrewmate;
        public static CustomOption SchrodingerCatExiledTeamChanges;
        public static CustomOption ExecutionerCanTargetImpostor;
        public static CustomOption ExecutionerChangeRolesAfterTargetKilled;
        public static CustomOption JackalKillCooldown;
        public static CustomOption JackalCanVent;
        public static CustomOption JackalCanUseSabotage;
        public static CustomOption JackalHasImpostorVision;

        public static CustomOption JackalHasSidekick;
        public static CustomOption SidekickCanKill;
        public static CustomOption SidekickGetsPromoted;

        public static CustomOption JesterHasImpostorVision;
        public static CustomOption VultureHasImpostorVision;

        public static CustomOption JuggerKillCooldown;
        public static CustomOption JuggerCanVent;
        public static CustomOption JuggerDecrease;

        public static CustomOption MarksmanKillCooldown;
        public static CustomOption MarksmanCanVent;

        public static CustomOption SilenceCooldown;

        // HideAndSeek
        public static CustomOption AllowCloseDoors;
        public static CustomOption KillDelay;
        public static CustomOption IgnoreCosmetics;
        public static CustomOption IgnoreVent;
        public static CustomOption FlashDuration;
        public static CustomOption FlashCooldown;
        public static CustomOption GrenadierCanVent;
        public static float HideAndSeekKillDelayTimer = 0f;

        // COLOR WARS //

        public static CustomOption CWAllowCloseDoors;
        public static CustomOption CWCD;
        public static CustomOption CWIgnoreVent;

        // SPLATOON //

        public static CustomOption STCD;
        public static CustomOption STIgnoreVent;

        //デバイスブロック
        public static CustomOption DisableDevices;
        public static CustomOption DisableAdmin;
        public static CustomOption WhichDisableAdmin;

        // ボタン回数
        public static CustomOption SyncButtonMode;
        public static CustomOption SyncedButtonCount;
        public static int UsedButtonCount = 0;

        // タスク無効化
        public static CustomOption DisableTasks;
        public static CustomOption DisableSwipeCard;
        public static CustomOption DisableSubmitScan;
        public static CustomOption DisableUnlockSafe;
        public static CustomOption DisableUploadData;
        public static CustomOption DisableStartReactor;
        public static CustomOption DisableResetBreaker;
        public static CustomOption DisableFixWiring;

        // ランダムマップ
        public static CustomOption RandomMapsMode;
        public static CustomOption AddedTheSkeld;
        public static CustomOption AddedMiraHQ;
        public static CustomOption AddedPolus;
        public static CustomOption AddedTheAirShip;
        public static CustomOption AddedDleks;

        // 投票モード
        public static CustomOption VoteMode;
        public static CustomOption WhenSkipVote;
        public static CustomOption WhenNonVote;
        public static CustomOption Customise;
        public static CustomOption RolesLikeToU;
        public static readonly string[] voteModes =
        {
            "Default", "Suicide", "SelfVote", "Skip"
        };
        public static VoteMode GetWhenSkipVote() => (VoteMode)WhenSkipVote.GetSelection();
        public static VoteMode GetWhenNonVote() => (VoteMode)WhenNonVote.GetSelection();

        //転落死
        public static CustomOption LadderDeath;
        public static CustomOption LadderDeathChance;

        // 通常モードでかくれんぼ
        public static bool IsStandardHAS => StandardHAS.GetBool() && CurrentGameMode() == CustomGameMode.Standard;
        public static CustomOption StandardHAS;
        public static CustomOption StandardHASWaitingTime;

        // リアクターの時間制御
        public static CustomOption SabotageTimeControl;
        public static CustomOption PolusReactorTimeLimit;
        public static CustomOption AirshipReactorTimeLimit;
        public static CustomOption PaintersHaveImpVision;

        // タスク上書き
        public static OverrideTasksData MadGuardianTasks;
        public static OverrideTasksData TerroristTasks;
        public static OverrideTasksData SnitchTasks;
        public static OverrideTasksData MadSnitchTasks;

        // その他
        public static CustomOption NoGameEnd;
        public static CustomOption CamoComms;
        public static CustomOption AutoDisplayLastResult;
        public static CustomOption SuffixMode;
        public static CustomOption ColorNameMode;
        public static CustomOption GhostCanSeeOtherRoles;
        public static CustomOption GhostCanSeeOtherVotes;
        public static CustomOption HideGameSettings;
        public static CustomOption BodiesAmount;
        public static CustomOption ModifierRestrict;

        public static CustomOption BewilderVision;
        public static CustomOption FlashSpeed;
        public static CustomOption DiseasedMultiplier;

        public static CustomOption LoversDieTogether;
        public static CustomOption LoversKnowRoleOfOtherLover;

        //coven
        //coven main info
        public static CustomOption CovenKillCooldown;
        public static CustomOption CovenMeetings;
        //role info
        public static CustomOption HexMasterOn;
        public static CustomOption PotionMasterOn;
        public static CustomOption VampireDitchesOn;
        public static CustomOption MedusaOn;
        public static CustomOption MimicOn;
        public static CustomOption NecromancerOn;
        public static CustomOption ConjurorOn;

        public static CustomOption StoneCD;
        public static CustomOption StoneDuration;
        public static CustomOption StoneReport;
        public static CustomOption HexCD;
        public static CustomOption MaxHexesPerRound;
        public static CustomOption PKTAH;
        public static CustomOption NecroCanUseSheriff;

        //VETERAN
        public static CustomOption VetCD;
        public static CustomOption VetDuration;
        public static CustomOption NumOfVets;
        public static CustomOption CrewRolesVetted;
        public static CustomOption PestiAttacksVet;

        // SURVIVOR //
        public static CustomOption VestCD;
        public static CustomOption VestDuration;
        public static CustomOption NumOfVests;


        // OTHER NEUTRALS

        // BLOOD-KNIHT //

        public static CustomOption BKcanVent;
        public static CustomOption BKkillCd;
        public static CustomOption BKprotectDur;

        // THE GLITCH //
        public static CustomOption GlitchCanVent;
        public static CustomOption GlitchRoleBlockCooldown;
        public static CustomOption GlobalRoleBlockDuration;
        public static CustomOption GlitchKillCooldown;
        // WEREWOLF //
        public static CustomOption RampageCD;
        public static CustomOption RampageDur;
        public static CustomOption WWkillCD;
        public static CustomOption VentWhileRampaged;
        // GUARDIAN ANGEL (TOU VERSION) //
        public static CustomOption NumOfProtects;
        public static CustomOption GuardDur;
        public static CustomOption GuardCD;
        public static CustomOption GAknowsRole;
        public static CustomOption TargetKnowsGA;
        public static CustomOption WhenGaTargetDies;
        public static CustomOption SaboAmount;
        public static CustomOption DemoSuicideTime;

        // RANDOM ROLES INFO //
        public static CustomOption MaxNK;
        public static CustomOption MinNK;
        public static CustomOption MaxNonNK;
        public static CustomOption MinNonNK;
        public static CustomOption ImpostorKnowsRolesOfTeam;
        public static CustomOption CovenKnowsRolesOfTeam;

        // GAMEMODE //
        public static CustomOption SplatoonOn;
        public static CustomOption ColorWarsOn;
        public static CustomOption FreeForAllOn;

        // TRAITOR //
        public static CustomOption PlayersForTraitor;
        public static CustomOption SheriffCorrupted;
        public static CustomOption TraitorCanSpawnIfNK;
        public static CustomOption TraitorCanSpawnIfCoven;
        public static CustomOption LaptopPercentages;

        // PSYCHIC //
        public static CustomOption CkshowEvil;
        public static CustomOption NBshowEvil;
        public static CustomOption NEshowEvil;
        public static CustomOption MadmatesAreEvil;
        public static CustomOption GAdependsOnTaregtRole;
        public static CustomOption ExeTargetShowsEvil;
        // PHANTOM //
        public static CustomOption TasksRemainingForPhantomClicked;
        public static CustomOption TasksRemaningForPhantomAlert;
        // YIN YANGER or COLLIDER //
        public static CustomOption YinYangCooldown;
        public static CustomOption ResetToYinYang;
        public static readonly string[] suffixModes =
        {
            "SuffixMode.None",
            "SuffixMode.Version",
            "SuffixMode.Streaming",
            "SuffixMode.Recording"
        };
        public static SuffixModes GetSuffixMode()
        {
            return (SuffixModes)SuffixMode.GetSelection();
        }



        public static int SnitchExposeTaskLeft = 1;


        public static bool IsEvilWatcher = false;
        public static void SetWatcherTeam(float EvilWatcherRate)
        {
            EvilWatcherRate = Options.EvilWatcherChance.GetFloat();
            IsEvilWatcher = UnityEngine.Random.Range(1, 100) < EvilWatcherRate;
        }
        private static bool IsLoaded = false;

        static Options()
        {
            ResetRoleCounts();
        }
        public static void ResetRoleCounts()
        {
            roleCounts = new Dictionary<CustomRoles, int>();
            roleSpawnChances = new Dictionary<CustomRoles, float>();

            foreach (var role in Enum.GetValues(typeof(CustomRoles)).Cast<CustomRoles>())
            {
                roleCounts.Add(role, 0);
                roleSpawnChances.Add(role, 0);
            }
        }

        public static void SetRoleCount(CustomRoles role, int count)
        {
            roleCounts[role] = count;

            if (CustomRoleCounts.TryGetValue(role, out var option))
            {
                option.UpdateSelection(count - 1);
            }
        }

        public static int GetRoleCount(CustomRoles role)
        {
            var chance = CustomRoleSpawnChances.TryGetValue(role, out var sc) ? sc.GetSelection() : 0;
            return chance == 0 ? 0 : CustomRoleCounts.TryGetValue(role, out var option) ? (int)option.GetFloat() : roleCounts[role];
        }

        public static float GetRoleChance(CustomRoles role)
        {
            return CustomRoleSpawnChances.TryGetValue(role, out var option) ? option.GetSelection() / 10f : roleSpawnChances[role];
        }
        public static void Load()
        {
            if (IsLoaded) return;
            // プリセット
            _ = CustomOption.Create(0, new Color(204f / 255f, 204f / 255f, 0, 1f), "Preset", presets, presets[0], null, true)
                .HiddenOnDisplay(true)
                .SetGameMode(CustomGameMode.All);

            // ゲームモード
            GameMode = CustomOption.Create(1, new Color(204f / 255f, 204f / 255f, 0, 1f), "GameMode", gameModes, gameModes[0], null, true)
                .SetGameMode(CustomGameMode.All);

            #region 役職・詳細設定
            CustomRoleCounts = new Dictionary<CustomRoles, CustomOption>();
            CustomRoleSpawnChances = new Dictionary<CustomRoles, CustomOption>();
            // GM
            //EnableGM = CustomOption.Create(100, Utils.GetRoleColor(CustomRoles.GM), "GM", false, null, true)
            //     .SetGameMode(CustomGameMode.Standard);
            LaptopPercentages = CustomOption.Create(100, Color.white, "LaptopPercentages", false, null, true);

            // Impostor
            BountyHunter.SetupCustomOption();
            SerialKiller.SetupCustomOption();
            // SetupRoleOptions(1200, CustomRoles.ShapeMaster);
            // ShapeMasterShapeshiftDuration = CustomOption.Create(1210, Color.white, "ShapeMasterShapeshiftDuration", 10, 1, 1000, 1, CustomRoleSpawnChances[CustomRoles.ShapeMaster]);
            SetupRoleOptions(1300, CustomRoles.Vampire);
            VampireKillDelay = CustomOption.Create(1310, Color.white, "VampireKillDelay", 5, 1, 1000, 1, CustomRoleSpawnChances[CustomRoles.Vampire]);
            VampireBuff = CustomOption.Create(1311, Color.white, "VampBuff", true, CustomRoleSpawnChances[CustomRoles.Vampire]);
            SetupRoleOptions(1400, CustomRoles.Warlock);
            SetupRoleOptions(1500, CustomRoles.Witch);
            SetupRoleOptions(1600, CustomRoles.Mafia);
            FireWorks.SetupCustomOption();
            Sniper.SetupCustomOption();
            SetupRoleOptions(2000, CustomRoles.Puppeteer);
            SetupSingleRoleOptions(200099, CustomRoles.YingYanger, 1);
            YinYangCooldown = CustomOption.Create(1213, Color.white, "YinYangCooldown", 15, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.YingYanger]);
            ResetToYinYang = CustomOption.Create(1314, Color.white, "ResetToYinYang", true, CustomRoleSpawnChances[CustomRoles.YingYanger]);
            SetupSingleRoleOptions(9999, CustomRoles.Grenadier, 1);
            FlashCooldown = CustomOption.Create(200129, Color.white, "FlashCD", 30, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Grenadier]);
            FlashDuration = CustomOption.Create(2001299, Color.white, "FlashDur", 15, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Grenadier]);
            GrenadierCanVent = CustomOption.Create(1312, Color.white, "GrenadierCanVent", true, CustomRoleSpawnChances[CustomRoles.Grenadier]);
            Mare.SetupCustomOption();
            TimeThief.SetupCustomOption();
            SetupRoleOptions(2009, CustomRoles.VoteStealer);
            Camouflager.SetupCustomOption();
            Ninja.SetupCustomOption();
            SetupSingleRoleOptions(22434, CustomRoles.CorruptedSheriff, 1);
            PlayersForTraitor = CustomOption.Create(2040030, Color.white, "TraitorSpawn", 1, 0, 15, 1, CustomRoleSpawnChances[CustomRoles.CorruptedSheriff]);
            TraitorCanSpawnIfNK = CustomOption.Create(2040031, Color.white, "TraitorCanSpawnIfNK", true, CustomRoleSpawnChances[CustomRoles.CorruptedSheriff]);
            TraitorCanSpawnIfCoven = CustomOption.Create(2040032, Color.white, "TraitorCanSpawnIfCoven", true, CustomRoleSpawnChances[CustomRoles.CorruptedSheriff]);
            //SetupSingleRoleOptions(2600, CustomRoles.Silencer, 1);
            //SilenceCooldown = CustomOption.Create(2610, Color.white, "SilenceDelay", 30, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Silencer]);
            //SetupSingleRoleOptions(2700, CustomRoles.CorruptedSheriff, 1);
            //PlayersForTraitor = CustomOption.Create(2710, Color.white, "TraitorSpawn", 1, 0, 15, 1, CustomRoleSpawnChances[CustomRoles.CorruptedSheriff]);

            DefaultShapeshiftCooldown = CustomOption.Create(5011, Color.white, "DefaultShapeshiftCooldown", 15, 5, 999, 5, null, true);
            CanMakeMadmateCount = CustomOption.Create(5012, Color.white, "CanMakeMadmateCount", 0, 0, 15, 1, null, true);

            // Madmate
            SetupRoleOptions(10000, CustomRoles.Madmate);
            SetupRoleOptions(10100, CustomRoles.MadGuardian);
            MadGuardianCanSeeWhoTriedToKill = CustomOption.Create(10110, Color.white, "MadGuardianCanSeeWhoTriedToKill", false, CustomRoleSpawnChances[CustomRoles.MadGuardian]);
            //ID10120~10123を使用
            MadGuardianTasks = OverrideTasksData.Create(10120, CustomRoles.MadGuardian);
            SetupRoleOptions(205000, CustomRoles.CrewPostor);
            SetupRoleOptions(10200, CustomRoles.MadSnitch);
            MadSnitchCanVent = CustomOption.Create(10210, Color.white, "MadSnitchCanVent", false, CustomRoleSpawnChances[CustomRoles.MadSnitch]);
            //ID10220~10223を使用
            MadSnitchTasks = OverrideTasksData.Create(10220, CustomRoles.MadSnitch);
            SetupSingleRoleOptions(10333, CustomRoles.Parasite, 1);
            // Madmate Common Options
            MadmateCanFixLightsOut = CustomOption.Create(15010, Color.white, "MadmateCanFixLightsOut", false, null, true, false);
            MadmateCanFixComms = CustomOption.Create(15011, Color.white, "MadmateCanFixComms", false);
            MadmateHasImpostorVision = CustomOption.Create(15012, Color.white, "MadmateHasImpostorVision", false);
            MadmateVentCooldown = CustomOption.Create(15213, Color.white, "MadmateVentCooldown", 0f, 0f, 180f, 5f);
            MadmateVentMaxTime = CustomOption.Create(15214, Color.white, "MadmateVentMaxTime", 0f, 0f, 180f, 5f);
            // Both
            //SetupRoleOptions(30000, CustomRoles.Watcher);
            //EvilWatcherChance = CustomOption.Create(30010, Color.white, "EvilWatcherChance", 0, 0, 100, 10, CustomRoleSpawnChances[CustomRoles.Watcher]);
            Guesser.SetupCustomOption();
            // Crewmate
            //SetupRoleOptions(20000, CustomRoles.Bait);
            //  SetupRoleOptions(20001, CustomRoles.Sleuth);
            /* SetupSingleRoleOptions(20002, CustomRoles.Oblivious, 1);
             SetupSingleRoleOptions(20003, CustomRoles.TieBreaker, 1);
             SetupSingleRoleOptions(20004, CustomRoles.Torch, 1);*/
            SetupRoleOptions(20002, CustomRoles.Demolitionist);
            DemoSuicideTime = CustomOption.Create(20004, Color.white, "DemoSuicideTime", 5f, 1f, 180, 1, CustomRoleSpawnChances[CustomRoles.Demolitionist]);
            /*SetupSingleRoleOptions(20006, CustomRoles.Flash, 1);
            FlashSpeed = CustomOption.Create(20030, Color.white, "SpeedBoosterUpSpeed", 2f, 0.25f, 3f, 0.25f, CustomRoleSpawnChances[CustomRoles.Flash]); */
            SetupRoleOptions(20850, CustomRoles.Bastion);
            BastionVentsRemoveOnBomb = CustomOption.Create(1319, Color.white, "BastionVentsRemoveOnBomb", true, CustomRoleSpawnChances[CustomRoles.Bastion]);
            //SetupRoleOptions(700850, CustomRoles.Alturist);
            SetupRoleOptions(700850, CustomRoles.Psychic);
            CkshowEvil = CustomOption.Create(1318, Color.white, "CrewKillingRed", true, CustomRoleSpawnChances[CustomRoles.Psychic]);
            NBshowEvil = CustomOption.Create(1313, Color.white, "NBareRed", true, CustomRoleSpawnChances[CustomRoles.Psychic]);
            NEshowEvil = CustomOption.Create(1314, Color.white, "NEareRed", true, CustomRoleSpawnChances[CustomRoles.Psychic]);
            MadmatesAreEvil = CustomOption.Create(1315, Color.white, "MadMateIsRed", true, CustomRoleSpawnChances[CustomRoles.Psychic]);
            GAdependsOnTaregtRole = CustomOption.Create(1316, Color.white, "GAdependsOnTaregtRole", true, CustomRoleSpawnChances[CustomRoles.Psychic]);
            ExeTargetShowsEvil = CustomOption.Create(1317, Color.white, "ExeTargetShowsEvil", true, CustomRoleSpawnChances[CustomRoles.Psychic]);
            SetupSingleRoleOptions(20010, CustomRoles.Veteran, 1);
            NumOfVets = CustomOption.Create(20011, Color.white, "NVet", 10, 1, 15, 1, CustomRoleSpawnChances[CustomRoles.Veteran]);
            VetCD = CustomOption.Create(20012, Color.white, "VetCD", 30, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Veteran]);
            VetDuration = CustomOption.Create(20013, Color.white, "VetDur", 15, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Veteran]);
            CrewRolesVetted = CustomOption.Create(20014, Color.white, "CRGV", true, CustomRoleSpawnChances[CustomRoles.Veteran]);
            PestiAttacksVet = CustomOption.Create(20015, Color.white, "PestiAttacks", PestiAttacksVetString, PestiAttacksVetString[2], CustomRoleSpawnChances[CustomRoles.Veteran]);
            SetupRoleOptions(20100, CustomRoles.Lighter);
            LighterTaskCompletedVision = CustomOption.Create(20110, Color.white, "LighterTaskCompletedVision", 2f, 0f, 5f, 0.25f, CustomRoleSpawnChances[CustomRoles.Lighter]);
            LighterTaskCompletedDisableLightOut = CustomOption.Create(20111, Color.white, "LighterTaskCompletedDisableLightOut", true, CustomRoleSpawnChances[CustomRoles.Lighter]);
            SetupRoleOptions(20200, CustomRoles.Mayor);
            MayorAdditionalVote = CustomOption.Create(20210, Color.white, "MayorAdditionalVote", 1, 1, 99, 1, CustomRoleSpawnChances[CustomRoles.Mayor]);
            MayorHasPortableButton = CustomOption.Create(20211, Color.white, "MayorHasPortableButton", false, CustomRoleSpawnChances[CustomRoles.Mayor]);
            MayorNumOfUseButton = CustomOption.Create(20212, Color.white, "MayorNumOfUseButton", 1, 1, 99, 1, MayorHasPortableButton);
            SabotageMaster.SetupCustomOption();
            Sheriff.SetupCustomOption();
            Investigator.SetupCustomOption();
            SetupRoleOptions(20500, CustomRoles.Snitch);
            SnitchEnableTargetArrow = CustomOption.Create(20510, Color.white, "SnitchEnableTargetArrow", false, CustomRoleSpawnChances[CustomRoles.Snitch]);
            SnitchCanGetArrowColor = CustomOption.Create(20511, Color.white, "SnitchCanGetArrowColor", false, CustomRoleSpawnChances[CustomRoles.Snitch]);
            SnitchCanFindNeutralKiller = CustomOption.Create(20512, Color.white, "SnitchCanFindNeutralKiller", false, CustomRoleSpawnChances[CustomRoles.Snitch]);
            SnitchCanFindCoven = CustomOption.Create(20513, Color.white, "SnitchCanFindCoven", false, CustomRoleSpawnChances[CustomRoles.Snitch]);
            //20520~20523を使用
            SnitchTasks = OverrideTasksData.Create(20520, CustomRoles.Snitch);
            SetupRoleOptions(121400, CustomRoles.Medium);
            SetupRoleOptions(20600, CustomRoles.SpeedBooster);
            SpeedBoosterUpSpeed = CustomOption.Create(20610, Color.white, "SpeedBoosterUpSpeed", 2f, 0.25f, 3f, 0.25f, CustomRoleSpawnChances[CustomRoles.SpeedBooster]);
            SetupRoleOptions(20700, CustomRoles.Doctor);
            DoctorTaskCompletedBatteryCharge = CustomOption.Create(20710, Color.white, "DoctorTaskCompletedBatteryCharge", 5, 0, 10, 1, CustomRoleSpawnChances[CustomRoles.Doctor]);
            SetupRoleOptions(20800, CustomRoles.Trapper);
            TrapperBlockMoveTime = CustomOption.Create(20810, Color.white, "TrapperBlockMoveTime", 5f, 1f, 180, 1, CustomRoleSpawnChances[CustomRoles.Trapper]);
            SetupRoleOptions(20900, CustomRoles.Dictator);

            SetupRoleOptions(30009, CustomRoles.Mystic);

            SetupRoleOptions(30008, CustomRoles.Child);
            ChildKnown = CustomOption.Create(30011, Color.white, "ChildKnown", false, CustomRoleSpawnChances[CustomRoles.Child]);
            //SetupRoleOptions(30100, CustomRoles.Sleuth);
            //SleuthReport = CustomOption.Create(30110, Color.white, "SleuthReport", false, CustomRoleSpawnChances[CustomRoles.Sleuth]);
            // Neutral
            SetupSingleRoleOptions(50500, CustomRoles.Arsonist, 1);
            ArsonistDouseTime = CustomOption.Create(50510, Color.white, "ArsonistDouseTime", 3, 0, 10, 1, CustomRoleSpawnChances[CustomRoles.Arsonist]);
            ArsonistCooldown = CustomOption.Create(50511, Color.white, "ArsonistCooldown", 10, 5, 100, 1, CustomRoleSpawnChances[CustomRoles.Arsonist]);
            TOuRArso = CustomOption.Create(50512, Color.white, "TourArso", false, CustomRoleSpawnChances[CustomRoles.Arsonist]);
            SetupSingleRoleOptions(50550, CustomRoles.PlagueBearer, 1);
            InfectCooldown = CustomOption.Create(50560, Color.white, "InfectCD", 20, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.PlagueBearer]);
            PestilKillCooldown = CustomOption.Create(50561, Color.white, "PestiKillCooldown", 30, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.PlagueBearer]);
            PestiCanVent = CustomOption.Create(50562, Color.white, "PestiCanVent", true, CustomRoleSpawnChances[CustomRoles.PlagueBearer]);
            InfectionSkip = CustomOption.Create(50563, Color.white, "SkipInfect", false, CustomRoleSpawnChances[CustomRoles.PlagueBearer]);
            SetupRoleOptions(50000, CustomRoles.Jester);
            JesterCanVent = CustomOption.Create(50010, Color.white, "JesterVent", false, CustomRoleSpawnChances[CustomRoles.Jester]);
            JesterHasImpostorVision = CustomOption.Create(6000013, Color.white, "JesterHasImpostorVision", false, CustomRoleSpawnChances[CustomRoles.Jester]);

            SetupRoleOptions(50100, CustomRoles.Survivor);
            NumOfVests = CustomOption.Create(50110, Color.white, "NVest", 11, 1, 15, 1, CustomRoleSpawnChances[CustomRoles.Survivor]);
            VestCD = CustomOption.Create(50120, Color.white, "VestCD", 30, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Survivor]);
            VestDuration = CustomOption.Create(50130, Color.white, "VestDur", 15, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Survivor]);
            SetupRoleOptions(50200, CustomRoles.Terrorist);
            CanTerroristSuicideWin = CustomOption.Create(50210, Color.white, "CanTerroristSuicideWin", false, CustomRoleSpawnChances[CustomRoles.Terrorist], false)
                .SetGameMode(CustomGameMode.Standard);
            //50220~50223を使用
            TerroristTasks = OverrideTasksData.Create(50220, CustomRoles.Terrorist);
            SetupRoleOptions(50250, CustomRoles.Hacker);
            SaboAmount = CustomOption.Create(50260, Color.white, "SA", 20, 10, 99, 1, CustomRoleSpawnChances[CustomRoles.Hacker]);
            SetupSingleRoleOptions(50300, CustomRoles.LoversRecode, 2);
            LoversDieTogether = CustomOption.Create(503005, Color.white, "LoversDieTogether", false, CustomRoleSpawnChances[CustomRoles.LoversRecode]);
            LoversKnowRoleOfOtherLover = CustomOption.Create(503005, Color.white, "LoversKnowRoleOfOtherLover", true, CustomRoleSpawnChances[CustomRoles.LoversRecode]);

            SetupSingleRoleOptions(905003, CustomRoles.Amnesiac, 1);
            SetupSingleRoleOptions(905004, CustomRoles.Phantom, 1);
            TasksRemainingForPhantomClicked = CustomOption.Create(50515, Color.white, "TasksRemainingForPhantomClicked", 3, 1, 10, 1, CustomRoleSpawnChances[CustomRoles.Phantom]);
            TasksRemaningForPhantomAlert = CustomOption.Create(50516, Color.white, "TasksRemaningForPhantomAlert", 1, 1, 5, 1, CustomRoleSpawnChances[CustomRoles.Phantom]);

            SetupRoleOptions(50400, CustomRoles.SchrodingerCat);
            CanBeforeSchrodingerCatWinTheCrewmate = CustomOption.Create(50410, Color.white, "CanBeforeSchrodingerCatWinTheCrewmate", false, CustomRoleSpawnChances[CustomRoles.SchrodingerCat]);
            SchrodingerCatExiledTeamChanges = CustomOption.Create(50411, Color.white, "SchrodingerCatExiledTeamChanges", false, CustomRoleSpawnChances[CustomRoles.SchrodingerCat]);
            Egoist.SetupCustomOption();
            //SetupRoleOptions(50680, CustomRoles.Amnesiac);
            SetupRoleOptions(50700, CustomRoles.Executioner);
            ExecutionerCanTargetImpostor = CustomOption.Create(50710, Color.white, "ExecutionerCanTargetImpostor", false, CustomRoleSpawnChances[CustomRoles.Executioner]);
            ExecutionerChangeRolesAfterTargetKilled = CustomOption.Create(50711, Color.white, "ExecutionerChangeRolesAfterTargetKilled", ExecutionerChangeRoles, ExecutionerChangeRoles[1], CustomRoleSpawnChances[CustomRoles.Executioner]);
            //Jackalは1人固定
            SetupSingleRoleOptions(509000, CustomRoles.BloodKnight, 1);
            BKcanVent = CustomOption.Create(09005, Color.white, "CanVent", true, CustomRoleSpawnChances[CustomRoles.BloodKnight]);
            BKkillCd = CustomOption.Create(509010, Color.white, "KillCD", 30, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.BloodKnight]);
            BKprotectDur = CustomOption.Create(509011, Color.white, "BKdur", 15, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.BloodKnight]);

            SetupSingleRoleOptions(50900, CustomRoles.Jackal, 1);
            JackalKillCooldown = CustomOption.Create(50910, Color.white, "JackalKillCooldown", 30, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Jackal]);
            JackalCanVent = CustomOption.Create(50911, Color.white, "JackalCanVent", true, CustomRoleSpawnChances[CustomRoles.Jackal]);
            JackalCanUseSabotage = CustomOption.Create(50912, Color.white, "JackalCanUseSabotage", false, CustomRoleSpawnChances[CustomRoles.Jackal]);
            JackalHasImpostorVision = CustomOption.Create(50913, Color.white, "JackalHasImpostorVision", true, CustomRoleSpawnChances[CustomRoles.Jackal]);

            JackalHasSidekick = CustomOption.Create(50914, Color.white, "JackalHasSidekick", false, CustomRoleSpawnChances[CustomRoles.Jackal]);
            SidekickCanKill = CustomOption.Create(50915, Color.white, "SidekickCanKill", false, JackalHasSidekick);
            SidekickGetsPromoted = CustomOption.Create(50916, Color.white, "SidekickGetsPromoted", true, JackalHasSidekick);

            SetupSingleRoleOptions(60000, CustomRoles.Coven, 3);
            CovenKillCooldown = CustomOption.Create(60020, Color.white, "CovenKillCooldown", 30, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Coven]);
            CovenMeetings = CustomOption.Create(60021, Color.white, "CovenMeetings", 3, 0, 15, 1, CustomRoleSpawnChances[CustomRoles.Coven]);
            HexMasterOn = CustomOption.Create(60022, Color.white, "HexMasterOn", false, CustomRoleSpawnChances[CustomRoles.Coven]);

            HexCD = CustomOption.Create(60028, Color.white, "HexCD", 30, 2.5f, 180, 2.5f, HexMasterOn);
            PKTAH = CustomOption.Create(60029, Color.white, "PKTAH", true, HexMasterOn);
            MaxHexesPerRound = CustomOption.Create(60030, Color.white, "MHPR", 3, 1, 15, 1, HexMasterOn);

            //PotionMasterOn = CustomOption.Create(60013, Color.white, "PotionMasterOn", false, CustomRoleSpawnChances[CustomRoles.Coven]);
            VampireDitchesOn = CustomOption.Create(60014, Color.white, "VampireDitchesOn", false, CustomRoleSpawnChances[CustomRoles.Coven]);

            MedusaOn = CustomOption.Create(60015, Color.white, "MedusaOn", false, CustomRoleSpawnChances[CustomRoles.Coven]);
            StoneCD = CustomOption.Create(60025, Color.white, "StoneCD", 30, 2.5f, 180, 2.5f, MedusaOn);
            StoneDuration = CustomOption.Create(60026, Color.white, "StoneDur", 15, 2.5f, 180, 2.5f, MedusaOn);
            StoneReport = CustomOption.Create(60027, Color.white, "StoneTime", 35, 2.5f, 180, 2.5f, MedusaOn);

            //MimicOn = CustomOption.Create(60016, Color.white, "MimicOn", false, CustomRoleSpawnChances[CustomRoles.Coven]);
            //NecromancerOn = CustomOption.Create(60017, Color.white, "NecromancerOn", false, CustomRoleSpawnChances[CustomRoles.Coven]);
            // NecroCanUseSheriff = CustomOption.Create(60019, Color.white, "NecroCanUseSheriff", false, NecromancerOn);
            //ConjurorOn = CustomOption.Create(60018, Color.white, "ConjurorOn", false, CustomRoleSpawnChances[CustomRoles.Coven]);

            SetupSingleRoleOptions(70000, CustomRoles.Juggernaut, 1);
            JuggerKillCooldown = CustomOption.Create(60010, Color.white, "JuggerKillCooldown", 40, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Juggernaut]);
            JuggerDecrease = CustomOption.Create(60011, Color.white, "JuggerDecrease", 5, 2.5f, 60, 2.5f, CustomRoleSpawnChances[CustomRoles.Juggernaut]);
            JuggerCanVent = CustomOption.Create(60012, Color.white, "JuggerCanVent", true, CustomRoleSpawnChances[CustomRoles.Juggernaut]);

            SetupSingleRoleOptions(70001, CustomRoles.Marksman, 1);
            MarksmanKillCooldown = CustomOption.Create(600110, Color.white, "MarksmanKillCooldown", 30, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Marksman]);
            MarksmanCanVent = CustomOption.Create(60032, Color.white, "MarksmanCanVent", true, CustomRoleSpawnChances[CustomRoles.Marksman]);

            SetupSingleRoleOptions(80000, CustomRoles.Vulture, 1);
            BodiesAmount = CustomOption.Create(50515, Color.white, "Bodies", 3, 1, 10, 1, CustomRoleSpawnChances[CustomRoles.Vulture]);
            VultureCanVent = CustomOption.Create(6000017, Color.white, "VultureVent", false, CustomRoleSpawnChances[CustomRoles.Vulture]);
            VultureHasImpostorVision = CustomOption.Create(6000015, Color.white, "VultureHasImpostorVision", false, CustomRoleSpawnChances[CustomRoles.Vulture]);
            VultureArrow = CustomOption.Create(6000019, Color.white, "VultureHasArrow", false, CustomRoleSpawnChances[CustomRoles.Vulture]);

            SetupSingleRoleOptions(80500, CustomRoles.TheGlitch, 1);
            GlitchRoleBlockCooldown = CustomOption.Create(80510, Color.white, "RBC", 20, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.TheGlitch]);
            GlitchKillCooldown = CustomOption.Create(80511, Color.white, "KillCD", 30, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.TheGlitch]);
            GlitchCanVent = CustomOption.Create(80512, Color.white, "HPV", true, CustomRoleSpawnChances[CustomRoles.TheGlitch]);

            SetupSingleRoleOptions(90000, CustomRoles.Werewolf, 1);
            RampageCD = CustomOption.Create(90010, Color.white, "RCD", 30, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Werewolf]);
            RampageDur = CustomOption.Create(90020, Color.white, "RDur", 25, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Werewolf]);
            WWkillCD = CustomOption.Create(90030, Color.white, "KillCD", 3, 1, 30, 1, CustomRoleSpawnChances[CustomRoles.Werewolf]);
            VentWhileRampaged = CustomOption.Create(90040, Color.white, "CanVentR", true, CustomRoleSpawnChances[CustomRoles.Werewolf]);

            SetupSingleRoleOptions(90500, CustomRoles.GuardianAngelTOU, 1);
            NumOfProtects = CustomOption.Create(905010, Color.white, "NProtects", 15, 1, 15, 1, CustomRoleSpawnChances[CustomRoles.GuardianAngelTOU]);
            GuardCD = CustomOption.Create(90511, Color.white, "PCD", 30, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.GuardianAngelTOU]);
            GuardDur = CustomOption.Create(90512, Color.white, "PDur", 30, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.GuardianAngelTOU]);
            GAknowsRole = CustomOption.Create(90513, Color.white, "GAKR", true, CustomRoleSpawnChances[CustomRoles.GuardianAngelTOU]);
            TargetKnowsGA = CustomOption.Create(90514, Color.white, "TKGA", true, CustomRoleSpawnChances[CustomRoles.GuardianAngelTOU]);
            WhenGaTargetDies = CustomOption.Create(90515, Color.white, "WhenGAdies", GAChangeRoles, GAChangeRoles[2], CustomRoleSpawnChances[CustomRoles.GuardianAngelTOU]);
            //NumOfCoven = CustomOption.Create(60010, Color.white, "ArsonistDouseTime", 3, 1, 3, 1, CustomRoleSpawnChances[CustomRoles.Coven]);

            SetupSingleRoleOptions(200045, CustomRoles.Torch, 1);
            SetupSingleRoleOptions(20000, CustomRoles.Bait, 1);
            SetupSingleRoleOptions(20005, CustomRoles.Bewilder, 1);
            BewilderVision = CustomOption.Create(20020, Color.white, "BewilderVision", 0.5f, 0f, 5f, 0.25f, CustomRoleSpawnChances[CustomRoles.Bewilder]);
            SetupSingleRoleOptions(200025, CustomRoles.Diseased, 1);
            DiseasedMultiplier = CustomOption.Create(20021, Color.white, "DiseasedMultiplier", 2f, 1.5f, 5f, 0.25f, CustomRoleSpawnChances[CustomRoles.Diseased]);

            SetupSingleRoleOptions(200025, CustomRoles.Oblivious, 1);
            SetupSingleRoleOptions(200035, CustomRoles.Flash, 1);
            FlashSpeed = CustomOption.Create(200305, Color.white, "SpeedBoosterUpSpeed", 2f, 0.25f, 3f, 0.25f, CustomRoleSpawnChances[CustomRoles.Flash]);
            SetupSingleRoleOptions(30100, CustomRoles.Sleuth, 1);
            SetupSingleRoleOptions(301859, CustomRoles.TieBreaker, 1);
            SetupSingleRoleOptions(30000, CustomRoles.Watcher, 1);
            //EvilWatcherChance = CustomOption.Create(30010, Color.white, "EvilWatcherChance", 0, 0, 100, 10, CustomRoleSpawnChances[CustomRoles.Watcher]);

            // Attribute
            ModifierRestrict = CustomOption.Create(1314, Color.white, "ModifierRestrict", true, null, true)
                .SetGameMode(CustomGameMode.Standard);
            ImpostorKnowsRolesOfTeam = CustomOption.Create(102000, Color.white, "ImpostorKnowsRolesOfTeam", true, null, true)
                .SetGameMode(CustomGameMode.Standard);
            CovenKnowsRolesOfTeam = CustomOption.Create(102300, Color.white, "CovenKnowsRolesOfTeam", true, null, true)
                .SetGameMode(CustomGameMode.Standard);
            GlobalRoleBlockDuration = CustomOption.Create(80009, Color.yellow, "GRB", 30, 2.5f, 180, 2.5f, null, true)
                .SetGameMode(CustomGameMode.Standard);
            EnableLastImpostor = CustomOption.Create(80010, Utils.GetRoleColor(CustomRoles.Impostor), "LastImpostor", false, null, true)
                .SetGameMode(CustomGameMode.Standard);
            LastImpostorKillCooldown = CustomOption.Create(80020, Color.white, "LastImpostorKillCooldown", 15, 0, 180, 1, EnableLastImpostor)
                .SetGameMode(CustomGameMode.Standard);
            #endregion

            // HideAndSeek
            SetupRoleOptions(100000, CustomRoles.HASFox, CustomGameMode.HideAndSeek);
            SetupRoleOptions(100100, CustomRoles.HASTroll, CustomGameMode.HideAndSeek);
            AllowCloseDoors = CustomOption.Create(101000, Color.white, "AllowCloseDoors", false, null, true)
                .SetGameMode(CustomGameMode.HideAndSeek);
            KillDelay = CustomOption.Create(101001, Color.white, "HideAndSeekWaitingTime", 10, 0, 180, 5)
                .SetGameMode(CustomGameMode.HideAndSeek);
            //IgnoreCosmetics = CustomOption.Create(101002, Color.white, "IgnoreCosmetics", false)
            //    .SetGameMode(CustomGameMode.HideAndSeek);
            IgnoreVent = CustomOption.Create(101003, Color.white, "IgnoreVent", false)
                .SetGameMode(CustomGameMode.HideAndSeek);

            FreeForAllOn = CustomOption.Create(1001009, Color.white, "FreeForAllOn", false)
                .SetGameMode(CustomGameMode.HideAndSeek);

            SplatoonOn = CustomOption.Create(1001008, Color.white, "Splatoon", false)
                .SetGameMode(CustomGameMode.HideAndSeek);
            SetupRoleOptions(100110, CustomRoles.Supporter, CustomGameMode.HideAndSeek);
            SetupRoleOptions(100111, CustomRoles.Janitor, CustomGameMode.HideAndSeek);
            STCD = CustomOption.Create(1001001, Color.white, "KillCDT", 25, 2.5f, 60, 2.5f)
                .SetGameMode(CustomGameMode.HideAndSeek);
            STIgnoreVent = CustomOption.Create(1001003, Color.white, "CanVent", false)
                .SetGameMode(CustomGameMode.HideAndSeek);
            PaintersHaveImpVision = CustomOption.Create(1001004, Color.white, "PaintersHaveImpVision", false)
            .SetGameMode(CustomGameMode.HideAndSeek);

            //#region ColorWars
            /*
            // COLOR WARS //

            // CWAllowCloseDoors = CustomOption.Create(1011000, Color.white, "AllowCloseDoors", false, null, true)
            //    .SetGameMode(CustomGameMode.ColorWars);
            SetupRoleOptions(1000000, CustomRoles.TeamBlue, CustomGameMode.ColorWars);
            SetupRoleOptions(1001000, CustomRoles.TeamGreen, CustomGameMode.ColorWars);
            SetupRoleOptions(1002000, CustomRoles.TeamRed, CustomGameMode.ColorWars);
            CWCD = CustomOption.Create(1011001, Color.white, "KillCD", 30, 2.5f, 60, 2.5f)
                .SetGameMode(CustomGameMode.ColorWars);
            CWIgnoreVent = CustomOption.Create(1011003, Color.white, "CanVent", false)
                .SetGameMode(CustomGameMode.ColorWars);
            //#endregion

            //#region Splatoon
            // SPLATOON //
            STCD = CustomOption.Create(1001001, Color.white, "KillCDT", 25, 2.5f, 60, 2.5f)
                .SetGameMode(CustomGameMode.Splatoon);
            STIgnoreVent = CustomOption.Create(1001003, Color.white, "CanVent", false)
                .SetGameMode(CustomGameMode.Splatoon);
            // #endregion

            // FFA //
            SetupSingleRoleOptions(50900, CustomRoles.Jackal, 15, CustomGameMode.FFA);
            JackalKillCooldown = CustomOption.Create(50910, Color.white, "JackalKillCooldown", 30, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Jackal])
                .SetGameMode(CustomGameMode.FFA);
            JackalCanVent = CustomOption.Create(50911, Color.white, "JackalCanVent", true, CustomRoleSpawnChances[CustomRoles.Jackal])
                .SetGameMode(CustomGameMode.FFA);
            JackalHasImpostorVision = CustomOption.Create(50913, Color.white, "JackalHasImpostorVision", true, CustomRoleSpawnChances[CustomRoles.Jackal])
                .SetGameMode(CustomGameMode.FFA);
                */

            //デバイス無効化
            DisableDevices = CustomOption.Create(101200, Color.white, "DisableDevices", false, null, true)
                .SetGameMode(CustomGameMode.Standard);
            DisableAdmin = CustomOption.Create(101210, Color.white, "DisableAdmin", false, DisableDevices)
                .SetGameMode(CustomGameMode.Standard);
            WhichDisableAdmin = CustomOption.Create(101211, Color.white, "WhichDisableAdmin", whichDisableAdmin, whichDisableAdmin[0], DisableAdmin)
                .SetGameMode(CustomGameMode.Standard);

            // ボタン回数同期
            SyncButtonMode = CustomOption.Create(100200, Color.white, "SyncButtonMode", false, null, true)
                .SetGameMode(CustomGameMode.Standard);
            SyncedButtonCount = CustomOption.Create(100201, Color.white, "SyncedButtonCount", 10, 0, 100, 1, SyncButtonMode)
                .SetGameMode(CustomGameMode.Standard);

            // リアクターの時間制御
            SabotageTimeControl = CustomOption.Create(100800, Color.white, "SabotageTimeControl", false, null, true)
                .SetGameMode(CustomGameMode.Standard);
            PolusReactorTimeLimit = CustomOption.Create(100801, Color.white, "PolusReactorTimeLimit", 30, 1, 60, 1, SabotageTimeControl)
                .SetGameMode(CustomGameMode.Standard);
            AirshipReactorTimeLimit = CustomOption.Create(100802, Color.white, "AirshipReactorTimeLimit", 60, 1, 90, 1, SabotageTimeControl)
                .SetGameMode(CustomGameMode.Standard);

            // タスク無効化
            Customise = CustomOption.Create(101900, Color.white, "Customise", true, null, true)
                .SetGameMode(CustomGameMode.All);
            RolesLikeToU = CustomOption.Create(102000, Color.white, "RolesLikeToU", false, null, true)
            .SetGameMode(CustomGameMode.All);
            DisableTasks = CustomOption.Create(100300, Color.white, "DisableTasks", false, null, true)
                .SetGameMode(CustomGameMode.All);
            DisableSwipeCard = CustomOption.Create(100301, Color.white, "DisableSwipeCardTask", false, DisableTasks)
                .SetGameMode(CustomGameMode.All);
            DisableSubmitScan = CustomOption.Create(100302, Color.white, "DisableSubmitScanTask", false, DisableTasks)
                .SetGameMode(CustomGameMode.All);
            DisableUnlockSafe = CustomOption.Create(100303, Color.white, "DisableUnlockSafeTask", false, DisableTasks)
                .SetGameMode(CustomGameMode.All);
            DisableUploadData = CustomOption.Create(100304, Color.white, "DisableUploadDataTask", false, DisableTasks)
                .SetGameMode(CustomGameMode.All);
            DisableStartReactor = CustomOption.Create(100305, Color.white, "DisableStartReactorTask", false, DisableTasks)
                .SetGameMode(CustomGameMode.All);
            DisableResetBreaker = CustomOption.Create(100306, Color.white, "DisableResetBreakerTask", false, DisableTasks)
                .SetGameMode(CustomGameMode.All);
            DisableFixWiring = CustomOption.Create(100307, Color.white, "DisableFixWiring", false, DisableTasks)
                .SetGameMode(CustomGameMode.All);

            // ランダムマップ
            RandomMapsMode = CustomOption.Create(100400, Color.white, "RandomMapsMode", false, null, true)
                .SetGameMode(CustomGameMode.All);
            AddedTheSkeld = CustomOption.Create(100401, Color.white, "AddedTheSkeld", false, RandomMapsMode)
                .SetGameMode(CustomGameMode.All);
            AddedMiraHQ = CustomOption.Create(100402, Color.white, "AddedMIRAHQ", false, RandomMapsMode)
                .SetGameMode(CustomGameMode.All);
            AddedPolus = CustomOption.Create(100403, Color.white, "AddedPolus", false, RandomMapsMode)
                .SetGameMode(CustomGameMode.All);
            AddedTheAirShip = CustomOption.Create(100404, Color.white, "AddedTheAirShip", false, RandomMapsMode)
                .SetGameMode(CustomGameMode.All);
            // MapDleks = CustomOption.Create(100405, Color.white, "AddedDleks", false, RandomMapMode)
            //     .SetGameMode(CustomGameMode.All);

            // 投票モード
            VoteMode = CustomOption.Create(100500, Color.white, "VoteMode", false, null, true)
                .SetGameMode(CustomGameMode.Standard);
            WhenSkipVote = CustomOption.Create(100501, Color.white, "WhenSkipVote", voteModes[0..3], voteModes[0], VoteMode)
                .SetGameMode(CustomGameMode.Standard);
            WhenNonVote = CustomOption.Create(100502, Color.white, "WhenNonVote", voteModes, voteModes[0], VoteMode)
                .SetGameMode(CustomGameMode.Standard);

            // 転落死
            LadderDeath = CustomOption.Create(101100, Color.white, "LadderDeath", false, null, true);
            LadderDeathChance = CustomOption.Create(101110, Color.white, "LadderDeathChance", rates[1..], rates[2], LadderDeath);

            // 通常モードでかくれんぼ用
            StandardHAS = CustomOption.Create(100700, Color.white, "StandardHAS", false, null, true)
                .SetGameMode(CustomGameMode.Standard);
            StandardHASWaitingTime = CustomOption.Create(100701, Color.white, "StandardHASWaitingTime", 10f, 0f, 180f, 2.5f, StandardHAS)
                .SetGameMode(CustomGameMode.Standard);

            MinNK = CustomOption.Create(1007012, Color.white, "MinNK", 0, 0, 15, 1, null, true)
                .SetGameMode(CustomGameMode.Standard);
            MaxNK = CustomOption.Create(1007013, Color.white, "MaxNK", 0, 0, 15, 1, null, true)
            .SetGameMode(CustomGameMode.Standard);
            MinNonNK = CustomOption.Create(1007014, Color.white, "MinNonNK", 0, 0, 15, 1, null, true)
            .SetGameMode(CustomGameMode.Standard);
            MaxNonNK = CustomOption.Create(1007015, Color.white, "MaxNonNK", 0, 0, 15, 1, null, true)
                .SetGameMode(CustomGameMode.Standard);

            // その他
            CamoComms = CustomOption.Create(100607, Color.white, "CamoComms", false, null, true)
                .SetGameMode(CustomGameMode.All);
            NoGameEnd = CustomOption.Create(100600, Color.white, "NoGameEnd", false, null, true)
                .SetGameMode(CustomGameMode.All);
            AutoDisplayLastResult = CustomOption.Create(100601, Color.white, "AutoDisplayLastResult", false)
                .SetGameMode(CustomGameMode.All);
            SuffixMode = CustomOption.Create(100602, Color.white, "SuffixMode", suffixModes, suffixModes[0])
                .SetGameMode(CustomGameMode.All);
            ColorNameMode = CustomOption.Create(100605, Color.white, "ColorNameMode", false)
                .SetGameMode(CustomGameMode.All);
            GhostCanSeeOtherRoles = CustomOption.Create(100603, Color.white, "GhostCanSeeOtherRoles", true)
                .SetGameMode(CustomGameMode.All);
            GhostCanSeeOtherVotes = CustomOption.Create(100604, Color.white, "GhostCanSeeOtherVotes", true)
                .SetGameMode(CustomGameMode.All);
            HideGameSettings = CustomOption.Create(100606, Color.white, "HideGameSettings", false)
                .SetGameMode(CustomGameMode.All);

            IsLoaded = true;
        }

        public static void SetupRoleOptions(int id, CustomRoles role, CustomGameMode customGameMode = CustomGameMode.Standard)
        {
            var spawnOption = CustomOption.Create(id, Utils.GetRoleColor(role), role.ToString(), rates, rates[0], null, true)
                .HiddenOnDisplay(true)
                .SetGameMode(customGameMode);
            var countOption = CustomOption.Create(id + 1, Color.white, "Maximum", 1, 1, 15, 1, spawnOption, false)
                .HiddenOnDisplay(true)
                .SetGameMode(customGameMode);

            CustomRoleSpawnChances.Add(role, spawnOption);
            CustomRoleCounts.Add(role, countOption);
        }
        private static void SetupLoversRoleOptionsToggle(int id, CustomGameMode customGameMode = CustomGameMode.Standard)
        {
            var role = CustomRoles.LoversRecode
            ;
            var spawnOption = CustomOption.Create(id, Utils.GetRoleColor(role), role.ToString(), rates, rates[0], null, true)
                .HiddenOnDisplay(true)
                .SetGameMode(customGameMode);

            var countOption = CustomOption.Create(id + 1, Color.white, "NumberOfLovers", 2, 1, 15, 1, spawnOption, false, true)
                .HiddenOnDisplay(false)
                .SetGameMode(customGameMode);

            CustomRoleSpawnChances.Add(role, spawnOption);
            CustomRoleCounts.Add(role, countOption);
        }
        public static void SetupSingleRoleOptions(int id, CustomRoles role, int count, CustomGameMode customGameMode = CustomGameMode.Standard)
        {
            var spawnOption = CustomOption.Create(id, Utils.GetRoleColor(role), role.ToString(), rates, rates[0], null, true)
                .HiddenOnDisplay(true)
                .SetGameMode(customGameMode);
            // 初期値,最大値,最小値が同じで、stepが0のどうやっても変えることができない個数オプション
            var countOption = CustomOption.Create(id + 1, Color.white, "Maximum", count, count, count, count, spawnOption, false, true)
                .HiddenOnDisplay(false)
                .SetGameMode(customGameMode);

            CustomRoleSpawnChances.Add(role, spawnOption);
            CustomRoleCounts.Add(role, countOption);
        }
        public class OverrideTasksData
        {
            public static Dictionary<CustomRoles, OverrideTasksData> AllData = new();
            public CustomRoles Role { get; private set; }
            public int IdStart { get; private set; }
            public CustomOption doOverride;
            public CustomOption assignCommonTasks;
            public CustomOption numLongTasks;
            public CustomOption numShortTasks;

            public OverrideTasksData(int idStart, CustomRoles role)
            {
                this.IdStart = idStart;
                this.Role = role;
                Dictionary<string, string> replacementDic = new() { { "%role%", Utils.GetRoleName(role) } };
                doOverride = CustomOption.Create(idStart++, Color.white, "doOverride", false, CustomRoleSpawnChances[role], false, false, "", replacementDic);
                assignCommonTasks = CustomOption.Create(idStart++, Color.white, "assignCommonTasks", true, doOverride, false, false, "", replacementDic);
                numLongTasks = CustomOption.Create(idStart++, Color.white, "roleLongTasksNum", 3, 0, 99, 1, doOverride, false, false, "", replacementDic);
                numShortTasks = CustomOption.Create(idStart++, Color.white, "roleShortTasksNum", 3, 0, 99, 1, doOverride, false, false, "", replacementDic);

                if (!AllData.ContainsKey(role)) AllData.Add(role, this);
                else Logger.Warn("重複したCustomRolesを対象とするOverrideTasksDataが作成されました", "OverrideTasksData");
            }
            public static OverrideTasksData Create(int idStart, CustomRoles role)
            {
                return new OverrideTasksData(idStart, role);
            }
        }
    }
}
