using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using UnityEngine;

namespace TownOfHost.Patches;
/*
 * HUGE THANKS TO
 * ImaMapleTree / 단풍잎
 * FOR THE CODE
 *
 * THIS IS JUST SMALL FOR NOW BUT MAY EVENTUALLY BE BIG
*/

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.TryPet))]
class LocalPetPatch
{
    public static void Prefix(PlayerControl __instance)
    {
        if (!(AmongUsClient.Instance.AmHost && AmongUsClient.Instance.AmClient)) return;
        __instance.petting = true;
        if (GameStates.IsLobby) return;
        ExternalRpcPetPatch.Prefix(__instance.MyPhysics, 51, new MessageReader());
    }

    public static void Postfix(PlayerControl __instance)
    {
        if (!(AmongUsClient.Instance.AmHost && AmongUsClient.Instance.AmClient)) return;
        __instance.petting = false;
    }

}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
class ExternalRpcPetPatch
{
    public static void Prefix(PlayerPhysics __instance, [HarmonyArgument(0)] byte callId,
        [HarmonyArgument(1)] MessageReader reader)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        var rpcType = callId == 51 ? RpcCalls.Pet : (RpcCalls)callId;
        if (rpcType != RpcCalls.Pet) return;

        PlayerControl playerControl = __instance.myPlayer;

        if (callId == 51 && playerControl.GetCustomRole().PetActivatedAbility() && GameStates.IsInGame)
            //  if (playerControl.GetCustomRole().PetActivatedAbility() && GameStates.IsInGame)
            __instance.CancelPet();
        if (callId != 51)
        {
            CustomRpcSender sender = CustomRpcSender.Create("SelectRoles Sender", SendOption.Reliable);

            if (AmongUsClient.Instance.AmHost && playerControl.GetCustomRole().PetActivatedAbility() && GameStates.IsInGame)
                __instance.CancelPet();
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                AmongUsClient.Instance.FinishRpcImmediately(AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, 50, SendOption.None, player.GetClientId()));
        }

        Logger.Info($"Player {playerControl.GetNameWithRole()} has Pet", "RPCDEBUG");

        if (playerControl.Is(CustomRoles.Miner))
        {
            if (Main.LastEnteredVent.ContainsKey(playerControl.PlayerId))
            {
                int ventId = Main.LastEnteredVent[playerControl.PlayerId].Id;
                var vent = Main.LastEnteredVent[playerControl.PlayerId];
                var position = Main.LastEnteredVentLocation[playerControl.PlayerId];
                Logger.Msg($"{playerControl.GetNameWithRole()}:{position}", "MinerTeleport");
                Utils.TP(playerControl.NetTransform, new Vector2(position.x, position.y + 0.3636f));
            }
        }
        if (playerControl.Is(CustomRoles.Escapist))
        {
            Escapist.OnPet(playerControl);
        }
        if (playerControl.Is(CustomRoles.Creeper))
        {
            Logger.Info("The creeper ignited!", "Creeper");
            bool suicide = false;
            foreach (PlayerControl target in PlayerControl.AllPlayerControls)
            {
                if (target.Data.IsDead) continue;
                if (target.Is(CustomRoles.Phantom)) continue;
                if (target.Is(CustomRoles.Pestilence)) continue;
                if (target.Is(CustomRoles.Pestilence)) continue;

                var dis = Vector2.Distance(playerControl.transform.position, target.transform.position);
                if (dis > 3f) continue;

                if (target == playerControl)
                {
                    suicide = true;
                }
                else
                {
                    PlayerState.SetDeathReason(target.PlayerId, PlayerState.DeathReason.Bombed);
                    target.RpcMurderPlayer(target);
                }
            }
            if (suicide)
            {
                PlayerState.SetDeathReason(playerControl.PlayerId, PlayerState.DeathReason.Suicide);
                playerControl.RpcMurderPlayer(playerControl);
            }
        }
        if (playerControl.Is(CustomRoles.Veteran))
        {
            if (!Main.VetIsAlerted && Main.VetCanAlert && Main.VetAlerts != Options.NumOfVets.GetInt())
            {
                playerControl.VetAlerted();
                Utils.NotifyRoles(GameStates.IsMeeting, playerControl);
            }
        }
        if (playerControl.Is(CustomRoles.TheGlitch))
        {
            Main.IsHackMode = !Main.IsHackMode;
            MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetGlitchState, Hazel.SendOption.Reliable, -1);
            writer2.Write(Main.IsHackMode);
            AmongUsClient.Instance.FinishRpcImmediately(writer2);
            Utils.NotifyRoles();
        }
        if (playerControl.Is(CustomRoles.Transporter))
        {
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
                    }, Options.VetCD.GetFloat(), "Transporter Transport Cooldown (Pet Button)", true);
                }
            }
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetTransportNumber, Hazel.SendOption.Reliable, -1);
            writer.Write(Main.TransportsLeft);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            playerControl.CustomSyncSettings();
        }
    }
}