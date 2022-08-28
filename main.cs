using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using UnityEngine;

[assembly: AssemblyFileVersionAttribute(TownOfHost.Main.PluginVersion)]
[assembly: AssemblyInformationalVersionAttribute(TownOfHost.Main.PluginVersion)]
namespace TownOfHost
{
    [BepInPlugin(PluginGuid, "Town Of Host: The Other Roles", PluginVersion)]
    [BepInProcess("Among Us.exe")]
    public class Main : BasePlugin
    {
        //Sorry for many Japanese comments.
        public const string PluginGuid = "com.emptybottle.townofhost";
        public const string PluginVersion = "0.6.1";
        public Harmony Harmony { get; } = new Harmony(PluginGuid);
        public static Version version = Version.Parse(PluginVersion);
        public static BepInEx.Logging.ManualLogSource Logger;
        public static bool hasArgumentException = false;
        public static string ExceptionMessage;
        public static bool ExceptionMessageIsShown = false;
        public static string credentialsText;
        //Client Options
        public static ConfigEntry<string> HideName { get; private set; }
        public static ConfigEntry<string> HideColor { get; private set; }
        public static ConfigEntry<bool> ForceJapanese { get; private set; }
        public static ConfigEntry<bool> JapaneseRoleName { get; private set; }
        public static ConfigEntry<bool> AmDebugger { get; private set; }
        public static ConfigEntry<string> ShowPopUpVersion { get; private set; }
        public static ConfigEntry<int> MessageWait { get; private set; }

        public static LanguageUnit EnglishLang { get; private set; }
        public static Dictionary<byte, PlayerVersion> playerVersion = new();
        //Other Configs
        public static ConfigEntry<bool> IgnoreWinnerCommand { get; private set; }
        public static ConfigEntry<string> WebhookURL { get; private set; }
        public static ConfigEntry<float> LastKillCooldown { get; private set; }
        public static CustomWinner currentWinner;
        public static HashSet<AdditionalWinners> additionalwinners = new();
        public static GameOptionsData RealOptionsData;
        public static Dictionary<byte, string> AllPlayerNames;
        public static Dictionary<(byte, byte), string> LastNotifyNames;
        public static Dictionary<byte, CustomRoles> AllPlayerCustomRoles;
        public static Dictionary<byte, CustomRoles> AllPlayerCustomSubRoles;
        public static Dictionary<byte, Color32> PlayerColors = new();
        public static Dictionary<byte, PlayerState.DeathReason> AfterMeetingDeathPlayers = new();
        public static Dictionary<CustomRoles, String> roleColors;
        //これ変えたらmod名とかの色が変わる
        public static string modColor = "#4FF918";
        public static bool IsFixedCooldown => CustomRoles.Vampire.IsEnable();
        public static float RefixCooldownDelay = 0f;
        public static int BeforeFixMeetingCooldown = 10;
        public static List<byte> ResetCamPlayerList;
        public static List<byte> winnerList;
        public static List<(string, byte)> MessagesToSend;
        public static bool isChatCommand = false;
        public static string TextCursor => TextCursorVisible ? "_" : "";
        public static bool TextCursorVisible;
        public static float TextCursorTimer;
        public static List<PlayerControl> LoversPlayers = new();
        public static bool isLoversDead = true;

