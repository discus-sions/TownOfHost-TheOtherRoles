using System;
using System.Linq;
using HarmonyLib;
using Il2CppInterop;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Collections.Generic;

namespace TownOfHost
{
    public static class Patch
    {
        public static float LobbyTextRowHeight { get; set; } = 0.081F;

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        private class LobbyManagerUpdate
        {
            private const float
                MinX = -5.233334F /*-5.3F*/,
                OriginalY = 2.9F,
                MinY = 3F; // Differs to cause excess options to appear cut off to encourage scrolling

            private static Scroller Scroller;
            private static Vector3 LastPosition = new Vector3(MinX, MinY);

            public static void Prefix(HudManager __instance)
            {
                /* if (GameStates.InGame)
                 {
                     Scroller = null;
                     return;
                 }
                 if (__instance.GameSettings?.transform == null || !GameStates.IsLobby)
                 {
                     return;
                 }

                 CreateScroller(__instance);

                 Scroller.gameObject.SetActive(__instance.GameSettings.gameObject.activeSelf);

                 if (!Scroller.gameObject.active) return;

                 var rows = OptionShower.GetText().Count(c => c == '\n');
                 var maxY = Mathf.Max(MinY, rows * LobbyTextRowHeight + rows * LobbyTextRowHeight);

                 Scroller.ContentYBounds = new FloatRange(MinY, maxY);

                 // Prevent scrolling when the player is interacting with a menu
                 if (PlayerControl.LocalPlayer?.CanMove != true && GameStates.IsLobby)
                 {
                     __instance.GameSettings.transform.localPosition = LastPosition;

                     return;
                 }

                 if (__instance.GameSettings.transform.localPosition.x != MinX ||
                     __instance.GameSettings.transform.localPosition.y < MinY) return;

                 LastPosition = __instance.GameSettings.transform.localPosition;*/
            }

            private static void CreateScroller(HudManager __instance)
            {
                if (Scroller != null) return;

                Scroller = new GameObject("SettingsScroller").AddComponent<Scroller>();
                Scroller.transform.SetParent(__instance.GameSettings.transform.parent);
                Scroller.gameObject.layer = 5;

                Scroller.transform.localScale = Vector3.one;
                Scroller.allowX = false;
                Scroller.allowY = true;
                Scroller.active = true;
                Scroller.velocity = new Vector2(0, 0);
                Scroller.ScrollbarYBounds = new FloatRange(0, 0);
                Scroller.ContentXBounds = new FloatRange(MinX, MinX);
                Scroller.enabled = true;

                Scroller.Inner = __instance.GameSettings.transform;
                __instance.GameSettings.transform.SetParent(Scroller.transform);
            }
        }
    }
}
