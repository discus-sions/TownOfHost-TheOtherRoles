using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using Hazel;
using InnerNet;
using static TownOfHost.Translator;
using AmongUs.GameOptions;
using static AmongUs.GameOptions.GameOptionsFactory;
using Il2CppSystem.IO;

namespace TownOfHost
{
    public class FallFromLadder
    {
        public static Dictionary<byte, Vector3> TargetLadderData;
        private static int Chance => Options.LadderDeathChance.GetSelection() + 1;
        public static void Reset()
        {
            TargetLadderData = new();
        }
        public static void OnClimbLadder(PlayerPhysics player, Ladder source)
        {
            if (!Options.LadderDeath.GetBool()) return;
            var sourcepos = source.transform.position;
            var targetpos = source.Destination.transform.position;
            if (sourcepos.y > targetpos.y)
            {
                int chance = UnityEngine.Random.Range(1, 10);
                if (chance <= Chance)
                {
                    TargetLadderData[player.myPlayer.PlayerId] = targetpos;
                    // TargetLadderData[player.GetPrivateField<PlayerControl>("myPlayer").PlayerId] = targetpos;
                }
            }
        }
        public static void FixedUpdate(PlayerControl player)
        {
            if (player.Data.Disconnected) return;
            if (TargetLadderData.ContainsKey(player.PlayerId))
            {
                if (Vector2.Distance(TargetLadderData[player.PlayerId], player.transform.position) < 0.5f)
                {
                    if (player.Data.IsDead) return;
                    //LateTaskを入れるため、先に死亡判定を入れておく
                    player.Data.IsDead = true;
                    new LateTask(() =>
                    {
                        Vector2 targetpos = (Vector2)TargetLadderData[player.PlayerId] + new Vector2(0.1f, 0f);
                        ushort num = (ushort)(NetHelpers.XRange.ReverseLerp(targetpos.x) * 65535f);
                        ushort num2 = (ushort)(NetHelpers.YRange.ReverseLerp(targetpos.y) * 65535f);
                        CustomRpcSender sender = CustomRpcSender.Create("LadderFallRpc", sendOption: Hazel.SendOption.None);
                        sender.AutoStartRpc(player.NetTransform.NetId, (byte)RpcCalls.SnapTo)
                              .Write(num)
                              .Write(num2)
                        .EndRpc();
                        sender.AutoStartRpc(player.NetId, (byte)RpcCalls.MurderPlayer)
                                .WriteNetObject(player)
                        .EndRpc();
                        sender.SendMessage();
                        player.NetTransform.SnapTo(targetpos);
                        player.MurderPlayer(player);
                        PlayerState.SetDeathReason(player.PlayerId, PlayerState.DeathReason.Fell);
                        PlayerState.SetDead(player.PlayerId);
                    }, 0.05f, "LadderFallTask");
                }
            }
        }

    }
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.ClimbLadder))]
    class LadderPatch
    {
        public static void Postfix(PlayerPhysics __instance, Ladder source, byte climbLadderSid)
        {
            FallFromLadder.OnClimbLadder(__instance, source);
        }
    }
}
