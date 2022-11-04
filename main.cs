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
        public const string PluginVersion = "0.9.3.3";
        public const string DevVersion = "4";
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
        public static ConfigEntry<int> MessageWait { get; private set; }
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
        public static GameOptionsData RealOptionsData;
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
        public static int HexesThisRound;
        public static int SKMadmateNowCount;
        public static bool witchMeeting;
        public static bool isCursed;
        public static List<byte> firstKill = new();
        public static Dictionary<byte, List<byte>> knownGhosts = new();
        public static Dictionary<byte, (int, bool, bool, bool, bool)> SurvivorStuff = new(); // KEY - player ID, Item1 - NumberOfVests, Item2 - IsVesting, Item3 - HasVested, Item4 - VestedThisRound, Item5 - RoundOneVest
        public static List<byte> unreportableBodies = new();
        public static List<PlayerControl> SilencedPlayer = new();
        public static List<PlayerControl> ColliderPlayers = new();
        public static List<byte> KilledBewilder = new();
        public static List<byte> KilledDiseased = new();
        public static List<byte> KilledDemo = new();
        public static bool isSilenced;
        public static bool isShipStart;
        public static bool showEjections;
        public static Dictionary<byte, bool> CheckShapeshift = new();
        public static Dictionary<(byte, byte), string> targetArrows = new();
        public static List<PlayerControl> AllCovenPlayers = new();
        public static Dictionary<byte, PlayerControl> whoKilledWho = new();
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


        public static int MarksmanKills = 0;
        public static bool FirstMeetingOccurded = false;

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
            LastWinner = "None";
            ColliderPlayers = new List<PlayerControl>();
            WitchesThisRound = 0;
            CleanerCanClean = new Dictionary<byte, bool>();
            HasTarget = new Dictionary<byte, bool>();
            isDoused = new Dictionary<(byte, byte), bool>();
            isHexed = new Dictionary<(byte, byte), bool>();
            isInfected = new Dictionary<(byte, byte), bool>();
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
            MedusaOn = false;
            MimicOn = false;
            FirstMeetingOccurded = false;
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
            AmDebugger.Value = true;
            ShowPopUpVersion = Config.Bind("Other", "ShowPopUpVersion", "0");
            MessageWait = Config.Bind("Other", "MessageWait", 1);
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
                    { CustomRoles.Scientist, "#b6f0ff"},
                    { CustomRoles.Mechanic, "#FFA60A"},
                    { CustomRoles.Physicist, "#b6f0ff"},
                    { CustomRoles.GuardianAngel, "#ffffff"},
                    {CustomRoles.Target, "#000000"},
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
                    // GM
                    //サブ役職
                    { CustomRoles.NoSubRoleAssigned, "#ffffff"},
                    { CustomRoles.Lovers, "#FF66CC"},
                    { CustomRoles.LoversRecode, "#FF66CC"},
                    { CustomRoles.LoversWin, "#FF66CC"},
                    { CustomRoles.Flash, "#FF8080"},
                    { CustomRoles.Oblivious, "#808080"},
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

                    // TAGS //
                    //TEXT COLORS ROSIE
                    { CustomRoles.sns1, "#FFF9DB"},
                    { CustomRoles.sns2, "#FCECE0"},
                    { CustomRoles.sns3, "#F9DEE5"},
                    { CustomRoles.sns4, "#F2C2EE"},
                    { CustomRoles.sns5, "#F0ABF1"},
                    { CustomRoles.sns6, "#ED93F4"},
                    { CustomRoles.sns7, "#EA7BF7"},
                    { CustomRoles.sns8, "#E763F9"},
                    { CustomRoles.rosecolor, "#FFD6EC"},
                    // MISC //
                    { CustomRoles.eevee, "#FF8D1C"},
                    {CustomRoles.serverbooster, "#f47fff"},
                    { CustomRoles.thetaa, "#9A9AEB"},
                    // SELF//
                    { CustomRoles.minaa, "#C8A2C8"},

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
        MSchrodingerCat,
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
        CSchrodingerCat,
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
        EgoSchrodingerCat,
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
        SchrodingerCat,
        Terrorist,
        Executioner,
        Jackal,
        Sidekick,
        JSchrodingerCat,
        //HideAndSeek
        HASFox,
        HASTroll,
        //GM
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
        rosecolor,
        // random //
        thetaa,
        eevee,
        serverbooster,
        // SELF //
        minaa,
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
        Painter = CustomRoles.Painter
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
}
