using System.Linq;
using System;
using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using UnityEngine;
using static TownOfHost.Translator;
using AmongUs.GameOptions;
using Convert = System.Convert;
using Exception = System.Exception;
using Math = System.Math;
using NotImplementedException = System.NotImplementedException;
using String = System.String;
using TownOfHost.PrivateExtensions;
using TownOfHost;
namespace TownOfHost
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    class ChangeRoleSettings
    {
        public static void Postfix(AmongUsClient __instance)
        {
            try
            {
                Logger.Msg("Game Starting. Resetting vars (1/4)", "Load Check (CoStartGame)");
                PlayerState.Init();
                SoundEffectsManager.Load();
                Logger.Msg("Game Starting. Resetting vars (2/4)", "Load Check (CoStartGame)");
                Main.currentWinner = CustomWinner.Default;
                Main.CustomWinTrigger = false;
                Main.AllPlayerCustomRoles = new Dictionary<byte, CustomRoles>();
                Main.AllPlayerCustomSubRoles = new Dictionary<byte, CustomRoles>();
                Main.LastPlayerCustomRoles = new Dictionary<byte, CustomRoles>();
                Main.LastPlayerCustomSubRoles = new Dictionary<byte, CustomRoles>();
                Main.AllPlayerKillCooldown = new Dictionary<byte, float>();
                Main.AllPlayerSpeed = new Dictionary<byte, float>();
                Main.BitPlayers = new Dictionary<byte, (byte, float)>();
                Main.WarlockTimer = new Dictionary<byte, float>();
                Main.isDoused = new Dictionary<(byte, byte), bool>();
                Main.SurvivorStuff = new Dictionary<byte, (int, bool, bool, bool, bool)>();
                Main.isHexed = new Dictionary<(byte, byte), bool>();
                Main.ArsonistTimer = new Dictionary<byte, (PlayerControl, float)>();
                Main.isInfected = new Dictionary<(byte, byte), bool>();
                Main.PlagueBearerTimer = new Dictionary<byte, (PlayerControl, float)>();
                Main.CursedPlayers = new Dictionary<byte, PlayerControl>();
                Main.isCurseAndKill = new Dictionary<byte, bool>();
                Main.AirshipMeetingTimer = new Dictionary<byte, float>();
                Main.ExecutionerTarget = new Dictionary<byte, byte>();
                Main.FirstMeetingPassed = false;
                Main.GuardianAngelTarget = new Dictionary<byte, byte>();
                Main.SKMadmateNowCount = 0;
                Main.WitchesThisRound = 0;
                Main.HexesThisRound = 0;
                Main.isCursed = false;
                Main.PuppeteerList = new Dictionary<byte, byte>();
                Main.WitchedList = new Dictionary<byte, byte>();
                Main.LastWinner = "None";
                Main.WitchList = new Dictionary<byte, byte>();
                Main.unvotablePlayers = new();

                Main.AfterMeetingDeathPlayers = new();
                Main.ResetCamPlayerList = new();

                Main.chosenEngiRoles = new List<CustomRoles>();
                Main.chosenScientistRoles = new List<CustomRoles>();
                Main.chosenShifterRoles = new List<CustomRoles>();
                Main.chosenRoles = new List<CustomRoles>();
                Main.chosenImpRoles = new List<CustomRoles>();
                Main.chosenNK = new List<CustomRoles>();
                Main.chosenNonNK = new List<CustomRoles>();

                Main.SpelledPlayer = new List<PlayerControl>();
                Main.witchMeeting = false;
                Main.firstKill = new List<byte>();
                Main.knownGhosts = new Dictionary<byte, List<byte>>();
                Main.unreportableBodies = new List<byte>();
                Main.LastVotedPlayer = "";
                Main.dousedIDs = new List<byte>();
                Main.isSilenced = false;
                Main.ExeCanChangeRoles = true;
                Main.MercCanSuicide = true;
                Main.SilencedPlayer = new List<PlayerControl>();
                Main.DeadPlayersThisRound = new List<byte>();
                Main.CheckShapeshift = new Dictionary<byte, bool>();
                Main.SpeedBoostTarget = new Dictionary<byte, byte>();
                Main.MayorUsedButtonCount = new Dictionary<byte, int>();
                Main.AliveAtTheEndOfTheRound = new List<byte>();
                Main.HackerFixedSaboCount = new Dictionary<byte, int>();
                Main.LastEnteredVent = new Dictionary<byte, Vent>();
                Main.LastEnteredVentLocation = new Dictionary<byte, Vector2>();
                Main.CurrentEnterdVent = new Dictionary<byte, Vent>();
                Main.HasModifier = new Dictionary<byte, CustomRoles>();
                Main.KilledBewilder = new List<byte>();
                Main.AllPlayerSkin = new();
                Main.KilledDemo = new List<byte>();
                Main.targetArrows = new();
                Main.KilledDiseased = new List<byte>();
                EndGameHelper.EveryoneDied = false;
                Main.VetCanAlert = true;
                Main.CanTransport = true;
                Main.JugKillAmounts = 0;
                Main.AteBodies = 0;
                Main.TeamJuggernautAlive = false;
                Main.TeamPestiAlive = false;
                Main.Grenaiding = false;
                Main.ResetVision = false;
                Main.CamoComms = false;
                Main.JackalDied = false;
                Main.PhantomAlert = false;
                Main.PhantomCanBeKilled = false;
                Main.PickpocketKills = new Dictionary<byte, int>();
                Main.KillCount = new Dictionary<byte, int>();

                Main.LoversPlayers = new List<PlayerControl>();
                Main.ColliderPlayers = new List<byte>();
                Main.isLoversDead = false;

                ////////////// COVEN INFO //////////////
                Main.TeamCovenAlive = 3;
                Main.CovenMeetings = 0;
                Main.HasNecronomicon = false;
                Main.HexMasterOn = false;
                Main.PotionMasterOn = false;
                Main.VampireDitchesOn = false;
                Main.MedusaOn = false;
                Main.MimicOn = false;
                Main.NecromancerOn = false;
                Main.ConjurorOn = false;
                Main.ChoseWitch = false;
                Main.DoingYingYang = true;
                Main.WitchProtected = false;
                ////////////// COVEN INFO //////////////
                Main.chosenDesyncRoles = new List<CustomRoles>();
                Logger.Msg("Game Starting. Resetting vars (3/4)", "Load Check (CoStartGame)");

                Options.UsedButtonCount = 0;
                Main.showEjections = GameOptionsManager.Instance.CurrentGameOptions.AsNormalOptions()!.ConfirmImpostor;
                GameOptionsManager.Instance.CurrentGameOptions.AsNormalOptions()!.ConfirmImpostor = false;
                Main.RealOptionsData = null;
                Main.RealOptionsData = GameOptionsManager.Instance.CurrentGameOptions.DeepCopy();

                if (AmongUsClient.Instance.AmHost && Options.SpeedrunGamemode.GetBool())
                {
                    Main.KillingSpree = new List<byte>();
                    GameOptionsManager.Instance.CurrentGameOptions.AsNormalOptions()!.NumImpostors = 0;
                }

                Main.introDestroyed = false;
                Main.VettedThisRound = false;
                Main.VetIsAlerted = false;
                Main.IsRoundOne = true;
                Main.IsRoundOneGA = true;
                Main.MarksmanKills = 0;
                Main.GAprotects = 0;
                Main.ProtectedThisRound = false;
                Main.CurrentTarget = new Dictionary<byte, byte>();
                Main.HasProtected = false;

                Main.IsGazing = false;
                Main.MareHasRedName = false;
                Main.FirstMeetingOccured = false;
                Main.GazeReady = true;

                Main.IsRampaged = false;
                Main.RampageReady = true;
                Main.Impostors = new();
                Main.lastAmountOfTasks = new();
                Main.AllImpostorCount = 0;
                Main.HasTarget = new Dictionary<byte, bool>();
                Main.IsHackMode = false;
                Main.bkProtected = false;
                Main.CleanerCanClean = new Dictionary<byte, bool>();
                Main.bombedVents = new List<int>();
                Logger.Msg("Game Starting. Resetting vars (4/4)", "Load Check (CoStartGame)");
                if (CustomRoles.Transporter.IsEnable())
                    Main.TransportsLeft = Options.NumOfTransports.GetInt();

                Main.WonFFATeam = 255;
                Main.DiscussionTime = Main.RealOptionsData.AsNormalOptions()!.DiscussionTime;
                Main.VotingTime = Main.RealOptionsData.AsNormalOptions()!.VotingTime;

                NameColorManager.Instance.RpcReset();
                Main.LastNotifyNames = new();
                Main.IsShapeShifted = new List<byte>();
                Main.DictatesRemaining = new();

                Main.currentDousingTarget = 255;
                Main.currentFreezingTarget = 255;
                Main.VetAlerts = 0;
                Main.ProtectsSoFar = 0;
                Main.rolesRevealedNextMeeting = new List<byte>();
                Main.IsProtected = false;
                Main.IsInvis = false;
                Main.CanGoInvis = true;
                Main.PlayerColors = new();
                Main.whoKilledWho = new Dictionary<byte, byte>();
                Main.SleuthReported = new();
                //名前の記録
                Main.AllPlayerNames = new();
                Logger.Msg("Game Starting. Resetting vars (5/6)", "Load Check (CoStartGame)");

                if (AmongUsClient.Instance.AmHost)
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (Main.devNames.ContainsKey(pc.PlayerId))
                        {
                            //pc.name = Main.devNames[pc.PlayerId];
                            //pc.Data.PlayerName = Main.devNames[pc.PlayerId];
                            pc.RpcSetName(Main.devNames[pc.PlayerId]);
                        }
                    }
                }

                foreach (var target in PlayerControl.AllPlayerControls)
                {
                    foreach (var seer in PlayerControl.AllPlayerControls)
                    {
                        var pair = (target.PlayerId, seer.PlayerId);
                        Main.LastNotifyNames[pair] = target.name;
                    }
                }
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (AmongUsClient.Instance.AmHost && Options.ColorNameMode.GetBool()) pc.RpcSetName(Palette.GetColorName(pc.Data.DefaultOutfit.ColorId));
                    Main.AllPlayerNames[pc.PlayerId] = pc?.Data?.PlayerName;

                    Main.PlayerColors[pc.PlayerId] = Palette.PlayerColors[pc.Data.DefaultOutfit.ColorId];
                    Main.AllPlayerSpeed[pc.PlayerId] = Main.RealOptionsData.AsNormalOptions()!.PlayerSpeedMod; //移動速度をデフォルトの移動速度に変更
                    pc.cosmetics.nameText.text = pc.name;
                }
                Main.VisibleTasksCount = true;
                SaveSkin();
                if (__instance.AmHost)
                {
                    RPC.SyncCustomSettingsRPC();
                    Main.RefixCooldownDelay = 0;
                    if (Options.CurrentGameMode() == CustomGameMode.HideAndSeek)
                    {
                        Options.HideAndSeekKillDelayTimer = Options.KillDelay.GetFloat();
                    }
                    if (Options.IsStandardHAS)
                    {
                        Options.HideAndSeekKillDelayTimer = Options.StandardHASWaitingTime.GetFloat();
                    }
                }
                Logger.Msg("Game Starting. Resetting vars (6/6)", "Load Check (CoStartGame)");
                Main.devNames = new Dictionary<byte, string>();
                FallFromLadder.Reset();
                BountyHunter.Init();
                SerialKiller.Init();
                FireWorks.Init();
                Sniper.Init();
                TimeThief.Init();
                Mare.Init();
                Egoist.Init();
                Bomber.Init();
                Sheriff.Init();
                Investigator.Init();
                Camouflager.Init();
                Postman.Reset();
                Ninja.Init();
                Escapist.Init();
                Necromancer.Init();
                Guesser.Init();
                Manipulator.Reset();
                AgiTater.Reset();
                AntiBlackout.Reset();
                Logger.Msg("Load Check Succeded!", "Load Check (CoStartGame)");
            }
            catch (Exception e)
            {
                Logger.Error($"Error encountered while resetting player info. Game has force ended to prevent black screen.\nError:\n{e}\n(Report to Dev if Needed + Log File)", "Reset Vars");
                if (!AmongUsClient.Instance.AmHost) return;
                Utils.SendMessage($"Error encountered while resetting player info. Game has force ended to prevent black screen.\nError:\n{e}\n(Report to Dev if Needed + Log File)");
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EndGame, Hazel.SendOption.Reliable, -1);
                writer.Write((int)CustomWinner.Draw);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPC.ForceEndGame();
            }
        }
        private static void SaveSkin()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                var color = player.CurrentOutfit.ColorId;
                var hat = player.CurrentOutfit.HatId;
                var skin = player.CurrentOutfit.SkinId;
                var visor = player.CurrentOutfit.VisorId;
                var pet = player.CurrentOutfit.PetId;
                Main.AllPlayerSkin[player.PlayerId] = (color, hat, skin, visor, pet, player.GetRealName(true));
            }
        }
    }
    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    class SelectRolesPatch
    {
        public static void Prefix()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            try
            {
                CustomRpcSender sender = CustomRpcSender.Create("SelectRoles Sender", SendOption.Reliable);
                RpcSetRoleReplacer.StartReplace(sender);
                Logger.Msg("RoleManager.SelectRoles", "Load Check (Select Roles)");

                var rand = new System.Random();
                if (Options.CurrentGameMode() != CustomGameMode.HideAndSeek)
                {
                    Logger.Msg("RoleManager.SelectRoles / Check (1/10)", "Load Check (Select Roles)");
                    List<PlayerControl> AllPlayers = new();
                    List<PlayerControl> AllNKPlayers = new();
                    List<PlayerControl> AllnonNKPlayers = new();
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        //  if (Options.EnableGM.GetBool() && pc.PlayerId == PlayerControl.LocalPlayer.PlayerId) continue;
                        AllPlayers.Add(pc);
                    }
                    if (Options.EnableGM.GetBool())
                    {
                        AllPlayers.RemoveAll(x => x == PlayerControl.LocalPlayer);
                        PlayerControl.LocalPlayer.RpcSetCustomRole(CustomRoles.GM);
                        PlayerControl.LocalPlayer.RpcSetRole(RoleTypes.Crewmate);
                        PlayerControl.LocalPlayer.Data.IsDead = true;
                    }
                    // GIVE PLAYERS ROLE //

                    int numofNks = 0;
                    bool hunterSpawn = false;
                    int numofNonNks = 0;

                    if (Options.MaxNK.GetInt() != 0)
                        numofNks = UnityEngine.Random.RandomRange(Options.MinNK.GetInt(), Options.MaxNK.GetInt());
                    if (Options.MaxNonNK.GetInt() != 0)
                        numofNonNks = UnityEngine.Random.RandomRange(Options.MinNonNK.GetInt(), Options.MaxNonNK.GetInt());

                    numofNks = Mathf.RoundToInt(numofNks);
                    numofNonNks = Mathf.RoundToInt(numofNonNks);

                    Logger.Msg("RoleManager.SelectRoles / Check (2/10)", "Load Check (Select Roles)");

                    if (Options.MaxNK.GetInt() != 0)
                        for (var i = 0; i < numofNks; i++)
                        {
                            var rando = new System.Random();
                            var player = AllPlayers[rando.Next(0, AllPlayers.Count)];
                            AllPlayers.Remove(player);
                            AllNKPlayers.Add(player);
                        }
                    if (Options.MaxNonNK.GetInt() != 0)
                        for (var i = 0; i < numofNonNks; i++)
                        {
                            var rando = new System.Random();
                            var player = AllPlayers[rando.Next(0, AllPlayers.Count)];
                            AllPlayers.Remove(player);
                            AllnonNKPlayers.Add(player);
                        }
                    Logger.Msg("RoleManager.SelectRoles / Check (3/10)", "Load Check (Select Roles)");

                    // ASSIGN NK ROLES //
                    List<CustomRoles> rolesChosen = new();
                    if (Options.MaxNK.GetInt() != 0)
                    {
                        if (RoleGoingInList(CustomRoles.Arsonist))
                            rolesChosen.Add(CustomRoles.Arsonist);

                        if (RoleGoingInList(CustomRoles.Jackal))
                            rolesChosen.Add(CustomRoles.Jackal);

                        if (RoleGoingInList(CustomRoles.Juggernaut))
                            rolesChosen.Add(CustomRoles.Juggernaut);

                        if (RoleGoingInList(CustomRoles.Egoist))
                            rolesChosen.Add(CustomRoles.Egoist);

                        if (RoleGoingInList(CustomRoles.PlagueBearer))
                            rolesChosen.Add(CustomRoles.PlagueBearer);

                        if (RoleGoingInList(CustomRoles.TheGlitch))
                            rolesChosen.Add(CustomRoles.TheGlitch);

                        if (RoleGoingInList(CustomRoles.Werewolf))
                            rolesChosen.Add(CustomRoles.Werewolf);

                        if (RoleGoingInList(CustomRoles.NeutWitch))
                            rolesChosen.Add(CustomRoles.NeutWitch);

                        if (RoleGoingInList(CustomRoles.AgiTater))
                            rolesChosen.Add(CustomRoles.AgiTater);

                        if (RoleGoingInList(CustomRoles.BloodKnight))
                            rolesChosen.Add(CustomRoles.BloodKnight);

                        if (RoleGoingInList(CustomRoles.Marksman))
                            rolesChosen.Add(CustomRoles.Marksman);

                        if (RoleGoingInList(CustomRoles.CrewPostor))
                            rolesChosen.Add(CustomRoles.CrewPostor);

                        if (RoleGoingInList(CustomRoles.PoisonMaster))
                            rolesChosen.Add(CustomRoles.PoisonMaster);

                        if (RoleGoingInList(CustomRoles.Pirate))
                            rolesChosen.Add(CustomRoles.Pirate);

                        if (rolesChosen.Count < numofNks)
                            numofNks = rolesChosen.Count;

                        if (rolesChosen.Count != 0)
                            if (numofNks != 0)
                                for (var i = 0; i < numofNks; i++)
                                {
                                    var rando = new System.Random();
                                    var random = new System.Random();
                                    var role = rolesChosen[rando.Next(0, rolesChosen.Count)];
                                    var player = AllNKPlayers[random.Next(0, AllNKPlayers.Count)];
                                    if (Main.chosenNK.Contains(role)) break;
                                    rolesChosen.Remove(role);
                                    AllNKPlayers.Remove(player);
                                    Main.chosenNK.Add(role);
                                    List<PlayerControl> urself = new();
                                    if (role is CustomRoles.Jackal) hunterSpawn = true;
                                    urself.Add(player);
                                    if (role.IsShapeShifter())
                                    {
                                        if (role == CustomRoles.Egoist)
                                            if (Main.RealOptionsData.NumImpostors > 1)
                                                Main.chosenShifterRoles.Add(role);
                                            else { }
                                        else
                                            AssignDesyncRole(role, urself, sender, BaseRole: RoleTypes.Shapeshifter);
                                    }
                                    else if (role is CustomRoles.CrewPostor or CustomRoles.Pirate)
                                        Main.chosenNonNK.Add(role);
                                    else
                                        AssignDesyncRole(role, urself, sender, BaseRole: RoleTypes.Impostor);
                                }
                    }

                    Logger.Msg("RoleManager.SelectRoles / Check (4/10)", "Load Check (Select Roles)");

                    // ASSIGN NON-NK ROLES //
                    List<CustomRoles> rolesChosenNon = new();
                    if (Options.MaxNonNK.GetInt() != 0)
                    {
                        if (RoleGoingInList(CustomRoles.Jester))
                            rolesChosenNon.Add(CustomRoles.Jester);

                        if (RoleGoingInList(CustomRoles.Survivor))
                            rolesChosenNon.Add(CustomRoles.Survivor);

                        if (RoleGoingInList(CustomRoles.SchrodingerCat))
                            rolesChosenNon.Add(CustomRoles.SchrodingerCat);

                        if (RoleGoingInList(CustomRoles.Terrorist))
                            rolesChosenNon.Add(CustomRoles.Terrorist);

                        if (RoleGoingInList(CustomRoles.Executioner))
                            rolesChosenNon.Add(CustomRoles.Executioner);

                        if (RoleGoingInList(CustomRoles.Swapper))
                            rolesChosenNon.Add(CustomRoles.Swapper);

                        if (RoleGoingInList(CustomRoles.Postman))
                            rolesChosenNon.Add(CustomRoles.Postman);

                        if (RoleGoingInList(CustomRoles.GuardianAngelTOU))
                            rolesChosenNon.Add(CustomRoles.GuardianAngelTOU);

                        if (RoleGoingInList(CustomRoles.Hacker))
                            rolesChosenNon.Add(CustomRoles.Hacker);

                        if (RoleGoingInList(CustomRoles.Opportunist))
                            rolesChosenNon.Add(CustomRoles.Opportunist);

                        if (RoleGoingInList(CustomRoles.Vulture))
                            rolesChosenNon.Add(CustomRoles.Vulture);

                        if (RoleGoingInList(CustomRoles.Amnesiac))
                            rolesChosenNon.Add(CustomRoles.Amnesiac);

                        if (RoleGoingInList(CustomRoles.Phantom))
                            rolesChosenNon.Add(CustomRoles.Phantom);

                        if (RoleGoingInList(CustomRoles.Hitman))
                            rolesChosenNon.Add(CustomRoles.Hitman);

                        if (rolesChosenNon.Count < numofNonNks)
                            numofNonNks = rolesChosenNon.Count;

                        if (rolesChosenNon.Count != 0)
                            if (numofNonNks != 0)
                                for (var i = 0; i < numofNonNks; i++)
                                {
                                    var rando = new System.Random();
                                    var random = new System.Random();
                                    var role = rolesChosenNon[rando.Next(0, rolesChosenNon.Count)];
                                    var player = AllnonNKPlayers[random.Next(0, AllnonNKPlayers.Count)];
                                    //if (Main.chosenNonNK.Contains(role) || Main.chosenEngiRoles.Contains(role)) continue;
                                    rolesChosenNon.Remove(role);
                                    AllnonNKPlayers.Remove(player);
                                    if (role.IsEngineer())
                                        Main.chosenEngiRoles.Add(role);
                                    else if (role is CustomRoles.Amnesiac or CustomRoles.Hitman)
                                    {
                                        List<PlayerControl> urself = new();
                                        urself.Add(player);
                                        AssignDesyncRole(role, urself, sender, BaseRole: RoleTypes.Impostor);
                                    }
                                    else
                                        Main.chosenNonNK.Add(role);
                                }
                    }

                    Logger.Msg("RoleManager.SelectRoles / Check (5/10)", "Load Check (Select Roles)");

                    List<CustomRoles> rolesChosenImp = new();
                    List<CustomRoles> chosenCrew = new();

                    foreach (CustomRoles role in Enum.GetValues(typeof(CustomRoles)))
                    {
                        if (role.IsVanilla()) continue;
                        if (role.RoleCannotBeInList()) continue;
                        if (role.IsNeutral()) continue;
                        if (role.IsModifier()) continue;
                        if (role.IsEnable() && role.IsImpostor())
                        {
                            for (var i = 0; i < role.GetCount(); i++)
                            {
                                if (RoleGoingInList(role))
                                {
                                    rolesChosenImp.Add(role);
                                }
                            }
                        }
                        else
                        {
                            if (role.IsCrewmate() && role.IsEnable())
                            {
                                // role is crew //
                                for (var i = 0; i < role.GetCount(); i++)
                                {
                                    if (RoleGoingInList(role))
                                    {
                                        chosenCrew.Add(role);
                                    }
                                }
                            }
                            else if (role.IsMadmate() && role.IsEnable())
                            {
                                // role is madmate. madmate has a random chance of replacing //
                                for (var i = 0; i < role.GetCount(); i++)
                                {
                                    if (RoleGoingInList(role))
                                    {
                                        chosenCrew.Add(role);
                                    }
                                }
                            }
                        }
                    }

                    Logger.Msg("RoleManager.SelectRoles / Check (6/10)", "Load Check (Select Roles)");

                    var impnum = Main.RealOptionsData.NumImpostors;
                    if (rolesChosenImp.Count != 0)
                    {
                        if (impnum > rolesChosenImp.Count) impnum = rolesChosenImp.Count;
                        if (impnum > 0)
                            for (var i = 0; i < impnum; i++)
                            {
                                var rando = new System.Random();
                                var role = rolesChosenImp[rando.Next(0, rolesChosenImp.Count)];
                                rolesChosenImp.Remove(role);
                                if (role.IsShapeShifter())
                                    Main.chosenShifterRoles.Add(role);
                                else if (role is CustomRoles.Vampire)
                                {
                                    bool vampress = UnityEngine.Random.RandomRange(1, 100) <= 10;
                                    if (vampress && Options.EnableHiddenRoles.GetBool())
                                    {
                                        role = CustomRoles.Vampress;
                                        Main.chosenShifterRoles.Add(CustomRoles.Vampress);
                                    }
                                    else
                                        Main.chosenImpRoles.Add(role);
                                }
                                else
                                    Main.chosenImpRoles.Add(role);
                            }
                    }

                    Logger.Msg("RoleManager.SelectRoles / Check (7/10)", "Load Check (Select Roles)");
                    // NOW WE CHOOSE CREW ROLES //
                    if (chosenCrew.Count != 0)
                    {
                        var crewnum = AllPlayers.Count;
                        if (crewnum > chosenCrew.Count) crewnum = chosenCrew.Count;
                        if (crewnum > 0)
                            for (var i = 0; i < crewnum; i++)
                            {
                                var rando = new System.Random();
                                var role = chosenCrew[rando.Next(0, chosenCrew.Count)];
                                chosenCrew.Remove(role);
                                if (role.IsDesyncRole())
                                    Main.chosenDesyncRoles.Add(role);
                                else if (role.IsEngineer())
                                    Main.chosenEngiRoles.Add(role);
                                else if (role is CustomRoles.Doctor or CustomRoles.Physicist)
                                    Main.chosenScientistRoles.Add(role);
                                else
                                    Main.chosenRoles.Add(role);
                            }
                    }

                    Logger.Msg("RoleManager.SelectRoles / Check (8/10)", "Load Check (Select Roles)");

                    RoleOptionsCollectionV07 roleOpt = GameOptionsManager.Instance.currentNormalGameOptions.roleOptions;
                    int ScientistNum = roleOpt.GetNumPerGame(RoleTypes.Scientist);
                    int AdditionalScientistNum = Main.chosenScientistRoles.Count;
                    roleOpt.SetRoleRate(RoleTypes.Scientist, ScientistNum + AdditionalScientistNum, AdditionalScientistNum > 0 ? 100 : roleOpt.GetChancePerGame(RoleTypes.Scientist));

                    int EngineerNum = roleOpt.GetNumPerGame(RoleTypes.Engineer);
                    int AdditionalEngineerNum = Main.chosenEngiRoles.Count;
                    roleOpt.SetRoleRate(RoleTypes.Engineer, EngineerNum + AdditionalEngineerNum, AdditionalEngineerNum > 0 ? 100 : roleOpt.GetChancePerGame(RoleTypes.Engineer));

                    int ShapeshifterNum = roleOpt.GetNumPerGame(RoleTypes.Shapeshifter);
                    int AdditionalShapeshifterNum = Main.chosenShifterRoles.Count;
                    roleOpt.SetRoleRate(RoleTypes.Shapeshifter, ShapeshifterNum + AdditionalShapeshifterNum, AdditionalShapeshifterNum > 0 ? 100 : roleOpt.GetChancePerGame(RoleTypes.Shapeshifter));

                    Logger.Msg("RoleManager.SelectRoles / Check (9/10)", "Load Check (Select Roles)");

                    if (RoleGoingInList(CustomRoles.Coven))
                        ForceAssignRole(CustomRoles.Coven, AllPlayers, sender, Count: CustomRoles.Coven.GetCount(), BaseRole: RoleTypes.Impostor);
                    if (hunterSpawn)
                        if (Options.JackalHasSidekick.GetBool())
                            ForceAssignRole(CustomRoles.Sidekick, AllPlayers, sender, Count: 1, BaseRole: RoleTypes.Impostor);

                    if (Main.chosenDesyncRoles.Contains(CustomRoles.Parasite))
                        AssignDesyncShiftingRole(CustomRoles.Parasite, AllPlayers, sender);
                    if (Main.chosenDesyncRoles.Contains(CustomRoles.Sheriff))
                        AssignDesyncRole(CustomRoles.Sheriff, AllPlayers, sender, BaseRole: RoleTypes.Impostor);
                    if (Main.chosenDesyncRoles.Contains(CustomRoles.Investigator))
                        AssignDesyncRole(CustomRoles.Investigator, AllPlayers, sender, BaseRole: RoleTypes.Impostor);
                    if (Main.chosenDesyncRoles.Contains(CustomRoles.Escort))
                        AssignDesyncRole(CustomRoles.Escort, AllPlayers, sender, BaseRole: RoleTypes.Impostor);
                    if (Main.chosenDesyncRoles.Contains(CustomRoles.Crusader))
                        AssignDesyncRole(CustomRoles.Crusader, AllPlayers, sender, BaseRole: RoleTypes.Impostor);

                    Logger.Msg("RoleManager.SelectRoles / Check (10/10)", "Load Check (Select Roles)");
                }
                else
                {
                    if (Options.SplatoonOn.GetBool())
                    {
                        List<PlayerControl> AllPlayers = new();
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            AllPlayers.Add(pc);
                        }
                        //AssignDesyncRole(CustomRoles.Supporter, AllPlayers, sender, BaseRole: RoleTypes.Crewmate);
                        AssignDesyncRole(CustomRoles.Janitor, AllPlayers, sender, BaseRole: RoleTypes.Impostor);
                        AssignPainters(CustomRoles.Painter, AllPlayers, sender, BaseRole: RoleTypes.Impostor);
                        Logger.Msg("Assigned Roles for Splatoon", "Load Check (Select Roles)");
                    }
                    else if (Options.FreeForAllOn.GetBool())
                    {
                        List<PlayerControl> AllPlayers = new();
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            AllPlayers.Add(pc);
                        }
                        ForceAssignRole(CustomRoles.Jackal, AllPlayers, sender, Count: AllPlayers.Count, BaseRole: RoleTypes.Impostor);
                        Logger.Msg("Assigned Roles for FreeForAll", "Load Check (Select Roles)");
                    }
                    else if (Options.SpeedrunGamemode.GetBool())
                    {
                        Logger.Msg("Speedrun GameMode Check", "Load Check (Select Roles)");
                    }
                }
                if (sender.CurrentState == CustomRpcSender.State.InRootMessage) sender.EndMessage();
            }
            catch
            {
                Logger.Error("Error encountered while generating roles. Game has force ended to prevent black screen.", "Select Roles");
                Utils.SendMessage("Error encountered while generating roles. Game has force ended to prevent black screen.");
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EndGame, Hazel.SendOption.Reliable, -1);
                writer.Write((int)CustomWinner.Draw);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPC.ForceEndGame();
            }
        }
        public static void Postfix()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            try
            {
                Logger.Msg("RoleManager.SelectRoles.Postfix Check", "Load Check (Select Roles.Postfix)");
                RpcSetRoleReplacer.Release();
                RpcSetRoleReplacer.sender.SendMessage();

                //Utils.ApplySuffix();

                var rand = new System.Random();
                Main.KillOrSpell = new Dictionary<byte, bool>();

                List<PlayerControl> Crewmates = new();
                List<PlayerControl> Impostors = new();
                List<PlayerControl> Scientists = new();
                List<PlayerControl> Engineers = new();
                List<PlayerControl> GuardianAngels = new();
                List<PlayerControl> Shapeshifters = new();
                List<PlayerControl> ImpGhosts = new();
                List<PlayerControl> CrewGhosts = new();

                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    pc.Data.IsDead = false;
                    if (Main.AllPlayerCustomRoles.ContainsKey(pc.PlayerId)) continue;
                    switch (pc.Data.Role.Role)
                    {
                        case RoleTypes.Crewmate:
                            Crewmates.Add(pc);
                            Main.AllPlayerCustomRoles.Add(pc.PlayerId, CustomRoles.Crewmate);
                            break;
                        case RoleTypes.Impostor:
                            Impostors.Add(pc);
                            Main.AllPlayerCustomRoles.Add(pc.PlayerId, CustomRoles.Impostor);
                            break;
                        case RoleTypes.Scientist:
                            Scientists.Add(pc);
                            Main.AllPlayerCustomRoles.Add(pc.PlayerId, CustomRoles.Scientist);
                            break;
                        case RoleTypes.Engineer:
                            Engineers.Add(pc);
                            Main.AllPlayerCustomRoles.Add(pc.PlayerId, CustomRoles.Engineer);
                            break;
                        case RoleTypes.GuardianAngel:
                            GuardianAngels.Add(pc);
                            Main.AllPlayerCustomRoles.Add(pc.PlayerId, CustomRoles.GuardianAngel);
                            break;
                        case RoleTypes.Shapeshifter:
                            Shapeshifters.Add(pc);
                            Main.AllPlayerCustomRoles.Add(pc.PlayerId, CustomRoles.Shapeshifter);
                            break;
                        case RoleTypes.CrewmateGhost:
                            CrewGhosts.Add(pc);
                            Main.AllPlayerCustomRoles.Add(pc.PlayerId, CustomRoles.CrewmateGhost);
                            break;
                        case RoleTypes.ImpostorGhost:
                            ImpGhosts.Add(pc);
                            Main.AllPlayerCustomRoles.Add(pc.PlayerId, CustomRoles.ImpostorGhost);
                            break;
                        default:
                            Logger.SendInGame(string.Format(GetString("Error.InvalidRoleAssignment"), pc?.Data?.PlayerName));
                            break;
                    }
                }

                if (Options.CurrentGameMode() == CustomGameMode.HideAndSeek)
                {
                    if (!Options.FreeForAllOn.GetBool())
                    {
                        if (Options.SpeedrunGamemode.GetBool())
                        {
                            SetColorPatch.IsAntiGlitchDisabled = true;
                            AssignCustomRolesFromList(CustomRoles.Tasker, Crewmates, Crewmates.Count);
                            foreach (var pair in Main.AllPlayerCustomRoles)
                            {
                                ExtendedPlayerControl.RpcSetCustomRole(pair.Key, pair.Value);
                            }
                            SetColorPatch.IsAntiGlitchDisabled = true;
                        }
                        else if (!Options.SplatoonOn.GetBool())
                        {
                            SetColorPatch.IsAntiGlitchDisabled = true;
                            foreach (var pc in PlayerControl.AllPlayerControls)
                            {
                                if (pc.Is(RoleType.Impostor))
                                    pc.RpcSetColor(0);
                                else if (pc.Is(RoleType.Crewmate))
                                    pc.RpcSetColor(1);
                            }

                            //役職設定処理
                            AssignCustomRolesFromList(CustomRoles.HASFox, Crewmates);
                            AssignCustomRolesFromList(CustomRoles.HASTroll, Crewmates);
                            foreach (var pair in Main.AllPlayerCustomRoles)
                            {
                                //RPCによる同期
                                ExtendedPlayerControl.RpcSetCustomRole(pair.Key, pair.Value);
                            }
                            //色設定処理
                            SetColorPatch.IsAntiGlitchDisabled = true;
                        }
                        else
                        {
                            AssignCustomRolesFromList(CustomRoles.Supporter, Crewmates);
                            foreach (var pair in Main.AllPlayerCustomRoles)
                            {
                                //RPCによる同期
                                ExtendedPlayerControl.RpcSetCustomRole(pair.Key, pair.Value);
                            }
                            foreach (var pc in PlayerControl.AllPlayerControls)
                            {
                                if (pc.Is(RoleType.Impostor))
                                    Main.AllPlayerCustomRoles[pc.PlayerId] = CustomRoles.Painter;
                            }
                        }
                    }
                    else
                    {
                        foreach (var pair in Main.AllPlayerCustomRoles)
                        {
                            //RPCによる同期
                            ExtendedPlayerControl.RpcSetCustomRole(pair.Key, pair.Value);
                        }
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc.Is(RoleType.Impostor))
                                Main.AllPlayerCustomRoles[pc.PlayerId] = CustomRoles.Jackal;
                        }
                    }
                }
                else if (Options.CurrentGameMode() == CustomGameMode.Splatoon)
                {
                    SetColorPatch.IsAntiGlitchDisabled = true;

                    //役職設定処理
                    AssignCustomRolesFromList(CustomRoles.HASFox, Crewmates);
                    AssignCustomRolesFromList(CustomRoles.HASTroll, Crewmates);
                    foreach (var pair in Main.AllPlayerCustomRoles)
                    {
                        //RPCによる同期
                        ExtendedPlayerControl.RpcSetCustomRole(pair.Key, pair.Value);
                    }
                    //色設定処理
                    Utils.CustomSyncAllSettings();
                    SetColorPatch.IsAntiGlitchDisabled = true;
                }
                else if (Options.CurrentGameMode() == CustomGameMode.ColorWars)
                {

                }
                else if (Options.CurrentGameMode() == CustomGameMode.FFA)
                {
                    foreach (var pair in Main.AllPlayerCustomRoles)
                    {
                        //RPCによる同期
                        ExtendedPlayerControl.RpcSetCustomRole(pair.Key, pair.Value);
                    }
                }
                else
                {
                    Logger.Msg("RoleManager.SelectRoles.Postfix / Check (1/10)", "Load Check (Select Roles.Postfix)");
                    List<PlayerControl> AllPlayers = new();
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        AllPlayers.Add(pc);
                    }

                    Logger.Msg("RoleManager.SelectRoles.Postfix / Check (2/10)", "Load Check (Select Roles.Postfix)");

                    foreach (var role in Main.chosenNonNK)
                    {
                        if (role.IsEngineer())
                            AssignCustomRolesFromList(role, Engineers);
                        else
                            AssignCustomRolesFromList(role, Crewmates);
                    }
                    Logger.Msg("RoleManager.SelectRoles.Postfix / Check (3/10)", "Load Check (Select Roles.Postfix)");
                    foreach (var role in Main.chosenRoles)
                    {
                        if (role.IsEngineer())
                            AssignCustomRolesFromList(role, Engineers);
                        else
                            AssignCustomRolesFromList(role, Crewmates);
                    }
                    Logger.Msg("RoleManager.SelectRoles.Postfix / Check (4/10)", "Load Check (Select Roles.Postfix)");
                    foreach (var role in Main.chosenEngiRoles)
                    {
                        if (role.IsEngineer())
                            AssignCustomRolesFromList(role, Engineers);
                        else
                            AssignCustomRolesFromList(role, Crewmates);
                    }
                    Logger.Msg("RoleManager.SelectRoles.Postfix / Check (5/10)", "Load Check (Select Roles.Postfix)");
                    foreach (var role in Main.chosenImpRoles)
                    {
                        AssignCustomRolesFromList(role, Impostors);
                    }
                    Logger.Msg("RoleManager.SelectRoles.Postfix / Check (6/10)", "Load Check (Select Roles.Postfix)");
                    foreach (var role in Main.chosenScientistRoles)
                    {
                        AssignCustomRolesFromList(role, Scientists);
                    }
                    Logger.Msg("RoleManager.SelectRoles.Postfix / Check (7/10)", "Load Check (Select Roles.Postfix)");
                    foreach (var role in Main.chosenShifterRoles)
                    {
                        if (!role.IsNeutral() | role.IsImpostor())
                            AssignCustomRolesFromList(role, Shapeshifters);
                        else if (role == CustomRoles.Egoist)
                            if (Main.RealOptionsData.NumImpostors > 1)
                                AssignCustomRolesFromList(role, Shapeshifters);
                    }
                    Logger.Msg("RoleManager.SelectRoles.Postfix / Check (8/10)", "Load Check (Select Roles.Postfix)");
                    if (RoleGoingInList(CustomRoles.LoversRecode))
                        AssignLoversRoles(2);

                    if (RoleGoingInList(CustomRoles.Torch))
                        GiveModifier(CustomRoles.Torch);
                    if (RoleGoingInList(CustomRoles.Bait))
                        GiveModifier(CustomRoles.Bait);
                    if (RoleGoingInList(CustomRoles.Bewilder))
                        GiveModifier(CustomRoles.Bewilder);
                    if (RoleGoingInList(CustomRoles.Diseased))
                        GiveModifier(CustomRoles.Diseased);

                    if (RoleGoingInList(CustomRoles.DoubleShot))
                        GiveModifier(CustomRoles.DoubleShot);
                    if (RoleGoingInList(CustomRoles.Oblivious))
                        GiveModifier(CustomRoles.Oblivious);
                    if (RoleGoingInList(CustomRoles.Obvious))
                        GiveModifier(CustomRoles.Obvious);
                    if (RoleGoingInList(CustomRoles.Flash))
                        GiveModifier(CustomRoles.Flash);
                    if (RoleGoingInList(CustomRoles.Sleuth))
                        GiveModifier(CustomRoles.Sleuth);
                    if (RoleGoingInList(CustomRoles.TieBreaker))
                        GiveModifier(CustomRoles.TieBreaker);
                    if (RoleGoingInList(CustomRoles.Watcher))
                        GiveModifier(CustomRoles.Watcher);

                    Logger.Msg("RoleManager.SelectRoles.Postfix / Check (9/10)", "Load Check (Select Roles.Postfix)");

                    //RPCによる同期
                    bool jackalExists = false;
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc.Is(CustomRoles.Jackal)) jackalExists = true;
                        if (pc.Is(CustomRoles.PlagueBearer) && Options.InfectionSkip.GetBool())
                        {
                            Main.AllPlayerCustomRoles[pc.PlayerId] = CustomRoles.Pestilence;
                            Main.TeamPestiAlive = true;
                        }
                        if (pc.Is(CustomRoles.Vampire) && Options.VampireDitchesOn.GetBool() && !Main.VampireDitchesOn)
                        {
                            Main.AllPlayerCustomRoles[pc.PlayerId] = CustomRoles.Coven;
                            Main.VampireDitchesOn = true;
                        }

                        if (pc.PlayerId == PlayerControl.LocalPlayer.PlayerId && Main.CachedDevMode)
                        {
                            //Main.AllPlayerCustomRoles[pc.PlayerId] = CustomRoles.Bomber;
                        }
                    }
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc.Is(CustomRoles.Sidekick))
                            if (!jackalExists)
                                Main.AllPlayerCustomRoles[pc.PlayerId] = CustomRoles.Jackal;
                    }

                    if (CustomRoles.Coven.IsEnable())
                    {
                        List<PlayerControl> AllCovenPlayers = new();
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc.Is(CustomRoles.Coven))
                                AllCovenPlayers.Add(pc);
                        }
                        CustomRpcSender sender = CustomRpcSender.Create("SelectRoles Sender", SendOption.Reliable);
                        RpcSetRoleReplacer.StartReplace(sender);
                        ForceAssignRole(CustomRoles.CovenWitch, AllCovenPlayers, sender, BaseRole: RoleTypes.Impostor, skip: false);
                        if (Options.HexMasterOn.GetBool())
                            ForceAssignRole(CustomRoles.HexMaster, AllCovenPlayers, sender, BaseRole: RoleTypes.Impostor, skip: false);
                        //  if (Options.PotionMasterOn.GetBool())
                        //      ForceAssignRole(CustomRoles.PotionMaster, AllCovenPlayers, sender, BaseRole: RoleTypes.Impostor, skip: false);
                        if (Options.VampireDitchesOn.GetBool())
                            ForceAssignRole(CustomRoles.Poisoner, AllCovenPlayers, sender, BaseRole: RoleTypes.Impostor, skip: false);
                        if (Options.MedusaOn.GetBool())
                            ForceAssignRole(CustomRoles.Medusa, AllCovenPlayers, sender, BaseRole: RoleTypes.Impostor, skip: false);
                        // if (Options.MimicOn.GetBool())
                        //    ForceAssignRole(CustomRoles.Mimic, AllCovenPlayers, sender, BaseRole: RoleTypes.Impostor, skip: false);
                        // if (Options.NecromancerOn.GetBool())
                        //   ForceAssignRole(CustomRoles.Necromancer, AllCovenPlayers, sender, BaseRole: RoleTypes.Impostor, skip: false);
                    }
                    Logger.Msg("RoleManager.SelectRoles.Postfix / Check (10/10)", "Load Check (Select Roles.Postfix)");
                    Logger.Msg("RoleManager.SelectRoles.Postfix Pt. 2 / Check (1/6)", "Load Check (Select Roles.Postfix Pt. 2)");
                    foreach (var pair in Main.AllPlayerCustomRoles)
                    {
                        ExtendedPlayerControl.RpcSetCustomRole(pair.Key, pair.Value);
                    }
                    foreach (var pair in Main.AllPlayerCustomSubRoles)
                    {
                        ExtendedPlayerControl.RpcSetCustomRole(pair.Key, pair.Value);
                    }
                    Logger.Msg("RoleManager.SelectRoles.Postfix Pt. 2 / Check (2/6)", "Load Check (Select Roles.Postfix Pt. 2)");

                    HudManager.Instance.SetHudActive(true);
                    Main.KillOrSpell = new Dictionary<byte, bool>();
                    if (Options.CovenMeetings.GetInt() == 0)
                        Main.HasNecronomicon = true;
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        Main.KillCount.Add(pc.PlayerId, 0);
                        if (pc.Data.Role.Role == RoleTypes.Shapeshifter) Main.CheckShapeshift.Add(pc.PlayerId, false);
                        switch (pc.GetCustomRole())
                        {
                            case CustomRoles.VoteStealer:
                                Main.MayorUsedButtonCount[pc.PlayerId] = 0;
                                Main.PickpocketKills.Add(pc.PlayerId, 0);
                                break;
                            case CustomRoles.BountyHunter:
                                BountyHunter.Add(pc);
                                break;
                            case CustomRoles.Manipulator:
                                Manipulator.Add(pc);
                                break;
                            case CustomRoles.AgiTater:
                                AgiTater.Add(pc);
                                break;
                            case CustomRoles.SerialKiller:
                                SerialKiller.Add(pc.PlayerId);
                                break;
                            case CustomRoles.Escapist:
                                Escapist.Add(pc);
                                break;
                            case CustomRoles.Witch:
                                Main.KillOrSpell.Add(pc.PlayerId, false);
                                break;
                            case CustomRoles.TheGlitch:
                            case CustomRoles.Warlock:
                            case CustomRoles.Consort:
                            case CustomRoles.Escort:
                                Main.CursedPlayers.Add(pc.PlayerId, null);
                                Main.isCurseAndKill.Add(pc.PlayerId, false);
                                break;
                            case CustomRoles.Veteran:
                                //Main.VetAlerts = Options.NumOfVets.GetInt();
                                break;
                            case CustomRoles.FireWorks:
                                FireWorks.Add(pc.PlayerId);
                                break;
                            case CustomRoles.Silencer:
                                Main.KillOrSilence.Add(pc.PlayerId, false);
                                break;
                            case CustomRoles.TimeThief:
                                TimeThief.Add(pc, pc.PlayerId);
                                break;
                            case CustomRoles.Sniper:
                                Sniper.Add(pc.PlayerId);
                                break;
                            case CustomRoles.Mare:
                                Mare.Add(pc.PlayerId);
                                break;
                            case CustomRoles.Ninja:
                                Ninja.Add(pc.PlayerId);
                                break;
                            case CustomRoles.Bomber:
                                Bomber.Add(pc);
                                break;
                            case CustomRoles.Necromancer:
                                Necromancer.Add(pc.PlayerId);
                                break;
                            case CustomRoles.Arsonist:
                                foreach (var ar in PlayerControl.AllPlayerControls)
                                    if (!ar.Is(CustomRoles.Phantom))
                                        Main.isDoused.Add((pc.PlayerId, ar.PlayerId), false);
                                break;
                            case CustomRoles.PlagueBearer:
                                foreach (var ar in PlayerControl.AllPlayerControls)
                                    if (!ar.Is(CustomRoles.Phantom))
                                        Main.isInfected.Add((pc.PlayerId, ar.PlayerId), false);
                                break;
                            case CustomRoles.Survivor:
                                Main.SurvivorStuff.Add(pc.PlayerId, (0, false, false, false, true));
                                break;
                            case CustomRoles.Cleaner:
                                Main.CleanerCanClean.Add(pc.PlayerId, true);
                                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RpcSetCleanerClean, Hazel.SendOption.Reliable, -1);
                                writer.Write(pc.PlayerId);
                                writer.Write(true);
                                AmongUsClient.Instance.FinishRpcImmediately(writer);
                                break;
                            case CustomRoles.Swapper:
                            case CustomRoles.Executioner:
                                List<PlayerControl> targetList = new();
                                rand = new System.Random();
                                foreach (var target in PlayerControl.AllPlayerControls)
                                {
                                    if (pc == target) continue;
                                    if (!Options.ExecutionerCanTargetImpostor.GetBool() && target.GetCustomRole().IsImpostor() | target.GetCustomRole().IsMadmate()) continue;
                                    if (target.GetCustomRole().IsNeutral()) continue;
                                    if (target.GetCustomRole().IsCoven()) continue;
                                    if (target.Is(CustomRoles.Phantom)) continue;
                                    if (target.Is(CustomRoles.GM)) continue;
                                    if (Main.ExecutionerTarget.ContainsValue(target.PlayerId)) continue;
                                    if (target == null || target.Data.IsDead || target.Data.Disconnected) continue;

                                    targetList.Add(target);
                                }
                                var Target = targetList[rand.Next(targetList.Count)];
                                Main.ExecutionerTarget.Add(pc.PlayerId, Target.PlayerId);
                                RPC.SendExecutionerTarget(pc.PlayerId, Target.PlayerId);
                                Logger.Info($"{pc.GetNameWithRole()}:{Target.GetNameWithRole()}", "Executioner/Swapper");
                                break;
                            case CustomRoles.GuardianAngelTOU:
                                List<PlayerControl> protectList = new();
                                rand = new System.Random();
                                foreach (var target in PlayerControl.AllPlayerControls)
                                {
                                    if (pc == target) continue;
                                    if (target.Is(CustomRoles.Phantom)) continue;
                                    if (target.Is(CustomRoles.GM)) continue;

                                    protectList.Add(target);
                                }
                                var Person = protectList[rand.Next(protectList.Count)];
                                Main.GuardianAngelTarget.Add(pc.PlayerId, Person.PlayerId);
                                RPC.SendGATarget(pc.PlayerId, Person.PlayerId);
                                Logger.Info($"{pc.GetNameWithRole()}:{Person.GetNameWithRole()}", "Guardian Angel");
                                break;
                            case CustomRoles.Egoist:
                                Egoist.Add(pc.PlayerId);
                                break;
                            case CustomRoles.IdentityTheft:
                                Main.CheckShapeshift.Add(pc.PlayerId, false);
                                break;
                            case CustomRoles.Dictator:
                                Main.DictatesRemaining.Add(pc.PlayerId, Options.DictatesUntilSuicde.GetInt());
                                break;
                            case CustomRoles.Sheriff:
                                Sheriff.Add(pc.PlayerId);
                                break;
                            case CustomRoles.Mayor:
                                Main.MayorUsedButtonCount[pc.PlayerId] = 0;
                                break;
                            case CustomRoles.Hacker:
                                Main.HackerFixedSaboCount[pc.PlayerId] = 0;
                                break;
                            case CustomRoles.CrewPostor:
                                Main.lastAmountOfTasks.Add(pc.PlayerId, 0);
                                break;
                            case CustomRoles.SabotageMaster:
                                SabotageMaster.Add(pc.PlayerId);
                                break;
                            case CustomRoles.HexMaster:
                                foreach (var ar in PlayerControl.AllPlayerControls)
                                {
                                    if (!ar.GetCustomRole().IsCoven())
                                        if (!ar.Is(CustomRoles.Phantom))
                                            Main.isHexed.Add((pc.PlayerId, ar.PlayerId), false);
                                }
                                break;
                            case CustomRoles.Medium:
                                Main.knownGhosts.Add(pc.PlayerId, new List<byte>());
                                break;
                            case CustomRoles.Investigator:
                                Investigator.Add(pc.PlayerId);
                                foreach (var ar in PlayerControl.AllPlayerControls)
                                {
                                    Investigator.hasSeered.Add(ar.PlayerId, false);
                                }
                                break;
                            case CustomRoles.Postman:
                                Postman.Add(pc);
                                foreach (var ar in PlayerControl.AllPlayerControls)
                                {
                                    if (ar.PlayerId != pc.PlayerId)
                                        Postman.hasDelivered.Add(ar.PlayerId, false);
                                }
                                break;
                            /*case CustomRoles.Sleuth:
                                foreach (var ar in PlayerControl.AllPlayerControls)
                                {
                                    Main.SleuthReported.Add(pc.PlayerId, (ar.PlayerId, false));
                                }
                                break;*/
                            case CustomRoles.EvilGuesser:
                            case CustomRoles.NiceGuesser:
                                Guesser.Add(pc.PlayerId);
                                break;
                            case CustomRoles.Pirate:
                                Guesser.Add(pc.PlayerId);
                                Guesser.PirateGuess.Add(pc.PlayerId, 0);
                                break;
                            case CustomRoles.Phantom:
                                Main.lastAmountOfTasks.Add(pc.PlayerId, 0);
                                break;
                            case CustomRoles.Medic:
                            case CustomRoles.Bodyguard:
                            case CustomRoles.Oracle:
                            case CustomRoles.Crusader:
                                Main.CurrentTarget.Add(pc.PlayerId, 255);
                                Main.HasTarget.Add(pc.PlayerId, false);
                                break;
                        }
                        pc.ResetKillCooldown();
                    }
                    if (Guesser.IsEnable()) Guesser.SetRoleAndNumber();
                    Logger.Msg("RoleManager.SelectRoles.Postfix Pt. 2 / Check (3/6)", "Load Check (Select Roles.Postfix Pt. 2)");

                    //役職の人数を戻す
                    RoleOptionsCollectionV07 roleOpt = GameOptionsManager.Instance.currentNormalGameOptions.roleOptions;
                    int ScientistNum = roleOpt.GetNumPerGame(RoleTypes.Scientist);
                    ScientistNum -= Main.chosenScientistRoles.Count;
                    roleOpt.SetRoleRate(RoleTypes.Scientist, ScientistNum, roleOpt.GetChancePerGame(RoleTypes.Scientist));
                    Logger.Msg("RoleManager.SelectRoles.Postfix Pt. 2 / Check (4/6)", "Load Check (Select Roles.Postfix Pt. 2)");

                    int EngineerNum = roleOpt.GetNumPerGame(RoleTypes.Engineer);
                    EngineerNum -= Main.chosenEngiRoles.Count;
                    roleOpt.SetRoleRate(RoleTypes.Engineer, EngineerNum, roleOpt.GetChancePerGame(RoleTypes.Engineer));
                    Logger.Msg("RoleManager.SelectRoles.Postfix Pt. 2 / Check (5/6)", "Load Check (Select Roles.Postfix Pt. 2)");

                    int ShapeshifterNum = roleOpt.GetNumPerGame(RoleTypes.Shapeshifter);
                    ShapeshifterNum -= Main.chosenShifterRoles.Count;
                    roleOpt.SetRoleRate(RoleTypes.Shapeshifter, ShapeshifterNum, roleOpt.GetChancePerGame(RoleTypes.Shapeshifter));
                    Logger.Msg("RoleManager.SelectRoles.Postfix Pt. 2 / Check (6/6)", "Load Check (Select Roles.Postfix Pt. 2)");
                }

                Main.ResetCamPlayerList.AddRange(PlayerControl.AllPlayerControls.ToArray().Where(p => p.IsDesyncRole()).Select(p => p.PlayerId));
                Utils.CountAliveImpostors();
                Utils.CustomSyncAllSettings();
                SetColorPatch.IsAntiGlitchDisabled = false;
                Logger.Msg("RoleManager.SelectRoles.Postfix / CHECK DONE !", "Load Check (Select Roles.Postfix )");
            }
            catch (Exception e)
            {
                Logger.Error($"Error encountered while assigning roles.\n{e}", "SelectRoles.Postfix");
            }
        }
        private static void AssignDesyncRole(CustomRoles role, List<PlayerControl> AllPlayers, CustomRpcSender sender, RoleTypes BaseRole, RoleTypes hostBaseRole = RoleTypes.Crewmate, int Count = -1)
        {
            if (!role.IsEnable()) return;

            var count = role.GetCount();

            if (Count != -1)
                count = Count;
            for (var i = 0; i < count; i++)
            {
                if (AllPlayers.Count <= 0) break;
                if (Count == -1)
                {
                    if (!RoleGoingInList(role)) break;
                }
                var rand = new System.Random();
                var player = AllPlayers[rand.Next(0, AllPlayers.Count)];
                AllPlayers.Remove(player);
                Main.AllPlayerCustomRoles[player.PlayerId] = role;
                if (!player.IsModClient() || hostBaseRole == RoleTypes.Shapeshifter)
                {
                    int playerCID = player.GetClientId();
                    sender.RpcSetRole(player, BaseRole, playerCID);
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc == player) continue;
                        sender.RpcSetRole(pc, RoleTypes.Scientist, playerCID);
                    }
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc == player) continue;
                        if (pc.PlayerId == 0) player.SetRole(RoleTypes.Scientist); //ホスト視点用
                        else sender.RpcSetRole(player, RoleTypes.Scientist, pc.GetClientId());
                    }
                }
                else
                {
                    player.SetRole(hostBaseRole);
                    sender.RpcSetRole(player, hostBaseRole);
                }
                player.Data.IsDead = true;
            }
        }
        private static void AssignDesyncShiftingRole(CustomRoles role, List<PlayerControl> AllPlayers, CustomRpcSender sender, int Count = -1)
        {
            if (!role.IsEnable()) return;

            var count = role.GetCount();

            if (Count != -1)
                count = Count;
            for (var i = 0; i < count; i++)
            {
                if (AllPlayers.Count <= 0) break;
                if (Count == -1)
                {
                    if (!RoleGoingInList(role)) break;
                }
                var rand = new System.Random();
                var player = AllPlayers[rand.Next(0, AllPlayers.Count)];
                AllPlayers.Remove(player);
                Main.AllPlayerCustomRoles[player.PlayerId] = role;
                //ここからDesyncが始まる
                int playerCID = player.GetClientId();
                sender.RpcSetRole(player, RoleTypes.Shapeshifter, playerCID);
                //Desyncする人視点で他プレイヤーを科学者にするループ
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc == player) continue;
                    sender.RpcSetRole(pc, RoleTypes.Scientist, playerCID);
                }
                //他視点でDesyncする人の役職を科学者にするループ
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc == player) continue;
                    if (pc.PlayerId == 0) player.SetRole(RoleTypes.Scientist); //ホスト視点用
                    else sender.RpcSetRole(player, RoleTypes.Scientist, pc.GetClientId());
                }
                player.Data.IsDead = true;
            }
        }
        private static void ForceAssignRole(CustomRoles role, List<PlayerControl> AllPlayers, CustomRpcSender sender, RoleTypes BaseRole, RoleTypes hostBaseRole = RoleTypes.Crewmate, bool skip = false, int Count = -1)
        {
            var count = 1;

            if (Count != -1)
                count = Count;
            for (var i = 0; i < count; i++)
            {
                if (AllPlayers.Count <= 0) break;
                var rand = new System.Random();
                var player = AllPlayers[rand.Next(0, AllPlayers.Count)];
                AllPlayers.Remove(player);
                Main.AllPlayerCustomRoles[player.PlayerId] = role;
                if (!skip)
                {
                    if (!player.IsModClient())
                    {
                        int playerCID = player.GetClientId();
                        sender.RpcSetRole(player, BaseRole, playerCID);
                        //Desyncする人視点で他プレイヤーを科学者にするループ
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc == player) continue;
                            sender.RpcSetRole(pc, RoleTypes.Scientist, playerCID);
                        }
                        //他視点でDesyncする人の役職を科学者にするループ
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc == player) continue;
                            if (pc.PlayerId == 0) player.SetRole(RoleTypes.Scientist); //ホスト視点用
                            else sender.RpcSetRole(player, RoleTypes.Scientist, pc.GetClientId());
                        }
                    }
                    else
                    {
                        //ホストは別の役職にする
                        player.SetRole(hostBaseRole); //ホスト視点用
                        sender.RpcSetRole(player, hostBaseRole);
                    }
                }
            }
        }
        private static void AssignPainters(CustomRoles role, List<PlayerControl> AllPlayers, CustomRpcSender sender, RoleTypes BaseRole, RoleTypes hostBaseRole = RoleTypes.Crewmate, bool skip = false)
        {
            var count = AllPlayers.Count - CustomRoles.Supporter.GetCount();
            for (var i = 0; i < count; i++)
            {
                if (AllPlayers.Count <= 0) break;
                var rand = new System.Random();
                var player = AllPlayers[rand.Next(0, AllPlayers.Count)];
                AllPlayers.Remove(player);
                Main.AllPlayerCustomRoles[player.PlayerId] = role;
                if (!skip)
                {
                    if (!player.IsModClient())
                    {
                        int playerCID = player.GetClientId();
                        sender.RpcSetRole(player, BaseRole, playerCID);
                        //Desyncする人視点で他プレイヤーを科学者にするループ
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc == player) continue;
                            sender.RpcSetRole(pc, RoleTypes.Scientist, playerCID);
                        }
                        //他視点でDesyncする人の役職を科学者にするループ
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc == player) continue;
                            if (pc.PlayerId == 0) player.SetRole(RoleTypes.Scientist); //ホスト視点用
                            else sender.RpcSetRole(player, RoleTypes.Scientist, pc.GetClientId());
                        }
                    }
                    else
                    {
                        //ホストは別の役職にする
                        player.SetRole(hostBaseRole); //ホスト視点用
                        sender.RpcSetRole(player, hostBaseRole);
                    }
                }
            }
        }
        private static List<PlayerControl> AssignCustomRolesFromList(CustomRoles role, List<PlayerControl> players, int RawCount = -1)
        {
            if (players == null || players.Count <= 0) return null;
            var rand = new System.Random();
            var count = Math.Clamp(RawCount, 0, players.Count);
            if (RawCount == -1) count = Math.Clamp(role.GetCount(), 0, players.Count);
            if (role is CustomRoles.Vampress) count = 1;
            if (count <= 0) return null;
            List<PlayerControl> AssignedPlayers = new();
            SetColorPatch.IsAntiGlitchDisabled = true;
            for (var i = 0; i < count; i++)
            {
                if (Options.CheckRoleTwiceBeforeAdd.GetBool())
                    if (!RoleGoingInList(role)) return null;
                /*  float RoleRate = role.GetChance();
                  bool IsChosen = UnityEngine.Random.Range(1, 100) < RoleRate;
                  if (IsChosen || Options.CurrentGameMode() == CustomGameMode.HideAndSeek)
                  {*/
                var player = players[rand.Next(0, players.Count)];
                AssignedPlayers.Add(player);
                players.Remove(player);
                Main.AllPlayerCustomRoles[player.PlayerId] = role;
                Logger.Info("役職設定:" + player?.Data?.PlayerName + " = " + role.ToString(), "AssignRoles");

                if (Options.CurrentGameMode() == CustomGameMode.HideAndSeek)
                {
                    if (player.Is(CustomRoles.HASTroll))
                        player.RpcSetColor(2);
                    else if (player.Is(CustomRoles.HASFox))
                        player.RpcSetColor(3);
                }
                // }
            }
            SetColorPatch.IsAntiGlitchDisabled = false;
            return AssignedPlayers;
        }
        private static List<PlayerControl> AssignCovenRoles(CustomRoles role, List<PlayerControl> players, int RawCount = -1)
        {
            if (players == null || players.Count <= 0) return null;
            var rand = new System.Random();
            var count = Math.Clamp(RawCount, 0, players.Count);
            if (RawCount == -1) count = Math.Clamp(1, 0, players.Count);
            if (count <= 0) return null;
            List<PlayerControl> AssignedPlayers = new();
            SetColorPatch.IsAntiGlitchDisabled = true;
            for (var i = 0; i < count; i++)
            {
                var player = players[rand.Next(0, players.Count)];
                AssignedPlayers.Add(player);
                players.Remove(player);
                Main.AllPlayerCustomRoles[player.PlayerId] = role;
                Logger.Info("役職設定:" + player?.Data?.PlayerName + " = " + role.ToString(), "AssignRoles");
            }
            SetColorPatch.IsAntiGlitchDisabled = false;
            return AssignedPlayers;
        }

        private static bool RoleGoingInList(CustomRoles role)
        {
            if (!role.IsEnable()) return false;
            var number = Convert.ToUInt32(PercentageChecker.CheckPercentage(role.ToString(), role: role));
            bool isRole = UnityEngine.Random.RandomRange(1, 100) <= number;
            return isRole;
        }

        private static void AssignLoversRoles(int RawCount = -1)
        {
            var allPlayers = new List<PlayerControl>();
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.Is(CustomRoles.Child)) continue;
                if (player.Is(CustomRoles.Phantom)) continue;
                if (player.Is(CustomRoles.GM)) continue;
                if (Main.HasModifier.ContainsKey(player.PlayerId)) continue;
                allPlayers.Add(player);
            }
            var loversRole = CustomRoles.LoversRecode;
            var rand = new System.Random();
            var count = Math.Clamp(RawCount, 0, allPlayers.Count);
            if (RawCount == -1) count = Math.Clamp(loversRole.GetCount(), 0, allPlayers.Count);
            if (count <= 0) return;

            var player1 = allPlayers[rand.Next(0, allPlayers.Count)];
            allPlayers.Remove(player1);
            var player2 = allPlayers[rand.Next(0, allPlayers.Count)];
            allPlayers.Remove(player2);
            List<PlayerControl> lovers = new();
            lovers.Add(player1);
            lovers.Add(player2);

            if (player1 && player2 != null)
                for (var i = 0; i < count; i++)
                {
                    var player = lovers[rand.Next(0, lovers.Count)];
                    Main.LoversPlayers.Add(player);
                    lovers.Remove(player);
                    Main.AllPlayerCustomSubRoles[player.PlayerId] = loversRole;
                    Main.HasModifier.Add(player.PlayerId, loversRole);
                    Logger.Info("役職設定:" + player?.Data?.PlayerName + " = " + player.GetCustomRole().ToString() + " + " + loversRole.ToString(), "AssignLovers");
                }
            RPC.SyncLoversPlayers();
        }

        public static void MakeDesyncSender(Dictionary<byte, CustomRpcSender> senders, Dictionary<(byte, byte), RoleTypes> rolesMap)
        {
            var hostId = PlayerControl.LocalPlayer.PlayerId;
            foreach (var seer in PlayerControl.AllPlayerControls)
            {
                var sender = senders[seer.PlayerId];
                foreach (var target in PlayerControl.AllPlayerControls)
                {
                    if (rolesMap.TryGetValue((seer.PlayerId, target.PlayerId), out var role))
                    {
                        sender.RpcSetRole(seer, role, target.GetClientId());
                    }
                }
            }
        }


        private static void GiveModifier(CustomRoles role, int RawCount = -1)
        {
            if (!role.IsEnable()) return;
            var allPlayers = new List<PlayerControl>();
            if (role is CustomRoles.Flash)
            {
                bool Escalation = UnityEngine.Random.RandomRange(1, 100) <= 10;
                if (Escalation && Options.EnableHiddenRoles.GetBool())
                    role = CustomRoles.Escalation;
            }
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.Is(CustomRoles.GM)) continue;
                if (Main.AllPlayerCustomRoles[player.PlayerId] == CustomRoles.LoversRecode) continue;
                if (Main.HasModifier.ContainsKey(player.PlayerId)) continue;
                if (player.Is(CustomRoles.Phantom)) continue;
                if (Options.ModifierRestrict.GetBool())
                    if (player.Is(CustomRoles.GM)) continue;
                {
                    switch (role)
                    {
                        case CustomRoles.Sleuth:
                            if (player.GetCustomRole() is CustomRoles.Medium or CustomRoles.Amnesiac or CustomRoles.Vulture or CustomRoles.Cleaner) continue;
                            break;
                        case CustomRoles.TieBreaker:
                            break;
                        case CustomRoles.Oblivious:
                            if (player.GetCustomRole() is CustomRoles.Medium or CustomRoles.Amnesiac or CustomRoles.Vulture or CustomRoles.Cleaner) continue;
                            break;
                        case CustomRoles.Escalation:
                        case CustomRoles.Flash:
                            if (player.GetCustomRole() is CustomRoles.SpeedBooster or CustomRoles.Mare) continue;
                            break;
                        case CustomRoles.Bait:
                            if (player.Is(CustomRoles.Trapper)) continue;
                            break;
                        case CustomRoles.Bewilder:
                            break;
                        case CustomRoles.DoubleShot:
                            if (player.GetCustomRole() is not CustomRoles.EvilGuesser and not CustomRoles.NiceGuesser) continue;
                            break;
                        case CustomRoles.Torch:
                            if (player.Is(CustomRoles.Lighter)) continue;
                            break;
                    }
                    if (role.IsCrewModifier())
                    {
                        if (!player.GetCustomRole().CanGetCrewModifier()) continue;
                    }
                }
                allPlayers.Add(player);
            }
            var rand = new System.Random();
            var count = 1;

            if (allPlayers.Count <= 0) return;

            for (var i = 0; i < count; i++)
            {
                var player = allPlayers[rand.Next(0, allPlayers.Count)];
                Main.HasModifier.Add(player.PlayerId, role);
                allPlayers.Remove(player);
                Main.AllPlayerCustomSubRoles[player.PlayerId] = role;
                Logger.Info("役職設定:" + player?.Data?.PlayerName + " = " + player.GetCustomRole().ToString() + " + " + role.ToString(), "Assign Modifier");
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetRole))]
        class RpcSetRoleReplacer
        {
            public static bool doReplace = false;
            public static CustomRpcSender sender;
            public static List<(PlayerControl, RoleTypes)> StoragedData = new();
            public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] RoleTypes roleType)
            {
                if (doReplace && sender != null)
                {
                    StoragedData.Add((__instance, roleType));
                    return false;
                }
                else return true;
            }
            public static void Release()
            {
                sender.StartMessage(-1);
                foreach (var pair in StoragedData)
                {
                    pair.Item1.SetRole(pair.Item2);
                    sender.StartRpc(pair.Item1.NetId, RpcCalls.SetRole)
                        .Write((ushort)pair.Item2)
                        .EndRpc();
                }
                sender.EndMessage();
                doReplace = false;
            }
            public static void StartReplace(CustomRpcSender sender)
            {
                RpcSetRoleReplacer.sender = sender;
                StoragedData = new();
                doReplace = true;
            }
        }
    }
}
