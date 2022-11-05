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
        //public static CustomOption EnableGM;
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
        public static CustomOption MediumCooldown;

        // HideAndSeek
        public static CustomOption AllowCloseDoors;
        public static CustomOption KillDelay;
        public static CustomOption IgnoreCosmetics;
        public static CustomOption IgnoreVent;
        public static CustomOption FlashDuration;
        public static CustomOption FlashCooldown;
        public static CustomOption GrenadierCanVent;
        public static float HideAndSeekKillDelayTimer = 0f;
        // arrow //
        public static CustomOption VultureArrow;
        public static CustomOption MediumArrow;
        public static CustomOption AmnesiacArrow;

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
        public static OverrideTasksData CrewPostorTasks;

        // その他
        public static CustomOption NoGameEnd;
        public static CustomOption CamoComms;
        public static CustomOption AutoDisplayLastResult;
        public static CustomOption SuffixMode;
        public static CustomOption ColorNameMode;
        public static CustomOption GhostCanSeeOtherRoles;
        public static CustomOption GhostCanSeeOtherVotes;
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

        // DISPERSER //
        public static CustomOption DisperseCooldown;

        // SWOOPER //
        public static CustomOption SwooperDuration;
        public static CustomOption SwooperCooldown;
        public static CustomOption SwooperCanVentInvis;

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
        // OTHER STUFF //
        public static CustomOption SaboAmount;
        public static CustomOption DemoSuicideTime;
        public static CustomOption KillFlashDuration;

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
        public static OverrideTasksData PhantomTaskOverride;
        // YIN YANGER or COLLIDER //
        public static CustomOption YinYangCooldown;
        public static CustomOption ResetToYinYang;
        // TRANSPORTER //
        public static CustomOption NumOfTransports;
        public static CustomOption TransportCooldown;
        // FREEZER //
        public static CustomOption FreezerDuration;
        public static CustomOption FreezerCooldown;
        // HITMAN //
        public static CustomOption HitmanKillCooldown;
        public static CustomOption HitmanCanVent;
        public static CustomOption HitmanHasImpVision;
        public static CustomOption HitmanCanWinWithExeJes;
        // MAYOR //
        public static CustomOption MayorVotesAppearBlack;
        public static CustomOption TOuRMayor;
        public static CustomOption MayorInitialVoteBank;
        // Escort //
        public static CustomOption EscortCooldown;
        public static CustomOption EscortPreventsVent;
        // Crusader //
        public static CustomOption CrusadeCooldown;
        // NEUTRAL WITCH //
        public static CustomOption ControlCooldown;
        public static CustomOption NumOfWitchesPerRound;
        // OTHER //
        public static CustomOption CheckRoleTwiceBeforeAdd;
        public static readonly string[] suffixModes =
        {
            "SuffixMode.None",
            "SuffixMode.Version",
            "SuffixMode.Streaming",
            "SuffixMode.Recording",
            //"SuffixMode.Dev"
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

        public static CustomOption DisableTaskWin;
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
            _ = CustomOption.Create(0, new Color(204f / 255f, 204f / 255f, 0, 1f), "Preset", AmongUsExtensions.OptionType.GameOption, presets, presets[0], null, true)
                .HiddenOnDisplay(true)
                .SetGameMode(CustomGameMode.All);

            // ゲームモード
            GameMode = CustomOption.Create(1, new Color(204f / 255f, 204f / 255f, 0, 1f), "GameMode", AmongUsExtensions.OptionType.GameOption, gameModes, gameModes[0], null, true)
                .SetGameMode(CustomGameMode.All);

            #region 役職・詳細設定
            CustomRoleCounts = new Dictionary<CustomRoles, CustomOption>();
            CustomRoleSpawnChances = new Dictionary<CustomRoles, CustomOption>();
            // GM
            //EnableGM = CustomOption.Create(100, Utils.GetRoleColor(CustomRoles.GM), "GM", false, null, true)
            //     .SetGameMode(CustomGameMode.Standard);
            //LaptopPercentages = CustomOption.Create(100, Color.white, "LaptopPercentages", AmongUsExtensions.OptionType.GameOption, false, null, true);

            // Impostor
            //BountyHunter
            BountyHunter.SetupCustomOption();
            //CamouFlager
            Camouflager.SetupCustomOption();
            //Cleaner
            SetupRoleOptions(2007424234, CustomRoles.Cleaner, AmongUsExtensions.OptionType.Impostor);
            //Consort
            SetupRoleOptions(23473, CustomRoles.Consort, AmongUsExtensions.OptionType.Impostor);
            //Disperser
            SetupRoleOptions(2007424235, CustomRoles.Disperser, AmongUsExtensions.OptionType.Impostor);
            DisperseCooldown = CustomOption.Create(200126, Color.white, "DisperseCooldown", AmongUsExtensions.OptionType.Impostor, 30, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Disperser]);
            //FireWorks
            FireWorks.SetupCustomOption();
            //Freezer
            SetupSingleRoleOptions(2010, CustomRoles.Freezer, 1, AmongUsExtensions.OptionType.Impostor);
            FreezerCooldown = CustomOption.Create(200125, Color.white, "FreezerCooldown", AmongUsExtensions.OptionType.Impostor, 20, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Freezer]);
            FreezerDuration = CustomOption.Create(200135, Color.white, "FreezerDuration", AmongUsExtensions.OptionType.Impostor, 10, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Freezer]);
            //Grenadier
            SetupSingleRoleOptions(9999, CustomRoles.Grenadier, 1, AmongUsExtensions.OptionType.Impostor);
            FlashCooldown = CustomOption.Create(200129, Color.white, "FlashCD", AmongUsExtensions.OptionType.Impostor, 30, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Grenadier]);
            FlashDuration = CustomOption.Create(2001299, Color.white, "FlashDur", AmongUsExtensions.OptionType.Impostor, 15, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Grenadier]);
            GrenadierCanVent = CustomOption.Create(1312, Color.white, "GrenadierCanVent", AmongUsExtensions.OptionType.Impostor, true, CustomRoleSpawnChances[CustomRoles.Grenadier]);
            //Mafia
            SetupRoleOptions(1600, CustomRoles.Mafia, AmongUsExtensions.OptionType.Impostor);
            //Mare
            Mare.SetupCustomOption();
            //Miner
            SetupRoleOptions(2008234234, CustomRoles.Miner, AmongUsExtensions.OptionType.Impostor);
            //Morphling
            SetupRoleOptions(1301, CustomRoles.Morphling, AmongUsExtensions.OptionType.Impostor);
            //Ninja
            Ninja.SetupCustomOption();
            //Puppeteer
            SetupRoleOptions(2000, CustomRoles.Puppeteer, AmongUsExtensions.OptionType.Impostor);
            //VoteStealer PickPocket
            SetupRoleOptions(20094334, CustomRoles.VoteStealer, AmongUsExtensions.OptionType.Impostor);
            //ShapeMaster
            // SetupRoleOptions(1200, CustomRoles.ShapeMaster);
            // ShapeMasterShapeshiftDuration = CustomOption.Create(1210, Color.white, "ShapeMasterShapeshiftDuration", 10, 1, 1000, 1, CustomRoleSpawnChances[CustomRoles.ShapeMaster]);
            //SerialKiller
            SerialKiller.SetupCustomOption();//Silencer
            SetupSingleRoleOptions(2609, CustomRoles.Silencer, 1, AmongUsExtensions.OptionType.Impostor);
            SilenceCooldown = CustomOption.Create(2610, Color.white, "SilenceDelay", AmongUsExtensions.OptionType.Impostor, 30, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Silencer]);
            //Sniper
            Sniper.SetupCustomOption();
            //Swooper
            // SetupSingleRoleOptions(2600, CustomRoles.Swooper, 1, AmongUsExtensions.OptionType.Impostor);
            // SwooperDuration = CustomOption.Create(260010, Color.white, "SwooperDuration", AmongUsExtensions.OptionType.Impostor, 30f, 2.5f, 60f, 2.5f, CustomRoleSpawnChances[CustomRoles.Swooper]);
            // SwooperCooldown = CustomOption.Create(260011, Color.white, "SwooperCooldown", AmongUsExtensions.OptionType.Impostor, 15f, 2.5f, 60f, 2.5f, CustomRoleSpawnChances[CustomRoles.Swooper]);
            // SwooperCanVentInvis = CustomOption.Create(260012, Color.white, "SwooperCanVentInvis", AmongUsExtensions.OptionType.Impostor, true, CustomRoleSpawnChances[CustomRoles.Swooper]);
            //Traitor
            SetupSingleRoleOptions(22434, CustomRoles.CorruptedSheriff, 1, AmongUsExtensions.OptionType.Impostor);
            PlayersForTraitor = CustomOption.Create(2040030, Color.white, "TraitorSpawn", AmongUsExtensions.OptionType.Impostor, 1, 0, 15, 1, CustomRoleSpawnChances[CustomRoles.CorruptedSheriff]);
            TraitorCanSpawnIfNK = CustomOption.Create(2040031, Color.white, "TraitorCanSpawnIfNK", AmongUsExtensions.OptionType.Impostor, true, CustomRoleSpawnChances[CustomRoles.CorruptedSheriff]);
            TraitorCanSpawnIfCoven = CustomOption.Create(2040032, Color.white, "TraitorCanSpawnIfCoven", AmongUsExtensions.OptionType.Impostor, true, CustomRoleSpawnChances[CustomRoles.CorruptedSheriff]);
            //PlayersForTraitor = CustomOption.Create(2710, Color.white, "TraitorSpawn", 1, 0, 15, 1, CustomRoleSpawnChances[CustomRoles.CorruptedSheriff]);
            //TimeThief
            TimeThief.SetupCustomOption();
            //Vampire
            SetupRoleOptions(1300, CustomRoles.Vampire, AmongUsExtensions.OptionType.Impostor);
            VampireKillDelay = CustomOption.Create(1310, Color.white, "VampireKillDelay", AmongUsExtensions.OptionType.Impostor, 5, 1, 1000, 1, CustomRoleSpawnChances[CustomRoles.Vampire]);
            VampireBuff = CustomOption.Create(1311, Color.white, "VampBuff", AmongUsExtensions.OptionType.Impostor, true, CustomRoleSpawnChances[CustomRoles.Vampire]);
            //Warlock
            SetupRoleOptions(1400, CustomRoles.Warlock, AmongUsExtensions.OptionType.Impostor);
            //Witch ImpWitch
            SetupRoleOptions(1500, CustomRoles.Witch, AmongUsExtensions.OptionType.Impostor);
            //YingYanger
            SetupSingleRoleOptions(200099, CustomRoles.YingYanger, 1, AmongUsExtensions.OptionType.Impostor);
            YinYangCooldown = CustomOption.Create(1213, Color.white, "YinYangCooldown", AmongUsExtensions.OptionType.Impostor, 15, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.YingYanger]);
            ResetToYinYang = CustomOption.Create(1314, Color.white, "ResetToYinYang", AmongUsExtensions.OptionType.Impostor, true, CustomRoleSpawnChances[CustomRoles.YingYanger]);


            DefaultShapeshiftCooldown = CustomOption.Create(5011, Color.white, "DefaultShapeshiftCooldown", AmongUsExtensions.OptionType.GameOption, 15, 5, 999, 5, null, true);
            CanMakeMadmateCount = CustomOption.Create(5012, Color.white, "CanMakeMadmateCount", AmongUsExtensions.OptionType.GameOption, 0, 0, 15, 1, null, true);

            // Madmates
            //dMadMates = CustomOption.Create(51511, Color.red, "Mad Mates", AmongUsExtensions.OptionType.Roles, "", "", null, true, false, "Mad Mates");
            //MadGuardian
            SetupRoleOptions(10100, CustomRoles.MadGuardian, AmongUsExtensions.OptionType.Impostor);
            MadGuardianCanSeeWhoTriedToKill = CustomOption.Create(10110, Color.white, "MadGuardianCanSeeWhoTriedToKill", AmongUsExtensions.OptionType.Impostor, false, CustomRoleSpawnChances[CustomRoles.MadGuardian]);
            //ID10120~10123を使用
            MadGuardianTasks = OverrideTasksData.Create(10120, CustomRoles.MadGuardian, AmongUsExtensions.OptionType.Impostor);
            // Madmate Common Options
            SetupRoleOptions(10000, CustomRoles.Madmate, AmongUsExtensions.OptionType.Impostor);
            MadmateCanFixLightsOut = CustomOption.Create(15010, Color.white, "MadmateCanFixLightsOut", AmongUsExtensions.OptionType.GameOption, false, null, true, false);
            MadmateCanFixComms = CustomOption.Create(15011, Color.white, "MadmateCanFixComms", AmongUsExtensions.OptionType.GameOption, false);
            MadmateHasImpostorVision = CustomOption.Create(15012, Color.white, "MadmateHasImpostorVision", AmongUsExtensions.OptionType.GameOption, false);
            MadmateVentCooldown = CustomOption.Create(15213, Color.white, "MadmateVentCooldown", AmongUsExtensions.OptionType.GameOption, 0f, 0f, 180f, 5f);
            MadmateVentMaxTime = CustomOption.Create(15214, Color.white, "MadmateVentMaxTime", AmongUsExtensions.OptionType.GameOption, 0f, 0f, 180f, 5f);
            //MadSnitch
            SetupRoleOptions(10200, CustomRoles.MadSnitch, AmongUsExtensions.OptionType.Impostor);
            MadSnitchCanVent = CustomOption.Create(10210, Color.white, "MadSnitchCanVent", AmongUsExtensions.OptionType.Impostor, false, CustomRoleSpawnChances[CustomRoles.MadSnitch]);
            //ID10220~10223を使用
            MadSnitchTasks = OverrideTasksData.Create(10220, CustomRoles.MadSnitch, AmongUsExtensions.OptionType.Impostor);
            SetupSingleRoleOptions(10333, CustomRoles.Parasite, 1, AmongUsExtensions.OptionType.Impostor);
            // Both
            //SetupRoleOptions(30000, CustomRoles.Watcher);
            //EvilWatcherChance = CustomOption.Create(30010, Color.white, "EvilWatcherChance", 0, 0, 100, 10, CustomRoleSpawnChances[CustomRoles.Watcher]);
            //Guesser Assassin, Pirate, Vigilante
            Guesser.SetupCustomOption();
            //Coven
            //SetupSingleRoleOptions(383012, CustomRoles.PoisonMaster, 1, AmongUsExtensions.OptionType.Neutral);
            //SetupSingleRoleOptions(60000, CustomRoles.Coven, 3, AmongUsExtensions.OptionType.Neutral);
            SetUpCovenOptions(60000);
            CovenKillCooldown = CustomOption.Create(60020, Color.white, "CovenKillCooldown", AmongUsExtensions.OptionType.Neutral, 30, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Coven]);
            CovenMeetings = CustomOption.Create(60021, Color.white, "CovenMeetings", AmongUsExtensions.OptionType.Neutral, 3, 0, 15, 1, CustomRoleSpawnChances[CustomRoles.Coven]);
            HexMasterOn = CustomOption.Create(60022, Color.white, "HexMasterOn", AmongUsExtensions.OptionType.Neutral, false, CustomRoleSpawnChances[CustomRoles.Coven]);

            HexCD = CustomOption.Create(60028, Color.white, "HexCD", AmongUsExtensions.OptionType.Neutral, 30, 2.5f, 180, 2.5f, HexMasterOn);
            PKTAH = CustomOption.Create(60029, Color.white, "PKTAH", AmongUsExtensions.OptionType.Neutral, true, HexMasterOn);
            MaxHexesPerRound = CustomOption.Create(60030, Color.white, "MHPR", AmongUsExtensions.OptionType.Neutral, 3, 1, 15, 1, HexMasterOn);

            //PotionMasterOn = CustomOption.Create(60013, Color.white, "PotionMasterOn", false, CustomRoleSpawnChances[CustomRoles.Coven]);
            VampireDitchesOn = CustomOption.Create(60014, Color.white, "VampireDitchesOn", AmongUsExtensions.OptionType.Neutral, false, CustomRoleSpawnChances[CustomRoles.Coven]);

            MedusaOn = CustomOption.Create(60015, Color.white, "MedusaOn", AmongUsExtensions.OptionType.Neutral, false, CustomRoleSpawnChances[CustomRoles.Coven]);
            StoneCD = CustomOption.Create(60025, Color.white, "StoneCD", AmongUsExtensions.OptionType.Neutral, 30, 2.5f, 180, 2.5f, MedusaOn);
            StoneDuration = CustomOption.Create(60026, Color.white, "StoneDur", AmongUsExtensions.OptionType.Neutral, 15, 2.5f, 180, 2.5f, MedusaOn);
            StoneReport = CustomOption.Create(60027, Color.white, "StoneTime", AmongUsExtensions.OptionType.Neutral, 35, 2.5f, 180, 2.5f, MedusaOn);

            //MimicOn = CustomOption.Create(60016, Color.white, "MimicOn", false, CustomRoleSpawnChances[CustomRoles.Coven]);
            //NecromancerOn = CustomOption.Create(60017, Color.white, "NecromancerOn", false, CustomRoleSpawnChances[CustomRoles.Coven]);
            // NecroCanUseSheriff = CustomOption.Create(60019, Color.white, "NecroCanUseSheriff", false, NecromancerOn);
            //ConjurorOn = CustomOption.Create(60018, Color.white, "ConjurorOn", false, CustomRoleSpawnChances[CustomRoles.Coven]);
            //NumOfCoven = CustomOption.Create(60010, Color.white, "ArsonistDouseTime", 3, 1, 3, 1, CustomRoleSpawnChances[CustomRoles.Coven]);

            //Neutral Killers
            //Arsonist
            SetupSingleRoleOptions(50500, CustomRoles.Arsonist, 1, AmongUsExtensions.OptionType.Neutral);
            ArsonistDouseTime = CustomOption.Create(50510, Color.white, "ArsonistDouseTime", AmongUsExtensions.OptionType.Neutral, 3, 0, 10, 1, CustomRoleSpawnChances[CustomRoles.Arsonist]);
            ArsonistCooldown = CustomOption.Create(50511, Color.white, "ArsonistCooldown", AmongUsExtensions.OptionType.Neutral, 10, 5, 100, 1, CustomRoleSpawnChances[CustomRoles.Arsonist]);
            TOuRArso = CustomOption.Create(50512, Color.white, "TourArso", AmongUsExtensions.OptionType.Neutral, false, CustomRoleSpawnChances[CustomRoles.Arsonist]);
            //BloodKnight
            SetupSingleRoleOptions(509000, CustomRoles.BloodKnight, 1, AmongUsExtensions.OptionType.Neutral);
            BKcanVent = CustomOption.Create(09005, Color.white, "CanVent", AmongUsExtensions.OptionType.Neutral, true, CustomRoleSpawnChances[CustomRoles.BloodKnight]);
            BKkillCd = CustomOption.Create(509012, Color.white, "KillCD", AmongUsExtensions.OptionType.Neutral, 30, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.BloodKnight]);
            BKprotectDur = CustomOption.Create(509011, Color.white, "BKdur", AmongUsExtensions.OptionType.Neutral, 15, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.BloodKnight]);
            //CrewPostor
            SetupRoleOptions(205000, CustomRoles.CrewPostor, AmongUsExtensions.OptionType.Impostor);
            CrewPostorTasks = OverrideTasksData.Create(10121, CustomRoles.CrewPostor, AmongUsExtensions.OptionType.Neutral);
            //Egoist
            Egoist.SetupCustomOption();

            //TheGlitch
            SetupSingleRoleOptions(80500, CustomRoles.TheGlitch, 1, AmongUsExtensions.OptionType.Neutral);
            GlitchRoleBlockCooldown = CustomOption.Create(80510, Color.white, "RBC", AmongUsExtensions.OptionType.Neutral, 20, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.TheGlitch]);
            GlitchKillCooldown = CustomOption.Create(80511, Color.white, "KillCD", AmongUsExtensions.OptionType.Neutral, 30, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.TheGlitch]);
            GlitchCanVent = CustomOption.Create(80512, Color.white, "HPV", AmongUsExtensions.OptionType.Neutral, true, CustomRoleSpawnChances[CustomRoles.TheGlitch]);
            //Jackal
            SetupSingleRoleOptions(50900, CustomRoles.Jackal, 1, AmongUsExtensions.OptionType.Neutral);
            JackalKillCooldown = CustomOption.Create(50910, Color.white, "JackalKillCooldown", AmongUsExtensions.OptionType.Neutral, 30, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Jackal]);
            JackalCanVent = CustomOption.Create(50911, Color.white, "JackalCanVent", AmongUsExtensions.OptionType.Neutral, true, CustomRoleSpawnChances[CustomRoles.Jackal]);
            JackalCanUseSabotage = CustomOption.Create(50912, Color.white, "JackalCanUseSabotage", AmongUsExtensions.OptionType.Neutral, false, CustomRoleSpawnChances[CustomRoles.Jackal]);
            JackalHasImpostorVision = CustomOption.Create(50913, Color.white, "JackalHasImpostorVision", AmongUsExtensions.OptionType.Neutral, true, CustomRoleSpawnChances[CustomRoles.Jackal]);

            JackalHasSidekick = CustomOption.Create(50914, Color.white, "JackalHasSidekick", AmongUsExtensions.OptionType.Neutral, false, CustomRoleSpawnChances[CustomRoles.Jackal]);
            SidekickCanKill = CustomOption.Create(50915, Color.white, "SidekickCanKill", AmongUsExtensions.OptionType.Neutral, false, JackalHasSidekick);
            SidekickGetsPromoted = CustomOption.Create(50916, Color.white, "SidekickGetsPromoted", AmongUsExtensions.OptionType.Neutral, true, JackalHasSidekick);
            //Juggernaut
            SetupSingleRoleOptions(70000, CustomRoles.Juggernaut, 1, AmongUsExtensions.OptionType.Neutral);
            JuggerKillCooldown = CustomOption.Create(60010, Color.white, "JuggerKillCooldown", AmongUsExtensions.OptionType.Neutral, 40, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Juggernaut]);
            JuggerDecrease = CustomOption.Create(60011, Color.white, "JuggerDecrease", AmongUsExtensions.OptionType.Neutral, 5, 2.5f, 60, 2.5f, CustomRoleSpawnChances[CustomRoles.Juggernaut]);
            JuggerCanVent = CustomOption.Create(60012, Color.white, "JuggerCanVent", AmongUsExtensions.OptionType.Neutral, true, CustomRoleSpawnChances[CustomRoles.Juggernaut]);
            //Marksman
            SetupSingleRoleOptions(70001, CustomRoles.Marksman, 1, AmongUsExtensions.OptionType.Neutral);
            MarksmanKillCooldown = CustomOption.Create(600110, Color.white, "MarksmanKillCooldown", AmongUsExtensions.OptionType.Neutral, 30, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Marksman]);
            MarksmanCanVent = CustomOption.Create(60032, Color.white, "MarksmanCanVent", AmongUsExtensions.OptionType.Neutral, true, CustomRoleSpawnChances[CustomRoles.Marksman]);
            //Plaguebearer Pestilence
            SetupSingleRoleOptions(50550, CustomRoles.PlagueBearer, 1, AmongUsExtensions.OptionType.Neutral);
            InfectCooldown = CustomOption.Create(50560, Color.white, "InfectCD", AmongUsExtensions.OptionType.Neutral, 20, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.PlagueBearer]);
            PestilKillCooldown = CustomOption.Create(50561, Color.white, "PestiKillCooldown", AmongUsExtensions.OptionType.Neutral, 30, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.PlagueBearer]);
            PestiCanVent = CustomOption.Create(50562, Color.white, "PestiCanVent", AmongUsExtensions.OptionType.Neutral, true, CustomRoleSpawnChances[CustomRoles.PlagueBearer]);
            InfectionSkip = CustomOption.Create(50563, Color.white, "SkipInfect", AmongUsExtensions.OptionType.Neutral, false, CustomRoleSpawnChances[CustomRoles.PlagueBearer]);
            //Werewolf
            SetupSingleRoleOptions(90000, CustomRoles.Werewolf, 1, AmongUsExtensions.OptionType.Neutral);
            RampageCD = CustomOption.Create(90010, Color.white, "RCD", AmongUsExtensions.OptionType.Neutral, 30, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Werewolf]);
            RampageDur = CustomOption.Create(90020, Color.white, "RDur", AmongUsExtensions.OptionType.Neutral, 25, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Werewolf]);
            WWkillCD = CustomOption.Create(90030, Color.white, "KillCD", AmongUsExtensions.OptionType.Neutral, 3, 1, 30, 1, CustomRoleSpawnChances[CustomRoles.Werewolf]);
            VentWhileRampaged = CustomOption.Create(90040, Color.white, "CanVentR", AmongUsExtensions.OptionType.Neutral, true, CustomRoleSpawnChances[CustomRoles.Werewolf]);
            //Neutral Witch NeutWitch
            SetupSingleRoleOptions(50918, CustomRoles.NeutWitch, 1, AmongUsExtensions.OptionType.Neutral);
            ControlCooldown = CustomOption.Create(50917, Color.white, "ControlCooldown", AmongUsExtensions.OptionType.Neutral, 30, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.NeutWitch]);
            NumOfWitchesPerRound = CustomOption.Create(60031, Color.white, "NumOfWitchesPerRound", AmongUsExtensions.OptionType.Neutral, 3, 1, 15, 1, CustomRoleSpawnChances[CustomRoles.NeutWitch]);
            //Neutral
            //Amnesiac
            SetupSingleRoleOptions(905003, CustomRoles.Amnesiac, 1, AmongUsExtensions.OptionType.Neutral);
            AmnesiacArrow = CustomOption.Create(6000020, Color.white, "AmnesiacHasArrow", AmongUsExtensions.OptionType.Neutral, false, CustomRoleSpawnChances[CustomRoles.Amnesiac]);
            //Executioner
            SetupRoleOptions(50700, CustomRoles.Executioner, AmongUsExtensions.OptionType.Neutral);
            ExecutionerCanTargetImpostor = CustomOption.Create(50710, Color.white, "ExecutionerCanTargetImpostor", AmongUsExtensions.OptionType.Neutral, false, CustomRoleSpawnChances[CustomRoles.Executioner]);
            ExecutionerChangeRolesAfterTargetKilled = CustomOption.Create(50711, Color.white, "ExecutionerChangeRolesAfterTargetKilled", AmongUsExtensions.OptionType.Neutral, ExecutionerChangeRoles, ExecutionerChangeRoles[1], CustomRoleSpawnChances[CustomRoles.Executioner]);
            //GuardianAngelTOU
            SetupSingleRoleOptions(90500, CustomRoles.GuardianAngelTOU, 1, AmongUsExtensions.OptionType.Neutral);
            NumOfProtects = CustomOption.Create(905010, Color.white, "NProtects", AmongUsExtensions.OptionType.Neutral, 15, 1, 15, 1, CustomRoleSpawnChances[CustomRoles.GuardianAngelTOU]);
            GuardCD = CustomOption.Create(90511, Color.white, "PCD", AmongUsExtensions.OptionType.Neutral, 30, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.GuardianAngelTOU]);
            GuardDur = CustomOption.Create(90512, Color.white, "PDur", AmongUsExtensions.OptionType.Neutral, 30, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.GuardianAngelTOU]);
            GAknowsRole = CustomOption.Create(90513, Color.white, "GAKR", AmongUsExtensions.OptionType.Neutral, true, CustomRoleSpawnChances[CustomRoles.GuardianAngelTOU]);
            TargetKnowsGA = CustomOption.Create(90514, Color.white, "TKGA", AmongUsExtensions.OptionType.Neutral, true, CustomRoleSpawnChances[CustomRoles.GuardianAngelTOU]);
            WhenGaTargetDies = CustomOption.Create(90515, Color.white, "WhenGAdies", AmongUsExtensions.OptionType.Neutral, GAChangeRoles, GAChangeRoles[2], CustomRoleSpawnChances[CustomRoles.GuardianAngelTOU]);
            //Hacker
            SetupRoleOptions(50250, CustomRoles.Hacker, AmongUsExtensions.OptionType.Neutral);
            SaboAmount = CustomOption.Create(50260, Color.white, "SA", AmongUsExtensions.OptionType.Neutral, 20, 10, 99, 1, CustomRoleSpawnChances[CustomRoles.Hacker]);
            //Hitman
            SetupSingleRoleOptions(509009, CustomRoles.Hitman, 1, AmongUsExtensions.OptionType.Neutral);
            //HitmanKillCooldown = CustomOption.Create(509108, Color.white, "HitmanKillCooldown", AmongUsExtensions.OptionType.Neutral, 30, 5, 60, 2.5f, CustomRoleSpawnChances[CustomRoles.Hitman]);
            HitmanCanVent = CustomOption.Create(509119, Color.white, "HitmanCanVent", AmongUsExtensions.OptionType.Neutral, true, CustomRoleSpawnChances[CustomRoles.Hitman]);
            HitmanHasImpVision = CustomOption.Create(509129, Color.white, "HitmanHasImpVision", AmongUsExtensions.OptionType.Neutral, false, CustomRoleSpawnChances[CustomRoles.Hitman]);
            HitmanCanWinWithExeJes = CustomOption.Create(509139, Color.white, "HitmanCanWinWithExeJes", AmongUsExtensions.OptionType.Neutral, false, CustomRoleSpawnChances[CustomRoles.Hitman]);
            //Jester
            SetupRoleOptions(50000, CustomRoles.Jester, AmongUsExtensions.OptionType.Neutral);
            JesterCanVent = CustomOption.Create(50010, Color.white, "JesterVent", AmongUsExtensions.OptionType.Neutral, false, CustomRoleSpawnChances[CustomRoles.Jester]);
            JesterHasImpostorVision = CustomOption.Create(6000013, Color.white, "JesterHasImpostorVision", AmongUsExtensions.OptionType.Neutral, false, CustomRoleSpawnChances[CustomRoles.Jester]);
            //Phantom
            SetupSingleRoleOptions(905004, CustomRoles.Phantom, 1, AmongUsExtensions.OptionType.Neutral);
            TasksRemainingForPhantomClicked = CustomOption.Create(50515, Color.white, "TasksRemainingForPhantomClicked", AmongUsExtensions.OptionType.Neutral, 3, 1, 10, 1, CustomRoleSpawnChances[CustomRoles.Phantom]);
            TasksRemaningForPhantomAlert = CustomOption.Create(50516, Color.white, "TasksRemaningForPhantomAlert", AmongUsExtensions.OptionType.Neutral, 1, 1, 5, 1, CustomRoleSpawnChances[CustomRoles.Phantom]);
            PhantomTaskOverride = OverrideTasksData.Create(3782387, CustomRoles.Phantom, AmongUsExtensions.OptionType.Neutral);
            //SchrodingerCat
            SetupRoleOptions(50400, CustomRoles.SchrodingerCat, AmongUsExtensions.OptionType.Neutral);
            CanBeforeSchrodingerCatWinTheCrewmate = CustomOption.Create(50410, Color.white, "CanBeforeSchrodingerCatWinTheCrewmate", AmongUsExtensions.OptionType.Neutral, false, CustomRoleSpawnChances[CustomRoles.SchrodingerCat]);
            SchrodingerCatExiledTeamChanges = CustomOption.Create(50411, Color.white, "SchrodingerCatExiledTeamChanges", AmongUsExtensions.OptionType.Neutral, false, CustomRoleSpawnChances[CustomRoles.SchrodingerCat]);
            //Survivor
            SetupRoleOptions(50100, CustomRoles.Survivor, AmongUsExtensions.OptionType.Neutral);
            NumOfVests = CustomOption.Create(50110, Color.white, "NVest", AmongUsExtensions.OptionType.Neutral, 11, 1, 15, 1, CustomRoleSpawnChances[CustomRoles.Survivor]);
            VestCD = CustomOption.Create(50120, Color.white, "VestCD", AmongUsExtensions.OptionType.Neutral, 30, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Survivor]);
            VestDuration = CustomOption.Create(50130, Color.white, "VestDur", AmongUsExtensions.OptionType.Neutral, 15, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Survivor]);
            //Swapper
            SetupSingleRoleOptions(50712, CustomRoles.Swapper, 1, AmongUsExtensions.OptionType.Neutral);
            //Terrorist
            SetupRoleOptions(50200, CustomRoles.Terrorist, AmongUsExtensions.OptionType.Neutral);
            CanTerroristSuicideWin = CustomOption.Create(50210, Color.white, "CanTerroristSuicideWin", AmongUsExtensions.OptionType.Neutral, false, CustomRoleSpawnChances[CustomRoles.Terrorist], false)
                .SetGameMode(CustomGameMode.Standard);
            //50220~50223を使用
            TerroristTasks = OverrideTasksData.Create(50220, CustomRoles.Terrorist, AmongUsExtensions.OptionType.Neutral);
            //Vulture
            SetupSingleRoleOptions(80000, CustomRoles.Vulture, 1, AmongUsExtensions.OptionType.Neutral);
            BodiesAmount = CustomOption.Create(50515, Color.white, "Bodies", AmongUsExtensions.OptionType.Neutral, 3, 1, 10, 1, CustomRoleSpawnChances[CustomRoles.Vulture]);
            VultureCanVent = CustomOption.Create(6000017, Color.white, "VultureVent", AmongUsExtensions.OptionType.Neutral, false, CustomRoleSpawnChances[CustomRoles.Vulture]);
            VultureHasImpostorVision = CustomOption.Create(6000015, Color.white, "VultureHasImpostorVision", AmongUsExtensions.OptionType.Neutral, false, CustomRoleSpawnChances[CustomRoles.Vulture]);
            VultureArrow = CustomOption.Create(6000019, Color.white, "VultureHasArrow", AmongUsExtensions.OptionType.Neutral, false, CustomRoleSpawnChances[CustomRoles.Vulture]);

            // Crewmate
            //SetupRoleOptions(20000, CustomRoles.Bait);
            //  SetupRoleOptions(20001, CustomRoles.Sleuth);
            /* SetupSingleRoleOptions(20002, CustomRoles.Oblivious, 1);
             SetupSingleRoleOptions(20003, CustomRoles.TieBreaker, 1);
             SetupSingleRoleOptions(20004, CustomRoles.Torch, 1);*/
            /*SetupSingleRoleOptions(20006, CustomRoles.Flash, 1);
            FlashSpeed = CustomOption.Create(20030, Color.white, "SpeedBoosterUpSpeed", 2f, 0.25f, 3f, 0.25f, CustomRoleSpawnChances[CustomRoles.Flash]); */
            SetupRoleOptions(20850, CustomRoles.Bastion, AmongUsExtensions.OptionType.Crewmate);
            BastionVentsRemoveOnBomb = CustomOption.Create(1319, Color.white, "BastionVentsRemoveOnBomb", AmongUsExtensions.OptionType.Crewmate, true, CustomRoleSpawnChances[CustomRoles.Bastion]);
            //SetupRoleOptions(700850, CustomRoles.Alturist);
            //Bodyguard
            SetupRoleOptions(700851, CustomRoles.Bodyguard, AmongUsExtensions.OptionType.Crewmate);
            //Child
            SetupRoleOptions(30008, CustomRoles.Child, AmongUsExtensions.OptionType.Crewmate);
            ChildKnown = CustomOption.Create(30011, Color.white, "ChildKnown", AmongUsExtensions.OptionType.Crewmate, false, CustomRoleSpawnChances[CustomRoles.Child]);
            //Crusader
            SetupRoleOptions(1214002, CustomRoles.Crusader, AmongUsExtensions.OptionType.Crewmate);
            CrusadeCooldown = CustomOption.Create(80523, Color.white, "CrusadeCooldown", AmongUsExtensions.OptionType.Crewmate, 20, 10, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Crusader]);
            //Demolitionist
            SetupRoleOptions(20002, CustomRoles.Demolitionist, AmongUsExtensions.OptionType.Crewmate);
            DemoSuicideTime = CustomOption.Create(20004, Color.white, "DemoSuicideTime", AmongUsExtensions.OptionType.Crewmate, 5f, 1f, 180, 1, CustomRoleSpawnChances[CustomRoles.Demolitionist]);
            //Dictator
            SetupRoleOptions(20900, CustomRoles.Dictator, AmongUsExtensions.OptionType.Crewmate);
            //Doctor
            SetupRoleOptions(20700, CustomRoles.Doctor, AmongUsExtensions.OptionType.Crewmate);
            DoctorTaskCompletedBatteryCharge = CustomOption.Create(20710, Color.white, "DoctorTaskCompletedBatteryCharge", AmongUsExtensions.OptionType.Crewmate, 5, 0, 10, 1, CustomRoleSpawnChances[CustomRoles.Doctor]);
            //Escort
            SetupRoleOptions(7043087, CustomRoles.Escort, AmongUsExtensions.OptionType.Crewmate);
            EscortCooldown = CustomOption.Create(984309, Color.white, "EscortCooldown", AmongUsExtensions.OptionType.Crewmate, 20, 10, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Escort]);
            EscortPreventsVent = CustomOption.Create(80522, Color.white, "EscortPreventsVent", AmongUsExtensions.OptionType.Crewmate, true, CustomRoleSpawnChances[CustomRoles.Escort]);
            //Investigator
            Investigator.SetupCustomOption();
            //Lighter
            SetupRoleOptions(20100, CustomRoles.Lighter, AmongUsExtensions.OptionType.Crewmate);
            LighterTaskCompletedVision = CustomOption.Create(20110, Color.white, "LighterTaskCompletedVision", AmongUsExtensions.OptionType.Crewmate, 2f, 0f, 5f, 0.25f, CustomRoleSpawnChances[CustomRoles.Lighter]);
            LighterTaskCompletedDisableLightOut = CustomOption.Create(20111, Color.white, "LighterTaskCompletedDisableLightOut", AmongUsExtensions.OptionType.Crewmate, true, CustomRoleSpawnChances[CustomRoles.Lighter]);
            //Mayor
            SetupRoleOptions(20200, CustomRoles.Mayor, AmongUsExtensions.OptionType.Crewmate);
            MayorAdditionalVote = CustomOption.Create(20210, Color.white, "MayorAdditionalVote", AmongUsExtensions.OptionType.Crewmate, 1, 1, 99, 1, CustomRoleSpawnChances[CustomRoles.Mayor]);
            MayorVotesAppearBlack = CustomOption.Create(20213, Color.white, "MayorVotesAppearBlack", AmongUsExtensions.OptionType.Crewmate, true, CustomRoleSpawnChances[CustomRoles.Mayor]);
            MayorHasPortableButton = CustomOption.Create(20211, Color.white, "MayorHasPortableButton", AmongUsExtensions.OptionType.Crewmate, false, CustomRoleSpawnChances[CustomRoles.Mayor]);
            MayorNumOfUseButton = CustomOption.Create(20212, Color.white, "MayorNumOfUseButton", AmongUsExtensions.OptionType.Crewmate, 1, 1, 99, 1, MayorHasPortableButton);

            //Mechanic Engineer
            SetupRoleOptions(1302, CustomRoles.Mechanic, AmongUsExtensions.OptionType.Crewmate);
            //Medic
            SetupRoleOptions(700852, CustomRoles.Medic, AmongUsExtensions.OptionType.Crewmate);
            //Medium
            SetupRoleOptions(121400, CustomRoles.Medium, AmongUsExtensions.OptionType.Crewmate);
            MediumArrow = CustomOption.Create(6000021, Color.white, "MediumHasArrow", AmongUsExtensions.OptionType.Crewmate, false, CustomRoleSpawnChances[CustomRoles.Medium]);
            MediumCooldown = CustomOption.Create(6000022, Color.white, "MediumCooldown", AmongUsExtensions.OptionType.Crewmate, 30, 2.5f, 60, 2.5f, MediumArrow);
            //Mystic
            SetupRoleOptions(30009, CustomRoles.Mystic, AmongUsExtensions.OptionType.Crewmate);
            KillFlashDuration = CustomOption.Create(90000, Color.white, "KillFlashDuration", AmongUsExtensions.OptionType.Crewmate, 0.3f, 0.1f, 1, 0.1f, CustomRoleSpawnChances[CustomRoles.Mystic]);
            //Oracle
            SetupRoleOptions(700853, CustomRoles.Oracle, AmongUsExtensions.OptionType.Crewmate);
            //Physicyst
            SetupRoleOptions(1303, CustomRoles.Physicist, AmongUsExtensions.OptionType.Crewmate);
            //Psychic
            SetupRoleOptions(700850, CustomRoles.Psychic, AmongUsExtensions.OptionType.Crewmate);
            CkshowEvil = CustomOption.Create(1318, Color.white, "CrewKillingRed", AmongUsExtensions.OptionType.Crewmate, true, CustomRoleSpawnChances[CustomRoles.Psychic]);
            NBshowEvil = CustomOption.Create(1313, Color.white, "NBareRed", AmongUsExtensions.OptionType.Crewmate, true, CustomRoleSpawnChances[CustomRoles.Psychic]);
            NEshowEvil = CustomOption.Create(1314, Color.white, "NEareRed", AmongUsExtensions.OptionType.Crewmate, true, CustomRoleSpawnChances[CustomRoles.Psychic]);
            MadmatesAreEvil = CustomOption.Create(1315, Color.white, "MadMateIsRed", AmongUsExtensions.OptionType.Crewmate, true, CustomRoleSpawnChances[CustomRoles.Psychic]);
            GAdependsOnTaregtRole = CustomOption.Create(1316, Color.white, "GAdependsOnTaregtRole", AmongUsExtensions.OptionType.Crewmate, true, CustomRoleSpawnChances[CustomRoles.Psychic]);
            ExeTargetShowsEvil = CustomOption.Create(1317, Color.white, "ExeTargetShowsEvil", AmongUsExtensions.OptionType.Crewmate, true, CustomRoleSpawnChances[CustomRoles.Psychic]);
            //SabotageMaster
            SabotageMaster.SetupCustomOption();
            //Sheriff
            Sheriff.SetupCustomOption();
            //Snitch
            SetupRoleOptions(20500, CustomRoles.Snitch, AmongUsExtensions.OptionType.Crewmate);
            SnitchEnableTargetArrow = CustomOption.Create(20510, Color.white, "SnitchEnableTargetArrow", AmongUsExtensions.OptionType.Crewmate, false, CustomRoleSpawnChances[CustomRoles.Snitch]);
            SnitchCanGetArrowColor = CustomOption.Create(20511, Color.white, "SnitchCanGetArrowColor", AmongUsExtensions.OptionType.Crewmate, false, CustomRoleSpawnChances[CustomRoles.Snitch]);
            SnitchCanFindNeutralKiller = CustomOption.Create(20512, Color.white, "SnitchCanFindNeutralKiller", AmongUsExtensions.OptionType.Crewmate, false, CustomRoleSpawnChances[CustomRoles.Snitch]);
            SnitchCanFindCoven = CustomOption.Create(20513, Color.white, "SnitchCanFindCoven", AmongUsExtensions.OptionType.Crewmate, false, CustomRoleSpawnChances[CustomRoles.Snitch]);
            //20520~20523を使用
            SnitchTasks = OverrideTasksData.Create(20520, CustomRoles.Snitch, AmongUsExtensions.OptionType.Crewmate);
            //SpeedBooster
            SetupRoleOptions(20600, CustomRoles.SpeedBooster, AmongUsExtensions.OptionType.Crewmate);
            SpeedBoosterUpSpeed = CustomOption.Create(20610, Color.white, "SpeedBoosterUpSpeed", AmongUsExtensions.OptionType.Crewmate, 2f, 0.25f, 3f, 0.25f, CustomRoleSpawnChances[CustomRoles.SpeedBooster]);
            //Transporter
            SetupSingleRoleOptions(200100, CustomRoles.Transporter, 1, AmongUsExtensions.OptionType.Crewmate);
            NumOfTransports = CustomOption.Create(200110, Color.white, "NumOfTransports", AmongUsExtensions.OptionType.Crewmate, 10, 1, 15, 1, CustomRoleSpawnChances[CustomRoles.Transporter]);
            TransportCooldown = CustomOption.Create(200120, Color.white, "TransportCooldown", AmongUsExtensions.OptionType.Crewmate, 20, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Transporter]);
            //Trapper Trapster
            SetupRoleOptions(20800, CustomRoles.Trapper, AmongUsExtensions.OptionType.Crewmate);
            TrapperBlockMoveTime = CustomOption.Create(20810, Color.white, "TrapperBlockMoveTime", AmongUsExtensions.OptionType.Crewmate, 5f, 1f, 180, 1, CustomRoleSpawnChances[CustomRoles.Trapper]);
            //Veteran
            SetupSingleRoleOptions(20010, CustomRoles.Veteran, 1, AmongUsExtensions.OptionType.Crewmate);
            NumOfVets = CustomOption.Create(20011, Color.white, "NVet", AmongUsExtensions.OptionType.Crewmate, 10, 1, 15, 1, CustomRoleSpawnChances[CustomRoles.Veteran]);
            VetCD = CustomOption.Create(20012, Color.white, "VetCD", AmongUsExtensions.OptionType.Crewmate, 30, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Veteran]);
            VetDuration = CustomOption.Create(20013, Color.white, "VetDur", AmongUsExtensions.OptionType.Crewmate, 15, 2.5f, 180, 2.5f, CustomRoleSpawnChances[CustomRoles.Veteran]);
            CrewRolesVetted = CustomOption.Create(20014, Color.white, "CRGV", AmongUsExtensions.OptionType.Crewmate, true, CustomRoleSpawnChances[CustomRoles.Veteran]);
            PestiAttacksVet = CustomOption.Create(20015, Color.white, "PestiAttacks", AmongUsExtensions.OptionType.Crewmate, PestiAttacksVetString, PestiAttacksVetString[2], CustomRoleSpawnChances[CustomRoles.Veteran]);

            //SetupRoleOptions(30100, CustomRoles.Sleuth);
            //SleuthReport = CustomOption.Create(30110, Color.white, "SleuthReport", false, CustomRoleSpawnChances[CustomRoles.Sleuth]);

            //SetupRoleOptions(50680, CustomRoles.Amnesiac);
            //Modifiers
            //Bait
            SetupSingleRoleOptions(20000, CustomRoles.Bait, 1, AmongUsExtensions.OptionType.Modifier);
            //Bewilder
            SetupSingleRoleOptions(20005, CustomRoles.Bewilder, 1, AmongUsExtensions.OptionType.Modifier);
            BewilderVision = CustomOption.Create(20020, Color.white, "BewilderVision", AmongUsExtensions.OptionType.Modifier, 0.5f, 0f, 5f, 0.25f, CustomRoleSpawnChances[CustomRoles.Bewilder]);
            //Diseased
            SetupSingleRoleOptions(200026, CustomRoles.Diseased, 1, AmongUsExtensions.OptionType.Modifier);
            DiseasedMultiplier = CustomOption.Create(20021, Color.white, "DiseasedMultiplier", AmongUsExtensions.OptionType.Modifier, 2f, 1.5f, 5f, 0.25f, CustomRoleSpawnChances[CustomRoles.Diseased]);
            //Flash
            SetupSingleRoleOptions(200035, CustomRoles.Flash, 1, AmongUsExtensions.OptionType.Modifier);
            FlashSpeed = CustomOption.Create(200305, Color.white, "SpeedBoosterUpSpeed", AmongUsExtensions.OptionType.Modifier, 2f, 0.25f, 3f, 0.25f, CustomRoleSpawnChances[CustomRoles.Flash]);
            //Lovers
            SetupSingleRoleOptions(50300, CustomRoles.LoversRecode, 2, AmongUsExtensions.OptionType.Modifier);
            LoversDieTogether = CustomOption.Create(503005, Color.white, "LoversDieTogether", AmongUsExtensions.OptionType.Modifier, false, CustomRoleSpawnChances[CustomRoles.LoversRecode]);
            LoversKnowRoleOfOtherLover = CustomOption.Create(503005, Color.white, "LoversKnowRoleOfOtherLover", AmongUsExtensions.OptionType.Modifier, true, CustomRoleSpawnChances[CustomRoles.LoversRecode]);
            //Oblivious
            SetupSingleRoleOptions(200025, CustomRoles.Oblivious, 1, AmongUsExtensions.OptionType.Modifier);
            //Sleuth
            SetupSingleRoleOptions(30100, CustomRoles.Sleuth, 1, AmongUsExtensions.OptionType.Modifier);
            //TieBreaker
            SetupSingleRoleOptions(301859, CustomRoles.TieBreaker, 1, AmongUsExtensions.OptionType.Modifier);
            //Torch
            SetupSingleRoleOptions(200045, CustomRoles.Torch, 1, AmongUsExtensions.OptionType.Modifier);
            //Watcher
            SetupSingleRoleOptions(30000, CustomRoles.Watcher, 1, AmongUsExtensions.OptionType.Modifier);
            //EvilWatcherChance = CustomOption.Create(30010, Color.white, "EvilWatcherChance", 0, 0, 100, 10, CustomRoleSpawnChances[CustomRoles.Watcher]);

            // Attribute
            ModifierRestrict = CustomOption.Create(1314, Color.white, "ModifierRestrict", AmongUsExtensions.OptionType.GameOption, true, null, true)
                .SetGameMode(CustomGameMode.Standard);
            ImpostorKnowsRolesOfTeam = CustomOption.Create(102000, Color.white, "ImpostorKnowsRolesOfTeam", AmongUsExtensions.OptionType.GameOption, true, null, true)
                .SetGameMode(CustomGameMode.Standard);
            CovenKnowsRolesOfTeam = CustomOption.Create(102300, Color.white, "CovenKnowsRolesOfTeam", AmongUsExtensions.OptionType.GameOption, true, null, true)
                .SetGameMode(CustomGameMode.Standard);
            GlobalRoleBlockDuration = CustomOption.Create(80009, Color.yellow, "GRB", AmongUsExtensions.OptionType.GameOption, 30, 2.5f, 180, 2.5f, null, true)
                .SetGameMode(CustomGameMode.Standard);
            EnableLastImpostor = CustomOption.Create(80010, Utils.GetRoleColor(CustomRoles.Impostor), "LastImpostor", AmongUsExtensions.OptionType.GameOption, false, null, true)
                .SetGameMode(CustomGameMode.Standard);
            LastImpostorKillCooldown = CustomOption.Create(80020, Color.white, "LastImpostorKillCooldown", AmongUsExtensions.OptionType.GameOption, 15, 0, 180, 1, EnableLastImpostor)
                .SetGameMode(CustomGameMode.Standard);
            #endregion

            // HideAndSeek
            SetupRoleOptions(100000, CustomRoles.HASFox, AmongUsExtensions.OptionType.Neutral, CustomGameMode.HideAndSeek);
            SetupRoleOptions(100100, CustomRoles.HASTroll, AmongUsExtensions.OptionType.Neutral, CustomGameMode.HideAndSeek);
            AllowCloseDoors = CustomOption.Create(101000, Color.white, "AllowCloseDoors", AmongUsExtensions.OptionType.GameOption, false, null, true)
                .SetGameMode(CustomGameMode.HideAndSeek);
            KillDelay = CustomOption.Create(101001, Color.white, "HideAndSeekWaitingTime", AmongUsExtensions.OptionType.GameOption, 10, 0, 180, 5)
                .SetGameMode(CustomGameMode.HideAndSeek);
            //IgnoreCosmetics = CustomOption.Create(101002, Color.white, "IgnoreCosmetics", false)
            //    .SetGameMode(CustomGameMode.HideAndSeek);
            IgnoreVent = CustomOption.Create(101003, Color.white, "IgnoreVent", AmongUsExtensions.OptionType.GameOption, false)
                .SetGameMode(CustomGameMode.HideAndSeek);

            FreeForAllOn = CustomOption.Create(1001009, Color.white, "FreeForAllOn", AmongUsExtensions.OptionType.GameOption, false)
                .SetGameMode(CustomGameMode.HideAndSeek);

            SplatoonOn = CustomOption.Create(1001008, Color.white, "Splatoon", AmongUsExtensions.OptionType.GameOption, false)
                .SetGameMode(CustomGameMode.HideAndSeek);
            SetupRoleOptions(100110, CustomRoles.Supporter, AmongUsExtensions.OptionType.Neutral, CustomGameMode.HideAndSeek);
            SetupRoleOptions(100111, CustomRoles.Janitor, AmongUsExtensions.OptionType.Neutral, CustomGameMode.HideAndSeek);
            STCD = CustomOption.Create(1001001, Color.white, "KillCDT", AmongUsExtensions.OptionType.GameOption, 25, 2.5f, 60, 2.5f)
                .SetGameMode(CustomGameMode.HideAndSeek);
            STIgnoreVent = CustomOption.Create(1001003, Color.white, "CanVent", AmongUsExtensions.OptionType.GameOption, false)
                .SetGameMode(CustomGameMode.HideAndSeek);
            PaintersHaveImpVision = CustomOption.Create(1001004, Color.white, "PaintersHaveImpVision", AmongUsExtensions.OptionType.GameOption, false)
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
            DisableDevices = CustomOption.Create(101200, Color.white, "DisableDevices", AmongUsExtensions.OptionType.GameOption, false, null, true)
                .SetGameMode(CustomGameMode.Standard);
            DisableAdmin = CustomOption.Create(101210, Color.white, "DisableAdmin", AmongUsExtensions.OptionType.GameOption, false, DisableDevices)
                .SetGameMode(CustomGameMode.Standard);
            WhichDisableAdmin = CustomOption.Create(101211, Color.white, "WhichDisableAdmin", AmongUsExtensions.OptionType.GameOption, whichDisableAdmin, whichDisableAdmin[0], DisableAdmin)
                .SetGameMode(CustomGameMode.Standard);

            // ボタン回数同期
            SyncButtonMode = CustomOption.Create(100200, Color.white, "SyncButtonMode", AmongUsExtensions.OptionType.GameOption, false, null, true)
                .SetGameMode(CustomGameMode.Standard);
            SyncedButtonCount = CustomOption.Create(100201, Color.white, "SyncedButtonCount", AmongUsExtensions.OptionType.GameOption, 10, 0, 100, 1, SyncButtonMode)
                .SetGameMode(CustomGameMode.Standard);

            // リアクターの時間制御
            SabotageTimeControl = CustomOption.Create(100800, Color.white, "SabotageTimeControl", AmongUsExtensions.OptionType.GameOption, false, null, true)
                .SetGameMode(CustomGameMode.Standard);
            PolusReactorTimeLimit = CustomOption.Create(100801, Color.white, "PolusReactorTimeLimit", AmongUsExtensions.OptionType.GameOption, 30, 1, 60, 1, SabotageTimeControl)
                .SetGameMode(CustomGameMode.Standard);
            AirshipReactorTimeLimit = CustomOption.Create(100802, Color.white, "AirshipReactorTimeLimit", AmongUsExtensions.OptionType.GameOption, 60, 1, 90, 1, SabotageTimeControl)
                .SetGameMode(CustomGameMode.Standard);

            // タスク無効化
            Customise = CustomOption.Create(101900, Color.white, "Customise", AmongUsExtensions.OptionType.GameOption, true, null, true)
                .SetGameMode(CustomGameMode.All);
            CheckRoleTwiceBeforeAdd = CustomOption.Create(101901, Color.white, "CheckRoleTwiceBeforeAdd", AmongUsExtensions.OptionType.GameOption, false, null, true)
            .SetGameMode(CustomGameMode.All);
            RolesLikeToU = CustomOption.Create(102000, Color.white, "RolesLikeToU", AmongUsExtensions.OptionType.GameOption, false, null, true)
            .SetGameMode(CustomGameMode.All);
            DisableTasks = CustomOption.Create(100300, Color.white, "DisableTasks", AmongUsExtensions.OptionType.GameOption, false, null, true)
                .SetGameMode(CustomGameMode.All);
            DisableSwipeCard = CustomOption.Create(100301, Color.white, "DisableSwipeCardTask", AmongUsExtensions.OptionType.GameOption, false, DisableTasks)
                .SetGameMode(CustomGameMode.All);
            DisableSubmitScan = CustomOption.Create(100302, Color.white, "DisableSubmitScanTask", AmongUsExtensions.OptionType.GameOption, false, DisableTasks)
                .SetGameMode(CustomGameMode.All);
            DisableUnlockSafe = CustomOption.Create(100303, Color.white, "DisableUnlockSafeTask", AmongUsExtensions.OptionType.GameOption, false, DisableTasks)
                .SetGameMode(CustomGameMode.All);
            DisableUploadData = CustomOption.Create(100304, Color.white, "DisableUploadDataTask", AmongUsExtensions.OptionType.GameOption, false, DisableTasks)
                .SetGameMode(CustomGameMode.All);
            DisableStartReactor = CustomOption.Create(100305, Color.white, "DisableStartReactorTask", AmongUsExtensions.OptionType.GameOption, false, DisableTasks)
                .SetGameMode(CustomGameMode.All);
            DisableResetBreaker = CustomOption.Create(100306, Color.white, "DisableResetBreakerTask", AmongUsExtensions.OptionType.GameOption, false, DisableTasks)
                .SetGameMode(CustomGameMode.All);
            DisableFixWiring = CustomOption.Create(100307, Color.white, "DisableFixWiring", AmongUsExtensions.OptionType.GameOption, false, DisableTasks)
                .SetGameMode(CustomGameMode.All);

            // ランダムマップ
            RandomMapsMode = CustomOption.Create(100400, Color.white, "RandomMapsMode", AmongUsExtensions.OptionType.GameOption, false, null, true)
                .SetGameMode(CustomGameMode.All);
            AddedTheSkeld = CustomOption.Create(100401, Color.white, "AddedTheSkeld", AmongUsExtensions.OptionType.GameOption, false, RandomMapsMode)
                .SetGameMode(CustomGameMode.All);
            AddedMiraHQ = CustomOption.Create(100402, Color.white, "AddedMIRAHQ", AmongUsExtensions.OptionType.GameOption, false, RandomMapsMode)
                .SetGameMode(CustomGameMode.All);
            AddedPolus = CustomOption.Create(100403, Color.white, "AddedPolus", AmongUsExtensions.OptionType.GameOption, false, RandomMapsMode)
                .SetGameMode(CustomGameMode.All);
            AddedTheAirShip = CustomOption.Create(100404, Color.white, "AddedTheAirShip", AmongUsExtensions.OptionType.GameOption, false, RandomMapsMode)
                .SetGameMode(CustomGameMode.All);
            // MapDleks = CustomOption.Create(100405, Color.white, "AddedDleks",AmongUsExtensions.OptionType.GameOption, false, RandomMapMode)
            //     .SetGameMode(CustomGameMode.All);

            // 投票モード
            VoteMode = CustomOption.Create(100500, Color.white, "VoteMode", AmongUsExtensions.OptionType.GameOption, false, null, true)
                .SetGameMode(CustomGameMode.Standard);
            WhenSkipVote = CustomOption.Create(100501, Color.white, "WhenSkipVote", AmongUsExtensions.OptionType.GameOption, voteModes[0..3], voteModes[0], VoteMode)
                .SetGameMode(CustomGameMode.Standard);
            WhenNonVote = CustomOption.Create(100502, Color.white, "WhenNonVote", AmongUsExtensions.OptionType.GameOption, voteModes, voteModes[0], VoteMode)
                .SetGameMode(CustomGameMode.Standard);

            // 転落死
            LadderDeath = CustomOption.Create(101100, Color.white, "LadderDeath", AmongUsExtensions.OptionType.GameOption, false, null, true);
            LadderDeathChance = CustomOption.Create(101110, Color.white, "LadderDeathChance", AmongUsExtensions.OptionType.GameOption, rates[1..], rates[2], LadderDeath);

            // 通常モードでかくれんぼ用
            StandardHAS = CustomOption.Create(100700, Color.white, "StandardHAS", AmongUsExtensions.OptionType.GameOption, false, null, true)
                .SetGameMode(CustomGameMode.Standard);
            StandardHASWaitingTime = CustomOption.Create(100701, Color.white, "StandardHASWaitingTime", AmongUsExtensions.OptionType.GameOption, 10f, 0f, 180f, 2.5f, StandardHAS)
                .SetGameMode(CustomGameMode.Standard);

            MinNK = CustomOption.Create(1007012, Color.white, "MinNK", AmongUsExtensions.OptionType.GameOption, 0, 0, 11, 1, null, true)
                .SetGameMode(CustomGameMode.Standard);
            MaxNK = CustomOption.Create(1007013, Color.white, "MaxNK", AmongUsExtensions.OptionType.GameOption, 0, 0, 11, 1, null, true)
                .SetGameMode(CustomGameMode.Standard);
            MinNonNK = CustomOption.Create(1007014, Color.white, "MinNonNK", AmongUsExtensions.OptionType.GameOption, 0, 0, 11, 1, null, true)
                .SetGameMode(CustomGameMode.Standard);
            MaxNonNK = CustomOption.Create(1007015, Color.white, "MaxNonNK", AmongUsExtensions.OptionType.GameOption, 0, 0, 11, 1, null, true)
                .SetGameMode(CustomGameMode.Standard);

            // その他
            CamoComms = CustomOption.Create(100607, Color.white, "CamoComms", AmongUsExtensions.OptionType.GameOption, false, null, true)
                .SetGameMode(CustomGameMode.All);
            NoGameEnd = CustomOption.Create(100600, Color.white, "NoGameEnd", AmongUsExtensions.OptionType.GameOption, false, null, true)
                .SetGameMode(CustomGameMode.All);
            AutoDisplayLastResult = CustomOption.Create(100601, Color.white, "AutoDisplayLastResult", AmongUsExtensions.OptionType.GameOption, false)
                .SetGameMode(CustomGameMode.All);
            SuffixMode = CustomOption.Create(100602, Color.white, "SuffixMode", AmongUsExtensions.OptionType.GameOption, suffixModes, suffixModes[0])
                .SetGameMode(CustomGameMode.All);
            ColorNameMode = CustomOption.Create(100605, Color.white, "ColorNameMode", AmongUsExtensions.OptionType.GameOption, false)
                .SetGameMode(CustomGameMode.All);
            DisableTaskWin = CustomOption.Create(100609, Color.white, "DisableTaskWin", AmongUsExtensions.OptionType.GameOption, false)
                .SetGameMode(CustomGameMode.All);
            GhostCanSeeOtherRoles = CustomOption.Create(100603, Color.white, "GhostCanSeeOtherRoles", AmongUsExtensions.OptionType.GameOption, true)
                .SetGameMode(CustomGameMode.All);
            GhostCanSeeOtherVotes = CustomOption.Create(100604, Color.white, "GhostCanSeeOtherVotes", AmongUsExtensions.OptionType.GameOption, true)
                .SetGameMode(CustomGameMode.All);

            IsLoaded = true;
        }

        public static void SetupRoleOptions(int id, CustomRoles role, AmongUsExtensions.OptionType type, CustomGameMode customGameMode = CustomGameMode.Standard)
        {
            var spawnOption = CustomOption.Create(id, Utils.GetRoleColor(role), role.ToString(), type, rates, rates[0], null, true)
                .HiddenOnDisplay(true)
                .SetGameMode(customGameMode);
            var countOption = CustomOption.Create(id + 1, Color.white, "Maximum", type, 1, 1, 15, 1, spawnOption, false)
                .HiddenOnDisplay(true)
                .SetGameMode(customGameMode);

            CustomRoleSpawnChances.Add(role, spawnOption);
            CustomRoleCounts.Add(role, countOption);
        }
        public static void SetUpCovenOptions(int id, CustomGameMode customGameMode = CustomGameMode.Standard)
        {
            var spawnOption = CustomOption.Create(id, Utils.GetRoleColor(CustomRoles.Coven), CustomRoles.Coven.ToString(), AmongUsExtensions.OptionType.Neutral, rates, rates[0], null, true)
                .HiddenOnDisplay(true)
                .SetGameMode(customGameMode);
            var countOption = CustomOption.Create(id + 1, Color.white, "Maximum", AmongUsExtensions.OptionType.Neutral, 1, 1, 3, 1, spawnOption, false)
                .HiddenOnDisplay(true)
                .SetGameMode(customGameMode);

            CustomRoleSpawnChances.Add(CustomRoles.Coven, spawnOption);
            CustomRoleCounts.Add(CustomRoles.Coven, countOption);
        }
        private static void SetupLoversRoleOptionsToggle(int id, CustomGameMode customGameMode = CustomGameMode.Standard)
        {
            var role = CustomRoles.LoversRecode
            ;
            var spawnOption = CustomOption.Create(id, Utils.GetRoleColor(role), role.ToString(), AmongUsExtensions.OptionType.Modifier, rates, rates[0], null, true)
                .HiddenOnDisplay(true)
                .SetGameMode(customGameMode);

            var countOption = CustomOption.Create(id + 1, Color.white, "NumberOfLovers", AmongUsExtensions.OptionType.Modifier, 2, 1, 15, 1, spawnOption, false, true)
                .HiddenOnDisplay(false)
                .SetGameMode(customGameMode);

            CustomRoleSpawnChances.Add(role, spawnOption);
            CustomRoleCounts.Add(role, countOption);
        }
        public static void SetupSingleRoleOptions(int id, CustomRoles role, int count, AmongUsExtensions.OptionType type, CustomGameMode customGameMode = CustomGameMode.Standard)
        {
            var spawnOption = CustomOption.Create(id, Utils.GetRoleColor(role), role.ToString(), type, rates, rates[0], null, true)
                .HiddenOnDisplay(true)
                .SetGameMode(customGameMode);
            // 初期値,最大値,最小値が同じで、stepが0のどうやっても変えることができない個数オプション
            var countOption = CustomOption.Create(id + 1, Color.white, "Maximum", type, count, count, count, count, spawnOption, false, true)
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

            public OverrideTasksData(int idStart, CustomRoles role, AmongUsExtensions.OptionType type)
            {
                this.IdStart = idStart;
                this.Role = role;
                Dictionary<string, string> replacementDic = new() { { "%role%", Utils.GetRoleName(role) } };
                doOverride = CustomOption.Create(idStart++, Color.white, "doOverride", type, false, CustomRoleSpawnChances[role], false, false, "", replacementDic);
                assignCommonTasks = CustomOption.Create(idStart++, Color.white, "assignCommonTasks", type, true, doOverride, false, false, "", replacementDic);
                numLongTasks = CustomOption.Create(idStart++, Color.white, "roleLongTasksNum", type, 3, 0, 99, 1, doOverride, false, false, "", replacementDic);
                numShortTasks = CustomOption.Create(idStart++, Color.white, "roleShortTasksNum", type, 3, 0, 99, 1, doOverride, false, false, "", replacementDic);

                if (!AllData.ContainsKey(role)) AllData.Add(role, this);
                else Logger.Warn("重複したCustomRolesを対象とするOverrideTasksDataが作成されました", "OverrideTasksData");
            }
            public static OverrideTasksData Create(int idStart, CustomRoles role, AmongUsExtensions.OptionType type)
            {
                return new OverrideTasksData(idStart, role, type);
            }
        }
    }
}