        public static Dictionary<CustomRoles, PlayerControl> HasModifier = new();
        public static List<CustomRoles> modifiersList = new();
        public static Dictionary<byte, float> AllPlayerKillCooldown = new();
        public static Dictionary<byte, float> AllPlayerSpeed = new();
        public static Dictionary<byte, (byte, float)> BitPlayers = new();
        public static Dictionary<byte, float> WarlockTimer = new();
        public static Dictionary<byte, PlayerControl> CursedPlayers = new();
        public static List<PlayerControl> SpelledPlayer = new();
        public static Dictionary<byte, bool> KillOrSpell = new();
        public static Dictionary<byte, bool> KillOrSilence = new();
        public static Dictionary<byte, bool> isCurseAndKill = new();
        public static Dictionary<(byte, byte), bool> isDoused = new();
        public static Dictionary<(byte, byte), bool> isHexed = new();
        public static Dictionary<byte, (PlayerControl, float)> ArsonistTimer = new();
        public static Dictionary<byte, float> AirshipMeetingTimer = new();
        public static Dictionary<byte, byte> ExecutionerTarget = new(); //Key : Executioner, Value : target
        public static Dictionary<byte, byte> GuardianAngelTarget = new(); //Key : GA, Value : target
        public static Dictionary<byte, byte> PuppeteerList = new(); // Key: targetId, Value: PuppeteerId
        public static Dictionary<byte, byte> WitchedList = new(); // Key: targetId, Value: WitchId
        public static Dictionary<byte, byte> SpeedBoostTarget = new();
        public static Dictionary<byte, int> MayorUsedButtonCount = new();
        public static Dictionary<byte, int> HackerFixedSaboCount = new();
        public static int AliveImpostorCount;
        public static string LastVotedPlayer;
        public static int HexesThisRound;
        public static int SKMadmateNowCount;
        public static bool witchMeeting;
        public static bool isCursed;
        public static List<PlayerControl> firstKill = new();
        public static List<byte> unreportableBodies = new();
        public static List<PlayerControl> SilencedPlayer = new();
        public static List<byte> KilledBewilder = new();
        public static List<byte> KilledDemo = new();
        public static bool isSilenced;
        public static bool isShipStart;
        public static Dictionary<byte, bool> CheckShapeshift = new();
        public static Dictionary<(byte, byte), string> targetArrows = new();
        public static byte WonTrollID;
        public static byte ExiledJesterID;
        public static byte WonTerroristID;
        public static byte WonExecutionerID;
        public static byte WonHackerID;
        public static byte WonArsonistID;
        public static byte WonChildID;
        public static bool CustomWinTrigger;
        public static bool VisibleTasksCount;
        public static string nickName = "";
        public static bool introDestroyed = false;
        public static int DiscussionTime;
        public static int VotingTime;
        public static int JugKillAmounts;
        public static int AteBodies;
        public static byte currentDousingTarget;
        public static int VetAlerts;
        public static bool IsRoundOne;

        //plague info.
        public static byte currentInfectingTarget;
        public static Dictionary<(byte, byte), bool> isInfected = new();
        public static Dictionary<byte, (PlayerControl, float)> PlagueBearerTimer = new();
        public static List<int> bombedVents = new();

        public static Main Instance;
        public static bool CamoComms;

        //coven
        //coven main info
        public static int CovenMeetings;
        public static bool HasNecronomicon;
        public static bool ChoseWitch;
        public static bool WitchProtected;
        //role info
        public static bool HexMasterOn;
        public static bool PotionMasterOn;
        public static bool VampireDitchesOn;
        public static bool MedusaOn;
        public static bool MimicOn;
        public static bool NecromancerOn;
        public static bool ConjurorOn;

        public static bool GazeReady;
        public static bool IsGazing;

        // VETERAN STUFF //
        public static bool VettedThisRound;
        public static bool VetIsAlerted;

        public static int GAprotects;

        //TEAM TRACKS
        public static int TeamCovenAlive;
        public static bool TeamPestiAlive;
        public static bool TeamJuggernautAlive;
        public static bool ProtectedThisRound;
        public static bool HasProtected;
        public static int ProtectsSoFar;
        public static bool IsProtected;
        public static bool IsRoundOneGA;

