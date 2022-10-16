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
        // Utils.SendMessage("Hide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHide\nHid Guess Message");


        public static TMPro.TextMeshPro nameText(this PlayerControl p) => p.cosmetics.nameText;

        public static TMPro.TextMeshPro NameText(this PoolablePlayer p) => p.cosmetics.nameText;

        public static UnityEngine.SpriteRenderer myRend(this PlayerControl p) => p.cosmetics.currentBodySprite.BodySprite;
    }
}