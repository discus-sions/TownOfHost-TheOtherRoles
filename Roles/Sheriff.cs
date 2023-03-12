using System.Collections.Generic;
using Hazel;
using System;
using UnityEngine;

namespace TownOfHost
{
    public static class Sheriff
    {
        private static readonly int Id = 20400;
        public static List<byte> playerIdList = new();
        public static PlayerControl seer;
        public static bool csheriff = false;

        private static CustomOption KillCooldown;
        private static CustomOption CanKillArsonist;
        private static CustomOption CanKillMadmate;
        private static CustomOption CanKillJester;
        private static CustomOption CanKillTerrorist;
        private static CustomOption CanKillOpportunist;
        private static CustomOption CanKillEgoist;
        private static CustomOption CanKillEgoShrodingerCat;
        private static CustomOption CanKillExecutioner;
        private static CustomOption CanKillJackal;
        private static CustomOption CanKillJShrodingerCat;
        private static CustomOption CanKillPlagueBearer;
        private static CustomOption CanKillCrewmatesAsIt;
        private static CustomOption CanKillJug;
        private static CustomOption ShotLimitOpt;
        // public static CustomOption PlayersForTraitor;
        // public static CustomOption SheriffCorrupted;
        private static CustomOption CanKillVulture;
        private static CustomOption SheriffCanKillCoven;
        private static CustomOption CanKillGlitch;
        private static CustomOption CanKillWerewolf;
        private static CustomOption CanKillHitman;
        private static CustomOption CanKillAgitater;
        public static CustomOption NoDeathPenalty;
        public static CustomOption CanKillPostman;

