namespace TownOfHost
{
    static class CustomRolesHelper
    {
        public static bool IsImpostor(this CustomRoles role)
        {
            return
                role is CustomRoles.Impostor or
                CustomRoles.Shapeshifter or
                CustomRoles.BountyHunter or
                CustomRoles.Vampire or
                CustomRoles.Witch or
                CustomRoles.Silencer or
                CustomRoles.Warlock or
                CustomRoles.SerialKiller or
                CustomRoles.Mare or
                CustomRoles.Puppeteer or
                CustomRoles.EvilWatcher or
                CustomRoles.TimeThief or
                CustomRoles.Mafia or
                CustomRoles.FireWorks or
                CustomRoles.Sniper or
                CustomRoles.Swooper or
                CustomRoles.Camouflager or
                CustomRoles.VoteStealer or
                CustomRoles.YingYanger or
                CustomRoles.Grenadier or
                CustomRoles.Miner or
                CustomRoles.Ninja or
                CustomRoles.CorruptedSheriff or
                CustomRoles.EvilGuesser or
                CustomRoles.LastImpostor;
        }
        public static bool IsMadmate(this CustomRoles role)
        {
            return
                role is CustomRoles.Madmate or
                CustomRoles.SKMadmate or
                CustomRoles.MadGuardian or
                CustomRoles.MadSnitch or
                CustomRoles.Parasite or
                CustomRoles.MSchrodingerCat;
        }
        public static bool IsImpostorTeam(this CustomRoles role) => role.IsImpostor() || role.IsMadmate() || role == CustomRoles.CrewPostor;
        public static bool IsNeutral(this CustomRoles role)
        {
            return
                role is CustomRoles.Jester or
                CustomRoles.Vulture or
                CustomRoles.Opportunist or
                CustomRoles.Survivor or
                CustomRoles.SchrodingerCat or
                CustomRoles.Terrorist or
                CustomRoles.Executioner or
                CustomRoles.Arsonist or
                CustomRoles.Egoist or
                CustomRoles.EgoSchrodingerCat or
                CustomRoles.CrewPostor or
                CustomRoles.Marksman or
                CustomRoles.Pirate or
                CustomRoles.Jackal or
                CustomRoles.PlagueBearer or
                CustomRoles.Pestilence or
                CustomRoles.TheGlitch or
                CustomRoles.Werewolf or
                CustomRoles.GuardianAngelTOU or
                CustomRoles.Amnesiac or
                CustomRoles.Juggernaut or
                CustomRoles.Sidekick or
                CustomRoles.JSchrodingerCat or
                CustomRoles.Hacker or
                CustomRoles.Phantom or
                CustomRoles.BloodKnight or
                CustomRoles.HASTroll or
                CustomRoles.Painter or
                CustomRoles.HASFox;
        }
        public static bool IsNeutralBad(this CustomRoles role)
        {
            return
                role is CustomRoles.Vulture or
                CustomRoles.Terrorist or
                //CustomRoles.Executioner or
                CustomRoles.Arsonist or
                CustomRoles.Egoist or
                CustomRoles.EgoSchrodingerCat or
                CustomRoles.CrewPostor or
                CustomRoles.Phantom or
                CustomRoles.Marksman or
                CustomRoles.Pirate or
                CustomRoles.Jackal or
                CustomRoles.PlagueBearer or
                CustomRoles.Pestilence or
                CustomRoles.TheGlitch or
                CustomRoles.Werewolf or
                CustomRoles.Amnesiac or
                CustomRoles.Juggernaut or
                CustomRoles.Sidekick or
                CustomRoles.JSchrodingerCat or
                CustomRoles.Hacker or
                CustomRoles.BloodKnight or
                CustomRoles.HASTroll or
                CustomRoles.Painter;
        }
        public static bool IsNonNK(this CustomRoles role)
        {
            return
                role is CustomRoles.Jester or
                CustomRoles.Vulture or
                CustomRoles.Opportunist or
                CustomRoles.Survivor or
                CustomRoles.SchrodingerCat or
                CustomRoles.Terrorist or
                CustomRoles.Executioner or
                CustomRoles.EgoSchrodingerCat or
                CustomRoles.GuardianAngelTOU or
                CustomRoles.Amnesiac or
                CustomRoles.JSchrodingerCat or
                CustomRoles.Hacker;
        }
        public static bool UsesVents(this CustomRoles role)
        {
            return
                role is CustomRoles.Jester or
                CustomRoles.Terrorist or
                CustomRoles.GuardianAngelTOU;
        }
        public static bool IsNeutralKilling(this CustomRoles role)
        {
            return
                role is CustomRoles.Arsonist or
                CustomRoles.Egoist or
                CustomRoles.Jackal or
                CustomRoles.PlagueBearer or
                CustomRoles.Pestilence or
                CustomRoles.CrewPostor or
                CustomRoles.Sidekick or
                CustomRoles.TheGlitch or
                CustomRoles.Marksman or
                CustomRoles.Werewolf or
                CustomRoles.BloodKnight or
                CustomRoles.Juggernaut;
        }
        public static bool IsJackalTeam(this CustomRoles role)
        {
            return
                role is CustomRoles.Jackal or
                CustomRoles.JSchrodingerCat or
                CustomRoles.Sidekick;
        }
        public static bool IsCrewmate(this CustomRoles role) => !role.IsImpostorTeam() && !role.IsNeutral() && !role.IsCoven();
        public static bool CanGetCrewModifier(this CustomRoles role) => !role.IsImpostorTeam() && !role.IsNeutralBad() && !role.IsCoven();
        public static bool IsVanilla(this CustomRoles role)
        {
            return
                role is CustomRoles.Crewmate or
                CustomRoles.Engineer or
                CustomRoles.Scientist or
                CustomRoles.GuardianAngel or
                CustomRoles.Impostor or
                CustomRoles.Shapeshifter;
        }

