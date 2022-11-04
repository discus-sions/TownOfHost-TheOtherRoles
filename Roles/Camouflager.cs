using System.Collections.Generic;
using UnityEngine;

namespace TownOfHost
{
    public static class Camouflager
    {
        static readonly int Id = 2500;

        public static CustomOption CamouflagerCamouflageCoolDown;
        public static CustomOption CamouflagerCamouflageDuration;
        public static CustomOption CamouflagerCanVent;
        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id, CustomRoles.Camouflager, AmongUsExtensions.OptionType.Impostor);
            CamouflagerCamouflageCoolDown = CustomOption.Create(Id + 10, Color.white, "CamouflagerCamouflageCoolDown", AmongUsExtensions.OptionType.Impostor, 30f, 2.5f, 60f, 2.5f, Options.CustomRoleSpawnChances[CustomRoles.Camouflager]);
            CamouflagerCamouflageDuration = CustomOption.Create(Id + 11, Color.white, "CamouflagerCamouflageDuration", AmongUsExtensions.OptionType.Impostor, 15f, 2.5f, 60f, 2.5f, Options.CustomRoleSpawnChances[CustomRoles.Camouflager]);
            CamouflagerCanVent = CustomOption.Create(Id + 12, Color.white, "CamouflagerCanVent", AmongUsExtensions.OptionType.Impostor, true, Options.CustomRoleSpawnChances[CustomRoles.Camouflager]);
        }
        public static void Init()
        {
            DidCamo = false;
        }
        public static bool DidCamo = false;
        public static bool CanVent()
        {
            return CamouflagerCanVent.GetBool();
        }
        public static void ShapeShiftState(PlayerControl shifter, bool shapeshifting, PlayerControl shiftinginto)
        {
            if (DidCamo)
            {
                if (shapeshifting) return;
                if (shifter == null || shifter.Data.IsDead) return;
                Logger.Info($"Camouflager Revert ShapeShift", "Camouflager");
                foreach (PlayerControl revert in PlayerControl.AllPlayerControls)
                {
                    if (revert.Is(CustomRoles.Phantom) || revert == null || revert.Data.IsDead || revert.Data.Disconnected || revert.PlayerId == shifter.PlayerId) continue;
                    revert.RpcRevertShapeshift(true);
                }
                DidCamo = false;
            }
            else if (shapeshifting)
            {
                if (shifter == null || shifter.Data.IsDead) return;
                Logger.Info($"Camouflager ShapeShift", "Camouflager");
                foreach (PlayerControl target in PlayerControl.AllPlayerControls)
                {
                    if (target == shifter) continue;
                    if (target == shiftinginto) continue;
                    if (target.Is(CustomRoles.Phantom)) continue;
                    target.RpcShapeshift(shiftinginto, true);
                }
                DidCamo = true;
            }
        }
    }
}
