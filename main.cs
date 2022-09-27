using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using UnityEngine;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
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
        public const string PluginGuid = "com.emptybottle.townofhost";
        public const string PluginVersion = "0.8.4";
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
        public static Dictionary<(byte, byte), bool> isDoused = new();
        public static List<byte> dousedIDs = new();
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
        public static Dictionary<byte, int> LastEnteredVent = new();
        public static Dictionary<byte, Vector2> LastEnteredVentLocation = new();
        public static int AliveImpostorCount;
        public static int AllImpostorCount;
        public static string LastVotedPlayer;
        public static int HexesThisRound;
        public static int SKMadmateNowCount;
        public static bool witchMeeting;
        public static bool isCursed;
        public static List<byte> firstKill = new();
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
        public static Dictionary<PlayerControl, PlayerControl> whoKilledWho = new();
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
        public static Dictionary<byte, (byte, bool)> SleuthReported = new();

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


        public static int MarksmanKills = 0;

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
            CustomWinTrigger = false;
            BitPlayers = new Dictionary<byte, (byte, float)>();
            SurvivorStuff = new Dictionary<byte, (int, bool, bool, bool, bool)>();
            WarlockTimer = new Dictionary<byte, float>();
            CursedPlayers = new Dictionary<byte, PlayerControl>();
            SpelledPlayer = new List<PlayerControl>();
            Impostors = new List<PlayerControl>();
            SilencedPlayer = new List<PlayerControl>();
            ColliderPlayers = new List<PlayerControl>();
            isDoused = new Dictionary<(byte, byte), bool>();
            isHexed = new Dictionary<(byte, byte), bool>();
            isInfected = new Dictionary<(byte, byte), bool>();
            ArsonistTimer = new Dictionary<byte, (PlayerControl, float)>();
            PlagueBearerTimer = new Dictionary<byte, (PlayerControl, float)>();
            ExecutionerTarget = new Dictionary<byte, byte>();
            GuardianAngelTarget = new Dictionary<byte, byte>();
            MayorUsedButtonCount = new Dictionary<byte, int>();
            HackerFixedSaboCount = new Dictionary<byte, int>();
            LastEnteredVent = new Dictionary<byte, int>();
            LastEnteredVentLocation = new Dictionary<byte, Vector2>();
            HasModifier = new Dictionary<byte, CustomRoles>();
            // /DeadPlayersThisRound = new List<byte>();
            LoversPlayers = new List<PlayerControl>();
            dousedIDs = new List<byte>();
            //firstKill = new Dictionary<byte, (PlayerControl, float)>();
            winnerList = new();
            VisibleTasksCount = false;
            MercCanSuicide = true;
            ExeCanChangeRoles = true;
            MessagesToSend = new List<(string, byte)>();
            currentDousingTarget = 255;
            currentInfectingTarget = 255;
            JugKillAmounts = 0;
            AteBodies = 0;
            MarksmanKills = 0;
            CovenMeetings = 0;
            GAprotects = 0;
            ProtectedThisRound = false;
            HasProtected = false;
            VetAlerts = 0;
            ProtectsSoFar = 0;
            IsProtected = false;
            ResetVision = false;
            Grenaiding = false;
            DoingYingYang = true;
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
            showEjections = false;

            IsRampaged = false;
            RampageReady = false;

            IsHackMode = false;
            GazeReady = true;
            IsGazing = false;
            CamoComms = false;
            HexesThisRound = 0;
            JackalDied = false;
            LastVotedPlayer = "";
            bkProtected = false;
            AlertSprite = Helpers.LoadSpriteFromResources("TownOfHost.Resources.Alert.png", 100f);
            DouseSprite = Helpers.LoadSpriteFromResources("TownOfHost.Resources.Doused.png", 100f);
            HackSprite = Helpers.LoadSpriteFromResources("TownOfHost.Resources.Hack.png", 100f);
            IgniteSprite = Helpers.LoadSpriteFromResources("TownOfHost.Resources.Ignite.png", 100f);
            InfectSprite = Helpers.LoadSpriteFromResources("TownOfHost.Resources.Infect.png", 100f);
            MimicSprite = Helpers.LoadSpriteFromResources("TownOfHost.Resources.Mimic.png", 100f);
            PoisonSprite = Helpers.LoadSpriteFromResources("TownOfHost.Resources.Poison.png", 100f);
            ProtectSprite = Helpers.LoadSpriteFromResources("TownOfHost.Resources.Protect.png", 100f);
            RampageSprite = Helpers.LoadSpriteFromResources("TownOfHost.Resources.Rampage.png", 100f);
            RememberSprite = Helpers.LoadSpriteFromResources("TownOfHost.Resources.Remember.png", 100f);
            SeerSprite = Helpers.LoadSpriteFromResources("TownOfHost.Resources.Seer.png", 100f);
            SheriffSprite = Helpers.LoadSpriteFromResources("TownOfHost.Resources.Sheriff.png", 100f);
            VestSprite = Helpers.LoadSpriteFromResources("TownOfHost.Resources.Vest.png", 100f);

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
                    { CustomRoles.GuardianAngel, "#ffffff"},
                    {CustomRoles.Target, "#000000"},
                    { CustomRoles.CorruptedSheriff, "#ff0000"},
                    //インポスター、シェイプシフター
                    //特殊インポスター役職
                    //マッドメイト系役職
                        //後で追加
                    //両陣営可能役職
                    { CustomRoles.Watcher, "#800080"},
                    { CustomRoles.Guesser, "#ffff00"},
                    { CustomRoles.NiceGuesser, "#E4E085"},
                    { CustomRoles.Pirate, "#EDC240"},
                    //特殊クルー役職
                    { CustomRoles.NiceWatcher, "#800080"}, //ウォッチャーの派生
                    { CustomRoles.Bait, "#00B3B3"},
                    { CustomRoles.SabotageMaster, "#0000ff"},
                    { CustomRoles.Snitch, "#b8fb4f"},
                    { CustomRoles.Mayor, "#204d42"},
                    { CustomRoles.Sheriff, "#f8cd46"},
                    { CustomRoles.Investigator, "#ffca81"},
                    { CustomRoles.Lighter, "#eee5be"},
                    { CustomRoles.SpeedBooster, "#00ffff"},
                    { CustomRoles.Swapper, "#4D99E6"},
                    { CustomRoles.Doctor, "#80ffdd"},
                    { CustomRoles.Child, "#FFFFFF"},
                    { CustomRoles.Trapper, "#5a8fd0"},
                    { CustomRoles.Dictator, "#df9b00"},
                    { CustomRoles.Sleuth, "#803333"},
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
                    { CustomRoles.SchrodingerCat, "#696969"},
                    { CustomRoles.Egoist, "#5600ff"},
                    { CustomRoles.EgoSchrodingerCat, "#5600ff"},
                    { CustomRoles.Jackal, "#00b4eb"},
                    { CustomRoles.Sidekick, "#00b4eb"},
                    { CustomRoles.Marksman, "#440101"},
                    { CustomRoles.Juggernaut, "#670038"},
                    { CustomRoles.JSchrodingerCat, "#00b4eb"},
                    { CustomRoles.Phantom, "#662962"},
                    { CustomRoles.Hitman, "#704FA8"},
                    //HideAndSeek
                    { CustomRoles.HASFox, "#e478ff"},
                    { CustomRoles.BloodKnight, "#630000"},
                    { CustomRoles.HASTroll, "#00ff00"},
                    { CustomRoles.Painter, "#FF5733"},
                    { CustomRoles.Janitor, "#c67051"},
                    { CustomRoles.Supporter, "#00b4eb"},
                    // GM
                    { CustomRoles.GM, "#ff5b70"},
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
                    { CustomRoles.Mystic, "#4D99E6"},
                    { CustomRoles.Coven, "#592e98"},
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
                    //TEXT COLORS KRAMPUS
                    { CustomRoles.tancolor, "#9e9888"},
                    { CustomRoles.tancolor2, "#63554f"},
                    { CustomRoles.rosecolor, "#FFD6EC"},
                    { CustomRoles.pinkcolor, "#ff35c6"},
                    { CustomRoles.thirdcolor, "#86A873"},
                    { CustomRoles.fourthcolor, "#C1CC99"},
                    //TEXT COLORS AUGUST
                    { CustomRoles.aug1, "#FF00A6"},
                    { CustomRoles.aug2, "#FF33B8"},
                    { CustomRoles.aug3, "#FF5CC6"},
                    { CustomRoles.aug4, "#FF85D4"},
                    { CustomRoles.aug5, "#FFADE2"},
                    { CustomRoles.aug6, "#FFD6F1"},
                    { CustomRoles.aug7, "#FFEBF8"},
                    //TEXT COLORS ROSIE
                    { CustomRoles.sns1, "#FFF9DB"},
                    { CustomRoles.sns2, "#FCECE0"},
                    { CustomRoles.sns3, "#F9DEE5"},
                    { CustomRoles.sns4, "#F2C2EE"},
                    { CustomRoles.sns5, "#F0ABF1"},
                    { CustomRoles.sns6, "#ED93F4"},
                    { CustomRoles.sns7, "#EA7BF7"},
                    { CustomRoles.sns8, "#E763F9"},
                    //TEXT COLORS PUSHINP
                    { CustomRoles.psh1, "#EF807F"},
                    { CustomRoles.psh2, "#F3969C"},
                    { CustomRoles.psh3, "#F7ABB8"},
                    { CustomRoles.psh4, "#FBC1D5"},
                    { CustomRoles.psh5, "#FBC6E9"},
                    { CustomRoles.psh6, "#F6B6E0"},
                    { CustomRoles.psh7, "#F4AEDC"},
                    { CustomRoles.psh8, "#F1A6D7"},
                    { CustomRoles.psh9, "#EC96CE"},
                    //TEXT COLORS JESSI
                    { CustomRoles.jss1, "#B50000"},
                    { CustomRoles.jss2, "#BF1A0A"},
                    { CustomRoles.jss3, "#C83414"},
                    { CustomRoles.jss4, "#DA6827"},
                    { CustomRoles.jss5, "#E48231"},
                    { CustomRoles.jss6, "#E98F36"},
                    { CustomRoles.jss7, "#ED9C3B"},
                    { CustomRoles.jss8, "#F6B645"},
                    { CustomRoles.jss9, "#FFD04E"},
                    //TEXT COLORS CANDY
                    { CustomRoles.cnd1, "#84ABFF"},
                    { CustomRoles.cnd2, "#85A1F8"},
                    { CustomRoles.cnd3, "#8696F1"},
                    { CustomRoles.cnd4, "#8881E3"},
                    { CustomRoles.cnd5, "#8A6CD5"},
                    { CustomRoles.cnd6, "#8B56C6"},
                    { CustomRoles.cnd7, "#8D41B8"},
                    { CustomRoles.cnd8, "#8E2BA9"},
                    { CustomRoles.cnd9, "#91008C"},
                    { CustomRoles.nd1, "#11121b"},
                    { CustomRoles.nd2, "#191c27"}
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
            if (!File.Exists("percentage.txt"))
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
            }

            Harmony.PatchAll();
        }
        private delegate bool DLoadImage(IntPtr tex, IntPtr data, bool markNonReadable);


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
        Target,
        //Impostor
        BountyHunter,
        EvilWatcher,
        VoteStealer,
        FireWorks,
        Mafia,
        SerialKiller,
        //ShapeMaster,
        Sniper,
        Vampire,
        Witch,
        Warlock,
        Mare,
        Miner,
        YingYanger,
        Grenadier,
        Puppeteer,
        TimeThief,
        Silencer,
        Ninja,
        Swooper,
        Camouflager,
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
        MSchrodingerCat,//インポスター陣営のシュレディンガーの猫
                        //両陣営
        Guesser,
        // Watcher,
        //Crewmate(Vanilla)
        Engineer,
        GuardianAngel,
        Scientist,
        //Crewmate
        //Bait,
        //Sleuth,
        Alturist,
        //Bewilder,
        Lighter,
        Medium,
        Demolitionist,
        Bastion,
        NiceGuesser,
        Hacker,
        Psychic,
        Swapper,
        Mayor,
        NiceWatcher,
        SabotageMaster,
        Sheriff,
        Investigator,
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
        Marksman,
        GuardianAngelTOU,
        Supporter,
        EgoSchrodingerCat,//エゴイスト陣営のシュレディンガーの猫
        Jester,
        Amnesiac,
        BloodKnight,
        Hitman,
        Phantom,
        Pirate,
        Juggernaut,
        Opportunist,
        Survivor,
        SchrodingerCat,//第三陣営のシュレディンガーの猫
        Terrorist,
        Executioner,
        Jackal,
        Sidekick,
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

        // NEW GAMEMODE ROLES //

        TeamRed,
        TeamBlue,
        TeamGreen,
        Painter,
        Janitor,

        // RANDOM ROLE HELPERS //
        LoversWin,
        // Sub-roles are After 500. Meaning, all roles under this are Modifiers.
        NoSubRoleAssigned = 500,

        // GLOBAL MODIFIERS //
        Lovers,
        LoversRecode,
        Flash, // DONE
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
        Mystic,
        // CUSTOM COLORS
        tancolor,
        tancolor2,
        rosecolor,
        pinkcolor,
        fourthcolor,
        thirdcolor,
        //CUSTOM COLORS AUGUST
        aug1,
        aug2,
        aug3,
        aug4,
        aug5,
        aug6,
        aug7,
        //CUSTOM COLORS ROSIE
        sns1,
        sns2,
        sns3,
        sns4,
        sns5,
        sns6,
        sns7,
        sns8,
        //CUSTOM COLORS PUSHINP
        psh1,
        psh2,
        psh3,
        psh4,
        psh5,
        psh6,
        psh7,
        psh8,
        psh9,
        //CUSTOM COLORS JESSI
        jss1,
        jss2,
        jss3,
        jss4,
        jss5,
        jss6,
        jss7,
        jss8,
        jss9,
        //CUSTOM COLORS CANDY
        cnd1,
        cnd2,
        cnd3,
        cnd4,
        cnd5,
        cnd6,
        cnd7,
        cnd8,
        cnd9,
        nd1,
        nd2
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
        Discord,
        Hosting,
        Testing,
        Simping,
        Trolling
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
