using System.Linq;
using System;
using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using UnityEngine;
using InnerNet;
using static TownOfHost.Translator;

namespace TownOfHost
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    class ChangeRoleSettings
    {
        public static void Postfix(AmongUsClient __instance)
        {
            //注:この時点では役職は設定されていません。
            PlayerState.Init();

            Main.currentWinner = CustomWinner.Default;
            Main.CustomWinTrigger = false;
            Main.AllPlayerCustomRoles = new Dictionary<byte, CustomRoles>();
            Main.AllPlayerCustomSubRoles = new Dictionary<byte, CustomRoles>();
            Main.AllPlayerKillCooldown = new Dictionary<byte, float>();
            Main.AllPlayerSpeed = new Dictionary<byte, float>();
            Main.BitPlayers = new Dictionary<byte, (byte, float)>();
            Main.WarlockTimer = new Dictionary<byte, float>();
            Main.isDoused = new Dictionary<(byte, byte), bool>();
            Main.isHexed = new Dictionary<(byte, byte), bool>();
            Main.ArsonistTimer = new Dictionary<byte, (PlayerControl, float)>();
            Main.isInfected = new Dictionary<(byte, byte), bool>();
            Main.PlagueBearerTimer = new Dictionary<byte, (PlayerControl, float)>();
            Main.CursedPlayers = new Dictionary<byte, PlayerControl>();
            Main.isCurseAndKill = new Dictionary<byte, bool>();
            Main.AirshipMeetingTimer = new Dictionary<byte, float>();
            Main.ExecutionerTarget = new Dictionary<byte, byte>();
            Main.GuardianAngelTarget = new Dictionary<byte, byte>();
            Main.SKMadmateNowCount = 0;
            Main.HexesThisRound = 0;
            Main.isCursed = false;
            Main.PuppeteerList = new Dictionary<byte, byte>();
            Main.WitchedList = new Dictionary<byte, byte>();

            Main.AfterMeetingDeathPlayers = new();
            Main.ResetCamPlayerList = new();

            Main.SpelledPlayer = new List<PlayerControl>();
            Main.witchMeeting = false;
            Main.firstKill = new List<PlayerControl>();
            Main.unreportableBodies = new List<byte>();
            Main.isSilenced = false;
            Main.SilencedPlayer = new List<PlayerControl>();
            Main.CheckShapeshift = new Dictionary<byte, bool>();
            Main.SpeedBoostTarget = new Dictionary<byte, byte>();
            Main.MayorUsedButtonCount = new Dictionary<byte, int>();
            Main.HackerFixedSaboCount = new Dictionary<byte, int>();
            Main.KilledBewilder = new();
            Main.KilledDemo = new();
            Main.targetArrows = new();
            Main.JugKillAmounts = 0;
            Main.AteBodies = 0;
            Main.TeamJuggernautAlive = false;
            Main.TeamPestiAlive = false;
            Main.CamoComms = false;

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
            Main.WitchProtected = false;
            ////////////// COVEN INFO //////////////    

            Options.UsedButtonCount = 0;
            Main.RealOptionsData = PlayerControl.GameOptions.DeepCopy();

            Main.introDestroyed = false;
            Main.VettedThisRound = false;
            Main.VetIsAlerted = false;
            Main.IsRoundOne = true;
            Main.IsRoundOneGA = true;
            Main.GAprotects = 0;
            Main.ProtectedThisRound = false;
            Main.HasProtected = false;

            Main.IsGazing = false;
            Main.GazeReady = true;

            Main.IsRampaged = false;
            Main.RampageReady = true;
            Main.IsHackMode = false;
            Main.bkProtected = false;
            Main.bombedVents = new List<int>();

            Main.DiscussionTime = Main.RealOptionsData.DiscussionTime;
            Main.VotingTime = Main.RealOptionsData.VotingTime;

            NameColorManager.Instance.RpcReset();
            Main.LastNotifyNames = new();

            Main.currentDousingTarget = 255;
            Main.VetAlerts = 0;
            Main.ProtectsSoFar = 0;
            Main.IsProtected = false;
            Main.PlayerColors = new();
            Main.whoKilledWho = new();
            //名前の記録
            Main.AllPlayerNames = new();

            Main.chosenRoles = new();
            Main.chosenDesyncRoles = new();
            Main.chosenNK = new();
            Main.chosenNonNK = new();
            Main.chosenImpRoles = new();
            Main.AllPlayerSkin = new();

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
                Main.AllPlayerSpeed[pc.PlayerId] = Main.RealOptionsData.PlayerSpeedMod; //移動速度をデフォルトの移動速度に変更
                pc.cosmetics.nameText.text = pc.name;
            }
            Main.VisibleTasksCount = true;
            if (__instance.AmHost)
            {
                SaveSkin();
                RPC.SyncCustomSettingsRPC();
                Main.RefixCooldownDelay = 0;
                if (Options.CurrentGameMode == CustomGameMode.HideAndSeek)
                {
                    Options.HideAndSeekKillDelayTimer = Options.KillDelay.GetFloat();
                }
                if (Options.IsStandardHAS)
                {
                    Options.HideAndSeekKillDelayTimer = Options.StandardHASWaitingTime.GetFloat();
                }
            }
            FallFromLadder.Reset();
            BountyHunter.Init();
            SerialKiller.Init();
            FireWorks.Init();
            Sniper.Init();
            TimeThief.Init();
            Mare.Init();
            Egoist.Init();
            Sheriff.Init();
            Investigator.Init();
            Camouflager.Init();
            Ninja.Init();
            Camouflague.did = false;
            Camouflague.IsActive = false;
            AntiBlackout.Reset();
        }
        private static void SaveSkin()
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                var color = player.CurrentOutfit.ColorId;
                var hat = player.CurrentOutfit.HatId;
                var skin = player.CurrentOutfit.SkinId;
                var visor = player.CurrentOutfit.VisorId;
                var pet = player.CurrentOutfit.PetId;
                var name = player.GetRealName();
                Main.AllPlayerSkin[player.PlayerId] = (color, hat, skin, visor, pet, name);
            }
        }
    }
    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
    class SelectRolesPatch
    {
        public static void Prefix()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            //CustomRpcSenderとRpcSetRoleReplacerの初期化
            CustomRpcSender sender = CustomRpcSender.Create("SelectRoles Sender", SendOption.Reliable);
            RpcSetRoleReplacer.StartReplace(sender);

            //ウォッチャーの陣営抽選
            Options.SetWatcherTeam(Options.EvilWatcherChance.GetFloat());

            var rand = new System.Random();

            /*
                        Main.chosenRoles = new();
                        Main.chosenDesyncRoles = new();
                        Main.chosenNK = new();
                        Main.chosenNonNK = new();
            */
            if (Options.CurrentGameMode != CustomGameMode.HideAndSeek)
            {
                //役職の人数を指定
                RoleOptionsData roleOpt = PlayerControl.GameOptions.RoleOptions;
                int ScientistNum = roleOpt.GetNumPerGame(RoleTypes.Scientist);
                int AdditionalScientistNum = CustomRoles.Doctor.GetCount();
                roleOpt.SetRoleRate(RoleTypes.Scientist, ScientistNum + AdditionalScientistNum, AdditionalScientistNum > 0 ? 100 : roleOpt.GetChancePerGame(RoleTypes.Scientist));

                int EngineerNum = roleOpt.GetNumPerGame(RoleTypes.Engineer);

                int AdditionalEngineerNum = CustomRoles.Madmate.GetCount() + CustomRoles.Terrorist.GetCount();// - EngineerNum;

                if (Options.MayorHasPortableButton.GetBool())
                    AdditionalEngineerNum += CustomRoles.Mayor.GetCount();

                if (Options.MadSnitchCanVent.GetBool())
                    AdditionalEngineerNum += CustomRoles.MadSnitch.GetCount();

                if (Options.JesterCanVent.GetBool())
                    AdditionalEngineerNum += CustomRoles.Jester.GetCount();

                if (CustomRoles.Bastion.IsEnable())
                    AdditionalEngineerNum += CustomRoles.Bastion.GetCount();

                if (CustomRoles.Veteran.IsEnable())
                    AdditionalEngineerNum += CustomRoles.Veteran.GetCount();

                if (CustomRoles.GuardianAngelTOU.IsEnable())
                    AdditionalEngineerNum += CustomRoles.GuardianAngelTOU.GetCount();

                roleOpt.SetRoleRate(RoleTypes.Engineer, EngineerNum + AdditionalEngineerNum, AdditionalEngineerNum > 0 ? 100 : roleOpt.GetChancePerGame(RoleTypes.Engineer));

                int ShapeshifterNum = roleOpt.GetNumPerGame(RoleTypes.Shapeshifter);
                int AdditionalShapeshifterNum = CustomRoles.SerialKiller.GetCount() + CustomRoles.TheGlitch.GetCount() + CustomRoles.BountyHunter.GetCount() + CustomRoles.Camouflager.GetCount() + CustomRoles.Warlock.GetCount()/* + CustomRoles.ShapeMaster.GetCount()*/ + CustomRoles.FireWorks.GetCount() + CustomRoles.Sniper.GetCount() + CustomRoles.Ninja.GetCount(); ;//- ShapeshifterNum;
                if (Main.RealOptionsData.NumImpostors > 1)
                    AdditionalShapeshifterNum += CustomRoles.Egoist.GetCount();
                //if (CustomRoles.TheGlitch.IsEnable())
                //   AdditionalShapeshifterNum += CustomRoles.TheGlitch.GetCount();
                roleOpt.SetRoleRate(RoleTypes.Shapeshifter, ShapeshifterNum + AdditionalShapeshifterNum, AdditionalShapeshifterNum > 0 ? 100 : roleOpt.GetChancePerGame(RoleTypes.Shapeshifter));


                List<PlayerControl> AllPlayers = new();
                List<PlayerControl> AllNKPlayers = new();
                List<PlayerControl> AllnonNKPlayers = new();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
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

                int numofNks = UnityEngine.Random.Range(Options.MinNK.GetInt(), Options.MaxNK.GetInt());
                int numofNonNks = UnityEngine.Random.Range(Options.MinNonNK.GetInt(), Options.MaxNonNK.GetInt());

                for (var i = 0; i < numofNks; i++)
                {
                    var rando = new System.Random();
                    var player = AllPlayers[rando.Next(0, AllPlayers.Count)];
                    AllPlayers.Remove(player);
                    AllNKPlayers.Add(player);
                }
                for (var i = 0; i < numofNonNks; i++)
                {
                    var rando = new System.Random();
                    var player = AllPlayers[rando.Next(0, AllPlayers.Count)];
                    AllPlayers.Remove(player);
                    AllnonNKPlayers.Add(player);
                }

                if (RoleGoingInList(CustomRoles.Sheriff))
                    AssignDesyncRole(CustomRoles.Sheriff, AllPlayers, sender, BaseRole: RoleTypes.Impostor);
                if (RoleGoingInList(CustomRoles.Investigator))
                    AssignDesyncRole(CustomRoles.Investigator, AllPlayers, sender, BaseRole: RoleTypes.Impostor);
                // ASSIGN NK ROLES //
                if (RoleGoingInList(CustomRoles.Arsonist))
                    Main.chosenNK.Add(CustomRoles.Arsonist);

                if (RoleGoingInList(CustomRoles.Jackal))
                    Main.chosenNK.Add(CustomRoles.Jackal);

                if (RoleGoingInList(CustomRoles.Juggernaut))
                    Main.chosenNK.Add(CustomRoles.Juggernaut);

                if (RoleGoingInList(CustomRoles.PlagueBearer))
                    Main.chosenNK.Add(CustomRoles.PlagueBearer);

                if (RoleGoingInList(CustomRoles.TheGlitch))
                    Main.chosenNK.Add(CustomRoles.TheGlitch);

                if (RoleGoingInList(CustomRoles.Werewolf))
                    Main.chosenNK.Add(CustomRoles.Werewolf);

                if (RoleGoingInList(CustomRoles.BloodKnight))
                    Main.chosenNK.Add(CustomRoles.BloodKnight);

                for (var i = 0; i < numofNks; i++)
                {
                    var rando = new System.Random();
                    var random = new System.Random();
                    var role = Main.chosenNK[rando.Next(0, Main.chosenNK.Count)];
                    var player = AllNKPlayers[random.Next(0, AllNKPlayers.Count)];
                    Main.chosenNK.Remove(role);
                    AllNKPlayers.Remove(player);
                    List<PlayerControl> urself = new();
                    urself.Add(player);
                    if (role == CustomRoles.TheGlitch)
                        AssignDesyncRole(CustomRoles.TheGlitch, urself, sender, BaseRole: RoleTypes.Shapeshifter);
                    else
                        AssignDesyncRole(role, urself, sender, BaseRole: RoleTypes.Impostor);
                }

                // ASSIGN NON-NK ROLES //
                /*
                CustomRoles.Vulture or
                CustomRoles.Opportunist or
                CustomRoles.SchrodingerCat or
                CustomRoles.Terrorist or
                CustomRoles.Executioner or
                CustomRoles.EgoSchrodingerCat or
                CustomRoles.GuardianAngelTOU or
                CustomRoles.Amnesiac or
                CustomRoles.JSchrodingerCat or
                CustomRoles.Hacker;*/

                if (RoleGoingInList(CustomRoles.Jester))
                    Main.chosenNonNK.Add(CustomRoles.Jester);

                if (RoleGoingInList(CustomRoles.Opportunist))
                    Main.chosenNonNK.Add(CustomRoles.Opportunist);

                if (RoleGoingInList(CustomRoles.SchrodingerCat))
                    Main.chosenNonNK.Add(CustomRoles.SchrodingerCat);

                if (RoleGoingInList(CustomRoles.Terrorist))
                    Main.chosenNonNK.Add(CustomRoles.Terrorist);

                if (RoleGoingInList(CustomRoles.Executioner))
                    Main.chosenNonNK.Add(CustomRoles.Executioner);

                if (RoleGoingInList(CustomRoles.SchrodingerCat))
                    Main.chosenNonNK.Add(CustomRoles.SchrodingerCat);

                if (RoleGoingInList(CustomRoles.GuardianAngelTOU))
                    Main.chosenNonNK.Add(CustomRoles.GuardianAngelTOU);

                if (RoleGoingInList(CustomRoles.Hacker))
                    Main.chosenNonNK.Add(CustomRoles.Hacker);

                for (var i = 0; i < numofNonNks; i++)
                {
                    var rando = new System.Random();
                    var random = new System.Random();
                    var role = Main.chosenNonNK[rando.Next(0, Main.chosenNonNK.Count)];
                    var player = AllnonNKPlayers[random.Next(0, AllnonNKPlayers.Count)];
                    Main.chosenNK.Remove(role);
                    AllnonNKPlayers.Remove(player);
                    List<PlayerControl> urself = new();
                    urself.Add(player);
                    if (role.UsesVents())
                    {
                        if (role == CustomRoles.Jester)
                            if (Options.JesterCanVent.GetBool())
                                //AssignDesyncRole(role, urself, sender, BaseRole: RoleTypes.Engineer);
                                AssignCustomRolesFromList(role, urself);
                            else
                                //AssignDesyncRole(role, urself, sender, BaseRole: RoleTypes.Crewmate);
                                AssignCustomRolesFromList(role, urself);
                        else
                            //AssignDesyncRole(role, urself, sender, BaseRole: RoleTypes.Engineer);
                            AssignCustomRolesFromList(role, urself);
                    }
                    else
                        //AssignDesyncRole(role, urself, sender, BaseRole: RoleTypes.Crewmate);
                        AssignCustomRolesFromList(role, urself);
                }

                /*AssignDesyncRole(CustomRoles.Arsonist, AllPlayers, sender, BaseRole: RoleTypes.Impostor);
                AssignDesyncRole(CustomRoles.Jackal, AllPlayers, sender, BaseRole: RoleTypes.Impostor);
                AssignDesyncRole(CustomRoles.Juggernaut, AllPlayers, sender, BaseRole: RoleTypes.Impostor);
                AssignDesyncRole(CustomRoles.PlagueBearer, AllPlayers, sender, BaseRole: RoleTypes.Impostor);
                AssignDesyncRole(CustomRoles.TheGlitch, AllPlayers, sender, BaseRole: RoleTypes.Shapeshifter);
                AssignDesyncRole(CustomRoles.Werewolf, AllPlayers, sender, BaseRole: RoleTypes.Impostor);
                AssignDesyncRole(CustomRoles.BloodKnight, AllPlayers, sender, BaseRole: RoleTypes.Impostor);

                AssignDesyncRole(CustomRoles.Amnesiac, AllPlayers, sender, BaseRole: RoleTypes.Impostor);*/

                //COVEN 
                AssignDesyncRole(CustomRoles.Coven, AllPlayers, sender, BaseRole: RoleTypes.Impostor);
                //AssignCovenRoles
                AssignDesyncRole(CustomRoles.CovenWitch, AllPlayers, sender, BaseRole: RoleTypes.Impostor);
                AssignDesyncRole(CustomRoles.HexMaster, AllPlayers, sender, BaseRole: RoleTypes.Impostor);
                AssignDesyncRole(CustomRoles.PotionMaster, AllPlayers, sender, BaseRole: RoleTypes.Impostor);
                AssignDesyncRole(CustomRoles.Poisoner, AllPlayers, sender, BaseRole: RoleTypes.Impostor);
                AssignDesyncRole(CustomRoles.Medusa, AllPlayers, sender, BaseRole: RoleTypes.Impostor);
                AssignDesyncRole(CustomRoles.Mimic, AllPlayers, sender, BaseRole: RoleTypes.Impostor);
                AssignDesyncRole(CustomRoles.Necromancer, AllPlayers, sender, BaseRole: RoleTypes.Impostor);
            }
            if (sender.CurrentState == CustomRpcSender.State.InRootMessage) sender.EndMessage();
            //以下、バニラ側の役職割り当てが入る
        }
        public static void Postfix()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            RpcSetRoleReplacer.Release(); //保存していたSetRoleRpcを一気に書く
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

            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                pc.Data.IsDead = false; //プレイヤーの死を解除する
                if (Main.AllPlayerCustomRoles.ContainsKey(pc.PlayerId)) continue; //既にカスタム役職が割り当てられていればスキップ
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
                    default:
                        Logger.SendInGame(string.Format(GetString("Error.InvalidRoleAssignment"), pc?.Data?.PlayerName));
                        break;
                }
            }

            if (Options.CurrentGameMode == CustomGameMode.HideAndSeek)
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
                SetupRoles();
                AssignCustomRolesFromList(CustomRoles.FireWorks, Shapeshifters, -1, true);
                AssignCustomRolesFromList(CustomRoles.Sniper, Shapeshifters, -1, true);
                AssignCustomRolesFromList(CustomRoles.Jester, Options.JesterCanVent.GetBool() ? Engineers : Crewmates);
                AssignCustomRolesFromList(CustomRoles.Madmate, Engineers);
                AssignCustomRolesFromList(CustomRoles.Bastion, Engineers);
                AssignCustomRolesFromList(CustomRoles.Bait, Crewmates);
                AssignCustomRolesFromList(CustomRoles.Demolitionist, Crewmates);
                AssignCustomRolesFromList(CustomRoles.Veteran, Engineers);
                AssignCustomRolesFromList(CustomRoles.Sleuth, Crewmates);
                AssignCustomRolesFromList(CustomRoles.Medium, Crewmates);
                AssignCustomRolesFromList(CustomRoles.Child, Crewmates);
                AssignCustomRolesFromList(CustomRoles.Bewilder, Crewmates);
                AssignCustomRolesFromList(CustomRoles.GuardianAngelTOU, Engineers);
                AssignCustomRolesFromList(CustomRoles.MadGuardian, Crewmates);
                AssignCustomRolesFromList(CustomRoles.MadSnitch, Options.MadSnitchCanVent.GetBool() ? Engineers : Crewmates);
                AssignCustomRolesFromList(CustomRoles.Mayor, Options.MayorHasPortableButton.GetBool() ? Engineers : Crewmates);
                AssignCustomRolesFromList(CustomRoles.Opportunist, Crewmates);
                AssignCustomRolesFromList(CustomRoles.Snitch, Crewmates);
                AssignCustomRolesFromList(CustomRoles.SabotageMaster, Crewmates);
                AssignCustomRolesFromList(CustomRoles.Hacker, Crewmates);
                AssignCustomRolesFromList(CustomRoles.Mafia, Impostors, -1, true);
                AssignCustomRolesFromList(CustomRoles.Terrorist, Engineers);
                AssignCustomRolesFromList(CustomRoles.Executioner, Crewmates);
                AssignCustomRolesFromList(CustomRoles.Vulture, Crewmates);
                AssignCustomRolesFromList(CustomRoles.Camouflager, Shapeshifters, -1, true);
                AssignCustomRolesFromList(CustomRoles.Ninja, Shapeshifters, -1, true);
                AssignCustomRolesFromList(CustomRoles.Vampire, Impostors, -1, true);
                AssignCustomRolesFromList(CustomRoles.BountyHunter, Shapeshifters, -1, true);
                AssignCustomRolesFromList(CustomRoles.Witch, Impostors, -1, true);
                AssignCustomRolesFromList(CustomRoles.Silencer, Impostors, -1, true);
                AssignCustomRolesFromList(CustomRoles.CorruptedSheriff, Impostors, -1, true);
                //AssignCustomRolesFromList(CustomRoles.ShapeMaster, Shapeshifters);
                AssignCustomRolesFromList(CustomRoles.Warlock, Shapeshifters, -1, true);
                AssignCustomRolesFromList(CustomRoles.SerialKiller, Shapeshifters, -1, true);
                AssignCustomRolesFromList(CustomRoles.Lighter, Crewmates);
                //AssignCustomRolesFromList(CustomRoles.Coven, Crewmates);
                AssignLoversRolesFromList();
                AssignCustomRolesFromList(CustomRoles.SpeedBooster, Crewmates);
                AssignCustomRolesFromList(CustomRoles.Trapper, Crewmates);
                AssignCustomRolesFromList(CustomRoles.Dictator, Crewmates);
                AssignCustomRolesFromList(CustomRoles.SchrodingerCat, Crewmates);
                if (Options.IsEvilWatcher) AssignCustomRolesFromList(CustomRoles.Watcher, Impostors, -1, true);
                else AssignCustomRolesFromList(CustomRoles.Watcher, Crewmates);
                if (Main.RealOptionsData.NumImpostors > 1)
                    AssignCustomRolesFromList(CustomRoles.Egoist, Shapeshifters, -1, true);
                AssignCustomRolesFromList(CustomRoles.Mare, Impostors, -1, true);
                AssignCustomRolesFromList(CustomRoles.Doctor, Scientists);
                AssignCustomRolesFromList(CustomRoles.Puppeteer, Impostors, -1, true);
                AssignCustomRolesFromList(CustomRoles.TimeThief, Impostors, -1, true);

                // Main.HasModifier.Clear();
                //  Main.modifiersList.Clear();
                // int NumOfModifers = 0;
                // FILLING THE LIST WITH USELESS ROLE //
                /*Main.modifiersList.Add(CustomRoles.NoSubRoleAssigned);
                Main.modifiersList.Add(CustomRoles.NoSubRoleAssigned);
                Main.modifiersList.Add(CustomRoles.NoSubRoleAssigned);
                Main.modifiersList.Add(CustomRoles.NoSubRoleAssigned);
                Main.modifiersList.Add(CustomRoles.NoSubRoleAssigned);*/
                /*   if (CustomRoles.Bait.IsEnable())
                   {
                       NumOfModifers++;
                       Main.modifiersList.Add(CustomRoles.Bait);
                   }
                   if (CustomRoles.Sleuth.IsEnable())
                   {
                       NumOfModifers++;
                       Main.modifiersList.Add(CustomRoles.Sleuth);
                   }
                   if (CustomRoles.Flash.IsEnable())
                   {
                       NumOfModifers++;
                       Main.modifiersList.Add(CustomRoles.Flash);
                   }
                   if (CustomRoles.TieBreaker.IsEnable())
                   {
                       NumOfModifers++;
                       Main.modifiersList.Add(CustomRoles.TieBreaker);
                   }
                   if (CustomRoles.Oblivious.IsEnable())
                   {
                       NumOfModifers++;
                       Main.modifiersList.Add(CustomRoles.Oblivious);
                   }
                   if (CustomRoles.Torch.IsEnable())
                   {
                       NumOfModifers++;
                       Main.modifiersList.Add(CustomRoles.Torch);
                   }
                   if (CustomRoles.Bewilder.IsEnable())
                   {
                       NumOfModifers++;
                       Main.modifiersList.Add(CustomRoles.Bewilder);
                   }

                   AssignModifiers(Main.modifiersList, NumOfModifers);*/

                //RPCによる同期
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.Is(CustomRoles.Watcher) && Options.IsEvilWatcher)
                        Main.AllPlayerCustomRoles[pc.PlayerId] = CustomRoles.EvilWatcher;
                    if (pc.Is(CustomRoles.Watcher) && !Options.IsEvilWatcher)
                        Main.AllPlayerCustomRoles[pc.PlayerId] = CustomRoles.NiceWatcher;
                    if (pc.Is(CustomRoles.PlagueBearer) && Options.InfectionSkip.GetBool())
                    {
                        Main.AllPlayerCustomRoles[pc.PlayerId] = CustomRoles.Pestilence;
                        Main.TeamPestiAlive = true;
                    }
                    if (pc.Is(CustomRoles.Vampire) && Options.VampireDitchesOn.GetBool() && !Main.VampireDitchesOn)
                    {
                        Main.AllPlayerCustomRoles[pc.PlayerId] = CustomRoles.Coven;
                        // so we dont have multiple poisoners
                        Main.VampireDitchesOn = true;
                    }
                }
                //if (CustomRoles.Veteran.IsEnable())
                //    Main.VetAlerts = Options.NumOfVets.GetInt();
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    //time for coven
                    if (CustomRolesHelper.GetRoleType(pc.GetCustomRole()) == RoleType.Coven)
                    {
                        //if they are coven.
                        //I KNOW THIS CODE IS TRASH. ILL FIX IT SOON, BUT NOT NOW
                        if (pc.Is(CustomRoles.Coven))
                        {
                            if (!Main.ChoseWitch)
                            {
                                Main.AllPlayerCustomRoles[pc.PlayerId] = CustomRoles.CovenWitch;
                                Main.ChoseWitch = true;
                            }
                            else if (!Main.HexMasterOn && Options.HexMasterOn.GetBool())
                            {
                                Main.AllPlayerCustomRoles[pc.PlayerId] = CustomRoles.HexMaster;
                                Main.HexMasterOn = true;
                            }
                            else if (!Main.PotionMasterOn && Options.PotionMasterOn.GetBool())
                            {
                                Main.AllPlayerCustomRoles[pc.PlayerId] = CustomRoles.PotionMaster;
                                Main.PotionMasterOn = true;
                            }
                            else if (!Main.MedusaOn && Options.MedusaOn.GetBool())
                            {
                                Main.AllPlayerCustomRoles[pc.PlayerId] = CustomRoles.Medusa;
                                Main.MedusaOn = true;
                            }
                            else if (!Main.MimicOn && !Main.NecromancerOn && Options.MimicOn.GetBool())
                            {
                                Main.AllPlayerCustomRoles[pc.PlayerId] = CustomRoles.Mimic;
                                Main.MimicOn = true;
                            }
                            else if (!Main.NecromancerOn && !Main.MimicOn && Options.NecromancerOn.GetBool())
                            {
                                Main.AllPlayerCustomRoles[pc.PlayerId] = CustomRoles.Necromancer;
                                Main.NecromancerOn = true;
                            }
                            else if (!Main.ConjurorOn && Options.ConjurorOn.GetBool())
                            {
                                Main.AllPlayerCustomRoles[pc.PlayerId] = CustomRoles.Conjuror;
                                Main.ConjurorOn = true;
                            }
                            else
                            {
                                //person is regular Coven.
                            }
                        }
                        else
                        {
                            //Poisoner
                        }
                    }
                }
                foreach (var pair in Main.AllPlayerCustomRoles)
                {
                    ExtendedPlayerControl.RpcSetCustomRole(pair.Key, pair.Value);
                }
                foreach (var pair in Main.AllPlayerCustomSubRoles)
                {
                    ExtendedPlayerControl.RpcSetCustomRole(pair.Key, pair.Value);
                }

                HudManager.Instance.SetHudActive(true);
                Main.KillOrSpell = new Dictionary<byte, bool>();
                if (Options.CovenMeetings.GetInt() == 0)
                    Main.HasNecronomicon = true;
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc.Data.Role.Role == RoleTypes.Shapeshifter) Main.CheckShapeshift.Add(pc.PlayerId, false);
                    switch (pc.GetCustomRole())
                    {
                        case CustomRoles.BountyHunter:
                            BountyHunter.Add(pc);
                            break;
                        case CustomRoles.SerialKiller:
                            SerialKiller.Add(pc.PlayerId);
                            break;
                        case CustomRoles.Witch:
                            Main.KillOrSpell.Add(pc.PlayerId, false);
                            break;
                        case CustomRoles.Warlock:
                            Main.CursedPlayers.Add(pc.PlayerId, null);
                            Main.isCurseAndKill.Add(pc.PlayerId, false);
                            break;
                        case CustomRoles.Child:
                            if (Options.ChildKnown.GetBool() == true)
                            {
                                //their name will have (C)
                                pc.name += Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Jackal), " (C)");
                            }
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

                        case CustomRoles.Arsonist:
                            foreach (var ar in PlayerControl.AllPlayerControls)
                                Main.isDoused.Add((pc.PlayerId, ar.PlayerId), false);
                            break;
                        case CustomRoles.PlagueBearer:
                            foreach (var ar in PlayerControl.AllPlayerControls)
                                Main.isInfected.Add((pc.PlayerId, ar.PlayerId), false);
                            break;
                        case CustomRoles.Executioner:
                            List<PlayerControl> targetList = new();
                            rand = new System.Random();
                            foreach (var target in PlayerControl.AllPlayerControls)
                            {
                                if (pc == target) continue;
                                else if (!Options.ExecutionerCanTargetImpostor.GetBool() && target.GetCustomRole().IsImpostor()) continue;

                                targetList.Add(target);
                            }
                            var Target = targetList[rand.Next(targetList.Count)];
                            Main.ExecutionerTarget.Add(pc.PlayerId, Target.PlayerId);
                            RPC.SendExecutionerTarget(pc.PlayerId, Target.PlayerId);
                            Logger.Info($"{pc.GetNameWithRole()}:{Target.GetNameWithRole()}", "Executioner");
                            break;
                        case CustomRoles.GuardianAngelTOU:
                            List<PlayerControl> protectList = new();
                            rand = new System.Random();
                            foreach (var target in PlayerControl.AllPlayerControls)
                            {
                                if (pc == target) continue;

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

                        case CustomRoles.Sheriff:
                            Sheriff.Add(pc.PlayerId);
                            break;
                        case CustomRoles.Mayor:
                            Main.MayorUsedButtonCount[pc.PlayerId] = 0;
                            break;
                        case CustomRoles.Hacker:
                            Main.HackerFixedSaboCount[pc.PlayerId] = 0;
                            break;
                        case CustomRoles.SabotageMaster:
                            SabotageMaster.Add(pc.PlayerId);
                            break;
                        case CustomRoles.HexMaster:
                            foreach (var ar in PlayerControl.AllPlayerControls)
                            {
                                if (!ar.GetCustomRole().IsCoven())
                                    Main.isHexed.Add((pc.PlayerId, ar.PlayerId), false);
                            }
                            break;
                        case CustomRoles.Investigator:
                            Investigator.Add(pc.PlayerId);
                            foreach (var ar in PlayerControl.AllPlayerControls)
                            {
                                Investigator.hasSeered.Add(ar.PlayerId, false);
                            }
                            break;
                    }
                    pc.ResetKillCooldown();
                }

                //役職の人数を戻す
                RoleOptionsData roleOpt = PlayerControl.GameOptions.RoleOptions;
                int ScientistNum = roleOpt.GetNumPerGame(RoleTypes.Scientist);
                ScientistNum -= CustomRoles.Doctor.GetCount();
                roleOpt.SetRoleRate(RoleTypes.Scientist, ScientistNum, roleOpt.GetChancePerGame(RoleTypes.Scientist));

                int EngineerNum = roleOpt.GetNumPerGame(RoleTypes.Engineer);

                EngineerNum -= CustomRoles.Madmate.GetCount() + CustomRoles.Terrorist.GetCount();

                if (Options.MayorHasPortableButton.GetBool())
                    EngineerNum -= CustomRoles.Mayor.GetCount();

                if (Options.MadSnitchCanVent.GetBool())
                    EngineerNum -= CustomRoles.MadSnitch.GetCount();

                // if (CustomRoles.GuardianAngelTOU.IsEnable())
                //      EngineerNum -= CustomRoles.GuardianAngelTOU.GetCount();

                roleOpt.SetRoleRate(RoleTypes.Engineer, EngineerNum, roleOpt.GetChancePerGame(RoleTypes.Engineer));

                int ShapeshifterNum = roleOpt.GetNumPerGame(RoleTypes.Shapeshifter);
                ShapeshifterNum -= CustomRoles.SerialKiller.GetCount() + CustomRoles.BountyHunter.GetCount() + CustomRoles.TheGlitch.GetCount() + CustomRoles.Warlock.GetCount()/* + CustomRoles.ShapeMaster.GetCount()*/ + CustomRoles.FireWorks.GetCount() + CustomRoles.Sniper.GetCount() + CustomRoles.Camouflager.GetCount() + CustomRoles.Ninja.GetCount();
                if (Main.RealOptionsData.NumImpostors > 1)
                    ShapeshifterNum -= CustomRoles.Egoist.GetCount();
                //if (CustomRoles.TheGlitch.IsEnable())
                //    ShapeshifterNum -= CustomRoles.TheGlitch.GetCount();
                roleOpt.SetRoleRate(RoleTypes.Shapeshifter, ShapeshifterNum, roleOpt.GetChancePerGame(RoleTypes.Shapeshifter));
            }

            // ResetCamが必要なプレイヤーのリストにクラス化が済んでいない役職のプレイヤーを追加
            Main.ResetCamPlayerList.AddRange(PlayerControl.AllPlayerControls.ToArray().Where(p => p.GetCustomRole() is CustomRoles.Arsonist or CustomRoles.Jackal or CustomRoles.PlagueBearer or CustomRoles.Pestilence or CustomRoles.Werewolf or CustomRoles.TheGlitch).Select(p => p.PlayerId));
            Utils.CountAliveImpostors();
            Utils.CustomSyncAllSettings();
            SetColorPatch.IsAntiGlitchDisabled = false;
        }
        private static void AssignDesyncRole(CustomRoles role, List<PlayerControl> AllPlayers, CustomRpcSender sender, RoleTypes BaseRole, RoleTypes hostBaseRole = RoleTypes.Crewmate)
        {
            if (!role.IsEnable()) return;

            for (var i = 0; i < role.GetCount(); i++)
            {
                if (AllPlayers.Count <= 0) break;
                var rand = new System.Random();
                var player = AllPlayers[rand.Next(0, AllPlayers.Count)];
                AllPlayers.Remove(player);
                Main.AllPlayerCustomRoles[player.PlayerId] = role;
                //ここからDesyncが始まる
                if (player.PlayerId != 0)
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
                player.Data.IsDead = true;
            }
        }
        private static List<PlayerControl> AssignCustomRolesFromList(CustomRoles role, List<PlayerControl> players, int RawCount = -1, bool isImpRole = false)
        {
            if (players == null || players.Count <= 0) return null;
            var rand = new System.Random();
            var rando = new System.Random();
            var count = Math.Clamp(RawCount, 0, players.Count);
            if (RawCount == -1) count = Math.Clamp(role.GetCount(), 0, players.Count);
            if (count <= 0) return null;
            List<PlayerControl> AssignedPlayers = new();
            SetColorPatch.IsAntiGlitchDisabled = true;
            for (var i = 0; i < count; i++)
            {
                switch (isImpRole)
                {
                    case false:
                        if (!Main.chosenRoles.Contains(role)) break;
                        var player = players[rand.Next(0, players.Count)];
                        var r = Main.chosenRoles[rando.Next(0, Main.chosenRoles.Count)];
                        AssignedPlayers.Add(player);
                        players.Remove(player);
                        Main.chosenRoles.Remove(r);
                        Main.AllPlayerCustomRoles[player.PlayerId] = r;
                        Logger.Info("役職設定:" + player?.Data?.PlayerName + " = " + r.ToString(), "AssignRoles");

                        if (Options.CurrentGameMode == CustomGameMode.HideAndSeek)
                        {
                            if (player.Is(CustomRoles.HASTroll))
                                player.RpcSetColor(2);
                            else if (player.Is(CustomRoles.HASFox))
                                player.RpcSetColor(3);
                        }
                        break;
                    case true:
                        if (!Main.chosenImpRoles.Contains(role)) break;
                        var p = players[rand.Next(0, players.Count)];
                        var ro = Main.chosenImpRoles[rando.Next(0, Main.chosenImpRoles.Count)];
                        AssignedPlayers.Add(p);
                        players.Remove(p);
                        Main.chosenImpRoles.Remove(ro);
                        Main.AllPlayerCustomRoles[p.PlayerId] = ro;
                        Logger.Info("役職設定:" + p?.Data?.PlayerName + " = " + ro.ToString(), "AssignRoles");

                        if (Options.CurrentGameMode == CustomGameMode.HideAndSeek)
                        {
                            if (p.Is(CustomRoles.HASTroll))
                                p.RpcSetColor(2);
                            else if (p.Is(CustomRoles.HASFox))
                                pS.RpcSetColor(3);
                        }
                        break;
                }
            }
            SetColorPatch.IsAntiGlitchDisabled = false;
            return AssignedPlayers;
        }

        private static bool RoleGoingInList(CustomRoles role)
        {
            if (!role.IsEnable()) return false;
            bool isRole = UnityEngine.Random.Range(1, 100) <= Options.CustomRoleSpawnChances[role].GetFloat();
            return isRole;
        }

        private static void RoleAddedToList(CustomRoles role, bool IsImpostor = false)
        {
            switch (IsImpostor)
            {
                case false:
                    if (RoleGoingInList(role))
                        Main.chosenRoles.Add(role);
                    break;
                case true:
                    if (RoleGoingInList(role))
                        Main.chosenImpRoles.Add(role);
                    break;
            }
        }
        private static void SetupRoles()
        {
            RoleAddedToList(CustomRoles.FireWorks, true);
            RoleAddedToList(CustomRoles.Sniper, true);
            RoleAddedToList(CustomRoles.Madmate);
            RoleAddedToList(CustomRoles.Bastion);
            RoleAddedToList(CustomRoles.Bait);
            RoleAddedToList(CustomRoles.Demolitionist);
            RoleAddedToList(CustomRoles.Veteran);
            RoleAddedToList(CustomRoles.Sleuth);
            RoleAddedToList(CustomRoles.Medium);
            RoleAddedToList(CustomRoles.Child);
            RoleAddedToList(CustomRoles.Bewilder);
            RoleAddedToList(CustomRoles.GuardianAngelTOU);
            RoleAddedToList(CustomRoles.MadGuardian);
            RoleAddedToList(CustomRoles.MadSnitch);
            RoleAddedToList(CustomRoles.Mayor);
            RoleAddedToList(CustomRoles.Opportunist);
            RoleAddedToList(CustomRoles.Snitch);
            RoleAddedToList(CustomRoles.SabotageMaster);
            RoleAddedToList(CustomRoles.Hacker);
            RoleAddedToList(CustomRoles.Mafia, true);
            RoleAddedToList(CustomRoles.Terrorist);
            RoleAddedToList(CustomRoles.Executioner);
            RoleAddedToList(CustomRoles.Vulture);
            RoleAddedToList(CustomRoles.Camouflager, true);
            RoleAddedToList(CustomRoles.Ninja, true);
            RoleAddedToList(CustomRoles.Vampire, true);
            RoleAddedToList(CustomRoles.BountyHunter, true);
            RoleAddedToList(CustomRoles.Witch, true);
            RoleAddedToList(CustomRoles.Silencer, true);
            RoleAddedToList(CustomRoles.CorruptedSheriff, true);
            //RoleAddedToList(CustomRoles.ShapeMaster, true);
            RoleAddedToList(CustomRoles.Warlock, true);
            RoleAddedToList(CustomRoles.SerialKiller, true);
            RoleAddedToList(CustomRoles.Lighter);
            //RoleAddedToList(CustomRoles.Coven);
            RoleAddedToList(CustomRoles.SpeedBooster);
            RoleAddedToList(CustomRoles.Trapper);
            RoleAddedToList(CustomRoles.Dictator);
            RoleAddedToList(CustomRoles.SchrodingerCat);
            if (Options.IsEvilWatcher) RoleAddedToList(CustomRoles.Watcher, true);
            else RoleAddedToList(CustomRoles.Watcher);
            if (Main.RealOptionsData.NumImpostors > 1)
                RoleAddedToList(CustomRoles.Egoist, true);
            RoleAddedToList(CustomRoles.Mare, true);
            RoleAddedToList(CustomRoles.Doctor);
            RoleAddedToList(CustomRoles.Puppeteer, true);
            RoleAddedToList(CustomRoles.TimeThief, true);
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

                if (Options.CurrentGameMode == CustomGameMode.HideAndSeek)
                {
                    if (player.Is(CustomRoles.HASTroll))
                        player.RpcSetColor(2);
                    else if (player.Is(CustomRoles.HASFox))
                        player.RpcSetColor(3);
                }
            }
            SetColorPatch.IsAntiGlitchDisabled = false;
            return AssignedPlayers;
        }

        private static void AssignLoversRolesFromList()
        {
            if (CustomRoles.Lovers.IsEnable())
            {
                //Loversを初期化
                Main.LoversPlayers.Clear();
                Main.isLoversDead = false;
                //ランダムに2人選出
                AssignLoversRoles(2);
            }
        }
        private static void AssignLoversRoles(int RawCount = -1)
        {
            var allPlayers = new List<PlayerControl>();
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.Is(CustomRoles.GM)) continue;
                allPlayers.Add(player);
            }
            var loversRole = CustomRoles.Lovers;
            var rand = new System.Random();
            var count = Math.Clamp(RawCount, 0, allPlayers.Count);
            if (RawCount == -1) count = Math.Clamp(loversRole.GetCount(), 0, allPlayers.Count);
            if (count <= 0) return;

            for (var i = 0; i < count; i++)
            {
                var player = allPlayers[rand.Next(0, allPlayers.Count)];
                while (!player.Is(CustomRoles.Child))
                {
                    Main.LoversPlayers.Add(player);
                    allPlayers.Remove(player);
                    Main.AllPlayerCustomSubRoles[player.PlayerId] = loversRole;
                    Logger.Info("役職設定:" + player?.Data?.PlayerName + " = " + player.GetCustomRole().ToString() + " + " + loversRole.ToString(), "AssignLovers");
                }
            }
            RPC.SyncLoversPlayers();
        }

        private static void AssignModifiers(List<CustomRoles> PossibleModifers, int RawCount = -1)
        {
            var allPlayers = new List<PlayerControl>();
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.Is(CustomRoles.GM)) continue;
                allPlayers.Add(player);
            }
            var roles = PossibleModifers;
            var rand = new System.Random();
            var randMod = new System.Random();
            var count = Math.Clamp(RawCount, 0, allPlayers.Count);
            if (RawCount == -1) count = Math.Clamp(roles.Count, 0, allPlayers.Count);
            if (count <= 0) return;

            for (var i = 0; i < count; i++)
            {
                var player = allPlayers[rand.Next(0, allPlayers.Count)];
                var role = roles[rand.Next(0, PossibleModifers.Count)];
                bool checkModifier = true;
                if (role == CustomRoles.NoSubRoleAssigned)
                    checkModifier = false;
                while (Main.AllPlayerCustomSubRoles[player.PlayerId] != CustomRoles.Lovers)
                {
                    if (checkModifier)
                    {
                        if (Main.HasModifier.ContainsKey(role))
                        {
                            Main.HasModifier.Add(role, player);
                            Main.modifiersList.Remove(role); // NO 2 PEOPLE CAN HAVE THE SAME MODIFIER //
                            allPlayers.Remove(player);
                            Main.AllPlayerCustomSubRoles[player.PlayerId] = role;
                            Logger.Info("役職設定:" + player?.Data?.PlayerName + " = " + player.GetCustomRole().ToString() + " + " + role.ToString(), "AssignModifiers");
                        }
                    }
                }
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
