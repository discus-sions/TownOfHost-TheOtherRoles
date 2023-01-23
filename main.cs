using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using Il2CppInterop.Runtime;
using UnityEngine.SceneManagement;
using System.Net;
using System.Net.Sockets;
using AmongUs.GameOptions;

[assembly: AssemblyFileVersionAttribute(TownOfHost.Main.PluginVersion)]
[assembly: AssemblyInformationalVersionAttribute(TownOfHost.Main.PluginVersion)]
namespace TownOfHost
{
    [BepInPlugin(PluginGuid, "Town Of Host: The Other Roles", PluginVersion)]
    [BepInProcess("Among Us.exe")]
    public class Main : BasePlugin
    {
        //Sorry for many Japanese comments.
        public const string PluginGuid = "com.discussions.tohtor";
        public static readonly string TEMPLATE_FILE_PATH = "./TOR_DATA/template.txt";
        public static readonly string BANNEDWORDS_FILE_PATH = "./TOR_DATA/bannedwords.txt";
        public static readonly string BANNEDFRIENDCODES_FILE_PATH = "./TOR_DATA/bannedfriendcodes.txt";
        public static readonly string DiscordInviteUrl = "https://discord.gg/tohtor";
        public static readonly bool ShowDiscordButton = true;
        public const string PluginVersion = "0.9.3.7";
        public const string DevVersion = "10.1";
        public const string FullDevVersion = $" dev {DevVersion}";
        public Harmony Harmony { get; } = new Harmony(PluginGuid);
        public static Version version = Version.Parse(PluginVersion);
        public static BepInEx.Logging.ManualLogSource Logger;
        public static bool hasArgumentException = false;
        public static string ExceptionMessage;
        public static bool ExceptionMessageIsShown = false;
        public static string credentialsText;
        public static string versionText;
        //Client Options
        public static ConfigEntry<string> HideName { get; private set; }
        public static ConfigEntry<string> HideColor { get; private set; }
        public static ConfigEntry<bool> ForceJapanese { get; private set; }
        public static ConfigEntry<bool> JapaneseRoleName { get; private set; }
        public static ConfigEntry<bool> AmDebugger { get; private set; }
        public static ConfigEntry<string> ShowPopUpVersion { get; private set; }
        public static ConfigEntry<float> MessageWait { get; private set; }
        public static ConfigEntry<bool> ButtonImages { get; private set; }

        public static LanguageUnit EnglishLang { get; private set; }
        public static Dictionary<byte, PlayerVersion> playerVersion = new();
        public static Dictionary<byte, string> devNames = new();
        //Other Configs
        public static ConfigEntry<bool> IgnoreWinnerCommand { get; private set; }
        public static ConfigEntry<string> WebhookURL { get; private set; }
        public static ConfigEntry<float> LastKillCooldown { get; private set; }
        public static CustomWinner currentWinner;
        public static HashSet<AdditionalWinners> additionalwinners = new();
        public static IGameOptions RealOptionsData;
        public static Dictionary<byte, string> AllPlayerNames;
        public static Dictionary<(byte, byte), string> LastNotifyNames;
        public static Dictionary<byte, CustomRoles> AllPlayerCustomRoles;
        public static Dictionary<byte, CustomRoles> AllPlayerCustomSubRoles;
        public static Dictionary<byte, CustomRoles> LastPlayerCustomRoles;
        public static Dictionary<byte, CustomRoles> LastPlayerCustomSubRoles;
        public static Dictionary<byte, Color32> PlayerColors = new();
        public static Dictionary<byte, PlayerState.DeathReason> AfterMeetingDeathPlayers = new();
        public static Dictionary<CustomRoles, string> roleColors;
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
        public static bool ExeCanChangeRoles = true;
        public static bool MercCanSuicide = true;
        public static bool DoingYingYang = true;
        public static bool Grenaiding = false;
        public static bool ResetVision = false;
        public static bool IsInvis = false;

        public static Dictionary<byte, CustomRoles> HasModifier = new();
        public static List<CustomRoles> modifiersList = new();
        public static Dictionary<byte, float> AllPlayerKillCooldown = new();
        public static Dictionary<byte, float> AllPlayerSpeed = new();
        public static Dictionary<byte, (byte, float)> BitPlayers = new();
        public static Dictionary<byte, float> WarlockTimer = new();
        public static Dictionary<byte, PlayerControl> CursedPlayers = new();
        public static List<PlayerControl> SpelledPlayer = new();
        public static List<PlayerControl> Impostors = new();
        public static List<byte> DeadPlayersThisRound = new();
        public static Dictionary<byte, bool> KillOrSpell = new();
        public static Dictionary<byte, bool> KillOrSilence = new();
        public static Dictionary<byte, bool> isCurseAndKill = new();
        public static Dictionary<byte, bool> RemoveChat = new();
        public static Dictionary<byte, bool> HasTarget = new();
        public static Dictionary<(byte, byte), bool> isDoused = new();
        public static List<byte> dousedIDs = new();
        public static Dictionary<(byte, byte), bool> isHexed = new();
        public static Dictionary<byte, (PlayerControl, float)> ArsonistTimer = new();
        public static Dictionary<byte, float> AirshipMeetingTimer = new();
        public static Dictionary<byte, byte> ExecutionerTarget = new(); //Key : Executioner, Value : target
        public static Dictionary<byte, byte> GuardianAngelTarget = new(); //Key : GA, Value : target
        public static Dictionary<byte, byte> PuppeteerList = new(); // Key: targetId, Value: PuppeteerId
        public static Dictionary<byte, byte> WitchList = new(); // Key: targetId, Value: NeutWitchId
        public static Dictionary<byte, byte> WitchedList = new(); // Key: targetId, Value: WitchId
        public static Dictionary<byte, byte> CurrentTarget = new(); //Key : Player, Value : Target
        public static Dictionary<byte, byte> SpeedBoostTarget = new();
        public static Dictionary<byte, int> MayorUsedButtonCount = new();
        public static Dictionary<byte, int> HackerFixedSaboCount = new();
        public static Dictionary<byte, Vent> LastEnteredVent = new();
        public static Dictionary<byte, Vector2> LastEnteredVentLocation = new();
        public static int AliveImpostorCount;
        public static int AllImpostorCount;
        public static string LastVotedPlayer;
        public static bool CanTransport;
        public static int HexesThisRound;
        public static int SKMadmateNowCount;
        public static bool witchMeeting;
        public static bool isCursed;
        public static List<byte> firstKill = new();
        public static Dictionary<byte, List<byte>> knownGhosts = new();
        public static Dictionary<byte, (int, bool, bool, bool, bool)> SurvivorStuff = new(); // KEY - player ID, Item1 - NumberOfVests, Item2 - IsVesting, Item3 - HasVested, Item4 - VestedThisRound, Item5 - RoundOneVest
        public static List<byte> unreportableBodies = new();
        public static List<PlayerControl> SilencedPlayer = new();
        public static Dictionary<byte, int> DictatesRemaining = new();
        public static List<byte> ColliderPlayers = new();
        public static List<byte> KilledBewilder = new();
        public static List<byte> KilledDiseased = new();
        public static List<byte> KilledDemo = new();
        public static bool isSilenced;
        public static bool isShipStart;
        public static bool showEjections;
        public static Dictionary<byte, bool> CheckShapeshift = new();
        public static Dictionary<(byte, byte), string> targetArrows = new();
        public static List<PlayerControl> AllCovenPlayers = new();
        public static Dictionary<byte, byte> whoKilledWho = new();
        public static int WonFFATeam;
        public static byte WonTrollID;
        public static byte ExiledJesterID;
        public static byte WonTerroristID;
        public static byte WonPirateID;
        public static byte WonExecutionerID;
        public static byte WonHackerID;
        public static byte WonArsonistID;
        public static byte WonChildID;
        public static byte WonFFAid;
        public static bool CustomWinTrigger;
        public static bool VisibleTasksCount;
        public static string nickName = "";
        public static bool introDestroyed = false;
        public static bool bkProtected = false;
        public static bool devIsHost = false;
        public static int DiscussionTime;
        public static int VotingTime;
        public static int JugKillAmounts;
        public static int AteBodies;
        public static byte currentDousingTarget;
        public static byte currentFreezingTarget;
        public static int VetAlerts;
        public static int TransportsLeft;
        public static bool IsRoundOne;

