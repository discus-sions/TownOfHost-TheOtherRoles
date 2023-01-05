using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;

namespace TownOfHost
{
    public class AnnouncementPatch
    {
        [HarmonyPatch(typeof(AnnouncementPopUp), nameof(AnnouncementPopUp.UpdateAnnounceText))]
        class SetAnnouncement
        {
            public static void Postfix()
            {
                // useless state
            }
        }
    }
}