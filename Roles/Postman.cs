using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hazel;
using UnityEngine;
using static TownOfHost.Translator;
using HarmonyLib;
using TownOfHost.RoleHelpers;

namespace TownOfHost
{
    public class Postman
    {
        private static readonly int Id = 190000;
        public static List<byte> playerIdList = new();
        public static Dictionary<byte, bool> hasDelivered = new();
        public static Distance DistanceType = Distance.None;
        public static bool DoneDelivering = false;
        public static bool PostmanWins = false;
        public static bool IsDelivering = false;
        public static bool AlreadyKilled = false;
        public static PlayerControl? target = null;

        public static CustomOption? ArrowPointingToRecievers;
        public static CustomOption? DeliverDistance;
        public static Options.OverrideTasksData? PostmanTasks;

        public static void SetupCustomOption()
        {
            Options.SetupSingleRoleOptions(Id, CustomRoles.Postman, 1, AmongUsExtensions.OptionType.Neutral);
            ArrowPointingToRecievers = CustomOption.Create(Id + 10, Color.white, "ArrowPointingToRecievers", AmongUsExtensions.OptionType.Neutral, true, Options.CustomRoleSpawnChances[CustomRoles.Postman]);
            DeliverDistance = CustomOption.Create(Id + 11, Color.white, "DeliverDistance", AmongUsExtensions.OptionType.Neutral, DistanceHelper.distanceModes[1..4], DistanceHelper.distanceModes[2], Options.CustomRoleSpawnChances[CustomRoles.Postman]);
            PostmanTasks = Options.OverrideTasksData.Create(Id + 12, CustomRoles.Postman, AmongUsExtensions.OptionType.Neutral);
        }

        public static bool IsEnable() => playerIdList.Count != 0;

        public static void Reset()
        {
            playerIdList = new();
            hasDelivered = new();
            target = null;
            DoneDelivering = false;
            AlreadyKilled = false;
            IsDelivering = false;
            PostmanWins = false;
            if (CustomRoles.Postman.IsEnable())
                DistanceType = DistanceHelper.GetDistanceFromOption(DeliverDistance);
        }

        public static PlayerControl GetPostman() => Utils.GetPlayerById(playerIdList.FirstOrDefault());

        // OTHER STUFF //

        public static void Add(PlayerControl postman)
        {
            Main.lastAmountOfTasks.Add(postman.PlayerId, 0);
            playerIdList.Add(postman.PlayerId);
        }

        public static void Remove(byte player)
        {
            if (hasDelivered.ContainsKey(player))
            {
                hasDelivered.Remove(player);
            }
        }

        public static void CheckForTarget(PlayerControl killed)
        {
            if (target == null) return;
            if (target.PlayerId == killed.PlayerId)
            {
                target = null;
                hasDelivered.Where(p => Utils.GetPlayerById(p.Key).Data.IsDead | playerIdList.Contains(p.Key)).ToArray().Do(RemoveDeadPerson);
                SendRPC("RemoveTarget");
            }
        }

        public static void OnMeeting()
        {
            if (DoneDelivering && IsEnable())
            {
                Utils.SendMessage("The Postman has finished delivering letters! They will bomb everyone if they aren't ejected this meeting!");
                Logger.Info("PostMan delivered Messages to Everyone. They now just have to survive to win.", "Postman");
            }
        }

        private static int GetAllOfValue(Dictionary<byte, bool> dictionary, bool value)
        {
            int amount = 0;
            foreach (var pair in dictionary)
            {
                if (pair.Value == value)
                    amount++;
            }
            return amount;
        }

        private static List<byte> GetAllKeysOfValue(Dictionary<byte, bool> dictionary, bool value)
        {
            List<byte> keys = new();
            foreach (var pair in dictionary)
            {
                if (pair.Value == value)
                    keys.Add(pair.Key);
            }
            return keys;
        }

        public static void OnTaskComplete(byte playerId, TaskState taskState)
        {
            if (AlreadyKilled) return;
            if (IsDelivering && taskState.CompletedTasksCount > Main.lastAmountOfTasks[playerId])
            {
                _ = new LateTask(() =>
                {
                    var player = Utils.GetPlayerById(playerId);
                    if (player != null && !AlreadyKilled)
                    {
                        player.RpcMurderPlayer(player);
                        PlayerState.SetDeathReason(playerId, PlayerState.DeathReason.Suicide);
                        AlreadyKilled = true;
                    }
                }, 1f, "Postman Kill");
            }
            if (DoneDelivering | PostmanWins | Utils.GetPlayerById(playerId).Data.IsDead | taskState.CompletedTasksCount <= 0 | taskState.CompletedTasksCount == Main.lastAmountOfTasks[playerId] || IsDelivering) return;
            Logger.Info("PostMan completed a Task! Delevering Message. . .", "Postman");
            Main.lastAmountOfTasks[playerId]++;
            hasDelivered.Where(p => Utils.GetPlayerById(p.Key).Data.IsDead | playerIdList.Contains(p.Key)).ToArray().Do(RemoveDeadPerson);
            List<byte> deliverTo = GetAllKeysOfValue(hasDelivered, false);

            if (deliverTo.Count != 0 && taskState.RemainingTasksCount != 0)
            {
                target = Utils.GetPlayerById(deliverTo.GetRandom());
                IsDelivering = true;
                Utils.NotifyRoles();
                SendRPC("SetTarget", target.PlayerId);
                Logger.Info($"PostMan has to deliver to {target.GetNameWithRole()}", "Postman");
            }
            else
            {
                if (!DoneDelivering)
                    Logger.Info($"PostMan has finished delivering messages!", "Postman");
                DoneDelivering = true;
            }
        }

        public static void DeliverMessage(PlayerControl postman, PlayerControl reciever)
        {
            if (!IsDelivering | DoneDelivering | target == null) return;
            Logger.Info($"PostMan delivered a Message to {reciever.GetNameWithRole()} : {target.GetNameWithRole()}", "Postman");
            IsDelivering = false;
            hasDelivered[reciever.PlayerId] = true;
            hasDelivered.Where(p => Utils.GetPlayerById(p.Key).Data.IsDead | playerIdList.Contains(p.Key)).ToArray().Do(RemoveDeadPerson);
            postman.RpcGuardAndKill(reciever);
            target = null;
            Utils.NotifyRoles();
            SendRPC("RemoveTarget");
        }

        public static void DisplayTarget(PlayerControl postman, TMPro.TextMeshPro lowerInfoText)
        {
            lowerInfoText.text = target == null ? "No Target Currently." : $"{GetString("BountyCurrentTarget")}:{target.name}";
            lowerInfoText.enabled = target != null || Main.CachedDevMode;
        }

        public static string GetProgressText(byte playerId)
        {
            string response = "";
            int deliveredTo = GetAllOfValue(hasDelivered, true);
            response = Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Postman), $"({deliveredTo}/{hasDelivered.Count})");
            return response;
        }

        private static void RemoveDeadPerson(KeyValuePair<byte, bool> pair) => hasDelivered.Remove(pair.Key);

        private static void SendRPC(string type, byte playerId = 255)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SendPostmanInfo, Hazel.SendOption.Reliable, -1);
            writer.Write(type);
            switch (type)
            {
                case "SetTarget":
                    writer.Write(playerId);
                    break;
            }
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }

        public static void RecieveRPC(MessageReader reader)
        {
            switch (reader.ReadString())
            {
                case "SetTarget":
                    target = Utils.GetPlayerById(reader.ReadByte());
                    break;
                case "RemoveTarget":
                    target = null;
                    break;
            }
        }
    }
}