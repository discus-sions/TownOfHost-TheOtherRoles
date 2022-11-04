using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace TownOfHost
{
    public static class AmongUsExtensions
    {
        public static bool IsNullOrDestroyed(this System.Object obj)
        {

            if (object.ReferenceEquals(obj, null)) return true;

            if (obj is UnityEngine.Object) return (obj as UnityEngine.Object) == null;

            return false;
        }
        public static void AddRoleText(this PlayerVoteArea pva, bool showProgressText)
        {
            if (!Options.RolesLikeToU.GetBool())
            {
                var pc = Utils.GetPlayerById(pva.TargetPlayerId);
                if (pc == null || pc.Data.Disconnected) return;
                var RoleTextData = Utils.GetRoleText(pc);
                var roleTextMeeting = UnityEngine.Object.Instantiate(pva.NameText);
                roleTextMeeting.transform.SetParent(pva.NameText.transform);
                roleTextMeeting.transform.localPosition = new Vector3(0f, -0.18f, 0f);
                roleTextMeeting.fontSize = 1.5f;
                roleTextMeeting.text = RoleTextData.Item1;
                if (Main.VisibleTasksCount && !Main.rolesRevealedNextMeeting.Contains(pva.TargetPlayerId) && showProgressText) roleTextMeeting.text += Utils.GetProgressText(pc);
                roleTextMeeting.color = RoleTextData.Item2;
                roleTextMeeting.gameObject.name = "RoleTextMeeting";
                roleTextMeeting.enableWordWrapping = false;
                roleTextMeeting.enabled =
                    pva.TargetPlayerId == PlayerControl.LocalPlayer.PlayerId || Main.rolesRevealedNextMeeting.Contains(pva.TargetPlayerId) || (PlayerControl.LocalPlayer.GetCustomRole().IsImpostor() && Options.ImpostorKnowsRolesOfTeam.GetBool() && pc.GetCustomRole().IsImpostor()) ||
                    (Main.VisibleTasksCount && PlayerControl.LocalPlayer.Data.IsDead && Options.GhostCanSeeOtherRoles.GetBool()) || (PlayerControl.LocalPlayer.GetCustomRole().IsCoven() && Options.CovenKnowsRolesOfTeam.GetBool() && pc.GetCustomRole().IsCoven());
            }
            else
            {
                var pc = Utils.GetPlayerById(pva.TargetPlayerId);
                if (pc == null || pc.Data.Disconnected) return;
                bool continues = pva.TargetPlayerId == PlayerControl.LocalPlayer.PlayerId || Main.rolesRevealedNextMeeting.Contains(pva.TargetPlayerId) || (PlayerControl.LocalPlayer.GetCustomRole().IsImpostor() && Options.ImpostorKnowsRolesOfTeam.GetBool() && pc.GetCustomRole().IsImpostor()) ||
                    (Main.VisibleTasksCount && PlayerControl.LocalPlayer.Data.IsDead && Options.GhostCanSeeOtherRoles.GetBool()) || (PlayerControl.LocalPlayer.GetCustomRole().IsCoven() && Options.CovenKnowsRolesOfTeam.GetBool() && pc.GetCustomRole().IsCoven());
                if (!continues) return;
                string name = pva.NameText.text + " ";
                if (Main.VisibleTasksCount && showProgressText) name += Utils.GetProgressText(pc);
                name += "\r\n";
                name += Utils.GetRoleName(pc.GetCustomRole());
                pva.NameText.text = name;
                pva.NameText.color = Utils.GetRoleColor(pc.GetCustomRole());
            }
        }
        public static void ChangeTextColor(this ActionButton button, Color color, bool changeButtonColor = false)
        {
            if (changeButtonColor)
                button.OverrideColor(color);
            button.buttonLabelText.color = color;
        }
        // Utils.SendMessage("Hide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHid Guess Message");


        public static TMPro.TextMeshPro nameText(this PlayerControl p) => p.cosmetics.nameText;

        public static TMPro.TextMeshPro NameText(this PoolablePlayer p) => p.cosmetics.nameText;

        public static UnityEngine.SpriteRenderer myRend(this PlayerControl p) => p.cosmetics.currentBodySprite.BodySprite;

        public enum OptionType
        {
            GameOption,
            Crewmate,
            Neutral,
            Impostor,
            Modifier,
            None,
            Roles
        }
    }
}