        //plague info.
        public static byte currentInfectingTarget;
        public static Dictionary<(byte, byte), bool> isInfected = new();
        public static Dictionary<byte, (PlayerControl, float)> PlagueBearerTimer = new();
        public static List<int> bombedVents = new();
        public static Dictionary<byte, (byte, bool)> SleuthReported = new();
        public static Dictionary<AmongUsExtensions.OptionType, List<CustomOption>> Options = new();

        public static bool JackalDied;

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
        public static bool CanGoInvis;

        // VETERAN STUFF //
        public static bool VettedThisRound;
        public static bool VetIsAlerted;
        public static bool VetCanAlert;

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
        public static bool MareHasRedName;

        // NEUTRALS //
        public static bool IsRampaged;
        public static bool RampageReady;
        public static bool IsHackMode;
        public static bool PhantomCanBeKilled;
        public static bool PhantomAlert;

        // TRULY RANDOM ROLES TEST //
        public static List<CustomRoles> chosenRoles = new();
        public static List<CustomRoles> chosenImpRoles = new();
        public static List<CustomRoles> chosenDesyncRoles = new();
        public static List<CustomRoles> chosenNK = new(); // ROLE : Value -- IsShapeshifter -- Key
        public static List<CustomRoles> chosenNonNK = new();

        // specific roles //
        public static List<CustomRoles> chosenEngiRoles = new();
        public static List<CustomRoles> chosenScientistRoles = new();
        public static List<CustomRoles> chosenShifterRoles = new();
        public static List<byte> rolesRevealedNextMeeting = new();
        public static Dictionary<byte, bool> CleanerCanClean = new();
        public static List<byte> IsShapeShifted = new();
        public static Dictionary<byte, int> PickpocketKills = new();
        public static Dictionary<byte, int> KillCount = new();
        public static List<byte> KillingSpree = new();

        public static int MarksmanKills = 0;
        public static bool FirstMeetingOccured = false;

        public static Dictionary<byte, int> lastAmountOfTasks = new(); // PLayerID : Value ---- AMOUNT : KEY
        public static Dictionary<byte, (int, string, string, string, string, string)> AllPlayerSkin = new(); //Key : PlayerId, Value : (1: color, 2: hat, 3: skin, 4:visor, 5: pet)
        // SPRIES //
        public static Sprite AlertSprite;
        public static Sprite DouseSprite;
        public static Sprite HackSprite;
        public static Sprite IgniteSprite;
        public static Sprite InfectSprite;
        public static Sprite MimicSprite;
        public static Sprite PoisonSprite;
        public static Sprite ProtectSprite;
        public static Sprite RampageSprite;
        public static Sprite RememberSprite;
        public static Sprite SeerSprite;
        public static Sprite SheriffSprite;
        public static Sprite VestSprite;
        public static Sprite CleanSprite;
        public static Sprite TransportSprite;
        public static Sprite FlashSprite;
        public static Sprite PoisonedSprite;
        public static Sprite MediumSprite;
        public static Sprite BlackmailSprite;
        public static Sprite MinerSprite;
        public static Sprite TargetSprite;
        public static Sprite AssassinateSprite;
        public static int WitchesThisRound = 0;
        public static string LastWinner = "None";

        public static AmongUsExtensions.OptionType currentType;
        // 31628
        public static bool FirstMeetingPassed = false;
        // ATTACK AND DEFENSE STUFF //
        public static Dictionary<CustomRoles, AttackEnum> attackValues;
        public static Dictionary<CustomRoles, DefenseEnum> defenseValues;
        public static List<byte> unvotablePlayers = new();