        public static bool IsCoven(this CustomRoles role)
        {
            return
                role is CustomRoles.Coven or
                CustomRoles.Poisoner or
                CustomRoles.HexMaster or
                CustomRoles.PotionMaster or
                CustomRoles.CovenWitch or
                CustomRoles.Medusa or
                CustomRoles.Mimic or
                CustomRoles.Conjuror or
                CustomRoles.Necromancer;
        }
        public static bool RoleCannotBeInList(this CustomRoles role)
        {
            return
                role is CustomRoles.Poisoner or
                CustomRoles.HexMaster or
                CustomRoles.PotionMaster or
                CustomRoles.CovenWitch or
                CustomRoles.Bait or
                CustomRoles.Bewilder or
                CustomRoles.Flash or
                CustomRoles.Target or
                CustomRoles.Lovers or
                CustomRoles.LoversRecode or
                CustomRoles.Sleuth or
                CustomRoles.Torch or
                CustomRoles.Medusa or
                CustomRoles.Mimic or
                CustomRoles.Conjuror or
                CustomRoles.Necromancer or
                CustomRoles.EgoSchrodingerCat or
                CustomRoles.Impostor or
                CustomRoles.Phantom or
                CustomRoles.Crewmate or
                CustomRoles.Engineer or
                CustomRoles.GuardianAngel or
                CustomRoles.Pestilence or
                CustomRoles.JSchrodingerCat or
                CustomRoles.LastImpostor or
                CustomRoles.CorruptedSheriff or
                CustomRoles.Sidekick or
                CustomRoles.Painter or
                CustomRoles.Janitor or
                CustomRoles.Painter or
                CustomRoles.Alturist or
                //CustomRoles.Miner or
                CustomRoles.Amnesiac or
                CustomRoles.CSchrodingerCat or
                CustomRoles.MSchrodingerCat;
        }
        public static RoleType GetRoleType(this CustomRoles role)
        {
            RoleType type = RoleType.Crewmate;
            if (role.IsImpostor()) type = RoleType.Impostor;
            if (role.IsNeutral()) type = RoleType.Neutral;
            if (role.IsMadmate()) type = RoleType.Madmate;
            if (role.IsCoven()) type = RoleType.Coven;
            return type;
        }
        public static RoleTeam GetRoleTeam(this CustomRoles role)
        {
            RoleTeam type = RoleTeam.None;
            if (role.IsImpostor()) type = RoleTeam.Evil;
            if (role.IsNeutralKilling()) type = RoleTeam.Killing;
            if (role.IsMadmate()) type = RoleTeam.Benign;
            if (role.IsCoven()) type = RoleTeam.Killing;
            return type;
        }
        public static ModifierType GetModifierType(this CustomRoles role)
        {
            if (role < CustomRoles.NoSubRoleAssigned) return ModifierType.None;
            if (role == CustomRoles.NoSubRoleAssigned) return ModifierType.None;
            if (!role.IsModifier()) return ModifierType.None;
            ModifierType type = ModifierType.Global;
            if (role.IsCrewModifier()) type = ModifierType.Crew;
            return type;
        }
        public static bool IsCrewModifier(this CustomRoles role)
        {
            return
                role is CustomRoles.Bait or
                CustomRoles.Bewilder or
                CustomRoles.Diseased or
                CustomRoles.Torch or
                CustomRoles.Mystic;
        }
        public static bool IsModifier(this CustomRoles role)
        {
            return
                role is CustomRoles.Bait or
                CustomRoles.Bewilder or
                CustomRoles.Flash or
                CustomRoles.Oblivious or
                CustomRoles.Lovers or
                CustomRoles.LoversRecode or
                CustomRoles.Diseased or
                CustomRoles.Watcher or
                CustomRoles.Sleuth or
                CustomRoles.Torch or
                CustomRoles.TieBreaker or
                CustomRoles.Mystic;
        }
        public static bool IsDesyncRole(this CustomRoles role)
        {
            return
                role is CustomRoles.Sheriff or
                CustomRoles.Investigator or
                CustomRoles.Jackal or
                CustomRoles.Sidekick;
        }
        public static void SetCount(this CustomRoles role, int num) => Options.SetRoleCount(role, num);
        public static int GetCount(this CustomRoles role)
        {
            if (role.IsVanilla())
            {
                RoleOptionsData roleOpt = PlayerControl.GameOptions.RoleOptions;
                return role switch
                {
                    CustomRoles.Engineer => roleOpt.GetNumPerGame(RoleTypes.Engineer),
                    CustomRoles.Scientist => roleOpt.GetNumPerGame(RoleTypes.Scientist),
                    CustomRoles.Shapeshifter => roleOpt.GetNumPerGame(RoleTypes.Shapeshifter),
                    CustomRoles.GuardianAngel => roleOpt.GetNumPerGame(RoleTypes.GuardianAngel),
                    CustomRoles.Crewmate => roleOpt.GetNumPerGame(RoleTypes.Crewmate),
                    _ => 0
                };
            }
            else
            {
                return Options.GetRoleCount(role);
            }
        }
        public static float GetChance(this CustomRoles role)
        {
            if (role.IsVanilla())
            {
                RoleOptionsData roleOpt = PlayerControl.GameOptions.RoleOptions;
                return role switch
                {
                    CustomRoles.Engineer => roleOpt.GetChancePerGame(RoleTypes.Engineer),
                    CustomRoles.Scientist => roleOpt.GetChancePerGame(RoleTypes.Scientist),
                    CustomRoles.Shapeshifter => roleOpt.GetChancePerGame(RoleTypes.Shapeshifter),
                    CustomRoles.GuardianAngel => roleOpt.GetChancePerGame(RoleTypes.GuardianAngel),
                    CustomRoles.Crewmate => roleOpt.GetChancePerGame(RoleTypes.Crewmate),
                    _ => 0
                } / 100f;
            }
            else
            {
                return Options.GetRoleChance(role);
            }
        }
        public static bool IsEnable(this CustomRoles role) => role.GetCount() > 0;

