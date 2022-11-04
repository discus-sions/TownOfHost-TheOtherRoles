using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Hazel;
using Assets.CoreScripts;
using HarmonyLib;
using UnityEngine;
using static TownOfHost.Translator;


namespace TownOfHost
{
    public static class PercentageChecker
    {
        public static int CheckPercentage(string str = "", byte playerId = 0xff, bool noErr = false, CustomRoles role = CustomRoles.Amnesiac)
        {
            bool checkLaptop = true;
            if (!checkLaptop)
            {
                if (!File.Exists("percentage.txt"))
                {
                    HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, "Could not find percentage.txt in the same folder as Among Us.exe. \nPlease redownload the mod.");
                    File.WriteAllText(@"percentage.txt", "Download the correct version at: https://github.com/music-discussion/TownOfHost-TheOtherRoles");
                    return 0;
                }
                using StreamReader sr = new(@"percentage.txt", Encoding.GetEncoding("UTF-8"));
                string text;
                string[] tmp = { };
                List<string> sendList = new();
                HashSet<string> tags = new();
                while ((text = sr.ReadLine()) != null)
                {
                    tmp = text.Split(":");
                    if (tmp.Length > 1 && tmp[1] != "")
                    {
                        tags.Add(tmp[0]);
                        if (tmp[0] == str) sendList.Add(tmp.Skip(1).Join(delimiter: "").Replace("\\n", "\n"));
                    }
                }
                if (sendList.Count == 0 && !noErr)
                {
                    if (playerId == 0xff)
                        HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, "The file was not found. Download the file for percentages to work correctly. You tried to search for: " + str);
                    else Utils.SendMessage("The file was not found. Please alert the host. You tried to search for: " + str, playerId);
                    return 0;
                }
                else for (int i = 0; i < sendList.Count; i++) return System.Convert.ToInt32(sendList[i]);
                return 0;
            }
            else
            {
                return Convert.ToInt32(role.GetChance() * 100);
            }
        }
    }
}