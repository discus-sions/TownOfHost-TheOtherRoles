using HarmonyLib;
using UnityEngine;

namespace TownOfHost
{
    [HarmonyPatch(typeof(Console), nameof(Console.CanUse))]
    class CanUsePatch
    {
        public static bool Prefix(ref float __result, Console __instance, [HarmonyArgument(0)] GameData.PlayerInfo pc, [HarmonyArgument(1)] out bool canUse, [HarmonyArgument(2)] out bool couldUse)
        {
            canUse = couldUse = false;
            //こいつをfalseでreturnしても、タスク(サボ含む)以外の使用可能な物は使えるまま(ボタンなど)
            return __instance.AllowImpostor || Utils.HasTasks(PlayerControl.LocalPlayer.Data, false);
        }
    }
    [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Update))]
    class EmergencyMinigamePatch
    {
        public static void Postfix(EmergencyMinigame __instance)
        {
            if (Options.CurrentGameMode() == CustomGameMode.HideAndSeek) __instance.Close();
        }
    }
    [HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
    class CanUseVentPatch
    {
        public static bool Prefix(Vent __instance, [HarmonyArgument(0)] GameData.PlayerInfo pc,
            [HarmonyArgument(1)] ref bool canUse,
            [HarmonyArgument(2)] ref bool couldUse,
            ref float __result)
        {
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //#######################################
            //     ==ベント処理==
            //#######################################
            //参考:https://github.com/Eisbison/TheOtherRoles/blob/main/TheOtherRoles/Patches/UsablesPatch.cs

            bool VentForTrigger = false;
            float num = float.MaxValue;

            var usableDistance = __instance.UsableDistance;

            if (pc.IsDead) return false; //死んでる人は強制的にfalseに。
            else if (pc.Object.Is(CustomRoles.Sheriff) || pc.Object.Is(CustomRoles.PlagueBearer) || pc.Object.Is(CustomRoles.Amnesiac) || pc.Object.Is(CustomRoles.Escort) || pc.Object.Is(CustomRoles.Crusader) || pc.Object.Is(CustomRoles.Janitor) || pc.Object.Is(CustomRoles.Investigator) || (pc.Object.Is(CustomRoles.Arsonist) && !pc.Object.IsDouseDone() && !Options.TOuRArso.GetBool()))
                return false;
            else if (pc.Object.Is(CustomRoles.Arsonist) && pc.Object.IsDouseDone() && !Options.TOuRArso.GetBool())
                canUse = couldUse = VentForTrigger = true;
            else if (pc.Object.Is(CustomRoles.Arsonist) && Options.TOuRArso.GetBool())
                canUse = couldUse = true;
            else if (pc.Object.Is(CustomRoles.Jackal) || pc.Object.Is(CustomRoles.Sidekick))
                canUse = couldUse = Options.JackalCanVent.GetBool();
            else if (pc.Object.Is(CustomRoles.Jester))
                canUse = couldUse = Options.JesterCanVent.GetBool();
            else if (pc.Object.Is(CustomRoles.Pestilence))
                canUse = couldUse = Options.PestiCanVent.GetBool();
            else if (pc.Object.Is(CustomRoles.Grenadier))
                canUse = couldUse = Options.GrenadierCanVent.GetBool();
            else if (pc.Object.Is(CustomRoles.Juggernaut))
                canUse = couldUse = Options.JuggerCanVent.GetBool();
            else if (pc.Object.Is(CustomRoles.Marksman))
                canUse = couldUse = Options.MarksmanCanVent.GetBool();
            else if (pc.Object.Is(CustomRoles.Camouflager))
                canUse = couldUse = Camouflager.CanVent();
            else if (pc.Object.Is(CustomRoles.Hitman))
                canUse = couldUse = Options.HitmanCanVent.GetBool();
            else if (pc.Object.Is(CustomRoles.BloodKnight))
                canUse = couldUse = Options.BKcanVent.GetBool();
            else if (pc.Object.Is(CustomRoles.TheGlitch))
                canUse = couldUse = true;
            else if (pc.Object.Is(CustomRoles.Painter))
                canUse = couldUse = Options.STIgnoreVent.GetBool();
            else if (pc.Object.Is(CustomRoles.Janitor))
                canUse = couldUse = Options.STIgnoreVent.GetBool();
            else if (pc.Object.Is(CustomRoles.Werewolf))
                canUse = couldUse = true;
            else if (pc.Object.Is(CustomRoles.CorruptedSheriff))
                canUse = couldUse = true;
            else if (pc.Object.Is(CustomRoles.Necromancer))
                canUse = couldUse = Necromancer.CanUseVent();
            else if (pc.Object.Is(CustomRoles.CovenWitch) && Main.HasNecronomicon)
                canUse = couldUse = true;
            else if (pc.Object.Is(CustomRoles.HexMaster) && Main.HasNecronomicon)
                canUse = couldUse = true;
            else if (pc.Object.Is(CustomRoles.Medusa))
                canUse = couldUse = true;
            else if (pc.Role.TeamType == RoleTeamTypes.Impostor || pc.Role.Role == RoleTypes.Engineer) // インポスター陣営ベースの役職とエンジニアベースの役職は常にtrue
                canUse = couldUse = true;

            canUse = couldUse = (pc.Object.inVent || canUse) && (pc.Object.CanMove || pc.Object.inVent);
            //canUse = couldUse = true;

            if (VentForTrigger && pc.Object.inVent)
            {
                canUse = couldUse = false;
                return false;
            }
            if (canUse)
            {
                Vector2 truePosition = pc.Object.GetTruePosition();
                Vector3 position = __instance.transform.position;
                num = Vector2.Distance(truePosition, position);
                canUse &= num <= usableDistance && !PhysicsHelpers.AnythingBetween(truePosition, position, Constants.ShipOnlyMask, false);
            }
            __result = num;
            return false;
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        }
    }
}