        // NEUTRALS //
        public static bool IsRampaged;
        public static bool RampageReady;
        public static bool IsHackMode;
        public override void Load()
        {
            Instance = this;

            TextCursorTimer = 0f;
            TextCursorVisible = true;

            //Client Options
            HideName = Config.Bind("Client Options", "Hide Game Code Name", "Town Of Host");
            HideColor = Config.Bind("Client Options", "Hide Game Code Color", $"{modColor}");
            ForceJapanese = Config.Bind("Client Options", "Force Japanese", false);
            JapaneseRoleName = Config.Bind("Client Options", "Japanese Role Name", true);
            Logger = BepInEx.Logging.Logger.CreateLogSource("TownOfHost");
            TownOfHost.Logger.Enable();
            TownOfHost.Logger.Disable("NotifyRoles");
            TownOfHost.Logger.Disable("SendRPC");
            TownOfHost.Logger.Disable("ReceiveRPC");
            TownOfHost.Logger.Disable("SwitchSystem");
            //TownOfHost.Logger.isDetail = true;

            currentWinner = CustomWinner.Default;
            additionalwinners = new HashSet<AdditionalWinners>();

            AllPlayerCustomRoles = new Dictionary<byte, CustomRoles>();
            AllPlayerCustomSubRoles = new Dictionary<byte, CustomRoles>();
            CustomWinTrigger = false;
            BitPlayers = new Dictionary<byte, (byte, float)>();
            WarlockTimer = new Dictionary<byte, float>();
            CursedPlayers = new Dictionary<byte, PlayerControl>();
            SpelledPlayer = new List<PlayerControl>();
            SilencedPlayer = new List<PlayerControl>();
            isDoused = new Dictionary<(byte, byte), bool>();
            isHexed = new Dictionary<(byte, byte), bool>();
            isInfected = new Dictionary<(byte, byte), bool>();
            ArsonistTimer = new Dictionary<byte, (PlayerControl, float)>();
            PlagueBearerTimer = new Dictionary<byte, (PlayerControl, float)>();
            ExecutionerTarget = new Dictionary<byte, byte>();
            GuardianAngelTarget = new Dictionary<byte, byte>();
            MayorUsedButtonCount = new Dictionary<byte, int>();
            HackerFixedSaboCount = new Dictionary<byte, int>();
            //firstKill = new Dictionary<byte, (PlayerControl, float)>();
            winnerList = new();
            VisibleTasksCount = false;
            MessagesToSend = new List<(string, byte)>();
            currentDousingTarget = 255;
            currentInfectingTarget = 255;
            JugKillAmounts = 0;
            AteBodies = 0;
            CovenMeetings = 0;
            GAprotects = 0;
            ProtectedThisRound = false;
            HasProtected = false;
            VetAlerts = 0;
            ProtectsSoFar = 0;
            IsProtected = false;
            VettedThisRound = false;
            WitchProtected = false;
            HexMasterOn = false;
            PotionMasterOn = false;
            VampireDitchesOn = false;
            MedusaOn = false;
            MimicOn = false;
            NecromancerOn = false;
            ConjurorOn = false;
            ChoseWitch = false;
            HasNecronomicon = false;
            VetIsAlerted = false;
            IsRoundOne = false;
            IsRoundOneGA = false;

            IsRampaged = false;
            RampageReady = false;

            IsHackMode = false;
            GazeReady = true;
            IsGazing = false;
            CamoComms = false;
            HexesThisRound = 0;
            LastVotedPlayer = "";

            // OTHER//

            TeamJuggernautAlive = false;
            TeamPestiAlive = false;
            TeamCovenAlive = 3;

            IgnoreWinnerCommand = Config.Bind("Other", "IgnoreWinnerCommand", true);
            WebhookURL = Config.Bind("Other", "WebhookURL", "none");
            AmDebugger = Config.Bind("Other", "AmDebugger", false);
            ShowPopUpVersion = Config.Bind("Other", "ShowPopUpVersion", "0");
            MessageWait = Config.Bind("Other", "MessageWait", 1);
            LastKillCooldown = Config.Bind("Other", "LastKillCooldown", (float)30);

            NameColorManager.Begin();

            Translator.Init();

            hasArgumentException = false;
            ExceptionMessage = "";
            try
            {

                roleColors = new Dictionary<CustomRoles, string>()
                {
                    //バニラ役職
                    {CustomRoles.Crewmate, "#ffffff"},
                    {CustomRoles.Engineer, "#b6f0ff"},
                    { CustomRoles.Scientist, "#b6f0ff"},
                    { CustomRoles.GuardianAngel, "#ffffff"},
                    { CustomRoles.CorruptedSheriff, "#ff0000"},
                    //インポスター、シェイプシフター
                    //特殊インポスター役職
                    //マッドメイト系役職
                        //後で追加
                    //両陣営可能役職
                    { CustomRoles.Watcher, "#800080"},
                    //特殊クルー役職
                    { CustomRoles.NiceWatcher, "#800080"}, //ウォッチャーの派生
                    { CustomRoles.Bait, "#00f7ff"},
                    { CustomRoles.SabotageMaster, "#0000ff"},
                    { CustomRoles.Snitch, "#b8fb4f"},
                    { CustomRoles.Mayor, "#204d42"},
                    { CustomRoles.Sheriff, "#f8cd46"},
                    { CustomRoles.Lighter, "#eee5be"},
                    { CustomRoles.SpeedBooster, "#00ffff"},
                    { CustomRoles.Doctor, "#80ffdd"},
                    { CustomRoles.Child, "#FFFFFF"},
                    { CustomRoles.Trapper, "#5a8fd0"},
                    { CustomRoles.Dictator, "#df9b00"},
                    { CustomRoles.Sleuth, "#800000"},
                    { CustomRoles.PlagueBearer, "#F1F89E"},
                    { CustomRoles.Pestilence, "#393939"},
                    { CustomRoles.Vulture, "#a36727"},
                    { CustomRoles.CSchrodingerCat, "#ffffff"}, //シュレディンガーの猫の派生
                    //第三陣営役職
                    { CustomRoles.Arsonist, "#ff6633"},
                    { CustomRoles.Jester, "#ec62a5"},
                    { CustomRoles.Terrorist, "#00ff00"},
                    { CustomRoles.Executioner, "#C96600"},
                    { CustomRoles.Opportunist, "#00ff00"},
                    { CustomRoles.SchrodingerCat, "#696969"},
                    { CustomRoles.Egoist, "#5600ff"},
                    { CustomRoles.EgoSchrodingerCat, "#5600ff"},
                    { CustomRoles.Jackal, "#00b4eb"},
                    //{ CustomRoles.Juggernaut, "#882ee8"},
                    { CustomRoles.Juggernaut, "#670038"},
                    { CustomRoles.JSchrodingerCat, "#00b4eb"},
                    //HideAndSeek
                    { CustomRoles.HASFox, "#e478ff"},
                    { CustomRoles.HASTroll, "#00ff00"},
                    // GM
                    { CustomRoles.GM, "#ff5b70"},
                    //サブ役職
                    { CustomRoles.NoSubRoleAssigned, "#ffffff"},
                    { CustomRoles.Lovers, "#ffaaaa"},
                    { CustomRoles.Coven, "#592e98"},
                    { CustomRoles.Veteran, "#978046"},
                    { CustomRoles.GuardianAngelTOU, "#26FEFE"},
                    { CustomRoles.TheGlitch, "#00FA05"},
                    { CustomRoles.Werewolf, "#B2762A"},
                    { CustomRoles.Amnesiac, "#81DDFC"},
                    { CustomRoles.Bewilder, "#292644"},
                    { CustomRoles.Demolitionist, "#5e2801"},
                    { CustomRoles.Bastion, "#524f4d"},
                    { CustomRoles.Hacker, "#358013"},
                };
                foreach (var role in Enum.GetValues(typeof(CustomRoles)).Cast<CustomRoles>())
                {
                    switch (role.GetRoleType())
                    {
                        case RoleType.Impostor:
                            roleColors.TryAdd(role, "#ff0000");
                            break;
                        case RoleType.Madmate:
                            roleColors.TryAdd(role, "#ff0000");
                            break;
                        case RoleType.Coven:
                            roleColors.TryAdd(role, "#592e98");
                            break;
                        default:
                            break;
                    }
                    //switch (role.GetRole)
                }
            }
            catch (ArgumentException ex)
            {
                TownOfHost.Logger.Error("エラー:Dictionaryの値の重複を検出しました", "LoadDictionary");
                TownOfHost.Logger.Error(ex.Message, "LoadDictionary");
                hasArgumentException = true;
                ExceptionMessage = ex.Message;
                ExceptionMessageIsShown = false;
            }
            TownOfHost.Logger.Info($"{nameof(ThisAssembly.Git.Branch)}: {ThisAssembly.Git.Branch}", "GitVersion");
            TownOfHost.Logger.Info($"{nameof(ThisAssembly.Git.BaseTag)}: {ThisAssembly.Git.BaseTag}", "GitVersion");
            TownOfHost.Logger.Info($"{nameof(ThisAssembly.Git.Commit)}: {ThisAssembly.Git.Commit}", "GitVersion");
            TownOfHost.Logger.Info($"{nameof(ThisAssembly.Git.Commits)}: {ThisAssembly.Git.Commits}", "GitVersion");
            TownOfHost.Logger.Info($"{nameof(ThisAssembly.Git.IsDirty)}: {ThisAssembly.Git.IsDirty}", "GitVersion");
            TownOfHost.Logger.Info($"{nameof(ThisAssembly.Git.Sha)}: {ThisAssembly.Git.Sha}", "GitVersion");
            TownOfHost.Logger.Info($"{nameof(ThisAssembly.Git.Tag)}: {ThisAssembly.Git.Tag}", "GitVersion");

            if (!File.Exists("template.txt"))
            {
                TownOfHost.Logger.Info("Among Us.exeと同じフォルダにtemplate.txtが見つかりませんでした。新規作成します。", "Template");
                try
                {
                    File.WriteAllText(@"template.txt", "test:This is template text.\\nLine breaks are also possible.\ntest:これは定型文です。\\n改行も可能です。");
                }
                catch (Exception ex)
                {
                    TownOfHost.Logger.Error(ex.ToString(), "Template");
                }
            }

            Harmony.PatchAll();
        }

