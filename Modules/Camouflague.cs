using System.Collections.Generic;

namespace TownOfHost
{
    public static class Camouflague
    {
        public static bool IsActive = false;
        public static bool InMeeting = false;
        public static bool did = false;
        public static void Grenade(bool shapeshifting)
        {
            switch (shapeshifting)
            {
                case false:
                    if (Utils.IsActive(SystemTypes.Electrical) || Utils.IsActive(SystemTypes.Comms) ||
                    Utils.IsActive(SystemTypes.LifeSupp) || Utils.IsActive(SystemTypes.Reactor)) break;
                    Main.Grenaiding = false;
                    new LateTask(() =>
                    {
                        Main.ResetVision = true;
                        Utils.CustomSyncAllSettings();
                        new LateTask(() =>
                        {
                            Main.ResetVision = false;
                            Utils.CustomSyncAllSettings();
                        }, 1, "Reset Vision 2");
                    }, 5, "Reset Vision");
                    Utils.NotifyRoles();
                    break;
                case true:
                    if (Utils.IsActive(SystemTypes.Electrical) || Utils.IsActive(SystemTypes.Comms) ||
                        Utils.IsActive(SystemTypes.LifeSupp) || Utils.IsActive(SystemTypes.Reactor)) break;
                    Main.ResetVision = false;
                    Main.Grenaiding = true;
                    Utils.CustomSyncAllSettings();
                    new LateTask(() =>
                    {
                        Utils.NotifyRoles(NoCache: true);
                    },
                    1.2f, "ShapeShiftNotify");
                    break;
            }
        }
        public static void Cause()
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                player.RpcSetCamouflague();
            }

            IsActive = true;
            RpcToggleCamouflague(IsActive);
            Utils.NotifyRoles();
        }
        public static void Revert()
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                player.RpcRevertSkins();
            }

            IsActive = false;
            RpcToggleCamouflague(IsActive);

            Utils.NotifyRoles();
        }
        public static void MeetingCause()
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                player.RpcSetCamouflague();
            }

            ///IsActive = true;
            //RpcToggleCamouflague(IsActive);
            Utils.NotifyRoles();
        }
        public static void MeetingRevert()
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                player.RpcRevertSkins();
            }

            //MeetingRpcToggleCamouflague();
            IsActive = true;
            Utils.NotifyRoles();
        }
        public static void RpcSetCamouflague(this PlayerControl player)
        {
            if (!AmongUsClient.Instance.AmHost) return;

            int colorId = Main.AllPlayerSkin[player.PlayerId].Item1;


            var sender = CustomRpcSender.Create(name: "RpcSetCamouflague");

            player.SetName("");
            player.SetColor(15); //グレー
            /*sender.AutoStartRpc(player.NetId, (byte)RpcCalls.SetColor)
                .Write(15)
                .EndRpc();*/

            player.SetHat("", colorId);
            /*sender.AutoStartRpc(player.NetId, (byte)RpcCalls.SetHatStr)
                .Write("")
                .EndRpc();*/

            player.SetSkin("", colorId);
            /*sender.AutoStartRpc(player.NetId, (byte)RpcCalls.SetSkinStr)
                .Write("")
                .EndRpc();*/

            player.SetVisor("", colorId);
            /*sender.AutoStartRpc(player.NetId, (byte)RpcCalls.SetVisorStr)
                .Write("")
                .EndRpc();*/

            player.SetPet("");
            /*sender.AutoStartRpc(player.NetId, (byte)RpcCalls.SetPetStr)
                .Write("")
                .EndRpc();*/

            player.Shapeshift(PlayerControl.LocalPlayer, false);

            sender.AutoStartRpc(player.NetId, (byte)RpcCalls.Shapeshift)
                .Write(PlayerControl.LocalPlayer)
                .Write(false)
                .EndRpc();

            sender.SendMessage();
        }

        public static void RpcRevertSkins(this PlayerControl player)
        {
            if (!AmongUsClient.Instance.AmHost) return;

            int colorId = Main.AllPlayerSkin[player.PlayerId].Item1;
            string hatId = Main.AllPlayerSkin[player.PlayerId].Item2;
            string skinId = Main.AllPlayerSkin[player.PlayerId].Item3;
            string visorId = Main.AllPlayerSkin[player.PlayerId].Item4;
            string petId = Main.AllPlayerSkin[player.PlayerId].Item5;
            string name = Main.AllPlayerSkin[player.PlayerId].Item6;

            var sender = CustomRpcSender.Create(name: "RpcRevertSkins");

            player.SetName(name);

            player.SetColor(colorId); //グレー
            /*sender.AutoStartRpc(player.NetId, (byte)RpcCalls.SetColor)
                .Write(colorId)
                .EndRpc();*/

            player.SetHat(hatId, colorId);
            /*sender.AutoStartRpc(player.NetId, (byte)RpcCalls.SetHatStr)
                .Write(hatId)
                .EndRpc();*/

            player.SetSkin(skinId, colorId);
            /*sender.AutoStartRpc(player.NetId, (byte)RpcCalls.SetSkinStr)
                .Write(skinId)
                .EndRpc();*/

            player.SetVisor(visorId, colorId);
            /*sender.AutoStartRpc(player.NetId, (byte)RpcCalls.SetVisorStr)
                .Write(visorId)
                .EndRpc();*/

            player.SetPet(petId);
            /*sender.AutoStartRpc(player.NetId, (byte)RpcCalls.SetPetStr)
                .Write(petId)
                .EndRpc();*/

            player.RpcShapeshift(player, false);
            sender.AutoStartRpc(player.NetId, (byte)RpcCalls.Shapeshift)
                .Write(player)
                .Write(false)
                .EndRpc();

            sender.SendMessage();
        }
        public static void RpcToggleCamouflague(bool IsActive)
        {
            var sender = CustomRpcSender.Create("Set Camouflague IsActive");

            sender.AutoStartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ToggleCamouflagueActive)
                .Write(IsActive)
                .EndRpc();
            sender.SendMessage();
        }

        public static void MeetingRpcToggleCamouflague()
        {
            var sender = CustomRpcSender.Create("Set Camouflague IsActive");

            sender.AutoStartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ToggleCamouflagueActive)
                .Write(true)
                .EndRpc();
            sender.SendMessage();
        }
    }
}