        // SPECIAL STUFF
        public static bool IsChristmas = DateTime.Now.Month == 12 && DateTime.Now.Day is 24 or 25;
        public static bool IsInitialRelease = DateTime.Now.Month == 8 && DateTime.Now.Day is 18;
        public static bool NewYears = (DateTime.Now.Month == 12 && DateTime.Now.Day is 31) || (DateTime.Now.Month == 1 && DateTime.Now.Day is 1);
        public override void Load()
        {
            Instance = this;
            SoundEffectsManager.Load();

            TextCursorTimer = 0f;
            TextCursorVisible = true;

            //Client Options
            HideName = Config.Bind("Client Options", "Hide Game Code Name", "Town Of Host");
            HideColor = Config.Bind("Client Options", "Hide Game Code Color", $"{modColor}");
            ForceJapanese = Config.Bind("Client Options", "Force Japanese", false);
            JapaneseRoleName = Config.Bind("Client Options", "Japanese Role Name", true);
            ButtonImages = Config.Bind("Client Options", "Custom Button Images", false);
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
            LastPlayerCustomRoles = new Dictionary<byte, CustomRoles>();
            LastPlayerCustomSubRoles = new Dictionary<byte, CustomRoles>();
            CustomWinTrigger = false;
            BitPlayers = new Dictionary<byte, (byte, float)>();
            SurvivorStuff = new Dictionary<byte, (int, bool, bool, bool, bool)>();
            WarlockTimer = new Dictionary<byte, float>();
            CursedPlayers = new Dictionary<byte, PlayerControl>();
            RemoveChat = new Dictionary<byte, bool>();
            SpelledPlayer = new List<PlayerControl>();
            Impostors = new List<PlayerControl>();
            rolesRevealedNextMeeting = new List<byte>();
            SilencedPlayer = new List<PlayerControl>();
            FirstMeetingPassed = false;
            LastWinner = "None";
            ColliderPlayers = new List<byte>();
            WitchesThisRound = 0;
            CleanerCanClean = new Dictionary<byte, bool>();
            HasTarget = new Dictionary<byte, bool>();
            isDoused = new Dictionary<(byte, byte), bool>();
            isHexed = new Dictionary<(byte, byte), bool>();
            isInfected = new Dictionary<(byte, byte), bool>();
            VetCanAlert = true;
            currentType = AmongUsExtensions.OptionType.None;
            ArsonistTimer = new Dictionary<byte, (PlayerControl, float)>();
            PlagueBearerTimer = new Dictionary<byte, (PlayerControl, float)>();
            ExecutionerTarget = new Dictionary<byte, byte>();
            GuardianAngelTarget = new Dictionary<byte, byte>();
            MayorUsedButtonCount = new Dictionary<byte, int>();
            HackerFixedSaboCount = new Dictionary<byte, int>();
            LastEnteredVent = new Dictionary<byte, Vent>();
            knownGhosts = new Dictionary<byte, List<byte>>();
            LastEnteredVentLocation = new Dictionary<byte, Vector2>();
            CurrentTarget = new Dictionary<byte, byte>();
            WitchList = new Dictionary<byte, byte>();
            HasModifier = new Dictionary<byte, CustomRoles>();
            // /DeadPlayersThisRound = new List<byte>();
            LoversPlayers = new List<PlayerControl>();
            dousedIDs = new List<byte>();
            //firstKill = new Dictionary<byte, (PlayerControl, float)>();
            winnerList = new List<byte>();
            KillingSpree = new List<byte>();
            unvotablePlayers = new();
            VisibleTasksCount = false;
            MercCanSuicide = true;
            devIsHost = false;
            ExeCanChangeRoles = true;
            MessagesToSend = new List<(string, byte)>();
            currentDousingTarget = 255;
            currentFreezingTarget = 255;
            currentInfectingTarget = 255;
            JugKillAmounts = 0;
            AteBodies = 0;
            MarksmanKills = 0;
            CovenMeetings = 0;
            GAprotects = 0;
            CanTransport = true;
            PickpocketKills = new Dictionary<byte, int>();
            KillCount = new Dictionary<byte, int>();
            ProtectedThisRound = false;
            HasProtected = false;
            VetAlerts = 0;
            TransportsLeft = 0;
            ProtectsSoFar = 0;
            IsProtected = false;
            ResetVision = false;
            Grenaiding = false;
            DoingYingYang = true;
            VettedThisRound = false;
            MareHasRedName = false;
            WitchProtected = false;
            HexMasterOn = false;
            PotionMasterOn = false;
            VampireDitchesOn = false;
            IsShapeShifted = new List<byte>();
            MedusaOn = false;
            MimicOn = false;
            FirstMeetingOccured = false;
            NecromancerOn = false;
            ConjurorOn = false;
            ChoseWitch = false;
            HasNecronomicon = false;
            VetIsAlerted = false;
            IsRoundOne = false;
            IsRoundOneGA = false;
            showEjections = false;

            IsRampaged = false;
            IsInvis = false;
            CanGoInvis = false;
            RampageReady = false;

            IsHackMode = false;
            GazeReady = true;
            IsGazing = false;
            CamoComms = false;
            HexesThisRound = 0;
            JackalDied = false;
            LastVotedPlayer = "";
            bkProtected = false;
            AlertSprite = Helpers.LoadSpriteFromResourcesTOR("TownOfHost.Resources.Alert.png", 100f);
            DouseSprite = Helpers.LoadSpriteFromResourcesTOR("TownOfHost.Resources.Douse.png", 100f);
            HackSprite = Helpers.LoadSpriteFromResourcesTOR("TownOfHost.Resources.Hack.png", 100f);
            IgniteSprite = Helpers.LoadSpriteFromResourcesTOR("TownOfHost.Resources.Ignite.png", 100f);
            InfectSprite = Helpers.LoadSpriteFromResourcesTOR("TownOfHost.Resources.Infect.png", 100f);
            MimicSprite = Helpers.LoadSpriteFromResourcesTOR("TownOfHost.Resources.Mimic.png", 100f);
            PoisonSprite = Helpers.LoadSpriteFromResourcesTOR("TownOfHost.Resources.Poison.png", 100f);
            ProtectSprite = Helpers.LoadSpriteFromResourcesTOR("TownOfHost.Resources.Protect.png", 100f);
            RampageSprite = Helpers.LoadSpriteFromResourcesTOR("TownOfHost.Resources.Rampage.png", 100f);
            RememberSprite = Helpers.LoadSpriteFromResourcesTOR("TownOfHost.Resources.Remember.png", 100f);
            SeerSprite = Helpers.LoadSpriteFromResourcesTOR("TownOfHost.Resources.Seer.png", 100f);
            SheriffSprite = Helpers.LoadSpriteFromResourcesTOR("TownOfHost.Resources.Sheriff.png", 100f);
            VestSprite = Helpers.LoadSpriteFromResourcesTOR("TownOfHost.Resources.Vest.png", 100f);
            CleanSprite = Helpers.LoadSpriteFromResourcesTOR("TownOfHost.Resources.Janitor.png", 100f);
            TransportSprite = Helpers.LoadSpriteFromResourcesTOR("TownOfHost.Resources.Transport.png", 100f);
            FlashSprite = Helpers.LoadSpriteFromResourcesTOR("TownOfHost.Resources.Flash.png", 100f);
            PoisonedSprite = Helpers.LoadSpriteFromResourcesTOR("TownOfHost.Resources.Poisoned.png", 100f);
            BlackmailSprite = Helpers.LoadSpriteFromResourcesTOR("TownOfHost.Resources.Blackmail.png", 100f);
            MediumSprite = Helpers.LoadSpriteFromResourcesTOR("TownOfHost.Resources.Mediate.png", 100f);
            MinerSprite = Helpers.LoadSpriteFromResourcesTOR("TownOfHost.Resources.Mine.png", 100f);
            TargetSprite = Helpers.LoadSpriteFromResourcesTOR("TownOfHost.Resources.NinjaMarkButton.png", 100f);
            AssassinateSprite = Helpers.LoadSpriteFromResourcesTOR("TownOfHost.Resources.NinjaAssassinateButton.png", 100f);

            // OTHER//

            TeamJuggernautAlive = false;
            TeamPestiAlive = false;
            TeamCovenAlive = 3;
            PhantomAlert = false;
            PhantomCanBeKilled = false;

            IgnoreWinnerCommand = Config.Bind("Other", "IgnoreWinnerCommand", true);
            WebhookURL = Config.Bind("Other", "WebhookURL", "none");
            AmDebugger = Config.Bind("Other", "AmDebugger", true);
            AmDebugger.Value = false;
            ShowPopUpVersion = Config.Bind("Other", "ShowPopUpVersion", "0");
            MessageWait = Config.Bind("Other", "MessageWait", 1f);
            LastKillCooldown = Config.Bind("Other", "LastKillCooldown", (float)30);

            NameColorManager.Begin();

            Translator.Init();

            hasArgumentException = false;
            AllPlayerSkin = new();
            unreportableBodies = new();
            ExceptionMessage = "";
            try
            {

                roleColors = new Dictionary<CustomRoles, string>()
                {
                    //バニラ役職
                    {CustomRoles.Crewmate, "#ffffff"},
                    {CustomRoles.Engineer, "#b6f0ff"},
                    {CustomRoles.CrewmateGhost, "#ffffff"},
                    { CustomRoles.Scientist, "#b6f0ff"},
                    { CustomRoles.Mechanic, "#FFA60A"},
                    { CustomRoles.Physicist, "#b6f0ff"},
                    { CustomRoles.GuardianAngel, "#ffffff"},
                    { CustomRoles.Target, "#000000"},
                    { CustomRoles.CorruptedSheriff, "#ff0000"},
                    { CustomRoles.Watcher, "#800080"},
                    { CustomRoles.NiceGuesser, "#E4E085"},
                    { CustomRoles.Pirate, "#EDC240"},
                    //特殊クルー役職
                    { CustomRoles.Bait, "#00B3B3"},
                    { CustomRoles.SabotageMaster, "#0000ff"},
                    { CustomRoles.Snitch, "#b8fb4f"},
                    { CustomRoles.Mayor, "#204d42"},
                    { CustomRoles.Sheriff, "#f8cd46"},
                    { CustomRoles.Investigator, "#ffca81"},
                    { CustomRoles.Lighter, "#eee5be"},
                    //{ CustomRoles.Bodyguard, "#7FFF00"},
                    //{ CustomRoles.Oracle, "#0042FF"},
                    { CustomRoles.Bodyguard, "#5d5d5d"},
                    { CustomRoles.Oracle, "#c88dd0"},
                    { CustomRoles.Medic, "#006600"},
                    { CustomRoles.SpeedBooster, "#00ffff"},
                    { CustomRoles.Mystic, "#4D99E6"},
                    { CustomRoles.Swapper, "#66E666"},
                    { CustomRoles.Transporter, "#00EEFF"},
                    { CustomRoles.Doctor, "#80ffdd"},
                    { CustomRoles.Child, "#FFFFFF"},
                    { CustomRoles.Trapper, "#5a8fd0"},
                    { CustomRoles.Dictator, "#df9b00"},
                    { CustomRoles.Sleuth, "#803333"},
                    { CustomRoles.Crusader, "#c65c39"},
                    { CustomRoles.Escort, "#ffb9eb"},
                    { CustomRoles.PlagueBearer, "#E6FFB3"},
                    { CustomRoles.Pestilence, "#393939"},
                    { CustomRoles.Vulture, "#a36727"},
                    { CustomRoles.CSchrodingerCat, "#ffffff"}, //シュレディンガーの猫の派生
                    { CustomRoles.Medium, "#A680FF"},
                    { CustomRoles.Alturist, "#660000"},
                    { CustomRoles.Psychic, "#6F698C"},
                    //第三陣営役職
                    { CustomRoles.Arsonist, "#ff6633"},
                    { CustomRoles.Jester, "#ec62a5"},
                    { CustomRoles.Terrorist, "#00ff00"},
                    { CustomRoles.Executioner, "#C96600"},
                    { CustomRoles.Opportunist, "#00ff00"},
                    { CustomRoles.Survivor, "#FFE64D"},
                    { CustomRoles.AgiTater, "#F4A460"},
                    { CustomRoles.PoisonMaster, "#ed2f91"},
                    { CustomRoles.SchrodingerCat, "#696969"},
                    { CustomRoles.Egoist, "#5600ff"},
                    { CustomRoles.EgoSchrodingerCat, "#5600ff"},
                    { CustomRoles.Jackal, "#00b4eb"},
                    { CustomRoles.Sidekick, "#00b4eb"},
                    { CustomRoles.Marksman, "#440101"},
                    { CustomRoles.Juggernaut, "#670038"},
                    { CustomRoles.JSchrodingerCat, "#00b4eb"},
                    { CustomRoles.Phantom, "#662962"},
                    { CustomRoles.NeutWitch, "#592e98"},
                    { CustomRoles.Hitman, "#ce1924"},
                    //HideAndSeek
                    { CustomRoles.HASFox, "#e478ff"},
                    { CustomRoles.BloodKnight, "#630000"},
                    { CustomRoles.HASTroll, "#00ff00"},
                    { CustomRoles.Painter, "#FF5733"},
                    { CustomRoles.Janitor, "#c67051"},
                    { CustomRoles.Supporter, "#00b4eb"},
                    { CustomRoles.Tasker, "#2c68dc"},
                    // GM
                    { CustomRoles.GM, "#ff5b70"},
                    //サブ役職
                    { CustomRoles.NoSubRoleAssigned, "#ffffff"},
                    { CustomRoles.Lovers, "#FF66CC"},
                    { CustomRoles.LoversRecode, "#FF66CC"},
                    { CustomRoles.LoversWin, "#FF66CC"},
                    { CustomRoles.Flash, "#FF8080"},
                    { CustomRoles.Oblivious, "#808080"},
                    { CustomRoles.DoubleShot, "#ff0000"},
                    { CustomRoles.Torch, "#FFFF99"},
                    { CustomRoles.Diseased, "#AAAAAA"},
                    { CustomRoles.TieBreaker, "#99E699"},
                    { CustomRoles.Obvious, "#D3D3D3"},
                    { CustomRoles.Escalation, "#FFB34D"},

                    { CustomRoles.Coven, "#bd5dfd"},
                    { CustomRoles.Veteran, "#998040"},
                    { CustomRoles.GuardianAngelTOU, "#B3FFFF"},
                    { CustomRoles.TheGlitch, "#00FF00"},
                    { CustomRoles.Werewolf, "#A86629"},
                    { CustomRoles.Amnesiac, "#81DDFC"},
                    { CustomRoles.Bewilder, "#292644"},
                    { CustomRoles.Demolitionist, "#5e2801"},
                    { CustomRoles.Bastion, "#524f4d"},
                    { CustomRoles.Hacker, "#358013"},
                    { CustomRoles.CrewPostor, "#DC6601"},

                    { CustomRoles.CPSchrodingerCat, "#DC6601"},
                    { CustomRoles.TGSchrodingerCat, "#00FF00"},
                    { CustomRoles.WWSchrodingerCat, "#A86629"},
                    { CustomRoles.JugSchrodingerCat, "#670038"},
                    { CustomRoles.MMSchrodingerCat, "#440101"},
                    { CustomRoles.PesSchrodingerCat, "#393939"},
                    { CustomRoles.BKSchrodingerCat, "#630000"},

                    // TAGS //
                   //TEXT COLORS ROSIE
                    { CustomRoles.sns1, "#FFF9DB"},
                    { CustomRoles.sns2, "#FBE0E2"},
                    { CustomRoles.sns3, "#F6C6E8"},
                    { CustomRoles.sns4, "#F2ADEE"},
                    { CustomRoles.sns5, "#ED93F4"},
                    { CustomRoles.sns6, "#DDA1EE"},
                    { CustomRoles.sns7, "#CCAEE8"},
                    { CustomRoles.sns8, "#AAC9DB"},
                    { CustomRoles.sns9, "#88E4CF"},
                    { CustomRoles.sns10, "#66FFC2"},
                    { CustomRoles.rosecolor, "#FFD6EC"},
                    // MISC //
                    { CustomRoles.eevee, "#FF8D1C"},
                    { CustomRoles.serverbooster, "#f47fff"},
                    { CustomRoles.thetaa, "#9A9AEB"},
                    // SELF//
                    { CustomRoles.minaa, "#C8A2C8"},
                    { CustomRoles.ess, "#EAC4FB"},

                    //TEXT COLORS Candy
                    { CustomRoles.psh1, "#EF807F"},
                    { CustomRoles.psh2, "#F3969C"},
                    { CustomRoles.psh3, "#F7ABB8"},
                    { CustomRoles.psh4, "#FBC1D5"},
                    { CustomRoles.psh5, "#FBC6E9"},
                    { CustomRoles.psh6, "#F6B6E0"},
                    { CustomRoles.psh7, "#F4AEDC"},
                    { CustomRoles.psh8, "#F1A6D7"},
                    { CustomRoles.psh9, "#EC96CE"},
                    //GURGE44
                    { CustomRoles.gu1, "#FFFF47" },
                    { CustomRoles.gu2, "#FFEE42" },
                    { CustomRoles.gu3, "#FFDC3D" },
                    { CustomRoles.gu4, "#FFCA38" },
                    { CustomRoles.gu5, "#FFC136" },
                    { CustomRoles.gu6, "#FFB833" },
                    { CustomRoles.gu7, "#FF9733" },
                    { CustomRoles.gu8, "#FF7633" },
                    { CustomRoles.gu9, "#FF5533" },
                    { CustomRoles.gu10, "#FF3333" },
                    //Pineappleman
                    { CustomRoles.pi1, "#FFFF33" },
                    { CustomRoles.pi2, "#F3FF33" },
                    { CustomRoles.pi3, "#E6FF33" },
                    { CustomRoles.pi4, "#CCFF33" },
                    { CustomRoles.pi5, "#99FF33" },
                    { CustomRoles.pi6, "#33FF33" },
                    { CustomRoles.pi7, "#27E527" },
                    { CustomRoles.pi8, "#1ACB1A" },
                    { CustomRoles.pi9, "#0DB10D" },
                    { CustomRoles.pi10, "#009600" },
                    //yoclobo
                    { CustomRoles.yo1, "#FF8674" },
                    { CustomRoles.yo2, "#FFA588" },
                    { CustomRoles.yo3, "#FFC39B" },
                    { CustomRoles.yo4, "#FFE1AF" },
                    { CustomRoles.yo5, "#FFF0B9" },
                    { CustomRoles.yo6, "#FFFFC2" },
                    { CustomRoles.yo7, "#FFE1AF" },
                    { CustomRoles.yo8, "#FFC39B" },
                    { CustomRoles.yo9, "#FFA588" },
                    { CustomRoles.yo10, "#FF8674" },
                    //Nicky G
                    { CustomRoles.ni1, "#CC0000" },
                    { CustomRoles.ni2, "#B30000" },
                    { CustomRoles.ni3, "#990000" },
                    { CustomRoles.ni4, "#660000" },
                    { CustomRoles.ni5, "#330000" },
                    { CustomRoles.ni6, "#1A0000" },
                    { CustomRoles.ni7, "#000000" },
                    //Milk
                    { CustomRoles.ml1, "#FBD5EE" },
                    { CustomRoles.ml2, "#FAC5E7" },
                    { CustomRoles.ml3, "#F9B4E0" },
                    { CustomRoles.ml4, "#F9ACDD" },
                    { CustomRoles.ml5, "#F8A3D9" },
                    { CustomRoles.ml6, "#F692D2" },
                    { CustomRoles.ml7, "#F582CB" },
                    { CustomRoles.ml8, "#F471C4" },
                    { CustomRoles.ml9, "#F360BD" },
                    { CustomRoles.ml10, "#F14FB6" },
                    //PAIGE
                    { CustomRoles.pg1, "#FFECE5" },
                    { CustomRoles.pg2, "#FFDFD3" },
                    { CustomRoles.pg3, "#FFD1C0" },
                    { CustomRoles.pg4, "#FFC4AE" },
                    { CustomRoles.pg5, "#FFB69B" },
                    { CustomRoles.pg6, "#FFA989" },
                    { CustomRoles.pg7, "#FF9B76" },
                    { CustomRoles.pg8, "#FF8D63" },
                    { CustomRoles.pg9, "#FF865A" },
                    { CustomRoles.pg10, "#FF7F50" },
                    // ck
                    { CustomRoles.cc1, "#FF3636" },
                    { CustomRoles.cc2, "#FF4636" },
                    { CustomRoles.cc3, "#FF5536" },
                    { CustomRoles.cc4, "#FE7335" },
                    { CustomRoles.cc5, "#FD9135" },
                    { CustomRoles.cc6, "#FCAF34" },
                    { CustomRoles.cc7, "#FCC137" },
                    { CustomRoles.cc8, "#FCD239" },
                    { CustomRoles.cc9, "#FCE43B" },
                    { CustomRoles.cc10, "#FCF53D" },
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
                            roleColors.TryAdd(role, "#bd5dfd");
                            break;
                        default:
                            break;
                    }
                }
                attackValues = new Dictionary<CustomRoles, AttackEnum>()
                {
                    {CustomRoles.Crewmate, AttackEnum.None},
                    {CustomRoles.Engineer, AttackEnum.None},
                    { CustomRoles.Scientist, AttackEnum.None},
                    { CustomRoles.Mechanic, AttackEnum.None},
                    { CustomRoles.Physicist, AttackEnum.None},
                    { CustomRoles.GuardianAngel, AttackEnum.None},
                    { CustomRoles.Target, AttackEnum.None},
                    { CustomRoles.Watcher, AttackEnum.None},
                    { CustomRoles.NiceGuesser, AttackEnum.Powerful},
                    { CustomRoles.Pirate, AttackEnum.Powerful},
                    { CustomRoles.Bait, AttackEnum.None},
                    { CustomRoles.SabotageMaster, AttackEnum.None},
                    { CustomRoles.Snitch, AttackEnum.None},
                    { CustomRoles.Mayor, AttackEnum.None},
                    { CustomRoles.Sheriff, AttackEnum.Basic},
                    { CustomRoles.Investigator, AttackEnum.None},
                    { CustomRoles.Lighter, AttackEnum.None},
                    { CustomRoles.Bodyguard, AttackEnum.Powerful},
                    { CustomRoles.Oracle, AttackEnum.None},
                    { CustomRoles.Medic, AttackEnum.None},
                    { CustomRoles.SpeedBooster, AttackEnum.None},
                    { CustomRoles.Mystic, AttackEnum.None},
                    { CustomRoles.Swapper, AttackEnum.None},
                    { CustomRoles.Transporter, AttackEnum.None},
                    { CustomRoles.Doctor, AttackEnum.None},
                    { CustomRoles.Child, AttackEnum.Unblockable},
                    { CustomRoles.Trapper, AttackEnum.None},
                    { CustomRoles.Dictator, AttackEnum.Unblockable},
                    { CustomRoles.Sleuth ,AttackEnum.None},
                    { CustomRoles.Crusader, AttackEnum.Powerful},
                    { CustomRoles.Escort, AttackEnum.None},
                    { CustomRoles.PlagueBearer, AttackEnum.None},
                    { CustomRoles.Pestilence, AttackEnum.Powerful},
                    { CustomRoles.Vulture, AttackEnum.None},
                    { CustomRoles.CSchrodingerCat, AttackEnum.None},
                    { CustomRoles.Medium, AttackEnum.None},
                    { CustomRoles.Alturist, AttackEnum.None},
                    { CustomRoles.Psychic, AttackEnum.None},
                    { CustomRoles.Arsonist, AttackEnum.Powerful},
                    { CustomRoles.Jester, AttackEnum.None},
                    { CustomRoles.Terrorist, AttackEnum.Unblockable},
                    { CustomRoles.Executioner, AttackEnum.None},
                    { CustomRoles.Opportunist, AttackEnum.None},
                    { CustomRoles.Survivor, AttackEnum.None},
                    { CustomRoles.AgiTater, AttackEnum.Powerful},
                    { CustomRoles.PoisonMaster, AttackEnum.Basic},
                    { CustomRoles.SchrodingerCat, AttackEnum.None},
                    { CustomRoles.Egoist, AttackEnum.Basic},
                    { CustomRoles.EgoSchrodingerCat, AttackEnum.None},
                    { CustomRoles.Jackal, AttackEnum.Basic},
                    { CustomRoles.Sidekick, AttackEnum.Basic},
                    { CustomRoles.Marksman, AttackEnum.Basic},
                    { CustomRoles.Juggernaut, AttackEnum.Powerful},
                    { CustomRoles.JSchrodingerCat, AttackEnum.None},
                    { CustomRoles.Phantom, AttackEnum.None},
                    { CustomRoles.NeutWitch, AttackEnum.None},
                    { CustomRoles.Hitman, AttackEnum.Basic},
                    { CustomRoles.BloodKnight, AttackEnum.Powerful},
                    { CustomRoles.Veteran, AttackEnum.Powerful},
                    { CustomRoles.GuardianAngelTOU, AttackEnum.None},
                    { CustomRoles.TheGlitch, AttackEnum.Basic},
                    { CustomRoles.Werewolf, AttackEnum.Powerful},
                    { CustomRoles.Amnesiac, AttackEnum.None},
                    { CustomRoles.Demolitionist, AttackEnum.Unstoppable},
                    { CustomRoles.Bastion, AttackEnum.Unstoppable},
                    { CustomRoles.Hacker, AttackEnum.None},
                    { CustomRoles.CrewPostor, AttackEnum.Basic},
                    { CustomRoles.CPSchrodingerCat, AttackEnum.None},
                    { CustomRoles.TGSchrodingerCat, AttackEnum.None},
                    { CustomRoles.WWSchrodingerCat, AttackEnum.None},
                    { CustomRoles.JugSchrodingerCat,AttackEnum.None},
                    { CustomRoles.MMSchrodingerCat, AttackEnum.None},
                    { CustomRoles.PesSchrodingerCat, AttackEnum.None},
                    { CustomRoles.BKSchrodingerCat, AttackEnum.None},
                };
                foreach (var role in Enum.GetValues(typeof(CustomRoles)).Cast<CustomRoles>())
                {
                    switch (role.GetRoleType())
                    {
                        case RoleType.Impostor:
                            attackValues.Add(role, AttackEnum.Basic);
                            break;
                        case RoleType.Madmate:
                            if (role == CustomRoles.Parasite)
                                attackValues.Add(role, AttackEnum.Basic);
                            else
                                attackValues.Add(role, AttackEnum.None);
                            break;
                        case RoleType.Coven:
                            attackValues.Add(role, AttackEnum.Basic);
                            break;
                        default:
                            if (!attackValues.ContainsKey(role))
                                attackValues.Add(role, AttackEnum.None);
                            break;
                    }
                }
                defenseValues = new Dictionary<CustomRoles, DefenseEnum>()
                {
                    {CustomRoles.Crewmate, DefenseEnum.None},
                    {CustomRoles.Engineer, DefenseEnum.None},
                    { CustomRoles.Scientist, DefenseEnum.None},
                    { CustomRoles.Mechanic, DefenseEnum.None},
                    { CustomRoles.Physicist, DefenseEnum.None},
                    { CustomRoles.GuardianAngel, DefenseEnum.None},
                    { CustomRoles.Target, DefenseEnum.None},
                    { CustomRoles.Watcher, DefenseEnum.None},
                    { CustomRoles.NiceGuesser, DefenseEnum.None},
                    { CustomRoles.Pirate, DefenseEnum.Basic},
                    { CustomRoles.Bait, DefenseEnum.None},
                    { CustomRoles.SabotageMaster, DefenseEnum.None},
                    { CustomRoles.Snitch, DefenseEnum.None},
                    { CustomRoles.Mayor, DefenseEnum.None},
                    { CustomRoles.Sheriff, DefenseEnum.None},
                    { CustomRoles.Investigator, DefenseEnum.None},
                    { CustomRoles.Lighter, DefenseEnum.None},
                    { CustomRoles.Bodyguard, DefenseEnum.Basic},
                    { CustomRoles.Oracle, DefenseEnum.None},
                    { CustomRoles.Medic, DefenseEnum.None},
                    { CustomRoles.SpeedBooster, DefenseEnum.None},
                    { CustomRoles.Mystic, DefenseEnum.None},
                    { CustomRoles.Swapper, DefenseEnum.None},
                    { CustomRoles.Transporter, DefenseEnum.None},
                    { CustomRoles.Doctor, DefenseEnum.None},
                    { CustomRoles.Child, DefenseEnum.None},
                    { CustomRoles.Trapper, DefenseEnum.None},
                    { CustomRoles.Dictator, DefenseEnum.None},
                    { CustomRoles.Sleuth ,DefenseEnum.None},
                    { CustomRoles.Crusader, DefenseEnum.None},
                    { CustomRoles.Escort, DefenseEnum.None},
                    { CustomRoles.PlagueBearer, DefenseEnum.None},
                    { CustomRoles.Pestilence, DefenseEnum.Invincible},
                    { CustomRoles.Vulture, DefenseEnum.None},
                    { CustomRoles.CSchrodingerCat, DefenseEnum.None},
                    { CustomRoles.Medium, DefenseEnum.None},
                    { CustomRoles.Alturist, DefenseEnum.None},
                    { CustomRoles.Psychic, DefenseEnum.None},
                    { CustomRoles.Arsonist, DefenseEnum.Basic},
                    { CustomRoles.Jester, DefenseEnum.None},
                    { CustomRoles.Terrorist, DefenseEnum.None},
                    { CustomRoles.Executioner, DefenseEnum.Basic},
                    { CustomRoles.Opportunist, DefenseEnum.None},
                    { CustomRoles.Survivor, DefenseEnum.None},
                    { CustomRoles.AgiTater, DefenseEnum.Basic},
                    { CustomRoles.PoisonMaster, DefenseEnum.Basic},
                    { CustomRoles.SchrodingerCat, DefenseEnum.None},
                    { CustomRoles.Egoist, DefenseEnum.None},
                    { CustomRoles.EgoSchrodingerCat, DefenseEnum.None},
                    { CustomRoles.Jackal, DefenseEnum.Basic},
                    { CustomRoles.Sidekick, DefenseEnum.Basic},
                    { CustomRoles.Marksman, DefenseEnum.Basic},
                    { CustomRoles.Juggernaut, DefenseEnum.Basic},
                    { CustomRoles.JSchrodingerCat, DefenseEnum.None},
                    { CustomRoles.Phantom, DefenseEnum.None},
                    { CustomRoles.NeutWitch, DefenseEnum.None},
                    { CustomRoles.Hitman, DefenseEnum.None},
                    { CustomRoles.BloodKnight, DefenseEnum.None},
                    { CustomRoles.Veteran, DefenseEnum.None},
                    { CustomRoles.GuardianAngelTOU, DefenseEnum.None},
                    { CustomRoles.TheGlitch, DefenseEnum.None},
                    { CustomRoles.Werewolf, DefenseEnum.Basic},
                    { CustomRoles.Amnesiac, DefenseEnum.None},
                    { CustomRoles.Demolitionist, DefenseEnum.None},
                    { CustomRoles.Bastion, DefenseEnum.Basic},
                    { CustomRoles.Hacker, DefenseEnum.None},
                    { CustomRoles.CrewPostor, DefenseEnum.None},
                    { CustomRoles.CPSchrodingerCat, DefenseEnum.None},
                    { CustomRoles.TGSchrodingerCat, DefenseEnum.None},
                    { CustomRoles.WWSchrodingerCat, DefenseEnum.None},
                    { CustomRoles.JugSchrodingerCat,DefenseEnum.None},
                    { CustomRoles.MMSchrodingerCat, DefenseEnum.None},
                    { CustomRoles.PesSchrodingerCat, DefenseEnum.None},
                    { CustomRoles.BKSchrodingerCat, DefenseEnum.None},
                };
                foreach (var role in Enum.GetValues(typeof(CustomRoles)).Cast<CustomRoles>())
                {
                    switch (role.GetRoleType())
                    {
                        case RoleType.Impostor:
                            defenseValues.Add(role, DefenseEnum.None);
                            break;
                        case RoleType.Madmate:
                            defenseValues.Add(role, DefenseEnum.None);
                            break;
                        case RoleType.Coven:
                            if (role == CustomRoles.CovenWitch)
                                defenseValues.Add(role, DefenseEnum.Basic);
                            else
                                defenseValues.Add(role, DefenseEnum.None);
                            break;
                        default:
                            if (!defenseValues.ContainsKey(role))
                                defenseValues.Add(role, DefenseEnum.None);
                            break;
                    }
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

            if (!File.Exists(TEMPLATE_FILE_PATH))
            {
                try
                {
                    if (!Directory.Exists(@"TOR_DATA")) Directory.CreateDirectory(@"TOR_DATA");
                    if (File.Exists(@"./template.txt"))
                    {
                        File.Move(@"./template.txt", TEMPLATE_FILE_PATH);
                    }
                    else
                    {
                        TownOfHost.Logger.Info("No template.txt file found.", "TemplateManager");
                        File.WriteAllText(TEMPLATE_FILE_PATH, "test:This is template text.\\nLine breaks are also possible.\ntest:これは定型文です。\\n改行も可能です。");
                    }
                }
                catch (Exception ex)
                {
                    TownOfHost.Logger.Exception(ex, "TemplateManager");
                }
            }
            if (!File.Exists(BANNEDWORDS_FILE_PATH))
            {
                try
                {
                    if (!Directory.Exists(@"TOR_DATA")) Directory.CreateDirectory(@"TOR_DATA");
                    if (File.Exists(@"./bannedwords.txt"))
                    {
                        File.Move(@"./bannedwords.txt", BANNEDWORDS_FILE_PATH);
                    }
                    else
                    {
                        TownOfHost.Logger.Info("No bannedwords.txt file found.", "BannedWordsManager");
                        File.WriteAllText(BANNEDWORDS_FILE_PATH, $"Enter banned words here. Note, the game will take each message and turn each character into the lowercase version. So no need to include every variation of one word.");
                    }
                }
                catch (Exception ex)
                {
                    TownOfHost.Logger.Exception(ex, "TemplateManager");
                }
            }
            if (!File.Exists(BANNEDFRIENDCODES_FILE_PATH))
            {
                try
                {
                    if (!Directory.Exists(@"TOR_DATA")) Directory.CreateDirectory(@"TOR_DATA");
                    if (File.Exists(@"./bannedfriendcodes.txt"))
                    {
                        File.Move(@"./bannedfriendcodes.txt", BANNEDFRIENDCODES_FILE_PATH);
                    }
                    else
                    {
                        TownOfHost.Logger.Info("No bannedfriendcodes.txt file found.", "BannedFriendCodesManager");
                        File.WriteAllText(BANNEDFRIENDCODES_FILE_PATH, $"Please include the part before and after #. Make sure each friend code is on a new line.\nHere are some example friend codes:\nbrassfive#8929\nraggedsofa#2041\nmerryrule#0412\ngnuedaphic#7196\nNOTE: These people were people who were banned from the TOH TOR Server for various reasons. It is recommended to keep these people here.\nNOTE: Devs of the mod are unbannable. Putting their friend codes in this file has no affect.");
                    }
                }
                catch (Exception ex)
                {
                    TownOfHost.Logger.Exception(ex, "TemplateManager");
                }
            }
            /*if (!File.Exists("percentage.txt"))
            {
                TownOfHost.Logger.Info("Could not find percentage.txt in the same folder as Among Us.exe. This will cause roles to not spawn at all. Please redownload the mod.", "Percentage");
                try
                {
                    File.WriteAllText(@"percentage.txt", "Download the correct version at: https://github.com/music-discussion/TownOfHost-TheOtherRoles");
                }
                catch (Exception ex)
                {
                    TownOfHost.Logger.Error(ex.ToString(), "Percentage");
                }
            }*/

            Harmony.PatchAll();
        }
        private delegate bool DLoadImage(IntPtr tex, IntPtr data, bool markNonReadable);
    }
    public enum CustomRoles
    {
        //Default
        Crewmate = 0,
        //Impostor(Vanilla)
        Impostor,
        Shapeshifter,
        CrewmateGhost,
        ImpostorGhost,
        Morphling,
        Mechanic,
        Physicist,
        Target,
        //Impostor
        BountyHunter,
        VoteStealer,
        FireWorks,
        Mafia,
        SerialKiller,
        //ShapeMaster,
        Sniper,
        Vampire,
        Vampress,
        Witch,
        Warlock,
        Mare,
        Miner,
        Consort,
        YingYanger,
        Grenadier,
        Disperser,
        Puppeteer,
        // EVENT WINNING ROLES
        IdentityTheft,
        Manipulator,
        //    AgiTater,
        Bomber,
        // JK NOW //
        TimeThief,
        Silencer,
        Ninja,
        Swooper,
        Camouflager,
        Freezer,
        Cleaner,
        EvilGuesser,
        LastImpostor,
        //Madmate
        MadGuardian,
        Madmate,
        MadSnitch,
        CrewPostor,
        CorruptedSheriff,
        SKMadmate,
        Parasite,
        // SPECIAL ROLES //
        Cultist,
        Whisperer,
        Chameleon,
        GodFather,
        Mafioso,
        Framer,
        Disguiser,
        // VANILLA
        Engineer,
        GuardianAngel,
        Scientist,
        //Crewmate
        Alturist,
        Lighter,
        Medium,
        Demolitionist,
        Bastion,
        NiceGuesser,
        Escort,
        Crusader,
        Psychic,
        Mystic,
        Swapper,
        Mayor,
        SabotageMaster,
        Oracle,
        Medic,
        Bodyguard,
        Sheriff,
        Investigator,
        Snitch,
        Transporter,
        SpeedBooster,
        Trapper,
        Dictator,
        Doctor,
        Child,
        Veteran,
        //Neutral
        Arsonist,
        Egoist,
        PlagueBearer,
        Pestilence,
        Vulture,
        TheGlitch,
        Postman,
        Werewolf,
        NeutWitch,
        Marksman,
        GuardianAngelTOU,
        Jester,
        Amnesiac,
        Hacker,
        PoisonMaster,
        BloodKnight,
        Hitman,
        Phantom,
        Pirate,
        Juggernaut,
        Opportunist,
        Survivor,
        Terrorist,
        Executioner,
        Jackal,
        AgiTater,
        Sidekick,
        // ALL CAT ROLES //
        SchrodingerCat,
        JSchrodingerCat,
        CSchrodingerCat,
        MSchrodingerCat,
        EgoSchrodingerCat,
        BKSchrodingerCat,
        CPSchrodingerCat,
        JugSchrodingerCat,
        MMSchrodingerCat,
        PesSchrodingerCat,
        WWSchrodingerCat,
        TGSchrodingerCat,
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

