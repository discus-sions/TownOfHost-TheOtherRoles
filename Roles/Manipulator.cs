using System.Collections.Generic;
using Hazel;
using UnityEngine;
using static TownOfHost.Translator;

namespace TownOfHost
{
    public static class Manipulator
    {
        private static readonly int Id = 32434932;
        public static List<byte> playerIdList = new();
        public static List<byte> killedList = new();

        public static bool MeetingIsSabotaged = false;

        public static CustomOption AddedKillCooldown;
        public static CustomOption VotingTimeOnSabotage;
        public static CustomOption DiscussionTimeOnSabotage;

        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id, CustomRoles.Manipulator, AmongUsExtensions.OptionType.Impostor);
            AddedKillCooldown = CustomOption.Create(Id + 10, Color.white, "AddedKillCooldown", AmongUsExtensions.OptionType.Impostor, 10f, 5f, 45f, 2.5f, Options.CustomRoleSpawnChances[CustomRoles.Manipulator]);
            DiscussionTimeOnSabotage = CustomOption.Create(Id + 11, Color.white, "DiscussionTimeOnSabotage", AmongUsExtensions.OptionType.Impostor, 10, 10, 30, 1, Options.CustomRoleSpawnChances[CustomRoles.Manipulator]);
            VotingTimeOnSabotage = CustomOption.Create(Id + 12, Color.white, "VotingTimeOnSabotage", AmongUsExtensions.OptionType.Impostor, 30, 20, 60, 1, Options.CustomRoleSpawnChances[CustomRoles.Manipulator]);
        }

        public static bool IsEnable() => playerIdList.Count != 0;

        public static void Reset()
        {
            playerIdList = new();
            killedList = new();
            MeetingIsSabotaged = false;
        }

        public static void Add(PlayerControl manip)
        {
            playerIdList.Add(manip.PlayerId);
        }

        public static void SabotagedMeetingReport()
        {
            MeetingIsSabotaged = true;
        }

        public static void ResetSabotagedMeeting()
        {
            if (MeetingIsSabotaged && IsEnable())
                MeetingIsSabotaged = false;
        }
    }
}