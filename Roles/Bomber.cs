using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;
using TownOfHost.PrivateExtensions;
using UnityEngine;

namespace TownOfHost;

public static class Bomber
{
    public static readonly int ID = 48709130;
    public static List<byte> PlayerIdList;

    public static Dictionary<byte, (PlayerControl, float)> BomberTimer;

    public static byte CurrentDouseTarget = 255;
    public static byte CurrentBombedPlayer = 255;
    public static bool TargetIsRoleBlocked = false;
    public static bool InKillProgress = false;

    public static CustomOption BombCooldown;
    public static CustomOption BombTime;
    public static CustomOption BombedPlayerRoleblockTime;

    public static CustomOption AllImpostorsSeeBombedPlayer;
    public static CustomOption BomberIsTrappedWhileBombedPlayerIsRoleBlocked;

    public static void SetupCustomOption()
    {
        Options.SetupSingleRoleOptions(ID, CustomRoles.Bomber, 1, AmongUsExtensions.OptionType.Impostor);
        BombTime = CustomOption.Create(ID + 10, Color.white, "BombTime", AmongUsExtensions.OptionType.Impostor, 3, 0, 30, 1, Options.CustomRoleSpawnChances[CustomRoles.Bomber]);
        BombCooldown = CustomOption.Create(ID + 11, Color.white, "BombCooldown", AmongUsExtensions.OptionType.Impostor, 25f, 2.5f, 60f, 2.5f, Options.CustomRoleSpawnChances[CustomRoles.Bomber]);
        BombedPlayerRoleblockTime = CustomOption.Create(ID + 12, Color.white, "BombedPlayerRoleblockTime", AmongUsExtensions.OptionType.Impostor, 5f, 2.5f, 20f, 2.5f, Options.CustomRoleSpawnChances[CustomRoles.Bomber]);
        AllImpostorsSeeBombedPlayer = CustomOption.Create(ID + 13, Color.white, "AllImpostorsSeeBombedPlayer", AmongUsExtensions.OptionType.Impostor, true, Options.CustomRoleSpawnChances[CustomRoles.Bomber]);
        BomberIsTrappedWhileBombedPlayerIsRoleBlocked = CustomOption.Create(ID + 14, Color.white, "BomberIsTrappedWhileBombedPlayerIsRoleBlocked", AmongUsExtensions.OptionType.Impostor, true, Options.CustomRoleSpawnChances[CustomRoles.Bomber]);
    }

    public static void Init()
    {
        PlayerIdList = new List<byte>();
        BomberTimer = new Dictionary<byte, (PlayerControl, float)>();
        CurrentDouseTarget = 255;
        CurrentBombedPlayer = 255;
        TargetIsRoleBlocked = false;
        InKillProgress = false;
    }

    public static bool DoesExist() => PlayerIdList.Count > 0;

    public static void Add(PlayerControl player)
    {
        PlayerIdList.Add(player.PlayerId);
    }

    public static void ApplyGameOptions(PlayerControl player, NormalGameOptionsV07 options)
    {

    }

    public static void OnKillButton(PlayerControl bomber, PlayerControl target)
    {
        CurrentDouseTarget = target.PlayerId;
    }

    public static void OnReportDeadBody()
    {
        if (!DoesExist()) return;
        BomberTimer.Clear();
        if (CurrentBombedPlayer != 255)
        {
            if (!TargetIsRoleBlocked && !InKillProgress)
            {
                PlayerControl bombedPlayer = Utils.GetPlayerById(CurrentBombedPlayer);
                if (bombedPlayer != null)
                {
                    bombedPlayer.RpcMurderPlayer(bombedPlayer);
                    PlayerState.SetDeathReason(bombedPlayer.PlayerId, PlayerState.DeathReason.Bombed);
                }
            }
        }
        CurrentBombedPlayer = 255;
        CurrentDouseTarget = 255;
        TargetIsRoleBlocked = false;
        InKillProgress = false;
    }

    public static void OnTargetCollide(PlayerControl target)
    {
        PlayerControl bombedPlayer = Utils.GetPlayerById(CurrentBombedPlayer);
        //if (bombedPlayer == null) return;
        PlayerControl bomber = Utils.GetPlayerById(PlayerIdList[0]);
        //if (bomber == null) return;
        bombedPlayer.RpcMurderPlayer(target);
        TargetIsRoleBlocked = true;
        InKillProgress = true;
        Main.AllPlayerSpeed[bombedPlayer.PlayerId] = 0.00001f;
        if (BomberIsTrappedWhileBombedPlayerIsRoleBlocked.GetBool())
        {
            Main.AllPlayerSpeed[bomber.PlayerId] = 0.00001f;
        }
        _ = new LateTask(() =>
        {
            TargetIsRoleBlocked = false;
            CurrentBombedPlayer = 255;
            InKillProgress = false;
            Main.AllPlayerSpeed[bombedPlayer.PlayerId] = Main.RealOptionsData.AsNormalOptions()!.PlayerSpeedMod;
            if (BomberIsTrappedWhileBombedPlayerIsRoleBlocked.GetBool())
            {
                Main.AllPlayerSpeed[bomber.PlayerId] = Main.RealOptionsData.AsNormalOptions()!.PlayerSpeedMod;
            }
        }, BombedPlayerRoleblockTime.GetFloat(), "Bombed Player RoleBlock Timer");
    }

    public static void SendRPC(byte currentDousedTarget = 255, byte currentBombedPlayer = 255)
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetBomberTargets, SendOption.Reliable, -1);
        writer.Write(currentDousedTarget);
        writer.Write(currentBombedPlayer);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    public static void RecieveRPC(MessageReader reader)
    {
        byte currentDousedTarget = reader.ReadByte();
        byte currentBombedPlayer = reader.ReadByte();
        CurrentDouseTarget = currentDousedTarget;
        CurrentBombedPlayer = currentBombedPlayer;
    }
}