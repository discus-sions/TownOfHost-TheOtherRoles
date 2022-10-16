using HarmonyLib;
using UnityEngine;

namespace TownOfHost
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class KillButtonSprite
    {
        private static Sprite Douse => Main.DouseSprite;
        private static Sprite Hack => Main.HackSprite;
        private static Sprite Infect => Main.InfectSprite;
        private static Sprite Poison => Main.PoisonSprite;
        private static Sprite Seer => Main.SeerSprite;
        private static Sprite Sheriff => Main.SheriffSprite;
        private static Sprite Poisoned => Main.PoisonedSprite;
        private static Sprite Blackmail => Main.BlackmailSprite;
        private static Sprite Target => Main.TargetSprite;
        private static Sprite Kill;
        private static bool HasCustomButton(CustomRoles role)
        {
            switch (role)
            {
                case CustomRoles.TheGlitch:
                case CustomRoles.Arsonist:
                case CustomRoles.Sheriff:
                case CustomRoles.Vampire:
                case CustomRoles.Poisoner:
                case CustomRoles.Pestilence:
                case CustomRoles.Silencer:
                case CustomRoles.Investigator:
                    return true;
                default:
                    return false;
            }
        }
        public static void Postfix(HudManager __instance)
        {
            if (!Kill) Kill = __instance.KillButton.graphic.sprite;
            if (!Main.ButtonImages.Value)
            {
                __instance.KillButton.transform.Find("Text_TMP").gameObject.SetActive(true);
                __instance.KillButton.graphic.sprite = Kill;
                return;
            }
            if (!HasCustomButton(PlayerControl.LocalPlayer.GetCustomRole()))
            {
                __instance.KillButton.transform.Find("Text_TMP").gameObject.SetActive(true);
                __instance.KillButton.graphic.sprite = Kill;
                return;
            }

            switch (PlayerControl.LocalPlayer.GetCustomRole())
            {
                case CustomRoles.Arsonist:
                    __instance.KillButton.transform.Find("Text_TMP").gameObject.SetActive(false);
                    __instance.KillButton.graphic.sprite = Douse;
                    break;
                case CustomRoles.Sheriff:
                    __instance.KillButton.transform.Find("Text_TMP").gameObject.SetActive(false);
                    __instance.KillButton.graphic.sprite = Sheriff;
                    break;
                case CustomRoles.PlagueBearer:
                    __instance.KillButton.transform.Find("Text_TMP").gameObject.SetActive(false);
                    __instance.KillButton.graphic.sprite = Infect;
                    break;
                case CustomRoles.Ninja:
                    if (Main.CheckShapeshift[PlayerControl.LocalPlayer.PlayerId])
                    {
                        __instance.KillButton.transform.Find("Text_TMP").gameObject.SetActive(false);
                        __instance.KillButton.graphic.sprite = Target;
                    }
                    else
                    {
                        __instance.KillButton.transform.Find("Text_TMP").gameObject.SetActive(true);
                        __instance.KillButton.graphic.sprite = Kill;
                    }
                    break;
                case CustomRoles.Vampire:
                case CustomRoles.Poisoner:
                    __instance.KillButton.transform.Find("Text_TMP").gameObject.SetActive(false);
                    bool poisoned = false;
                    foreach (var pair in Main.BitPlayers)
                    {
                        if (pair.Value.Item1 == PlayerControl.LocalPlayer.PlayerId)
                            poisoned = true;
                    }
                    if (poisoned)
                        __instance.KillButton.graphic.sprite = Poisoned;
                    else
                        __instance.KillButton.graphic.sprite = Poison;
                    break;
                case CustomRoles.Pestilence:
                    __instance.KillButton.transform.Find("Text_TMP").gameObject.SetActive(true);
                    __instance.KillButton.graphic.sprite = Kill;
                    break;
                case CustomRoles.Silencer:
                    if (Main.SilencedPlayer.Count > 0)
                    {
                        __instance.KillButton.transform.Find("Text_TMP").gameObject.SetActive(false);
                        __instance.KillButton.graphic.sprite = Blackmail;
                    }
                    else
                    {
                        __instance.KillButton.transform.Find("Text_TMP").gameObject.SetActive(true);
                        __instance.KillButton.graphic.sprite = Kill;
                    }
                    break;
                case CustomRoles.TheGlitch:
                    if (Main.IsHackMode)
                    {
                        __instance.KillButton.transform.Find("Text_TMP").gameObject.SetActive(false);
                        __instance.KillButton.graphic.sprite = Hack;
                    }
                    else
                    {
                        __instance.KillButton.transform.Find("Text_TMP").gameObject.SetActive(true);
                        __instance.KillButton.graphic.sprite = Kill;
                    }
                    break;
                case CustomRoles.Investigator:
                    __instance.KillButton.transform.Find("Text_TMP").gameObject.SetActive(false);
                    __instance.KillButton.graphic.sprite = Seer;
                    break;
                default:
                    __instance.KillButton.transform.Find("Text_TMP").gameObject.SetActive(true);
                    __instance.KillButton.graphic.sprite = Kill;
                    break;
            }
        }
    }
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class ReportButtonSprite
    {
        private static Sprite Remember => Main.RememberSprite;
        private static Sprite Clean => Main.CleanSprite;
        private static Sprite Report;
        private static bool HasCustomButton(CustomRoles role)
        {
            switch (role)
            {
                case CustomRoles.Amnesiac:
                case CustomRoles.Cleaner:
                    return true;
                default:
                    return false;
            }
        }
        public static void Postfix(HudManager __instance)
        {
            if (!Report) Report = __instance.ReportButton.graphic.sprite;
            if (!Main.ButtonImages.Value)
            {
                __instance.ReportButton.transform.Find("Text_TMP").gameObject.SetActive(true);
                __instance.ReportButton.graphic.sprite = Report;
                return;
            }
            if (!HasCustomButton(PlayerControl.LocalPlayer.GetCustomRole()))
            {
                __instance.ReportButton.transform.Find("Text_TMP").gameObject.SetActive(true);
                __instance.ReportButton.graphic.sprite = Report;
                return;
            }

            switch (PlayerControl.LocalPlayer.GetCustomRole())
            {
                case CustomRoles.Amnesiac:
                    __instance.ReportButton.transform.Find("Text_TMP").gameObject.SetActive(false);
                    __instance.ReportButton.graphic.sprite = Remember;
                    break;
                case CustomRoles.Cleaner:
                    __instance.ReportButton.transform.Find("Text_TMP").gameObject.SetActive(false);
                    __instance.ReportButton.graphic.sprite = Clean;
                    break;
                default:
                    __instance.ReportButton.transform.Find("Text_TMP").gameObject.SetActive(true);
                    __instance.ReportButton.graphic.sprite = Report;
                    break;
            }
        }
    }
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class AbilityButtonSprite
    {
        private static Sprite Alert => Main.AlertSprite;
        private static Sprite Protect => Main.ProtectSprite;
        private static Sprite Vest => Main.VestSprite;
        private static Sprite Mimic => Main.MimicSprite;
        private static Sprite Transport => Main.TransportSprite;
        private static Sprite Flash => Main.FlashSprite;
        private static Sprite Medium => Main.MediumSprite;
        private static Sprite Miner => Main.MinerSprite;
        private static Sprite Assassinate => Main.AssassinateSprite;
        private static Sprite Ability;
        private static bool HasCustomButton(CustomRoles role)
        {
            switch (role)
            {
                case CustomRoles.TheGlitch:
                case CustomRoles.GuardianAngelTOU:
                case CustomRoles.Survivor:
                case CustomRoles.Veteran:
                case CustomRoles.Miner:
                case CustomRoles.Medium:
                case CustomRoles.Grenadier:
                case CustomRoles.Ninja:
                case CustomRoles.Transporter:
                    return true;
                default:
                    return false;
            }
        }
        public static void Postfix(HudManager __instance)
        {
            if (!Ability) Ability = __instance.AbilityButton.graphic.sprite;
            if (!Main.ButtonImages.Value)
            {
                __instance.AbilityButton.transform.Find("Text_TMP").gameObject.SetActive(true);
                __instance.AbilityButton.graphic.sprite = Ability;
                return;
            }
            if (!HasCustomButton(PlayerControl.LocalPlayer.GetCustomRole()))
            {
                __instance.AbilityButton.transform.Find("Text_TMP").gameObject.SetActive(true);
                __instance.AbilityButton.graphic.sprite = Ability;
                return;
            }

            switch (PlayerControl.LocalPlayer.GetCustomRole())
            {
                case CustomRoles.TheGlitch:
                    __instance.AbilityButton.transform.Find("Text_TMP").gameObject.SetActive(false);
                    __instance.AbilityButton.graphic.sprite = Mimic;
                    break;
                case CustomRoles.GuardianAngelTOU:
                    __instance.AbilityButton.transform.Find("Text_TMP").gameObject.SetActive(false);
                    __instance.AbilityButton.graphic.sprite = Protect;
                    break;
                case CustomRoles.Survivor:
                    __instance.AbilityButton.transform.Find("Text_TMP").gameObject.SetActive(false);
                    __instance.AbilityButton.graphic.sprite = Vest;
                    break;
                case CustomRoles.Ninja:
                    if (Ninja.NinjaKillTarget.Count != 0)
                    {
                        __instance.AbilityButton.transform.Find("Text_TMP").gameObject.SetActive(false);
                        __instance.AbilityButton.graphic.sprite = Assassinate;
                    }
                    else
                    {
                        __instance.AbilityButton.transform.Find("Text_TMP").gameObject.SetActive(true);
                        __instance.AbilityButton.graphic.sprite = Ability;
                    }
                    break;
                case CustomRoles.Veteran:
                    __instance.AbilityButton.transform.Find("Text_TMP").gameObject.SetActive(false);
                    __instance.AbilityButton.graphic.sprite = Alert;
                    break;
                case CustomRoles.Miner:
                    __instance.AbilityButton.transform.Find("Text_TMP").gameObject.SetActive(false);
                    __instance.AbilityButton.graphic.sprite = Miner;
                    break;
                case CustomRoles.Medium:
                    __instance.AbilityButton.transform.Find("Text_TMP").gameObject.SetActive(false);
                    __instance.AbilityButton.graphic.sprite = Medium;
                    break;
                case CustomRoles.Grenadier:
                    __instance.AbilityButton.transform.Find("Text_TMP").gameObject.SetActive(false);
                    __instance.AbilityButton.graphic.sprite = Flash;
                    break;
                case CustomRoles.Transporter:
                    __instance.AbilityButton.transform.Find("Text_TMP").gameObject.SetActive(false);
                    __instance.AbilityButton.graphic.sprite = Transport;
                    break;
                default:
                    __instance.AbilityButton.transform.Find("Text_TMP").gameObject.SetActive(true);
                    __instance.AbilityButton.graphic.sprite = Ability;
                    break;
            }
        }
    }
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class VentButtonSprite
    {
        private static Sprite Ignite => Main.IgniteSprite;
        private static Sprite Rampage => Main.RampageSprite;
        private static Sprite Vent;
        private static bool HasCustomButton(CustomRoles role)
        {
            switch (role)
            {
                case CustomRoles.Werewolf:
                case CustomRoles.Arsonist:
                    return true;
                default:
                    return false;
            }
        }
        public static void Postfix(HudManager __instance)
        {
            if (!Vent) Vent = __instance.ImpostorVentButton.graphic.sprite;
            if (!Main.ButtonImages.Value)
            {
                __instance.ImpostorVentButton.transform.Find("Text_TMP").gameObject.SetActive(true);
                __instance.ImpostorVentButton.graphic.sprite = Vent;
                return;
            }
            if (!HasCustomButton(PlayerControl.LocalPlayer.GetCustomRole()))
            {
                __instance.ImpostorVentButton.transform.Find("Text_TMP").gameObject.SetActive(true);
                __instance.ImpostorVentButton.graphic.sprite = Vent;
                return;
            }

            switch (PlayerControl.LocalPlayer.GetCustomRole())
            {
                case CustomRoles.Arsonist:
                    __instance.ImpostorVentButton.transform.Find("Text_TMP").gameObject.SetActive(false);
                    __instance.ImpostorVentButton.graphic.sprite = Ignite;
                    break;
                case CustomRoles.Werewolf:
                    __instance.ImpostorVentButton.transform.Find("Text_TMP").gameObject.SetActive(false);
                    if (!Main.IsRampaged)
                        __instance.ImpostorVentButton.graphic.sprite = Rampage;
                    else
                    {
                        __instance.ImpostorVentButton.transform.Find("Text_TMP").gameObject.SetActive(true);
                        __instance.ImpostorVentButton.graphic.sprite = Vent;
                    }
                    break;
                default:
                    __instance.ImpostorVentButton.transform.Find("Text_TMP").gameObject.SetActive(true);
                    __instance.ImpostorVentButton.graphic.sprite = Vent;
                    break;
            }
        }
    }
}