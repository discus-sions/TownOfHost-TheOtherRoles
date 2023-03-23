using HarmonyLib;
using InnerNet;
using AmongUs.Data;
using UnityEngine;
using AmongUs.GameOptions;
using System.Collections.Generic;
using System;
using System.Linq;
using Hazel;
using static TownOfHost.Translator;
using static AmongUs.GameOptions.GameOptionsFactory;
using Il2CppSystem.IO;
using TownOfHost.PrivateExtensions;

namespace TownOfHost
{
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    public static class GameStartManagerUpdatePatch
    {
        public static void Prefix(GameStartManager __instance)
        {
            __instance.MinPlayers = 1;
        }
    }
    //タイマーとコード隠し
    public class GameStartManagerPatch
    {
        private static float timer = 600f;
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        public class GameStartManagerStartPatch
        {
            public static TMPro.TextMeshPro HideName;
            public static void Postfix(GameStartManager __instance)
            {
                __instance.GameRoomNameCode.text = GameCode.IntToGameName(AmongUsClient.Instance.GameId);
                // Reset lobby countdown timer
                timer = 600f;

                if (AmongUsClient.Instance.AmHost && Options.AutoDisplayLastResult.GetBool() && Main.AllPlayerCustomRoles.Count != 0)
                {
                    new LateTask(() =>
                    {
                        Main.isChatCommand = true;
                        Utils.ShowLastResult();
                    }, 5f, "DisplayLastRoles");
                }
                HideName = UnityEngine.Object.Instantiate(__instance.GameRoomNameCode, __instance.GameRoomNameCode.transform);
                HideName.text = ColorUtility.TryParseHtmlString(Main.HideColor.Value, out _)
                        ? $"<color={Main.HideColor.Value}>{Main.HideName.Value}</color>"
                        : $"<color={Main.modColor}>{Main.HideName.Value}</color>";

                // Make Public Button
                bool NameIncludeMod = DataManager.Player.Customization.Name.ToLower().Contains("mod");
                bool NameIncludeTOR = DataManager.Player.Customization.Name.ToUpper().Contains("TOR");
                if (ModUpdater.isBroken || ModUpdater.hasUpdate || (NameIncludeMod && !NameIncludeTOR))
                {
                    __instance.MakePublicButton.color = Palette.DisabledClear;
                    __instance.privatePublicText.color = Palette.DisabledClear;
                }
            }
        }

        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public class GameStartManagerUpdatePatch
        {
            private static bool update = false;
            private static string currentText = "";
            public static void Prefix(GameStartManager __instance)
            {
                // Lobby code
                if (DataManager.Settings.Gameplay.StreamerMode)
                {
                    __instance.GameRoomNameCode.color = new(255, 255, 255, 0);
                    GameStartManagerStartPatch.HideName.enabled = true;
                }
                else
                {
                    __instance.GameRoomNameCode.color = new(255, 255, 255, 255);
                    GameStartManagerStartPatch.HideName.enabled = false;
                }
                if (!AmongUsClient.Instance.AmHost || !GameData.Instance || AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame) return; // Not host or no instance or LocalGame
                update = GameData.Instance.PlayerCount != __instance.LastPlayerCount;
            }
            public static void Postfix(GameStartManager __instance)
            {
                // Lobby timer
                if (!AmongUsClient.Instance.AmHost || !GameData.Instance || AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame) return;

                if (update) currentText = __instance.PlayerCounter.text;

                timer = Mathf.Max(0f, timer -= Time.deltaTime);
                int minutes = (int)timer / 60;
                int seconds = (int)timer % 60;
                string suffix = $" ({minutes:00}:{seconds:00})";
                if (timer <= 60) suffix = Helpers.ColorString(Color.red, suffix);
                if (minutes == 0 && seconds < 15)
                    SoundManager.Instance.PlaySound(ShipStatus.Instance.SabotageSound, false, 0.8f);

                __instance.PlayerCounter.text = currentText + suffix;
                __instance.PlayerCounter.autoSizeTextContainer = true;
            }
        }
        [HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.SetText))]
        public static class HiddenTextPatch
        {
            private static void Postfix(TextBoxTMP __instance)
            {
                if (__instance.name == "GameIdText") __instance.outputText.text = new string('*', __instance.text.Length);
            }
        }
    }
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
    public class GameStartRandomMap
    {
        public static void Prefix()
        {
            RPC.SyncCustomSettingsRPC();
            if (GameOptionsManager.Instance.currentGameMode == GameModes.Normal)
            {
                Options.DefaultKillCooldown = GameOptionsManager.Instance.CurrentGameOptions.AsNormalOptions()!.KillCooldown;
                Main.LastKillCooldown.Value = GameOptionsManager.Instance.CurrentGameOptions.AsNormalOptions()!.KillCooldown;
                GameOptionsManager.Instance.CurrentGameOptions.AsNormalOptions()!.KillCooldown = 0.1f;
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc == null || pc.Data.Disconnected) continue;
                    if (pc.CurrentOutfit.ColorId > 17)
                        pc.RpcSetColor(17);
                }
                Main.RealOptionsData = GameOptionsManager.Instance.CurrentGameOptions.DeepCopy();
                PlayerControl.LocalPlayer.RpcSyncSettings(Main.RealOptionsData.ToBytes());
            }
            else
            {
                Options.DefaultKillCooldown = GameOptionsManager.Instance.CurrentGameOptions.AsHnsOptions()!.KillCooldown;
                Main.LastKillCooldown.Value = GameOptionsManager.Instance.CurrentGameOptions.AsHnsOptions()!.KillCooldown;
                // GameOptionsManager.Instance.CurrentGameOptions.AsHnsOptions()!.KillCooldown = 0.1f;
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc == null || pc.Data.Disconnected) continue;
                    if (pc.CurrentOutfit.ColorId > 17)
                        pc.RpcSetColor(17);
                }
                Main.RealOptionsData = GameOptionsManager.Instance.CurrentGameOptions.DeepCopy();
                PlayerControl.LocalPlayer.RpcSyncSettings(Main.RealOptionsData.ToBytes());
            }
        }
        public static bool Prefix(GameStartRandomMap __instance)
        {
            bool continueStart = true;
            if (Options.RandomMapsMode.GetBool())
            {
                var rand = new System.Random();
                System.Collections.Generic.List<byte> RandomMaps = new();
                /*TheSkeld   = 0
                MIRAHQ     = 1
                Polus      = 2
                Dleks      = 3
                TheAirShip = 4*/
                if (Options.AddedTheSkeld.GetBool()) RandomMaps.Add(0);
                if (Options.AddedMiraHQ.GetBool()) RandomMaps.Add(1);
                if (Options.AddedPolus.GetBool()) RandomMaps.Add(2);
                // if (Options.AddedDleks.GetBool()) RandomMaps.Add(3);
                if (Options.AddedTheAirShip.GetBool()) RandomMaps.Add(4);

                if (RandomMaps.Count <= 0) return true;
                var MapsId = RandomMaps[rand.Next(RandomMaps.Count)];
                GameOptionsManager.Instance.CurrentGameOptions.AsNormalOptions()!.MapId = MapsId;
            }
            return continueStart;
        }
    }
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.ResetStartState))]
    class ResetStartStatePatch
    {
        public static void Prefix()
        {
            if (GameStates.IsCountDown)
            {
                GameOptionsManager.Instance.CurrentGameOptions.AsNormalOptions()!.KillCooldown = Options.DefaultKillCooldown;
                PlayerControl.LocalPlayer.RpcSyncSettings(GameOptionsManager.Instance.CurrentGameOptions.ToBytes());
            }
        }
    }
    [HarmonyPatch(typeof(LogicOptions), nameof(LogicOptions.GetAdjustedNumImpostors))]
    class UnrestrictedNumImpostorsPatch
    {
        public static bool Prefix(ref int __result)
        {
            __result = GameOptionsManager.Instance.CurrentGameOptions.NumImpostors;
            return false;
        }
    }
}