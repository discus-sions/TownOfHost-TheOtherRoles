using System.Collections.Generic;
using Hazel;
using UnityEngine;

namespace TownOfHost
{
    public static class Investigator
    {
        private static readonly int Id = 120400;
        public static List<byte> playerIdList = new();
        public static Dictionary<byte, bool> hasSeered = new();

        private static CustomOption KillCooldown;
        private static CustomOption NBareRed;
        private static CustomOption NKareRed;
        private static CustomOption NEareRed;
        private static CustomOption CrewKillingRed;
        public static CustomOption CovenIsPurple;
        private static CustomOption ChildIsRed;
        private static CustomOption TerroIsRed;
        public static CustomOption CSheriffSwitches;
        private static CustomOption MadMateIsRed;

        public static bool SeeredCSheriff;
        public static Dictionary<byte, float> ShotLimit = new();
        public static Dictionary<byte, float> CurrentKillCooldown = new();
        public static void SetupCustomOption()
        {
            Options.SetupSingleRoleOptions(Id, CustomRoles.Investigator, 1);
            KillCooldown = CustomOption.Create(Id + 10, Color.white, "SeerCooldown", 45, 1, 990, 1, Options.CustomRoleSpawnChances[CustomRoles.Investigator]);
            NBareRed = CustomOption.Create(Id + 11, Color.white, "NBareRed", false, Options.CustomRoleSpawnChances[CustomRoles.Investigator]);
            NKareRed = CustomOption.Create(Id + 12, Color.white, "NKareRed", true, Options.CustomRoleSpawnChances[CustomRoles.Investigator]);
            NEareRed = CustomOption.Create(Id + 13, Color.white, "NEareRed", true, Options.CustomRoleSpawnChances[CustomRoles.Investigator]);
            CrewKillingRed = CustomOption.Create(Id + 14, Color.white, "CrewKillingRed", true, Options.CustomRoleSpawnChances[CustomRoles.Investigator]);
            CovenIsPurple = CustomOption.Create(Id + 15, Color.white, "CovenIsPurple", true, Options.CustomRoleSpawnChances[CustomRoles.Investigator]);
            ChildIsRed = CustomOption.Create(Id + 15, Color.white, "ChildIsRed", true, Options.CustomRoleSpawnChances[CustomRoles.Investigator]);
            TerroIsRed = CustomOption.Create(Id + 16, Color.white, "TerroIsRed", true, Options.CustomRoleSpawnChances[CustomRoles.Investigator]);
            CSheriffSwitches = CustomOption.Create(Id + 17, Color.white, "CSheriffSwitches", true, Options.CustomRoleSpawnChances[CustomRoles.Investigator]);
            MadMateIsRed = CustomOption.Create(Id + 18, Color.white, "MadMateIsRed", true, Options.CustomRoleSpawnChances[CustomRoles.Investigator]);
        }
        public static void Init()
        {
            playerIdList = new();
            hasSeered = new();
            ShotLimit = new();
            CurrentKillCooldown = new();
            SeeredCSheriff = false;
        }
        public static void Add(byte playerId)
        {
            playerIdList.Add(playerId);
            CurrentKillCooldown.Add(playerId, KillCooldown.GetFloat());

            if (!Main.ResetCamPlayerList.Contains(playerId))
                Main.ResetCamPlayerList.Add(playerId);

            Logger.Info($"{Utils.GetPlayerById(playerId)?.GetNameWithRole()} : Add Investigator Role", "Investigator");
        }
        public static bool IsEnable => playerIdList.Count > 0;
        private static void SendRPC(byte playerId)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetSheriffShotLimit, SendOption.Reliable, -1);
            writer.Write(playerId);
            writer.Write(ShotLimit[playerId]);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static void ReceiveRPC(MessageReader reader)
        {
            byte SheriffId = reader.ReadByte();
            float Limit = reader.ReadSingle();
        }
        public static void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = CurrentKillCooldown[id];
        public static bool CanUseKillButton(PlayerControl player)
        {
            if (player.Data.IsDead)
                return false;

            return true;
        }
        public static bool OnCheckMurder(PlayerControl killer, PlayerControl target, string Process)
        {
            Logger.Info($"{killer.GetNameWithRole()} : Investigated Player: {target.GetNameWithRole()}", "Investigated");
            SendRPC(killer.PlayerId);
            killer.RpcGuardAndKill(target);
            return true;
        }
        public static bool IsRed(this PlayerControl player)
        {
            var cRole = player.GetCustomRole();
            return cRole switch
            {
                CustomRoles.Jester => NEareRed.GetBool(),
                CustomRoles.Terrorist => TerroIsRed.GetBool(),
                CustomRoles.Executioner => NEareRed.GetBool(),
                CustomRoles.Swapper => NEareRed.GetBool(),
                CustomRoles.Opportunist => NBareRed.GetBool(),
                CustomRoles.Survivor => NBareRed.GetBool(),
                CustomRoles.Arsonist => NKareRed.GetBool(),
                CustomRoles.Egoist => NKareRed.GetBool(),
                CustomRoles.EgoSchrodingerCat => NBareRed.GetBool(),
                CustomRoles.Jackal => NKareRed.GetBool(),
                CustomRoles.Sidekick => NKareRed.GetBool(),
                CustomRoles.Juggernaut => NKareRed.GetBool(),
                CustomRoles.JSchrodingerCat => NBareRed.GetBool(),
                CustomRoles.PlagueBearer => NKareRed.GetBool(),
                CustomRoles.Marksman => NKareRed.GetBool(),
                CustomRoles.Vulture => NEareRed.GetBool(),
                CustomRoles.Pirate => NKareRed.GetBool(),
                CustomRoles.NiceGuesser => CrewKillingRed.GetBool(),
                CustomRoles.TheGlitch => NKareRed.GetBool(),
                CustomRoles.BloodKnight => NKareRed.GetBool(),
                CustomRoles.Werewolf => NKareRed.GetBool(),
                CustomRoles.Child => ChildIsRed.GetBool(),
                CustomRoles.Sheriff => CrewKillingRed.GetBool(),
                CustomRoles.Demolitionist => CrewKillingRed.GetBool(),
                CustomRoles.Bodyguard => CrewKillingRed.GetBool(),
                CustomRoles.Crusader => CrewKillingRed.GetBool(),
                CustomRoles.Bastion => CrewKillingRed.GetBool(),
                CustomRoles.Veteran => CrewKillingRed.GetBool(),
                // COVEN //
                CustomRoles.Coven => true,
                CustomRoles.CovenWitch => true,
                CustomRoles.Poisoner => true,
                CustomRoles.HexMaster => true,
                CustomRoles.PotionMaster => true,
                CustomRoles.Medusa => true,
                CustomRoles.Mimic => true,
                CustomRoles.Necromancer => true,
                CustomRoles.Conjuror => true,
                // AFTER COVEN //
                CustomRoles.SchrodingerCat => NBareRed.GetBool(),
                CustomRoles.Hacker => true,
                _ => cRole.GetRoleType() switch
                {
                    RoleType.Impostor => true,
                    RoleType.Madmate => MadMateIsRed.GetBool(),
                    _ => false,
                }
            };
        }
    }
}