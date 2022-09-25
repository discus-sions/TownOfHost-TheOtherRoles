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
                    if (client.Character != null) ChatCommands.SendTemplate("welcome", client.Character.PlayerId, true);
                    string rname = client.Character.Data.PlayerName;
                    string fontSize2 = "0.7";
                    string fontSize3 = "0.5";
                    string fontSize4 = "0.75";
                    string fontSize1 = "0.8";
                    string fontSize = "1.5";
                    string fontSize5 = "1";
                    string dev = $"<size={fontSize2}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.TheGlitch), "Developer")}</size>";
                    string dscfr = $"<size={fontSize2}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.GM), "ToH:ToR Discord Member")}</size>";
                    string sns = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pinkcolor), "♥")}</size>";
                    string allie = $"<size={fontSize4}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Amnesiac), "Aviator")}</size>";
                    string pushp = $"<size={fontSize2}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Jester), "Demolitionist")}</size>";
                    string slnc = $"<size={fontSize4}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Amnesiac), "Icyyy")}</size>";
                    string snf = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Amnesiac), "米")}</size>";
                    string aug1 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.aug1), "♡")}</size>";
                    string augc = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.aug2), "C")}</size>";
                    string augu = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.aug3), "u")}</size>";
                    string augt = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.aug4), "t")}</size>";
                    string augi = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.aug5), "i")}</size>";
                    string auge = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.aug6), "e")}</size>";
                    string aug2 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.aug7), "♡")}</size>";
                    string sns1 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns1), "♡")}</size>";
                    string sns2 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns2), "H")}</size>";
                    string sns3 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns3), "o")}</size>";
                    string sns4 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns4), "t")}</size>";
                    string sns5 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns5), "t")}</size>";
                    string sns6 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns6), "i")}</size>";
                    string sns7 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns7), "e")}</size>";
                    string sns8 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns8), "♡")}</size>";
                    string sns91 = $"<size={fontSize5}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns1), "♡")}</size>";
                    string sns9 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns1), "shi")}</size>";
                    string sns0 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns3), "ft")}</size>";
                    string sns01 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns5), "yr")}</size>";
                    string sns02 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns6), "os")}</size>";
                    string sns03 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns6), "e")}</size>";
                    string sns92 = $"<size={fontSize5}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns7), "♡")}</size>";
                    string name = dev + "\r\n" + rname;
                    string named = dscfr + "\r\n" + rname;
                    string snsname = sns1 + sns2 + sns3 + sns4 + sns5 + sns6 + sns7 + sns8 + "\r\n" + sns91 + sns9 + sns0 + sns01 + sns02 + sns03 + sns92;
                    string nameb = allie + "\r\n" + rname;
                    string namep = sns + pushp + sns + "\r\n" + rname;
                    string namesilence = slnc + snf + "\r\n" + rname;
                    string nameaugust = aug1 + augc+augu+augt+augi+auge + aug2 + "\r\n" + rname;
                    if (client.FriendCode is "nullrelish#9615" or "vastblaze#8009" or "ironbling#3600" or "tillhoppy#6167" or "gnuedaphic#7196" or "pingrating#9371")
                    {
                        client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.rosecolor), name)}");
                        Main.devNames.Add(client.Character.PlayerId, rname);
                    }
                    if (client.FriendCode is "Available#9898" or "mossmodel#2348" or "Mossmodel#2348")
                    {
                        client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.aug4), nameaugust)}");
                        Main.devNames.Add(client.Character.PlayerId, rname);
                    }
                    if (client.FriendCode is "leafywinch#2382" or "jailtoy#0133" or "alphabye#3999" or "walkingdice#5285" or "scoopgooey#9820" or "innfancy#2127" or "artfulcod#9001" or "frostmolar#1359" or "everyswap#7877" or "waterpupal#6193" or "iconicoar#2342" or "steamquits#4906" or "ruffseated#8388" or "nicestone#7505" or "ravencalyx#2196" or "iconicpun#5624" or "flathomey#1351" or "talentsalt#4516" or "namebasic#9510" or "envykindly#7034" or "waterpupal#6193" or "privyeater#0729" or "tigerbitty#4312" or "honeytired#7330" or "waryclaw#7449" or "basicstork#6394" or "mobileswap#4514" or "sparebank#8022" or "artfulcod#9001" or "sunnysolid#5221" or "nmossmodel#2348" or "beansimple#8487" or "epicflower#1116" or "fuzzytub#9375" or "earthygale#6105" or "luckyplus#8283")
                    {
                        client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.thirdcolor), named)}");
                        Main.devNames.Add(client.Character.PlayerId, rname);
                    }
                    if (client.FriendCode is "available#2385" or "envykindly#7034")
                    {
                        client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.rosecolor), snsname)}");
                        Main.devNames.Add(client.Character.PlayerId, rname);
                    }
                    if (client.FriendCode is "available#2385" or "sunlitmoon#2472")
                    {
                        client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.rosecolor), nameb)}");
                        Main.devNames.Add(client.Character.PlayerId, rname);
                    }
                    if (client.FriendCode is "available#2110" or "testfly#6512")
                    {
                        client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Demolitionist), namep)}");
                        Main.devNames.Add(client.Character.PlayerId, rname);
                    }
                    if (client.FriendCode is "riskylatte#0409" or "shotnote#2620" or "furrycoin#0508")
                    {
                        client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.rosecolor), namesilence)}");
                        Main.devNames.Add(client.Character.PlayerId, rname);
                    }
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
                    if (Sheriff.csheriff)
                    {
                        ShipStatus.Instance.enabled = false;
                        ShipStatus.RpcEndGame(GameOverReason.ImpostorDisconnect, false);
                    }
                    else
                    {
                        var localPlayer = PlayerControl.LocalPlayer;
                        if (!localPlayer.GetCustomRole().IsNeutralKilling())
                        {
                            localPlayer.RpcSetCustomRole(CustomRoles.CorruptedSheriff);
                        }
                        else
                        {
                            if (localPlayer.Is(CustomRoles.CrewPostor))
                                localPlayer.RpcSetCustomRole(CustomRoles.CorruptedSheriff);
                        }
                        RoleManager.Instance.SetRole(localPlayer, RoleTypes.Impostor);
                        Sheriff.csheriff = true;
                    }
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
            Logger.Info($"{data.PlayerName}(ClientID:{data.Id})が切断(理由:{reason})", "Session");
        }
    }
}
