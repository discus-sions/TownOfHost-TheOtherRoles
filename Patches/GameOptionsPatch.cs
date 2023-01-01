using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using UnityEngine;
using System;
using InnerNet;
using Object = UnityEngine.Object;
using AmongUs.GameOptions;

namespace TownOfHost
{
    [HarmonyPatch(typeof(RoleOptionSetting), nameof(RoleOptionSetting.UpdateValuesAndText))]
    class ChanceChangePatch
    {
        public static void Postfix(RoleOptionSetting __instance)
        {
            bool forced = false;
            if (__instance.Role.Role == RoleTypes.Scientist)
            {
                __instance.TitleText.color = Utils.GetRoleColor(CustomRoles.Scientist);
                if (CustomRoles.Doctor.IsEnable()) forced = true;
            }
            if (__instance.Role.Role == RoleTypes.Engineer)
            {
                __instance.TitleText.color = Utils.GetRoleColor(CustomRoles.Engineer);
                foreach (CustomRoles role in Enum.GetValues(typeof(CustomRoles)))
                {
                    if (role.IsEnable() && role.IsEngineer() && !forced)
                        forced = true;
                }
            }
            if (__instance.Role.Role == RoleTypes.GuardianAngel)
            {
                __instance.TitleText.color = Utils.GetRoleColor(CustomRoles.GuardianAngel);
            }
            if (__instance.Role.Role == RoleTypes.Shapeshifter)
            {
                __instance.TitleText.color = Utils.GetRoleColor(CustomRoles.Shapeshifter);
                foreach (CustomRoles role in Enum.GetValues(typeof(CustomRoles)))
                {
                    if (role.IsEnable() && role.IsShapeShifter() && !forced)
                        forced = true;
                }
            }

            if (forced)
            {
                ((TMPro.TMP_Text)__instance.ChanceText).text = "Always";
            }
        }
    }
}