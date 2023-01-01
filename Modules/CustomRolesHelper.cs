using System;
using BepInEx;
using AmongUs.GameOptions;

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
                CustomRoles.Vampress or
                CustomRoles.Witch or
                CustomRoles.Silencer or
                CustomRoles.Warlock or
                CustomRoles.Consort or
                CustomRoles.Morphling or
                CustomRoles.SerialKiller or
                CustomRoles.Mare or
                CustomRoles.ImpostorGhost or
                CustomRoles.Puppeteer or
                CustomRoles.TimeThief or
                CustomRoles.Mafia or
                CustomRoles.FireWorks or
                CustomRoles.IdentityTheft or
                CustomRoles.Bomber or
                CustomRoles.Manipulator or
                CustomRoles.Sniper or
                CustomRoles.Swooper or
                CustomRoles.Camouflager or
                CustomRoles.VoteStealer or
                CustomRoles.YingYanger or
                CustomRoles.Grenadier or
                CustomRoles.Freezer or
                CustomRoles.Cleaner or
                CustomRoles.Miner or
                CustomRoles.Ninja or
                CustomRoles.CorruptedSheriff or
                CustomRoles.Disperser or
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
                CustomRoles.AgiTater or
                CustomRoles.EgoSchrodingerCat or
                CustomRoles.Hitman or
                CustomRoles.CrewPostor or
                CustomRoles.Marksman or
                CustomRoles.PoisonMaster or
                CustomRoles.Pirate or
                CustomRoles.Jackal or
                CustomRoles.PlagueBearer or
                CustomRoles.Pestilence or
                CustomRoles.TheGlitch or
                CustomRoles.Postman or
                CustomRoles.Werewolf or
                CustomRoles.Swapper or
                CustomRoles.GuardianAngelTOU or
                CustomRoles.NeutWitch or
                CustomRoles.Amnesiac or
                CustomRoles.Juggernaut or
                CustomRoles.Sidekick or
                CustomRoles.JSchrodingerCat or
                CustomRoles.Hacker or
                CustomRoles.Phantom or
                CustomRoles.BloodKnight or
                CustomRoles.HASTroll or
                CustomRoles.Painter or
                CustomRoles.HASFox or // CAT
                CustomRoles.BKSchrodingerCat or
                CustomRoles.CPSchrodingerCat or
                CustomRoles.JugSchrodingerCat or
                CustomRoles.MMSchrodingerCat or
                CustomRoles.PesSchrodingerCat or
                CustomRoles.WWSchrodingerCat or
                CustomRoles.TGSchrodingerCat;
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
                CustomRoles.PoisonMaster or
                CustomRoles.Jackal or
                CustomRoles.Postman or
                CustomRoles.NeutWitch or
                CustomRoles.PlagueBearer or
                CustomRoles.Pestilence or
                CustomRoles.TheGlitch or
                CustomRoles.AgiTater or
                CustomRoles.Werewolf or
                CustomRoles.Amnesiac or
                CustomRoles.Juggernaut or
                CustomRoles.Sidekick or
                CustomRoles.JSchrodingerCat or
                CustomRoles.Hacker or
                CustomRoles.BloodKnight or
                CustomRoles.HASTroll or
                CustomRoles.Painter or // CAT
                CustomRoles.BKSchrodingerCat or
                CustomRoles.CPSchrodingerCat or
                CustomRoles.JugSchrodingerCat or
                CustomRoles.MMSchrodingerCat or
                CustomRoles.PesSchrodingerCat or
                CustomRoles.WWSchrodingerCat or
                CustomRoles.TGSchrodingerCat;
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
                CustomRoles.AgiTater or
                CustomRoles.Marksman or
                CustomRoles.Werewolf or
                CustomRoles.Pirate or
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
                CustomRoles.Torch;
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
                CustomRoles.TieBreaker;
        }
        public static bool IsDesyncRole(this CustomRoles role)
        {
            return
                role is CustomRoles.Sheriff or
                CustomRoles.Investigator or
                CustomRoles.Parasite or
                CustomRoles.Escort or
                CustomRoles.Hitman or
                CustomRoles.Escort or
                CustomRoles.BloodKnight or
                CustomRoles.Hitman or
                CustomRoles.Jackal or
                CustomRoles.Crusader or
                CustomRoles.Sidekick or
                CustomRoles.Arsonist or
                CustomRoles.Egoist or
                CustomRoles.Jackal or
                CustomRoles.PlagueBearer or
                CustomRoles.Pestilence or
                CustomRoles.Sidekick or
                CustomRoles.TheGlitch or
                CustomRoles.AgiTater or
                CustomRoles.Marksman or
                CustomRoles.Werewolf or
                CustomRoles.BloodKnight or
                CustomRoles.Juggernaut;
        }
        public static bool CanRoleBlock(this CustomRoles role)
        {
            return
                role is CustomRoles.TheGlitch or
                CustomRoles.Escort or
                CustomRoles.Consort;
        }
        public static bool HostRedName(this CustomRoles role) => /*AmongUsClient.Instance.AmHost && role is CustomRoles.Hitman or CustomRoles.Crusader or CustomRoles.Escort or CustomRoles.NeutWitch;*/ false;
        public static void SetCount(this CustomRoles role, int num) => Options.SetRoleCount(role, num);
        public static int GetCount(this CustomRoles role)
        {
            if (role.IsVanilla())
            {
                IRoleOptionsCollection roleOpt = GameOptionsManager.Instance.CurrentGameOptions.RoleOptions;
                return role switch
                {
                    CustomRoles.Engineer => roleOpt.GetNumPerGame(AmongUs.GameOptions.RoleTypes.Engineer),
                    CustomRoles.Scientist => roleOpt.GetNumPerGame(AmongUs.GameOptions.RoleTypes.Scientist),
                    CustomRoles.Shapeshifter => roleOpt.GetNumPerGame(AmongUs.GameOptions.RoleTypes.Shapeshifter),
                    CustomRoles.GuardianAngel => roleOpt.GetNumPerGame(AmongUs.GameOptions.RoleTypes.GuardianAngel),
                    CustomRoles.Crewmate => roleOpt.GetNumPerGame(AmongUs.GameOptions.RoleTypes.Crewmate),
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
                AmongUs.GameOptions.RoleOptionsData roleOpt = null;
                return role switch
                {
                    CustomRoles.Engineer => roleOpt.GetChancePerGame(AmongUs.GameOptions.RoleTypes.Engineer),
                    CustomRoles.Scientist => roleOpt.GetChancePerGame(AmongUs.GameOptions.RoleTypes.Scientist),
                    CustomRoles.Shapeshifter => roleOpt.GetChancePerGame(AmongUs.GameOptions.RoleTypes.Shapeshifter),
                    CustomRoles.GuardianAngel => roleOpt.GetChancePerGame(AmongUs.GameOptions.RoleTypes.GuardianAngel),
                    CustomRoles.Crewmate => roleOpt.GetChancePerGame(AmongUs.GameOptions.RoleTypes.Crewmate),
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
                CustomRoles.Morphling or
                CustomRoles.FireWorks or
                CustomRoles.Sniper or
                CustomRoles.Consort or
                CustomRoles.Parasite or
                CustomRoles.Egoist or
                CustomRoles.Disperser or
                CustomRoles.Freezer or
                CustomRoles.Camouflager or
                CustomRoles.Vampress or
                CustomRoles.Grenadier or
                CustomRoles.Ninja or
                CustomRoles.TheGlitch;
        }
        public static bool PetActivatedAbility(this CustomRoles role)
        {
            return
                role is CustomRoles.Veteran or
                CustomRoles.Miner or
                CustomRoles.TheGlitch or
                CustomRoles.Transporter;
        }
        public static bool IsEngineer(this CustomRoles role)
        {
            if (Options.JesterCanVent.GetBool() && role == CustomRoles.Jester) return true;
            if (Options.VultureCanVent.GetBool() && role == CustomRoles.Vulture) return true;
            if (Options.MadSnitchCanVent.GetBool() && role == CustomRoles.MadSnitch) return true;
            if (Options.MayorHasPortableButton.GetBool() && role == CustomRoles.Mayor) return true;
            if (Options.MediumArrow.GetBool() && role == CustomRoles.Medium) return true;
            return
                role is CustomRoles.Engineer or
                CustomRoles.Survivor or
                CustomRoles.Madmate or
                CustomRoles.Bastion or
                CustomRoles.Mechanic or
                CustomRoles.Terrorist or
                CustomRoles.GuardianAngelTOU;
        }
        // CAT STUFF //
        public static bool IsShieldedRole(this CustomRoles role)
        {
            return
             role is CustomRoles.Arsonist or
             CustomRoles.Investigator or
             CustomRoles.PlagueBearer or
             CustomRoles.AgiTater;
        }
        public static bool IsGuesser(this CustomRoles role)
        {
            if (role.IsCoven() && role != CustomRoles.Mimic)
                return true;
            return
             role is CustomRoles.Pirate or
             CustomRoles.NiceGuesser or
             CustomRoles.EvilGuesser;
        }
        // MISC //
        public static bool RoleGoingInList(this CustomRoles role)
        {
            if (!role.IsEnable()) return false;
            var number = Convert.ToUInt32(PercentageChecker.CheckPercentage(role.ToString(), role: role));
            bool isRole = UnityEngine.Random.RandomRange(1, 100) <= number;
            return isRole;
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