        // SPECIFIC ROLE TYPES //
        public static bool IsShapeShifter(this CustomRoles role)
        {
            return
                role is CustomRoles.Shapeshifter or
                CustomRoles.BountyHunter or
                CustomRoles.Warlock or
                CustomRoles.SerialKiller or
                CustomRoles.FireWorks or
                CustomRoles.Sniper or
                CustomRoles.Parasite or
                CustomRoles.Egoist or
                CustomRoles.Camouflager or
                CustomRoles.Grenadier or
                CustomRoles.Miner or
                CustomRoles.Ninja or
                CustomRoles.TheGlitch;
        }
        public static bool IsEngineer(this CustomRoles role)
        {
            if (Options.JesterCanVent.GetBool() && role == CustomRoles.Jester) return true;
            if (Options.VultureCanVent.GetBool() && role == CustomRoles.Vulture) return true;
            if (Options.MadSnitchCanVent.GetBool() && role == CustomRoles.MadSnitch) return true;
            if (Options.MayorHasPortableButton.GetBool() && role == CustomRoles.Mayor) return true;
            return
                role is CustomRoles.Veteran or
                CustomRoles.Engineer or
                CustomRoles.Survivor or
                CustomRoles.Madmate or
                CustomRoles.Bastion or
                CustomRoles.Terrorist or
                CustomRoles.GuardianAngelTOU;
        }
    }
    public enum RoleType
    {
        Crewmate,
        Impostor,
        Neutral,
        Madmate,
        Coven
    }
    public enum RoleTeam
    {
        None,
        Evil,
        Killing,
        Benign,
        Support,
        Protective
    }
    public enum ModifierType
    {
        None,
        Crew,
        Global
    }
}
