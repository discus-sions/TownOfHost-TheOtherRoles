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
    public static class CustomTags
    {
        private static readonly string TEMPLATE_FILE_PATH = "./TOR_DATA/";
        public static List<string> ReturnTagInfo(string friendCode)
        {
            List<string> returned = new();
            if (!File.Exists(GetFilePath(friendCode)))
            {
                returned.Add("None");
                return returned;
            }
            else
            {
                string fileCode = GetTemplateFromFile(friendCode, "code");
                if (fileCode != friendCode)
                {
                    returned.Add("None");
                    return returned;
                }
                string type = GetTemplateFromFile(friendCode, "type");
                returned.Add(type);
                switch (type)
                {
                    case "sforce":
                        returned.Add(GetTemplateFromFile(friendCode, "color"));
                        if (IsBadString(GetTemplateFromFile(friendCode, "toptext")))
                        {
                            returned = new();
                            returned.Add("None");
                            HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"Top Text contains \"Dev\". So it has been considered invalid.");
                            return returned;
                        }
                        returned.Add(GetTemplateFromFile(friendCode, "toptext"));
                        if (IsBadString(GetTemplateFromFile(friendCode, "name")))
                        {
                            returned = new();
                            returned.Add("None");
                            HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"Name contains \"Dev\". So it has been considered invalid.");
                            return returned;
                        }
                        returned.Add(GetTemplateFromFile(friendCode, "name"));
                        break;
                    case "static":
                        returned.Add(GetTemplateFromFile(friendCode, "color"));
                        var text = GetTemplateFromFile(friendCode, "text");
                        if (IsBadString(text))
                        {
                            returned = new();
                            returned.Add("None");
                            HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"Top Text contains \"Dev\". So it has been considered invalid.");
                            return returned;
                        }
                        returned.Add(text);
                        break;
                    default:
                    case "gradient":
                        returned = new();
                        returned.Add("None");
                        HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"Invalid type given.");
                        return returned;
                }
                Logger.Info($"{returned}", "Custom Tag Info");
                return returned;
            }
        }
        public static List<string> ReturnTagInfoFromString(string value)
        {
            List<string> returned = new();
            string[] values = value.Split("\n");

            foreach (var pair in values)
            {
                string[] entires = pair.Split(":");
                if (entires[0] != "code")
                    returned.Add(entires[1]);
            }

            Logger.Info($"{returned}", "Custom Tag Info");
            return returned;
        }
        public static bool IsBadString(string msg)
        {
            string text = msg.ToLower();
            if (text.Contains("dev")) return true;
            return false;
        }
        public static string GetTemplateFromFile(string fileName, string str = "", bool noErr = false)
        {
            if (!File.Exists(GetFilePath(fileName)))
            {
                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"{GetFilePath(fileName)} does not exist. Their tag has not been given.");
                return "None";
            }
            using StreamReader sr = new(GetFilePath(fileName), Encoding.GetEncoding("UTF-8"));
            string text;
            string[] tmp = { };
            List<string> sendList = new();
            string sendBack = "";
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
            if (sendList.Count != 0)
                sendBack = sendList[0];
            if (sendList.Count == 0 && !noErr)
            {
                HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, string.Format(GetString("Message.TemplateNotFoundHost"), str, tags.Join(delimiter: ", ")));
                return "None";
            }
            else for (int i = 0; i < sendList.Count; i++) return sendList[i];
            return "None";
        }
        public static string GetFilePath(string txtname) => TEMPLATE_FILE_PATH + txtname + ".txt";
    }
}