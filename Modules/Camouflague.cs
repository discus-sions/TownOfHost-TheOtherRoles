using System.Collections.Generic;

namespace TownOfHost
{
    public static class Camouflague
    {
        public static bool IsActive = false;
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

            Utils.NotifyRoles();
        }
        public static void RpcSetCamouflague(this PlayerControl player)
        {
            if (!AmongUsClient.Instance.AmHost) return;

            int colorId = Main.AllPlayerSkin[player.PlayerId].Item1;


            var sender = CustomRpcSender.Create(name: "RpcSetCamouflague");

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


            var sender = CustomRpcSender.Create(name: "RpcRevertSkins");

            player.SetName(" ");

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