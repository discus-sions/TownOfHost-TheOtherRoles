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
        //public static CustomOption TraitorCanSpawnIfNK;
        //public static CustomOption TraitorCanSpawnIfCoven;

        public static Dictionary<byte, float> ShotLimit = new();
        public static Dictionary<byte, float> CurrentKillCooldown = new();
        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id, CustomRoles.Sheriff);
            KillCooldown = CustomOption.Create(Id + 10, Color.white, "SheriffKillCooldown", 30, 0, 990, 1, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillArsonist = CustomOption.Create(Id + 11, Color.white, "SheriffCanKillArsonist", true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillMadmate = CustomOption.Create(Id + 12, Color.white, "SheriffCanKillMadmate", true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillJester = CustomOption.Create(Id + 13, Color.white, "SheriffCanKillJester", true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillTerrorist = CustomOption.Create(Id + 14, Color.white, "SheriffCanKillTerrorist", true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillOpportunist = CustomOption.Create(Id + 15, Color.white, "SheriffCanKillOpportunist", true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillEgoist = CustomOption.Create(Id + 16, Color.white, "SheriffCanKillEgoist", true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillEgoShrodingerCat = CustomOption.Create(Id + 17, Color.white, "SheriffCanKillEgoShrodingerCat", true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillExecutioner = CustomOption.Create(Id + 18, Color.white, "SheriffCanKillExecutioner", true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillJackal = CustomOption.Create(Id + 19, Color.white, "SheriffCanKillJackal", true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillJShrodingerCat = CustomOption.Create(Id + 20, Color.white, "SheriffCanKillJShrodingerCat", true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillPlagueBearer = CustomOption.Create(Id + 21, Color.white, "SheriffCanKillPB", true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillJug = CustomOption.Create(Id + 22, Color.white, "SheriffCanKillJug", true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            SheriffCanKillCoven = CustomOption.Create(Id + 23, Color.white, "SheriffCanKillCoven", true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillVulture = CustomOption.Create(Id + 24, Color.white, "SheriffCanKillVulture", true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillGlitch = CustomOption.Create(Id + 25, Color.white, "SCKTG", true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillWerewolf = CustomOption.Create(Id + 26, Color.white, "SCKWW", true, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            CanKillCrewmatesAsIt = CustomOption.Create(Id + 27, Color.white, "SheriffCanKillCrewmatesAsIt", false, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            ShotLimitOpt = CustomOption.Create(Id + 28, Color.white, "SheriffShotLimit", 99, -1, 15, 1, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            /*SheriffCorrupted = CustomOption.Create(Id + 29, Color.white, "TurnCorrupt", false, Options.CustomRoleSpawnChances[CustomRoles.Sheriff]);
            PlayersForTraitor = CustomOption.Create(Id + 30, Color.white, "TraitorSpawn", 1, 0, 15, 1, SheriffCorrupted);
            TraitorCanSpawnIfNK = CustomOption.Create(Id + 31, Color.white, "TraitorCanSpawnIfNK", true, SheriffCorrupted);
            TraitorCanSpawnIfCoven = CustomOption.Create(Id + 32, Color.white, "TraitorCanSpawnIfCoven", true, SheriffCorrupted);*/
        }
        public static void Init()
        {
            playerIdList = new();
            ShotLimit = new();
            CurrentKillCooldown = new();
            seer = null;
            var number = System.Convert.ToUInt32(PercentageChecker.CheckPercentage(CustomRoles.CorruptedSheriff.ToString(), role: CustomRoles.CorruptedSheriff));
            bool role = UnityEngine.Random.Range(1, 100) <= number;
            csheriff = true;
            if (role)
                csheriff = false;
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
            if (!csheriff)
                if (target.CurrentlyLastImpostor())
                {
                    //bool LocalPlayerKnowsImpostor = false;
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
                            //PlayerControl seer = PlayerControl.LocalPlayer;
                            foreach (var pc in PlayerControl.AllPlayerControls)
                            {
                                if (!pc.Data.Disconnected)
                                    if (!pc.Data.IsDead)
                                    {
                                        IsAlive++;
                                        if (pc.GetCustomRole().IsNeutralKilling() && !Options.TraitorCanSpawnIfNK.GetBool())
                                            numNKalive++;
                                        if (pc.GetCustomRole().IsCoven() && !Options.TraitorCanSpawnIfCoven.GetBool())
                                            numCovenAlive++;
                                        if (pc.Is(CustomRoles.Sheriff) || pc.Is(CustomRoles.Investigator))
                                            couldBeTraitors.Add(pc);
                                        if (pc.Is(CustomRoles.Sheriff) || pc.Is(CustomRoles.Investigator))
                                            couldBeTraitorsid.Add(pc.PlayerId);
                                        if (pc.GetCustomRole().IsImpostor())
                                            numImpsAlive++;
                                    }
                            }

                            foreach (var pc in PlayerControl.AllPlayerControls)
                            {
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
                                    if (seer.IsModClient())
                                        RoleManager.Instance.SetRole(seer, RoleTypes.Impostor);
                                }
                            }
                        }
                    }
                }
        }
        public static string GetShotLimit(byte playerId) => Helpers.ColorString(Color.yellow, ShotLimit.TryGetValue(playerId, out var shotLimit) ? $"({shotLimit})" : "Invalid");
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
                CustomRoles.BloodKnight => CanKillJug.GetBool(),
                CustomRoles.Vulture => CanKillVulture.GetBool(),
                CustomRoles.TheGlitch => CanKillGlitch.GetBool(),
                CustomRoles.Werewolf => CanKillWerewolf.GetBool(),
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
                CustomRoles.Hacker => true,
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