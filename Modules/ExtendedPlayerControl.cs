using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using InnerNet;
using UnityEngine;
using static TownOfHost.Translator;
using AmongUs.GameOptions;
using System.Reflection;
using AmongUs.GameOptions;
using PowerTools;
using TownOfHost.PrivateExtensions;

namespace TownOfHost
{
    static class ExtendedPlayerControl
    {
        public static void RpcSetCustomRole(this PlayerControl player, CustomRoles role)
        {
            if (role < CustomRoles.NoSubRoleAssigned)
            {
                Main.AllPlayerCustomRoles[player.PlayerId] = role;
            }
            else if (role >= CustomRoles.NoSubRoleAssigned)   //500:NoSubRole 501~:SubRole
            {
                Main.AllPlayerCustomSubRoles[player.PlayerId] = role;
            }
            if (AmongUsClient.Instance.AmHost)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetCustomRole, Hazel.SendOption.Reliable, -1);
                writer.Write(player.PlayerId);
                writer.WritePacked((int)role);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
        }
        public static void SetDefaultRole(this PlayerControl player)
        {
            switch (player.GetRoleType())
            {
                case RoleType.Crewmate:
                    player.RpcSetCustomRole(CustomRoles.Crewmate);
                    RoleManager.Instance.SetRole(player, RoleTypes.Crewmate);
                    break;
                case RoleType.Impostor:
                    player.RpcSetCustomRole(CustomRoles.Impostor);
                    RoleManager.Instance.SetRole(player, RoleTypes.Impostor);
                    break;
                case RoleType.Neutral:
                    if (!player.GetCustomRole().IsNeutralKilling())
                    {
                        player.RpcSetCustomRole(CustomRoles.Opportunist);
                        RoleManager.Instance.SetRole(player, RoleTypes.Crewmate);
                    }
                    break;
                case RoleType.Coven:
                    player.RpcSetCustomRole(CustomRoles.Coven);
                    // RoleManager.Instance.SetRole(player, RoleTypes.Impostor);
                    break;
                case RoleType.Madmate:
                    player.RpcSetCustomRole(CustomRoles.Madmate);
                    RoleManager.Instance.SetRole(player, RoleTypes.Engineer);
                    break;
            }
        }
        public static void RpcSetCustomRole(byte PlayerId, CustomRoles role)
        {
            if (AmongUsClient.Instance.AmHost)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetCustomRole, Hazel.SendOption.Reliable, -1);
                writer.Write(PlayerId);
                writer.WritePacked((int)role);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
        }
        public static void SetCustomRole(this PlayerControl player, CustomRoles role)
        {
            Main.AllPlayerCustomRoles[player.PlayerId] = role;
        }

        public static void RpcExile(this PlayerControl player)
        {
            RPC.ExileAsync(player);
        }
        public static ClientData GetClient(this PlayerControl player)
        {
            Il2CppSystem.Collections.Generic.List<ClientData> clients = new();
            AmongUsClient.Instance.GetAllClients(clients);

            List<ClientData> clientDatas = new();
            foreach (ClientData clientData in clients)
            {
                clientDatas.Add(clientData);
            }

            var client = clientDatas.Find(cd => cd.Character.PlayerId == player.PlayerId);
            return client;
        }
        public static int GetClientId(this PlayerControl player)
        {
            var client = player.GetClient();
            return client == null ? -1 : client.Id;
        }
        public static CustomRoles GetCustomRole(this GameData.PlayerInfo player)
        {
            return player == null || player.Object == null ? CustomRoles.Crewmate : player.Object.GetCustomRole();
        }

        public static CustomRoles GetCustomRole(this PlayerControl player)
        {
            var cRole = CustomRoles.Crewmate;
            if (player == null)
            {
                var caller = new System.Diagnostics.StackFrame(1, false);
                var callerMethod = caller.GetMethod();
                string callerMethodName = callerMethod?.Name;
                string callerClassName = callerMethod?.DeclaringType?.FullName;
                Logger.Warn(callerClassName + "." + callerMethodName + "がCustomRoleを取得しようとしましたが、対象がnullでした。", "GetCustomRole");
                return cRole;
            }
            var cRoleFound = Main.AllPlayerCustomRoles.TryGetValue(player.PlayerId, out cRole);
            return cRoleFound || player.Data.Role == null
                ? cRole
                : player.Data.Role.Role switch
                {
                    RoleTypes.Crewmate => CustomRoles.Crewmate,
                    RoleTypes.Engineer => CustomRoles.Engineer,
                    RoleTypes.Scientist => CustomRoles.Scientist,
                    RoleTypes.GuardianAngel => CustomRoles.GuardianAngel,
                    RoleTypes.Impostor => CustomRoles.Impostor,
                    RoleTypes.Shapeshifter => CustomRoles.Shapeshifter,
                    _ => CustomRoles.Crewmate,
                };
        }

        public static RoleType GetRoleType(this PlayerControl player)
        {
            var cRole = RoleType.Crewmate;
            if (player == null)
            {
                var caller = new System.Diagnostics.StackFrame(1, false);
                var callerMethod = caller.GetMethod();
                string callerMethodName = callerMethod.Name;
                string callerClassName = callerMethod.DeclaringType.FullName;
                Logger.Warn(callerClassName + "." + callerMethodName + "がCustomRoleを取得しようとしましたが、対象がnullでした。", "GetRoleTeam");
                return cRole;
            }
            else if (player.Data.Role != null)
            {
                return CustomRolesHelper.GetRoleType(player.GetCustomRole());
            }
            return cRole;
        }

        public static CustomRoles GetCustomSubRole(this GameData.PlayerInfo player)
        {
            return player == null || player.Object == null ? CustomRoles.Crewmate : player.Object.GetCustomSubRole();
        }

        public static CustomRoles GetCustomSubRole(this PlayerControl player)
        {
            if (player == null)
            {
                Logger.Warn("CustomSubRoleを取得しようとしましたが、対象がnullでした。", "getCustomSubRole");
                return CustomRoles.NoSubRoleAssigned;
            }
            var cRoleFound = Main.AllPlayerCustomSubRoles.TryGetValue(player.PlayerId, out var cRole);
            return cRoleFound ? cRole : CustomRoles.NoSubRoleAssigned;
        }
        public static void RpcSetNameEx(this PlayerControl player, string name)
        {
            foreach (var seer in PlayerControl.AllPlayerControls)
            {
                Main.LastNotifyNames[(player.PlayerId, seer.PlayerId)] = name;
            }
            HudManagerPatch.LastSetNameDesyncCount++;

            Logger.Info($"Set:{player?.Data?.PlayerName}:{name} for All", "RpcSetNameEx");
            player.RpcSetName(name);
        }

        public static void RpcSetNamePrivate(this PlayerControl player, string name, bool DontShowOnModdedClient = false, PlayerControl seer = null, bool force = false)
        {
            //player: 名前の変更対象
            //seer: 上の変更を確認することができるプレイヤー
            if (player == null || name == null || !AmongUsClient.Instance.AmHost) return;
            if (seer == null) seer = player;
            if (!force && Main.LastNotifyNames[(player.PlayerId, seer.PlayerId)] == name)
            {
                //Logger.info($"Cancel:{player.name}:{name} for {seer.name}", "RpcSetNamePrivate");
                return;
            }
            Main.LastNotifyNames[(player.PlayerId, seer.PlayerId)] = name;
            HudManagerPatch.LastSetNameDesyncCount++;
            Logger.Info($"Set:{player?.Data?.PlayerName}:{name} for {seer.GetNameWithRole()}", "RpcSetNamePrivate");

            var clientId = seer.GetClientId();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetName, Hazel.SendOption.Reliable, clientId);
            writer.Write(name);
            writer.Write(DontShowOnModdedClient);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void RpcSetRoleDesync(this PlayerControl player, RoleTypes role, PlayerControl seer = null)
        {
            //player: 名前の変更対象
            //seer: 上の変更を確認することができるプレイヤー

            /*if (player == null) return;
            if (seer == null) seer = player;
            var clientId = seer.GetClientId();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetRole, Hazel.SendOption.Reliable, clientId);
            writer.Write((ushort)role);
            AmongUsClient.Instance.FinishRpcImmediately(writer);*/

            if (player == null) return;
            if (seer == null) seer = player;
            var clientId = seer.GetClientId();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetRole, Hazel.SendOption.Reliable, clientId);
            writer.Write((ushort)role);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcGuardAndKill(this PlayerControl killer, PlayerControl target = null, int colorId = 0)
        {
            if (target == null) target = killer;
            // Host
            killer.ProtectPlayer(target, colorId);
            killer.MurderPlayer(target);
            // Other Clients
            if (killer.PlayerId != 0)
            {
                var sender = CustomRpcSender.Create("GuardAndKill Sender", SendOption.Reliable);
                sender.StartMessage(killer.GetClientId());
                sender.StartRpc(killer.NetId, (byte)RpcCalls.ProtectPlayer)
                    .WriteNetObject((InnerNetObject)target)
                    .Write(colorId)
                    .EndRpc();
                sender.StartRpc(killer.NetId, (byte)RpcCalls.MurderPlayer)
                    .WriteNetObject((InnerNetObject)target)
                    .EndRpc();
                sender.EndMessage();
                sender.SendMessage();
            }
        }
        public static void RpcSpecificMurderPlayer(this PlayerControl killer, PlayerControl target = null)
        {
            if (target == null) target = killer;
            if (AmongUsClient.Instance.AmClient)
            {
                killer.MurderPlayer(target);
            }
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)RpcCalls.MurderPlayer, SendOption.Reliable, killer.GetClientId());
            messageWriter.WriteNetObject(target);
            AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
        }
        public static void RpcSpecificProtectPlayer(this PlayerControl killer, PlayerControl target = null, int colorId = 0)
        {
            if (AmongUsClient.Instance.AmClient)
            {
                killer.ProtectPlayer(target, colorId);
            }
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)RpcCalls.ProtectPlayer, SendOption.Reliable, killer.GetClientId());
            messageWriter.WriteNetObject(target);
            messageWriter.Write(colorId);
            AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
        }

        public static bool AttackIsStronger(this PlayerControl attacker, PlayerControl target)
        {
            var flag = false;
            var attack = Utils.GetAttackEnum(attacker.GetCustomRole());
            var defense = Utils.GetDefenseEnum(target.GetCustomRole());
            if ((byte)defense < (byte)attack)
            {
                flag = true;
            }
            if ((byte)defense == (byte)attack && defense == 0)
                flag = false;
            return flag;
        }
        public static void RpcResetAbilityCooldown(this PlayerControl target)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            Logger.Info($"アビリティクールダウンのリセット:{target.name}({target.PlayerId})", "RpcResetAbilityCooldown");
            if (PlayerControl.LocalPlayer == target)
            {
                //targetがホストだった場合
                PlayerControl.LocalPlayer.Data.Role.SetCooldown();
            }
            else
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(target.NetId, (byte)RpcCalls.ProtectPlayer, SendOption.None, target.GetClientId());
                writer.Write(0); //writer.WriteNetObject(null); と同じ
                writer.Write(0);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
        }
        public static void SetPetLocally(this PlayerControl pc)
        {
            //if (pc.GetPet().enabled) return;
            if (pc.CurrentOutfit.PetId != "") return;
            if (Options.GarunteePet.GetBool())
            {
                pc.RpcSetPet("pet_clank");
            }
            else
            {
                int clientId = pc.GetClientId();
                MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(pc.NetId, (byte)RpcCalls.SetPetStr, SendOption.Reliable, clientId);
                writer2.Write("pet_clank");
                AmongUsClient.Instance.FinishRpcImmediately(writer2);
            }
        }
        public static byte GetRoleCount(this Dictionary<CustomRoles, byte> dic, CustomRoles role)
        {
            if (!dic.ContainsKey(role))
            {
                dic[role] = 0;
            }

            return dic[role];
        }

        public static void SendDM(this PlayerControl target, string text)
        {
            Utils.SendMessage(text, target.PlayerId);
        }

        public static bool hidePlayerName(PlayerControl source, PlayerControl target)
        {
            if (Camouflager.DidCamo) return true; // No names are visible
            if (target.Is(CustomRoles.Swooper) && Main.IsInvis) return true;
            if (source == null || target == null) return true;
            return source != target; // Player sees his own name
        }

        public static void SetRolePublic(this PlayerControl target, RoleTypes roleType)
        {
            target.SetRole(roleType);
        }

        public static string GetDeathReason(this PlayerControl player) =>
            PlayerState.isDead[player.PlayerId] | player.Data.IsDead ? GetString("DeathReason." + PlayerState.GetDeathReason(player.PlayerId)) : GetString("Alive");

        public static void setDefaultLook(this PlayerControl target)
        {
            target.setLook(target.Data.PlayerName, target.Data.DefaultOutfit.ColorId, target.Data.DefaultOutfit.HatId, target.Data.DefaultOutfit.VisorId, target.Data.DefaultOutfit.SkinId, target.Data.DefaultOutfit.PetId);
        }

        public static void setLook(this PlayerControl target, String playerName, int colorId, string hatId, string visorId, string skinId, string petId)
        {
            target.SetColor(colorId);
            target.SetVisor(visorId, colorId);
            target.SetHat(hatId, colorId);
            target.SetName(hidePlayerName(PlayerControl.LocalPlayer, target) ? "" : playerName);

            SkinData skinData = DestroyableSingleton<HatManager>.Instance.GetSkinById(skinId);
            SkinViewData nextSkin = skinData.viewData.viewData;
            SkinLayer skinLayer = target.cosmetics.skin;
            if (skinLayer == null) return;

            AnimationClip clip;
            PlayerPhysics playerPhysics = target.MyPhysics;
            PlayerAnimationGroup pAnimationGroup = playerPhysics.Animations.animationGroups[0];
            SpriteAnim spriteAnimator = skinLayer.animator;

            AnimationClip currentAnimationClip = playerPhysics.Animations.Animator.GetCurrentAnimation();

            if (currentAnimationClip == pAnimationGroup.RunAnim) clip = nextSkin.RunAnim;
            else if (currentAnimationClip == pAnimationGroup.SpawnAnim) clip = nextSkin.SpawnAnim;
            else if (currentAnimationClip == pAnimationGroup.EnterVentAnim) clip = nextSkin.EnterVentAnim;
            else if (currentAnimationClip == pAnimationGroup.ExitVentAnim) clip = nextSkin.ExitVentAnim;
            else if (currentAnimationClip == pAnimationGroup.IdleAnim) clip = nextSkin.IdleAnim;
            else clip = nextSkin.IdleAnim;
            float progress = playerPhysics.Animations.Animator.m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

            target.cosmetics.skin.skin = nextSkin;
            skinLayer.UpdateMaterial();

            spriteAnimator.Play(clip, 1f);
            spriteAnimator.m_animator.Play("a", 0, progress % 1);
            spriteAnimator.m_animator.Update(0f);

            if (target.cosmetics.CurrentPet)
                UnityEngine.Object.Destroy(target.cosmetics.CurrentPet.gameObject);

            PetBehaviour pet = UnityEngine.Object.Instantiate(DestroyableSingleton<HatManager>.Instance.GetPetById(petId).viewData.viewData);
            pet.transform.position = target.transform.position;
            pet.Source = target;
            pet.Visible = target.Visible;
            target.SetPlayerMaterialColors(pet.rend);
        }
        public static void CustomSyncSettings(this PlayerControl player)
        {
            if (player == null || !AmongUsClient.Instance.AmHost) return;
            if (Main.RealOptionsData == null)
                Main.RealOptionsData ??= GameOptionsManager.Instance.CurrentGameOptions;

            int clientId = player.GetClientId();
            IGameOptions anyOptions = Main.RealOptionsData.DeepCopy();

            /*if (anyOptions.GetType() != typeof(NormalGameOptionsV07))
                throw new NotImplementedException("Unable to sync settings for non-standard game");*/

            NormalGameOptionsV07 options = anyOptions.AsNormalOptions()!;
            options.BlackOut(PlayerState.IsBlackOut[player.PlayerId]);
            RoleOptionsCollectionV07 roleCollection = (RoleOptionsCollectionV07)options.RoleOptions.Cast<RoleOptionsCollectionV07>();
            ShapeshifterRoleOptionsV07 shapeshifterOptions = options.GetShapeshifterOptions();
            EngineerRoleOptionsV07 engineerOptions = options.GetEngineerOptions();
            ScientistRoleOptionsV07 scientistOptions = options.GetScientistOptions();

            CustomRoles role = player.GetCustomRole();
            RoleType roleType = role.GetRoleType();

            switch (roleType)
            {
                case RoleType.Impostor:
                    shapeshifterOptions.ShapeshifterCooldown = Options.DefaultShapeshiftCooldown.GetFloat();
                    //opt.SetVision(player, true);
                    break;
                case RoleType.Madmate:
                    engineerOptions.EngineerCooldown = Options.MadmateVentCooldown.GetFloat();
                    engineerOptions.EngineerInVentMaxTime = Options.MadmateVentMaxTime.GetFloat();
                    if (Options.MadmateHasImpostorVision.GetBool())
                        options.SetVision(player, true);
                    break;
            }
            switch (player.GetCustomRole())
            {
                case CustomRoles.Painter:
                    options.SetVision(player, Options.PaintersHaveImpVision.GetBool());
                    break;
                case CustomRoles.Marksman:
                    options.KillDistance = Main.MarksmanKills;
                    options.SetVision(player, true);
                    break;
                case CustomRoles.Terrorist:
                    goto InfinityVent;
                // case CustomRoles.ShapeMaster:
                //     opt.RoleOptions.ShapeshifterCooldown = 0.1f;
                //     opt.RoleOptions.ShapeshifterLeaveSkin = false;
                //     opt.RoleOptions.ShapeshifterDuration = Options.ShapeMasterShapeshiftDuration.GetFloat();
                //     break;
                case CustomRoles.Bastion:
                    engineerOptions.EngineerCooldown = 25;
                    engineerOptions.EngineerInVentMaxTime = 0.1f;
                    break;
                case CustomRoles.Transporter:
                    engineerOptions.EngineerInVentMaxTime = 0.5f;
                    engineerOptions.EngineerCooldown = Options.TransportCooldown.GetFloat();
                    break;
                case CustomRoles.Warlock:
                    shapeshifterOptions.ShapeshifterCooldown = Main.isCursed ? 1f : Options.DefaultKillCooldown;
                    break;
                case CustomRoles.SerialKiller:
                    SerialKiller.ApplyGameOptions(options);
                    break;
                case CustomRoles.BountyHunter:
                    BountyHunter.ApplyGameOptions(options);
                    break;
                case CustomRoles.Escapist:
                    Escapist.ApplyGameOptions(player, options);
                    break;
                case CustomRoles.Bomber:
                    Bomber.ApplyGameOptions(player, options);
                    break;
                case CustomRoles.Sheriff:
                case CustomRoles.Investigator:
                case CustomRoles.Janitor:
                case CustomRoles.Arsonist:
                case CustomRoles.Amnesiac:
                case CustomRoles.Crusader:
                case CustomRoles.Escort:
                    options.SetVision(player, false);
                    break;
                case CustomRoles.PlagueBearer:
                    options.SetVision(player, false);
                    break;
                case CustomRoles.CorruptedSheriff:
                case CustomRoles.Pestilence:
                    options.SetVision(player, true);
                    break;
                case CustomRoles.Lighter:
                    if (player.GetPlayerTaskState().IsTaskFinished)
                    {
                        options.CrewLightMod = Options.LighterTaskCompletedVision.GetFloat();
                        if (Utils.IsActive(SystemTypes.Electrical) && Options.LighterTaskCompletedDisableLightOut.GetBool())
                            options.CrewLightMod *= 5;
                    }
                    break;
                case CustomRoles.Medium:
                    engineerOptions.EngineerCooldown = Options.MediumCooldown.GetFloat();
                    engineerOptions.EngineerInVentMaxTime = 0.5f;
                    break;
                case CustomRoles.BloodKnight:
                case CustomRoles.EgoSchrodingerCat:
                    options.SetVision(player, true);
                    break;
                case CustomRoles.Doctor:
                    scientistOptions.ScientistCooldown = 0f;
                    scientistOptions.ScientistBatteryCharge = Options.DoctorTaskCompletedBatteryCharge.GetFloat();
                    break;
                case CustomRoles.Camouflager:
                    shapeshifterOptions.ShapeshifterCooldown = Camouflager.CamouflagerCamouflageCoolDown.GetFloat();
                    shapeshifterOptions.ShapeshifterDuration = Camouflager.CamouflagerCamouflageDuration.GetFloat();
                    break;
                case CustomRoles.Juggernaut:
                    options.SetVision(player, true);
                    if (Options.JuggerCanVent.GetBool())
                        goto InfinityVent;
                    break;
                case CustomRoles.Freezer:
                    shapeshifterOptions.ShapeshifterCooldown = Options.FreezerCooldown.GetFloat();
                    shapeshifterOptions.ShapeshifterDuration = Options.FreezerDuration.GetFloat();
                    break;
                case CustomRoles.Disperser:
                    shapeshifterOptions.ShapeshifterCooldown = Options.DisperseCooldown.GetFloat();
                    shapeshifterOptions.ShapeshifterDuration = 1;
                    break;
                case CustomRoles.Vulture:
                    options.SetVision(player, Options.VultureHasImpostorVision.GetBool());
                    if (Options.VultureCanVent.GetBool())
                        goto InfinityVent;
                    break;
                case CustomRoles.Mayor:
                    engineerOptions.EngineerCooldown =
                        Main.MayorUsedButtonCount.TryGetValue(player.PlayerId, out var count) && count < Options.MayorNumOfUseButton.GetInt()
                        ? options.EmergencyCooldown
                        : 300f;
                    engineerOptions.EngineerInVentMaxTime = 1;
                    break;
                case CustomRoles.Veteran:
                    //5 lines of code calculating the next Vet CD.
                    /*if (Main.IsRoundOne)
                    {
                        engineerOptions.EngineerCooldown = 10f;
                        Main.IsRoundOne = false;
                    }
                    else if (!Main.VettedThisRound)
                        engineerOptions.EngineerCooldown = Options.VetCD.GetFloat();
                    else
                        engineerOptions.EngineerCooldown = Options.VetCD.GetFloat() + Options.VetDuration.GetFloat();*/
                    engineerOptions.EngineerCooldown = 5f;
                    engineerOptions.EngineerInVentMaxTime = 0.5f;
                    break;
                case CustomRoles.Survivor:
                    engineerOptions.EngineerInVentMaxTime = 1;
                    foreach (var ar in Main.SurvivorStuff)
                    {
                        if (ar.Key != player.PlayerId) break;
                        // now we set it to true
                        var stuff = Main.SurvivorStuff[player.PlayerId];
                        if (stuff.Item1 != Options.NumOfVests.GetInt())
                        {
                            if (stuff.Item5)
                            {
                                engineerOptions.EngineerCooldown = 10;
                                stuff.Item5 = false;
                                Main.SurvivorStuff[player.PlayerId] = stuff;
                            }
                            else if (!stuff.Item4)
                                engineerOptions.EngineerCooldown = Options.VestCD.GetFloat();
                            else
                                engineerOptions.EngineerCooldown = Options.VestCD.GetFloat() + Options.VestDuration.GetFloat();
                        }
                        else
                        {
                            engineerOptions.EngineerCooldown = 999;
                        }
                    }
                    break;
                case CustomRoles.Opportunist:
                    engineerOptions.EngineerInVentMaxTime = 1;
                    engineerOptions.EngineerCooldown = 999999;
                    break;
                case CustomRoles.GuardianAngelTOU:
                    if (Main.IsRoundOneGA)
                    {
                        engineerOptions.EngineerCooldown = 10f;
                        Main.IsRoundOneGA = false;
                    }
                    else if (!Main.ProtectedThisRound)
                        engineerOptions.EngineerCooldown = Options.GuardCD.GetFloat();
                    else
                        engineerOptions.EngineerCooldown = Options.GuardCD.GetFloat() + Options.GuardDur.GetFloat();
                    engineerOptions.EngineerInVentMaxTime = 1;
                    break;
                case CustomRoles.Jester:
                    options.SetVision(player, Options.JesterHasImpostorVision.GetBool());
                    if (Utils.IsActive(SystemTypes.Electrical) && Options.JesterHasImpostorVision.GetBool())
                        options.CrewLightMod *= 5;
                    if (Options.JesterCanVent.GetBool())
                        goto InfinityVent;
                    break;
                case CustomRoles.Mare:
                    Mare.ApplyGameOptions(options, player.PlayerId);
                    break;
                case CustomRoles.Ninja:
                    shapeshifterOptions.ShapeshifterCooldown = 0.1f;
                    shapeshifterOptions.ShapeshifterDuration = 0f;
                    break;
                case CustomRoles.Miner:
                    shapeshifterOptions.ShapeshifterCooldown = 0.1f;
                    shapeshifterOptions.ShapeshifterDuration = 1f;
                    break;
                case CustomRoles.Grenadier:
                    shapeshifterOptions.ShapeshifterCooldown = Options.FlashCooldown.GetFloat();
                    shapeshifterOptions.ShapeshifterDuration = Options.FlashDuration.GetFloat();
                    break;
                case CustomRoles.Hitman:
                    options.SetVision(player, Options.HitmanHasImpVision.GetBool());
                    break;
                case CustomRoles.Werewolf:
                    if (!Main.IsRampaged)
                        options.SetVision(player, false);
                    else
                        options.SetVision(player, true);
                    goto InfinityVent;
                //break;
                case CustomRoles.TheGlitch:
                    options.SetVision(player, true);
                    break;
                case CustomRoles.Jackal:
                case CustomRoles.Sidekick:
                case CustomRoles.JSchrodingerCat:
                    options.SetVision(player, Options.JackalHasImpostorVision.GetBool());
                    break;


                InfinityVent:
                    engineerOptions.EngineerCooldown = 0;
                    engineerOptions.EngineerInVentMaxTime = 0;
                    break;
            }
            // Modifiers and Other Things //
            switch (player.GetCustomSubRole())
            {
                case CustomRoles.Torch:
                    if (Utils.IsActive(SystemTypes.Electrical))
                        options.CrewLightMod *= 5;
                    break;
                case CustomRoles.Flash:
                    Main.AllPlayerSpeed[player.PlayerId] = Options.FlashSpeed.GetFloat();
                    break;
                case CustomRoles.Escalation:
                    Main.AllPlayerSpeed[player.PlayerId] = Main.RealOptionsData.AsNormalOptions()!.PlayerSpeedMod;
                    int deadAmount = PlayerState.GetDeadPeopleAmount();
                    if (deadAmount != 0 && Main.AllPlayerSpeed[player.PlayerId] != deadAmount * 0.25f + 1f)
                    {
                        Main.AllPlayerSpeed[player.PlayerId] *= deadAmount * 0.25f + 1f;
                        Logger.Info($"{player.GetNameWithRole()} has gotten faster! Their speed is now {Main.AllPlayerSpeed[player.PlayerId]}.", "Escalation Speed Update");
                    }
                    break;
                case CustomRoles.Bewilder:
                    if (player.Is(CustomRoles.Lighter))
                    {
                        if (player.GetPlayerTaskState().IsTaskFinished)
                        {
                            options.CrewLightMod = Options.LighterTaskCompletedVision.GetFloat();
                            if (Utils.IsActive(SystemTypes.Electrical) && Options.LighterTaskCompletedDisableLightOut.GetBool())
                                options.CrewLightMod *= 5;
                        }
                        else options.CrewLightMod = Options.BewilderVision.GetFloat();
                    }
                    else options.CrewLightMod = Options.BewilderVision.GetFloat();
                    break;
                case CustomRoles.Watcher:
                    if (options.AnonymousVotes)
                        options.AnonymousVotes = false;
                    break;
            }
            if (Main.AllPlayerKillCooldown.ContainsKey(player.PlayerId))
            {
                foreach (var kc in Main.AllPlayerKillCooldown)
                {
                    if (kc.Key == player.PlayerId)
                        options.KillCooldown = kc.Value > 0 ? kc.Value : 0.1f;
                }
            }

            switch (player.GetCustomRole())
            {
                case CustomRoles.NeutWitch:
                    options.KillCooldown = Options.ControlCooldown.GetFloat();
                    break;
                case CustomRoles.Hitman:
                    // Main.AllPlayerKillCooldown[player.PlayerId] = Options.HitmanKillCooldown.GetFloat();
                    break;
                case CustomRoles.Escort:
                    options.KillCooldown = Options.EscortCooldown.GetFloat() + Options.GlobalRoleBlockDuration.GetFloat();
                    break;
                case CustomRoles.Crusader:
                    options.KillCooldown = Options.CrusadeCooldown.GetFloat();
                    break;
            }

            if (Main.KilledDiseased.Contains(player.PlayerId))
                options.KillCooldown *= Options.DiseasedMultiplier.GetFloat();

            if (Main.AllPlayerSpeed.ContainsKey(player.PlayerId))
            {
                foreach (var speed in Main.AllPlayerSpeed)
                {
                    if (speed.Key == player.PlayerId)
                        options.PlayerSpeedMod = Mathf.Clamp(speed.Value, 0.0001f, 3f);
                }
            }
            if (Options.GhostCanSeeOtherVotes.GetBool() && player.Data.IsDead && options.AnonymousVotes)
                options.AnonymousVotes = false;
            if (Options.SyncButtonMode.GetBool() && Options.SyncedButtonCount.GetSelection() <= Options.UsedButtonCount)
                options.EmergencyCooldown = 3600;
            if (!Options.FreeForAllOn.GetBool())
                if ((Options.CurrentGameMode() == CustomGameMode.HideAndSeek || Options.IsStandardHAS) && Options.HideAndSeekKillDelayTimer > 0 && !Options.SplatoonOn.GetBool())
                {
                    options.ImpostorLightMod = 0f;
                    if (player.GetCustomRole().IsImpostor() || player.Is(CustomRoles.Egoist)) options.PlayerSpeedMod = 0.0001f;
                }
            if (Options.TosOptions.GetBool() && Options.GameProgression.GetBool())
            {
                options.DiscussionTime = 45;
                options.VotingTime = 30;
            }
            options.DiscussionTime = Mathf.Clamp(Main.DiscussionTime, 0, 300);
            options.VotingTime = Mathf.Clamp(Main.VotingTime, TimeThief.LowerLimitVotingTime.GetInt(), 300);

            if (Manipulator.MeetingIsSabotaged && !player.Data.IsDead && Manipulator.IsEnable())
            {
                options.AnonymousVotes = !options.AnonymousVotes;
                options.DiscussionTime = Manipulator.DiscussionTimeOnSabotage.GetInt();
                options.VotingTime = Manipulator.VotingTimeOnSabotage.GetInt();
            }
            if (Options.TosOptions.GetBool() && Options.RoundReview.GetBool())
                options.DiscussionTime += (Main.DeadPlayersThisRound.Count * 20) + 10;

            shapeshifterOptions.ShapeshifterCooldown = Mathf.Max(1f, shapeshifterOptions.ShapeshifterCooldown);
            if (Main.KilledBewilder.Contains(player.PlayerId) && !player.Is(CustomRoles.CovenWitch))
            {
                options.CrewLightMod = Options.BewilderVision.GetFloat();
                options.ImpostorLightMod = Options.BewilderVision.GetFloat();
            }
            if (player.GetCustomRole().IsCoven() && Main.HasNecronomicon)
            {
                options.SetVision(player, true);
                engineerOptions.EngineerCooldown = 0;
                engineerOptions.EngineerInVentMaxTime = 0;
            }
            if (Main.Grenaiding)
            {
                if (!player.GetCustomRole().IsImpostorTeam())
                {
                    options.CrewLightMod = 0f;
                    options.ImpostorLightMod = 0f;
                }
            }
            else if (Main.ResetVision)
            {
                if (!player.GetCustomRole().IsImpostorTeam())
                {
                    options.CrewLightMod = Main.RealOptionsData.AsNormalOptions()!.CrewLightMod;
                    options.ImpostorLightMod = Main.RealOptionsData.AsNormalOptions()!.ImpostorLightMod;
                }
            }

            if (player.AmOwner)
                GameOptionsManager.Instance.CurrentGameOptions = options.Cast<IGameOptions>();

            // MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SyncSettings, SendOption.Reliable, clientId);
            //writer.WriteBytesAndSize(options.Cast<IGameOptions>().ToBytes());
            //AmongUsClient.Instance.FinishRpcImmediately(writer);
            DesyncOptions.SyncToPlayer(options.Cast<IGameOptions>(), player);
        }
        public static IGameOptions DeepCopy(this IGameOptions opt)
        {
            return GameOptionsManager.Instance.gameOptionsFactory.FromBytes(opt.ToBytes());
        }
        public static TaskState GetPlayerTaskState(this PlayerControl player)
        {
            return PlayerState.taskState[player.PlayerId];
        }

        public static string GetRoleName(this PlayerControl player)
        {
            return $"{Utils.GetRoleName(player.GetCustomRole())}" /*({getString("Last")})"*/;
        }
        public static string GetSubRoleName(this PlayerControl player)
        {
            return $"{Utils.GetRoleName(player.GetCustomSubRole())}";
        }
        public static string GetAllRoleName(this PlayerControl player)
        {
            if (!player) return null;
            var text = player.GetRoleName();
            text += player.GetCustomSubRole() != CustomRoles.NoSubRoleAssigned ? $" + {player.GetSubRoleName()}" : "";
            return text;
        }
        public static string GetNameWithRole(this PlayerControl player)
        {
            return $"{player?.Data?.PlayerName}({player?.GetAllRoleName()})";
        }
        public static string GetRoleColorCode(this PlayerControl player)
        {
            return Utils.GetRoleColorCode(player.GetCustomRole());
        }
        public static Color GetRoleColor(this PlayerControl player)
        {
            return Utils.GetRoleColor(player.GetCustomRole());
        }
        public static void ResetPlayerCam(this PlayerControl pc, float delay = 0f)
        {
            if (pc == null || !AmongUsClient.Instance.AmHost || pc.AmOwner) return;
            int clientId = pc.GetClientId();

            byte reactorId = 3;
            if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 2) reactorId = 21;

            new LateTask(() =>
            {
                MessageWriter SabotageWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, clientId);
                SabotageWriter.Write(reactorId);
                MessageExtensions.WriteNetObject(SabotageWriter, pc);
                SabotageWriter.Write((byte)128);
                AmongUsClient.Instance.FinishRpcImmediately(SabotageWriter);
            }, 0f + delay, "Reactor Desync");

            new LateTask(() =>
            {
                MessageWriter MurderWriter = AmongUsClient.Instance.StartRpcImmediately(pc.NetId, (byte)RpcCalls.MurderPlayer, SendOption.Reliable, clientId);
                MessageExtensions.WriteNetObject(MurderWriter, pc);
                AmongUsClient.Instance.FinishRpcImmediately(MurderWriter);
            }, 0.2f + delay, "Murder To Reset Cam");

            new LateTask(() =>
            {
                MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, clientId);
                SabotageFixWriter.Write(reactorId);
                MessageExtensions.WriteNetObject(SabotageFixWriter, pc);
                SabotageFixWriter.Write((byte)16);
                AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
            }, 0.4f + delay, "Fix Desync Reactor");

            if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 4) //Airship用
                new LateTask(() =>
                {
                    MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, clientId);
                    SabotageFixWriter.Write(reactorId);
                    MessageExtensions.WriteNetObject(SabotageFixWriter, pc);
                    SabotageFixWriter.Write((byte)17);
                    AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
                }, 0.4f + delay, "Fix Desync Reactor 2");
        }
        public static void DoMysticStuff(this PlayerControl pc, float delay = 0f)
        {
            if (pc == null || !AmongUsClient.Instance.AmHost || pc.AmOwner) return;
            int clientId = pc.GetClientId();

            byte reactorId = 3;
            if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 2) reactorId = 21;

            new LateTask(() =>
            {
                MessageWriter SabotageWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, clientId);
                SabotageWriter.Write(reactorId);
                MessageExtensions.WriteNetObject(SabotageWriter, pc);
                SabotageWriter.Write((byte)128);
                AmongUsClient.Instance.FinishRpcImmediately(SabotageWriter);
            }, 0f + delay, "Reactor Desync for Mystic");

            new LateTask(() =>
            {
                MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, clientId);
                SabotageFixWriter.Write(reactorId);
                MessageExtensions.WriteNetObject(SabotageFixWriter, pc);
                SabotageFixWriter.Write((byte)16);
                AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
            }, 0.4f + delay, "Fix Desync Reactor for Mystic");

            if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 4) //Airship用
                new LateTask(() =>
                {
                    MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, clientId);
                    SabotageFixWriter.Write(reactorId);
                    MessageExtensions.WriteNetObject(SabotageFixWriter, pc);
                    SabotageFixWriter.Write((byte)17);
                    AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
                }, 0.4f + delay, "Fix Desync Reactor 2 for Mystic");
        }

        public static string GetRealName(this PlayerControl player, bool isMeeting = false)
        {
            return isMeeting ? player?.Data?.PlayerName : player?.name;
        }
        public static bool IsSpellMode(this PlayerControl player)
        {
            if (!Main.KillOrSpell.TryGetValue(player.PlayerId, out var KillOrSpell))
            {
                Main.KillOrSpell[player.PlayerId] = false;
                KillOrSpell = false;
            }
            return KillOrSpell;
        }
        public static bool IsHexMode(this PlayerControl player)
        {
            if (!Main.KillOrSpell.TryGetValue(player.PlayerId, out var KillOrHex))
            {
                if (Main.HasNecronomicon)
                {
                    Main.KillOrSpell[player.PlayerId] = false;
                    KillOrHex = false;
                }
                else
                {
                    Main.KillOrSpell[player.PlayerId] = true;
                    KillOrHex = true;
                }
            }
            return KillOrHex;
        }
        public static void SyncKillOrSpell(this PlayerControl player)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetKillOrSpell, SendOption.Reliable, -1);
            writer.Write(player.PlayerId);
            writer.Write(player.IsSpellMode());
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void SyncKillOrHex(this PlayerControl player)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetKillOrSpell, SendOption.Reliable, -1);
            writer.Write(player.PlayerId);
            writer.Write(player.IsHexMode());
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static bool CanUseKillButton(this PlayerControl pc)
        {
            bool canUse =
                pc.GetCustomRole().IsImpostor() || pc.Is(CustomRoles.Arsonist);

            return pc.GetCustomRole() switch
            {
                CustomRoles.Mafia => Utils.CanMafiaKill() && canUse,
                CustomRoles.Mare => Utils.IsActive(SystemTypes.Electrical) || Mare.MareCanKillLightsOn.GetBool(),
                CustomRoles.FireWorks => FireWorks.CanUseKillButton(pc),
                CustomRoles.Sniper => Sniper.CanUseKillButton(pc),
                CustomRoles.Sheriff => Sheriff.CanUseKillButton(pc),
                CustomRoles.Investigator => Investigator.CanUseKillButton(pc),
                CustomRoles.Arsonist => false,
                CustomRoles.PlagueBearer => true,
                CustomRoles.Pestilence => true,
                CustomRoles.Juggernaut => true,
                CustomRoles.Hitman => true,
                CustomRoles.Escort => true,
                CustomRoles.Crusader => true,
                CustomRoles.Werewolf => true,
                CustomRoles.TheGlitch => true,
                CustomRoles.Medusa => true,
                CustomRoles.Coven => true,
                CustomRoles.Painter => true,
                CustomRoles.Janitor => true,
                CustomRoles.CovenWitch => true,
                CustomRoles.PotionMaster => true,
                CustomRoles.HexMaster => true,
                _ => canUse,
            };
        }
        public static bool IsLastImpostor(this PlayerControl pc)
        { //キルクールを変更するインポスター役職は省く
            return pc.GetCustomRole().IsImpostor() &&
                !pc.Data.IsDead &&
                Options.CurrentGameMode() != CustomGameMode.HideAndSeek &&
                Options.EnableLastImpostor.GetBool() &&
                !pc.Is(CustomRoles.Vampire) &&
                !pc.Is(CustomRoles.CorruptedSheriff) &&
                !pc.Is(CustomRoles.BountyHunter) &&
                !pc.Is(CustomRoles.SerialKiller) &&
                Main.AliveImpostorCount == 1;
        }
        public static bool CurrentlyLastImpostor(this PlayerControl pc)
        { //キルクールを変更するインポスター役職は省く
            return pc.GetCustomRole().IsImpostor() &&
                !pc.Data.IsDead &&
                Options.CurrentGameMode() != CustomGameMode.HideAndSeek &&
                !pc.Is(CustomRoles.CorruptedSheriff) &&
                Main.AliveImpostorCount == 1;
        }
        public static bool LastImpostor(this PlayerControl pc)
        {
            return pc.GetCustomRole().IsImpostor() | pc.Is(CustomRoles.Egoist) &&
                Main.AllImpostorCount <= 1;
        }
        public static bool IsDousedPlayer(this PlayerControl arsonist, PlayerControl target)
        {
            if (arsonist == null) return false;
            if (target == null) return false;
            if (Main.isDoused == null) return false;
            Main.isDoused.TryGetValue((arsonist.PlayerId, target.PlayerId), out bool isDoused);
            return isDoused;
        }
        public static bool IsHexedPlayer(this PlayerControl hexer, PlayerControl target)
        {
            if (hexer == null) return false;
            if (target == null) return false;
            if (Main.isHexed == null) return false;
            Main.isHexed.TryGetValue((hexer.PlayerId, target.PlayerId), out bool isHexed);
            return isHexed;
        }
        public static bool IsInfectedPlayer(this PlayerControl plaguebearer, PlayerControl target)
        {
            if (plaguebearer == null) return false;
            if (target == null) return false;
            if (Main.isInfected == null) return false;
            Main.isInfected.TryGetValue((plaguebearer.PlayerId, target.PlayerId), out bool isInfected);
            return isInfected;
        }
        public static void RpcSetDousedPlayer(this PlayerControl player, PlayerControl target, bool isDoused)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetDousedPlayer, SendOption.Reliable, -1);//RPCによる同期
            writer.Write(player.PlayerId);
            writer.Write(target.PlayerId);
            writer.Write(isDoused);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RpcSetInfectedPlayer(this PlayerControl player, PlayerControl target, bool isDoused)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetInfectedPlayer, SendOption.Reliable, -1);//RPCによる同期
            writer.Write(player.PlayerId);
            writer.Write(target.PlayerId);
            writer.Write(isDoused);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void RpcSetHexedPlayer(this PlayerControl player, PlayerControl target, bool isDoused)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetHexedPlayer, SendOption.Reliable, -1);//RPCによる同期
            writer.Write(player.PlayerId);
            writer.Write(target.PlayerId);
            writer.Write(isDoused);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void ExiledSchrodingerCatTeamChange(this PlayerControl player)
        {
            var rand = new System.Random();
            List<CustomRoles> RandSchrodinger = new()
            {
                CustomRoles.CSchrodingerCat,
                CustomRoles.MSchrodingerCat
            };
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (CustomRoles.Egoist.IsEnable() && pc.Is(CustomRoles.Egoist) && !pc.Data.IsDead)
                    RandSchrodinger.Add(CustomRoles.EgoSchrodingerCat);

                if (CustomRoles.Jackal.IsEnable() && pc.Is(CustomRoles.Jackal) && !pc.Data.IsDead)
                    RandSchrodinger.Add(CustomRoles.JSchrodingerCat);
            }
            var SchrodingerTeam = RandSchrodinger[rand.Next(RandSchrodinger.Count)];
            player.RpcSetCustomRole(SchrodingerTeam);
        }
        public static bool SameTeamAsTarget(this PlayerControl killer, PlayerControl target)
        {
            if (killer.Is(CustomRoles.BloodKnight) && target.Is(CustomRoles.BKSchrodingerCat)) return true;
            if (killer.Is(CustomRoles.CrewPostor) && target.Is(CustomRoles.CPSchrodingerCat)) return true;
            if (killer.Is(CustomRoles.Juggernaut) && target.Is(CustomRoles.JugSchrodingerCat)) return true;
            if (killer.Is(CustomRoles.Marksman) && target.Is(CustomRoles.MMSchrodingerCat)) return true;
            if (killer.Is(CustomRoles.Pestilence) && target.Is(CustomRoles.PesSchrodingerCat)) return true;
            if (killer.Is(CustomRoles.Werewolf) && target.Is(CustomRoles.WWSchrodingerCat)) return true;
            if (killer.Is(CustomRoles.TheGlitch) && target.Is(CustomRoles.TGSchrodingerCat)) return true;
            return false;
        }
        public static void ResetKillCooldown(this PlayerControl player, bool meeting = false)
        {
            Main.AllPlayerKillCooldown[player.PlayerId] = Options.DefaultKillCooldown;
            switch (player.GetCustomRole())
            {
                case CustomRoles.Marksman:
                    Main.AllPlayerKillCooldown[player.PlayerId] = Options.MarksmanKillCooldown.GetFloat();
                    break;
                case CustomRoles.NeutWitch:
                    Main.AllPlayerKillCooldown[player.PlayerId] = Options.ControlCooldown.GetFloat();
                    break;
                case CustomRoles.Hitman:
                    // Main.AllPlayerKillCooldown[player.PlayerId] = Options.HitmanKillCooldown.GetFloat();
                    break;
                case CustomRoles.Manipulator:
                    Main.AllPlayerKillCooldown[player.PlayerId] += Manipulator.AddedKillCooldown.GetFloat();
                    break;
                case CustomRoles.AgiTater:
                    Main.AllPlayerKillCooldown[player.PlayerId] = AgiTater.BombCooldown.GetFloat();
                    break;
                case CustomRoles.Bomber:
                    Main.AllPlayerKillCooldown[player.PlayerId] = Bomber.BombCooldown.GetFloat();
                    break;
                case CustomRoles.Juggernaut:
                    float DecreasedAmount = Main.JugKillAmounts * Options.JuggerDecrease.GetFloat();
                    Main.AllPlayerKillCooldown[player.PlayerId] = Options.JuggerKillCooldown.GetFloat() - DecreasedAmount;
                    break;
                case CustomRoles.Escort:
                    Main.AllPlayerKillCooldown[player.PlayerId] = Options.EscortCooldown.GetFloat() + Options.GlobalRoleBlockDuration.GetFloat();
                    break;
                case CustomRoles.Crusader:
                    Main.AllPlayerKillCooldown[player.PlayerId] = Options.CrusadeCooldown.GetFloat();
                    break;
                case CustomRoles.TheGlitch:
                    if (Main.IsHackMode)
                        Main.AllPlayerKillCooldown[player.PlayerId] = Options.GlitchRoleBlockCooldown.GetFloat() + Options.GlobalRoleBlockDuration.GetFloat();
                    else
                        Main.AllPlayerKillCooldown[player.PlayerId] = Options.GlitchKillCooldown.GetFloat();
                    break;
                case CustomRoles.YingYanger:
                    if (Main.DoingYingYang)
                        Main.AllPlayerKillCooldown[player.PlayerId] = Options.YinYangCooldown.GetFloat();
                    break;
                case CustomRoles.SerialKiller:
                    SerialKiller.ApplyKillCooldown(player.PlayerId);
                    break;
                case CustomRoles.TimeThief:
                    TimeThief.SetKillCooldown(player.PlayerId);
                    break;
                case CustomRoles.Mare:
                    Mare.SetKillCooldown(player.PlayerId);
                    break;
                case CustomRoles.Arsonist:
                    Main.AllPlayerKillCooldown[player.PlayerId] = Options.ArsonistCooldown.GetFloat(); //アーソニストはアーソニストのキルクールに。
                    break;
                case CustomRoles.Werewolf:
                    Main.AllPlayerKillCooldown[player.PlayerId] = Options.WWkillCD.GetFloat(); //アーソニストはアーソニストのキルクールに。
                    break;
                case CustomRoles.Egoist:
                    Egoist.ApplyKillCooldown(player.PlayerId);
                    break;
                case CustomRoles.Silencer:
                    if (Main.SilencedPlayer.Count <= 0)
                    {
                        Main.AllPlayerKillCooldown[player.PlayerId] = Options.SilenceCooldown.GetFloat();
                    }
                    else
                    {
                        Main.AllPlayerKillCooldown[player.PlayerId] = Options.DefaultKillCooldown;
                    }
                    break;
                case CustomRoles.Sidekick:
                case CustomRoles.Jackal:
                    Main.AllPlayerKillCooldown[player.PlayerId] = Options.JackalKillCooldown.GetFloat();
                    break;
                case CustomRoles.CorruptedSheriff:
                case CustomRoles.Sheriff:
                    Sheriff.SetKillCooldown(player.PlayerId); //シェリフはシェリフのキルクールに。
                    break;
                case CustomRoles.Investigator:
                    Investigator.SetKillCooldown(player.PlayerId); //シェリフはシェリフのキルクールに。
                    break;
                case CustomRoles.Pestilence:
                    Main.AllPlayerKillCooldown[player.PlayerId] = Options.PestilKillCooldown.GetFloat();
                    break;
                case CustomRoles.BloodKnight:
                    Main.AllPlayerKillCooldown[player.PlayerId] = Options.BKkillCd.GetFloat();
                    break;
                case CustomRoles.PlagueBearer:
                    Main.AllPlayerKillCooldown[player.PlayerId] = Options.InfectCooldown.GetFloat();
                    break;
                case CustomRoles.CovenWitch:
                    Main.AllPlayerKillCooldown[player.PlayerId] = Options.CovenKillCooldown.GetFloat();
                    break;
                case CustomRoles.Medusa:
                    Main.AllPlayerKillCooldown[player.PlayerId] = Options.CovenKillCooldown.GetFloat();
                    break;
                case CustomRoles.HexMaster:
                    if (player.IsHexMode())
                        Main.AllPlayerKillCooldown[player.PlayerId] = Options.HexCD.GetFloat();
                    else
                        Main.AllPlayerKillCooldown[player.PlayerId] = Options.CovenKillCooldown.GetFloat();
                    break;
                case CustomRoles.Janitor:
                case CustomRoles.Painter:
                    Main.AllPlayerKillCooldown[player.PlayerId] = Options.STCD.GetFloat() * 2;
                    break;
            }
            if (player.IsLastImpostor() & !player.Is(CustomRoles.Bomber))
                Main.AllPlayerKillCooldown[player.PlayerId] = Options.LastImpostorKillCooldown.GetFloat();
            if (player.GetCustomRole() is CustomRoles.Vampire or CustomRoles.PlagueBearer or CustomRoles.Arsonist && meeting)
                Main.AllPlayerKillCooldown[player.PlayerId] /= 2;
        }
        public static float GetKillCooldown(this PlayerControl player)
        {
            float KillCooldown = Options.DefaultKillCooldown;
            switch (player.GetCustomRole())
            {
                case CustomRoles.Marksman:
                    KillCooldown = Options.MarksmanKillCooldown.GetFloat();
                    break;
                case CustomRoles.NeutWitch:
                    KillCooldown = Options.ControlCooldown.GetFloat();
                    break;
                case CustomRoles.Hitman:
                    // KillCooldown = Options.HitmanKillCooldown.GetFloat();
                    break;
                case CustomRoles.Juggernaut:
                    float DecreasedAmount = Main.JugKillAmounts * Options.JuggerDecrease.GetFloat();
                    KillCooldown = Options.JuggerKillCooldown.GetFloat() - DecreasedAmount;
                    break;
                case CustomRoles.Escort:
                    KillCooldown = Options.EscortCooldown.GetFloat() + Options.GlobalRoleBlockDuration.GetFloat();
                    break;
                case CustomRoles.Crusader:
                    KillCooldown = Options.CrusadeCooldown.GetFloat();
                    break;
                case CustomRoles.Bomber:
                    KillCooldown = Bomber.BombCooldown.GetFloat();
                    break;
                case CustomRoles.TheGlitch:
                    if (Main.IsHackMode)
                        KillCooldown = Options.GlitchRoleBlockCooldown.GetFloat() + Options.GlobalRoleBlockDuration.GetFloat();
                    else
                        KillCooldown = Options.GlitchKillCooldown.GetFloat();
                    break;
                case CustomRoles.YingYanger:
                    if (Main.DoingYingYang)
                        KillCooldown = Options.YinYangCooldown.GetFloat();
                    break;
                case CustomRoles.SerialKiller:
                    SerialKiller.ApplyKillCooldown(player.PlayerId);
                    break;
                case CustomRoles.TimeThief:
                    TimeThief.SetKillCooldown(player.PlayerId);
                    break;
                case CustomRoles.Mare:
                    Mare.SetKillCooldown(player.PlayerId);
                    break;
                case CustomRoles.Arsonist:
                    KillCooldown = Options.ArsonistCooldown.GetFloat(); //アーソニストはアーソニストのキルクールに。
                    break;
                case CustomRoles.Werewolf:
                    KillCooldown = Options.WWkillCD.GetFloat(); //アーソニストはアーソニストのキルクールに。
                    break;
                case CustomRoles.Egoist:
                    Egoist.ApplyKillCooldown(player.PlayerId);
                    break;
                case CustomRoles.Silencer:
                    if (Main.SilencedPlayer.Count <= 0)
                    {
                        KillCooldown = Options.SilenceCooldown.GetFloat();
                    }
                    else
                    {
                        KillCooldown = Options.DefaultKillCooldown;
                    }
                    break;
                case CustomRoles.Sidekick:
                case CustomRoles.Jackal:
                    KillCooldown = Options.JackalKillCooldown.GetFloat();
                    break;
                case CustomRoles.CorruptedSheriff:
                case CustomRoles.Sheriff:
                    Sheriff.SetKillCooldown(player.PlayerId); //シェリフはシェリフのキルクールに。
                    break;
                case CustomRoles.Investigator:
                    Investigator.SetKillCooldown(player.PlayerId); //シェリフはシェリフのキルクールに。
                    break;
                case CustomRoles.Pestilence:
                    KillCooldown = Options.PestilKillCooldown.GetFloat();
                    break;
                case CustomRoles.BloodKnight:
                    KillCooldown = Options.BKkillCd.GetFloat();
                    break;
                case CustomRoles.PlagueBearer:
                    KillCooldown = Options.InfectCooldown.GetFloat();
                    break;
                case CustomRoles.CovenWitch:
                    KillCooldown = Options.CovenKillCooldown.GetFloat();
                    break;
                case CustomRoles.Medusa:
                    KillCooldown = Options.CovenKillCooldown.GetFloat();
                    break;
                case CustomRoles.HexMaster:
                    if (player.IsHexMode())
                        KillCooldown = Options.HexCD.GetFloat();
                    else
                        KillCooldown = Options.CovenKillCooldown.GetFloat();
                    break;
                case CustomRoles.Janitor:
                case CustomRoles.Painter:
                    KillCooldown = Options.STCD.GetFloat() * 2;
                    break;
            }
            if (player.IsLastImpostor())
                KillCooldown = Options.LastImpostorKillCooldown.GetFloat();
            if (Main.KilledDiseased.Contains(player.PlayerId))
                KillCooldown *= Options.DiseasedMultiplier.GetFloat();
            return KillCooldown;
        }
        public static void TrapperKilled(this PlayerControl killer, PlayerControl target)
        {
            Logger.Info($"{target?.Data?.PlayerName}はTrapperだった", "Trapper");
            Main.AllPlayerSpeed[killer.PlayerId] = 0.00001f;
            killer.CustomSyncSettings();
            new LateTask(() =>
            {
                Main.AllPlayerSpeed[killer.PlayerId] = Main.RealOptionsData.AsNormalOptions()!.PlayerSpeedMod;
                killer.CustomSyncSettings();
                RPC.PlaySoundRPC(killer.PlayerId, Sounds.TaskComplete);
            }, Options.TrapperBlockMoveTime.GetFloat(), "Trapper BlockMove");
        }
        public static void KillFlash(this PlayerControl player)
        {
            //キルフラッシュ(ブラックアウト+リアクターフラッシュ)の処理
            bool ReactorCheck = false; //リアクターフラッシュの確認
            if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 2) ReactorCheck = Utils.IsActive(SystemTypes.Laboratory);
            else ReactorCheck = Utils.IsActive(SystemTypes.Reactor);

            var Duration = Options.KillFlashDuration.GetFloat();
            if (ReactorCheck) Duration += 0.2f; //リアクター中はブラックアウトを長くする

            //実行
            PlayerState.IsBlackOut[player.PlayerId] = true; //ブラックアウト
            if (player.PlayerId == 0)
            {
                Utils.FlashColor(new(1f, 0f, 0f, 0.5f));
                if (Constants.ShouldPlaySfx()) RPC.PlaySound(player.PlayerId, Sounds.KillSound);
            }
            else if (!ReactorCheck) player.ReactorFlash(0f); //リアクターフラッシュ
            CustomSyncSettings(player);
            new LateTask(() =>
            {
                PlayerState.IsBlackOut[player.PlayerId] = false; //ブラックアウト解除
                CustomSyncSettings(player);
            }, Options.KillFlashDuration.GetFloat(), "RemoveKillFlash");
        }
        public static void ClientExitVent(this PlayerControl pc, Vent vent)
        {
            if (pc == null) return;
            int clientId = pc.GetClientId();
            // MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(pc.NetId, (byte)RpcCalls.ExitVent, SendOption.Reliable, clientId);
            //   writer.Write(vent.Id);
            //  AmongUsClient.Instance.FinishRpcImmediately(writer);

            MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(pc.NetId, (byte)RpcCalls.BootFromVent, SendOption.Reliable, clientId);
            writer2.Write(vent.Id);
            AmongUsClient.Instance.FinishRpcImmediately(writer2);
        }
        public static void ReactorFlash(this PlayerControl pc, float delay = 0f)
        {
            if (pc == null) return;
            int clientId = pc.GetClientId();
            // Logger.Info($"{pc}", "ReactorFlash");
            byte reactorId = 3;
            if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 2) reactorId = 21;
            float FlashDuration = Options.KillFlashDuration.GetFloat();

            new LateTask(() =>
            {
                MessageWriter SabotageWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, clientId);
                SabotageWriter.Write(reactorId);
                MessageExtensions.WriteNetObject(SabotageWriter, pc);
                SabotageWriter.Write((byte)128);
                AmongUsClient.Instance.FinishRpcImmediately(SabotageWriter);
            }, 0f + delay, "Reactor Desync");

            new LateTask(() =>
            {
                MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, clientId);
                SabotageFixWriter.Write(reactorId);
                MessageExtensions.WriteNetObject(SabotageFixWriter, pc);
                SabotageFixWriter.Write((byte)16);
                AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
            }, FlashDuration + delay, "Fix Desync Reactor");

            if (GameOptionsManager.Instance.currentNormalGameOptions.MapId == 4) //Airship用
                new LateTask(() =>
                {
                    MessageWriter SabotageFixWriter = AmongUsClient.Instance.StartRpcImmediately(ShipStatus.Instance.NetId, (byte)RpcCalls.RepairSystem, SendOption.Reliable, clientId);
                    SabotageFixWriter.Write(reactorId);
                    MessageExtensions.WriteNetObject(SabotageFixWriter, pc);
                    SabotageFixWriter.Write((byte)17);
                    AmongUsClient.Instance.FinishRpcImmediately(SabotageFixWriter);
                }, FlashDuration + delay, "Fix Desync Reactor 2");
        }
        public static void DemoKilled(this PlayerControl killer, PlayerControl target)
        {
            Logger.Info($"{killer?.Data?.PlayerName}はTrapperだった", "KilledDemo");
            Logger.Info($"{target?.Data?.PlayerName}はTrapperだった", "IsDemo");
            //killer.Data.PlayerName += $"<color={Utils.GetRoleColorCode(CustomRoles.Demolitionist)}>▲</color>";
            Main.KilledDemo.Add(killer.PlayerId);
            killer.CustomSyncSettings();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.NotifyDemoKill, Hazel.SendOption.Reliable, -1);
            writer.Write(false);
            writer.Write(killer.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            Utils.NotifyRoles(false, killer);
            new LateTask(() =>
            {
                // killer.Data.PlayerName = killer.GetRealName();
                if (!GameStates.IsMeeting)
                {
                    Main.KilledDemo.Remove(killer.PlayerId);
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.NotifyDemoKill, Hazel.SendOption.Reliable, -1);
                    writer.Write(true);
                    writer.Write(killer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    Utils.NotifyRoles(false, killer);
                    if (!killer.inVent && !killer.Data.IsDead)
                    {
                        if (!killer.Is(CustomRoles.Pestilence))
                        {
                            killer.CustomSyncSettings();
                            if (killer.protectedByGuardian)
                                killer.RpcMurderPlayer(killer);
                            killer.RpcMurderPlayer(killer);
                            PlayerState.SetDeathReason(killer.PlayerId, PlayerState.DeathReason.Bombed);
                            PlayerState.SetDead(killer.PlayerId);
                        }
                    }
                    else
                    {
                        killer.CustomSyncSettings();
                        RPC.PlaySoundRPC(killer.PlayerId, Sounds.TaskComplete);
                    }
                }
            }, Options.DemoSuicideTime.GetFloat(), "Demolitionist Time", true);
        }
        public static void VetAlerted(this PlayerControl veteran)
        {
            if (veteran.Is(CustomRoles.Veteran) && !Main.VetIsAlerted)
            {
                Main.VetAlerts++;
                Main.VettedThisRound = true;
                Main.VetIsAlerted = true;
                Main.VetCanAlert = false;
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetVeteranAlert, Hazel.SendOption.Reliable, -1);
                writer.Write(Main.VetAlerts);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetVetAlertState, Hazel.SendOption.Reliable, -1);
                writer2.Write(Main.VetCanAlert);
                AmongUsClient.Instance.FinishRpcImmediately(writer2);
                if (!GameStates.IsMeeting)
                    Utils.NotifyRoles(GameStates.IsMeeting, veteran);
                new LateTask(() =>
                {
                    if (!GameStates.IsMeeting)
                    {
                        Main.VetIsAlerted = false;
                        Utils.NotifyRoles(GameStates.IsMeeting, veteran);
                    }
                    new LateTask(() =>
                    {
                        if (!GameStates.IsMeeting)
                        {
                            Main.VetCanAlert = true;
                            MessageWriter writer3 = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetVetAlertState, Hazel.SendOption.Reliable, -1);
                            writer3.Write(Main.VetCanAlert);
                            AmongUsClient.Instance.FinishRpcImmediately(writer3);
                            Utils.NotifyRoles(GameStates.IsMeeting, veteran);
                        }
                    }, Options.VetCD.GetFloat(), "Veteran Cooldown", true);
                }, Options.VetDuration.GetFloat(), "Veteran Duration", true);
            }
        }
        public static void SurvivorVested(this PlayerControl survivor)
        {
            if (survivor.Is(CustomRoles.Survivor))
            {
                var stuff = Main.SurvivorStuff[survivor.PlayerId];
                if (Main.SurvivorStuff[survivor.PlayerId].Item1 != Options.NumOfVests.GetInt())
                {
                    stuff.Item1++;
                    stuff.Item2 = true;
                    stuff.Item4 = true;
                    Main.SurvivorStuff[survivor.PlayerId] = stuff;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SendSurvivorInfo, Hazel.SendOption.Reliable, -1);
                    writer.Write(survivor.PlayerId);
                    writer.Write(stuff.Item1);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    new LateTask(() =>
                    {
                        stuff.Item2 = false;
                        Main.SurvivorStuff[survivor.PlayerId] = stuff;
                    }, Options.VestDuration.GetFloat(), "Survivor Vesting Duration", true);
                }
            }
        }
        public static void GaProtect(this PlayerControl ga)
        {
            if (ga.Is(CustomRoles.GuardianAngelTOU) && !Main.IsProtected)
            {
                Main.ProtectsSoFar++;
                Main.ProtectedThisRound = true;
                Main.IsProtected = true;
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UpdateGA, Hazel.SendOption.Reliable, -1);
                writer.Write(Main.ProtectsSoFar);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                new LateTask(() =>
                {
                    Main.IsProtected = false;
                }, Options.VetDuration.GetFloat(), "Guardian Angel Protect Duration");
            }
        }
        public static void StoneGazed(this PlayerControl veteran)
        {
            if (veteran.Is(CustomRoles.Medusa) && !Main.IsGazing && Main.GazeReady)
            {
                Main.IsGazing = true;
                Main.GazeReady = false;
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetPirateProgress, Hazel.SendOption.Reliable, -1);
                writer.Write(Main.IsGazing);
                writer.Write(Main.GazeReady);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                new LateTask(() =>
                {
                    Main.IsGazing = false;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetPirateProgress, Hazel.SendOption.Reliable, -1);
                    writer.Write(Main.IsGazing);
                    writer.Write(Main.GazeReady);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    new LateTask(() =>
                    {
                        Main.GazeReady = true;
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetPirateProgress, Hazel.SendOption.Reliable, -1);
                        writer.Write(Main.IsGazing);
                        writer.Write(Main.GazeReady);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                    }, Options.StoneCD.GetFloat(), "Gaze Cooldown");
                }, Options.StoneDuration.GetFloat(), "Gaze Duration");
            }
        }
        public static void CanUseImpostorVent(this PlayerControl player)
        {
            switch (player.GetCustomRole())
            {
                case CustomRoles.Amnesiac:
                case CustomRoles.Sheriff:
                case CustomRoles.Investigator:
                case CustomRoles.AgiTater:
                    DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.ToggleVisible(false);
                    player.Data.Role.CanVent = false;
                    return;
                case CustomRoles.Arsonist:
                    bool CanUse = player.IsDouseDone();
                    if (Options.TOuRArso.GetBool())
                        CanUse = true;
                    DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.ToggleVisible(CanUse && !player.Data.IsDead);
                    player.Data.Role.CanVent = CanUse;
                    return;
                case CustomRoles.Juggernaut:
                    bool jug_canUse = Options.JuggerCanVent.GetBool();
                    DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.ToggleVisible(jug_canUse && !player.Data.IsDead);
                    player.Data.Role.CanVent = jug_canUse;
                    return;
                case CustomRoles.Sidekick:
                case CustomRoles.Jackal:
                    bool jackal_canUse = Options.JackalCanVent.GetBool();
                    DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.ToggleVisible(jackal_canUse && !player.Data.IsDead);
                    player.Data.Role.CanVent = jackal_canUse;
                    return;
                case CustomRoles.Marksman:
                    bool marks_canUse = Options.MarksmanCanVent.GetBool();
                    DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.ToggleVisible(marks_canUse && !player.Data.IsDead);
                    player.Data.Role.CanVent = marks_canUse;
                    return;
                case CustomRoles.PlagueBearer:
                    DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.ToggleVisible(false);
                    player.Data.Role.CanVent = false;
                    return;
                case CustomRoles.Pestilence:
                    bool pesti_CanUse = Options.PestiCanVent.GetBool();
                    DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.ToggleVisible(pesti_CanUse && !player.Data.IsDead);
                    player.Data.Role.CanVent = pesti_CanUse;
                    return;
                case CustomRoles.TheGlitch:
                    bool gl_CanUse = true;
                    DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.ToggleVisible(gl_CanUse && !player.Data.IsDead);
                    player.Data.Role.CanVent = gl_CanUse;
                    return;
                case CustomRoles.Werewolf:
                    bool ww_CanUse = true;
                    DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.ToggleVisible(ww_CanUse && !player.Data.IsDead);
                    player.Data.Role.CanVent = ww_CanUse;
                    return;
                case CustomRoles.CorruptedSheriff:
                case CustomRoles.Medusa:
                    DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.ToggleVisible(true && !player.Data.IsDead);
                    player.Data.Role.CanVent = true;
                    return;
                case CustomRoles.HexMaster:
                case CustomRoles.CovenWitch:
                    DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.ToggleVisible(Main.HasNecronomicon && !player.Data.IsDead);
                    player.Data.Role.CanVent = Main.HasNecronomicon;
                    break;
                case CustomRoles.Janitor:
                case CustomRoles.Painter:
                    DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.ToggleVisible(Options.STIgnoreVent.GetBool() && !player.Data.IsDead);
                    player.Data.Role.CanVent = Options.STIgnoreVent.GetBool();
                    break;
            }
        }
        public static bool IsDouseDone(this PlayerControl player)
        {
            if (!player.Is(CustomRoles.Arsonist)) return false;
            var count = Utils.GetDousedPlayerCount(player.PlayerId);
            return count.Item1 == count.Item2;
        }
        public static bool IsHexedDone(this PlayerControl player)
        {
            if (!player.Is(CustomRoles.HexMaster)) return false;
            var count = Utils.GetHexedPlayerCount(player.PlayerId);
            return count.Item1 == count.Item2;
        }
        public static bool IsInfectDone(this PlayerControl player)
        {
            if (!player.Is(CustomRoles.PlagueBearer)) return false;
            var count = Utils.GetInfectedPlayerCount(player.PlayerId);
            return count.Item1 == count.Item2;
        }
        public static bool CanMakeMadmate(this PlayerControl player)
        {
            return Options.CanMakeMadmateCount.GetInt() > Main.SKMadmateNowCount
                    && player != null
                    && player.Data.Role.Role == RoleTypes.Shapeshifter
                    && !player.Is(CustomRoles.Warlock) && !player.Is(CustomRoles.FireWorks) && !player.Is(CustomRoles.Sniper) && !player.Is(CustomRoles.BountyHunter);
        }
        public static void RpcExileV2(this PlayerControl player)
        {
            player.Exiled();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.Exiled, SendOption.None, -1);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static string GetDeathMessage(this PlayerControl pc)
        {
            try
            {
                if (Main.unreportableBodies.Contains(pc.PlayerId))
                    return "We could not determine how they died. (Cleaned/Stoned)";
                if (PlayerState.GetDeathReason(pc.PlayerId) == PlayerState.DeathReason.Torched)
                    return "They were incinerated by an Arsonist.";
                if (PlayerState.GetDeathReason(pc.PlayerId) == PlayerState.DeathReason.Bombed)
                    return "They were bombed by an Agitater, Fireworks, Bastion, Demolitionist, Hex Master, Postman, Terrorist, or a Bomber if they didn't kill in time.";
                if (PlayerState.GetDeathReason(pc.PlayerId) == PlayerState.DeathReason.Suicide | PlayerState.GetDeathReason(pc.PlayerId) == PlayerState.DeathReason.LoversSuicide | Main.whoKilledWho[pc.Data.PlayerId] == pc.PlayerId)
                    return "They apparently committed suicide.";
                var killer = Utils.GetPlayerById(Main.whoKilledWho[pc.Data.PlayerId]);
                var role = killer.GetCustomRole();
                var roletype = role.GetRoleType();
                if (roletype is RoleType.Impostor)
                    return "They were killed by an Impostor.";
                switch (role)
                {
                    case CustomRoles.Bodyguard:
                        return $"They died from a Bodyguard.";
                    case CustomRoles.TheGlitch:
                        return $"They were hacked by a Glitch.";
                    case CustomRoles.Werewolf:
                        return $"They were mauled by the Werewolf.";
                    case CustomRoles.Sheriff:
                        return $"TThey were shot by the Sheriff.";
                    case CustomRoles.BloodKnight:
                        return $"The Blood Knight fed off of them.";
                    case CustomRoles.Medusa:
                        return $"They were turned to stone from a Medusa.";
                    case CustomRoles.Poisoner:
                        return $"They were poisoned by a Poisoner. (Coven)";
                    case CustomRoles.PoisonMaster:
                        return $"They were poisoned by a Poison Master. (Neutral)";
                    case CustomRoles.Vampire:
                        return $"They were bitten by a Vampire.";
                    case CustomRoles.Sidekick:
                        return $"They were stabbed by the Serial Killer's Sidekick.";
                    case CustomRoles.Jackal:
                        return $"They were stabbed by the Serial Killer.";
                    case CustomRoles.Juggernaut:
                        return $"They were assaulted by a Juggernaut.";
                    case CustomRoles.CrewPostor:
                        return $"They were knocked out by a CrewPostor.";
                    case CustomRoles.Veteran:
                        return $"They were shot by the Veteran.";
                    case CustomRoles.Crusader:
                        return $"They were attacked from a Crusader.";
                    default:
                        return $"They were killed by a {Utils.GetRoleName(killer.GetCustomRole())}";
                }
            }
            catch (Exception ex)
            {
                Logger.SendInGame($"Error loading death reason.\n{ex}");
                var deathReasonFound = PlayerState.deathReasons.TryGetValue(pc.PlayerId, out var deathReason);
                var reason = deathReasonFound ? GetString("DeathReason." + deathReason.ToString()) : "No Death Reason Found";
                return $"We could not determine their role. Their death reason is {reason}";
            }
        }
        public static void RpcMurderPlayerV2(this PlayerControl killer, PlayerControl target)
        {
            if (target == null) target = killer;
            if (AmongUsClient.Instance.AmClient)
            {
                killer.MurderPlayer(target);
            }
            MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(killer.NetId, (byte)RpcCalls.MurderPlayer, SendOption.None, -1);
            messageWriter.WriteNetObject(target);
            AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
            Utils.NotifyRoles();
        }
        public static void RpcShapeshiftV2(this PlayerControl shifter, PlayerControl target, bool shouldAnimate = true)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (shifter.Data.IsDead)
            {
                Utils.SendMessage("We were unable to shift you back into your regular self because of the Innersloth AntiCheat. Sorry!", shifter.PlayerId);
                return;
            }
            if (Main.IsShapeShifted.Contains(shifter.PlayerId))
            {
                Utils.SendMessage("We were unable to shift you back into your regular self because of the Innersloth AntiCheat. Sorry!", shifter.PlayerId);
                return;
            }
            if (Main.CheckShapeshift.ContainsKey(shifter.PlayerId))
            {
                if (Main.CheckShapeshift[shifter.PlayerId])
                {
                    Utils.SendMessage("We were unable to shift you back into your regular self because of the Innersloth AntiCheat. Sorry!", shifter.PlayerId);
                    return;
                }
            }
            shifter.RpcShapeshift(target, shouldAnimate);
        }
        public static void RpcRevertShapeshiftV2(this PlayerControl shifter, bool shouldAnimate = true)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (shifter.Data.IsDead)
            {
                Utils.SendMessage("We were unable to shift you back into your regular self because of the Innersloth AntiCheat. Sorry!", shifter.PlayerId);
                return;
            }
            if (Main.IsShapeShifted.Contains(shifter.PlayerId))
            {
                Utils.SendMessage("We were unable to shift you back into your regular self because of the Innersloth AntiCheat. Sorry!", shifter.PlayerId);
                return;
            }
            if (Main.CheckShapeshift.ContainsKey(shifter.PlayerId))
            {
                if (Main.CheckShapeshift[shifter.PlayerId])
                {
                    Utils.SendMessage("We were unable to shift you back into your regular self because of the Innersloth AntiCheat. Sorry!", shifter.PlayerId);
                    return;
                }
            }
            shifter.RpcRevertShapeshift(shouldAnimate);
        }
        public static void CheckVentSwap(PlayerControl player)
        {
            Vector2? lastLocation = Main.LastEnteredVentLocation.GetValueOrDefault(player.PlayerId);
            if (lastLocation == null) return;
            float distance = Vector2.Distance(lastLocation.Value, player.GetTruePosition());
            if (distance < 1) return;
            Logger.Error($"Player {player.GetNameWithRole()} swapped vents!", "Vent Swap");

            Vector2 playerPos = player.GetTruePosition();
            Dictionary<Vent, float> targetDistance = new();
            float dis;
            foreach (Vent vent in GameObject.FindObjectsOfType<Vent>())
            {
                dis = Vector2.Distance(playerPos, vent.transform.position);
                targetDistance.Add(vent, dis);
            }
            if (targetDistance.Count != 0)
            {
                var min = targetDistance.OrderBy(c => c.Value).FirstOrDefault();
                // gets the closest vent
                Vent vent = min.Key;
                if (Main.LastEnteredVent[player.PlayerId].Id != vent.Id)
                {
                    // they are at a different Id
                    Main.CurrentEnterdVent[player.PlayerId] = vent;
                }
            }
        }
        public static void NoCheckStartMeeting(this PlayerControl reporter, GameData.PlayerInfo target)
        { /*サボタージュ中でも関係なしに会議を起こせるメソッド
            targetがnullの場合はボタンとなる*/
            MeetingRoomManager.Instance.AssignSelf(reporter, target);
            DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(reporter);
            reporter.RpcStartMeeting(target);
        }
        public static bool IsModClient(this PlayerControl player) => Main.playerVersion.ContainsKey(player.PlayerId);
        ///<summary>
        ///プレイヤーのRoleBehaviourのGetPlayersInAbilityRangeSortedを実行し、戻り値を返します。
        ///</summary>
        ///<param name="ignoreColliders">trueにすると、壁の向こう側のプレイヤーが含まれるようになります。守護天使用</param>
        ///<returns>GetPlayersInAbilityRangeSortedの戻り値</returns>
        public static List<PlayerControl> GetPlayersInAbilityRangeSorted(this PlayerControl player, bool ignoreColliders = false) => GetPlayersInAbilityRangeSorted(player, pc => true, ignoreColliders);
        ///<summary>
        ///プレイヤーのRoleBehaviourのGetPlayersInAbilityRangeSortedを実行し、predicateの条件に合わないものを除外して返します。
        ///</summary>
        ///<param name="predicate">リストに入れるプレイヤーの条件 このpredicateに入れてfalseを返すプレイヤーは除外されます。</param>
        ///<param name="ignoreColliders">trueにすると、壁の向こう側のプレイヤーが含まれるようになります。守護天使用</param>
        ///<returns>GetPlayersInAbilityRangeSortedの戻り値から条件に合わないプレイヤーを除外したもの。</returns>
        public static List<PlayerControl> GetPlayersInAbilityRangeSorted(this PlayerControl player, Predicate<PlayerControl> predicate, bool ignoreColliders = false)
        {
            var rangePlayersIL = RoleBehaviour.GetTempPlayerList();
            List<PlayerControl> rangePlayers = new();
            player.Data.Role.GetPlayersInAbilityRangeSorted(rangePlayersIL, ignoreColliders);
            foreach (var pc in rangePlayersIL)
            {
                if (predicate(pc)) rangePlayers.Add(pc);
            }
            return rangePlayers;
        }
        public static bool IsNeutralKiller(this PlayerControl player)
        {
            if (player.GetCustomRole().IsCoven() && Options.SnitchCanFindCoven.GetBool()) return true;
            return
                player.GetCustomRole() is
                CustomRoles.Egoist or
                CustomRoles.Jackal or
                CustomRoles.NeutWitch or
                CustomRoles.Sidekick or
                CustomRoles.PlagueBearer or
                CustomRoles.Juggernaut or
                CustomRoles.AgiTater or
                CustomRoles.Arsonist or
                CustomRoles.Pestilence or
                CustomRoles.Pirate or
                CustomRoles.Marksman or
                CustomRoles.BloodKnight or
                CustomRoles.CorruptedSheriff or
                CustomRoles.TheGlitch or
                CustomRoles.Werewolf;
        }

        public static bool IsDesyncRole(this PlayerControl player)
        {
            if (player.GetCustomRole().IsCoven()) return true;
            return
                player.GetCustomRole() is
                CustomRoles.Egoist or
                CustomRoles.Jackal or
                CustomRoles.Sidekick or
                CustomRoles.PlagueBearer or
                CustomRoles.Juggernaut or
                CustomRoles.Sheriff or
                CustomRoles.CorruptedSheriff or
                CustomRoles.Investigator or
                CustomRoles.Parasite or
                CustomRoles.Janitor or
                CustomRoles.Painter or
                CustomRoles.AgiTater or
                CustomRoles.Arsonist or
                CustomRoles.Pestilence or
                CustomRoles.Crusader or
                CustomRoles.Hitman or
                CustomRoles.Escort or
                CustomRoles.NeutWitch or
                CustomRoles.PoisonMaster or
                CustomRoles.Marksman or
                CustomRoles.BloodKnight or
                CustomRoles.CorruptedSheriff or
                CustomRoles.TheGlitch or
                CustomRoles.Werewolf;
        }
        //汎用
        public static bool Is(this PlayerControl target, CustomRoles role) =>
            role > CustomRoles.NoSubRoleAssigned ? target.GetCustomSubRole() == role : target.GetCustomRole() == role;
        public static bool Is(this PlayerControl target, RoleType type) { return target.GetCustomRole().GetRoleType() == type; }
        public static bool IsAlive(this PlayerControl target) { return target != null && !PlayerState.isDead[target.PlayerId]; }

    }
}