        // NEW GAMEMODE ROLES //
        Painter,
        Janitor,
        Supporter,
        Tasker,

        // RANDOM ROLE HELPERS //
        LoversWin,
        // Sub-roles are After 500. Meaning, all roles under this are Modifiers.
        NoSubRoleAssigned = 500,

        // GLOBAL MODIFIERS //
        Lovers,
        LoversRecode,
        Flash, // DONE
        Escalation,
        TieBreaker, // DONE
        Oblivious, // DONE
        Sleuth, // DONE
        Watcher, // DONE
        Obvious,
        DoubleShot,

        // CREW MODIFIERS //
        Bewilder, // DONE
        Bait, // DONE
        Torch, // DONE
        Diseased,

        // TAG COLORS //
        sns1,
        sns2,
        sns3,
        sns4,
        sns5,
        sns6,
        sns7,
        sns8,
        sns9,
        sns10,
        rosecolor,
        // random //
        thetaa,
        eevee,
        serverbooster,
        // SELF //
        minaa,
        ess,
        // end random //
        psh1,
        psh2,
        psh3,
        psh4,
        psh5,
        psh6,
        psh7,
        psh8,
        psh9,

        //Gurge44
        gu1,
        gu2,
        gu3,
        gu4,
        gu5,
        gu6,
        gu7,
        gu8,
        gu9,
        gu10,
        //Pineapple man670
        pi1,
        pi2,
        pi3,
        pi4,
        pi5,
        pi6,
        pi7,
        pi8,
        pi9,
        pi10,
        //yoclobo
        yo1,
        yo2,
        yo3,
        yo4,
        yo5,
        yo6,
        yo7,
        yo8,
        yo9,
        yo10,
        //Nicky G
        ni1,
        ni2,
        ni3,
        ni4,
        ni5,
        ni6,
        ni7,
        //Milk
        ml1,
        ml2,
        ml3,
        ml4,
        ml5,
        ml6,
        ml7,
        ml8,
        ml9,
        ml10,
        //Paige
        pg1,
        pg2,
        pg3,
        pg4,
        pg5,
        pg6,
        pg7,
        pg8,
        pg9,
        pg10,
        //ck
        cc1,
        cc2,
        cc3,
        cc4,
        cc5,
        cc6,
        cc7,
        cc8,
        cc9,
        cc10,
    }
    //WinData
    public enum CustomWinner
    {
        Draw = -1,
        Default = -2,
        Impostor = CustomRoles.Impostor,
        Crewmate = CustomRoles.Crewmate,
        Jester = CustomRoles.Jester,
        Terrorist = CustomRoles.Terrorist,
        Lovers = CustomRoles.LoversWin,
        Child = CustomRoles.Child,
        Executioner = CustomRoles.Executioner,
        Arsonist = CustomRoles.Arsonist,
        Vulture = CustomRoles.Vulture,
        Egoist = CustomRoles.Egoist,
        Pestilence = CustomRoles.Pestilence,
        Jackal = CustomRoles.Jackal,
        Juggernaut = CustomRoles.Juggernaut,
        Swapper = CustomRoles.Swapper,
        HASTroll = CustomRoles.HASTroll,
        Phantom = CustomRoles.Phantom,
        Coven = CustomRoles.Coven,
        TheGlitch = CustomRoles.TheGlitch,
        Werewolf = CustomRoles.Werewolf,
        Hacker = CustomRoles.Hacker,
        BloodKnight = CustomRoles.BloodKnight,
        Pirate = CustomRoles.Pirate,
        Marksman = CustomRoles.Marksman,
        Painter = CustomRoles.Painter,
        AgiTater = CustomRoles.AgiTater,
        Tasker = CustomRoles.Tasker
    }
    public enum AdditionalWinners
    {
        None = -1,
        Opportunist = CustomRoles.Opportunist,
        Survivor = CustomRoles.Survivor,
        SchrodingerCat = CustomRoles.SchrodingerCat,
        Executioner = CustomRoles.Executioner,
        HASFox = CustomRoles.HASFox,
        GuardianAngelTOU = CustomRoles.GuardianAngelTOU,
        Hitman = CustomRoles.Hitman,
        Witch = CustomRoles.NeutWitch
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
        Recording,
        Dev
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

    // ATTACK AND DEFENSE
    public enum AttackEnum
    {
        None = 0,
        Basic,
        Powerful,
        Unstoppable,
        Unblockable
    }
    public enum DefenseEnum
    {
        None = 0,
        Basic,
        Powerful,
        Invincible
    }
}
