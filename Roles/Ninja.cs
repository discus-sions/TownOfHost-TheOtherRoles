using System.Collections.Generic;

namespace TownOfHost
{
    public static class Ninja
    {
        static readonly int Id = 2700;
        public static List<PlayerControl> NinjaKillTarget = new();
        static List<byte> playerIdList = new();

        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id, CustomRoles.Ninja, AmongUsExtensions.OptionType.Impostor);
        }
        public static void Init()
        {
            playerIdList = new();
        }
        public static void Add(byte playerId)
        {
            playerIdList.Add(playerId);
        }
        public static bool IsEnable()
        {
            return playerIdList.Count > 0;
        }
        public static void NewNinjaKillTarget()
        {
            NinjaKillTarget = new();
        }
        public static void KillCheck(this PlayerControl killer, PlayerControl target)
        {
            if (Main.CheckShapeshift[killer.PlayerId])
            {
                Main.AllPlayerKillCooldown[killer.PlayerId] = Options.DefaultKillCooldown * 2;
                killer.CustomSyncSettings(); //負荷軽減のため、killerだけがCustomSyncSettingsを実行
                killer.RpcGuardAndKill(target);
                NinjaKillTarget.Add(target);
            }
            else
            {
                killer.RpcMurderPlayer(target);
            }
        }
        public static void ShapeShiftCheck(this PlayerControl pc, bool shapeshifting)
        {
            if (!shapeshifting)
                Logger.Info("ShapeShift Release", "Ninja");
            {
                foreach (var ni in NinjaKillTarget)
                {
                    if (!ni.Data.IsDead && !ni.Is(CustomRoles.Ninja))//まれに会議終了後にニンジャがニンジャキルで死亡してしまうのでニンジャ以外のみに限定
                    {
                        Main.AllPlayerKillCooldown[pc.PlayerId] = Options.DefaultKillCooldown;
                        pc.RpcMurderPlayerV2(ni);
                        NinjaKillTarget.Remove(ni);
                        pc.RpcRevertShapeshift(true);//他視点シェイプシフトが解除されないように見える場合があるため強制解除。falseにするとホストとクライアントの挙動が違うようになるのでtrue
                        pc.CustomSyncSettings();//負荷軽減のため、pcだけがCustomSyncSettingsを実行
                    }
                }
            }
        }
    }
}