using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using InnerNet;

namespace TownOfHost
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
    class OnGameJoinedPatch
    {
        public static void Postfix(AmongUsClient __instance)
        {
            Logger.Info($"{__instance.GameId}に参加", "OnGameJoined");
            Main.playerVersion = new Dictionary<byte, PlayerVersion>();
            Main.devNames = new Dictionary<byte, string>();
            RPC.RpcVersionCheck();

            NameColorManager.Begin();
            Options.Load();
            if (AmongUsClient.Instance.AmHost) //以下、ホストのみ実行
            {
                if (PlayerControl.GameOptions.killCooldown == 0.1f)
                    PlayerControl.GameOptions.killCooldown = Main.LastKillCooldown.Value;
                new LateTask(() =>
                {
                    string rname = PlayerControl.LocalPlayer.Data.PlayerName;
                    string fontSize = "1.5";
                    string dev = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.TheGlitch), "Dev")}</size>";
                    string name = dev + "\r\n" + rname;
                    if (PlayerControl.LocalPlayer.Data.FriendCode is "nullrelish#9615" or "tillhoppy#6167" or "gnuedaphic#7196" or "pingrating#9371")
                    {
                        PlayerControl.LocalPlayer.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.TheGlitch), name)}");
                        Main.devNames.Add(PlayerControl.LocalPlayer.Data.PlayerId, rname);
                    }
                }, 3f, "Name Check for Host");
            }
        }
    }
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
    class OnPlayerJoinedPatch
    {
        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
        {
            Logger.Info($"{client.PlayerName}(ClientID:{client.Id}) (FreindCode:{client.FriendCode}) joined the game.", "Session");
            if (DestroyableSingleton<FriendsListManager>.Instance.IsPlayerBlockedUsername(client.FriendCode) && AmongUsClient.Instance.AmHost)
            {
                AmongUsClient.Instance.KickPlayer(client.Id, true);
                Logger.Info($"This is a blocked player. {client?.PlayerName}({client.FriendCode}) was banned.", "BAN");
            }
            Main.playerVersion = new Dictionary<byte, PlayerVersion>();
            RPC.RpcVersionCheck();
            if (AmongUsClient.Instance.AmHost)
            {
                new LateTask(() =>
                {
                    if (client.Character != null)
                    {
                        ChatCommands.SendTemplate("welcome", client.Character.PlayerId, true);
                        string rname = client.Character.Data.PlayerName;
                        string fontSize = "1.5";
                        string dev = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.TheGlitch), "Dev")}</size>";
                        string name = dev + "\r\n" + rname;
                        if (client.FriendCode is "nullrelish#9615" or "vastblaze#8009" or "ironbling#3600" or "tillhoppy#6167" or "gnuedaphic#7196" or "pingrating#9371")
                        {
                            client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.TheGlitch), name)}");
                            Main.devNames.Add(client.Character.PlayerId, rname);
                        }
                    }
                    //nice
                }, 3f, "Welcome Message & Name Check");
            }
        }
    }
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
    class OnPlayerLeftPatch
    {
        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData data, [HarmonyArgument(1)] DisconnectReasons reason)
        {
            //            Logger.info($"RealNames[{data.Character.PlayerId}]を削除");
            //            main.RealNames.Remove(data.Character.PlayerId);
            if (!AmongUsClient.Instance.AmHost) return;
            if (GameStates.IsInGame)
            {
                Utils.CountAliveImpostors();
                if (data.Character.Is(CustomRoles.TimeThief))
                    data.Character.ResetVotingTime();
                if (data.Character.GetCustomSubRole() == CustomRoles.LoversRecode && !data.Character.Data.IsDead)
                    foreach (var lovers in Main.LoversPlayers.ToArray())
                    {
                        Main.isLoversDead = true;
                        Main.LoversPlayers.Remove(lovers);
                        Main.HasModifier.Remove(lovers.PlayerId);
                        Main.AllPlayerCustomSubRoles[lovers.PlayerId] = CustomRoles.NoSubRoleAssigned;
                    }
                if (data.Character.Is(CustomRoles.Executioner) && Main.ExecutionerTarget.ContainsKey(data.Character.PlayerId) && Main.ExeCanChangeRoles)
                {
                    data.Character.RpcSetCustomRole(Options.CRoleExecutionerChangeRoles[Options.ExecutionerChangeRolesAfterTargetKilled.GetSelection()]);
                    Main.ExecutionerTarget.Remove(data.Character.PlayerId);
                    RPC.RemoveExecutionerKey(data.Character.PlayerId);
                }
                if (data.Character.Is(CustomRoles.GuardianAngelTOU) && Main.GuardianAngelTarget.ContainsKey(data.Character.PlayerId))
                {
                    data.Character.RpcSetCustomRole(Options.CRoleGuardianAngelChangeRoles[Options.WhenGaTargetDies.GetSelection()]);
                    if (data.Character.IsModClient())
                        data.Character.RpcSetCustomRole(Options.CRoleGuardianAngelChangeRoles[Options.WhenGaTargetDies.GetSelection()]); //対象がキルされたらオプションで設定した役職にする
                    else
                    {
                        if (Options.CRoleGuardianAngelChangeRoles[Options.WhenGaTargetDies.GetSelection()] != CustomRoles.Amnesiac)
                            data.Character.RpcSetCustomRole(Options.CRoleGuardianAngelChangeRoles[Options.WhenGaTargetDies.GetSelection()]); //対象がキルされたらオプションで設定した役職にする
                        else
                            data.Character.RpcSetCustomRole(Options.CRoleGuardianAngelChangeRoles[2]);
                    }
                    Main.GuardianAngelTarget.Remove(data.Character.PlayerId);
                    RPC.RemoveGAKey(data.Character.PlayerId);
                }
                if (data.Character.Is(CustomRoles.Jackal))
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
                if (Main.ColliderPlayers.Contains(data.Character) && CustomRoles.YingYanger.IsEnable() && Options.ResetToYinYang.GetBool())
                {
                    Main.DoingYingYang = false;
                }
                if (Main.ColliderPlayers.Contains(data.Character))
                    Main.ColliderPlayers.Remove(data.Character);
                if (data.Character.LastImpostor() || data.Character.Is(CustomRoles.Egoist))
                {
                    //Main.currentWinner = CustomWinner.None;
                    /*bool egoist = false;
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc.Data.Disconnected || pc == null) continue;
                        CustomRoles pc_role = pc.GetCustomRole();
                        if (pc_role == CustomRoles.Egoist && !pc.Data.IsDead) egoist = true;
                    }
                    if (data.Character.Is(CustomRoles.Egoist) && egoist)
                    {
                        if (Main.AliveImpostorCount != 1) egoist = false;
                    }
                    if (!egoist)*/
                    ShipStatus.Instance.enabled = false;
                    ShipStatus.RpcEndGame(GameOverReason.ImpostorDisconnect, false);
                }
                if (Main.ExecutionerTarget.ContainsValue(data.Character.PlayerId) && Main.ExeCanChangeRoles)
                {
                    byte Executioner = 0x73;
                    Main.ExecutionerTarget.Do(x =>
                    {
                        if (x.Value == data.Character.PlayerId)
                            Executioner = x.Key;
                    });
                    Utils.GetPlayerById(Executioner).RpcSetCustomRole(Options.CRoleExecutionerChangeRoles[Options.ExecutionerChangeRolesAfterTargetKilled.GetSelection()]);
                    Main.ExecutionerTarget.Remove(Executioner);
                    RPC.RemoveExecutionerKey(Executioner);
                    Utils.NotifyRoles();
                }
                if (Main.GuardianAngelTarget.ContainsValue(data.Character.PlayerId))
                {
                    byte GA = 0x73;
                    Main.ExecutionerTarget.Do(x =>
                    {
                        if (x.Value == data.Character.PlayerId)
                            GA = x.Key;
                    });
                    // Utils.GetPlayerById(GA).RpcSetCustomRole(Options.CRoleGuardianAngelChangeRoles[Options.WhenGaTargetDies.GetSelection()]);
                    if (Utils.GetPlayerById(GA).IsModClient())
                        Utils.GetPlayerById(GA).RpcSetCustomRole(Options.CRoleGuardianAngelChangeRoles[Options.WhenGaTargetDies.GetSelection()]); //対象がキルされたらオプションで設定した役職にする
                    else
                    {
                        if (Options.CRoleGuardianAngelChangeRoles[Options.WhenGaTargetDies.GetSelection()] != CustomRoles.Amnesiac)
                            Utils.GetPlayerById(GA).RpcSetCustomRole(Options.CRoleGuardianAngelChangeRoles[Options.WhenGaTargetDies.GetSelection()]); //対象がキルされたらオプションで設定した役職にする
                        else
                            Utils.GetPlayerById(GA).RpcSetCustomRole(Options.CRoleGuardianAngelChangeRoles[2]);
                    }
                    Main.GuardianAngelTarget.Remove(GA);
                    RPC.RemoveGAKey(GA);
                    Utils.NotifyRoles();
                }
                if (PlayerState.GetDeathReason(data.Character.PlayerId) == PlayerState.DeathReason.etc) //死因が設定されていなかったら
                {
                    PlayerState.SetDeathReason(data.Character.PlayerId, PlayerState.DeathReason.Disconnected);
                    PlayerState.SetDead(data.Character.PlayerId);
                }
                if (AmongUsClient.Instance.AmHost && GameStates.IsLobby)
                {
                    _ = new LateTask(() =>
                    {
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            pc.RpcSetNameEx(pc.GetRealName(isMeeting: true));
                        }
                    }, 1f, "SetName To Chat");
                }
            }
            if (Main.devNames.ContainsKey(data.Character.PlayerId))
                Main.devNames.Remove(data.Character.PlayerId);
            Logger.Info($"{data.PlayerName}(ClientID:{data.Id})が切断(理由:{reason})", "Session");
        }
    }
}