        public static Dictionary<byte, float> ShotLimit = new();
        public static Dictionary<byte, float> CurrentKillCooldown = new();
        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id, CustomRoles.Sheriff, AmongUsExtensions.OptionType.Crewmate);
            KillCooldown = CustomOption.Create(Id + 10, Color.white, "SheriffKillCooldown", AmongUsExtensions.OptionType.Crewmate, 30, 0, 120, 1, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillArsonist = CustomOption.Create(Id + 11, Color.white, "SheriffCanKillArsonist", AmongUsExtensions.OptionType.Crewmate, true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillMadmate = CustomOption.Create(Id + 12, Color.white, "SheriffCanKillMadmate", AmongUsExtensions.OptionType.Crewmate, true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillJester = CustomOption.Create(Id + 13, Color.white, "SheriffCanKillJester", AmongUsExtensions.OptionType.Crewmate, true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillTerrorist = CustomOption.Create(Id + 14, Color.white, "SheriffCanKillTerrorist", AmongUsExtensions.OptionType.Crewmate, true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillOpportunist = CustomOption.Create(Id + 15, Color.white, "SheriffCanKillOpportunist", AmongUsExtensions.OptionType.Crewmate, true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillEgoist = CustomOption.Create(Id + 16, Color.white, "SheriffCanKillEgoist", AmongUsExtensions.OptionType.Crewmate, true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillEgoShrodingerCat = CustomOption.Create(Id + 17, Color.white, "SheriffCanKillEgoShrodingerCat", AmongUsExtensions.OptionType.Crewmate, true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillExecutioner = CustomOption.Create(Id + 18, Color.white, "SheriffCanKillExecutioner", AmongUsExtensions.OptionType.Crewmate, true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillJackal = CustomOption.Create(Id + 19, Color.white, "SheriffCanKillJackal", AmongUsExtensions.OptionType.Crewmate, true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillJShrodingerCat = CustomOption.Create(Id + 20, Color.white, "SheriffCanKillJShrodingerCat", AmongUsExtensions.OptionType.Crewmate, true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillPlagueBearer = CustomOption.Create(Id + 21, Color.white, "SheriffCanKillPB", AmongUsExtensions.OptionType.Crewmate, true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillJug = CustomOption.Create(Id + 22, Color.white, "SheriffCanKillJug", AmongUsExtensions.OptionType.Crewmate, true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            SheriffCanKillCoven = CustomOption.Create(Id + 23, Color.white, "SheriffCanKillCoven", AmongUsExtensions.OptionType.Crewmate, true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillVulture = CustomOption.Create(Id + 24, Color.white, "SheriffCanKillVulture", AmongUsExtensions.OptionType.Crewmate, true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillGlitch = CustomOption.Create(Id + 25, Color.white, "SCKTG", AmongUsExtensions.OptionType.Crewmate, true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillWerewolf = CustomOption.Create(Id + 26, Color.white, "SCKWW", AmongUsExtensions.OptionType.Crewmate, true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillHitman = CustomOption.Create(Id + 29, Color.white, "SheriffCanKillHitman", AmongUsExtensions.OptionType.Crewmate, true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillAgitater = CustomOption.Create(Id + 30, Color.white, "SheriffCanKillAgitater", AmongUsExtensions.OptionType.Crewmate, true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillPostman = CustomOption.Create(Id + 32, Color.white, "SheriffCanKillPostman", AmongUsExtensions.OptionType.Crewmate, true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillCrewmatesAsIt = CustomOption.Create(Id + 27, Color.white, "SheriffCanKillCrewmatesAsIt", AmongUsExtensions.OptionType.Crewmate, false, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            NoDeathPenalty = CustomOption.Create(Id + 31, Color.white, "NoDeathPenalty", AmongUsExtensions.OptionType.Crewmate, false, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            ShotLimitOpt = CustomOption.Create(Id + 28, Color.white, "SheriffShotLimit", AmongUsExtensions.OptionType.Crewmate, 15, 0, 15, 1, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
        }
        public static void Init()
        {
            playerIdList = new();
            ShotLimit = new();
            CurrentKillCooldown = new();
            seer = null;
            var number = System.Convert.ToUInt32(PercentageChecker.CheckPercentage(CustomRoles.CorruptedSheriff.ToString(), role: CustomRoles.CorruptedSheriff));
            csheriff = UnityEngine.Random.Range(1, 100) <= number;
        }
        public static void Add(byte playerId)
        {
            playerIdList.Add(playerId);
            CurrentKillCooldown.Add(playerId, KillCooldown.GetFloat());

            if (!Main.ResetCamPlayerList.Contains(playerId))
                Main.ResetCamPlayerList.Add(playerId);

            ShotLimit.TryAdd(playerId, ShotLimitOpt.GetFloat());
            Logger.Info($"{Utils.GetPlayerById(playerId)?.GetNameWithRole()} : 残り{ShotLimit[playerId]}発", "Sheriff");
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
            if (ShotLimit.ContainsKey(SheriffId))
                ShotLimit[SheriffId] = Limit;
            else
                ShotLimit.Add(SheriffId, ShotLimitOpt.GetFloat());
        }
        public static void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = CurrentKillCooldown[id];
        public static bool CanUseKillButton(PlayerControl player)
        {
            if (player.Data.IsDead)
                return false;

            if (ShotLimitOpt.GetFloat() == 0)
                return true;

            if (ShotLimit[player.PlayerId] == 0)
            {
                //Logger.info($"{player.GetNameWithRole()} はキル可能回数に達したため、RoleTypeを守護天使に変更しました。", "Sheriff");
                //player.RpcSetRoleDesync(RoleTypes.GuardianAngel);
                //Utils.hasTasks(player.Data, false);
                //Utils.NotifyRoles();
                return false;
            }
            return true;
        }
        public static bool OnCheckMurder(PlayerControl killer, PlayerControl target, string Process)
        {
            switch (Process)
            {
                case "RemoveShotLimit":
                    ShotLimit[killer.PlayerId]--;
                    Logger.Info($"{killer.GetNameWithRole()} : 残り{ShotLimit[killer.PlayerId]}発", "Sheriff");
                    SendRPC(killer.PlayerId);
                    //SwitchToCorrupt(killer, target);
                    break;
                case "Suicide":
                    if (!target.CanBeKilledBySheriff())
                    {
                        PlayerState.SetDeathReason(killer.PlayerId, PlayerState.DeathReason.Misfire);
                        killer.RpcMurderPlayer(killer);
                        if (CanKillCrewmatesAsIt.GetBool())
                            killer.RpcMurderPlayer(target);
                        return false;
                    }
                    break;
            }
            return true;
        }
        public static void SwitchToCorrupt(PlayerControl killer, PlayerControl target)
        {
            try {
            if (!csheriff)
                if ((target.GetCustomRole().IsImpostor() | target.GetCustomRole().IsNeutralKilling() | target.GetCustomRole().IsCoven() | Main.AliveImpostorCount <= 0) && CustomRoles.CorruptedSheriff.IsEnable())
                {
                    if (Options.SheriffCorrupted.GetBool())
                    {
                        if (!csheriff)
                        {
                            int IsAlive = 0;
                            int numCovenAlive = 0;
                            int numImpsAlive = 0;
                            int numNKalive = 0;
                            List<PlayerControl> couldBeTraitors = new();
                            List<byte> couldBeTraitorsid = new();
                            var rando = new System.Random();
                            foreach (var pc in PlayerControl.AllPlayerControls)
                            {
                                if (pc == null) continue;
                                if (!pc.Data.Disconnected)
                                    if (!pc.Data.IsDead)
                                    {
                                        IsAlive++;
                                        if (pc.GetCustomRole().IsNeutralKilling() && !Options.TraitorCanSpawnIfNK.GetBool())
                                            numNKalive++;
                                        if (pc.GetCustomRole().IsCoven() && !Options.TraitorCanSpawnIfCoven.GetBool())
                                            numCovenAlive++;
                                        if (pc.Is(CustomRoles.Sheriff) || pc.Is(CustomRoles.Investigator) || pc.Is(CustomRoles.Hitman))
                                            couldBeTraitors.Add(pc);
                                        if (pc.Is(CustomRoles.Sheriff) || pc.Is(CustomRoles.Investigator) || pc.Is(CustomRoles.Hitman))
                                            couldBeTraitorsid.Add(pc.PlayerId);
                                        if (pc.GetCustomRole().IsImpostor())
                                            numImpsAlive++;
                                    }
                            }

                            foreach (var pc in PlayerControl.AllPlayerControls)
                            {
                                if (pc == null) continue;
                                if (!pc.Data.Disconnected)
                                    if (!pc.Data.IsDead)
                                    {
                                        if (!pc.IsModClient()) continue;
                                        if (!pc.GetCustomRole().IsCrewmate()) continue;
                                        if (!couldBeTraitorsid.Contains(pc.PlayerId))
                                        {
                                            couldBeTraitors.Add(pc);
                                            couldBeTraitorsid.Add(pc.PlayerId);
                                        }
                                    }
                            }

                            seer = couldBeTraitors[rando.Next(0, couldBeTraitors.Count)];

                            //foreach (var pva in __instance.playerStates)
                            if (IsAlive >= Options.PlayersForTraitor.GetFloat() && seer != null)
                            {
                                if (seer.GetCustomRole() == CustomRoles.Sheriff && numCovenAlive == 0 && numNKalive == 0 && numImpsAlive == 0)
                                {
                                    seer.RpcSetCustomRole(CustomRoles.CorruptedSheriff);
                                    seer.CustomSyncSettings();
                                    csheriff = true;
                                    RPC.SetTraitor(seer.PlayerId);
                                }
                            }
                        }
                    }
                }
            } catch (Exception e)
            {
                Logger.Error($"Error encountered while checking if Sheriff could turn into corrupt.\n{e}", "Sheriff.cs");
            }
        }

        public static string GetShotLimit(byte playerId)
        {
            if (ShotLimitOpt.GetInt() == 0) return "";
            return Helpers.ColorString(Color.yellow, ShotLimit.TryGetValue(playerId, out var shotLimit) ? $"({shotLimit})" : "Invalid");
        }
        public static bool CanBeKilledBySheriff(this PlayerControl player)
        {
            var cRole = player.GetCustomRole();
            return cRole switch
            {
                CustomRoles.Jester => CanKillJester.GetBool(),
                CustomRoles.Terrorist => CanKillTerrorist.GetBool(),
                CustomRoles.CrewPostor => CanKillTerrorist.GetBool(),
                CustomRoles.Executioner => CanKillExecutioner.GetBool(),
                CustomRoles.Swapper => CanKillExecutioner.GetBool(),
                CustomRoles.Opportunist => CanKillOpportunist.GetBool(),
                CustomRoles.Survivor => CanKillOpportunist.GetBool(),
                CustomRoles.Arsonist => CanKillArsonist.GetBool(),
                CustomRoles.Egoist => CanKillEgoist.GetBool(),
                CustomRoles.EgoSchrodingerCat => CanKillEgoShrodingerCat.GetBool(),
                CustomRoles.Jackal => CanKillJackal.GetBool(),
                CustomRoles.Sidekick => CanKillJackal.GetBool(),
                CustomRoles.JSchrodingerCat => CanKillJShrodingerCat.GetBool(),
                CustomRoles.PlagueBearer => CanKillPlagueBearer.GetBool(),
                CustomRoles.Juggernaut => CanKillJug.GetBool(),
                CustomRoles.Marksman => CanKillJug.GetBool(),
                CustomRoles.Hitman => CanKillHitman.GetBool(),
                CustomRoles.BloodKnight => CanKillJug.GetBool(),
                CustomRoles.Vulture => CanKillVulture.GetBool(),
                CustomRoles.TheGlitch => CanKillGlitch.GetBool(),
                CustomRoles.Werewolf => CanKillWerewolf.GetBool(),
                CustomRoles.AgiTater => CanKillAgitater.GetBool(),
                CustomRoles.Pirate => true,
                // COVEN //
                CustomRoles.Coven => SheriffCanKillCoven.GetBool(),
                CustomRoles.CovenWitch => SheriffCanKillCoven.GetBool(),
                CustomRoles.Poisoner => SheriffCanKillCoven.GetBool(),
                CustomRoles.HexMaster => SheriffCanKillCoven.GetBool(),
                CustomRoles.PotionMaster => SheriffCanKillCoven.GetBool(),
                CustomRoles.Medusa => SheriffCanKillCoven.GetBool(),
                CustomRoles.Mimic => SheriffCanKillCoven.GetBool(),
                CustomRoles.Necromancer => SheriffCanKillCoven.GetBool(),
                CustomRoles.Conjuror => SheriffCanKillCoven.GetBool(),
                // AFTER COVEN //
                CustomRoles.SchrodingerCat => true,
                CustomRoles.Phantom => true,
                CustomRoles.Hacker => true,
                CustomRoles.NeutWitch => true,
                _ => cRole.GetRoleType() switch
                {
                    RoleType.Impostor => true,
                    RoleType.Madmate => CanKillMadmate.GetBool(),
                    _ => false,
                }
            };
        }
    }
}