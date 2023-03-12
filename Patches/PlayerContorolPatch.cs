using System.Security.Authentication.ExtendedProtection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using HarmonyLib;
using Hazel;
using UnityEngine;
using static TownOfHost.Translator;
using Object = UnityEngine.Object;
using AmongUs.GameOptions;
using TownOfHost.PrivateExtensions;

namespace TownOfHost
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckProtect))]
    class CheckProtectPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            if (!AmongUsClient.Instance.AmHost) return false;
            Logger.Info("CheckProtect発生: " + __instance.GetNameWithRole() + "=>" + target.GetNameWithRole(), "CheckProtect");
            if (__instance.Is(CustomRoles.Sheriff))
            {
                if (__instance.Data.IsDead)
                {
                    Logger.Info("守護をブロックしました。", "CheckProtect");
                    return false;
                }
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
    class CheckMurderPatch
    {
        public static Dictionary<byte, float> TimeSinceLastKill = new();
        public static void Update()
        {
            for (byte i = 0; i < 15; i++)
            {
                if (TimeSinceLastKill.ContainsKey(i))
                {
                    TimeSinceLastKill[i] += Time.deltaTime;
                    if (15f < TimeSinceLastKill[i]) TimeSinceLastKill.Remove(i);
                }
            }
        }
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            if (!AmongUsClient.Instance.AmHost) return false;

            var killer = __instance; //読み替え変数

            Logger.Info($"{killer.GetNameWithRole()} => {target.GetNameWithRole()}", "CheckMurder");

            //死人はキルできない
            if (killer.Data.IsDead)
            {
                Logger.Info($"{killer.GetNameWithRole()}は死亡しているためキャンセルされました。", "CheckMurder");
                return false;
            }

            if (target.Data == null || //PlayerDataがnullじゃないか確認
                target.inVent || target.inMovingPlat //targetの状態をチェック
            )
            {
                Logger.Info("targetは現在キルできない状態です。", "CheckMurder");
                return false;
            }
            if (MeetingHud.Instance != null) //会議中でないかの判定
            {
                Logger.Info("会議が始まっていたため、キルをキャンセルしました。", "CheckMurder");
                return false;
            }
            if (Options.TosOptions.GetBool() && Options.AttackDefenseValues.GetBool())
            {
                var flag = killer.AttackIsStronger(target);
                if (!flag)
                {
                    Logger.Msg($"{killer.GetNameWithRole()}, who has an Attack Value of {Utils.GetAttackEnum(killer.GetCustomRole())}, failed to kill {target.GetNameWithRole()}, who has a Defense Value of {Utils.GetDefenseEnum(killer.GetCustomRole())}", $"Attack wasn't Strong Enough");
                    if (Options.KillCooldownResets.GetBool())
                        killer.RpcGuardAndKill(killer);
                    return false;
                }
            }
            float minTime = Mathf.Max(0.02f, AmongUsClient.Instance.Ping / 1000f * 6f);
            if (TimeSinceLastKill.TryGetValue(killer.PlayerId, out var time) && time < minTime)
            {
                Logger.Info("前回のキルからの時間が早すぎるため、キルをブロックしました。", "CheckMurder");
                return false;
            }
            TimeSinceLastKill[killer.PlayerId] = 0f;

            killer.ResetKillCooldown();

            if ((Options.CurrentGameMode() == CustomGameMode.HideAndSeek || Options.IsStandardHAS) && Options.HideAndSeekKillDelayTimer > 0)
            {
                Logger.Info(killer?.Data?.PlayerName + " Tried to kill before timer is up. (HideAndSeek)", "CheckMurder");
                return false;
            }

            if (killer.PlayerId != target.PlayerId)
            {
                switch (killer.GetCustomRole())
                {
                    //==========インポスター役職==========//
                    case CustomRoles.NeutWitch:
                        if (Options.NumOfWitchesPerRound.GetInt() - Main.WitchesThisRound <= 0)
                            return false;
                        break;
                    case CustomRoles.Mafia:
                        if (!killer.CanUseKillButton())
                        {
                            Logger.Info(killer?.Data?.PlayerName + " is Mafia and tried to kll when they can't.", "CheckMurder");
                            return false;
                        }
                        else
                        {
                            Logger.Info(killer?.Data?.PlayerName + " is Mafia and can now kill.", "CheckMurder");
                        }
                        break;
                    case CustomRoles.FireWorks:
                        if (!killer.CanUseKillButton())
                        {
                            return false;
                        }
                        break;
                    case CustomRoles.Sniper:
                        if (!killer.CanUseKillButton())
                        {
                            return false;
                        }
                        break;
                    case CustomRoles.AgiTater:
                        if (AgiTater.BombedThisRound || AgiTater.CurrentBombedPlayer != 255) return false;
                        break;
                    case CustomRoles.Bomber:
                        if (Bomber.CurrentBombedPlayer != 255) return false;
                        break;
                    case CustomRoles.Werewolf:
                        if (!Main.IsRampaged) return false;
                        break;
                    case CustomRoles.Mare:
                        if (!killer.CanUseKillButton())
                            return false;
                        break;
                    case CustomRoles.Crusader:
                        if (Main.HasTarget[killer.PlayerId]) return false;
                        break;
                    case CustomRoles.SKMadmate:
                        return false;

                    case CustomRoles.Sidekick:
                        if (!Main.JackalDied)
                        {
                            if (!Options.SidekickCanKill.GetBool())
                                return false;
                        }
                        break;
                    case CustomRoles.Sheriff:
                        if (!Sheriff.CanUseKillButton(killer))
                            return false;
                        break;
                    case CustomRoles.Investigator:
                        //if (!Investigator.CanUseKillButton(killer))
                        //     return false;
                        break;
                    case CustomRoles.PlagueBearer:
                    case CustomRoles.CorruptedSheriff:
                    case CustomRoles.BloodKnight:
                    case CustomRoles.Jackal:
                    case CustomRoles.Juggernaut:
                    case CustomRoles.Marksman:
                    case CustomRoles.Pestilence:
                        break;
                    case CustomRoles.Swapper:
                    case CustomRoles.Jester:
                    case CustomRoles.Executioner:
                    case CustomRoles.Amnesiac:
                    case CustomRoles.Opportunist:
                        return false;
                    default:
                        break;
                }
            }

            if (__instance.PlayerId == Bomber.CurrentBombedPlayer && Bomber.TargetIsRoleBlocked) return false;

            //キルされた時の特殊判定
            switch (target.GetCustomRole())
            {
                case CustomRoles.SchrodingerCat:
                    var canDirectKill = !killer.GetCustomRole().IsShieldedRole();
                    if (canDirectKill)
                    {
                        if (killer.GetCustomRole().IsCoven()) break;
                        killer.RpcGuardAndKill(target);
                        if (PlayerState.GetDeathReason(target.PlayerId) == PlayerState.DeathReason.Sniped)
                        {
                            target.RpcSetCustomRole(CustomRoles.MSchrodingerCat);
                            var sniperId = Sniper.GetSniper(target.PlayerId);
                            NameColorManager.Instance.RpcAdd(sniperId, target.PlayerId, $"{Utils.GetRoleColorCode(CustomRoles.SchrodingerCat)}");
                        }
                        else if (BountyHunter.GetTarget(killer) == target)
                            BountyHunter.ResetTarget(killer);
                        else
                        {
                            SerialKiller.OnCheckMurder(killer, isKilledSchrodingerCat: true);

                            switch (killer.GetCustomRole())
                            {
                                case CustomRoles.Egoist:
                                    target.RpcSetCustomRole(CustomRoles.EgoSchrodingerCat);
                                    break;
                                case CustomRoles.Sidekick:
                                case CustomRoles.Jackal:
                                    target.RpcSetCustomRole(CustomRoles.JSchrodingerCat);
                                    break;
                                case CustomRoles.BloodKnight:
                                    target.RpcSetCustomRole(CustomRoles.BKSchrodingerCat);
                                    break;
                                case CustomRoles.CrewPostor:
                                    target.RpcSetCustomRole(CustomRoles.CPSchrodingerCat);
                                    break;
                                case CustomRoles.Juggernaut:
                                    target.RpcSetCustomRole(CustomRoles.JugSchrodingerCat);
                                    break;
                                case CustomRoles.Marksman:
                                    target.RpcSetCustomRole(CustomRoles.MMSchrodingerCat);
                                    break;
                                case CustomRoles.Pestilence:
                                    target.RpcSetCustomRole(CustomRoles.PesSchrodingerCat);
                                    break;
                                case CustomRoles.Werewolf:
                                    target.RpcSetCustomRole(CustomRoles.WWSchrodingerCat);
                                    break;
                                case CustomRoles.TheGlitch:
                                    target.RpcSetCustomRole(CustomRoles.TGSchrodingerCat);
                                    break;
                            }
                            switch (killer.GetCustomRole().GetRoleType())
                            {
                                case RoleType.Madmate:
                                case RoleType.Impostor:
                                    target.RpcSetCustomRole(CustomRoles.MSchrodingerCat);
                                    break;
                                case RoleType.Crewmate:
                                    target.RpcSetCustomRole(CustomRoles.CSchrodingerCat);
                                    break;
                            }

                            NameColorManager.Instance.RpcAdd(killer.PlayerId, target.PlayerId, $"{Utils.GetRoleColorCode(CustomRoles.SchrodingerCat)}");
                        }
                        Utils.NotifyRoles();
                        Utils.CustomSyncAllSettings();
                        return false;
                    }
                    break;

                //==========マッドメイト系役職==========//
                case CustomRoles.MadGuardian:
                    if (killer.Is(CustomRoles.Arsonist)
                    ) break;
                    if (killer.Is(CustomRoles.PlagueBearer)
                    ) break;
                    if (killer.Is(CustomRoles.Investigator)
                    ) break;
                    if (killer.Is(CustomRoles.HexMaster) && !killer.IsHexMode()
                    ) break;

                    //MadGuardianを切れるかの判定処理
                    var taskState = target.GetPlayerTaskState();
                    if (taskState.IsTaskFinished)
                    {
                        int dataCountBefore = NameColorManager.Instance.NameColors.Count;
                        NameColorManager.Instance.RpcAdd(killer.PlayerId, target.PlayerId, "#ff0000");
                        if (Options.MadGuardianCanSeeWhoTriedToKill.GetBool())
                            NameColorManager.Instance.RpcAdd(target.PlayerId, killer.PlayerId, "#ff0000");

                        if (dataCountBefore != NameColorManager.Instance.NameColors.Count)
                            Utils.NotifyRoles();
                        return false;
                    }
                    break;
                case CustomRoles.Survivor:
                    var stuff = Main.SurvivorStuff[target.PlayerId];
                    if (stuff.Item2 == true)
                    {
                        killer.RpcGuardAndKill(target);
                        return false;
                    }
                    break;
                case CustomRoles.Phantom:
                    if (!Main.PhantomCanBeKilled)
                    {
                        var pc = target;
                        pc.SetName("");
                        var sender = CustomRpcSender.Create(name: "RpcChoosePhantom");
                        int colorId = pc.CurrentOutfit.ColorId;
                        pc.SetColor(colorId);
                        sender.AutoStartRpc(pc.NetId, (byte)RpcCalls.SetColor)
                            .Write(15)
                            .EndRpc();

                        pc.SetHat("", colorId);
                        sender.AutoStartRpc(pc.NetId, (byte)RpcCalls.SetHatStr)
                            .Write("")
                            .EndRpc();

                        pc.SetSkin("", colorId);
                        sender.AutoStartRpc(pc.NetId, (byte)RpcCalls.SetSkinStr)
                            .Write("")
                            .EndRpc();

                        pc.SetVisor("", colorId);
                        sender.AutoStartRpc(pc.NetId, (byte)RpcCalls.SetVisorStr)
                            .Write("")
                            .EndRpc();

                        pc.SetPet("", colorId);
                        sender.AutoStartRpc(pc.NetId, (byte)RpcCalls.SetPetStr)
                            .Write("")
                            .EndRpc();
                        pc.RpcShapeshiftV2(pc, true);
                        return false;
                    }
                    break;
            }

            if (killer.PlayerId != target.PlayerId)
            {
                if (CustomRoles.TheGlitch.IsEnable() | CustomRoles.Escort.IsEnable() | CustomRoles.Consort.IsEnable())
                {
                    List<byte> hackedPlayers = new();
                    foreach (var cp in Main.CursedPlayers)
                    {
                        if (cp.Value == null) continue;
                        if (Utils.GetPlayerById(cp.Key).GetCustomRole().CanRoleBlock())
                        {
                            hackedPlayers.Add(cp.Value.PlayerId);
                        }
                    }
                    if (hackedPlayers.Contains(killer.PlayerId))
                    {
                        return false;
                    }
                }
                if (killer.GetRoleType() == target.GetRoleType() && killer.GetRoleType() == RoleType.Coven)
                {
                    //they are both coven
                    return false;
                }
                if (Options.CurrentGameMode() == CustomGameMode.Standard)
                    if (target.GetCustomRole().IsJackalTeam() && killer.GetCustomRole().IsJackalTeam())
                    {
                        //they are both Jackal.
                        return false;
                    }
                    else if (killer.SameTeamAsTarget(target))
                    {
                        // they are on same team
                        return false;
                    }
                if (killer.GetCustomRole().IsImpostor() && target.GetCustomRole().IsImpostor())
                {
                    // cannot kill traitor. //
                    return false;
                }
                if (killer.GetCustomRole().IsCoven() && !Main.HasNecronomicon && !killer.Is(CustomRoles.PotionMaster) && !killer.Is(CustomRoles.HexMaster) && !killer.Is(CustomRoles.CovenWitch))
                    return false;
                foreach (var protect in Main.GuardianAngelTarget)
                {
                    if (target.PlayerId == protect.Value && Main.IsProtected)
                    {
                        killer.RpcGuardAndKill(target);
                        return false;
                    }
                }
                if (target.Is(CustomRoles.Pestilence) && !killer.Is(CustomRoles.Vampire) && !killer.Is(CustomRoles.Werewolf) && !killer.Is(CustomRoles.TheGlitch))
                {
                    target.RpcMurderPlayer(killer);
                    return false;
                }
                if (target.Is(CustomRoles.BloodKnight))
                {
                    if (!killer.Is(CustomRoles.Arsonist) && !killer.Is(CustomRoles.PlagueBearer) && !killer.Is(CustomRoles.Investigator) && Main.bkProtected) return false;
                }
                if (target.Is(CustomRoles.CovenWitch) && !Main.WitchProtected)
                {
                    // killer.RpcGuardAndKill(target);
                    if (!killer.Is(CustomRoles.Arsonist))
                    {
                        if (!killer.Is(CustomRoles.PlagueBearer))
                        {
                            if (!killer.Is(CustomRoles.Investigator))
                            {
                                killer.RpcGuardAndKill(killer);
                                Main.AllPlayerKillCooldown[target.PlayerId] = 1f;
                                target.RpcGuardAndKill(target);
                                target.ResetKillCooldown();
                                Main.WitchProtected = true;
                                return false;
                            }
                        }
                    }
                }
                if (!killer.Is(CustomRoles.Pestilence) && !killer.Is(CustomRoles.Werewolf))
                {
                    bool returnFalse = false;
                    if (Main.CurrentTarget.ContainsValue(target.PlayerId))
                    {
                        foreach (var key in Main.CurrentTarget)
                        {
                            if (key.Value != target.PlayerId) continue;
                            var player = Utils.GetPlayerById(key.Key);
                            switch (player.GetCustomRole())
                            {
                                case CustomRoles.Bodyguard:
                                    returnFalse = true;
                                    var TempPosition = player.transform.position;
                                    Utils.TP(player.NetTransform, new Vector2(target.GetTruePosition().x, target.GetTruePosition().y + 0.3636f));
                                    Utils.TP(target.NetTransform, new Vector2(TempPosition.x, TempPosition.y + 0.3636f));
                                    if (!Utils.IsProtectedByCrusader(player))
                                    {
                                        player.RpcMurderPlayer(killer);
                                    }
                                    else
                                    {
                                        PlayerControl protector = Utils.GetProtector(player);
                                        if (protector != null)
                                        {
                                            protector.RpcMurderPlayer(killer);
                                        }
                                        else
                                        {
                                            player.RpcMurderPlayer(killer);
                                        }
                                    }

                                    if (!Utils.IsProtectedByMedic(player))
                                    {
                                        if (!Utils.IsProtectedByCrusader(player))
                                        {
                                            killer.RpcMurderPlayer(player);
                                        }
                                    }

                                    if (Utils.IsProtectedByCrusader(player))
                                    {
                                        PlayerControl protector = Utils.GetProtector(player);
                                        if (protector != null)
                                        {
                                            Main.CurrentTarget[protector.PlayerId] = 255;
                                        }
                                    }

                                    if (player.Data.IsDead)
                                    {
                                        PlayerState.SetDeathReason(player.PlayerId, PlayerState.DeathReason.Suicide);
                                    }
                                    else
                                    {
                                        Main.CurrentTarget[player.PlayerId] = 255;
                                        Main.HasTarget[player.PlayerId] = false;
                                    }

                                    break;
                                case CustomRoles.Medic:
                                    returnFalse = true;
                                    killer.RpcGuardAndKill(killer);
                                    killer.RpcGuardAndKill(target);
                                    break;
                                case CustomRoles.Crusader:
                                    returnFalse = true;
                                    player.RpcMurderPlayer(killer);
                                    Main.CurrentTarget[player.PlayerId] = 255;
                                    Main.HasTarget[player.PlayerId] = false;
                                    break;
                            }
                        }
                    }
                    if (returnFalse) return false;
                }
                switch (killer.GetCustomRole())
                {
                    //==========インポスター役職==========//
                    case CustomRoles.Crusader:
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted && Options.CrewRolesVetted.GetBool())
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        Main.HasTarget[killer.PlayerId] = true;
                        Main.CurrentTarget[killer.PlayerId] = target.PlayerId;
                        killer.RpcGuardAndKill(target);
                        return false;
                    case CustomRoles.Medusa:
                        if (Main.HasNecronomicon)
                        {
                            if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                            {
                                target.RpcMurderPlayer(killer);
                                return false;
                            }
                            break;
                        }
                        else
                        {
                            return false;
                        }
                    case CustomRoles.Painter:
                        if (target.CurrentOutfit.ColorId == killer.CurrentOutfit.ColorId) return false;
                        killer.RpcGuardAndKill(target);
                        target.SetColor(killer.CurrentOutfit.ColorId);
                        killer.ResetKillCooldown();
                        target.RpcShapeshiftV2(target, true);
                        return false;
                    case CustomRoles.Janitor:
                        int startingColorId = Main.AllPlayerSkin[target.PlayerId].Item1;
                        if (target.CurrentOutfit.ColorId == startingColorId) return false;
                        killer.RpcGuardAndKill(target);
                        killer.ResetKillCooldown();
                        target.SetColor(startingColorId);
                        target.RpcRevertShapeshiftV2(true);
                        return false;
                    case CustomRoles.CovenWitch:
                        if (Main.HasNecronomicon)
                        {
                            Main.WitchedList[target.PlayerId] = 0;
                            if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                            {
                                target.RpcMurderPlayer(killer);
                                return false;
                            }
                            break;
                        }
                        else
                        {
                            if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                            {
                                target.RpcMurderPlayer(killer);
                                return false;
                            }
                            Main.WitchedList[target.PlayerId] = killer.PlayerId;
                            Main.AllPlayerKillCooldown[killer.PlayerId] = Options.CovenKillCooldown.GetFloat() * 2;
                            killer.CustomSyncSettings();
                            killer.RpcGuardAndKill(target);
                            return false;
                        }
                        break;
                    case CustomRoles.Cleaner:
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        Main.CleanerCanClean[killer.PlayerId] = false;
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RpcSetCleanerClean, Hazel.SendOption.Reliable, -1);
                        writer.Write(killer.PlayerId);
                        writer.Write(false);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        new LateTask(() =>
                        {
                            Main.CleanerCanClean[killer.PlayerId] = true;
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RpcSetCleanerClean, Hazel.SendOption.Reliable, -1);
                            writer.Write(killer.PlayerId);
                            writer.Write(false);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                        }, Main.AllPlayerKillCooldown[killer.PlayerId], $"Cleaner Can Clean Now {killer.GetNameWithRole()}");
                        break;
                    case CustomRoles.Swooper:
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        if (Main.IsInvis)
                        {
                            killer.RpcGuardAndKill(target);
                            target.RpcMurderPlayer(target);
                            Main.whoKilledWho.Remove(target.Data.PlayerId);
                            Main.whoKilledWho.Add(target.Data.PlayerId, killer.PlayerId);
                            Main.KillCount[killer.PlayerId] += 1;
                            if (target.GetCustomSubRole() == CustomRoles.Bait && killer.PlayerId != target.PlayerId)
                            {
                                Logger.Info(target?.Data?.PlayerName + "はBaitだった", "MurderPlayer");
                                new LateTask(() => killer.CmdReportDeadBody(target.Data), 0.15f, "Bait Self Report");
                            }
                            else
                                //Terrorist
                            if (target.Is(CustomRoles.Terrorist))
                            {
                                Logger.Info(target?.Data?.PlayerName + "はTerroristだった", "MurderPlayer");
                                Utils.CheckTerroristWin(target.Data);
                            }
                            //Child
                            else if (target.Is(CustomRoles.Child))
                            {
                                if (!killer.Is(CustomRoles.Arsonist)) //child doesn't win =(
                                {
                                    Logger.Info(target?.Data?.PlayerName + "はChildだった", "MurderPlayer");
                                    Utils.ChildWin(target.Data);
                                }
                            }
                            if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted && killer.PlayerId != target.PlayerId)
                            {
                                target.RpcMurderPlayer(killer);
                                // return false;
                            }
                            else if (target.GetCustomSubRole() == CustomRoles.Bewilder && killer.PlayerId != target.PlayerId)
                            {
                                Main.KilledBewilder.Add(killer.PlayerId);
                            }
                            else if (target.GetCustomSubRole() == CustomRoles.Diseased && killer.PlayerId != target.PlayerId)
                            {
                                Main.KilledDiseased.Add(killer.PlayerId);
                                killer.ResetKillCooldown();
                            }

                            if (target.Is(CustomRoles.Trapper) && !killer.Is(CustomRoles.Trapper))
                                killer.TrapperKilled(target);
                            if (target.Is(CustomRoles.Demolitionist) && !killer.Is(CustomRoles.Demolitionist))
                                killer.DemoKilled(target);
                            return false;
                        }
                        break;
                    case CustomRoles.Escort:
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted && Options.CrewRolesVetted.GetBool())
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        if (target.GetCustomRole().CanRoleBlock())
                        {
                            killer.RpcGuardAndKill(target);
                            killer.RpcGuardAndKill(killer);
                            return false;
                        }
                        if (Options.TosOptions.GetBool() && Options.SKkillsRoleblockers.GetBool() && target.Is(CustomRoles.Jackal))
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        Utils.CustomSyncAllSettings();
                        Main.CursedPlayers[killer.PlayerId] = target;
                        Main.WarlockTimer.Add(killer.PlayerId, 0f);
                        Main.isCurseAndKill[killer.PlayerId] = true;
                        killer.RpcGuardAndKill(target);
                        new LateTask(() =>
                        {
                            Main.CursedPlayers[killer.PlayerId] = null;
                            Main.isCurseAndKill[killer.PlayerId] = false;
                        }, Options.GlobalRoleBlockDuration.GetFloat(), "Consort Hack");
                        return false;
                    case CustomRoles.Consort:
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted && Options.CrewRolesVetted.GetBool())
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        if (!Main.CheckShapeshift[killer.PlayerId])
                        {
                            if (target.GetCustomRole().CanRoleBlock())
                            {
                                killer.RpcGuardAndKill(target);
                                killer.RpcGuardAndKill(killer);
                                return false;
                            }
                            if (Options.TosOptions.GetBool() && Options.SKkillsRoleblockers.GetBool() && target.Is(CustomRoles.Jackal))
                            {
                                target.RpcMurderPlayer(killer);
                                return false;
                            }
                            Main.AllPlayerKillCooldown[killer.PlayerId] = killer.GetKillCooldown() * 2;
                            Utils.CustomSyncAllSettings();
                            Main.CursedPlayers[killer.PlayerId] = target;
                            Main.WarlockTimer.Add(killer.PlayerId, 0f);
                            Main.isCurseAndKill[killer.PlayerId] = true;
                            killer.RpcGuardAndKill(target);
                            new LateTask(() =>
                            {
                                Main.CursedPlayers[killer.PlayerId] = null;
                                Main.isCurseAndKill[killer.PlayerId] = false;
                            }, Options.GlobalRoleBlockDuration.GetFloat(), "Escort Hack");
                            return false;
                        }
                        break;
                    case CustomRoles.Hitman:
                    case CustomRoles.Sidekick:
                    case CustomRoles.Jackal:
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        if (Options.FreeForAllOn.GetBool() && Options.CurrentGameMode() == CustomGameMode.HideAndSeek)
                            if (target.CurrentOutfit.ColorId == killer.CurrentOutfit.ColorId) return false;
                        break;
                    case CustomRoles.Manipulator:
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        Manipulator.killedList.Add(target.Data.PlayerId);
                        break;
                    case CustomRoles.Bomber:
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        Main.AllPlayerKillCooldown[killer.PlayerId] = 10f;
                        Utils.CustomSyncAllSettings();
                        if (Bomber.CurrentDouseTarget != target.PlayerId && !Bomber.BomberTimer.ContainsKey(killer.PlayerId))
                        {
                            Bomber.CurrentBombedPlayer = 255;
                            Bomber.CurrentDouseTarget = target.PlayerId;
                            Bomber.BomberTimer.Add(killer.PlayerId, (target, 0f));
                            Utils.NotifyRoles(SpecifySeer: __instance);
                            Bomber.SendRPC(target.PlayerId, 255);
                        }
                        return false;
                    case CustomRoles.TheGlitch:
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted && !Main.IsHackMode)
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        if (Main.IsHackMode && Main.CursedPlayers[killer.PlayerId] == null)
                        { //Warlockが変身時以外にキルしたら、呪われる処理
                            if (target.GetCustomRole().CanRoleBlock())
                            {
                                killer.RpcGuardAndKill(target);
                                killer.RpcGuardAndKill(killer);
                                return false;
                            }
                            if (Options.TosOptions.GetBool() && Options.SKkillsRoleblockers.GetBool() && target.Is(CustomRoles.Jackal))
                            {
                                target.RpcMurderPlayer(killer);
                                return false;
                            }
                            Utils.CustomSyncAllSettings();
                            Main.CursedPlayers[killer.PlayerId] = target;
                            Main.WarlockTimer.Add(killer.PlayerId, 0f);
                            Main.isCurseAndKill[killer.PlayerId] = true;
                            killer.RpcGuardAndKill(target);
                            new LateTask(() =>
                            {
                                Main.CursedPlayers[killer.PlayerId] = null;
                                Main.isCurseAndKill[killer.PlayerId] = false;
                            }, Options.GlobalRoleBlockDuration.GetFloat(), "Glitch Hacking");
                            return false;
                        }
                        if (!Main.IsHackMode)
                        {
                            if (target.Is(CustomRoles.Pestilence))
                            {
                                target.RpcMurderPlayer(killer);
                                return false;
                            }
                            killer.RpcMurderPlayer(target);
                            return false;
                        }
                        if (Main.isCurseAndKill[killer.PlayerId]) killer.RpcGuardAndKill(target);
                        return false;
                    //break;
                    case CustomRoles.Pestilence:
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                        {
                            switch (Options.PestiAttacksVet.GetSelection())
                            {
                                case 0:
                                    target.RpcMurderPlayer(killer);
                                    break;
                                case 1:
                                    killer.RpcMurderPlayer(target);
                                    target.RpcMurderPlayer(killer);
                                    break;
                                case 2:
                                    killer.RpcMurderPlayer(target);
                                    break;
                            }
                            return false;
                        }
                        break;
                    case CustomRoles.Ninja:
                        Ninja.KillCheck(killer, target);
                        return false;
                    case CustomRoles.Necromancer:
                        Necromancer.OnCheckMurder(killer, target);
                        return false;
                    case CustomRoles.Werewolf:
                        if (Main.IsRampaged)
                        {
                            if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                            {
                                target.RpcMurderPlayer(killer);
                                return false;
                            }
                            if (target.Is(CustomRoles.Pestilence))
                            {
                                target.RpcMurderPlayer(killer);
                                return false;
                            }
                            if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                            {
                                target.RpcMurderPlayer(killer);
                                new LateTask(() =>
                                {
                                    Main.unreportableBodies.Add(killer.PlayerId);
                                }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                                return false;
                            }
                            bool returnFalse = false;
                            if (Main.CurrentTarget.ContainsValue(target.PlayerId))
                            {
                                foreach (var key in Main.CurrentTarget)
                                {
                                    var player = Utils.GetPlayerById(key.Key);
                                    switch (player.GetCustomRole())
                                    {
                                        case CustomRoles.Bodyguard:
                                            returnFalse = true;
                                            var TempPosition = player.transform.position;
                                            Utils.TP(player.NetTransform, new Vector2(target.GetTruePosition().x, target.GetTruePosition().y + 0.3636f));
                                            Utils.TP(target.NetTransform, new Vector2(TempPosition.x, TempPosition.y + 0.3636f));
                                            player.RpcMurderPlayer(killer);
                                            killer.RpcMurderPlayer(player);
                                            Main.CurrentTarget.Remove(player.PlayerId);
                                            break;
                                        case CustomRoles.Medic:
                                            returnFalse = true;
                                            killer.RpcGuardAndKill(killer);
                                            killer.RpcGuardAndKill(target);
                                            break;
                                        case CustomRoles.Crusader:
                                            returnFalse = true;
                                            player.RpcMurderPlayer(killer);
                                            Main.CurrentTarget[player.PlayerId] = 255;
                                            break;
                                    }
                                }
                            }
                            if (returnFalse) return false;
                            //  killer.RpcMurderPlayer(target);
                            // return false;
                        }
                        else
                        {
                            return false;
                        }
                        break;
                    case CustomRoles.Marksman:
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        if (Main.MarksmanKills != 2)
                            Main.MarksmanKills++;
                        killer.CustomSyncSettings();
                        break;
                    case CustomRoles.AgiTater:
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        killer.RpcGuardAndKill(target);
                        AgiTater.BombedThisRound = true;
                        AgiTater.CurrentBombedPlayer = killer.PlayerId;
                        AgiTater.PassBomb(killer, target, true);
                        return false;
                    case CustomRoles.Juggernaut:
                        //calculating next kill cooldown
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        Main.JugKillAmounts++;
                        float DecreasedAmount = Main.JugKillAmounts * Options.JuggerDecrease.GetFloat();
                        Main.AllPlayerKillCooldown[killer.PlayerId] = Options.JuggerKillCooldown.GetFloat() - DecreasedAmount;
                        if (Main.AllPlayerKillCooldown[killer.PlayerId] < 1)
                            Main.AllPlayerKillCooldown[killer.PlayerId] = 1;
                        //after calculating make the kill happen ?
                        killer.CustomSyncSettings();
                        killer.RpcMurderPlayer(target);
                        return false;
                        break;
                    case CustomRoles.BountyHunter: //キルが発生する前にここの処理をしないとバグる
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        if (Main.CheckShapeshift[killer.PlayerId]) return false;
                        BountyHunter.OnCheckMurder(killer, target);
                        break;
                    case CustomRoles.SerialKiller:
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        SerialKiller.OnCheckMurder(killer);
                        break;
                    case CustomRoles.Vampress:
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        if (!Main.CheckShapeshift[killer.PlayerId])
                            if (target.GetCustomSubRole() != CustomRoles.Bait)
                            {
                                Utils.CustomSyncAllSettings();
                                Main.AllPlayerKillCooldown[killer.PlayerId] = Options.DefaultKillCooldown * 2;
                                killer.CustomSyncSettings();
                                killer.RpcGuardAndKill(target);
                                Main.BitPlayers.Add(target.PlayerId, (killer.PlayerId, 0f));
                                return false;
                            }
                            else
                            {
                                if (Options.VampireBuff.GetBool()) //Vampire Buff will still make Vampire report but later.
                                {
                                    Utils.CustomSyncAllSettings();
                                    Main.AllPlayerKillCooldown[killer.PlayerId] = Options.DefaultKillCooldown * 2;
                                    killer.CustomSyncSettings();
                                    killer.RpcGuardAndKill(target);
                                    Main.BitPlayers.Add(target.PlayerId, (killer.PlayerId, 0f));
                                    return false;
                                }
                            }
                        break;
                    case CustomRoles.Poisoner:
                    case CustomRoles.PoisonMaster:
                    case CustomRoles.Vampire:
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        if (target.GetCustomSubRole() != CustomRoles.Bait)
                        { //キルキャンセル&自爆処理
                          //if (!target.Is(CustomRoles.Bewilder))
                          // {
                            Utils.CustomSyncAllSettings();
                            Main.AllPlayerKillCooldown[killer.PlayerId] = Options.DefaultKillCooldown * 2;
                            killer.CustomSyncSettings(); //負荷軽減のため、killerだけがCustomSyncSettingsを実行
                            killer.RpcGuardAndKill(target);
                            Main.BitPlayers.Add(target.PlayerId, (killer.PlayerId, 0f));
                            return false;
                            //  }
                        }
                        else
                        {
                            if (Options.VampireBuff.GetBool()) //Vampire Buff will still make Vampire report but later.
                            {
                                Utils.CustomSyncAllSettings();
                                Main.AllPlayerKillCooldown[killer.PlayerId] = Options.DefaultKillCooldown * 2;
                                killer.CustomSyncSettings(); //負荷軽減のため、killerだけがCustomSyncSettingsを実行
                                killer.RpcGuardAndKill(target);
                                Main.BitPlayers.Add(target.PlayerId, (killer.PlayerId, 0f));
                                return false;
                            }
                        }
                        break;
                    case CustomRoles.Warlock:
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        if (!Main.CheckShapeshift[killer.PlayerId] && !Main.isCurseAndKill[killer.PlayerId])
                        { //Warlockが変身時以外にキルしたら、呪われる処理
                            Main.isCursed = true;
                            Utils.CustomSyncAllSettings();
                            killer.RpcGuardAndKill(target);
                            Main.CursedPlayers[killer.PlayerId] = target;
                            Main.WarlockTimer.Add(killer.PlayerId, 0f);
                            Main.isCurseAndKill[killer.PlayerId] = true;
                            return false;
                        }
                        if (Main.CheckShapeshift[killer.PlayerId])
                        {//呪われてる人がいないくて変身してるときに通常キルになる
                            killer.RpcMurderPlayer(target);
                            killer.RpcGuardAndKill(target);
                            return false;
                        }
                        if (Main.isCurseAndKill[killer.PlayerId]) killer.RpcGuardAndKill(target);
                        return false;
                    case CustomRoles.Silencer:
                        //Silenced Player
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        if (Main.SilencedPlayer.Count > 0)
                        {
                            killer.RpcMurderPlayer(target);
                            return false;
                        }
                        else if (Main.SilencedPlayer.Count <= 0)
                        {
                            killer.RpcGuardAndKill(target);
                            Main.SilencedPlayer.Add(target);
                            RPC.RpcDoSilence(target.PlayerId);
                            break;
                        }
                        if (!Main.firstKill.Contains(killer.PlayerId) && !Main.SilencedPlayer.Contains(target)) return false;
                        break;
                    case CustomRoles.Witch:
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        if (killer.IsSpellMode() && !Main.SpelledPlayer.Contains(target))
                        {
                            killer.RpcGuardAndKill(target);
                            Main.SpelledPlayer.Add(target);
                            RPC.RpcDoSpell(target.PlayerId);
                        }
                        Main.KillOrSpell[killer.PlayerId] = !killer.IsSpellMode();
                        Utils.NotifyRoles();
                        killer.SyncKillOrSpell();
                        if (!killer.IsSpellMode()) return false;
                        break;
                    case CustomRoles.HexMaster:
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted && Main.HexesThisRound != Options.MaxHexesPerRound.GetFloat())
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        Main.AllPlayerKillCooldown[killer.PlayerId] = 10f;
                        Utils.CustomSyncAllSettings();
                        if (!Main.isHexed[(killer.PlayerId, target.PlayerId)] && killer.IsHexMode() && Main.HexesThisRound != Options.MaxHexesPerRound.GetFloat())
                        {
                            killer.RpcGuardAndKill(target);
                            Main.HexesThisRound++;
                            Utils.NotifyRoles(SpecifySeer: __instance);
                            Main.isHexed[(killer.PlayerId, target.PlayerId)] = true;//塗り完了
                            killer.RpcSetHexedPlayer(target, true);
                            //RPC.SetCurrentDousingTarget(killer.PlayerId, target.PlayerId);
                        }
                        if (Main.HexesThisRound != Options.MaxHexesPerRound.GetFloat())
                            Main.KillOrSpell[killer.PlayerId] = !killer.IsHexMode();
                        Utils.NotifyRoles();
                        killer.SyncKillOrHex();
                        if (!killer.IsHexMode()) return false;
                        //return false;
                        if (!Main.HasNecronomicon && Main.HexesThisRound == Options.MaxHexesPerRound.GetFloat()) return false;
                        break;
                    case CustomRoles.Puppeteer:
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        Main.PuppeteerList[target.PlayerId] = killer.PlayerId;
                        Main.AllPlayerKillCooldown[killer.PlayerId] = Options.DefaultKillCooldown * 2;
                        killer.CustomSyncSettings(); //負荷軽減のため、killerだけがCustomSyncSettingsを実行
                        killer.RpcGuardAndKill(target);
                        return false;
                    case CustomRoles.IdentityTheft:
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        Main.IsShapeShifted.Add(killer.PlayerId);
                        if (!killer.Data.IsDead)
                            killer.RpcShapeshift(target, true);
                        new LateTask(() =>
                        {
                            if (!GameStates.IsMeeting && Main.IsShapeShifted.Contains(killer.PlayerId))
                            {
                                if (!killer.Data.IsDead)
                                    killer.RpcRevertShapeshift(true);
                                Main.IsShapeShifted.Remove(killer.PlayerId);
                            }
                        }, Main.AllPlayerKillCooldown[killer.PlayerId], "Identity Theft (Unshift Timer)", true);
                        break;
                    case CustomRoles.NeutWitch:
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        Main.WitchList[target.PlayerId] = killer.PlayerId;
                        Main.AllPlayerKillCooldown[killer.PlayerId] = Options.ControlCooldown.GetFloat() * 2f;
                        killer.CustomSyncSettings(); // sync settings so they see.
                        killer.RpcGuardAndKill(target);
                        Main.WitchesThisRound++;
                        MessageWriter writere = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetNumOfWitchesRemaining, Hazel.SendOption.Reliable, -1);
                        writere.Write(Main.WitchesThisRound);
                        AmongUsClient.Instance.FinishRpcImmediately(writere);
                        return false;
                    case CustomRoles.TimeThief:
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        TimeThief.OnCheckMurder(killer);
                        break;
                    case CustomRoles.VoteStealer:
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        Main.KillCount[killer.PlayerId]++;
                        if (Main.KillCount[killer.PlayerId] == Options.KillsForVote.GetInt())
                        {
                            Main.MayorUsedButtonCount[killer.PlayerId] += Options.VoteAmtOnCompletion.GetInt();
                            Main.KillCount[killer.PlayerId] = 0;
                        }
                        Logger.Msg($"Kills Until Next Vote:{Main.KillCount[killer.PlayerId]} - Votes: {Main.MayorUsedButtonCount[killer.PlayerId]}", $"Pickpocket Progress Check");
                        MessageWriter writeree = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RpcSetPickpocketProgress, Hazel.SendOption.Reliable, -1);
                        writeree.Write(killer.PlayerId);
                        writeree.Write(Main.KillCount[killer.PlayerId]);
                        AmongUsClient.Instance.FinishRpcImmediately(writeree);
                        break;
                    case CustomRoles.YingYanger:
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        if (Main.DoingYingYang)
                        {
                            if (Main.ColliderPlayers.Count != 2)
                            {
                                if (Main.ColliderPlayers.Contains(target.PlayerId)) return false;
                                Main.ColliderPlayers.Add(target.PlayerId);
                                killer.RpcGuardAndKill(target);
                                return false;
                            }
                            else
                            {
                                Main.DoingYingYang = false;
                                killer.ResetKillCooldown();
                            }
                        }
                        break;

                    //==========マッドメイト系役職==========//

                    //==========第三陣営役職==========//
                    case CustomRoles.Arsonist:
                        if (Options.TOuRArso.GetBool())
                        {
                            List<PlayerControl> doused = new List<PlayerControl>();
                            foreach (var player in PlayerControl.AllPlayerControls)
                            {
                                if (player == null ||
                                    player.Data.IsDead ||
                                    player.Data.Disconnected ||
                                    player.Is(CustomRoles.Phantom) ||
                                    player.Is(CustomRoles.Pestilence) ||
                                    player.PlayerId == killer.PlayerId
                                ) continue;

                                if (killer.IsDousedPlayer(player))
                                    doused.Add(player);
                            }
                            if (doused.Count >= Options.MaxDousedAtOnce.GetInt())
                                return false;
                        }
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        Main.AllPlayerKillCooldown[killer.PlayerId] = 10f;
                        Utils.CustomSyncAllSettings();
                        if (!Main.isDoused[(killer.PlayerId, target.PlayerId)] && !Main.ArsonistTimer.ContainsKey(killer.PlayerId))
                        {
                            Main.ArsonistTimer.Add(killer.PlayerId, (target, 0f));
                            Utils.NotifyRoles(SpecifySeer: __instance);
                            RPC.SetCurrentDousingTarget(killer.PlayerId, target.PlayerId);
                        }
                        return false;

                    //==========クルー役職==========//
                    case CustomRoles.PlagueBearer:
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        Main.AllPlayerKillCooldown[killer.PlayerId] = 10f;
                        Utils.CustomSyncAllSettings();
                        if (!Main.isInfected[(killer.PlayerId, target.PlayerId)] && !Main.PlagueBearerTimer.ContainsKey(killer.PlayerId))
                        {
                            Main.PlagueBearerTimer.Add(killer.PlayerId, (target, 0f));
                            Utils.NotifyRoles(SpecifySeer: __instance);
                            RPC.SetCurrentInfectingTarget(killer.PlayerId, target.PlayerId);
                            //Main.isInfected[(target.PlayerId, target.PlayerId)] = true;
                            //killer.RpcGuardAndKill(target);
                        }
                        return false;
                    case CustomRoles.Sheriff:
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted && Options.CrewRolesVetted.GetBool())
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        if (!Sheriff.NoDeathPenalty.GetBool())
                        {
                            Sheriff.OnCheckMurder(killer, target, Process: "RemoveShotLimit");


                            if (!Sheriff.OnCheckMurder(killer, target, Process: "Suicide"))
                                return false;
                        }
                        else
                        {
                            Sheriff.OnCheckMurder(killer, target, Process: "RemoveShotLimit");
                        }
                        break;
                    case CustomRoles.Investigator:
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted && Options.CrewRolesVetted.GetBool())
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        // if (Investigator.hasSeered[target.PlayerId]) if (!target.Is(CustomRoles.CorruptedSheriff)) return false;
                        //Investigator.OnCheckMurder(killer, target, Process: "RemoveShotLimit");
                        Investigator.hasSeered[target.PlayerId] = true;
                        if (target.Is(CustomRoles.CorruptedSheriff))
                            Investigator.SeeredCSheriff = true;
                        Logger.Info($"{killer.GetNameWithRole()} : Investigated Player: {target.GetNameWithRole()}", "Investigated");
                        killer.RpcGuardAndKill(target);
                        Utils.CustomSyncAllSettings();
                        Utils.NotifyRoles();
                        MessageWriter writereee = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SeeredPlayer, Hazel.SendOption.Reliable, -1);
                        writereee.Write(target.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writereee);
                        return false;
                    case CustomRoles.BloodKnight:

                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        if (!Main.bkProtected)
                        {
                            Main.bkProtected = true;
                            new LateTask(() =>
                            {
                                Main.bkProtected = false;
                            }, Options.BKprotectDur.GetFloat(), "Blood Knight Duration");
                        }
                        break;
                    default:
                        if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                        {
                            target.RpcMurderPlayer(killer);
                            return false;
                        }
                        if (target.Is(CustomRoles.Medusa) && Main.IsGazing)
                        {
                            target.RpcMurderPlayer(killer);
                            new LateTask(() =>
                            {
                                Main.unreportableBodies.Add(killer.PlayerId);
                            }, Options.StoneReport.GetFloat(), "Medusa Stone Gazing");
                            return false;
                        }
                        break;
                }
            }
            Sheriff.SwitchToCorrupt(killer, target);

            //==キル処理==D
            if (!killer.Is(CustomRoles.Silencer) && !killer.Is(CustomRoles.Painter))
            {
                if (!target.Is(CustomRoles.Pestilence))
                    killer.RpcMurderPlayer(target);
                else if (killer.Is(CustomRoles.Arsonist))
                {
                    killer.RpcMurderPlayer(target);
                }
                else
                    target.RpcMurderPlayer(killer);
            }
            //============

            return false;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ProtectPlayer))]
    class ProtectPlayerPatch
    {
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            if (!target.Data.IsDead || !AmongUsClient.Instance.AmHost) return;

            PlayerControl killer = __instance;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    class MurderPlayerPatch
    {
        public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            Logger.Info($"{__instance.GetNameWithRole()} => {target.GetNameWithRole()}{(target.protectedByGuardian ? "(Protected)" : "")}", "MurderPlayer");
        }
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            if (!target.Data.IsDead || !AmongUsClient.Instance.AmHost) return;

            PlayerControl killer = __instance; //読み替え変数
            Main.DeadPlayersThisRound.Add(target.PlayerId);
            if (PlayerState.GetDeathReason(target.PlayerId) == PlayerState.DeathReason.Sniped)
            {
                killer = Utils.GetPlayerById(Sniper.GetSniper(target.PlayerId));
            }
            if (PlayerState.GetDeathReason(target.PlayerId) == PlayerState.DeathReason.etc)
            {
                //死因が設定されていない場合は死亡判定
                PlayerState.SetDeathReason(target.PlayerId, PlayerState.DeathReason.Kill);
            }

            //When Bait is killed
            if (target.GetCustomSubRole() == CustomRoles.Bait && killer.PlayerId != target.PlayerId)
            {
                Logger.Info(target?.Data?.PlayerName + "はBaitだった", "MurderPlayer");
                new LateTask(() => killer.CmdReportDeadBody(target.Data), 0.15f, "Bait Self Report");
            }
            else
            //Terrorist
            if (target.Is(CustomRoles.Terrorist))
            {
                Logger.Info(target?.Data?.PlayerName + "はTerroristだった", "MurderPlayer");
                Utils.CheckTerroristWin(target.Data);
            }
            //Child
            else if (target.Is(CustomRoles.Child))
            {
                if (!killer.Is(CustomRoles.Arsonist)) //child doesn't win =(
                {
                    Logger.Info(target?.Data?.PlayerName + "はChildだった", "MurderPlayer");
                    Utils.ChildWin(target.Data);
                }
            }
            else if (target.Is(CustomRoles.Jackal))
            {
                Main.JackalDied = true;
                if (Options.SidekickGetsPromoted.GetBool())
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc.Is(CustomRoles.Sidekick))
                            pc.RpcSetCustomRole(CustomRoles.Jackal);
                    }
                }
            }
            else if (target.Is(CustomRoles.Investigator) && killer.PlayerId != target.PlayerId)
            {
                Investigator.hasSeered = new();
            }
            // Last Impostor
            else if (target.GetCustomRole().IsImpostor())
            {
                //bool LocalPlayerKnowsImpostor = false;
                if (Options.SheriffCorrupted.GetBool())
                {
                    if (!Sheriff.csheriff)
                    {
                        int IsAlive = 0;
                        int numCovenAlive = 0;
                        int numImpsAlive = 0;
                        int numNKalive = 0;
                        List<PlayerControl> couldBeTraitors = new();
                        List<byte> couldBeTraitorsid = new();
                        var rando = new System.Random();
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (!pc.Data.Disconnected)
                                if (!pc.Data.IsDead)
                                {
                                    IsAlive++;
                                    if (pc.GetCustomRole().IsNeutralKilling() && !Options.TraitorCanSpawnIfNK.GetBool())
                                        numNKalive++;
                                    if (pc.GetCustomRole().IsCoven() && !Options.TraitorCanSpawnIfCoven.GetBool())
                                        numCovenAlive++;
                                    if (pc.Is(CustomRoles.Sheriff) || pc.Is(CustomRoles.Investigator) || pc.Is(CustomRoles.Hitman))
                                        couldBeTraitors.Add(pc);
                                    if (pc.Is(CustomRoles.Sheriff) || pc.Is(CustomRoles.Investigator) || pc.Is(CustomRoles.Hitman))
                                        couldBeTraitorsid.Add(pc.PlayerId);
                                    if (pc.GetCustomRole().IsImpostor())
                                        numImpsAlive++;
                                }
                        }

                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (!pc.Data.Disconnected)
                                if (!pc.Data.IsDead)
                                {
                                    if (!pc.IsModClient()) continue;
                                    if (!pc.GetCustomRole().IsCrewmate()) continue;
                                    if (!couldBeTraitorsid.Contains(pc.PlayerId))
                                    {
                                        couldBeTraitors.Add(pc);
                                        couldBeTraitorsid.Add(pc.PlayerId);
                                    }
                                }
                        }

                        Sheriff.seer = couldBeTraitors[rando.Next(0, couldBeTraitors.Count)];

                        //foreach (var pva in __instance.playerStates)
                        if (IsAlive >= Options.PlayersForTraitor.GetFloat() && Sheriff.seer != null)
                        {
                            if (numCovenAlive == 0 && numNKalive == 0 && numCovenAlive == 0 && numImpsAlive == 0)
                            {
                                Sheriff.seer.RpcSetCustomRole(CustomRoles.CorruptedSheriff);
                                Sheriff.seer.CustomSyncSettings();
                                Sheriff.csheriff = true;
                                RPC.SetTraitor(Sheriff.seer.PlayerId);
                            }
                        }
                    }
                }
            }
            else
            if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted && killer.PlayerId != target.PlayerId)
            {
                target.RpcMurderPlayer(killer);
                // return false;
            }
            else if (target.GetCustomSubRole() == CustomRoles.Bewilder && killer.PlayerId != target.PlayerId)
            {
                Main.KilledBewilder.Add(killer.PlayerId);
            }
            else if (target.GetCustomSubRole() == CustomRoles.Diseased && killer.PlayerId != target.PlayerId)
            {
                Main.KilledDiseased.Add(killer.PlayerId);
                killer.ResetKillCooldown();
            }

            if (target.Is(CustomRoles.Trapper) && !killer.Is(CustomRoles.Trapper))
                killer.TrapperKilled(target);
            if (target.Is(CustomRoles.Demolitionist) && !killer.Is(CustomRoles.Demolitionist))
                killer.DemoKilled(target);
            if (Main.ExecutionerTarget.ContainsValue(target.PlayerId) && Main.ExeCanChangeRoles)
            {
                List<byte> RemoveExecutionerKey = new();
                foreach (var ExecutionerTarget in Main.ExecutionerTarget)
                {
                    var executioner = Utils.GetPlayerById(ExecutionerTarget.Key);
                    if (executioner == null) continue;
                    if (target.PlayerId == ExecutionerTarget.Value && !executioner.Data.IsDead && !executioner.Is(CustomRoles.Swapper))
                    {
                        executioner.RpcSetCustomRole(Options.CRoleExecutionerChangeRoles[Options.ExecutionerChangeRolesAfterTargetKilled.GetSelection()]); //対象がキルされたらオプションで設定した役職にする
                        RemoveExecutionerKey.Add(ExecutionerTarget.Key);
                    }
                }
                foreach (var RemoveKey in RemoveExecutionerKey)
                {
                    Main.ExecutionerTarget.Remove(RemoveKey);
                    RPC.RemoveExecutionerKey(RemoveKey);
                }
            }
            if (target.Is(CustomRoles.Executioner) && Main.ExecutionerTarget.ContainsKey(target.PlayerId))
            {
                Main.ExecutionerTarget.Remove(target.PlayerId);
                RPC.RemoveExecutionerKey(target.PlayerId);
            }

            if (Main.CurrentTarget.ContainsKey(target.PlayerId) && target.Is(CustomRoles.Oracle))
            {
                if (Main.CurrentTarget[target.PlayerId] != 255)
                {
                    Main.rolesRevealedNextMeeting.Add(Main.CurrentTarget[target.PlayerId]);
                    RPC.RpcAddOracleTarget(Main.CurrentTarget[target.PlayerId]);
                }
                Main.CurrentTarget.Remove(target.PlayerId);
            }
            if (Main.CurrentTarget.ContainsKey(target.PlayerId) && target.Is(CustomRoles.Bodyguard))
            {
                Main.CurrentTarget.Remove(target.PlayerId);
            }
            if (Main.CurrentTarget.ContainsKey(target.PlayerId) && target.Is(CustomRoles.Medic))
            {
                Main.CurrentTarget.Remove(target.PlayerId);
            }
            if (Main.CurrentTarget.ContainsKey(target.PlayerId) && target.Is(CustomRoles.Crusader))
            {
                Main.CurrentTarget.Remove(target.PlayerId);
            }
            if (Main.CurrentTarget.ContainsValue(target.PlayerId))
            {
                List<byte> RemoveTargetKey = new();
                foreach (var TargetKey in Main.CurrentTarget)
                {
                    var player = Utils.GetPlayerById(TargetKey.Key);
                    if (player == null) continue;
                    if (target.PlayerId == TargetKey.Value)
                    {
                        RemoveTargetKey.Add(TargetKey.Key);
                    }
                }
                foreach (var RemoveKey in RemoveTargetKey)
                {
                    Main.CurrentTarget.Remove(RemoveKey);
                }
            }
            if (target.Is(CustomRoles.Freezer))
            {
                if (Main.currentFreezingTarget != 255)
                {
                    var ftarget = Utils.GetPlayerById(Main.currentFreezingTarget);
                    Logger.Info($"{ftarget.Data.PlayerName} was unfrozen", "Freezer");
                    Main.AllPlayerSpeed[ftarget.PlayerId] = Main.RealOptionsData.AsNormalOptions()!.PlayerSpeedMod;
                    ftarget.CustomSyncSettings();
                    RPC.PlaySoundRPC(ftarget.PlayerId, Sounds.TaskComplete);
                }
                Main.currentFreezingTarget = 255;
            }
            if (Main.GuardianAngelTarget.ContainsValue(target.PlayerId))
            {
                List<byte> RemoveGAKey = new();
                foreach (var gaTarget in Main.GuardianAngelTarget)
                {
                    var ga = Utils.GetPlayerById(gaTarget.Key);
                    if (ga == null) continue;
                    if (target.PlayerId == gaTarget.Value && !ga.Data.IsDead)
                    {
                        // CRoleGuardianAngelChangeRoles
                        if (ga.IsModClient())
                            ga.RpcSetCustomRole(Options.CRoleGuardianAngelChangeRoles[Options.WhenGaTargetDies.GetSelection()]); //対象がキルされたらオプションで設定した役職にする
                        else
                        {
                            if (Options.CRoleGuardianAngelChangeRoles[Options.WhenGaTargetDies.GetSelection()] != CustomRoles.Amnesiac)
                                ga.RpcSetCustomRole(Options.CRoleGuardianAngelChangeRoles[Options.WhenGaTargetDies.GetSelection()]); //対象がキルされたらオプションで設定した役職にする
                            else
                                ga.RpcSetCustomRole(Options.CRoleGuardianAngelChangeRoles[2]);
                        }
                        RemoveGAKey.Add(gaTarget.Key);
                    }
                }
                foreach (var RemoveKey in RemoveGAKey)
                {
                    Main.GuardianAngelTarget.Remove(RemoveKey);
                    RPC.RemoveGAKey(RemoveKey);
                }
            }
            // if (target.Is(CustomRoles.GuardianAngelTOU) && Main.GuardianAngelTarget.ContainsKey(target.PlayerId))
            //  {
            //     Main.GuardianAngelTarget.Remove(target.PlayerId);
            ///     RPC.RemoveGAKey(target.PlayerId);
            //  }
            if (target.Is(CustomRoles.TimeThief))
                target.ResetVotingTime();

            if (Postman.IsEnable())
            {
                if (Postman.target != null)
                {
                    Postman.CheckForTarget(target);
                }
                if (Postman.hasDelivered.ContainsKey(target.PlayerId))
                {
                    Postman.Remove(target.PlayerId);
                }
            }

            if (target.PlayerId == AgiTater.CurrentBombedPlayer && AgiTater.IsEnable())
                AgiTater.ResetBomb(false);

            if (Main.ColliderPlayers.Contains(target.PlayerId) && CustomRoles.YingYanger.IsEnable() && Options.ResetToYinYang.GetBool())
            {
                Main.DoingYingYang = false;
            }
            if (Main.ColliderPlayers.Contains(target.PlayerId))
                Main.ColliderPlayers.Remove(target.PlayerId);

            if (target.Is(CustomRoles.Camouflager) && Camouflager.DidCamo)
            {
                Logger.Info($"Camouflager Revert ShapeShift (Killed Camouflager)", "Camouflager");
                Camouflager.DidCamo = false;
                foreach (PlayerControl revert in PlayerControl.AllPlayerControls)
                {
                    if (revert.Is(CustomRoles.Phantom) || revert == null || revert.Data.Disconnected || revert.PlayerId == target.PlayerId) continue;
                    if (revert.inVent)
                        revert.MyPhysics.ExitAllVents();
                    revert.RpcRevertShapeshiftV2(true);
                }
            }
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.Data.Disconnected || pc == null || pc.Data.IsDead) continue;
                if (pc.IsLastImpostor())
                    Main.AllPlayerKillCooldown[pc.PlayerId] = Options.LastImpostorKillCooldown.GetFloat();
                if (pc.Is(CustomRoles.Mystic))
                {
                    pc.KillFlash();
                }
            }
            if (target.GetCustomSubRole() == CustomRoles.LoversRecode)
            {
                PlayerControl lover = Main.LoversPlayers.ToArray().Where(pc => pc.PlayerId == target.PlayerId).FirstOrDefault();
                Main.LoversPlayers.Remove(lover);
                Main.isLoversDead = true;
                if (Options.LoversDieTogether.GetBool())
                {
                    foreach (var lp in Main.LoversPlayers)
                    {
                        if (!lp.Is(CustomRoles.Pestilence))
                        {
                            lp.RpcMurderPlayer(lp);
                            PlayerState.SetDeathReason(lp.PlayerId, PlayerState.DeathReason.LoversSuicide);
                        }
                        Main.LoversPlayers.Remove(lp);
                    }
                }
            }

            PlayerState.SetDead(target.PlayerId);
            if (!Main.whoKilledWho.ContainsKey(target.Data.PlayerId))
                Main.whoKilledWho.Add(target.Data.PlayerId, killer.PlayerId);
            if (AmongUsClient.Instance.AmHost && killer.PlayerId != target.PlayerId)
            {
                PlayerState.isDead[target.PlayerId] = true;
                RPC.SendDeathReason(target.PlayerId, PlayerState.deathReasons[target.PlayerId]);
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RpcAddKill, Hazel.SendOption.Reliable, -1);
                writer.Write(killer.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                Main.KillCount[killer.PlayerId]++;
            }
            Utils.CountAliveImpostors();
            Utils.CustomSyncAllSettings();
            Utils.NotifyRoles();
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
    class ShapeshiftPatch
    {
        public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            Logger.Info($"{__instance?.GetNameWithRole()} => {target?.GetNameWithRole()}", "Shapeshift");
            if (!AmongUsClient.Instance.AmHost) return;

            var shapeshifter = __instance;
            var shapeshifting = shapeshifter.PlayerId != target.PlayerId;

            Main.CheckShapeshift[shapeshifter.PlayerId] = shapeshifting;
            shapeshifter.ResetKillCooldown();
            shapeshifter.CustomSyncSettings();
            if (shapeshifter.Is(CustomRoles.Warlock))
            {
                if (Main.CursedPlayers[shapeshifter.PlayerId] != null)//呪われた人がいるか確認
                {
                    if (shapeshifting && !Main.CursedPlayers[shapeshifter.PlayerId].Data.IsDead)//変身解除の時に反応しない
                    {
                        try
                        {
                            var cp = Main.CursedPlayers[shapeshifter.PlayerId];
                            Vector2 cppos = cp.transform.position;//呪われた人の位置
                            Dictionary<PlayerControl, float> cpdistance = new();
                            float dis;
                            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                            {
                                if (!Options.WarlockCanKillAlliedPlayers.GetBool())
                                {
                                    if (target.GetCustomRole().IsImpostorTeam()) continue;
                                    if (Main.GuardianAngelTarget.ContainsKey(shapeshifter.PlayerId))
                                    {
                                        foreach (var pair in Main.GuardianAngelTarget)
                                        {
                                            if (pair.Value == shapeshifter.PlayerId && pair.Key == p.PlayerId)
                                                continue;
                                        }
                                    }
                                }
                                if (!p.Data.IsDead && p.PlayerId != cp.PlayerId)
                                {
                                    dis = Vector2.Distance(cppos, p.transform.position);
                                    cpdistance.Add(p, dis);
                                    Logger.Info($"{p?.Data?.PlayerName}の位置{dis}", "Warlock");
                                }
                            }
                            var min = cpdistance.OrderBy(c => c.Value).FirstOrDefault();//一番小さい値を取り出す
                            PlayerControl targetw = min.Key;
                            if (targetw != null)
                            {
                            Logger.Info($"{targetw.GetNameWithRole()}was killed", "Warlock");
                            if (targetw.Is(CustomRoles.Warlock) && Main.VetIsAlerted)
                                targetw.RpcMurderPlayer(shapeshifter);
                            else if (target.Is(CustomRoles.Pestilence))
                                targetw.RpcMurderPlayerV2(cp);
                            else
                            {
                                if (targetw.Is(CustomRoles.Survivor))
                                {
                                    Utils.CheckSurvivorVest(targetw, cp, false);
                                }
                                else
                                    cp.RpcMurderPlayerV2(targetw);//殺す
                            }
                            if (targetw.Data.IsDead)
                            {
                                if (Main.KillCount[cp.PlayerId] > 0)
                                    Main.KillCount[cp.PlayerId] -= 1;
                                Main.KillCount[shapeshifter.PlayerId] += 1;
                            }
                            shapeshifter.RpcGuardAndKill(shapeshifter);
                            Main.isCurseAndKill[shapeshifter.PlayerId] = false;
                            Main.CursedPlayers[shapeshifter.PlayerId] = null;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex.ToString(), "Warlock Shift Error");
                        }
                    }
                }
            }
            if (shapeshifter.Is(CustomRoles.Miner) && shapeshifting && Options.UseVentButtonInsteadOfPet.GetBool())
            {
                if (Main.LastEnteredVent.ContainsKey(shapeshifter.PlayerId))
                {
                    int ventId = Main.LastEnteredVent[shapeshifter.PlayerId].Id;
                    var vent = Main.LastEnteredVent[shapeshifter.PlayerId];
                    var position = Main.LastEnteredVentLocation[shapeshifter.PlayerId];
                    Logger.Info($"{shapeshifter.Data.PlayerName}:{position}", "MinerTeleport");
                    Utils.TP(shapeshifter.NetTransform, new Vector2(position.x, position.y + 0.3636f));
                }
            }
            if (shapeshifter.CanMakeMadmate() && shapeshifting)
            {//変身したとき一番近い人をマッドメイトにする処理
                Vector2 shapeshifterPosition = shapeshifter.transform.position;//変身者の位置
                Dictionary<PlayerControl, float> mpdistance = new();
                float dis;
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (!p.Data.IsDead && p.Data.Role.Role != RoleTypes.Shapeshifter && !p.Is(RoleType.Impostor) && !p.Is(CustomRoles.SKMadmate) && !p.GetCustomRole().IsNeutralKilling() && !p.Is(RoleType.Coven))
                    {
                        dis = Vector2.Distance(shapeshifterPosition, p.transform.position);
                        mpdistance.Add(p, dis);
                    }
                }
                if (mpdistance.Count() != 0)
                {
                    var min = mpdistance.OrderBy(c => c.Value).FirstOrDefault();//一番値が小さい
                    PlayerControl targetm = min.Key;
                    if (Main.ExecutionerTarget.ContainsKey(target.PlayerId))
                        Main.ExecutionerTarget.Remove(target.PlayerId);
                    if (Main.GuardianAngelTarget.ContainsKey(target.PlayerId))
                        Main.GuardianAngelTarget.Remove(target.PlayerId);
                    if (targetm.Is(CustomRoles.Sheriff))
                        targetm.RpcSetCustomRole(CustomRoles.CorruptedSheriff);
                    else if (targetm.Is(CustomRoles.Investigator))
                        targetm.RpcSetCustomRole(CustomRoles.CorruptedSheriff);
                    else if (targetm.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                        targetm.RpcMurderPlayer(shapeshifter);
                    else
                        targetm.RpcSetCustomRole(CustomRoles.SKMadmate);
                    Logger.Info($"Make SKMadmate:{targetm.name}", "Shapeshift");
                    Main.SKMadmateNowCount++;
                    Utils.CustomSyncAllSettings();
                    Utils.NotifyRoles();
                }
            }
            if (shapeshifter.Is(CustomRoles.Freezer))
            {
                switch (shapeshifting)
                {
                    case false:
                        if (Main.currentFreezingTarget != 255)
                        {
                            var ftarget = Utils.GetPlayerById(Main.currentFreezingTarget);
                            Logger.Info($"{ftarget.Data.PlayerName} was unfrozen", "Freezer");
                            Main.AllPlayerSpeed[ftarget.PlayerId] = Main.RealOptionsData.AsNormalOptions()!.PlayerSpeedMod;
                            ftarget.CustomSyncSettings();
                            RPC.PlaySoundRPC(ftarget.PlayerId, Sounds.TaskComplete);
                        }
                        Main.currentFreezingTarget = 255;
                        break;
                    case true:
                        Main.currentFreezingTarget = target.PlayerId;
                        var frtarget = Utils.GetPlayerById(Main.currentFreezingTarget);
                        Logger.Info($"{frtarget.Data.PlayerName} was frozen", "Freezer");
                        Main.AllPlayerSpeed[frtarget.PlayerId] = 0.00001f;
                        frtarget.CustomSyncSettings();
                        break;
                }
            }
            if (shapeshifter.Is(CustomRoles.Grenadier)) Camouflague.Grenade(shapeshifting);
            if (shapeshifter.Is(CustomRoles.FireWorks)) FireWorks.ShapeShiftState(shapeshifter, shapeshifting);
            if (shapeshifter.Is(CustomRoles.Sniper)) Sniper.ShapeShiftCheck(shapeshifter, shapeshifting, target);
            if (shapeshifter.Is(CustomRoles.Ninja) && !shapeshifter.Data.IsDead && Main.MercCanSuicide) Ninja.ShapeShiftCheck(shapeshifter, shapeshifting);
            if (shapeshifter.Is(CustomRoles.Necromancer)) Necromancer.OnShapeshiftCheck(shapeshifter, shapeshifting);
            if (shapeshifter.Is(CustomRoles.Disperser) && shapeshifting) Utils.DispersePlayers(shapeshifter);
            if (shapeshifter.Is(CustomRoles.BountyHunter) && !shapeshifting) BountyHunter.ResetTarget(shapeshifter);
            if (shapeshifter.Is(CustomRoles.Camouflager))
            {
                Camouflager.ShapeShiftState(shapeshifter, shapeshifting, target);
            }
            if (shapeshifter.Is(CustomRoles.SerialKiller) && !shapeshifter.Data.IsDead && Main.MercCanSuicide && shapeshifting)
            {
                shapeshifter.RpcMurderPlayer(shapeshifter); Sheriff.SwitchToCorrupt(shapeshifter, shapeshifter);
            }

            if (shapeshifter.Is(CustomRoles.Escapist) && Options.UseVentButtonInsteadOfPet.GetBool()) Escapist.OnShapeshift(shapeshifter, shapeshifting);

            if (!shapeshifting)
            {
                new LateTask(() =>
                {
                    Utils.NotifyRoles(NoCache: true);
                },
                1.2f, "ShapeShiftNotify");
            }
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
    class ReportDeadBodyPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] GameData.PlayerInfo target)
        {
            if (GameStates.IsMeeting) return false;

            //if (Main.KilledDemo.Contains(__instance.PlayerId)) return false;
            if (CustomRoles.TheGlitch.IsEnable() | CustomRoles.Escort.IsEnable() | CustomRoles.Consort.IsEnable())
            {
                List<byte> hackedPlayers = new();
                foreach (var cp in Main.CursedPlayers)
                {
                    if (cp.Value == null) continue;
                    if (Utils.GetPlayerById(cp.Key).GetCustomRole().CanRoleBlock())
                    {
                        hackedPlayers.Add(cp.Value.PlayerId);
                    }
                }
                if (hackedPlayers.Contains(__instance.PlayerId))
                {
                    return false;
                }
            }
            if (Camouflager.DidCamo) return false;
            if (Bomber.DoesExist())
            {
                if (__instance.PlayerId == Bomber.CurrentBombedPlayer && Bomber.TargetIsRoleBlocked) return false;
            }
            Logger.Info($"{__instance.GetNameWithRole()} => {target?.GetNameWithRole() ?? "null"}", "ReportDeadBody");
            if (target != null)
            {
                if (Main.unreportableBodies.Contains(target.PlayerId)) return false;
            }
            if (AgiTater.IsEnable() && AmongUsClient.Instance.AmHost)
            {
                if (AgiTater.CurrentBombedPlayer != 255 && AgiTater.ReportBait.GetBool())
                {
                    var bombed = Utils.GetPlayerById(AgiTater.CurrentBombedPlayer);
                    if (bombed.GetCustomSubRole() is CustomRoles.Bait)
                    {
                        bombed.RpcMurderPlayer(bombed);
                        PlayerState.SetDeathReason(bombed.PlayerId, PlayerState.DeathReason.Bombed);
                        foreach (var playerid in AgiTater.playerIdList)
                        {
                            var pc = Utils.GetPlayerById(playerid);
                            Logger.Info(bombed?.Data?.PlayerName + "はBaitだった", "MurderPlayer");
                            new LateTask(() => pc.CmdReportDeadBody(bombed.Data), 0.15f, "Bait Self Report");
                        }
                        AgiTater.ResetBomb();
                        return false;
                    }
                }
            }
            if (target != null)
            {
                if (__instance.Is(CustomRoles.Vulture) && !__instance.Data.IsDead && !Main.unreportableBodies.Contains(target.PlayerId) && target.GetCustomSubRole() != CustomRoles.Bait && AmongUsClient.Instance.AmHost)
                {
                    Main.unreportableBodies.Add(target.PlayerId);
                    Main.AteBodies++;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetVultureAmount, Hazel.SendOption.Reliable, -1);
                    writer.Write(Main.AteBodies);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    if (Main.AteBodies == Options.BodiesAmount.GetFloat())
                    {
                        //Vulture wins.
                        //CheckGameEndPatch.CheckAndEndGameForVultureWin();
                        //RPC.VultureWin();
                        //CheckForEndVotingPatch.Prefix();
                        // VULTURE IS HANDLED LIKE AN ACTUAL NEUTRAL NOW
                        return true;
                    }
                    return false;
                }
            }
            if (target != null)
            {
                if (__instance.Is(CustomRoles.Cleaner) && Main.CleanerCanClean[__instance.PlayerId] && !__instance.Data.IsDead && !Main.unreportableBodies.Contains(target.PlayerId) && target.GetCustomSubRole() != CustomRoles.Bait && AmongUsClient.Instance.AmHost)
                {
                    Main.unreportableBodies.Add(target.PlayerId);
                    Main.AllPlayerKillCooldown[__instance.PlayerId] = Main.AllPlayerKillCooldown[__instance.PlayerId] * 2;
                    __instance.RpcGuardAndKill(__instance);
                    Main.AllPlayerKillCooldown[__instance.PlayerId] = Main.AllPlayerKillCooldown[__instance.PlayerId] / 2;
                    foreach (DeadBody deadBody in GameObject.FindObjectsOfType<DeadBody>())
                    {
                        if (deadBody.ParentId == target.Object.PlayerId) Object.Destroy(deadBody.gameObject);
                    }
                    Main.CleanerCanClean[__instance.PlayerId] = false;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RpcSetCleanerClean, Hazel.SendOption.Reliable, -1);
                    writer.Write(__instance.PlayerId);
                    writer.Write(false);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    Utils.NotifyRoles(GameStates.IsMeeting, __instance);
                    new LateTask(() =>
                    {
                        Main.CleanerCanClean[__instance.PlayerId] = true;
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RpcSetCleanerClean, Hazel.SendOption.Reliable, -1);
                        writer.Write(__instance.PlayerId);
                        writer.Write(false);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        Utils.NotifyRoles(GameStates.IsMeeting, __instance);
                    }, Main.AllPlayerKillCooldown[__instance.PlayerId], $"Cleaner Can Clean Now {__instance.GetNameWithRole()}");
                    return false;
                }
            }
            if (target != null) //ボタン
            {
                if (__instance.Is(CustomRoles.Oblivious) && target.GetCustomSubRole() != CustomRoles.Bait)
                {
                    return false;
                }
            }
            if (target != null) //Alturist Revivng a Body.
            {
                if (__instance.Is(CustomRoles.Alturist))
                {
                    //Main.MayorUsedButtonCount[__instance.PlayerId] += 1;
                    var reviving = Utils.GetPlayerById(target.PlayerId);
                    Main.DeadPlayersThisRound.Remove(reviving.PlayerId);
                    reviving.Revive();
                    reviving.RpcMurderPlayer(__instance);
                    Utils.SendMessage("You chose to revive a player for sacrificing your own life.", __instance.PlayerId);
                    new LateTask(() => reviving.CmdReportDeadBody(__instance.Data), 0.15f, "Alturist Self Report");
                    return false;
                }
            }
            if (Options.IsStandardHAS && target != null && __instance == target.Object) return true; //[StandardHAS] ボタンでなく、通報者と死体が同じなら許可
            if (Options.CurrentGameMode() == CustomGameMode.HideAndSeek || Options.IsStandardHAS) return false;
            if (!AmongUsClient.Instance.AmHost) return true;
            BountyHunter.OnReportDeadBody();
            SerialKiller.OnReportDeadBody();
            Bomber.OnReportDeadBody();
            Main.bombedVents.Clear();
            // Main.KilledDemo.Clear();
            Main.ArsonistTimer.Clear();
            Main.PlagueBearerTimer.Clear();
            Main.IsRoundOne = false;
            Main.IsRoundOneGA = false;
            Main.IsGazing = false;
            Main.GazeReady = false;
            Main.bkProtected = false;
            if (target == null) //ボタン
            {
                if (__instance.Is(CustomRoles.Mayor))
                {
                    Main.MayorUsedButtonCount[__instance.PlayerId] += 1;
                }
            }

            if (target != null) //Sleuth Report for Non-Buttons
            {
                if (__instance.GetCustomSubRole() == CustomRoles.Sleuth)
                {
                    //Main.MayorUsedButtonCount[__instance.PlayerId] += 1;
                    Utils.SendMessage("The body you reported had a clue about their role. They were " + Utils.GetRoleName(target.GetCustomRole()) + ".", __instance.PlayerId);
                    foreach (var ar in Main.SleuthReported)
                    {
                        if (ar.Key != __instance.PlayerId) break;
                        if (ar.Value.Item1 != target.PlayerId) break;
                        // now we set it to true
                        var stuff = Main.SleuthReported[__instance.PlayerId];
                        stuff.Item2 = true;
                        Main.SleuthReported[__instance.PlayerId] = stuff;
                    }
                }
            }

            if (AgiTater.IsEnable() && AmongUsClient.Instance.AmHost)
            {
                if (AgiTater.CurrentBombedPlayer != 255)
                {
                    var bombed = Utils.GetPlayerById(AgiTater.CurrentBombedPlayer);
                    if (!bombed.Is(CustomRoles.Pestilence))
                    {
                        if (bombed.Is(CustomRoles.Veteran))
                        {
                            if (!Main.VetIsAlerted)
                            {
                                bombed.RpcMurderPlayer(bombed);
                                PlayerState.SetDeathReason(bombed.PlayerId, PlayerState.DeathReason.Bombed);
                            }
                        }
                        else
                        {
                            bombed.RpcMurderPlayer(bombed);
                            PlayerState.SetDeathReason(bombed.PlayerId, PlayerState.DeathReason.Bombed);
                        }
                    }
                }
                AgiTater.ResetBomb();
            }

            if (target != null) //Medium Report for Non-Buttons
            {
                if (__instance.Is(CustomRoles.Medium))
                {
                    try
                    {
                        if (Main.whoKilledWho.ContainsKey(target.PlayerId))
                        {
                            var killer = Utils.GetPlayerById(Main.whoKilledWho[target.PlayerId]);
                            if (killer.PlayerId != target.PlayerId)
                                Utils.SendMessage("The killer left a clue on their identity. The killer's role was " + Utils.GetRoleName(killer.GetCustomRole()) + ".", __instance.PlayerId);
                            else
                            {
                                var DeathReason = PlayerState.GetDeathReason(target.PlayerId);
                                if (PlayerState.GetDeathReason(target.PlayerId) == PlayerState.DeathReason.Bombed)
                                    Utils.SendMessage("The body was bombed by an Agitater, Fireworks, Bastion, Demolitionist, Hex Master, Postman, Terrorist, or a Bomber if they didn't kill in time.", __instance.PlayerId);
                                else if (PlayerState.GetDeathReason(target.PlayerId) == PlayerState.DeathReason.Torched)
                                    Utils.SendMessage("The body was incinerated by Arsonist.", __instance.PlayerId);
                                else if (PlayerState.GetDeathReason(target.PlayerId) == PlayerState.DeathReason.Misfire)
                                    Utils.SendMessage("They misfired as a Sheriff.", __instance.PlayerId);
                                else if (PlayerState.GetDeathReason(target.PlayerId) == PlayerState.DeathReason.Sniped)
                                    Utils.SendMessage("The body was a sniped by Sniper.", __instance.PlayerId);
                                else if (PlayerState.GetDeathReason(target.PlayerId) == PlayerState.DeathReason.Execution)
                                    Utils.SendMessage("This player appears to have been guessed.", __instance.PlayerId);
                                else if (PlayerState.GetDeathReason(target.PlayerId) == PlayerState.DeathReason.Bite)
                                    Utils.SendMessage("The body was bitten by Vampire.", __instance.PlayerId);
                                else if (PlayerState.GetDeathReason(target.PlayerId) == PlayerState.DeathReason.LoversSuicide)
                                    Utils.SendMessage("This person took their own life because their lover died.", __instance.PlayerId);
                                else if (PlayerState.GetDeathReason(target.PlayerId) == PlayerState.DeathReason.Suicide)
                                    Utils.SendMessage("They apparently commited suicide.", __instance.PlayerId);
                                else
                                {
                                    var deathReasonFound = PlayerState.deathReasons.TryGetValue(target.Object.PlayerId, out var deathReason);
                                    var reason = deathReasonFound ? GetString("DeathReason." + deathReason.ToString()) : "No Death Reason Found";
                                    Utils.SendMessage($"We were not able to find a message for this person's death reason. Their death reason is: {reason}", __instance.PlayerId);
                                }
                            }
                        }
                        else
                        {
                            Utils.SendMessage("The body had no hints on how they died.", __instance.PlayerId);
                        }
                    }
                    catch
                    {
                        var deathReasonFound = PlayerState.deathReasons.TryGetValue(target.Object.PlayerId, out var deathReason);
                        var reason = deathReasonFound ? GetString("DeathReason." + deathReason.ToString()) : "No Death Reason Found";
                        Utils.SendMessage($"An error occured displaying the message for this person. Their death reason is: {reason}", __instance.PlayerId);
                    }
                }
            }

            if (target != null) //Sleuth Report for Non-Buttons
            {
                if (__instance.Is(CustomRoles.Necromancer))
                {
                    Necromancer.OnReportBody(target.GetCustomRole(), __instance);
                }
            }

            if (target != null) //Amnesiac Stealing role
            {
                if (__instance.Is(CustomRoles.Amnesiac))
                {
                    var reported = Utils.GetPlayerById(target.PlayerId);
                    Utils.SendMessage("You joined that person's team! They were " + Utils.GetRoleName(target.GetCustomRole()) + ".", __instance.PlayerId);
                    Utils.SendMessage("The Amnesiac stole your role! Because of this, your role has been reset to the default one.", reported.PlayerId);

                    __instance.ResetKillCooldown();
                    PlayerControl pc = __instance;
                    var rand = new System.Random();
                    switch (target.GetCustomRole().GetRoleType())
                    {
                        case RoleType.Crewmate:
                            Sheriff.Add(__instance.PlayerId);
                            __instance.RpcSetCustomRole(CustomRoles.Sheriff);
                            break;
                        case RoleType.Neutral:
                            if (reported.IsNeutralKiller())
                            {
                                if (!reported.Is(CustomRoles.Pirate) && !reported.Is(CustomRoles.CrewPostor))
                                {
                                    __instance.RpcSetCustomRole(reported.GetCustomRole());
                                    switch (target.GetCustomRole())
                                    {
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
                                    }
                                }
                                else
                                {
                                    __instance.RpcSetCustomRole(CustomRoles.Opportunist);
                                }
                            }
                            else
                            {
                                switch (target.GetCustomRole())
                                {
                                    case CustomRoles.Hitman:
                                        __instance.RpcSetCustomRole(CustomRoles.Hitman);
                                        break;
                                    case CustomRoles.Executioner:
                                    case CustomRoles.Swapper:
                                    case CustomRoles.Jester:
                                        __instance.RpcSetCustomRole(target.GetCustomRole());
                                        if (__instance.Is(CustomRoles.Swapper) | __instance.Is(CustomRoles.Executioner))
                                        {
                                            List<PlayerControl> targetList = new();
                                            rand = new System.Random();
                                            foreach (var targete in PlayerControl.AllPlayerControls)
                                            {
                                                if (targete == null || targete.Data.IsDead || targete.Data.Disconnected) continue;
                                                if (pc == targete) continue;
                                                if (!Options.ExecutionerCanTargetImpostor.GetBool() && targete.GetCustomRole().IsImpostor() | targete.GetCustomRole().IsMadmate()) continue;
                                                if (targete.GetCustomRole().IsNeutral()) continue;
                                                if (targete.GetCustomRole().IsCoven()) continue;
                                                if (targete.Is(CustomRoles.Phantom)) continue;
                                                if (Main.ExecutionerTarget.ContainsValue(targete.PlayerId)) continue;

                                                targetList.Add(targete);
                                            }
                                            var Target = targetList[rand.Next(targetList.Count)];
                                            Main.ExecutionerTarget.Add(pc.PlayerId, Target.PlayerId);
                                            RPC.SendExecutionerTarget(pc.PlayerId, Target.PlayerId);
                                            Logger.Info($"{pc.GetNameWithRole()}:{Target.GetNameWithRole()}", "Executioner/Swapper");
                                        }
                                        break;
                                    default:
                                        __instance.RpcSetCustomRole(CustomRoles.Opportunist);
                                        break;
                                }
                            }

                            break;
                        case RoleType.Coven:
                            __instance.RpcSetCustomRole(CustomRoles.Coven);
                            break;
                        case RoleType.Madmate:
                        case RoleType.Impostor:
                            __instance.RpcSetCustomRole(CustomRoles.CorruptedSheriff);
                            RPC.SetTraitor(__instance.PlayerId);
                            break;
                    }
                    if (Options.TosOptions.GetBool() && Options.AmnesiacRememberAnnouncement.GetBool())
                        Utils.SendMessage($"An Amnesiac remembered that they were like the {Utils.GetRoleName(__instance.GetCustomRole())}!");
                    reported.SetDefaultRole();
                    // attempted revive fix
                    //return false;
                }
            }
            foreach (var key in Main.IsShapeShifted)
            {
                var shifter = Utils.GetPlayerById(key);
                shifter.RpcRevertShapeshiftV2(true);
                Main.IsShapeShifted.Remove(key);
            }
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc == null) continue;
                if (pc.Data.Disconnected) continue;
                if (pc.Data.IsDead) continue;
                if (pc.IsModClient()) continue;
                if (pc.Is(CustomRoles.CorruptedSheriff))
                {
                    Utils.SendMessage($"You have betrayed the Crewmates and joined the Impostors team.\nThe names in red are your fellow Impostors. \nKill and sabotage with your new team.", pc.PlayerId);
                }
                if (Main.HasModifier.ContainsKey(pc.PlayerId))
                {
                    CustomRoles role = CustomRoles.Amnesiac;
                    foreach (var modifier in Main.HasModifier)
                    {
                        if (modifier.Key == pc.PlayerId)
                            role = modifier.Value;
                    }
                    if (role != CustomRoles.Amnesiac)
                    {
                        Utils.SendMessage($"Modifier: {pc.GetSubRoleName()}", pc.PlayerId);
                        Utils.SendMessage($"{GetString(pc.GetSubRoleName() + "Info")}", pc.PlayerId);
                    }
                }
            }
            if (CustomRoles.Camouflager.IsEnable())
            {
                if (Camouflager.DidCamo)
                {
                    Logger.Info($"Camouflager Revert ShapeShift for Meeting", "Camouflager");
                    foreach (PlayerControl revert in PlayerControl.AllPlayerControls)
                    {
                        if (revert.Is(CustomRoles.Phantom) || revert == null || revert.Data.Disconnected) continue;
                        if (revert.inVent)
                            revert.MyPhysics.ExitAllVents();
                        revert.RpcRevertShapeshiftV2(true);
                    }
                    Camouflager.DidCamo = false;
                }
            }
            if (!Main.HasNecronomicon)
                Main.CovenMeetings++;
            if (Camouflague.IsActive && Options.CamoComms.GetBool())
            {
                // Camouflague.InMeeting = true;
                //Camouflague.MeetingRevert();
            }
            if (target != null)
            {
                if (Manipulator.IsEnable())
                    if (Manipulator.killedList.Contains(target.PlayerId))
                    {
                        Manipulator.SabotagedMeetingReport();
                    }
            }
            Guesser.canGuess = true;
            if (Main.CovenMeetings == Options.CovenMeetings.GetFloat() && !Main.HasNecronomicon && CustomRoles.Coven.IsEnable())
            {
                Main.HasNecronomicon = true;
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc == null || pc.Data.Disconnected) continue;
                    //time for coven
                    if (CustomRolesHelper.GetRoleType(pc.GetCustomRole()) == RoleType.Coven)
                    {
                        //if they are coven.
                        Utils.SendMessage("You now weild the Necronomicon. With this power, you gain venting, guessing, and a whole lot of other powers depending on your role.", pc.PlayerId);
                        switch (pc.GetCustomRole())
                        {

                            case CustomRoles.Poisoner:
                                Utils.SendMessage("Also With this power, you gain nothing.", pc.PlayerId);
                                break;
                            case CustomRoles.CovenWitch:
                                Utils.SendMessage("Also With this power, you no longer let others kill for you. You kill on your own now.", pc.PlayerId);
                                break;
                            case CustomRoles.Coven:
                                Utils.SendMessage("Also With this power, you gain nothing.", pc.PlayerId);
                                break;
                            case CustomRoles.HexMaster:
                                Utils.SendMessage("You have nothing extra to gain.", pc.PlayerId);
                                break;
                            case CustomRoles.PotionMaster:
                                Utils.SendMessage("Also With this power, you have shorter cooldowns. And the ability to kill when shifted.", pc.PlayerId);
                                break;
                            case CustomRoles.Medusa:
                                Utils.SendMessage("Also With this power, you can kill normally. However, you still cannot vent normally.", pc.PlayerId);
                                break;
                            case CustomRoles.Mimic:
                                Utils.SendMessage("Your role prevents you from having this power however.", pc.PlayerId);
                                break;
                            case CustomRoles.Necromancer:
                                Utils.SendMessage("Also With this power, the Veteran cannot kill you.", pc.PlayerId);
                                break;
                            case CustomRoles.Conjuror:
                                Utils.SendMessage("Also With this power, you can kill normally.", pc.PlayerId);
                                break;
                        }
                    }
                    else
                    {
                        Utils.SendMessage("The Coven now weild Necronomicon. With this power, they gain venting, guessing, and more depending on their role.", pc.PlayerId);
                    }
                }
            }

            if (Options.SyncButtonMode.GetBool() && target == null)
            {
                Logger.Info("最大:" + Options.SyncedButtonCount.GetInt() + ", 現在:" + Options.UsedButtonCount, "ReportDeadBody");
                if (Options.SyncedButtonCount.GetFloat() <= Options.UsedButtonCount)
                {
                    Logger.Info("使用可能ボタン回数が最大数を超えているため、ボタンはキャンセルされました。", "ReportDeadBody");
                    return false;
                }
                else Options.UsedButtonCount++;
                if (Options.SyncedButtonCount.GetFloat() == Options.UsedButtonCount)
                {
                    Logger.Info("使用可能ボタン回数が最大数に達しました。", "ReportDeadBody");
                }
            }

            foreach (var bp in Main.BitPlayers)
            {
                var vampireID = bp.Value.Item1;
                var bitten = Utils.GetPlayerById(bp.Key);

                if (!bitten.Is(CustomRoles.Veteran))
                {
                    if (!bitten.Data.IsDead)
                    {
                        PlayerControl vampire = Utils.GetPlayerById(vampireID);
                        if (bitten.Is(CustomRoles.Pestilence))
                            PlayerState.SetDeathReason(vampire.PlayerId, PlayerState.DeathReason.Bite);
                        else
                            PlayerState.SetDeathReason(bitten.PlayerId, PlayerState.DeathReason.Bite);
                        //Protectは強制的にはがす
                        // PlayerControl vampire = Utils.GetPlayerById(vampireID);
                        if (bitten.protectedByGuardian)
                            bitten.RpcMurderPlayer(bitten);
                        if (bitten.Is(CustomRoles.Pestilence))
                            vampire.RpcMurderPlayer(vampire);
                        else if (bitten.Is(CustomRoles.Survivor))
                        {
                            Utils.CheckSurvivorVest(bitten, vampire, false);
                        }
                        else
                            bitten.RpcMurderPlayer(bitten);
                        RPC.PlaySoundRPC(vampireID, Sounds.KillSound);
                        if (bitten.Is(CustomRoles.Demolitionist))
                            Main.KilledDemo.Add(vampireID);
                        if (bitten.GetCustomSubRole() == CustomRoles.Bewilder)
                            Main.KilledBewilder.Add(vampireID);
                        if (bitten.GetCustomSubRole() == CustomRoles.Diseased)
                            Main.KilledDiseased.Add(vampireID);
                        Logger.Info("Vampireに噛まれている" + bitten?.Data?.PlayerName + "を自爆させました。", "ReportDeadBody");
                    }
                    else
                        Logger.Info("Vampireに噛まれている" + bitten?.Data?.PlayerName + "はすでに死んでいました。", "ReportDeadBody");
                }
                else
                {
                    if (Main.VetIsAlerted)
                    {
                        PlayerState.SetDeathReason(vampireID, PlayerState.DeathReason.Kill);
                        //Protectは強制的にはがす
                        PlayerControl vampire = Utils.GetPlayerById(vampireID);
                        if (vampire.protectedByGuardian)
                            vampire.RpcMurderPlayer(vampire);
                        vampire.RpcMurderPlayer(vampire);
                        RPC.PlaySoundRPC(bitten.PlayerId, Sounds.KillSound);
                    }
                    else
                    {
                        if (!bitten.Data.IsDead)
                        {
                            PlayerState.SetDeathReason(bitten.PlayerId, PlayerState.DeathReason.Bite);
                            //Protectは強制的にはがす
                            if (bitten.protectedByGuardian)
                                bitten.RpcMurderPlayer(bitten);
                            bitten.RpcMurderPlayer(bitten);
                            RPC.PlaySoundRPC(vampireID, Sounds.KillSound);
                            Logger.Info("Vampireに噛まれている" + bitten?.Data?.PlayerName + "を自爆させました。", "ReportDeadBody");
                        }
                        else
                            Logger.Info("Vampireに噛まれている" + bitten?.Data?.PlayerName + "はすでに死んでいました。", "ReportDeadBody");
                    }
                }
            }
            foreach (var killer in Main.KilledDemo)
            {
                var realKiller = Utils.GetPlayerById(killer);
                if (!realKiller.Is(CustomRoles.Pestilence))
                {
                    if (!realKiller.inVent)
                    {
                        realKiller.CustomSyncSettings();
                        if (realKiller.protectedByGuardian)
                            realKiller.RpcMurderPlayer(realKiller);
                        if (realKiller.Is(CustomRoles.Survivor))
                        {
                            foreach (var ar in Main.SurvivorStuff)
                            {
                                if (ar.Key != realKiller.PlayerId) break;
                                var stuff = Main.SurvivorStuff[realKiller.PlayerId];
                                if (stuff.Item2)
                                {
                                    //killer.RpcGuardAndKill(killer);
                                    realKiller.RpcGuardAndKill(realKiller);
                                }
                                else
                                {
                                    realKiller.RpcMurderPlayerV2(realKiller);
                                }
                            }
                        }
                        else
                            realKiller.RpcMurderPlayer(realKiller);
                        PlayerState.SetDeathReason(killer, PlayerState.DeathReason.Suicide);
                        PlayerState.SetDead(killer);
                    }
                }
                Main.KilledDemo.Remove(killer);
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.NotifyDemoKill, Hazel.SendOption.Reliable, -1);
                writer.Write(true);
                writer.Write(killer);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
            Main.BitPlayers = new Dictionary<byte, (byte, float)>();
            foreach (var pair in Main.knownGhosts)
            {
                pair.Value.Clear();
            }
            Main.ColliderPlayers.Clear();
            Main.KilledDemo.Clear();
            Main.PuppeteerList.Clear();
            Main.WitchList.Clear();
            Main.WitchedList.Clear();
            Main.FirstMeetingOccured = false;
            Main.MareHasRedName = false;
            Main.MercCanSuicide = false;
            Sniper.OnStartMeeting();
            Main.VetIsAlerted = false;
            Main.IsRampaged = false;
            Main.RampageReady = false;
            if (Main.IsProtected)
            {
                Main.IsProtected = false;
                if (Options.TosOptions.GetBool() && Options.GuardianAngelVoteImmunity.GetBool())
                {
                    foreach (var gaTarget in Main.GuardianAngelTarget)
                    {
                        Main.unvotablePlayers.Add(gaTarget.Value);
                    }
                }
            }

            if (__instance.Data.IsDead) return true;
            //=============================================
            //以下、ボタンが押されることが確定したものとする。
            //=============================================

            Utils.CustomSyncAllSettings();
            return true;
        }
        public static async void ChangeLocalNameAndRevert(string name, int time)
        {
            //async Taskじゃ警告出るから仕方ないよね。
            var revertName = PlayerControl.LocalPlayer.name;
            PlayerControl.LocalPlayer.RpcSetNameEx(name);
            await Task.Delay(time);
            PlayerControl.LocalPlayer.RpcSetNameEx(revertName);
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    class FixedUpdatePatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            var player = __instance;

            if (AmongUsClient.Instance.AmHost)
            {
                if (GameStates.IsLobby && (ModUpdater.hasUpdate || ModUpdater.isBroken) && AmongUsClient.Instance.IsGamePublic)
                    AmongUsClient.Instance.ChangeGamePublic(false);

                //if (GameStates.IsInTask && __instance.inVent)
                //    ExtendedPlayerControl.CheckVentSwap(__instance);

                if (GameStates.IsInTask && CustomRoles.Vampire.IsEnable())
                {
                    //Vampireの処理
                    if (Main.BitPlayers.ContainsKey(player.PlayerId))
                    {
                        //__instance:キルされる予定のプレイヤー
                        //main.BitPlayers[__instance.PlayerId].Item1:キルしたプレイヤーのID
                        //main.BitPlayers[__instance.PlayerId].Item2:キルするまでの秒数
                        byte vampireID = Main.BitPlayers[player.PlayerId].Item1;
                        float killTimer = Main.BitPlayers[player.PlayerId].Item2;
                        if (killTimer >= Options.VampireKillDelay.GetFloat())
                        {
                            var bitten = player;
                            if (!bitten.Data.IsDead)
                            {
                                PlayerState.SetDeathReason(bitten.PlayerId, PlayerState.DeathReason.Bite);
                                var vampirePC = Utils.GetPlayerById(vampireID);
                                if (!bitten.Is(CustomRoles.Bait))
                                {
                                    if (vampirePC.IsAlive())
                                    {
                                        if (bitten.Is(CustomRoles.Pestilence))
                                            vampirePC.RpcMurderPlayer(vampirePC);
                                        else if (bitten.Is(CustomRoles.Survivor))
                                        {
                                            foreach (var ar in Main.SurvivorStuff)
                                            {
                                                if (ar.Key != bitten.PlayerId) break;
                                                var stuff = Main.SurvivorStuff[bitten.PlayerId];
                                                if (stuff.Item2)
                                                {
                                                    //killer.RpcGuardAndKill(killer);
                                                    vampirePC.RpcGuardAndKill(bitten);
                                                }
                                                else
                                                {
                                                    bitten.RpcMurderPlayerV2(bitten);
                                                }
                                            }
                                        }
                                        else
                                            bitten.RpcMurderPlayer(bitten);
                                    }
                                    //bitten.RpcMurderPlayer(bitten);
                                    else
                                    {
                                        if (Options.VampireBuff.GetBool())
                                        {
                                            if (bitten.Is(CustomRoles.Survivor)) { Utils.CheckSurvivorVest(bitten, vampirePC); }
                                            else
                                                bitten.RpcMurderPlayer(bitten);
                                        }
                                    }
                                }
                                else
                                {
                                    vampirePC.RpcMurderPlayer(bitten);
                                }

                                Logger.Info("Vampireに噛まれている" + bitten?.Data?.PlayerName + "を自爆させました。", "Vampire");
                                if (vampirePC.IsAlive())
                                {
                                    RPC.PlaySoundRPC(vampireID, Sounds.KillSound);
                                    if (bitten.Is(CustomRoles.Trapper))
                                        vampirePC.TrapperKilled(bitten);
                                    if (bitten.Is(CustomRoles.Demolitionist))
                                        Main.KilledDemo.Add(vampireID);
                                    if (bitten.GetCustomSubRole() == CustomRoles.Bewilder)
                                        Main.KilledBewilder.Add(vampireID);
                                    if (bitten.GetCustomSubRole() == CustomRoles.Diseased)
                                        Main.KilledDiseased.Add(vampireID);
                                }
                            }
                            else
                            {
                                Logger.Info("Vampireに噛まれている" + bitten?.Data?.PlayerName + "はすでに死んでいました。", "Vampire");
                            }
                            Main.BitPlayers.Remove(bitten.PlayerId);
                        }
                        else
                        {
                            Main.BitPlayers[player.PlayerId] =
                            (vampireID, killTimer + Time.fixedDeltaTime);
                        }
                    }
                }
                SerialKiller.FixedUpdate(player);
                if (GameStates.IsInTask && Main.WarlockTimer.ContainsKey(player.PlayerId))//処理を1秒遅らせる
                {
                    if (player.IsAlive())
                    {
                        if (Main.WarlockTimer[player.PlayerId] >= 1f)
                        {
                            player.RpcResetAbilityCooldown();
                            Main.isCursed = false;//変身クールを１秒に変更
                            Utils.CustomSyncAllSettings();
                            Main.WarlockTimer.Remove(player.PlayerId);
                        }
                        else Main.WarlockTimer[player.PlayerId] = Main.WarlockTimer[player.PlayerId] + Time.fixedDeltaTime;//時間をカウント
                    }
                    else
                    {
                        Main.WarlockTimer.Remove(player.PlayerId);
                    }
                }
                //ターゲットのリセット
                BountyHunter.FixedUpdate(player);
                if (GameStates.IsInTask && player.IsAlive() && Options.LadderDeath.GetBool())
                {
                    FallFromLadder.FixedUpdate(player);
                }
                /*if (GameStates.isInGame && main.AirshipMeetingTimer.ContainsKey(__instance.PlayerId)) //会議後すぐにここの処理をするため不要になったコードです。今後#465で変更した仕様がバグって、ここの処理が必要になった時のために残してコメントアウトしています
                {
                    if (main.AirshipMeetingTimer[__instance.PlayerId] >= 9f && !main.AirshipMeetingCheck)
                    {
                        main.AirshipMeetingCheck = true;
                        Utils.CustomSyncAllSettings();
                    }
                    if (main.AirshipMeetingTimer[__instance.PlayerId] >= 10f)
                    {
                        Utils.AfterMeetingTasks();
                        main.AirshipMeetingTimer.Remove(__instance.PlayerId);
                    }
                    else
                        main.AirshipMeetingTimer[__instance.PlayerId] = (main.AirshipMeetingTimer[__instance.PlayerId] + Time.fixedDeltaTime);
                    }
                }*/

                //if (GameStates.IsInGame) LoversSuicide();

                if (GameStates.IsInTask && Main.ArsonistTimer.ContainsKey(player.PlayerId))//アーソニストが誰かを塗っているとき
                {
                    if (!player.IsAlive())
                    {
                        Main.ArsonistTimer.Remove(player.PlayerId);
                        Utils.NotifyRoles(SpecifySeer: __instance);
                        RPC.ResetCurrentDousingTarget(player.PlayerId);
                    }
                    else
                    {
                        var ar_target = Main.ArsonistTimer[player.PlayerId].Item1;//塗られる人
                        var ar_time = Main.ArsonistTimer[player.PlayerId].Item2;//塗った時間
                        if (!ar_target.IsAlive())
                        {
                            Main.ArsonistTimer.Remove(player.PlayerId);
                        }
                        else if (ar_time >= Options.ArsonistDouseTime.GetFloat())//時間以上一緒にいて塗れた時
                        {
                            Main.AllPlayerKillCooldown[player.PlayerId] = Options.ArsonistCooldown.GetFloat() * 2;
                            Utils.CustomSyncAllSettings();//同期
                            player.RpcGuardAndKill(ar_target);//通知とクールリセット
                            Main.dousedIDs.Add(ar_target.PlayerId);
                            Main.ArsonistTimer.Remove(player.PlayerId);//塗が完了したのでDictionaryから削除
                            Main.isDoused[(player.PlayerId, ar_target.PlayerId)] = true;//塗り完了
                            player.RpcSetDousedPlayer(ar_target, true);
                            Utils.NotifyRoles();//名前変更
                            RPC.ResetCurrentDousingTarget(player.PlayerId);
                        }
                        else
                        {
                            float dis;
                            dis = Vector2.Distance(player.transform.position, ar_target.transform.position);//距離を出す
                            if (dis <= 1.75f)//一定の距離にターゲットがいるならば時間をカウント
                            {
                                Main.ArsonistTimer[player.PlayerId] = (ar_target, ar_time + Time.fixedDeltaTime);
                            }
                            else//それ以外は削除
                            {
                                Main.ArsonistTimer.Remove(player.PlayerId);
                                Utils.NotifyRoles(SpecifySeer: __instance);
                                RPC.ResetCurrentDousingTarget(player.PlayerId);

                                Logger.Info($"Canceled: {__instance.GetNameWithRole()}", "Arsonist");
                            }
                        }

                    }
                }
                if (GameStates.IsInTask && Bomber.BomberTimer.ContainsKey(player.PlayerId) && Bomber.CurrentBombedPlayer == 255)//アーソニストが誰かを塗っているとき
                {
                    if (!player.IsAlive())
                    {
                        Bomber.BomberTimer.Remove(player.PlayerId);
                        Bomber.SendRPC();
                        Utils.NotifyRoles(SpecifySeer: __instance);
                    }
                    else
                    {
                        var ar_target = Bomber.BomberTimer[player.PlayerId].Item1;//塗られる人
                        var ar_time = Bomber.BomberTimer[player.PlayerId].Item2;//塗った時間
                        if (!ar_target.IsAlive())
                        {
                            Bomber.BomberTimer.Remove(player.PlayerId);
                        }
                        else if (ar_time >= Bomber.BombTime.GetFloat())//時間以上一緒にいて塗れた時
                        {
                            Main.AllPlayerKillCooldown[player.PlayerId] = Bomber.BombCooldown.GetFloat() * 2;
                            Utils.CustomSyncAllSettings();//同期
                            player.RpcGuardAndKill(ar_target);//通知とクールリセット
                            Bomber.BomberTimer.Remove(player.PlayerId);//塗が完了したのでDictionaryから削
                            Bomber.CurrentDouseTarget = 255;
                            Bomber.CurrentBombedPlayer = ar_target.PlayerId;
                            Bomber.SendRPC(255, ar_target.PlayerId);
                            Utils.NotifyRoles();
                        }
                        else
                        {
                            float dis;
                            dis = Vector2.Distance(player.transform.position, ar_target.transform.position);//距離を出す
                            if (dis <= 1.75f)
                            {
                                Bomber.BomberTimer[player.PlayerId] = (ar_target, ar_time + Time.fixedDeltaTime);
                            }
                            else
                            {
                                Bomber.BomberTimer.Remove(player.PlayerId);
                                Bomber.CurrentDouseTarget = 255;
                                Bomber.SendRPC(255, 255);

                                Logger.Info($"Canceled: {__instance.GetNameWithRole()}", "Bomber");
                            }
                        }

                    }
                }
                if (GameStates.IsInTask && Main.PlagueBearerTimer.ContainsKey(player.PlayerId))//アーソニストが誰かを塗っているとき
                {
                    if (!player.IsAlive())
                    {
                        Main.PlagueBearerTimer.Remove(player.PlayerId);
                        Utils.NotifyRoles(SpecifySeer: __instance);
                        RPC.ResetCurrentInfectingTarget(player.PlayerId);
                    }
                    else
                    {
                        var ar_target = Main.PlagueBearerTimer[player.PlayerId].Item1;//塗られる人
                        var ar_time = Main.PlagueBearerTimer[player.PlayerId].Item2;//塗った時間
                        if (!ar_target.IsAlive())
                        {
                            Main.PlagueBearerTimer.Remove(player.PlayerId);
                        }
                        else if (ar_time >= 0)//時間以上一緒にいて塗れた時
                        {
                            Main.AllPlayerKillCooldown[player.PlayerId] = Options.InfectCooldown.GetFloat() * 2;
                            Utils.CustomSyncAllSettings();
                            player.RpcGuardAndKill(ar_target);
                            Main.PlagueBearerTimer.Remove(player.PlayerId);
                            Main.isInfected[(player.PlayerId, ar_target.PlayerId)] = true;
                            player.RpcSetInfectedPlayer(ar_target, true);
                            Utils.NotifyRoles();//名前変更
                            RPC.ResetCurrentInfectingTarget(player.PlayerId);
                        }

                    }
                }

                if (GameStates.IsInTask && Main.PuppeteerList.ContainsKey(player.PlayerId))
                {
                    if (!player.IsAlive())
                    {
                        Main.PuppeteerList.Remove(player.PlayerId);
                    }
                    else
                    {
                        Vector2 puppeteerPos = player.transform.position;//PuppeteerListのKeyの位置
                        Dictionary<byte, float> targetDistance = new();
                        float dis;
                        foreach (var target in PlayerControl.AllPlayerControls)
                        {
                            if (!target.IsAlive()) continue;
                            if (!Options.PuppeteerCanKillAlliedPlayers.GetBool())
                            {
                                // if (target.Data.IsImpostor()) continue;
                                if (target.GetCustomRole().IsImpostorTeam()) continue;
                                if (Main.GuardianAngelTarget.ContainsKey(Main.PuppeteerList[player.PlayerId]))
                                {
                                    foreach (var pair in Main.GuardianAngelTarget)
                                    {
                                        if (pair.Value == Main.PuppeteerList[player.PlayerId] && pair.Key == target.PlayerId)
                                            continue;
                                    }
                                }
                            }
                            if (target.PlayerId != player.PlayerId && !target.Data.IsDead && target.PlayerId != Main.PuppeteerList[player.PlayerId])
                            {
                                dis = Vector2.Distance(puppeteerPos, target.transform.position);
                                targetDistance.Add(target.PlayerId, dis);
                            }
                        }
                        if (targetDistance.Count() != 0)
                        {
                            var min = targetDistance.OrderBy(c => c.Value).FirstOrDefault();//一番値が小さい
                            PlayerControl target = Utils.GetPlayerById(min.Key);
                            var KillRange = GameOptionsData.KillDistances[Mathf.Clamp(GameOptionsManager.Instance.currentNormalGameOptions.KillDistance, 0, 2)];
                            if (min.Value <= KillRange && player.CanMove && target.CanMove)
                            {
                                RPC.PlaySoundRPC(Main.PuppeteerList[player.PlayerId], Sounds.KillSound);
                                if (target.Is(CustomRoles.Pestilence))
                                    target.RpcMurderPlayer(player);
                                else if (player.Is(CustomRoles.Survivor)) { Utils.CheckSurvivorVest(target, player, false); }
                                else
                                    player.RpcMurderPlayer(target);
                                Utils.CustomSyncAllSettings();
                                Main.PuppeteerList.Remove(player.PlayerId);
                                Utils.NotifyRoles();
                            }
                        }
                    }
                }
                if (GameStates.IsInTask && Bomber.DoesExist() && Bomber.CurrentBombedPlayer == player.PlayerId && !Bomber.InKillProgress)
                {
                    if (!player.IsAlive())
                    {
                        Bomber.CurrentBombedPlayer = 255;
                        Bomber.SendRPC(255, 255);
                    }
                    else
                    {
                        Vector2 puppeteerPos = player.transform.position;
                        Dictionary<byte, float> targetDistance = new();
                        float dis;
                        foreach (var target in PlayerControl.AllPlayerControls)
                        {
                            if (!target.IsAlive()) continue;
                            if (target.GetCustomRole().IsImpostorTeam()) continue;

                            if (target.PlayerId != player.PlayerId && !target.Data.IsDead && target.PlayerId != Bomber.CurrentDouseTarget)
                            {
                                dis = Vector2.Distance(puppeteerPos, target.transform.position);
                                targetDistance.Add(target.PlayerId, dis);
                            }
                        }
                        if (targetDistance.Count() != 0)
                        {
                            var min = targetDistance.OrderBy(c => c.Value).FirstOrDefault();//一番値が小さい
                            PlayerControl target = Utils.GetPlayerById(min.Key);
                            var KillRange = GameOptionsData.KillDistances[Mathf.Clamp(GameOptionsManager.Instance.currentNormalGameOptions.KillDistance, 0, 2)];
                            if (min.Value <= KillRange && player.CanMove && target.CanMove)
                            {
                                Bomber.OnTargetCollide(target);
                            }
                        }
                    }
                }
                if (GameStates.IsInTask && Main.WitchList.ContainsKey(player.PlayerId) && !player.Is(CustomRoles.NeutWitch))
                {
                    if (!player.IsAlive())
                    {
                        Main.WitchList.Remove(player.PlayerId);
                    }
                    else
                    {
                        Vector2 witchPos = player.transform.position;
                        Dictionary<byte, float> targetDistance = new();
                        float dis;
                        foreach (var target in PlayerControl.AllPlayerControls)
                        {
                            if (!target.IsAlive()) continue;
                            if (target.PlayerId != player.PlayerId && !target.Is(CustomRoles.NeutWitch))
                            {
                                dis = Vector2.Distance(witchPos, target.transform.position);
                                targetDistance.Add(target.PlayerId, dis);
                            }
                        }
                        if (targetDistance.Count() != 0)
                        {
                            var min = targetDistance.OrderBy(c => c.Value).FirstOrDefault();//一番値が小さい
                            PlayerControl target = Utils.GetPlayerById(min.Key);
                            var KillRange = GameOptionsData.KillDistances[Mathf.Clamp(GameOptionsManager.Instance.currentNormalGameOptions.KillDistance, 0, 2)];
                            if (min.Value <= KillRange && player.CanMove && target.CanMove)
                            {
                                RPC.PlaySoundRPC(Main.WitchList[player.PlayerId], Sounds.KillSound);
                                if (target.Is(CustomRoles.Pestilence))
                                    target.RpcMurderPlayer(player);
                                else if (player.Is(CustomRoles.Survivor)) { Utils.CheckSurvivorVest(target, player, false); }
                                else
                                    player.RpcMurderPlayer(target);
                                Utils.CustomSyncAllSettings();
                                Main.WitchList.Remove(player.PlayerId);
                                Utils.NotifyRoles();
                            }
                        }
                    }
                }
                if (GameStates.IsInTask && Main.ColliderPlayers.Contains(player.PlayerId))
                {
                    if (!player.IsAlive())
                    {
                        Main.ColliderPlayers.Remove(player.PlayerId);
                    }
                    else
                    {
                        Vector2 puppeteerPos = player.transform.position;
                        Dictionary<byte, float> targetDistance = new();
                        float dis;
                        foreach (var targetid in Main.ColliderPlayers)
                        {
                            var target = Utils.GetPlayerById(targetid);
                            if (!target.IsAlive()) continue;
                            if (target == player) continue;
                            if (target.PlayerId != player.PlayerId && !target.GetCustomRole().IsImpostor() && !target.Data.IsDead)
                            {
                                dis = Vector2.Distance(puppeteerPos, target.transform.position);
                                targetDistance.Add(target.PlayerId, dis);
                            }
                        }
                        if (targetDistance.Count() != 0)
                        {
                            var min = targetDistance.OrderBy(c => c.Value).FirstOrDefault();//一番値が小さい
                            PlayerControl target = Utils.GetPlayerById(min.Key);
                            var KillRange = GameOptionsData.KillDistances[Mathf.Clamp(GameOptionsManager.Instance.currentNormalGameOptions.KillDistance, 0, 2)];
                            if (min.Value <= KillRange && player.CanMove && target.CanMove)
                            {
                                //RPC.PlaySoundRPC(Main.PuppeteerList[player.PlayerId], Sounds.KillSound);
                                if (target.Is(CustomRoles.Pestilence))
                                    target.RpcMurderPlayer(player);
                                else if (player.Is(CustomRoles.Pestilence))
                                    player.RpcMurderPlayer(target);
                                else if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                                    target.RpcMurderPlayer(player);
                                else if (player.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                                    player.RpcMurderPlayer(target);
                                else if (player.Is(CustomRoles.Survivor)) { Utils.CheckSurvivorVest(target, player, false); }
                                else if (target.Is(CustomRoles.Survivor)) { Utils.CheckSurvivorVest(player, target, false); }
                                else
                                {
                                    player.RpcMurderPlayer(target);
                                    target.RpcMurderPlayer(player);
                                }
                                Utils.CustomSyncAllSettings();
                                //Main.ColliderPlayers.Remove(player.PlayerId);
                                Utils.NotifyRoles();
                                if (Options.ResetToYinYang.GetBool()) Main.DoingYingYang = true;
                            }
                        }
                    }
                }
                if (GameStates.IsInTask && Main.WitchedList.ContainsKey(player.PlayerId))
                {
                    if (!player.IsAlive())
                    {
                        Main.WitchedList.Remove(player.PlayerId);
                    }
                    else
                    {
                        Vector2 puppeteerPos = player.transform.position;//WitchedListのKeyの位置
                        Dictionary<byte, float> targetDistance = new();
                        float dis;
                        foreach (var target in PlayerControl.AllPlayerControls)
                        {
                            if (!target.IsAlive()) continue;
                            if (target.PlayerId != player.PlayerId && !target.GetCustomRole().IsCoven() && !target.Data.IsDead)
                            {
                                dis = Vector2.Distance(puppeteerPos, target.transform.position);
                                targetDistance.Add(target.PlayerId, dis);
                            }
                        }
                        if (targetDistance.Count() != 0)
                        {
                            var min = targetDistance.OrderBy(c => c.Value).FirstOrDefault();//一番値が小さい
                            PlayerControl target = Utils.GetPlayerById(min.Key);
                            var KillRange = GameOptionsData.KillDistances[Mathf.Clamp(GameOptionsManager.Instance.currentNormalGameOptions.KillDistance, 0, 2)];
                            if (min.Value <= KillRange && player.CanMove && target.CanMove)
                            {
                                RPC.PlaySoundRPC(Main.WitchedList[player.PlayerId], Sounds.KillSound);
                                if (target.Is(CustomRoles.Pestilence))
                                    target.RpcMurderPlayer(player);
                                else if (player.Is(CustomRoles.Survivor)) { Utils.CheckSurvivorVest(target, player, false); }
                                else
                                    player.RpcMurderPlayer(target);
                                Utils.CustomSyncAllSettings();
                                Main.WitchedList.Remove(player.PlayerId);
                                Utils.NotifyRoles();
                            }
                        }
                    }
                }
                if (Options.SpeedrunGamemode.GetBool() && Main.KillingSpree.Contains(player.PlayerId))
                {
                    if (player.Data.IsDead)
                    {
                        Main.KillingSpree.Remove(player.PlayerId);
                    }
                    else
                    {
                        Vector2 puppeteerPos = player.transform.position;
                        Dictionary<byte, float> targetDistance = new();
                        float dis;
                        foreach (var target in PlayerControl.AllPlayerControls)
                        {
                            if (!target.IsAlive()) continue;
                            if (target.PlayerId != player.PlayerId && !target.Data.IsDead)
                            {
                                dis = Vector2.Distance(puppeteerPos, target.transform.position);
                                targetDistance.Add(target.PlayerId, dis);
                            }
                        }
                        if (targetDistance.Count() != 0)
                        {
                            var min = targetDistance.OrderBy(c => c.Value).FirstOrDefault();
                            PlayerControl target = Utils.GetPlayerById(min.Key);
                            var KillRange = GameOptionsData.KillDistances[Mathf.Clamp(GameOptionsManager.Instance.currentNormalGameOptions.KillDistance, 0, 2)];
                            if (min.Value <= KillRange && player.CanMove && target.CanMove)
                            {
                                player.RpcMurderPlayer(target);
                                Utils.CustomSyncAllSettings();
                                Utils.NotifyRoles();
                            }
                        }
                    }
                }
                if (GameStates.IsInTask && player.GetCustomSubRole() is CustomRoles.Obvious && AmongUsClient.Instance.AmHost && player.CanMove && !player.Data.IsDead)
                {
                    Vector2 playerPos = player.transform.position;
                    Dictionary<DeadBody, float> targetDistance = new();
                    float dis;
                    foreach (DeadBody deadBody in GameObject.FindObjectsOfType<DeadBody>())
                    {
                        dis = Vector2.Distance(playerPos, deadBody.transform.position);
                        targetDistance.Add(deadBody, dis);
                    }
                    if (targetDistance.Count() != 0)
                    {
                        var min = targetDistance.OrderBy(c => c.Value).FirstOrDefault();
                        PlayerControl target = Utils.GetPlayerById(min.Key.ParentId);
                        if (min.Value <= player.MaxReportDistance)
                        {
                            player.CmdReportDeadBody(target.Data);
                        }
                    }
                }
                if (GameStates.IsInTask && AgiTater.CurrentBombedPlayer == player.PlayerId && AgiTater.IsEnable() && AgiTater.CanPass)
                {
                    if (!player.IsAlive())
                    {
                        AgiTater.ResetBomb(false);
                    }
                    else
                    {
                        Vector2 puppeteerPos = player.transform.position;
                        Dictionary<byte, float> targetDistance = new();
                        float dis;
                        foreach (var target in PlayerControl.AllPlayerControls)
                        {
                            if (!target.IsAlive()) continue;
                            if (target.PlayerId != player.PlayerId && target.PlayerId != AgiTater.LastBombedPlayer && !target.Data.IsDead)
                            {
                                dis = Vector2.Distance(puppeteerPos, target.transform.position);
                                targetDistance.Add(target.PlayerId, dis);
                            }
                        }
                        if (targetDistance.Count != 0)
                        {
                            var min = targetDistance.OrderBy(c => c.Value).FirstOrDefault();
                            PlayerControl target = Utils.GetPlayerById(min.Key);
                            var KillRange = GameOptionsData.KillDistances[Mathf.Clamp(GameOptionsManager.Instance.currentNormalGameOptions.KillDistance, 0, 2)];
                            if (min.Value <= KillRange && player.CanMove && target.CanMove)
                                AgiTater.PassBomb(player, target);
                        }
                    }
                }
                if (GameStates.IsInTask && Postman.playerIdList.Contains(player.PlayerId) && Postman.IsEnable() &&
                Postman.IsDelivering && Postman.target != null && !player.Data.IsDead)
                {
                    if (!player.IsAlive())
                    {
                        Postman.IsDelivering = false;
                    }
                    else
                    {
                        Vector2 puppeteerPos = player.transform.position;
                        var KillRange = GameOptionsData.KillDistances[Mathf.Clamp((int)Postman.DistanceType, 0, 2)];
                        if (Vector2.Distance(puppeteerPos, (Vector2)Postman.target.transform.position) <= KillRange && player.CanMove && Postman.target.CanMove)
                            Postman.DeliverMessage(player, Postman.target);
                    }
                }
                if (GameStates.IsInTask && player == PlayerControl.LocalPlayer)
                    DisableDevice.FixedUpdate();

                if (GameStates.IsInGame && Main.RefixCooldownDelay <= 0)
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (/*pc.Is(CustomRoles.Vampire) ||*/ pc.Is(CustomRoles.Warlock))
                            Main.AllPlayerKillCooldown[pc.PlayerId] = Options.DefaultKillCooldown * 2;
                    }

                if (__instance.AmOwner && !Main.devNames.ContainsKey(__instance.PlayerId)) Utils.ApplySuffix();
            }

            //LocalPlayer専用
            if (__instance.AmOwner)
            {
                if (GameStates.IsInTask && !GameStates.IsLobby && (__instance.Is(CustomRoles.Sheriff) || __instance.Is(CustomRoles.PoisonMaster) || __instance.Is(CustomRoles.AgiTater) || __instance.Is(CustomRoles.NeutWitch) || __instance.Is(CustomRoles.Investigator) || __instance.Is(CustomRoles.Escort) || __instance.Is(CustomRoles.Crusader) || __instance.Is(CustomRoles.Hitman) || __instance.Is(CustomRoles.Janitor) || __instance.Is(CustomRoles.Painter) || __instance.Is(CustomRoles.Marksman) || __instance.Is(CustomRoles.BloodKnight) || __instance.Is(CustomRoles.Sidekick) || __instance.Is(CustomRoles.CorruptedSheriff) || __instance.GetRoleType() == RoleType.Coven || __instance.Is(CustomRoles.Arsonist) || __instance.Is(CustomRoles.Werewolf) || __instance.Is(CustomRoles.TheGlitch) || __instance.Is(CustomRoles.Juggernaut) || __instance.Is(CustomRoles.PlagueBearer) || __instance.Is(CustomRoles.Pestilence) || __instance.Is(CustomRoles.Jackal)) && !__instance.Data.IsDead)
                {
                    var players = __instance.GetPlayersInAbilityRangeSorted(false);
                    PlayerControl closest = players.Count <= 0 ? null : players[0];
                    HudManager.Instance.KillButton.SetTarget(closest);
                }
            }

            //役職テキストの表示
            var RoleTextTransform = __instance.cosmetics.nameText.transform.Find("RoleText");
            var RoleText = RoleTextTransform.GetComponent<TMPro.TextMeshPro>();
            if (RoleText != null && __instance != null)
            {
                if (GameStates.IsLobby)
                {
                    if (__instance.FriendCode is "nullrelish#9615" or "tillhoppy#6167" or "pingrating#9371") { }
                    else
                    {
                        if (Main.playerVersion.TryGetValue(__instance.PlayerId, out var ver))
                        {
                            bool different = false;
                            if (Main.version.CompareTo(ver.version) == 0)
                            {
                                string name = ver.tag == $"{ThisAssembly.Git.Commit}({ThisAssembly.Git.Branch})" ? $"<color=#87cefa>{__instance.name}</color>" : $"<color=#ffff00><size=1.2>{ver.tag}</size>\n{__instance?.name}</color>";
                                different = ver.tag == $"{ThisAssembly.Git.Commit}({ThisAssembly.Git.Branch})" ? false : true;
                                __instance.cosmetics.nameText.text = name;
                            }
                            else different = true;
                            if (different && AmongUsClient.Instance.AmHost)
                            {
                                AmongUsClient.Instance.KickPlayer(__instance.GetClientId(), false);
                                Logger.Warn($"{__instance?.Data?.PlayerName} had a different version than host. So they got kicked.", "Kick");
                                Logger.SendInGame("Kicked for having a different version than host.");
                            }
                        }
                        else __instance.cosmetics.nameText.text = __instance?.Data?.PlayerName;
                    }
                }
                if (GameStates.IsInGame)
                {
                    if (!Options.RolesLikeToU.GetBool() || PlayerControl.LocalPlayer.Data.IsDead)
                    {
                        var RoleTextData = Utils.GetRoleText(__instance);
                        RoleText.text = RoleTextData.Item1;
                        RoleText.color = RoleTextData.Item2;
                        if (__instance.AmOwner) RoleText.enabled = true;
                        else if (Main.VisibleTasksCount && PlayerControl.LocalPlayer.Data.IsDead && Options.GhostCanSeeOtherRoles.GetBool()) RoleText.enabled = true;
                        else RoleText.enabled = false;
                        if (!AmongUsClient.Instance.IsGameStarted && AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay)
                        {
                            RoleText.enabled = false;
                            if (!__instance.AmOwner) __instance.cosmetics.nameText.text = __instance?.Data?.PlayerName;
                        }
                        if (Main.VisibleTasksCount)
                            RoleText.text += $" {Utils.GetProgressText(__instance)}";
                    }


                    //変数定義
                    var seer = PlayerControl.LocalPlayer;
                    var target = __instance;

                    string RealName;
                    string Mark = "";
                    string Suffix = "";
                    string TeamText = "";
                    //名前変更
                    RealName = target.GetRealName();

                    //名前色変更処理
                    //自分自身の名前の色を変更
                    /* if (seer.Is(CustomRoles.TheGlitch))
                     {
                         if (seer.Data.Role.Role != RoleTypes.Shapeshifter)
                         {
                             RoleManager.Instance.SetRole(player, RoleTypes.Shapeshifter);
                             player.RpcSetCustomRole(CustomRoles.TheGlitch);
                         }
                     }*/
                    if (target.AmOwner && AmongUsClient.Instance.IsGameStarted)
                    { //targetが自分自身
                        if (Options.RolesLikeToU.GetBool() && !target.Data.IsDead)
                        {
                            RealName += $"\r\n{target.GetRoleName()}";
                            RealName += $" {Utils.GetProgressText(__instance)}";
                        }
                        RealName = Helpers.ColorString(target.GetRoleColor(), RealName); //名前の色を変更
                                                                                         //   if (target.Is(CustomRoles.Child) && Options.ChildKnown.GetBool())
                                                                                         //            RealName += Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Jackal), " (C)");
                        if (target.Is(CustomRoles.Arsonist) && target.IsDouseDone())
                            RealName = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Arsonist), GetString("EnterVentToWin"));
                        if (Main.KilledDemo.Contains(seer.PlayerId))
                            RealName = $"</size>\r\n{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Demolitionist), "You killed Demolitionist!")}";
                    }
                    if (target.GetCustomRole().IsImpostor() && seer.GetCustomRole().HostRedName())
                        RealName = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Crewmate), RealName);
                    if (target.Is(CustomRoles.PlagueBearer) && target.IsInfectDone())
                        target.RpcSetCustomRole(CustomRoles.Pestilence);
                    //タスクを終わらせたMadSnitchがインポスターを確認できる
                    else if (seer.Is(CustomRoles.MadSnitch) && //seerがMadSnitch
                        target.GetCustomRole().IsImpostor() && //targetがインポスター
                        seer.GetPlayerTaskState().IsTaskFinished) //seerのタスクが終わっている
                    {
                        RealName = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Impostor), RealName); //targetの名前を赤色で表示
                    }
                    else if (seer.GetCustomRole().IsCoven() && //seerがMadSnitch
                        target.GetCustomRole().IsCoven()) //seerのタスクが終わっている
                    {
                        RealName = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Coven), RealName); //targetの名前を赤色で表示
                    }
                    //タスクを終わらせたSnitchがインポスターを確認できる
                    else if (PlayerControl.LocalPlayer.Is(CustomRoles.Snitch) && //LocalPlayerがSnitch
                        PlayerControl.LocalPlayer.GetPlayerTaskState().IsTaskFinished) //LocalPlayerのタスクが終わっている
                    {
                        var targetCheck = target.GetCustomRole().IsImpostor() || (Options.SnitchCanFindNeutralKiller.GetBool() && target.IsNeutralKiller());
                        if (targetCheck)//__instanceがターゲット
                        {
                            RealName = Helpers.ColorString(target.GetRoleColor(), RealName); //targetの名前を役職色で表示
                        }
                    }
                    else if (PlayerControl.LocalPlayer.Is(CustomRoles.Tasker) && PlayerControl.LocalPlayer.GetPlayerTaskState().IsTaskFinished && AmongUsClient.Instance.AmHost)
                    {
                        Main.KillingSpree.Add(PlayerControl.LocalPlayer.PlayerId);
                    }
                    else if (seer.GetCustomRole().IsImpostor() && //seerがインポスター
                        target.Is(CustomRoles.Egoist) && Egoist.ImpostorsKnowEgo.GetBool() //targetがエゴイスト
                    )
                        RealName = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Egoist), RealName); //targetの名前をエゴイスト色で表示

                    else if ((seer.Is(CustomRoles.EgoSchrodingerCat) && target.Is(CustomRoles.Egoist)) || //エゴ猫 --> エゴイスト
                             (seer.GetCustomRole().IsJackalTeam() && target.GetCustomRole().IsJackalTeam() && Options.CurrentGameMode() == CustomGameMode.Standard) ||
                             (seer.GetCustomRole().IsImpostor() && target.GetCustomRole().IsImpostor())  //J猫 --> ジャッカル
                    )
                        RealName = Helpers.ColorString(target.GetRoleColor(), RealName); //targetの名前をtargetの役職の色で表示
                    else if (target.Is(CustomRoles.Mare) && Utils.IsActive(SystemTypes.Electrical) && Main.MareHasRedName)
                        RealName = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Impostor), RealName); //targetの赤色で表示
                    else if (seer != null)
                    {//NameColorManager準拠の処理
                        var ncd = NameColorManager.Instance.GetData(seer.PlayerId, target.PlayerId);
                        RealName = ncd.OpenTag + RealName + ncd.CloseTag;
                    }
                    string fontSize = "1.5";
                    if (GameStates.IsMeeting && (seer.GetClient().PlatformData.Platform.ToString() == "Playstation" || seer.GetClient().PlatformData.Platform.ToString() == "Switch")) fontSize = "70%";

                    if (seer.Data.IsDead && Options.GhostCanSeeOtherRoles.GetBool())
                        RealName = Helpers.ColorString(Utils.GetRoleColor(target.GetCustomRole()), RealName);
                    if (seer.GetCustomRole().IsImpostor())
                    {
                        if (Options.ImpostorKnowsRolesOfTeam.GetBool())
                        {
                            //so we gotta make it so they can see the team. of their impostor
                            if (target.GetCustomRole().IsImpostor())
                            {
                                if (!seer.Data.IsDead)
                                {
                                    // TeamText += "\r\n";
                                    if (target != seer)
                                    {
                                        if (!Options.RolesLikeToU.GetBool())
                                            RealName += $"<size={fontSize}>{Helpers.ColorString(target.GetRoleColor(), target.GetRoleName())}</size>\r\n";
                                        else
                                            RealName += $"\r\n{Helpers.ColorString(target.GetRoleColor(), target.GetRoleName())}";
                                    }
                                }
                            }
                        }
                        if (target.Is(CustomRoles.CorruptedSheriff))
                            RealName = $"{Helpers.ColorString(target.GetRoleColor(), RealName)}";
                    }
                    if (seer.GetCustomRole().IsCoven())
                    {
                        if (Options.CovenKnowsRolesOfTeam.GetBool())
                        {
                            if (target.GetCustomRole().IsCoven())
                            {
                                if (!seer.Data.IsDead)
                                {
                                    // TeamText += "\r\n";
                                    if (target != seer)
                                    {
                                        if (!Options.RolesLikeToU.GetBool())
                                            RealName += $"<size={fontSize}>{Helpers.ColorString(target.GetRoleColor(), target.GetRoleName())}</size>\r\n";
                                        else
                                            RealName += $"\r\n{Helpers.ColorString(target.GetRoleColor(), target.GetRoleName())}";
                                    }
                                }
                            }
                        }
                    }

                    if (Bomber.DoesExist() && seer.GetCustomRole().IsImpostor())
                    {
                        if (Bomber.AllImpostorsSeeBombedPlayer.GetBool())
                        {
                            if (target.PlayerId == Bomber.CurrentBombedPlayer) //seerがtargetに既にオイルを塗っている(完了)
                            {
                                Mark += $"<color={Utils.GetRoleColorCode(CustomRoles.Bomber)}>▲</color>";
                            }
                            if (target.PlayerId == Bomber.CurrentDouseTarget)
                            {
                                Mark += $"<color={Utils.GetRoleColorCode(CustomRoles.Bomber)}>△</color>";
                            }
                        }
                        else if (seer.Is(CustomRoles.Bomber))
                        {
                            if (target.PlayerId == Bomber.CurrentBombedPlayer)
                            {
                                Mark += $"<color={Utils.GetRoleColorCode(CustomRoles.Bomber)}>▲</color>";
                            }
                            if (
                                Bomber.BomberTimer.TryGetValue(seer.PlayerId, out var ar_kvp) &&
                                ar_kvp.Item1 == target
                            )
                            {
                                Mark += $"<color={Utils.GetRoleColorCode(CustomRoles.Bomber)}>△</color>";
                            }
                        }
                    }


                    //インポスター/キル可能な第三陣営がタスクが終わりそうなSnitchを確認できる
                    var canFindSnitchRole = seer.GetCustomRole().IsImpostor() || //LocalPlayerがインポスター
                        (Options.SnitchCanFindNeutralKiller.GetBool() && seer.IsNeutralKiller());//or キル可能な第三陣営


                    switch (seer.GetRoleType())
                    {
                        case RoleType.Coven:
                            if (target.GetRoleType() == RoleType.Coven)
                                RealName = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Coven), RealName);
                            break;
                    }
                    if (seer.Is(CustomRoles.GuardianAngelTOU))
                    {

                    }
                    if (target.Is(CustomRoles.Phantom) && Main.PhantomAlert)
                    {
                        RealName = Helpers.ColorString(target.GetRoleColor(), RealName);
                    }
                    if (seer.Is(CustomRoles.Survivor) && !Main.SurvivorStuff.ContainsKey(seer.PlayerId))
                    {
                        Main.SurvivorStuff.Add(seer.PlayerId, (0, false, false, false, false));
                    }
                    foreach (var TargetGA in Main.GuardianAngelTarget)
                    {
                        //if (Options.)
                        if ((seer.PlayerId == TargetGA.Key || seer.Data.IsDead) && //seerがKey or Dead
                        target.PlayerId == TargetGA.Value) //targetがValue
                            Mark += $"<color={Utils.GetRoleColorCode(CustomRoles.GuardianAngelTOU)}>♦</color>";
                    }
                    foreach (var TargetGA in Main.GuardianAngelTarget)
                    {
                        //if (Options.TargetKnowsGA.GetBool())
                        //{
                        //    if (seer.PlayerId == TargetGA.Value || seer.Data.IsDead)
                        //        Mark += $"<color={Utils.GetRoleColorCode(CustomRoles.GuardianAngelTOU)}>♦</color>";
                        //}
                    }
                    if (seer.Is(CustomRoles.Arsonist))
                    {
                        if (seer.IsDousedPlayer(target))
                        {
                            Mark += $"<color={Utils.GetRoleColorCode(CustomRoles.Arsonist)}>▲</color>";
                        }
                        else if (
                            Main.currentDousingTarget != 255 &&
                            Main.currentDousingTarget == target.PlayerId
                        )
                        {
                            Mark += $"<color={Utils.GetRoleColorCode(CustomRoles.Arsonist)}>△</color>";
                        }
                    }
                    if (seer.Is(CustomRoles.YingYanger) && Main.ColliderPlayers.Contains(target.PlayerId))
                    {
                        RealName = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Target), RealName);
                    }
                    if (seer.Is(CustomRoles.Investigator))
                    {
                        if (Investigator.hasSeered[target.PlayerId] == true)
                        {
                            // Investigator has Seered Player.
                            if (target.Is(CustomRoles.CorruptedSheriff))
                            {
                                if (Investigator.CSheriffSwitches.GetBool())
                                {
                                    RealName = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Impostor), RealName);
                                }
                                else
                                {
                                    if (Investigator.SeeredCSheriff)
                                        RealName = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Impostor), RealName);
                                    else
                                        RealName = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.TheGlitch), RealName);
                                }
                            }
                            else
                            {
                                if (Investigator.IsRed(target))
                                {
                                    if (target.GetCustomRole().IsCoven())
                                    {
                                        if (Investigator.CovenIsPurple.GetBool())
                                            RealName = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Coven), RealName); //targetの名前をエゴイスト色で表示
                                        else
                                            RealName = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Impostor), RealName); //targetの名前をエゴイスト色で表示
                                    }
                                    else
                                    {
                                        RealName = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Impostor), RealName);
                                    }
                                }
                                else
                                {
                                    RealName = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.TheGlitch), RealName); //targetの名前をエゴイスト色で表示
                                }
                            }
                        }
                    }
                    if (seer.Is(CustomRoles.HexMaster))
                    {
                        if (seer.IsHexedPlayer(target))
                            Mark += $"<color={Utils.GetRoleColorCode(CustomRoles.Coven)}>†</color>";
                    }
                    if (target.Is(CustomRoles.Child))
                    {
                        if (Options.ChildKnown.GetBool())
                            Mark += Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Jackal), " (C)");
                    }
                    if (seer.Is(CustomRoles.Doctor) && target.Data.IsDead && !seer.Data.IsDead)
                    {
                        if (!target.Data.Disconnected)
                            Mark += $"({Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Doctor), Utils.GetVitalText(target.PlayerId))})";
                    }
                    if (seer.Is(CustomRoles.PlagueBearer) || seer.Data.IsDead)
                    {
                        if (seer.IsInfectedPlayer(target))
                        {
                            Mark += $"<color={Utils.GetRoleColorCode(CustomRoles.Pestilence)}>◆</color>";
                        }
                        else if (
                            Main.currentInfectingTarget != 255 &&
                            Main.currentInfectingTarget == target.PlayerId
                        )
                        {
                            Mark += $"<color={Utils.GetRoleColorCode(CustomRoles.Pestilence)}>♦</color>";
                        }
                    }
                    foreach (var ExecutionerTarget in Main.ExecutionerTarget)
                    {
                        if ((seer.PlayerId == ExecutionerTarget.Key) && //seerがKey or Dead
                        target.PlayerId == ExecutionerTarget.Value) //targetがValue
                            RealName = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Target), RealName);
                    }
                    if (seer.Is(CustomRoles.BountyHunter) && BountyHunter.GetTarget(seer) != null)
                    {
                        var bounty = BountyHunter.GetTarget(seer);
                        if (target == bounty) RealName = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Target), RealName);
                    }
                    if (seer.Is(CustomRoles.Postman) && Postman.target != null)
                    {
                        if (target == Postman.target) RealName = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Target), RealName);
                    }
                    if (seer.Is(CustomRoles.Puppeteer))
                    {
                        if (seer.Is(CustomRoles.Puppeteer) &&
                        Main.PuppeteerList.ContainsValue(seer.PlayerId) &&
                        Main.PuppeteerList.ContainsKey(target.PlayerId))
                            Mark += $"<color={Utils.GetRoleColorCode(CustomRoles.Impostor)}>◆</color>";
                    }
                    if (seer.Is(CustomRoles.NeutWitch))
                    {
                        if (seer.Is(CustomRoles.NeutWitch) &&
                        Main.WitchList.ContainsValue(seer.PlayerId) &&
                        Main.WitchList.ContainsKey(target.PlayerId))
                            Mark += $"<color={Utils.GetRoleColorCode(CustomRoles.NeutWitch)}>◆</color>";
                    }
                    if (seer.Is(CustomRoles.CovenWitch) && !Main.HasNecronomicon)
                    {
                        if (seer.Is(CustomRoles.CovenWitch) &&
                        Main.WitchedList.ContainsValue(seer.PlayerId) &&
                        Main.WitchedList.ContainsKey(target.PlayerId))
                            Mark += $"<color={Utils.GetRoleColorCode(CustomRoles.CovenWitch)}>◆</color>";
                    }
                    if (Sniper.IsEnable() && target.AmOwner)
                    {
                        //銃声が聞こえるかチェック
                        Mark += Sniper.GetShotNotify(target.PlayerId);

                    }
                    //タスクが終わりそうなSnitchがいるとき、インポスター/キル可能な第三陣営に警告が表示される
                    if (target.Is(CustomRoles.Phantom) && Main.PhantomAlert && !GameStates.IsMeeting)
                    {
                        var found = false;
                        var update = false;
                        var arrows = "";
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        { //全員分ループ
                            if (pc.Is(CustomRoles.Phantom) || pc.Data.IsDead || pc.Data.Disconnected) continue;
                            found = true;
                            update = CheckArrowUpdate(target, pc, update, false);
                            var key = (pc.PlayerId, target.PlayerId);
                            arrows += Main.targetArrows[key];
                        }
                        if (found && target.AmOwner) Mark += $"<color={Utils.GetRoleColorCode(CustomRoles.Phantom)}>★{arrows}</color>";
                        if (AmongUsClient.Instance.AmHost && seer.PlayerId != target.PlayerId && update)
                        {
                            Utils.NotifyRoles(SpecifySeer: seer);
                        }
                    }
                    if ((!GameStates.IsMeeting && target.GetCustomRole().IsImpostor())
                        || (Options.SnitchCanFindNeutralKiller.GetBool() && target.IsNeutralKiller()))
                    { //targetがインポスターかつ自分自身
                        var found = false;
                        var update = false;
                        var arrows = "";
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        { //全員分ループ
                            if (!pc.Is(CustomRoles.Snitch) || pc.Data.IsDead || pc.Data.Disconnected) continue; //(スニッチ以外 || 死者 || 切断者)に用はない
                            if (pc.GetPlayerTaskState().DoExpose)
                            { //タスクが終わりそうなSnitchが見つかった時
                                found = true;
                                //矢印表示しないならこれ以上は不要
                                if (!Options.SnitchEnableTargetArrow.GetBool()) break;
                                update = CheckArrowUpdate(target, pc, update, false);
                                var key = (target.PlayerId, pc.PlayerId);
                                arrows += Main.targetArrows[key];
                            }
                        }
                        if (found && target.AmOwner) Mark += $"<color={Utils.GetRoleColorCode(CustomRoles.Snitch)}>★{arrows}</color>"; //Snitch警告を表示
                        if (AmongUsClient.Instance.AmHost && seer.PlayerId != target.PlayerId && update)
                        {
                            //更新があったら非Modに通知
                            Utils.NotifyRoles(SpecifySeer: target);
                        }
                    }
                    if (!GameStates.IsMeeting && target.Is(CustomRoles.Phantom))
                    {
                        var found = false;
                        var update = false;
                        var arrows = "";
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        { //全員分ループ
                            if (!pc.Is(CustomRoles.Phantom) || pc.Data.IsDead || pc.Data.Disconnected) continue; //(スニッチ以外 || 死者 || 切断者)に用はない
                            if (Main.PhantomAlert)
                            { //タスクが終わりそうなSnitchが見つかった時
                                found = true;
                                //矢印表示しないならこれ以上は不要
                                if (!Options.SnitchEnableTargetArrow.GetBool()) break;
                                update = CheckArrowUpdate(target, pc, update, false);
                                var key = (target.PlayerId, pc.PlayerId);
                                arrows += Main.targetArrows[key];
                            }
                        }
                        if (found && target.AmOwner) Mark += $"<color={Utils.GetRoleColorCode(CustomRoles.Phantom)}>★{arrows}</color>"; //Snitch警告を表示
                        if (AmongUsClient.Instance.AmHost && seer.PlayerId != target.PlayerId && update)
                        {
                            //更新があったら非Modに通知
                            Utils.NotifyRoles(SpecifySeer: target);
                        }
                    }
                    if (!GameStates.IsMeeting && target.Data.IsDead)
                    { //targetがインポスターかつ自分自身
                        var found = false;
                        var update = false;
                        var arrows = "";
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        { //全員分ループ
                            if (pc == null || !pc.Is(CustomRoles.Medium) || !pc.Data.IsDead || pc.Data.Disconnected || !Main.knownGhosts.ContainsKey(pc.PlayerId)) continue; //(スニッチ以外 || 死者 || 切断者)に用はない
                            if (Main.knownGhosts[pc.PlayerId].Count != 0)
                            {
                                found = true;
                                if (!Options.MediumArrow.GetBool()) break;
                                update = CheckArrowUpdate(target, pc, update, false);
                                var key = (target.PlayerId, pc.PlayerId);
                                arrows += Main.targetArrows[key];
                            }
                        }
                        if (found && target.AmOwner) Mark += $"<color={Utils.GetRoleColorCode(CustomRoles.Medium)}>★{arrows}</color>"; //Snitch警告を表示
                        if (AmongUsClient.Instance.AmHost && seer.PlayerId != target.PlayerId && update)
                        {
                            //更新があったら非Modに通知
                            Utils.NotifyRoles(SpecifySeer: target);
                        }
                    }
                    //ハートマークを付ける(会議中MOD視点)
                    if (__instance.GetCustomSubRole() == CustomRoles.LoversRecode && PlayerControl.LocalPlayer.GetCustomSubRole() == CustomRoles.LoversRecode)
                    {
                        Mark += $"<color={Utils.GetRoleColorCode(CustomRoles.LoversRecode)}>♡</color>";
                    }
                    else if (__instance.GetCustomSubRole() == CustomRoles.LoversRecode && PlayerControl.LocalPlayer.Data.IsDead)
                    {
                        Mark += $"<color={Utils.GetRoleColorCode(CustomRoles.LoversRecode)}>♡</color>";
                    }

                    //矢印オプションありならタスクが終わったスニッチはインポスター/キル可能な第三陣営の方角がわかる
                    if (GameStates.IsInTask && Options.SnitchEnableTargetArrow.GetBool() && target.Is(CustomRoles.Snitch))
                    {
                        var TaskState = target.GetPlayerTaskState();
                        if (TaskState.IsTaskFinished)
                        {
                            var coloredArrow = Options.SnitchCanGetArrowColor.GetBool();
                            var update = false;
                            foreach (var pc in PlayerControl.AllPlayerControls)
                            {
                                var foundCheck =
                                    pc.GetCustomRole().IsImpostor() ||
                                    (Options.SnitchCanFindNeutralKiller.GetBool() && pc.IsNeutralKiller());

                                //発見対象じゃ無ければ次
                                if (!foundCheck) continue;

                                update = CheckArrowUpdate(target, pc, update, coloredArrow);
                                var key = (target.PlayerId, pc.PlayerId);
                                if (target.AmOwner)
                                {
                                    //MODなら矢印表示
                                    Suffix += Main.targetArrows[key];
                                }
                            }
                            if (AmongUsClient.Instance.AmHost && seer.PlayerId != target.PlayerId && update)
                            {
                                //更新があったら非Modに通知
                                Utils.NotifyRoles(SpecifySeer: target);
                            }
                        }
                    }
                    if (GameStates.IsInTask && Postman.ArrowPointingToRecievers.GetBool() && target.Is(CustomRoles.Postman))
                    {
                        if (Postman.IsDelivering && Postman.target != null)
                        {
                            var coloredArrow = Options.SnitchCanGetArrowColor.GetBool();
                            var update = false;
                            foreach (var pc in PlayerControl.AllPlayerControls)
                            {
                                var foundCheck = pc.PlayerId == Postman.target.PlayerId;

                                //発見対象じゃ無ければ次
                                if (!foundCheck) continue;

                                update = CheckArrowUpdate(target, pc, update, coloredArrow);
                                var key = (target.PlayerId, pc.PlayerId);
                                if (target.AmOwner)
                                {
                                    Suffix += Main.targetArrows[key];
                                }
                            }
                            if (AmongUsClient.Instance.AmHost && seer.PlayerId != target.PlayerId && update)
                            {
                                //更新があったら非Modに通知
                                Utils.NotifyRoles(SpecifySeer: target);
                            }
                        }
                    }
                    if (GameStates.IsInTask && Options.MediumArrow.GetBool() && target.Is(CustomRoles.Medium) && Main.knownGhosts.ContainsKey(target.PlayerId))
                    {
                        var TaskState = Main.knownGhosts[target.PlayerId].Count != 0;
                        if (TaskState)
                        {
                            var update = false;
                            foreach (var pc in PlayerControl.AllPlayerControls)
                            {
                                var foundCheck = pc.Data.IsDead;

                                if (!foundCheck) continue;

                                update = CheckArrowUpdate(target, pc, update, false);
                                var key = (target.PlayerId, pc.PlayerId);
                                if (target.AmOwner)
                                {
                                    Suffix += Main.targetArrows[key];
                                }
                            }
                            if (AmongUsClient.Instance.AmHost && seer.PlayerId != target.PlayerId && update)
                            {
                                Utils.NotifyRoles(SpecifySeer: target);
                            }
                        }
                    }
                    if (GameStates.IsInTask && Options.AmnesiacArrow.GetBool() && target.Is(CustomRoles.Amnesiac))
                    {
                        var TaskState = Options.AmnesiacArrow.GetBool();
                        if (TaskState)
                        {
                            var coloredArrow = true;
                            var update = false;
                            foreach (DeadBody deadBody in GameObject.FindObjectsOfType<DeadBody>())
                            {
                                update = VultureArrowUpdate(target, deadBody, update, coloredArrow);
                                var key = (target.PlayerId, deadBody.ParentId);
                                if (target.AmOwner)
                                {
                                    Suffix += Main.targetArrows[key];
                                }
                            }
                            if (AmongUsClient.Instance.AmHost && seer.PlayerId != target.PlayerId && update)
                            {
                                //更新があったら非Modに通知
                                Utils.NotifyRoles(SpecifySeer: target);
                            }
                        }
                    }
                    if (GameStates.IsInTask && Options.VultureArrow.GetBool() && target.Is(CustomRoles.Vulture))
                    {
                        var TaskState = Options.VultureArrow.GetBool();
                        if (TaskState)
                        {
                            var coloredArrow = true;
                            var update = false;
                            foreach (DeadBody deadBody in GameObject.FindObjectsOfType<DeadBody>())
                            {
                                update = VultureArrowUpdate(target, deadBody, update, coloredArrow);
                                var key = (target.PlayerId, deadBody.ParentId);
                                if (target.AmOwner)
                                {
                                    Suffix += Main.targetArrows[key];
                                }
                            }
                            if (AmongUsClient.Instance.AmHost && seer.PlayerId != target.PlayerId && update)
                            {
                                //更新があったら非Modに通知
                                Utils.NotifyRoles(SpecifySeer: target);
                            }
                        }
                    }

                    /*if(main.AmDebugger.Value && main.BlockKilling.TryGetValue(target.PlayerId, out var isBlocked)) {
                        Mark = isBlocked ? "(true)" : "(false)";
                    }*/

                    //Mark・Suffixの適用
                    target.cosmetics.nameText.text = $"{RealName}{Mark}";

                    if (Suffix != "")
                    {
                        //名前が2行になると役職テキストを上にずらす必要がある
                        RoleText.transform.SetLocalY(0.35f);
                        target.cosmetics.nameText.text += "\r\n" + Suffix;

                    }
                    else
                    {
                        //役職テキストの座標を初期値に戻す
                        RoleText.transform.SetLocalY(0.175f);
                    }

                }
                else
                {
                    //役職テキストの座標を初期値に戻す
                    RoleText.transform.SetLocalY(0.175f);
                }
            }
        }
        //FIXME: 役職クラス化のタイミングで、このメソッドは移動予定
        public static void LoversSuicide(byte deathId = 0x7f, bool isExiled = false)
        {
            if (CustomRoles.LoversRecode.IsEnable() && Main.isLoversDead == false)
            {
                foreach (var loversPlayer in Main.LoversPlayers)
                {
                    //生きていて死ぬ予定でなければスキップ
                    // .GetCustomSubRole() == CustomRoles.LoversRecode
                    if (!loversPlayer.Data.IsDead && loversPlayer.PlayerId != deathId) continue;

                    Main.isLoversDead = true;
                    if (!Options.LoversDieTogether.GetBool()) continue;
                    foreach (var partnerPlayer in Main.LoversPlayers)
                    {
                        //本人ならスキップ
                        if (loversPlayer.PlayerId == partnerPlayer.PlayerId) continue;

                        //残った恋人を全て殺す(2人以上可)
                        //生きていて死ぬ予定もない場合は心中
                        if (partnerPlayer.PlayerId != deathId && !partnerPlayer.Data.IsDead && !partnerPlayer.Is(CustomRoles.Pestilence))
                        {
                            PlayerState.SetDeathReason(partnerPlayer.PlayerId, PlayerState.DeathReason.LoversSuicide);
                            if (isExiled)
                                Main.AfterMeetingDeathPlayers.TryAdd(partnerPlayer.PlayerId, PlayerState.DeathReason.LoversSuicide);
                            else
                                partnerPlayer.RpcMurderPlayer(partnerPlayer);
                        }
                    }
                }
            }
        }

        public static bool CheckArrowUpdate(PlayerControl seer, PlayerControl target, bool updateFlag, bool coloredArrow)
        {
            var key = (seer.PlayerId, target.PlayerId);
            if (!Main.targetArrows.TryGetValue(key, out var oldArrow))
            {
                //初回は必ず被らないもの
                oldArrow = "_";
            }
            //初期値は死んでる場合の空白にしておく
            var arrow = "";
            if (!PlayerState.isDead[seer.PlayerId] && !PlayerState.isDead[target.PlayerId])
            {
                //対象の方角ベクトルを取る
                var dir = target.transform.position - seer.transform.position;
                byte index;
                if (dir.magnitude < 2)
                {
                    //近い時はドット表示
                    index = 8;
                }
                else
                {
                    //-22.5～22.5度を0とするindexに変換
                    var angle = Vector3.SignedAngle(Vector3.down, dir, Vector3.back) + 180 + 22.5;
                    index = (byte)(((int)(angle / 45)) % 8);
                }
                arrow = "↑↗→↘↓↙←↖・"[index].ToString();
                if (coloredArrow)
                {
                    arrow = $"<color={target.GetRoleColorCode()}>{arrow}</color>";
                }
            }
            if (oldArrow != arrow)
            {
                //前回から変わってたら登録して更新フラグ
                Main.targetArrows[key] = arrow;
                updateFlag = true;
                //Logger.info($"{seer.name}->{target.name}:{arrow}");
            }
            return updateFlag;
        }
        public static bool VultureArrowUpdate(PlayerControl seer, DeadBody target, bool updateFlag, bool coloredArrow)
        {
            var key = (seer.PlayerId, target.ParentId);
            if (Main.DeadPlayersThisRound.Contains(target.ParentId))
            {
                if (!Main.targetArrows.TryGetValue(key, out var oldArrow))
                {
                    //初回は必ず被らないもの
                    oldArrow = "_";
                }
                //初期値は死んでる場合の空白にしておく
                var arrow = "";
                if (!PlayerState.isDead[seer.PlayerId])
                {
                    //対象の方角ベクトルを取る
                    var dir = target.transform.position - seer.transform.position;
                    byte index;
                    if (dir.magnitude < 2)
                    {
                        //近い時はドット表示
                        index = 8;
                    }
                    else
                    {
                        //-22.5～22.5度を0とするindexに変換
                        var angle = Vector3.SignedAngle(Vector3.down, dir, Vector3.back) + 180 + 22.5;
                        index = (byte)(((int)(angle / 45)) % 8);
                    }
                    arrow = "↑↗→↘↓↙←↖・"[index].ToString();
                    if (coloredArrow)
                    {
                        arrow = $"<color={seer.GetRoleColorCode()}>{arrow}</color>";
                    }
                }
                if (oldArrow != arrow)
                {
                    Main.targetArrows[key] = arrow;
                    updateFlag = true;
                }
                return updateFlag;
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
    class PlayerStartPatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            var roleText = UnityEngine.Object.Instantiate(__instance.cosmetics.nameText);
            roleText.transform.SetParent(__instance.cosmetics.nameText.transform);
            roleText.transform.localPosition = new Vector3(0f, 0.175f, 0f);
            roleText.fontSize = 0.55f;
            roleText.text = "RoleText";
            roleText.gameObject.name = "RoleText";
            roleText.enabled = false;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetColor))]
    class SetColorPatch
    {
        public static bool IsAntiGlitchDisabled = false;
        public static bool Prefix(PlayerControl __instance, int bodyColor)
        {
            //色変更バグ対策
            if (!AmongUsClient.Instance.AmHost || __instance.CurrentOutfit.ColorId == bodyColor || IsAntiGlitchDisabled) return true;
            if (AmongUsClient.Instance.IsGameStarted && Options.CurrentGameMode() == CustomGameMode.HideAndSeek && !Options.SplatoonOn.GetBool())
            {
                __instance.RpcMurderPlayer(__instance);
                if (Options.AutoKick.GetBool())
                {
                    AmongUsClient.Instance.KickPlayer(__instance.GetClientId(), Options.BanInsteadOfKick.GetBool());
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Vent), nameof(Vent.EnterVent))]
    class EnterVentPatch
    {
        public static void Postfix(Vent __instance, [HarmonyArgument(0)] PlayerControl pc)
        {
            if (AmongUsClient.Instance.AmHost)
            {
                bool skipCheck = false;
                if (Main.LastEnteredVent.ContainsKey(pc.PlayerId))
                    Main.LastEnteredVent.Remove(pc.PlayerId);
                Main.LastEnteredVent.Add(pc.PlayerId, __instance);
                if (Main.LastEnteredVentLocation.ContainsKey(pc.PlayerId))
                    Main.LastEnteredVentLocation.Remove(pc.PlayerId);
                Main.LastEnteredVentLocation.Add(pc.PlayerId, pc.GetTruePosition());
                if (Options.CurrentGameMode() == CustomGameMode.HideAndSeek)
                    if (Options.SplatoonOn.GetBool())
                    {
                        if (!Options.STIgnoreVent.GetBool())
                        {
                            pc?.MyPhysics?.RpcBootFromVent(__instance.Id);
                        }
                    }
                if (CustomRoles.TheGlitch.IsEnable() | CustomRoles.Escort.IsEnable() | CustomRoles.Consort.IsEnable())
                {
                    List<byte> hackedPlayers = new();
                    foreach (var cp in Main.CursedPlayers)
                    {
                        if (cp.Value == null) continue;
                        if (Utils.GetPlayerById(cp.Key).GetCustomRole().CanRoleBlock())
                        {
                            var player = Utils.GetPlayerById(cp.Key);
                            if (player.Is(CustomRoles.TheGlitch) && Options.GlitchCanVent.GetBool())
                                hackedPlayers.Add(cp.Value.PlayerId);
                            if (player.Is(CustomRoles.Escort) | player.Is(CustomRoles.Consort) && Options.EscortPreventsVent.GetBool())
                                hackedPlayers.Add(cp.Value.PlayerId);
                        }
                    }
                    if (hackedPlayers.Contains(pc.PlayerId))
                    {
                        pc?.MyPhysics?.RpcBootFromVent(__instance.Id);
                        skipCheck = true;
                    }
                }
                if (pc.Is(CustomRoles.Grenadier))
                {
                    if (!Options.GrenadierCanVent.GetBool())
                    {
                        skipCheck = true;
                        pc.MyPhysics.RpcBootFromVent(__instance.Id);
                    }
                }
                if (pc.Is(CustomRoles.Escapist))
                {
                    if (!Escapist.CanVent())
                    {
                        skipCheck = true;
                        pc.MyPhysics.RpcBootFromVent(__instance.Id);
                    }
                }
                if (pc.Is(CustomRoles.Camouflager))
                {
                    if (!Camouflager.CanVent())
                    {
                        skipCheck = true;
                        pc.MyPhysics.RpcBootFromVent(__instance.Id);
                    }
                }
                if (pc.Is(CustomRoles.Medium))
                {
                    skipCheck = true;
                    pc.MyPhysics.RpcBootFromVent(__instance.Id);
                    foreach (var pcplayer in PlayerControl.AllPlayerControls)
                    {
                        if (pcplayer == null || pcplayer.Data.Disconnected || !pcplayer.Data.IsDead) continue;
                        if (!Main.DeadPlayersThisRound.Contains(pcplayer.PlayerId)) continue;
                        if (Main.knownGhosts[pc.PlayerId].Contains(pc.PlayerId)) continue;
                        Main.knownGhosts[pc.PlayerId].Add(pc.PlayerId);
                    }
                }
                if (Options.CurrentGameMode() == CustomGameMode.HideAndSeek && Options.IgnoreVent.GetBool() && !Options.SplatoonOn.GetBool())
                    pc.MyPhysics.RpcBootFromVent(__instance.Id);
                if (pc.Is(CustomRoles.Mayor))
                {
                    if (Main.MayorUsedButtonCount.TryGetValue(pc.PlayerId, out var count) && count < Options.MayorNumOfUseButton.GetInt())
                    {
                        pc?.CmdReportDeadBody(null);
                    }
                    pc?.MyPhysics?.RpcBootFromVent(__instance.Id);
                    skipCheck = true;
                }
                if (pc.Is(CustomRoles.Necromancer))
                {
                    Necromancer.OnUseVent(__instance.Id);
                    skipCheck = true;
                }
                if (pc.Is(CustomRoles.Survivor))
                {
                    pc.SurvivorVested();
                    pc?.MyPhysics?.RpcBootFromVent(__instance.Id);
                    skipCheck = true;
                }

                if (pc.PlayerId == Bomber.CurrentBombedPlayer && Bomber.TargetIsRoleBlocked)
                {
                    pc?.MyPhysics?.RpcBootFromVent(__instance.Id);
                    skipCheck = true;
                }

                if (pc.Is(CustomRoles.Arsonist) && Options.TOuRArso.GetBool())
                {
                    skipCheck = true;
                    pc?.MyPhysics?.RpcBootFromVent(__instance.Id);
                    List<PlayerControl> doused = new();
                    var statiscs = new CheckGameEndPatch.PlayerStatistics();
                    foreach (var player in PlayerControl.AllPlayerControls)
                    {
                        if (player == null ||
                            player.Data.IsDead ||
                            player.Data.Disconnected ||
                            player.Is(CustomRoles.Phantom) ||
                            player.Is(CustomRoles.Pestilence) ||
                            player.PlayerId == pc.PlayerId
                        ) continue;

                        if (pc.IsDousedPlayer(player))
                            doused.Add(player);
                    }
                    var skip = false;
                    if (statiscs.TotalAlive < Options.MinDouseToInginite.GetInt())
                        skip = true;
                    if (doused.Count != 0 && doused.Count >= Options.MinDouseToInginite.GetInt() | skip)
                    {
                        foreach (var pcd in doused)
                        {
                            if (pcd == null || pcd.Data.IsDead || pcd.Data.Disconnected) continue;
                            if (pcd.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                            {
                                pcd.RpcMurderPlayer(pc);
                                PlayerState.SetDeathReason(pc.PlayerId, PlayerState.DeathReason.Kill);
                                PlayerState.SetDead(pc.PlayerId);
                                break;
                            }
                            if (!pcd.Is(CustomRoles.Pestilence))
                            {
                                if (!Main.GuardianAngelTarget.ContainsValue(pcd.PlayerId))
                                {
                                    pcd.RpcMurderPlayer(pcd);
                                    PlayerState.SetDeathReason(pcd.PlayerId, PlayerState.DeathReason.Torched);
                                    PlayerState.SetDead(pcd.PlayerId);
                                    Main.KillCount[pc.PlayerId] += 1;
                                    switch (pcd.GetCustomSubRole())
                                    {
                                        case CustomRoles.Bait:
                                            Logger.Info(pcd?.Data?.PlayerName + " Bait Self Report", "MurderPlayer");
                                            new LateTask(() => pc.CmdReportDeadBody(pcd.Data), 0.15f, "Bait Self Report");
                                            break;
                                        case CustomRoles.Bewilder:
                                            Main.KilledBewilder.Add(pc.PlayerId);
                                            pc.CustomSyncSettings();
                                            break;
                                        case CustomRoles.Diseased:
                                            Main.KilledDiseased.Add(pc.PlayerId);
                                            pc.ResetKillCooldown();
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                else
                                {
                                    if (!Main.IsProtected)
                                    {
                                        pcd.RpcMurderPlayer(pcd);
                                        PlayerState.SetDeathReason(pcd.PlayerId, PlayerState.DeathReason.Torched);
                                        PlayerState.SetDead(pcd.PlayerId);
                                        Main.KillCount[pc.PlayerId] += 1;
                                        switch (pcd.GetCustomSubRole())
                                        {
                                            case CustomRoles.Bait:
                                                Logger.Info(pcd?.Data?.PlayerName + " Bait Self Report", "MurderPlayer");
                                                new LateTask(() => pc.CmdReportDeadBody(pcd.Data), 0.15f, "Bait Self Report");
                                                break;
                                            case CustomRoles.Bewilder:
                                                Main.KilledBewilder.Add(pc.PlayerId);
                                                pc.CustomSyncSettings();
                                                break;
                                            case CustomRoles.Diseased:
                                                Main.KilledDiseased.Add(pc.PlayerId);
                                                pc.ResetKillCooldown();
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                        RPC.PlaySoundRPC(pc.PlayerId, Sounds.KillSound);
                    }
                }
                if (pc.Is(CustomRoles.Swooper))
                {
                    if (!Main.IsInvis && Main.CanGoInvis)
                        skipCheck = true;
                    if (Main.IsInvis)
                    {
                        pc.MyPhysics.RpcBootFromVent(__instance.Id);
                        Main.IsInvis = false;
                        Utils.NotifyRoles(SpecifySeer: pc);
                        new LateTask(() =>
                        {
                            Main.CanGoInvis = true;
                            Utils.NotifyRoles(SpecifySeer: pc);
                        },
                        Options.SwooperCooldown.GetFloat(), "SwooperCooldown", true);
                    }
                }

                if (pc.Is(CustomRoles.TheGlitch))
                {
                    pc.MyPhysics.RpcBootFromVent(__instance.Id);
                    skipCheck = true;
                    if (Options.UseVentButtonInsteadOfPet.GetBool())
                    {
                        Main.IsHackMode = !Main.IsHackMode;
                        MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetGlitchState, Hazel.SendOption.Reliable, -1);
                        writer2.Write(Main.IsHackMode);
                        AmongUsClient.Instance.FinishRpcImmediately(writer2);
                        Utils.NotifyRoles();
                    }
                }

                if (pc.Is(CustomRoles.Jester) && !Options.JesterCanVent.GetBool())
                {
                    pc.MyPhysics.RpcBootFromVent(__instance.Id);
                    skipCheck = true;
                }
                if (pc.Is(CustomRoles.Veteran) && Options.UseVentButtonInsteadOfPet.GetBool())
                {
                    skipCheck = true;
                    pc.MyPhysics.RpcBootFromVent(__instance.Id);
                    if (!Main.VetIsAlerted && Main.VetCanAlert && Main.VetAlerts != Options.NumOfVets.GetInt())
                    {
                        pc.VetAlerted();
                        Utils.NotifyRoles(GameStates.IsMeeting, pc);
                    }
                }
                if (pc.Is(CustomRoles.Transporter) && Options.UseVentButtonInsteadOfPet.GetBool())
                {
                    skipCheck = true;
                    pc.MyPhysics.RpcBootFromVent(__instance.Id);
                    if (Main.TransportsLeft != 0 && Main.CanTransport)
                    {
                        Main.CanTransport = false;
                        PlayerControl TP1 = null;
                        PlayerControl TP2 = null;
                        DeadBody Player1Body = null;
                        DeadBody Player2Body = null;
                        List<PlayerControl> canTransport = new();
                        // GET TRANSPORTABLE PLAYERS //
                        foreach (var pcplayer in PlayerControl.AllPlayerControls)
                        {
                            if (pcplayer == null || pcplayer.Data.Disconnected || !pcplayer.CanMove || pcplayer.Data.IsDead) continue;
                            canTransport.Add(pcplayer);
                        }
                        // GET 2 RANDOM PLAYERS //
                        var rando = new System.Random();
                        var player = canTransport[rando.Next(0, canTransport.Count)];
                        TP1 = player;
                        canTransport.Remove(player);
                        var random = new System.Random();
                        var newplayer = canTransport[rando.Next(0, canTransport.Count)];
                        TP2 = newplayer;
                        canTransport.Remove(newplayer);
                        // MAKE SURE THEY AREN'T NULL (PREVENTS CRASHES WITH JUST 1 PLAYER) //
                        if (TP1 != null && TP2 != null)
                        {
                            Main.TransportsLeft--;
                            // IF PLAYER IS DEAD, WE TRANSPORT BODY INSTEAD //
                            if (TP1.Data.IsDead)
                                foreach (DeadBody body in GameObject.FindObjectsOfType<DeadBody>())
                                    if (body.ParentId == TP1.PlayerId)
                                        Player1Body = body;
                            if (TP2.Data.IsDead)
                                foreach (DeadBody body in GameObject.FindObjectsOfType<DeadBody>())
                                    if (body.ParentId == TP2.PlayerId)
                                        Player2Body = body;

                            // EXIT THEM OUT OF VENT //
                            if (TP1.inVent)
                                TP1.MyPhysics.ExitAllVents();
                            if (TP2.inVent)
                                TP2.MyPhysics.ExitAllVents();
                            if (Player1Body == null && Player2Body == null)
                            {
                                TP1.MyPhysics.ResetMoveState();
                                TP2.MyPhysics.ResetMoveState();
                                var TempPosition = TP1.GetTruePosition();
                                Utils.TP(TP1.NetTransform, new Vector2(TP2.GetTruePosition().x, TP2.GetTruePosition().y + 0.3636f));
                                Utils.TP(TP2.NetTransform, new Vector2(TempPosition.x, TempPosition.y + 0.3636f));
                            }
                            else if (Player1Body != null && Player2Body == null)
                            {
                                TP2.MyPhysics.ResetMoveState();
                                var TempPosition = Player1Body.TruePosition;
                                Player1Body.transform.position = TP2.GetTruePosition();
                                Utils.TP(TP2.NetTransform, new Vector2(TempPosition.x, TempPosition.y + 0.3636f));
                            }
                            else if (Player1Body == null && Player2Body != null)
                            {
                                TP1.MyPhysics.ResetMoveState();
                                var TempPosition = TP1.GetTruePosition();
                                Utils.TP(TP1.NetTransform, new Vector2(Player2Body.TruePosition.x, Player2Body.TruePosition.y + 0.3636f));
                                Player2Body.transform.position = TempPosition;
                            }
                            else if (Player1Body != null && Player2Body != null)
                            {
                                var TempPosition = Player1Body.TruePosition;
                                Player1Body.transform.position = Player2Body.TruePosition;
                                Player2Body.transform.position = TempPosition;
                            }

                            TP1.moveable = true;
                            TP2.moveable = true;
                            TP1.Collider.enabled = true;
                            TP2.Collider.enabled = true;
                            TP1.NetTransform.enabled = true;
                            TP2.NetTransform.enabled = true;
                            MessageWriter writer3 = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetTransportState, Hazel.SendOption.Reliable, -1);
                            writer3.Write(Main.CanTransport);
                            AmongUsClient.Instance.FinishRpcImmediately(writer3);
                            new LateTask(() =>
                            {
                                if (!GameStates.IsMeeting)
                                {
                                    Main.CanTransport = true;
                                    MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetTransportState, Hazel.SendOption.Reliable, -1);
                                    writer2.Write(Main.CanTransport);
                                    AmongUsClient.Instance.FinishRpcImmediately(writer2);
                                    Utils.NotifyRoles();
                                }
                            }, Options.TransportCooldown.GetFloat(), "Transporter Transport Cooldown (Pet Button)", true);
                        }
                    }
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetTransportNumber, Hazel.SendOption.Reliable, -1);
                    writer.Write(Main.TransportsLeft);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    pc.CustomSyncSettings();
                }
                if (pc.Is(CustomRoles.Bastion))
                {
                    if (Main.bombedVents.Contains(__instance.Id) && !skipCheck)
                    {
                        if (!pc.Is(CustomRoles.Pestilence))
                        {
                            pc.MyPhysics.RpcBootFromVent(__instance.Id);
                            pc.RpcMurderPlayer(pc);
                            PlayerState.SetDeathReason(pc.PlayerId, PlayerState.DeathReason.Bombed);
                            PlayerState.SetDead(pc.PlayerId);
                            if (Options.BastionVentsRemoveOnBomb.GetBool())
                                Main.bombedVents.Remove(__instance.Id);
                            skipCheck = true;
                        }
                    }
                    else
                    {
                        pc.MyPhysics.RpcBootFromVent(__instance.Id);
                        skipCheck = true;
                        Main.bombedVents.Add(__instance.Id);
                    }
                }
                if (Main.bombedVents.Contains(__instance.Id) && !skipCheck)
                {
                    if (!pc.Is(CustomRoles.Pestilence))
                    {
                        pc.MyPhysics.RpcBootFromVent(__instance.Id);
                        pc.RpcMurderPlayer(pc);
                        PlayerState.SetDeathReason(pc.PlayerId, PlayerState.DeathReason.Bombed);
                        PlayerState.SetDead(pc.PlayerId);
                        if (Options.BastionVentsRemoveOnBomb.GetBool())
                            Main.bombedVents.Remove(__instance.Id);
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoEnterVent))]
    class CoEnterVentPatch
    {
        public static bool Prefix(PlayerPhysics __instance, [HarmonyArgument(0)] int id)
        {
            if (AmongUsClient.Instance.AmHost)
            {
                if (AmongUsClient.Instance.IsGameStarted &&
                    __instance.myPlayer.IsDouseDone())
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (!pc.Data.IsDead)
                        {
                            if (pc != __instance.myPlayer && !pc.Is(CustomRoles.Pestilence))
                            {
                                //生存者は焼殺
                                //if (!pc.Is(CustomRoles.Pestilence))
                                pc.RpcMurderPlayer(pc);
                                PlayerState.SetDeathReason(pc.PlayerId, PlayerState.DeathReason.Torched);
                                PlayerState.SetDead(pc.PlayerId);
                            }
                            else
                                RPC.PlaySoundRPC(pc.PlayerId, Sounds.KillSound);
                        }
                    }
                    return true;
                }
                if (__instance.myPlayer.Is(CustomRoles.Sheriff) ||
                __instance.myPlayer.Is(CustomRoles.Investigator) ||
                __instance.myPlayer.Is(CustomRoles.Escort) ||
                __instance.myPlayer.Is(CustomRoles.Crusader) ||
                __instance.myPlayer.Is(CustomRoles.SKMadmate) ||
                (__instance.myPlayer.Is(CustomRoles.Jester) && !Options.JesterCanVent.GetBool()) ||
                __instance.myPlayer.Is(CustomRoles.Executioner) ||
                __instance.myPlayer.Is(CustomRoles.Swapper) ||
                __instance.myPlayer.Is(CustomRoles.Opportunist) ||
                __instance.myPlayer.Is(CustomRoles.NeutWitch) ||
                __instance.myPlayer.Is(CustomRoles.Amnesiac) ||
                __instance.myPlayer.Is(CustomRoles.AgiTater) ||
                __instance.myPlayer.Is(CustomRoles.PoisonMaster) ||
                __instance.myPlayer.Is(CustomRoles.TheGlitch) && !Options.UseVentButtonInsteadOfPet.GetBool() ||
                (__instance.myPlayer.Is(CustomRoles.Arsonist) && !Options.TOuRArso.GetBool()) ||
                __instance.myPlayer.Is(CustomRoles.PlagueBearer) ||
                (__instance.myPlayer.Is(CustomRoles.Juggernaut) && !Options.JuggerCanVent.GetBool()) ||
                (__instance.myPlayer.Is(CustomRoles.CovenWitch) && !Main.HasNecronomicon) || (__instance.myPlayer.Is(CustomRoles.HexMaster) && !Main.HasNecronomicon) ||
                (__instance.myPlayer.Is(CustomRoles.Mayor) && Main.MayorUsedButtonCount.TryGetValue(__instance.myPlayer.PlayerId, out var count) && count >= Options.MayorNumOfUseButton.GetInt()) ||
                (__instance.myPlayer.Is(CustomRoles.Jackal) && !Options.JackalCanVent.GetBool()) ||
                (__instance.myPlayer.Is(CustomRoles.Hitman) && !Options.HitmanCanVent.GetBool()) ||
                (__instance.myPlayer.Is(CustomRoles.Marksman) && !Options.MarksmanCanVent.GetBool()) ||
                (__instance.myPlayer.Is(CustomRoles.BloodKnight) && !Options.BKcanVent.GetBool()) ||
                (__instance.myPlayer.Is(CustomRoles.Pestilence) && !Options.PestiCanVent.GetBool())
                )
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.BootFromVent, SendOption.Reliable, -1);
                    writer.WritePacked(127);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    new LateTask(() =>
                    {
                        int clientId = __instance.myPlayer.GetClientId();
                        MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.BootFromVent, SendOption.Reliable, clientId);
                        writer2.Write(id);
                        AmongUsClient.Instance.FinishRpcImmediately(writer2);
                    }, 0.5f, "Fix DesyncImpostor Stuck");
                    return false;
                }
                if (__instance.myPlayer.GetRoleType() == RoleType.Coven && !Main.HasNecronomicon && !__instance.myPlayer.Is(CustomRoles.Mimic))
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.BootFromVent, SendOption.Reliable, -1);
                    writer.WritePacked(127);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    new LateTask(() =>
                    {
                        int clientId = __instance.myPlayer.GetClientId();
                        MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.BootFromVent, SendOption.Reliable, clientId);
                        writer2.Write(id);
                        AmongUsClient.Instance.FinishRpcImmediately(writer2);
                    }, 0.5f, "Fix DesyncImpostor Stuck");
                    return false;
                }
                if (__instance.myPlayer.Is(CustomRoles.Swooper))
                {
                    _ = new LateTask(() =>
                    {
                        if (!Main.IsInvis && Main.CanGoInvis)
                        {
                            MessageWriter writer3 = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, 34, SendOption.Reliable, (int)__instance.myPlayer.GetClientId());
                            writer3.WritePacked(id);
                            AmongUsClient.Instance.FinishRpcImmediately(writer3);

                            Main.IsInvis = true;
                            Main.CanGoInvis = false;
                            Utils.NotifyRoles(SpecifySeer: __instance.myPlayer);
                            new LateTask(() =>
                            {
                                __instance?.myPlayer?.MyPhysics?.RpcBootFromVent(id);
                                if (Main.IsInvis)
                                new LateTask(() =>
                                {
                                    Main.CanGoInvis = true;
                                    Utils.NotifyRoles(SpecifySeer: __instance.myPlayer);
                                },
                                Options.SwooperCooldown.GetFloat(), "SwooperCooldown", true);
                                Main.IsInvis = false;
                                Utils.NotifyRoles(SpecifySeer: __instance.myPlayer);
                            },
                            Options.SwooperDuration.GetFloat(), "SwooperDuration", true);
                        }
                        else
                        {
                            __instance.myPlayer.MyPhysics.RpcBootFromVent(id);
                        }
                        Utils.NotifyRoles(NoCache: true);
                    },
                    0.5f, "SwooperVent");
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetName))]
    class SetNamePatch
    {
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] string name)
        {
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
    class PlayerControlCompleteTaskPatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            var pc = __instance;
            Logger.Info($"TaskComplete:{pc.PlayerId}", "CompleteTask");
            PlayerState.UpdateTask(pc);
            Utils.NotifyRoles();
            if (pc.GetPlayerTaskState().IsTaskFinished &&
                pc.GetCustomRole() is CustomRoles.Lighter or CustomRoles.SpeedBooster or CustomRoles.Doctor || Main.KilledBewilder.Contains(pc.PlayerId))
            {
                Utils.CustomSyncAllSettings();
            }

        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ProtectPlayer))]
    class PlayerControlProtectPlayerPatch
    {
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            Logger.Info($"{__instance.GetNameWithRole()} => {target.GetNameWithRole()}", "ProtectPlayer");
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RemoveProtection))]
    class PlayerControlRemoveProtectionPatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            Logger.Info($"{__instance.GetNameWithRole()}", "RemoveProtection");
        }
    }
}
