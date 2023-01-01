using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Hazel;
using UnityEngine;
using TownOfHost.PrivateExtensions;

namespace TownOfHost
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcMurderPlayer))]
    class RpcMurderPlayerPatch
    {
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            if (!AmongUsClient.Instance.AmHost) return;
            PlayerControl killer = __instance; //読み替え変数
            Utils.NotifyRoles();
            if (!Main.whoKilledWho.ContainsKey(target.Data.PlayerId))
                Main.whoKilledWho.Add(target.Data.PlayerId, killer.PlayerId);

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
        }
    }

    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.IsGameOverDueToDeath))]
    class DontBlackoutPatch
    {
        public static void Postfix(ref bool __result)
        {
            __result = false;
        }
    }
}