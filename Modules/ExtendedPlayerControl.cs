using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using InnerNet;
using UnityEngine;
using static TownOfHost.Translator;

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
        public static InnerNet.ClientData GetClient(this PlayerControl player)
        {
            var client = AmongUsClient.Instance.allClients.ToArray().Where(cd => cd.Character.PlayerId == player.PlayerId).FirstOrDefault();
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
                string callerMethodName = callerMethod.Name;
                string callerClassName = callerMethod.DeclaringType.FullName;
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
        public static void RpcResetAbilityCooldown(this PlayerControl target)
        {
            if (!AmongUsClient.Instance.AmHost) return; //ホスト以外が実行しても何も起こさない
            Logger.Info($"アビリティクールダウンのリセット:{target.name}({target.PlayerId})", "RpcResetAbilityCooldown");
            if (PlayerControl.LocalPlayer == target)
            {
                //targetがホストだった場合
                PlayerControl.LocalPlayer.Data.Role.SetCooldown();
            }
            else
            {
                //targetがホスト以外だった場合
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(target.NetId, (byte)RpcCalls.ProtectPlayer, SendOption.None, target.GetClientId());
                writer.Write(0); //writer.WriteNetObject(null); と同じ
                writer.Write(0);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
            /*  nullにバリアを張ろうとすると、アビリティーのクールダウンがリセットされてからnull参照で中断されます。
                ホストに対しての場合、RPCを介さず直接クールダウンを書き換えています。
                万が一他クライアントへの影響があった場合を考慮して、Desyncを使っています。*/
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

        /*public static void RpcBeKilled(this PlayerControl player, PlayerControl KilledBy = null) {
            if(!AmongUsClient.Instance.AmHost) return;
            byte KilledById;
            if(KilledBy == null)
                KilledById = byte.MaxValue;
            else
                KilledById = KilledBy.PlayerId;

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)CustomRPC.BeKilled, Hazel.SendOption.Reliable, -1);
            writer.Write(player.PlayerId);
            writer.Write(KilledById);
            AmongUsClient.Instance.FinishRpcImmediately(writer);

            RPC.BeKilled(player.PlayerId, KilledById);
        }*/
        public static void CustomSyncSettings(this PlayerControl player)
        {
            if (player == null || !AmongUsClient.Instance.AmHost) return;
            if (Main.RealOptionsData == null)
            {
                Main.RealOptionsData = PlayerControl.GameOptions.DeepCopy();
            }

            var clientId = player.GetClientId();
            var opt = Main.RealOptionsData.DeepCopy();

            CustomRoles role = player.GetCustomRole();
            RoleType roleType = role.GetRoleType();
            switch (roleType)
            {
                case RoleType.Impostor:
                    opt.RoleOptions.ShapeshifterCooldown = Options.DefaultShapeshiftCooldown.GetFloat();
                    //opt.SetVision(player, true);
                    break;
                case RoleType.Madmate:
                    opt.RoleOptions.EngineerCooldown = Options.MadmateVentCooldown.GetFloat();
                    opt.RoleOptions.EngineerInVentMaxTime = Options.MadmateVentMaxTime.GetFloat();
                    if (Options.MadmateHasImpostorVision.GetBool())
                        opt.SetVision(player, true);
                    break;
            }

            switch (player.GetCustomRole())
            {
                case CustomRoles.Painter:
                    opt.SetVision(player, Options.PaintersHaveImpVision.GetBool());
                    break;
                case CustomRoles.Marksman:
                    opt.KillDistance = Main.MarksmanKills;
                    opt.killDistance = Main.MarksmanKills;
                    opt.SetVision(player, true);
                    break;
                case CustomRoles.Terrorist:
                    goto InfinityVent;
                // case CustomRoles.ShapeMaster:
                //     opt.RoleOptions.ShapeshifterCooldown = 0.1f;
                //     opt.RoleOptions.ShapeshifterLeaveSkin = false;
                //     opt.RoleOptions.ShapeshifterDuration = Options.ShapeMasterShapeshiftDuration.GetFloat();
                //     break;
                case CustomRoles.Bastion:
                    opt.RoleOptions.EngineerCooldown = 9999;
                    opt.RoleOptions.EngineerInVentMaxTime = 1;
                    break;
                case CustomRoles.Warlock:
                    opt.RoleOptions.ShapeshifterCooldown = Main.isCursed ? 1f : Options.DefaultKillCooldown;
                    break;
                case CustomRoles.SerialKiller:
                    SerialKiller.ApplyGameOptions(opt);
                    break;
                case CustomRoles.BountyHunter:
                    BountyHunter.ApplyGameOptions(opt);
                    break;
                case CustomRoles.EvilWatcher:
                case CustomRoles.NiceWatcher:
                    if (opt.AnonymousVotes)
                        opt.AnonymousVotes = false;
                    break;
                case CustomRoles.Sheriff:
                case CustomRoles.Investigator:
                case CustomRoles.Janitor:
                case CustomRoles.Arsonist:
                case CustomRoles.Amnesiac:
                    opt.SetVision(player, false);
                    break;
                case CustomRoles.PlagueBearer:
                    opt.SetVision(player, false);
                    break;
                case CustomRoles.CorruptedSheriff:
                case CustomRoles.Pestilence:
                    opt.SetVision(player, true);
                    break;
                case CustomRoles.Lighter:
                    if (player.GetPlayerTaskState().IsTaskFinished)
                    {
                        opt.CrewLightMod = Options.LighterTaskCompletedVision.GetFloat();
                        if (Utils.IsActive(SystemTypes.Electrical) && Options.LighterTaskCompletedDisableLightOut.GetBool())
                            opt.CrewLightMod *= 5;
                    }
                    break;
                case CustomRoles.BloodKnight:
                case CustomRoles.EgoSchrodingerCat:
                    opt.SetVision(player, true);
                    break;
                case CustomRoles.Doctor:
                    opt.RoleOptions.ScientistCooldown = 0f;
                    opt.RoleOptions.ScientistBatteryCharge = Options.DoctorTaskCompletedBatteryCharge.GetFloat();
                    break;
                case CustomRoles.Camouflager:
                    opt.RoleOptions.ShapeshifterCooldown = Camouflager.CamouflagerCamouflageCoolDown.GetFloat();
                    opt.RoleOptions.ShapeshifterDuration = Camouflager.CamouflagerCamouflageDuration.GetFloat();
                    break;
                case CustomRoles.Juggernaut:
                    opt.SetVision(player, true);
                    if (Options.JuggerCanVent.GetBool())
                        goto InfinityVent;
                    break;
                case CustomRoles.Vulture:
                    opt.SetVision(player, Options.VultureHasImpostorVision.GetBool());
                    if (Options.VultureCanVent.GetBool())
                        goto InfinityVent;
                    break;
                //case CustomRoles.WereW
                case CustomRoles.Mayor:
                    opt.RoleOptions.EngineerCooldown =
                        Main.MayorUsedButtonCount.TryGetValue(player.PlayerId, out var count) && count < Options.MayorNumOfUseButton.GetInt()
                        ? opt.EmergencyCooldown
                        : 300f;
                    opt.RoleOptions.EngineerInVentMaxTime = 1;
                    break;
                case CustomRoles.Veteran:
                    //5 lines of code calculating the next Vet CD.
                    if (Main.IsRoundOne)
                    {
                        opt.RoleOptions.EngineerCooldown = 10f;
                        Main.IsRoundOne = false;
                    }
                    else if (!Main.VettedThisRound)
                        opt.RoleOptions.EngineerCooldown = Options.VetCD.GetFloat();
                    else
                        opt.RoleOptions.EngineerCooldown = Options.VetCD.GetFloat() + Options.VetDuration.GetFloat();
                    opt.RoleOptions.EngineerInVentMaxTime = 1;
                    break;
                case CustomRoles.Survivor:
                    opt.RoleOptions.EngineerInVentMaxTime = 1;
                    foreach (var ar in Main.SurvivorStuff)
                    {
                        if (ar.Key != player.PlayerId) break;
                        // now we set it to true
                        var stuff = Main.SurvivorStuff[player.PlayerId];
                        if (stuff.Item1 != Options.NumOfVests.GetInt())
                        {
                            if (stuff.Item5)
                            {
                                opt.RoleOptions.EngineerCooldown = 10;
                                stuff.Item5 = false;
                                Main.SurvivorStuff[player.PlayerId] = stuff;
                            }
                            else if (!stuff.Item4)
                                opt.RoleOptions.EngineerCooldown = Options.VestCD.GetFloat();
                            else
                                opt.RoleOptions.EngineerCooldown = Options.VestCD.GetFloat() + Options.VestDuration.GetFloat();
                        }
                        else
                        {
                            opt.RoleOptions.EngineerCooldown = 999;
                        }
                    }
                    break;
                case CustomRoles.Opportunist:
                    opt.RoleOptions.EngineerInVentMaxTime = 1;
                    opt.RoleOptions.EngineerCooldown = 999999;
                    break;
                case CustomRoles.GuardianAngelTOU:
                    if (Main.IsRoundOneGA)
                    {
                        opt.RoleOptions.EngineerCooldown = 10f;
                        Main.IsRoundOneGA = false;
                    }
                    else if (!Main.ProtectedThisRound)
                        opt.RoleOptions.EngineerCooldown = Options.GuardCD.GetFloat();
                    else
                        opt.RoleOptions.EngineerCooldown = Options.GuardCD.GetFloat() + Options.GuardDur.GetFloat();
                    opt.RoleOptions.EngineerInVentMaxTime = 1;
                    break;
                case CustomRoles.Jester:
                    opt.SetVision(player, Options.JesterHasImpostorVision.GetBool());
                    if (Utils.IsActive(SystemTypes.Electrical) && Options.JesterHasImpostorVision.GetBool())
                        opt.CrewLightMod *= 5;
                    if (Options.JesterCanVent.GetBool())
                        goto InfinityVent;
                    break;
                case CustomRoles.Mare:
                    Mare.ApplyGameOptions(opt, player.PlayerId);
                    break;
                case CustomRoles.Ninja:
                    opt.RoleOptions.ShapeshifterCooldown = 0.1f;
                    opt.RoleOptions.ShapeshifterDuration = 0f;
                    break;
                case CustomRoles.Grenadier:
                    opt.RoleOptions.ShapeshifterCooldown = Options.FlashCooldown.GetFloat();
                    opt.RoleOptions.ShapeshifterDuration = Options.FlashDuration.GetFloat();
                    break;
                case CustomRoles.Werewolf:
                    if (!Main.IsRampaged)
                        opt.SetVision(player, false);
                    else
                        opt.SetVision(player, true);
                    goto InfinityVent;
                //break;
                case CustomRoles.TheGlitch:
                    opt.SetVision(player, true);
                    break;
                case CustomRoles.Jackal:
                case CustomRoles.Sidekick:
                case CustomRoles.JSchrodingerCat:
                    opt.SetVision(player, Options.JackalHasImpostorVision.GetBool());
                    break;


                InfinityVent:
                    opt.RoleOptions.EngineerCooldown = 0;
                    opt.RoleOptions.EngineerInVentMaxTime = 0;
                    break;
            }
            // Modifiers and Other Things //
            switch (player.GetCustomSubRole())
            {
                case CustomRoles.Torch:
                    if (Utils.IsActive(SystemTypes.Electrical))
                        opt.CrewLightMod *= 5;
                    break;
                case CustomRoles.Flash:
                    Main.AllPlayerSpeed[player.PlayerId] = Options.FlashSpeed.GetFloat();
                    break;
                case CustomRoles.Bewilder:
                    if (player.Is(CustomRoles.Lighter))
                    {
                        if (player.GetPlayerTaskState().IsTaskFinished)
                        {
                            opt.CrewLightMod = Options.LighterTaskCompletedVision.GetFloat();
                            if (Utils.IsActive(SystemTypes.Electrical) && Options.LighterTaskCompletedDisableLightOut.GetBool())
                                opt.CrewLightMod *= 5;
                        }
                        else opt.CrewLightMod = Options.BewilderVision.GetFloat();
                    }
                    else opt.CrewLightMod = Options.BewilderVision.GetFloat();
                    break;
                case CustomRoles.Watcher:
                case CustomRoles.EvilWatcher:
                case CustomRoles.NiceWatcher:
                    if (opt.AnonymousVotes)
                        opt.AnonymousVotes = false;
                    break;
            }
            if (Main.AllPlayerKillCooldown.ContainsKey(player.PlayerId))
            {
                foreach (var kc in Main.AllPlayerKillCooldown)
                {
                    if (kc.Key == player.PlayerId)
                        opt.KillCooldown = kc.Value > 0 ? kc.Value : 0.01f;
                }
            }

            if (Main.AllPlayerSpeed.ContainsKey(player.PlayerId))
            {
                foreach (var speed in Main.AllPlayerSpeed)
                {
                    if (speed.Key == player.PlayerId)
                        opt.PlayerSpeedMod = Mathf.Clamp(speed.Value, 0.0001f, 3f);
                }
            }
            if (Options.GhostCanSeeOtherVotes.GetBool() && player.Data.IsDead && opt.AnonymousVotes)
                opt.AnonymousVotes = false;
            if (Options.SyncButtonMode.GetBool() && Options.SyncedButtonCount.GetSelection() <= Options.UsedButtonCount)
                opt.EmergencyCooldown = 3600;
            if (!Options.FreeForAllOn.GetBool())
                if ((Options.CurrentGameMode() == CustomGameMode.HideAndSeek || Options.IsStandardHAS) && Options.HideAndSeekKillDelayTimer > 0 && !Options.SplatoonOn.GetBool())
                {
                    opt.ImpostorLightMod = 0f;
                    if (player.GetCustomRole().IsImpostor() || player.Is(CustomRoles.Egoist)) opt.PlayerSpeedMod = 0.0001f;
                }
            opt.DiscussionTime = Mathf.Clamp(Main.DiscussionTime, 0, 300);
            opt.VotingTime = Mathf.Clamp(Main.VotingTime, TimeThief.LowerLimitVotingTime.GetInt(), 300);

            opt.RoleOptions.ShapeshifterCooldown = Mathf.Max(1f, opt.RoleOptions.ShapeshifterCooldown);
            if (Main.KilledBewilder.Contains(player.PlayerId) && !player.Is(CustomRoles.CovenWitch))
            {
                opt.CrewLightMod = Options.BewilderVision.GetFloat();
                opt.ImpostorLightMod = Options.BewilderVision.GetFloat();
            }
            if (player.GetCustomRole().IsCoven() && Main.HasNecronomicon)
            {
                opt.SetVision(player, true);
                opt.RoleOptions.EngineerCooldown = 0;
                opt.RoleOptions.EngineerInVentMaxTime = 0;
            }
            if (Main.Grenaiding)
            {
                if (!player.GetCustomRole().IsImpostorTeam())
                {
                    opt.CrewLightMod = 0f;
                    opt.ImpostorLightMod = 0f;
                }
            }
            else if (Main.ResetVision)
            {
                if (!player.GetCustomRole().IsImpostorTeam())
                {
                    opt.CrewLightMod = Main.RealOptionsData.CrewLightMod;
                    opt.ImpostorLightMod = Main.RealOptionsData.ImpostorLightMod;
                }
            }

            if (player.AmOwner) PlayerControl.GameOptions = opt;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)RpcCalls.SyncSettings, SendOption.Reliable, clientId);
            writer.WriteBytesAndSize(opt.ToBytes(5));
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static TaskState GetPlayerTaskState(this PlayerControl player)
        {
            return PlayerState.taskState[player.PlayerId];
        }

        public static GameOptionsData DeepCopy(this GameOptionsData opt)
        {
            var optByte = opt.ToBytes(5);
            return GameOptionsData.FromBytes(optByte);
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
            if (PlayerControl.GameOptions.MapId == 2) reactorId = 21;

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

            if (PlayerControl.GameOptions.MapId == 4) //Airship用
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
            if (PlayerControl.GameOptions.MapId == 2) reactorId = 21;

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

            if (PlayerControl.GameOptions.MapId == 4) //Airship用
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
                CustomRoles.Mare => Utils.IsActive(SystemTypes.Electrical),
                CustomRoles.FireWorks => FireWorks.CanUseKillButton(pc),
                CustomRoles.Sniper => Sniper.CanUseKillButton(pc),
                CustomRoles.Sheriff => Sheriff.CanUseKillButton(pc),
                CustomRoles.Investigator => Investigator.CanUseKillButton(pc),
                CustomRoles.Arsonist => false,
                CustomRoles.PlagueBearer => true,
                CustomRoles.Pestilence => true,
                CustomRoles.Juggernaut => true,
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
            return pc.GetCustomRole().IsImpostor() &&
                //  Options.CurrentGameMode()!= CustomGameMode.HideAndSeek &&
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
        public static void ResetKillCooldown(this PlayerControl player)
        {
            //if (!player.Is(CustomRoles.Juggernaut))
            Main.AllPlayerKillCooldown[player.PlayerId] = Options.DefaultKillCooldown; //キルクールをデフォルトキルクールに変更
            switch (player.GetCustomRole())
            {
                case CustomRoles.Marksman:
                    Main.AllPlayerKillCooldown[player.PlayerId] = Options.MarksmanKillCooldown.GetFloat();
                    break;
                case CustomRoles.Juggernaut:
                    float DecreasedAmount = Main.JugKillAmounts * Options.JuggerDecrease.GetFloat();
                    Main.AllPlayerKillCooldown[player.PlayerId] = Options.JuggerKillCooldown.GetFloat() - DecreasedAmount;
                    if (Main.AllPlayerKillCooldown[player.PlayerId] < 1)
                        Main.AllPlayerKillCooldown[player.PlayerId] = 1;
                    break;
                case CustomRoles.TheGlitch:
                    if (Main.IsHackMode)
                        Main.AllPlayerKillCooldown[player.PlayerId] = Options.GlitchRoleBlockCooldown.GetFloat();
                    else
                        Main.AllPlayerKillCooldown[player.PlayerId] = Options.GlitchKillCooldown.GetFloat();
                    break;
                case CustomRoles.YingYanger:
                    if (Main.DoingYingYang)
                        Main.AllPlayerKillCooldown[player.PlayerId] = Options.YinYangCooldown.GetFloat();
                    break;
                case CustomRoles.SerialKiller:
                    SerialKiller.ApplyKillCooldown(player.PlayerId); //シリアルキラーはシリアルキラーのキルクールに。
                    break;
                case CustomRoles.TimeThief:
                    TimeThief.SetKillCooldown(player.PlayerId); //タイムシーフはタイムシーフのキルクールに。
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
            if (player.IsLastImpostor())
                Main.AllPlayerKillCooldown[player.PlayerId] = Options.LastImpostorKillCooldown.GetFloat();
            if (Main.KilledDiseased.Contains(player.PlayerId))
                Main.AllPlayerKillCooldown[player.PlayerId] *= Options.DiseasedMultiplier.GetFloat();
        }
        public static void TrapperKilled(this PlayerControl killer, PlayerControl target)
        {
            Logger.Info($"{target?.Data?.PlayerName}はTrapperだった", "Trapper");
            Main.AllPlayerSpeed[killer.PlayerId] = 0.00001f;
            killer.CustomSyncSettings();
            new LateTask(() =>
            {
                Main.AllPlayerSpeed[killer.PlayerId] = Main.RealOptionsData.PlayerSpeedMod;
                killer.CustomSyncSettings();
                RPC.PlaySoundRPC(killer.PlayerId, Sounds.TaskComplete);
            }, Options.TrapperBlockMoveTime.GetFloat(), "Trapper BlockMove");
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
                    if (!killer.inVent && !killer.Data.IsDead)
                    {
                        if (!killer.Is(CustomRoles.Pestilence))
                        {
                            killer.CustomSyncSettings();
                            if (killer.protectedByGuardian)
                                killer.RpcMurderPlayer(killer);
                            killer.RpcMurderPlayer(killer);
                            PlayerState.SetDeathReason(killer.PlayerId, PlayerState.DeathReason.Suicide);
                            Main.whoKilledWho.Add(killer, killer);
                            PlayerState.SetDead(killer.PlayerId);
                        }
                    }
                    else
                    {
                        killer.CustomSyncSettings();
                        RPC.PlaySoundRPC(killer.PlayerId, Sounds.TaskComplete);
                    }
                }
            }, Options.DemoSuicideTime.GetFloat(), "Demolitionist Time");
        }
        public static void VetAlerted(this PlayerControl veteran)
        {
            if (veteran.Is(CustomRoles.Veteran) && !Main.VetIsAlerted)
            {
                Main.VetAlerts++;
                Main.VettedThisRound = true;
                Main.VetIsAlerted = true;
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetVeteranAlert, Hazel.SendOption.Reliable, -1);
                writer.Write(Main.VetAlerts);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                new LateTask(() =>
                {
                    Main.VetIsAlerted = false;
                }, Options.VetDuration.GetFloat(), "Veteran Duration");
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
                    }, Options.VestDuration.GetFloat(), "Survivor Vesting Duration");
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
                CustomRoles.Sidekick or
                CustomRoles.PlagueBearer or
                CustomRoles.Juggernaut or
                CustomRoles.Pestilence or
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
