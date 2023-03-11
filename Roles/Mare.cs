using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using TownOfHost.PrivateExtensions;

namespace TownOfHost
{
    public static class Mare
    {
        private static readonly int Id = 2300;
        public static List<byte> playerIdList = new();

        private static CustomOption KillCooldownInLightsOut;
        private static CustomOption SpeedInLightsOut;
        public static CustomOption RedNameCooldownAfterLights;
        public static CustomOption RedNameCooldownAfterMeeting;
        public static CustomOption MareCanKillLightsOn;
        public static CustomOption AddedKillCooldown;

        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id, CustomRoles.Mare, AmongUsExtensions.OptionType.Impostor);
            SpeedInLightsOut = CustomOption.Create(Id + 10, Color.white, "MareSpeedInLightsOut", AmongUsExtensions.OptionType.Impostor, 2f, 0.25f, 3f, 0.25f, Options.CustomRoleSpawnChances[CustomRoles.Mare]);
            KillCooldownInLightsOut = CustomOption.Create(Id + 11, Color.white, "MareKillCooldownInLightsOut", AmongUsExtensions.OptionType.Impostor, 15f, 2.5f, 120f, 2.5f, Options.CustomRoleSpawnChances[CustomRoles.Mare]);
            RedNameCooldownAfterLights = CustomOption.Create(Id + 12, Color.white, "RedNameCooldownAfterLights", AmongUsExtensions.OptionType.Impostor, 5f, 0, 30f, 2.5f, Options.CustomRoleSpawnChances[CustomRoles.Mare]);
            RedNameCooldownAfterMeeting = CustomOption.Create(Id + 13, Color.white, "RedNameCooldownAfterMeeting", AmongUsExtensions.OptionType.Impostor, 15f, 0, 60f, 2.5f, Options.CustomRoleSpawnChances[CustomRoles.Mare]);
            MareCanKillLightsOn = CustomOption.Create(Id + 14, Color.white, "MareCanKillLightsOn", AmongUsExtensions.OptionType.Impostor, false, Options.CustomRoleSpawnChances[CustomRoles.Mare]);
            AddedKillCooldown = CustomOption.Create(Id + 15, Color.white, "AddedKillCooldown", AmongUsExtensions.OptionType.Impostor, 10f, 5f, 45f, 2.5f, MareCanKillLightsOn);
        }
        public static void Init()
        {
            playerIdList = new();
        }
        public static void Add(byte mare)
        {
            playerIdList.Add(mare);
        }
        public static bool IsEnable => playerIdList.Count > 0;
        public static float GetKillCooldown => Utils.IsActive(SystemTypes.Electrical) ? KillCooldownInLightsOut.GetFloat() : MareCanKillLightsOn.GetBool() ? Options.DefaultKillCooldown + AddedKillCooldown.GetFloat() : Options.DefaultKillCooldown;
        public static void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = GetKillCooldown;
        public static void ApplyGameOptions(NormalGameOptionsV07 opt, byte playerId)
        {
            Main.AllPlayerSpeed[playerId] = Main.RealOptionsData.AsNormalOptions()!.PlayerSpeedMod;
            if (Utils.IsActive(SystemTypes.Electrical))//もし停電発生した場合
                Main.AllPlayerSpeed[playerId] = SpeedInLightsOut.GetFloat();//Mareの速度を設定した値にする
        }

        public static void OnCheckMurder(PlayerControl killer)
        {
        }
        public static void FixedUpdate(PlayerControl player)
        {
        }
    }
}