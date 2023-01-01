using AmongUs.GameOptions;

namespace TownOfHost.PrivateExtensions;

public static class NormalGameOptionsExtensions
{
    public static EngineerRoleOptionsV07 GetEngineerOptions(this NormalGameOptionsV07 options)
    {
        return ((RoleOptionsCollectionV07) options.RoleOptions.Cast<RoleOptionsCollectionV07>()).roles[RoleTypes.Engineer].RoleOptions.Cast<EngineerRoleOptionsV07>();
    }

    public static ScientistRoleOptionsV07 GetScientistOptions(this NormalGameOptionsV07 options)
    {
        return ((RoleOptionsCollectionV07) options.RoleOptions.Cast<RoleOptionsCollectionV07>()).roles[RoleTypes.Scientist].RoleOptions.Cast<ScientistRoleOptionsV07>();
    }

    public static ShapeshifterRoleOptionsV07 GetShapeshifterOptions(this NormalGameOptionsV07 options)
    {
        return ((RoleOptionsCollectionV07) options.RoleOptions.Cast<RoleOptionsCollectionV07>()).roles[RoleTypes.Shapeshifter].RoleOptions.Cast<ShapeshifterRoleOptionsV07>();
    }

    public static GuardianAngelRoleOptionsV07 GetGuardianAngelOptions(this NormalGameOptionsV07 options)
    {
        return ((RoleOptionsCollectionV07) options.RoleOptions.Cast<RoleOptionsCollectionV07>()).roles[RoleTypes.GuardianAngel].RoleOptions.Cast<GuardianAngelRoleOptionsV07>();
    }
}