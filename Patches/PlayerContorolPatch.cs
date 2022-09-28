using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using HarmonyLib;
using Hazel;
using UnityEngine;
using static TownOfHost.Translator;
using Object = UnityEngine.Object;

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
                Logger.Info("HideAndSeekの待機時間中だったため、キルをキャンセルしました。", "CheckMurder");
                return false;
            }

            //キル可能判定
            if (killer.PlayerId != target.PlayerId)
            {
                switch (killer.GetCustomRole())
                {
                    //==========インポスター役職==========//
                    case CustomRoles.Mafia:
                        if (!killer.CanUseKillButton())
                        {
                            Logger.Info(killer?.Data?.PlayerName + "はMafiaだったので、キルはキャンセルされました。", "CheckMurder");
                            return false;
                        }
                        else
                        {
                            Logger.Info(killer?.Data?.PlayerName + "はMafiaですが、他のインポスターがいないのでキルが許可されました。", "CheckMurder");
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
                    case CustomRoles.Mare:
                        if (!killer.CanUseKillButton())
                            return false;
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
                    case CustomRoles.Pestilence:
                        break;
                }
            }


            //キルされた時の特殊判定
            switch (target.GetCustomRole())
            {
                case CustomRoles.SchrodingerCat:
                    var canDirectKill = !killer.Is(CustomRoles.Arsonist) && !killer.Is(CustomRoles.PlagueBearer) && !killer.Is(CustomRoles.HexMaster) && !killer.IsHexMode() && !killer.Is(CustomRoles.Investigator);
                    if (canDirectKill)
                    {
                        if (killer.Is(CustomRoles.Arsonist)) break;
                        if (killer.Is(CustomRoles.PlagueBearer)) break;
                        if (killer.Is(CustomRoles.Investigator)) break;
                        if (killer.Is(CustomRoles.HexMaster) && !killer.IsHexMode()) break;
                        killer.RpcGuardAndKill(target);
                        if (PlayerState.GetDeathReason(target.PlayerId) == PlayerState.DeathReason.Sniped)
                        {
                            //スナイプされた時
                            target.RpcSetCustomRole(CustomRoles.MSchrodingerCat);
                            var sniperId = Sniper.GetSniper(target.PlayerId);
                            NameColorManager.Instance.RpcAdd(sniperId, target.PlayerId, $"{Utils.GetRoleColorCode(CustomRoles.SchrodingerCat)}");
                        }
                        else if (BountyHunter.GetTarget(killer) == target)
                            BountyHunter.ResetTarget(killer);
                        else
                        {
                            SerialKiller.OnCheckMurder(killer, isKilledSchrodingerCat: true);
                            if (killer.GetCustomRole().IsImpostor())
                                target.RpcSetCustomRole(CustomRoles.MSchrodingerCat);
                            if (killer.Is(CustomRoles.Sheriff))
                                target.RpcSetCustomRole(CustomRoles.CSchrodingerCat);
                            if (killer.Is(CustomRoles.Egoist))
                                target.RpcSetCustomRole(CustomRoles.EgoSchrodingerCat);
                            if (killer.Is(CustomRoles.Jackal) || killer.Is(CustomRoles.Sidekick))
                                target.RpcSetCustomRole(CustomRoles.JSchrodingerCat);
                            if (killer.Is(CustomRoles.Pestilence))
                            {
                                //pesti cat.
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
                        pc.RpcShapeshift(pc, true);
                        return false;
                    }
                    break;
            }

            //Main.whoKilledWho.Add(target, killer);

            //キル時の特殊判定
            if (killer.PlayerId != target.PlayerId)
            {
                //自殺でない場合のみ役職チェック
                if (CustomRoles.TheGlitch.IsEnable())
                {
                    List<byte> hackedPlayers = new();
                    foreach (var cp in Main.CursedPlayers)
                    {
                        if (cp.Value == null) continue;
                        if (Utils.GetPlayerById(cp.Key).Is(CustomRoles.TheGlitch))
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
                switch (killer.GetCustomRole())
                {
                    //==========インポスター役職==========//
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
                        target.RpcShapeshift(target, true);
                        return false;
                    case CustomRoles.Janitor:
                        int startingColorId = Main.AllPlayerSkin[target.PlayerId].Item1;
                        if (target.CurrentOutfit.ColorId == startingColorId) return false;
                        killer.RpcGuardAndKill(target);
                        killer.ResetKillCooldown();
                        target.SetColor(startingColorId);
                        target.RpcRevertShapeshift(true);
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
                            //killer.RpcGuardAndKill(target);
                            return false;
                        }
                        if (Main.isCurseAndKill[killer.PlayerId]) killer.RpcGuardAndKill(target);
                        return false;
                    //break;
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
                            killer.RpcMurderPlayer(target);
                            return false;
                        }
                        else
                        {
                            return false;
                        }
                        break;
                    case CustomRoles.Amnesiac:
                        return false;
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
                        if (Main.MarksmanKills != 3)
                            Main.MarksmanKills++;
                        killer.CustomSyncSettings();
                        break;
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
                    case CustomRoles.Poisoner:
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
                            break;
                        }
                        else if (Main.SilencedPlayer.Count <= 0)
                        {
                            Main.firstKill.Add(killer.PlayerId);
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
                        Main.MayorUsedButtonCount[killer.PlayerId]++;
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
                                Main.ColliderPlayers.Add(target);
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
                        Sheriff.OnCheckMurder(killer, target, Process: "RemoveShotLimit");

                        if (!Sheriff.OnCheckMurder(killer, target, Process: "Suicide"))
                            return false;
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
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SeeredPlayer, Hazel.SendOption.Reliable, -1);
                        writer.Write(target.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
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
                else if (killer.Is(CustomRoles.Pestilence))
                {
                    //so ARSONIST, CHILD, TERRORIST
                    killer.RpcMurderPlayer(target);
                    //but IDC WHO IT IS PESTI DYING
                }
                else if (target.Is(CustomRoles.Veteran) && Main.VetIsAlerted)
                {
                    if (killer.Is(CustomRoles.Pestilence))
                    {
                        switch (Options.PestiAttacksVet.GetString())
                        {
                            case "Trade":
                                killer.RpcMurderPlayer(killer);
                                target.RpcMurderPlayer(target);
                                break;
                            case "VetKillsPesti":
                                target.RpcMurderPlayer(killer);
                                break;
                            case "PestiKillsVet":
                                killer.RpcMurderPlayer(target);
                                break;
                        }
                    }
                    else
                    {
                        target.RpcMurderPlayer(killer);
                    }
                }
                else
                    target.RpcMurderPlayer(killer);
            }
            //============

            return false;
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
            else if (target.CurrentlyLastImpostor())
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
                                    if (pc.Is(CustomRoles.Sheriff) || pc.Is(CustomRoles.Investigator))
                                        couldBeTraitors.Add(pc);
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
                                if (Sheriff.seer.IsModClient())
                                    RoleManager.Instance.SetRole(Sheriff.seer, RoleTypes.Impostor);
                            }
                        }
                    }
                }
            }
            else
            //Pestilence
            if (target.Is(CustomRoles.Pestilence) && killer.PlayerId != target.PlayerId)
            {
                target.RpcMurderPlayer(killer);
                //PestiLince cannot die.
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
                    if (target.PlayerId == ExecutionerTarget.Value && !executioner.Data.IsDead)
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

            if (Main.ColliderPlayers.Contains(target) && CustomRoles.YingYanger.IsEnable() && Options.ResetToYinYang.GetBool())
            {
                Main.DoingYingYang = false;
            }
            if (Main.ColliderPlayers.Contains(target))
                Main.ColliderPlayers.Remove(target);

            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.Data.Disconnected) continue;
                if (pc.IsLastImpostor())
                    Main.AllPlayerKillCooldown[pc.PlayerId] = Options.LastImpostorKillCooldown.GetFloat();
                if (pc.Data.IsDead) continue;
                if (pc.Is(CustomRoles.Mystic))
                {
                    pc.DoMysticStuff(0.1f);
                }
            }
            FixedUpdatePatch.LoversSuicide(target.PlayerId);

            PlayerState.SetDead(target.PlayerId);
            Utils.CountAliveImpostors();
            Utils.CustomSyncAllSettings();
            Utils.NotifyRoles();
            Main.whoKilledWho.Add(target, killer);
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
            if (shapeshifter.Is(CustomRoles.Warlock))
            {
                if (Main.CursedPlayers[shapeshifter.PlayerId] != null)//呪われた人がいるか確認
                {
                    if (shapeshifting && !Main.CursedPlayers[shapeshifter.PlayerId].Data.IsDead)//変身解除の時に反応しない
                    {
                        var cp = Main.CursedPlayers[shapeshifter.PlayerId];
                        Vector2 cppos = cp.transform.position;//呪われた人の位置
                        Dictionary<PlayerControl, float> cpdistance = new();
                        float dis;
                        foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                        {
                            if (!p.Data.IsDead && p != cp)
                            {
                                dis = Vector2.Distance(cppos, p.transform.position);
                                cpdistance.Add(p, dis);
                                Logger.Info($"{p?.Data?.PlayerName}の位置{dis}", "Warlock");
                            }
                        }
                        var min = cpdistance.OrderBy(c => c.Value).FirstOrDefault();//一番小さい値を取り出す
                        PlayerControl targetw = min.Key;
                        Logger.Info($"{targetw.GetNameWithRole()}was killed", "Warlock");
                        if (target.Is(CustomRoles.Pestilence))
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
                        shapeshifter.RpcGuardAndKill(shapeshifter);
                        Main.isCurseAndKill[shapeshifter.PlayerId] = false;
                    }
                    Main.CursedPlayers[shapeshifter.PlayerId] = null;
                }
            }
            if (shapeshifter.Is(CustomRoles.Miner) && shapeshifting)
            {
                if (Main.LastEnteredVent.ContainsKey(shapeshifter.PlayerId))
                {
                    int ventId = Main.LastEnteredVent[shapeshifter.PlayerId];
                    shapeshifter.NetTransform.RpcSnapTo(Main.LastEnteredVentLocation[shapeshifter.PlayerId]);
                    //shapeshifter?.MyPhysics?.RpcEnterVent(ventId);
                    //shapeshifter.MyPhysics.
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
                    if (targetm.Is(CustomRoles.Sheriff))
                        targetm.RpcSetCustomRole(CustomRoles.CorruptedSheriff);
                    else if (targetm.Is(CustomRoles.Investigator))
                        targetm.RpcSetCustomRole(CustomRoles.CorruptedSheriff);
                    else
                        targetm.RpcSetCustomRole(CustomRoles.SKMadmate);
                    Logger.Info($"Make SKMadmate:{targetm.name}", "Shapeshift");
                    Main.SKMadmateNowCount++;
                    Utils.CustomSyncAllSettings();
                    Utils.NotifyRoles();
                }
            }
            if (shapeshifter.Is(CustomRoles.Grenadier)) Camouflague.Grenade(shapeshifting);
            if (shapeshifter.Is(CustomRoles.FireWorks)) FireWorks.ShapeShiftState(shapeshifter, shapeshifting);
            if (shapeshifter.Is(CustomRoles.Sniper)) Sniper.ShapeShiftCheck(shapeshifter, shapeshifting);
            if (shapeshifter.Is(CustomRoles.Ninja) && !shapeshifter.Data.IsDead && Main.MercCanSuicide) Ninja.ShapeShiftCheck(shapeshifter, shapeshifting);
            if (shapeshifter.Is(CustomRoles.Necromancer)) Necromancer.OnShapeshiftCheck(shapeshifter, shapeshifting);
            if (shapeshifter.Is(CustomRoles.BountyHunter) && !shapeshifting) BountyHunter.ResetTarget(shapeshifter);
            if (shapeshifter.Is(CustomRoles.Camouflager))
            {
                Camouflager.ShapeShiftState(shapeshifter, shapeshifting, target);
            }
            if (shapeshifter.Is(CustomRoles.SerialKiller) && !shapeshifter.Data.IsDead && Main.MercCanSuicide)
            { shapeshifter.RpcMurderPlayer(shapeshifter); Sheriff.SwitchToCorrupt(shapeshifter, shapeshifter); }

            //変身解除のタイミングがずれて名前が直せなかった時のために強制書き換

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
            Logger.Info($"{__instance.GetNameWithRole()} => {target?.GetNameWithRole() ?? "null"}", "ReportDeadBody");
            if (target != null)
            {
                if (Main.unreportableBodies.Contains(target.PlayerId)) return false;
            }
            if (target != null)
            {
                if (__instance.Is(CustomRoles.Vulture) && !__instance.Data.IsDead && !Main.unreportableBodies.Contains(target.PlayerId))
                {
                    Main.unreportableBodies.Add(target.PlayerId);
                    Main.AteBodies++;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetVultureAmount, Hazel.SendOption.Reliable, -1);
                    writer.Write(Main.AteBodies);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    foreach (DeadBody deadBody in GameObject.FindObjectsOfType<DeadBody>())
                    {
                        if (deadBody.ParentId == target.Object.PlayerId) Object.Destroy(deadBody.gameObject);
                    }
                    if (Main.AteBodies == Options.BodiesAmount.GetFloat())
                    {
                        //Vulture wins.
                        //CheckGameEndPatch.CheckAndEndGameForVultureWin();
                        //RPC.VultureWin();
                        //CheckForEndVotingPatch.Prefix();
                        return true;
                    }
                    return false;
                }
            }
            if (target != null) //ボタン
            {
                if (__instance.Is(CustomRoles.Oblivious))
                {
                    return false;
                }
            }
            if (CustomRoles.TheGlitch.IsEnable())
            {
                List<byte> hackedPlayers = new();
                foreach (var cp in Main.CursedPlayers)
                {
                    if (cp.Value == null) continue;
                    if (Utils.GetPlayerById(cp.Key).Is(CustomRoles.TheGlitch))
                    {
                        hackedPlayers.Add(cp.Value.PlayerId);
                    }
                }
                if (hackedPlayers.Contains(__instance.PlayerId))
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
                    reviving.Data.IsDead = false;
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

            if (target != null) //Medium Report for Non-Buttons
            {
                if (__instance.Is(CustomRoles.Medium))
                {
                    var didKill = false;
                    foreach (var killed in Main.whoKilledWho)
                    {
                        if (killed.Key == target.Object)
                            didKill = true;
                    }
                    if (didKill)
                    {
                        var reason = PlayerState.GetDeathReason(target.PlayerId).ToString();
                        var killer = Main.whoKilledWho[target.Object];
                        if (killer != target.Object)
                            Utils.SendMessage("The killer left a clue on their identity. The killer's role was " + Utils.GetRoleName(killer.GetCustomRole()) + ".", __instance.PlayerId);
                        else
                        {
                            if (PlayerState.GetDeathReason(target.PlayerId) == PlayerState.DeathReason.Bombed)
                                Utils.SendMessage("The body was bombed by a Bastion.", __instance.PlayerId);
                            else if (PlayerState.GetDeathReason(target.PlayerId) == PlayerState.DeathReason.Torched)
                                Utils.SendMessage("The body was incinerated by Arsonist.", __instance.PlayerId);
                            else if (PlayerState.GetDeathReason(target.PlayerId) == PlayerState.DeathReason.Misfire)
                                Utils.SendMessage("The body was a misfire from Sheriff.", __instance.PlayerId);
                            else if (PlayerState.GetDeathReason(target.PlayerId) == PlayerState.DeathReason.Sniped)
                                Utils.SendMessage("The body was a sniped by Sniper.", __instance.PlayerId);
                            else if (PlayerState.GetDeathReason(target.PlayerId) == PlayerState.DeathReason.Execution)
                                Utils.SendMessage("This player appears to have been guessed.", __instance.PlayerId);
                            else if (PlayerState.GetDeathReason(target.PlayerId) == PlayerState.DeathReason.Bite)
                                Utils.SendMessage("The body was bitten by Vampire.", __instance.PlayerId);
                            else
                                Utils.SendMessage("The body appears to be a suicide!", __instance.PlayerId);
                        }
                    }
                    else
                    {
                        Utils.SendMessage("The body had no hints on how they died.", __instance.PlayerId);
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
                    Utils.SendMessage("You stole that person's role! They were " + Utils.GetRoleName(target.GetCustomRole()) + ".", __instance.PlayerId);
                    Utils.SendMessage("The Amnesiac stole your role! Because of this, your role has been reset to the default one.", reported.PlayerId);
                    __instance.RpcSetCustomRole(target.GetCustomRole());
                    //__instance.RpcSetRole(target.Role.Role);
                    if (!reported.GetCustomRole().IsNeutralKilling())
                        RoleManager.Instance.SetRole(__instance, reported.Data.Role.Role);
                    else
                        RoleManager.Instance.SetRole(__instance, RoleTypes.Crewmate);

                    __instance.ResetKillCooldown();
                    PlayerControl pc = __instance;
                    if (__instance.Data.Role.Role == RoleTypes.Shapeshifter) Main.CheckShapeshift.Add(pc.PlayerId, false);
                    var rand = new System.Random();

                    switch (target.GetCustomRole())
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
                        case CustomRoles.TheGlitch:
                        case CustomRoles.Warlock:
                            Main.CursedPlayers.Add(pc.PlayerId, null);
                            Main.isCurseAndKill.Add(pc.PlayerId, false);
                            break;
                        case CustomRoles.Veteran:
                            Main.VetAlerts = 0;
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
                        case CustomRoles.Executioner:
                            List<PlayerControl> targetList = new();
                            rand = new System.Random();
                            foreach (var targete in PlayerControl.AllPlayerControls)
                            {
                                if (pc == targete) continue;
                                else if (!Options.ExecutionerCanTargetImpostor.GetBool() && targete.GetCustomRole().IsImpostor()) continue;
                                else if (targete.GetCustomRole().IsNeutral()) continue;
                                else if (targete.Is(CustomRoles.GM)) continue;
                                else if (pc.Is(CustomRoles.Phantom)) continue;

                                targetList.Add(targete);
                            }
                            var Target = targetList[rand.Next(targetList.Count)];
                            Main.ExecutionerTarget.Add(pc.PlayerId, Target.PlayerId);
                            RPC.SendExecutionerTarget(pc.PlayerId, Target.PlayerId);
                            Logger.Info($"{pc.GetNameWithRole()}:{Target.GetNameWithRole()}", "Executioner");
                            break;
                        case CustomRoles.GuardianAngelTOU:
                            List<PlayerControl> protectList = new();
                            rand = new System.Random();
                            foreach (var targete in PlayerControl.AllPlayerControls)
                            {
                                if (pc == targete) continue;
                                else if (targete.Is(CustomRoles.GM)) continue;
                                else if (pc.Is(CustomRoles.Phantom)) continue;

                                protectList.Add(targete);
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
                                    Main.isHexed.Add((pc.PlayerId, ar.PlayerId), false);
                            }
                            break;
                        case CustomRoles.Investigator:
                            Investigator.Add(pc.PlayerId);
                            Investigator.hasSeered.Clear();
                            foreach (var ar in PlayerControl.AllPlayerControls)
                            {
                                Investigator.hasSeered.Add(ar.PlayerId, false);
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
                    }
                    if (!Utils.GetPlayerById(target.PlayerId).GetCustomRole().IsNeutral())
                        Utils.GetPlayerById(target.PlayerId).SetDefaultRole();
                }
            }
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (pc.Data.Disconnected) continue;
                if (pc.Data.IsDead) continue;
                if (pc.Is(CustomRoles.CorruptedSheriff))
                {
                    Utils.SendMessage($"You have betrayed the Crewmates and joined the Impostors team.\nThe names in red are your fellow Impostors. \nKill and sabotage with your new team.", pc.PlayerId);
                }
                if (pc.IsModClient()) continue;
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
            if (!Main.HasNecronomicon)
                Main.CovenMeetings++;
            if (Camouflague.IsActive && Options.CamoComms.GetBool())
            {
                // Camouflague.InMeeting = true;
                //Camouflague.MeetingRevert();
            }
            Guesser.canGuess = true;
            if (Main.CovenMeetings == Options.CovenMeetings.GetFloat() && !Main.HasNecronomicon && CustomRoles.Coven.IsEnable())
            {
                Main.HasNecronomicon = true;
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
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
            Main.ColliderPlayers = new List<PlayerControl>();
            Main.KilledDemo.Clear();
            Main.PuppeteerList.Clear();
            Main.DeadPlayersThisRound.Clear();
            Main.WitchedList.Clear();
            Main.MercCanSuicide = false;
            Sniper.OnStartMeeting();
            Main.VetIsAlerted = false;
            Main.IsRampaged = false;
            Main.RampageReady = false;

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
            {//実行クライアントがホストの場合のみ実行
                if (GameStates.IsLobby && (ModUpdater.hasUpdate || ModUpdater.isBroken) && AmongUsClient.Instance.IsGamePublic)
                    AmongUsClient.Instance.ChangeGamePublic(false);

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

                if (GameStates.IsInGame) LoversSuicide();

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
                            if (target.PlayerId != player.PlayerId && !target.GetCustomRole().IsImpostor())
                            {
                                dis = Vector2.Distance(puppeteerPos, target.transform.position);
                                targetDistance.Add(target.PlayerId, dis);
                            }
                        }
                        if (targetDistance.Count() != 0)
                        {
                            var min = targetDistance.OrderBy(c => c.Value).FirstOrDefault();//一番値が小さい
                            PlayerControl target = Utils.GetPlayerById(min.Key);
                            var KillRange = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
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
                if (GameStates.IsInTask && Main.ColliderPlayers.Contains(player) && !GameStates.IsMeeting)
                {
                    if (!player.IsAlive())
                    {
                        Main.ColliderPlayers.Remove(player);
                    }
                    else
                    {
                        Vector2 puppeteerPos = player.transform.position;
                        Dictionary<byte, float> targetDistance = new();
                        float dis;
                        foreach (var target in Main.ColliderPlayers)
                        {
                            if (!target.IsAlive()) continue;
                            if (target == player) continue;
                            if (target.PlayerId != player.PlayerId && !target.GetCustomRole().IsImpostor())
                            {
                                dis = Vector2.Distance(puppeteerPos, target.transform.position);
                                targetDistance.Add(target.PlayerId, dis);
                            }
                        }
                        if (targetDistance.Count() != 0)
                        {
                            var min = targetDistance.OrderBy(c => c.Value).FirstOrDefault();//一番値が小さい
                            PlayerControl target = Utils.GetPlayerById(min.Key);
                            var KillRange = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
                            if (min.Value <= KillRange && player.CanMove && target.CanMove)
                            {
                                //RPC.PlaySoundRPC(Main.PuppeteerList[player.PlayerId], Sounds.KillSound);
                                if (target.Is(CustomRoles.Pestilence))
                                    target.RpcMurderPlayer(player);
                                else if (player.Is(CustomRoles.Survivor)) { Utils.CheckSurvivorVest(target, player, false); }
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
                            if (target.PlayerId != player.PlayerId && !target.GetCustomRole().IsCoven())
                            {
                                dis = Vector2.Distance(puppeteerPos, target.transform.position);
                                targetDistance.Add(target.PlayerId, dis);
                            }
                        }
                        if (targetDistance.Count() != 0)
                        {
                            var min = targetDistance.OrderBy(c => c.Value).FirstOrDefault();//一番値が小さい
                            PlayerControl target = Utils.GetPlayerById(min.Key);
                            var KillRange = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
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
                if (GameStates.IsInTask && player == PlayerControl.LocalPlayer)
                    DisableDevice.FixedUpdate();

                if (GameStates.IsInGame && Main.RefixCooldownDelay <= 0)
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (/*pc.Is(CustomRoles.Vampire) ||*/ pc.Is(CustomRoles.Warlock))
                            Main.AllPlayerKillCooldown[pc.PlayerId] = Options.DefaultKillCooldown * 2;
                    }

                if (__instance.AmOwner) Utils.ApplySuffix();
            }

            //LocalPlayer専用
            if (__instance.AmOwner)
            {
                //キルターゲットの上書き処理
                if (GameStates.IsInTask && (__instance.Is(CustomRoles.Sheriff) || __instance.Is(CustomRoles.Investigator) || __instance.Is(CustomRoles.Janitor) || __instance.Is(CustomRoles.Painter) || __instance.Is(CustomRoles.Marksman) || __instance.Is(CustomRoles.BloodKnight) || __instance.Is(CustomRoles.Sidekick) || __instance.Is(CustomRoles.CorruptedSheriff) || __instance.GetRoleType() == RoleType.Coven || __instance.Is(CustomRoles.Arsonist) || __instance.Is(CustomRoles.Werewolf) || __instance.Is(CustomRoles.TheGlitch) || __instance.Is(CustomRoles.Juggernaut) || __instance.Is(CustomRoles.PlagueBearer) || __instance.Is(CustomRoles.Pestilence) || __instance.Is(CustomRoles.Jackal)) && !__instance.Data.IsDead)
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
                    if (Main.playerVersion.TryGetValue(__instance.PlayerId, out var ver))
                        if (Main.version.CompareTo(ver.version) == 0)
                            __instance.cosmetics.nameText.text = ver.tag == $"{ThisAssembly.Git.Commit}({ThisAssembly.Git.Branch})" ? $"<color=#87cefa>{__instance.name}</color>" : $"<color=#ffff00><size=1.2>{ver.tag}</size>\n{__instance?.name}</color>";
                        else __instance.cosmetics.nameText.text = $"<color=#ff0000><size=1.2>v{ver.version}</size>\n{__instance?.name}</color>";
                    else __instance.cosmetics.nameText.text = __instance?.Data?.PlayerName;
                }
                if (GameStates.IsInGame)
                {
                    if (!Options.RolesLikeToU.GetBool() || PlayerControl.LocalPlayer.Data.IsDead)
                    {
                        var RoleTextData = Utils.GetRoleText(__instance);
                        RoleText.text = RoleTextData.Item1;
                        RoleText.color = RoleTextData.Item2;
                        if (__instance.AmOwner) RoleText.enabled = true;
                        // else if (Main.VisibleTasksCount && !PlayerControl.LocalPlayer.Data.IsDead && Options.GhostCanSeeOtherRoles.GetBool() && !__instance.Data.IsDead) RoleText.enabled = false;
                        else if (Main.VisibleTasksCount && PlayerControl.LocalPlayer.Data.IsDead && Options.GhostCanSeeOtherRoles.GetBool()) RoleText.enabled = true;
                        else RoleText.enabled = false;
                        if (!AmongUsClient.Instance.IsGameStarted && AmongUsClient.Instance.GameMode != GameModes.FreePlay)
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
                    else if (seer.GetCustomRole().IsImpostor() && //seerがインポスター
                        target.Is(CustomRoles.Egoist) && Egoist.ImpostorsKnowEgo.GetBool() //targetがエゴイスト
                    )
                        RealName = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Egoist), RealName); //targetの名前をエゴイスト色で表示

                    else if ((seer.Is(CustomRoles.EgoSchrodingerCat) && target.Is(CustomRoles.Egoist)) || //エゴ猫 --> エゴイスト
                             (seer.GetCustomRole().IsJackalTeam() && target.GetCustomRole().IsJackalTeam()) ||
                             (seer.GetCustomRole().IsImpostor() && target.GetCustomRole().IsImpostor())  //J猫 --> ジャッカル
                    )
                        RealName = Helpers.ColorString(target.GetRoleColor(), RealName); //targetの名前をtargetの役職の色で表示
                    else if (target.Is(CustomRoles.Mare) && Utils.IsActive(SystemTypes.Electrical))
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
                    if (seer.Is(CustomRoles.YingYanger) && Main.ColliderPlayers.Contains(target))
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
                    if (seer.Is(CustomRoles.Puppeteer))
                    {
                        if (seer.Is(CustomRoles.Puppeteer) &&
                        Main.PuppeteerList.ContainsValue(seer.PlayerId) &&
                        Main.PuppeteerList.ContainsKey(target.PlayerId))
                            Mark += $"<color={Utils.GetRoleColorCode(CustomRoles.Impostor)}>◆</color>";
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
                    if (GameStates.IsInTask && Options.VultureArrow.GetBool() && target.Is(CustomRoles.Vulture))
                    {
                        var TaskState = Options.VultureArrow.GetBool();
                        if (TaskState)
                        {
                            var coloredArrow = true;
                            var update = false;
                            foreach (var pc in PlayerControl.AllPlayerControls)
                            {
                                var foundCheck = pc.Data.IsDead && !pc.Data.Disconnected;

                                //発見対象じゃ無ければ次
                                if (!foundCheck) continue;

                                update = VultureArrowUpdate(target, pc, update, coloredArrow);
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
        public static bool VultureArrowUpdate(PlayerControl seer, PlayerControl target, bool updateFlag, bool coloredArrow)
        {
            var key = (seer.PlayerId, target.PlayerId);
            if (Main.DeadPlayersThisRound.Contains(target.PlayerId))
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
                //ゲーム中に色を変えた場合
                __instance.RpcMurderPlayer(__instance);
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
                Main.LastEnteredVent.Add(pc.PlayerId, __instance.Id);
                if (Main.LastEnteredVentLocation.ContainsKey(pc.PlayerId))
                    Main.LastEnteredVentLocation.Remove(pc.PlayerId);
                Main.LastEnteredVentLocation.Add(pc.PlayerId, pc.transform.position);
                if (Options.CurrentGameMode() == CustomGameMode.HideAndSeek)
                    if (Options.SplatoonOn.GetBool())
                    {
                        if (!Options.STIgnoreVent.GetBool())
                        {
                            pc?.MyPhysics?.RpcBootFromVent(__instance.Id);
                        }
                    }
                if (CustomRoles.TheGlitch.IsEnable() && Options.GlitchCanVent.GetBool())
                {
                    List<byte> hackedPlayers = new();
                    foreach (var cp in Main.CursedPlayers)
                    {
                        if (cp.Value == null) continue;
                        if (Utils.GetPlayerById(cp.Key).Is(CustomRoles.TheGlitch))
                        {
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
                if (pc.Is(CustomRoles.Camouflager))
                {
                    if (!Camouflager.CanVent())
                    {
                        skipCheck = true;
                        pc.MyPhysics.RpcBootFromVent(__instance.Id);
                    }
                }
                if (Options.CurrentGameMode() == CustomGameMode.HideAndSeek && Options.IgnoreVent.GetBool() && !Options.SplatoonOn.GetBool())
                    pc.MyPhysics.RpcBootFromVent(__instance.Id);
                if (pc.Is(CustomRoles.Mayor))
                {
                    if (Main.MayorUsedButtonCount.TryGetValue(pc.PlayerId, out var count) && count < Options.MayorNumOfUseButton.GetInt())
                    {
                        pc?.ReportDeadBody(null);
                    }
                    pc?.MyPhysics?.RpcBootFromVent(__instance.Id);
                    skipCheck = true;
                }
                if (pc.Is(CustomRoles.Veteran))
                {
                    if (Main.VetAlerts != Options.NumOfVets.GetInt())
                    {
                        pc.VetAlerted();
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

                if (pc.Is(CustomRoles.Arsonist) && Options.TOuRArso.GetBool())
                {
                    skipCheck = true;
                    pc?.MyPhysics?.RpcBootFromVent(__instance.Id);
                    List<PlayerControl> doused = new();
                    foreach (var playerId in Main.dousedIDs)
                    {
                        var player = Utils.GetPlayerById(playerId);
                        if (!player.Data.Disconnected)
                        {
                            if (!player.Data.IsDead)
                                doused.Add(player);
                        }
                    }
                    if (doused.Count != 0)
                    {
                        foreach (var pcd in doused)
                        {
                            if (!pcd.Data.Disconnected)
                            {
                                if (!pcd.Data.Disconnected)
                                {
                                    if (!pcd.Is(CustomRoles.Pestilence))
                                    {
                                        //生存者は焼殺
                                        if (!Main.GuardianAngelTarget.ContainsValue(pcd.PlayerId))
                                        {
                                            pcd.RpcMurderPlayer(pcd);
                                            PlayerState.SetDeathReason(pcd.PlayerId, PlayerState.DeathReason.Torched);
                                            PlayerState.SetDead(pcd.PlayerId);
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
                    skipCheck = true;
                    pc?.MyPhysics?.RpcBootFromVent(__instance.Id);
                }
                /* if (pc.Is(CustomRoles.Camouflager))
                 {
                     pc?.MyPhysics?.RpcBootFromVent(__instance.Id);
                 }*/
                if (pc.Is(CustomRoles.Medusa))
                {
                    pc.StoneGazed();
                    pc?.MyPhysics?.RpcBootFromVent(__instance.Id);
                    skipCheck = true;
                    Utils.NotifyRoles();
                }
                if (pc.Is(CustomRoles.GuardianAngelTOU))
                {
                    if (Main.GAprotects != Options.NumOfProtects.GetInt())
                    {
                        pc.GaProtect();
                    }
                    pc?.MyPhysics?.RpcBootFromVent(__instance.Id);
                    skipCheck = true;
                }
                if (pc.Is(CustomRoles.TheGlitch))
                {
                    skipCheck = true;
                    if (Main.IsHackMode)
                        Main.IsHackMode = false;
                    else
                        Main.IsHackMode = true;
                    pc.MyPhysics.RpcBootFromVent(__instance.Id);
                    Utils.NotifyRoles();
                }
                if (pc.Is(CustomRoles.Bastion))
                {
                    skipCheck = true;
                    if (!Main.bombedVents.Contains(__instance.Id))
                        Main.bombedVents.Add(__instance.Id);
                    else
                    {
                        pc.MyPhysics.RpcBootFromVent(__instance.Id);
                        pc.RpcMurderPlayer(pc);
                        PlayerState.SetDeathReason(pc.PlayerId, PlayerState.DeathReason.Bombed);
                        PlayerState.SetDead(pc.PlayerId);
                        if (Options.BastionVentsRemoveOnBomb.GetBool())
                            Main.bombedVents.Remove(__instance.Id);
                    }
                    pc.MyPhysics.RpcBootFromVent(__instance.Id);
                }
                if (pc.Is(CustomRoles.Werewolf))
                {
                    skipCheck = true;
                    Utils.NotifyRoles();
                    if (Main.IsRampaged)
                    {

                        //do nothing.
                        if (!Options.VentWhileRampaged.GetBool())
                        {
                            // pc?.MyPhysics?.RpcBootFromVent(__instance.Id);
                            pc.MyPhysics.RpcBootFromVent(__instance.Id);
                        }
                        if (Options.VentWhileRampaged.GetBool())
                        {
                            if (Main.bombedVents.Contains(__instance.Id))
                            {
                                pc.RpcMurderPlayer(pc);
                                PlayerState.SetDeathReason(pc.PlayerId, PlayerState.DeathReason.Bombed);
                                Main.whoKilledWho.Add(pc, pc);
                                PlayerState.SetDead(pc.PlayerId);
                                if (Options.BastionVentsRemoveOnBomb.GetBool())
                                    Main.bombedVents.Remove(__instance.Id);
                            }
                        }
                    }
                    else
                    {
                        if (Main.RampageReady)
                        {
                            Main.RampageReady = false;
                            Main.IsRampaged = true;
                            Utils.CustomSyncAllSettings();
                            new LateTask(() =>
                            {
                                Main.IsRampaged = false;
                                pc?.MyPhysics?.RpcBootFromVent(__instance.Id);
                                Utils.CustomSyncAllSettings();
                                new LateTask(() =>
                                {
                                    pc?.MyPhysics?.RpcBootFromVent(__instance.Id);
                                    Main.RampageReady = true;
                                    Utils.CustomSyncAllSettings();
                                }, Options.RampageDur.GetFloat(), "Werewolf Rampage Cooldown");
                            }, Options.RampageDur.GetFloat(), "Werewolf Rampage Duration");
                        }
                        else
                        {
                            pc?.MyPhysics?.RpcBootFromVent(__instance.Id);
                        }
                    }
                }

                if (pc.Is(CustomRoles.Jester) && !Options.JesterCanVent.GetBool())
                {
                    pc.MyPhysics.RpcBootFromVent(__instance.Id);
                    skipCheck = true;
                }
                if (Main.bombedVents.Contains(__instance.Id) && !skipCheck)
                {
                    if (!pc.Is(CustomRoles.Pestilence))
                    {
                        pc.MyPhysics.RpcBootFromVent(__instance.Id);
                        pc.RpcMurderPlayer(pc);
                        PlayerState.SetDeathReason(pc.PlayerId, PlayerState.DeathReason.Bombed);
                        Main.whoKilledWho.Add(pc, pc);
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
                __instance.myPlayer.Is(CustomRoles.SKMadmate) ||
                __instance.myPlayer.Is(CustomRoles.Arsonist) && !Options.TOuRArso.GetBool() ||
                __instance.myPlayer.Is(CustomRoles.PlagueBearer) ||
                (__instance.myPlayer.Is(CustomRoles.Juggernaut) && !Options.JuggerCanVent.GetBool()) ||
                (__instance.myPlayer.Is(CustomRoles.CovenWitch) && !Main.HasNecronomicon) || (__instance.myPlayer.Is(CustomRoles.HexMaster) && !Main.HasNecronomicon) ||
                (__instance.myPlayer.Is(CustomRoles.Mayor) && Main.MayorUsedButtonCount.TryGetValue(__instance.myPlayer.PlayerId, out var count) && count >= Options.MayorNumOfUseButton.GetInt()) ||
                (__instance.myPlayer.Is(CustomRoles.Jackal) && !Options.JackalCanVent.GetBool()) ||
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
                //ライターもしくはスピードブースターもしくはドクターがいる試合のみタスク終了時にCustomSyncAllSettingsを実行する
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
