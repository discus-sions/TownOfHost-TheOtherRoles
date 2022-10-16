using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using UnityEngine;

namespace TownOfHost
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.FixedUpdate))]
    class ShipFixedUpdatePatch
    {
        public static void Postfix(ShipStatus __instance)
        {
            //ここより上、全員が実行する
            if (!AmongUsClient.Instance.AmHost) return;
            //ここより下、ホストのみが実行する
            //if (__instance.G)
            if (Main.IsFixedCooldown && Main.RefixCooldownDelay >= 0)
            {
                Main.RefixCooldownDelay -= Time.fixedDeltaTime;
            }
            else if (!float.IsNaN(Main.RefixCooldownDelay))
            {
                Utils.CustomSyncAllSettings();
                Main.RefixCooldownDelay = float.NaN;
                Logger.Info("Refix Cooldown", "CoolDown");
            }
            if ((Options.CurrentGameMode() == CustomGameMode.HideAndSeek || Options.IsStandardHAS) && Main.introDestroyed)
            {
                if (Options.HideAndSeekKillDelayTimer > 0)
                {
                    Options.HideAndSeekKillDelayTimer -= Time.fixedDeltaTime;
                }
                else if (!float.IsNaN(Options.HideAndSeekKillDelayTimer))
                {
                    Utils.CustomSyncAllSettings();
                    Options.HideAndSeekKillDelayTimer = float.NaN;
                    Logger.Info("キル能力解禁", "HideAndSeek");
                }
            }
        }
    }
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RepairSystem))]
    class RepairSystemPatch
    {
        public static bool IsComms;
        public static bool WasCaused = false;
        public static bool Prefix(ShipStatus __instance,
            [HarmonyArgument(0)] SystemTypes systemType,
            [HarmonyArgument(1)] PlayerControl player,
            [HarmonyArgument(2)] byte amount)
        {
            Logger.Msg("SystemType: " + systemType.ToString() + ", PlayerName: " + player.GetNameWithRole() + ", amount: " + amount, "RepairSystem");
            if (RepairSender.enabled && AmongUsClient.Instance.GameMode != GameModes.OnlineGame)
            {
                Logger.SendInGame("SystemType: " + systemType.ToString() + ", PlayerName: " + player.GetNameWithRole() + ", amount: " + amount);
            }
            IsComms = false;
            foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
                if (task.TaskType == TaskTypes.FixComms) IsComms = true;

            if (!AmongUsClient.Instance.AmHost) return true; //以下、ホストのみ実行
            if ((Options.CurrentGameMode() == CustomGameMode.HideAndSeek || Options.IsStandardHAS) && systemType == SystemTypes.Sabotage) return false;
            //SabotageMaster
            if (player.Is(CustomRoles.SabotageMaster))
                SabotageMaster.RepairSystem(__instance, systemType, amount);
            if (player.Is(CustomRoles.Hacker) && systemType is SystemTypes.Electrical or SystemTypes.Comms or SystemTypes.LifeSupp or SystemTypes.Reactor)
            {
                if (!player.Data.IsDead)
                {
                    SabotageMaster.HackerRepairSystem(__instance, systemType, amount);
                    Main.HackerFixedSaboCount[player.PlayerId]++;
                    MessageWriter writer1 = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetHackerProgress, Hazel.SendOption.Reliable, -1);
                    writer1.Write(player.PlayerId);
                    writer1.Write(Main.HackerFixedSaboCount[player.PlayerId]);
                    AmongUsClient.Instance.FinishRpcImmediately(writer1);
                    if (Main.HackerFixedSaboCount[player.PlayerId] >= Options.SaboAmount.GetFloat())
                    {
                        if (Main.HackerFixedSaboCount[player.PlayerId] >= Options.SaboAmount.GetFloat())
                            Main.HackerFixedSaboCount[player.PlayerId] = Options.SaboAmount.GetInt();
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EndGame, Hazel.SendOption.Reliable, -1);
                        writer.Write((byte)CustomWinner.Hacker);
                        writer.Write(player.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPC.HackerWin(player.PlayerId);
                    }
                }
            }

            if (!Options.MadmateCanFixLightsOut.GetBool() && //Madmateが停電を直せる設定がオフ
               systemType == SystemTypes.Electrical && //システムタイプが電気室
               0 <= amount && amount <= 4 && //配電盤操作のamount
               (player.Is(CustomRoles.Madmate) || player.Is(CustomRoles.MadGuardian) || player.Is(CustomRoles.MadSnitch) || player.Is(CustomRoles.SKMadmate))) //実行者がMadmateかMadGuardianかMadSnitchかSKMadmate)
                return false;
            if (!Options.MadmateCanFixComms.GetBool() && //Madmateがコミュサボを直せる設定がオフ
                systemType == SystemTypes.Comms && //システムタイプが通信室
                (player.Is(CustomRoles.Madmate) || player.Is(CustomRoles.MadGuardian))) //実行者がMadmateかMadGuardian)
                return false;
            /*if (systemType == SystemTypes.Comms && Options.CamoComms.GetBool())
            {
                foreach (PlayerControl target in PlayerControl.AllPlayerControls)
                    target.RpcRevertShapeshift(true);
            }*/
            if (player.Is(CustomRoles.Sheriff) || player.Is(CustomRoles.Investigator) || player.Is(CustomRoles.Escort) || player.Is(CustomRoles.Crusader) || player.Is(CustomRoles.Parasite) || player.Is(CustomRoles.Marksman) || player.Is(CustomRoles.BloodKnight) || player.Is(CustomRoles.Arsonist) || player.Is(CustomRoles.Werewolf) || player.Is(CustomRoles.TheGlitch) || player.GetRoleType() == RoleType.Coven || player.Is(CustomRoles.PlagueBearer) || player.Is(CustomRoles.Pestilence) || player.Is(CustomRoles.Juggernaut) || ((player.Is(CustomRoles.Jackal) || player.Is(CustomRoles.Sidekick)) && !Options.JackalCanUseSabotage.GetBool()) || Main.Grenaiding)
            {
                if (systemType == SystemTypes.Sabotage && AmongUsClient.Instance.GameMode != GameModes.FreePlay) return false; //シェリフにサボタージュをさせない ただしフリープレイは例外
            }
            else
            {
                if (CustomRoles.TheGlitch.IsEnable() | CustomRoles.Escort.IsEnable())
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
                    if (hackedPlayers.Contains(player.PlayerId))
                    {
                        return false;
                    }
                }
            }
            if (systemType == SystemTypes.Sabotage)
            {
                Main.MareHasRedName = false;
                new LateTask(() =>
                {
                    Main.MareHasRedName = true;
                }, Mare.RedNameCooldownAfterLights.GetFloat(), "Mare Red Name Cooldown (After Lights)");
            }
            return true;
        }
        public static void Postfix(ShipStatus __instance, [HarmonyArgument(0)] SystemTypes systemType)
        {
            if (Options.CamoComms.GetBool())
            {
                /*switch (systemType)
                {
                    case SystemTypes.Comms:
                        if (!WasCaused)
                        {
                            WasCaused = true;
                            Camouflague.Cause();
                            Camouflague.IsActive = true;
                        }
                        else
                        {
                            WasCaused = false;
                            Camouflague.Revert();
                            Logger.SendInGame("スキンが元に戻った...");
                            Camouflague.IsActive = true;
                        }
                        break;
                }*/
            }
            Utils.CustomSyncAllSettings();
            new LateTask(
                () =>
                {
                    if (!GameStates.IsMeeting)
                        Utils.NotifyRoles(ForceLoop: true);
                }, 0.1f, "RepairSystem NotifyRoles");
        }
        public static void CheckAndOpenDoorsRange(ShipStatus __instance, int amount, int min, int max)
        {
            var Ids = new List<int>();
            for (var i = min; i <= max; i++)
            {
                Ids.Add(i);
            }
            CheckAndOpenDoors(__instance, amount, Ids.ToArray());
        }
        private static void CheckAndOpenDoors(ShipStatus __instance, int amount, params int[] DoorIds)
        {
            if (DoorIds.Contains(amount)) foreach (var id in DoorIds)
                {
                    __instance.RpcRepairSystem(SystemTypes.Doors, id);
                }
        }
    }
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CloseDoorsOfType))]
    class CloseDoorsPatch
    {
        public static bool Prefix(ShipStatus __instance)
        {
            return !(Options.CurrentGameMode() == CustomGameMode.HideAndSeek || Options.IsStandardHAS) || Options.AllowCloseDoors.GetBool();
        }
    }
    [HarmonyPatch(typeof(SwitchSystem), nameof(SwitchSystem.RepairDamage))]
    class SwitchSystemRepairPatch
    {
        public static void Postfix(SwitchSystem __instance, [HarmonyArgument(0)] PlayerControl player, [HarmonyArgument(1)] byte amount)
        {
            if (player.Is(CustomRoles.SabotageMaster))
                SabotageMaster.SwitchSystemRepair(__instance, amount);
            if (player.Is(CustomRoles.Hacker))
                SabotageMaster.HackerSwitchSystemRepair(__instance, amount);
            //if (__instance.)
        }
    }
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
    class StartPatch
    {
        public static void Postfix()
        {
            Logger.CurrentMethod();
            Logger.Info("-----------ゲーム開始-----------", "Phase");

            Utils.CountAliveImpostors();
        }
    }
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
    class BeginPatch
    {
        public static void Postfix()
        {
            Logger.CurrentMethod();

            //ホストの役職初期設定はここで行うべき？
        }
    }
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CheckTaskCompletion))]
    class CheckTaskCompletionPatch
    {
        public static bool Prefix(ref bool __result)
        {
            if (Options.DisableTaskWin.GetBool())
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}
