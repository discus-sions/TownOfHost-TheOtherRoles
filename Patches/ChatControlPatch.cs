using HarmonyLib;
using UnityEngine;
using AmongUs.Data;

namespace TownOfHost
{
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
    class ChatControllerUpdatePatch
    {
        public static int CurrentHistorySelection = -1;
        public static void Prefix()
        {
            if (AmongUsClient.Instance.AmHost && DataManager.Settings.multiplayer.ChatMode == InnerNet.QuickChatModes.QuickChatOnly)
                DataManager.Settings.multiplayer.ChatMode = InnerNet.QuickChatModes.FreeChatOrQuickChat;
        }
        public static void Postfix(ChatController __instance)
        {
            if (!__instance.TextArea.hasFocus) return;
            if (Input.GetKey(KeyCode.LeftControl) | Input.GetKey(KeyCode.RightControl) && Input.GetKeyDown(KeyCode.C))
                ClipboardHelper.PutClipboardString(__instance.TextArea.text);
            if (Input.GetKey(KeyCode.LeftControl) | Input.GetKey(KeyCode.RightControl) && Input.GetKeyDown(KeyCode.V))
                __instance.TextArea.SetText(__instance.TextArea.text + GUIUtility.systemCopyBuffer);
            if (Input.GetKey(KeyCode.LeftControl) | Input.GetKey(KeyCode.RightControl) && Input.GetKeyDown(KeyCode.X))
            {
                ClipboardHelper.PutClipboardString(__instance.TextArea.text);
                __instance.TextArea.SetText("");
            }
            if (Input.GetKeyDown(KeyCode.UpArrow) && ChatCommands.ChatHistory.Count > 0)
            {
                CurrentHistorySelection = Mathf.Clamp(--CurrentHistorySelection, 0, ChatCommands.ChatHistory.Count - 1);
                __instance.TextArea.SetText(ChatCommands.ChatHistory[CurrentHistorySelection]);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) && ChatCommands.ChatHistory.Count > 0)
            {
                CurrentHistorySelection++;
                if (CurrentHistorySelection < ChatCommands.ChatHistory.Count)
                    __instance.TextArea.SetText(ChatCommands.ChatHistory[CurrentHistorySelection]);
                else __instance.TextArea.SetText("");
            }
        }
    }
}