        [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.Initialize))]
        class TranslationControllerInitializePatch
        {
            public static void Postfix(TranslationController __instance)
            {
                var english = __instance.Languages.Where(lang => lang.languageID == SupportedLangs.English).FirstOrDefault();
                EnglishLang = new LanguageUnit(english);
            }
        }
    }
    public enum CustomRoles
    {
        //Default
        Crewmate = 0,
        //Impostor(Vanilla)
        Impostor,
        Shapeshifter,
        //Impostor
        BountyHunter,
        EvilWatcher,
        FireWorks,
        Mafia,
        SerialKiller,
        //ShapeMaster,
        Sniper,
        Vampire,
        Witch,
        Warlock,
        Mare,
        Puppeteer,
        TimeThief,
        Silencer,
        EvilGuesser,
        LastImpostor,
        //Madmate
        MadGuardian,
        Madmate,
        MadSnitch,
        CorruptedSheriff,
        SKMadmate,
        MSchrodingerCat,//インポスター陣営のシュレディンガーの猫
                        //両陣営
        Watcher,
        //Crewmate(Vanilla)
        Engineer,
        GuardianAngel,
        Scientist,
        //Crewmate
        Bait,
        Sleuth,
        Bewilder,
        Lighter,
        Demolitionist,
        Bastion,
        NiceGuesser,
        Hacker,
        Mayor,
        NiceWatcher,
        SabotageMaster,
        Sheriff,
        Snitch,
        SpeedBooster,
        Trapper,
        Dictator,
        Doctor,
        Child,
        //Sleuth,
        Veteran,
        CSchrodingerCat,//クルー陣営のシュレディンガーの猫
                        //Neutral
        Arsonist,
        Egoist,
        PlagueBearer,
        Pestilence,
        Vulture,
        TheGlitch,
        Werewolf,
        GuardianAngelTOU,
        EgoSchrodingerCat,//エゴイスト陣営のシュレディンガーの猫
        Jester,
        Amnesiac,
        Pirate,
        Juggernaut,
        Opportunist,
        SchrodingerCat,//第三陣営のシュレディンガーの猫
        Terrorist,
        Executioner,
        Jackal,
        JSchrodingerCat,//ジャッカル陣営のシュレディンガーの猫
                        //HideAndSeek
        HASFox,
        HASTroll,
        //GM
        GM,
        //coven
        Coven,
        Poisoner,
        CovenWitch,
        HexMaster,
        PotionMaster,
        Medusa,
        Mimic,
        Necromancer,
        Conjuror,
        // Sub-roles are After 500. Meaning, all roles under this are Modifiers.
        NoSubRoleAssigned = 500,

        // GLOBAL MODIFIERS //
        Lovers,
        Flash, // DONE
        TieBreaker,
        Oblivious, // DONE
        //Sleuth, // DONE

        // CREW MODIFIERS //
        //Bewilder, // DONE
        //Bait, // DONE
        Torch, // DONE
    }
    //WinData
    public enum CustomWinner
    {
        Draw = -1,
        Default = -2,
        None = -3,
        Impostor = CustomRoles.Impostor,
        Crewmate = CustomRoles.Crewmate,
        Jester = CustomRoles.Jester,
        Terrorist = CustomRoles.Terrorist,
        Lovers = CustomRoles.Lovers,
        Child = CustomRoles.Child,
        Executioner = CustomRoles.Executioner,
        Arsonist = CustomRoles.Arsonist,
        Vulture = CustomRoles.Vulture,
        Egoist = CustomRoles.Egoist,
        Pestilence = CustomRoles.Pestilence,
        Jackal = CustomRoles.Jackal,
        Juggernaut = CustomRoles.Juggernaut,
        HASTroll = CustomRoles.HASTroll,
        Coven = CustomRoles.Coven,
        TheGlitch = CustomRoles.TheGlitch,
        Werewolf = CustomRoles.Werewolf,
        Hacker = CustomRoles.Hacker
    }
    public enum AdditionalWinners
    {
        None = -1,
        Opportunist = CustomRoles.Opportunist,
        SchrodingerCat = CustomRoles.SchrodingerCat,
        Executioner = CustomRoles.Executioner,
        HASFox = CustomRoles.HASFox,
        GuardianAngelTOU = CustomRoles.GuardianAngelTOU
    }
    /*public enum CustomRoles : byte
    {
        Default = 0,
        HASTroll = 1,
        HASHox = 2
    }*/
    public enum SuffixModes
    {
        None = 0,
        TOH,
        Streaming,
        Recording
    }
    public enum VersionTypes
    {
        Released = 0,
        Beta = 1
    }

    public enum VoteMode
    {
        Default,
        Suicide,
        SelfVote,
        Skip
    }
}
