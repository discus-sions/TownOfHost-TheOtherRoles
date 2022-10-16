using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnhollowerBaseLib;
using UnityEngine;
using static TownOfHost.Translator;

namespace TownOfHost
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class HudManagerPatch
    {
        public static bool ShowDebugText = false;
        public static int LastCallNotifyRolesPerSecond = 0;
        public static int NowCallNotifyRolesCount = 0;
        public static int LastSetNameDesyncCount = 0;
        public static int LastFPS = 0;
        public static int NowFrameCount = 0;
        public static float FrameRateTimer = 0.0f;
        public static TMPro.TextMeshPro LowerInfoText;
        public static void Postfix(HudManager __instance)
        {
            var player = PlayerControl.LocalPlayer;
            if (player == null) return;
            var TaskTextPrefix = "";
            var FakeTasksText = Helpers.ColorString(player.GetRoleColor(), DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.FakeTasks, new Il2CppReferenceArray<Il2CppSystem.Object>(0)));
            //壁抜け
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                if ((AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started || GameStates.IsFreePlay)
                    && player.MyAnim.ClipName is "Idle" or "Walk")
                {
                    player.Collider.offset = new Vector2(0f, 127f);
                }
            }
            //壁抜け解除
            if (player.Collider.offset.y == 127f)
            {
                if (!Input.GetKey(KeyCode.LeftControl) || AmongUsClient.Instance.IsGameStarted)
                {
                    player.Collider.offset = new Vector2(0f, -0.3636f);
                }
            }
            //MOD入り用のボタン下テキスト変更
            switch (player.GetCustomRole())
            {
                case CustomRoles.TheGlitch:
                    __instance.AbilityButton.OverrideText("MIMIC");
                    if (Main.IsHackMode)
                        __instance.KillButton.OverrideText("HACK");
                    else
                        __instance.KillButton.OverrideText($"{GetString("KillButtonText")}");
                    break;
                case CustomRoles.CovenWitch:
                    if (!Main.HasNecronomicon)
                        __instance.KillButton.OverrideText($"{GetString("PuppeteerOperateButtonText")}");
                    else
                        __instance.KillButton.OverrideText($"{GetString("KillButtonText")}");
                    break;
                case CustomRoles.Investigator:
                    __instance.KillButton.OverrideText("INVESTIGATE");
                    break;
                case CustomRoles.Ninja:
                    if (Main.CheckShapeshift[player.PlayerId])
                    {
                        if (Ninja.NinjaKillTarget.Count != 0)
                            __instance.AbilityButton.OverrideText("ASSASSINATE");
                        else
                            __instance.AbilityButton.OverrideText("END HUNT");
                        __instance.KillButton.OverrideText("TARGET");
                    }
                    else
                    {
                        __instance.KillButton.OverrideText($"{GetString("KillButtonText")}");
                        __instance.AbilityButton.OverrideText("HUNT");
                    }
                    break;
                case CustomRoles.Freezer:
                    if (Main.CheckShapeshift[player.PlayerId])
                        __instance.AbilityButton.OverrideText("UNFREEZE");
                    else
                        __instance.AbilityButton.OverrideText("FREEZE");
                    break;
                case CustomRoles.HexMaster:
                    if (player.IsHexMode())
                        __instance.KillButton.OverrideText("HEX");
                    else
                        __instance.KillButton.OverrideText($"{GetString("KillButtonText")}");
                    break;
                case CustomRoles.Escort:
                    __instance.KillButton.OverrideText("ESCORT");
                    break;
                case CustomRoles.Crusader:
                    __instance.KillButton.OverrideText("CRUSADE");
                    break;
                case CustomRoles.Sniper:
                    __instance.AbilityButton.OverrideText(Sniper.OverrideShapeText(player.PlayerId));
                    break;
                case CustomRoles.FireWorks:
                    if (FireWorks.nowFireWorksCount[player.PlayerId] == 0)
                        __instance.AbilityButton.OverrideText($"{GetString("FireWorksExplosionButtonText")}");
                    else
                        __instance.AbilityButton.OverrideText($"{GetString("FireWorksInstallAtionButtonText")}");
                    break;
                case CustomRoles.Camouflager:
                    __instance.AbilityButton.OverrideText("CAMOUFLAGE");
                    break;
                case CustomRoles.Grenadier:
                    if (!Utils.IsActive(SystemTypes.Electrical))
                        __instance.AbilityButton.OverrideText("FLASH");
                    else
                    {
                        __instance.AbilityButton.OverrideText($"{GetString("DefaultShapeshiftText")}");
                    }
                    break;
                case CustomRoles.SerialKiller:
                    SerialKiller.GetAbilityButtonText(__instance);
                    break;
                case CustomRoles.Vulture:
                    __instance.ReportButton.OverrideText("EAT");
                    break;
                case CustomRoles.Cleaner:
                    __instance.ReportButton.OverrideText("CLEAN");
                    break;
                case CustomRoles.Warlock:
                    if (!Main.CheckShapeshift[player.PlayerId])
                    {
                        __instance.KillButton.OverrideText($"{GetString("WarlockCurseButtonText")}");
                    }
                    else
                    {
                        __instance.KillButton.OverrideText($"{GetString("KillButtonText")}");
                    }
                    break;
                case CustomRoles.Witch:
                    if (player.IsSpellMode())
                    {
                        __instance.KillButton.OverrideText($"{GetString("WitchSpellButtonText")}");
                    }
                    else
                    {
                        __instance.KillButton.OverrideText($"{GetString("KillButtonText")}");
                    }
                    break;
                case CustomRoles.Silencer:
                    if (Main.SilencedPlayer.Count == 0)
                    {
                        __instance.KillButton.OverrideText($"{GetString("SilenceButtonText")}");
                    }
                    else
                    {
                        __instance.KillButton.OverrideText($"{GetString("KillButtonText")}");
                    }
                    break;
                case CustomRoles.Vampress:
                    if (!Main.CheckShapeshift[player.PlayerId])
                        __instance.KillButton.OverrideText($"{GetString("VampireBiteButtonText")}");
                    else
                        __instance.KillButton.OverrideText($"{GetString("KillButtonText")}");
                    break;
                case CustomRoles.Vampire:
                    __instance.KillButton.OverrideText($"{GetString("VampireBiteButtonText")}");
                    break;
                case CustomRoles.Arsonist:
                    __instance.KillButton.OverrideText($"{GetString("ArsonistDouseButtonText")}");
                    break;
                case CustomRoles.Puppeteer:
                    __instance.KillButton.OverrideText($"{GetString("PuppeteerOperateButtonText")}");
                    break;
                case CustomRoles.YingYanger:
                    if (Main.DoingYingYang)
                        __instance.KillButton.OverrideText($"{GetString("PuppeteerOperateButtonText")}");
                    else
                        __instance.KillButton.OverrideText($"{GetString("KillButtonText")}");
                    break;
                case CustomRoles.PlagueBearer:
                    __instance.KillButton.OverrideText($"{GetString("InfectButtonText")}");
                    break;
                case CustomRoles.Pestilence:
                    __instance.KillButton.OverrideText($"{GetString("KillButtonText")}");
                    break;
                case CustomRoles.Poisoner:
                    __instance.KillButton.OverrideText($"{GetString("PoisonButtonText")}");
                    break;
                case CustomRoles.BountyHunter:
                    BountyHunter.GetAbilityButtonText(__instance);
                    break;
                case CustomRoles.Veteran:
                    __instance.AbilityButton.OverrideText($"ALERT");
                    break;
                case CustomRoles.Bastion:
                    __instance.AbilityButton.OverrideText($"BOMB");
                    break;
                case CustomRoles.Medium:
                    __instance.AbilityButton.OverrideText($"MEDITATE");
                    break;
                case CustomRoles.Mayor:
                    __instance.AbilityButton.OverrideText($"BUTTON");
                    break;
                case CustomRoles.GuardianAngelTOU:
                    __instance.AbilityButton.OverrideText($"PROTECT");
                    break;
                case CustomRoles.Survivor:
                    __instance.AbilityButton.OverrideText($"VEST");
                    break;
                case CustomRoles.Transporter:
                    __instance.AbilityButton.OverrideText($"TRANSPORT");
                    break;
                case CustomRoles.Amnesiac:
                    __instance.ReportButton.OverrideText($"REMEMBER");
                    break;
            }

            __instance.GameSettings.text = OptionShower.GetText();
            __instance.GameSettings.fontSizeMin =
            __instance.GameSettings.fontSizeMax = (TranslationController.Instance.currentLanguage.languageID == SupportedLangs.Japanese || Main.ForceJapanese.Value) ? 1.05f : 1.2f;
            //ゲーム中でなければ以下は実行されない
            if (!AmongUsClient.Instance.IsGameStarted) return;
            //バウンティハンターのターゲットテキスト
            if (LowerInfoText == null)
            {
                LowerInfoText = UnityEngine.Object.Instantiate(__instance.KillButton.buttonLabelText);
                LowerInfoText.transform.parent = __instance.transform;
                LowerInfoText.transform.localPosition = new Vector3(0, -2f, 0);
                LowerInfoText.alignment = TMPro.TextAlignmentOptions.Center;
                LowerInfoText.overflowMode = TMPro.TextOverflowModes.Overflow;
                LowerInfoText.enableWordWrapping = false;
                LowerInfoText.color = Palette.EnabledColor;
                LowerInfoText.fontSizeMin = 2.0f;
                LowerInfoText.fontSizeMax = 2.0f;
            }

            if (player.Is(CustomRoles.BountyHunter)) BountyHunter.DisplayTarget(player, LowerInfoText);
            else if (player.Is(CustomRoles.Witch))
            {
                //魔女用処理
                var ModeLang = player.IsSpellMode() ? "WitchModeSpell" : "WitchModeKill";
                LowerInfoText.text = GetString("WitchCurrentMode") + ": " + GetString(ModeLang);
                LowerInfoText.enabled = true;
            }
            else if (player.Is(CustomRoles.HexMaster))
            {
                //魔女用処理
                var ModeLang = player.IsHexMode() ? "Hexing" : "Killing";
                LowerInfoText.text = "Current Mode" + ": " + ModeLang;
                LowerInfoText.enabled = true;
            }
            else if (player.Is(CustomRoles.Werewolf))
            {
                var ModeLang = Main.IsRampaged ? "True" : "False";
                var ReadyLang = Main.RampageReady ? "True" : "False";
                LowerInfoText.text = "Is Rampaging: " + ModeLang;
                LowerInfoText.text += "\nRampage Ready: " + ReadyLang;
                LowerInfoText.enabled = true;
            }
            else if (player.Is(CustomRoles.Medusa))
            {
                var ModeLang = Main.IsGazing ? "True" : "False";
                var ReadyLang = Main.GazeReady ? "True" : "False";
                LowerInfoText.text = "Is Gazing: " + ModeLang;
                LowerInfoText.text += "\nGazing Ready: " + ReadyLang;
                LowerInfoText.enabled = true;
            }
            else if (player.Is(CustomRoles.TheGlitch))
            {
                var ModeLang = Main.IsHackMode ? "Hack" : "Kill";
                LowerInfoText.text = "Glitch Current Mode: " + ModeLang;
                LowerInfoText.enabled = true;
            }
            else if (player.Is(CustomRoles.FireWorks))
            {
                var stateText = FireWorks.GetStateText(player);
                LowerInfoText.text = stateText;
                LowerInfoText.enabled = true;
            }
            else if (player.Is(CustomRoles.Swooper))
            {
                var ModeLang = Main.IsInvis ? "Yes" : "No";
                var ReadyLang = Main.CanGoInvis ? "Yes" : "No";
                LowerInfoText.text = "Is Swooping: " + ModeLang;
                LowerInfoText.text += "\nCan Swoop: " + ReadyLang;
                LowerInfoText.enabled = true;
            }
            else
            {
                LowerInfoText.enabled = false;
            }
            if (!AmongUsClient.Instance.IsGameStarted && AmongUsClient.Instance.GameMode != GameModes.FreePlay)
            {
                LowerInfoText.enabled = false;
            }

            if (!player.GetCustomRole().IsVanilla())
            {
                TaskTextPrefix = Helpers.ColorString(player.GetRoleColor(), $"Role: {player.GetRoleName()}\r\n");
                if (player.Is(CustomRoles.Mafia))
                    TaskTextPrefix += Helpers.ColorString(player.GetRoleColor(), GetString(player.CanUseKillButton() ? "AfterMafiaInfo" : "BeforeMafiaInfo"));
                else
                {
                    if (player.Is(CustomRoles.Pirate))
                        TaskTextPrefix += Helpers.ColorString(player.GetRoleColor(), $"Successfully plunder {Guesser.PirateGuessAmount.GetInt()} players.");
                    else if (player.Is(CustomRoles.Executioner) | player.Is(CustomRoles.Swapper))
                    {
                        byte target = 0x6;
                        foreach (var playere in Main.ExecutionerTarget)
                        {
                            if (playere.Key == player.PlayerId)
                                target = playere.Value;
                        }
                        TaskTextPrefix += Helpers.ColorString(player.GetRoleColor(), $"Vote {Utils.GetPlayerById(target).GetRealName(isMeeting: true)} Out");
                    }
                    else
                        TaskTextPrefix += Helpers.ColorString(player.GetRoleColor(), GetString(player.GetCustomRole() + "Info"));
                }
                TaskTextPrefix += "</color>\r\n";
            }
            else
            {
                switch (player.GetCustomRole())
                {
                    case CustomRoles.Crewmate:
                        TaskTextPrefix = Helpers.ColorString(player.GetRoleColor(), $"Role: {player.GetRoleName()}\r\n");
                        TaskTextPrefix += Helpers.ColorString(player.GetRoleColor(), "Do your tasks and get the impostors out.");
                        break;
                    case CustomRoles.Engineer:
                        TaskTextPrefix = Helpers.ColorString(player.GetRoleColor(), $"Role: {player.GetRoleName()}\r\n");
                        TaskTextPrefix += Helpers.ColorString(player.GetRoleColor(), "Use the Vents to Catch Killers in the Act");
                        break;
                    case CustomRoles.Scientist:
                        TaskTextPrefix = Helpers.ColorString(player.GetRoleColor(), $"Role: {player.GetRoleName()}\r\n");
                        TaskTextPrefix += Helpers.ColorString(player.GetRoleColor(), "Use Vitals to Catch Self Reports");
                        break;
                    case CustomRoles.Impostor:
                        TaskTextPrefix = Helpers.ColorString(player.GetRoleColor(), $"Role: {player.GetRoleName()}\r\n");
                        TaskTextPrefix += Helpers.ColorString(player.GetRoleColor(), "Sabotage and leave none standing.");
                        break;
                    case CustomRoles.Shapeshifter:
                        TaskTextPrefix = Helpers.ColorString(player.GetRoleColor(), $"Role: {player.GetRoleName()}\r\n");
                        TaskTextPrefix += Helpers.ColorString(player.GetRoleColor(), "Shift to confuse the crew of who's who.");
                        break;
                    case CustomRoles.GuardianAngel:
                        TaskTextPrefix = Helpers.ColorString(player.GetRoleColor(), $"Role: {player.GetRoleName()}\r\n");
                        TaskTextPrefix += Helpers.ColorString(player.GetRoleColor(), "Protect your fellow remaining crew.");
                        break;
                }
                TaskTextPrefix += "</color>\r\n";
            }
            var cSubRoleFound = Main.AllPlayerCustomSubRoles.TryGetValue(player.PlayerId, out var cSubRole);
            if (cSubRoleFound)
            {
                /*CustomRoles role = CustomRoles.Amnesiac;
                foreach (var modifier in Main.HasModifier)
                {
                    if (modifier.Value == player.PlayerId)
                        role = modifier.Key;
                }*/
                TaskTextPrefix += Helpers.ColorString(Utils.GetRoleColor(player.GetCustomSubRole()), $"Modifier: {player.GetSubRoleName()}\r\n");
                if (player.GetCustomSubRole() != CustomRoles.LoversRecode)
                    TaskTextPrefix += Helpers.ColorString(Utils.GetRoleColor(player.GetCustomSubRole()), $"{GetString(player.GetSubRoleName() + "Info")}\r\n");
                else
                {
                    string name = "";
                    foreach (var lp in Main.LoversPlayers)
                    {
                        if (lp.PlayerId == player.PlayerId) continue;
                        name = lp.GetRealName(true);
                    }
                    TaskTextPrefix += Helpers.ColorString(Utils.GetRoleColor(player.GetCustomSubRole()), $"You are in love with {name}.\r\n");
                }
            }
            if (!Utils.HasTasks(player.Data, false) && !player.GetCustomRole().IsImpostor())
                TaskTextPrefix += FakeTasksText;
            switch (player.GetCustomRole())
            {
                case CustomRoles.Mafia:
                case CustomRoles.Mare:
                case CustomRoles.FireWorks:
                case CustomRoles.Sniper:
                    if (player.CanUseKillButton())
                    {
                        __instance.KillButton.ToggleVisible(true && !player.Data.IsDead);
                    }
                    else
                    {
                        __instance.KillButton.SetDisabled();
                        __instance.KillButton.ToggleVisible(false);
                    }
                    break;
                case CustomRoles.SKMadmate:
                    //TaskTextPrefix += FakeTasksText;
                    __instance.KillButton.SetDisabled();
                    __instance.KillButton.ToggleVisible(false);
                    break;
                case CustomRoles.Investigator:
                    if (!Investigator.CanUseKillButton(player))
                    {
                        __instance.KillButton.SetDisabled();
                        __instance.KillButton.ToggleVisible(false);
                    }
                    player.CanUseImpostorVent();
                    goto DesyncImpostor;
                case CustomRoles.Sheriff:
                    if (Sheriff.ShotLimit.TryGetValue(player.PlayerId, out var count) && count == 0)
                    {
                        __instance.KillButton.SetDisabled();
                        __instance.KillButton.ToggleVisible(false);
                    }
                    player.CanUseImpostorVent();
                    goto DesyncImpostor;
                case CustomRoles.CorruptedSheriff:
                    player.CanUseImpostorVent();
                    goto DesyncImpostor;
                case CustomRoles.Arsonist:
                    // TaskTextPrefix += FakeTasksText;
                    if (player.IsDouseDone() && !Options.TOuRArso.GetBool())
                    {
                        __instance.KillButton.SetDisabled();
                        __instance.KillButton.ToggleVisible(false);
                    }
                    player.CanUseImpostorVent();
                    goto DesyncImpostor;
                case CustomRoles.Janitor:
                case CustomRoles.Painter:
                case CustomRoles.PlagueBearer:
                    // TaskTextPrefix += FakeTasksText;
                    player.CanUseImpostorVent();
                    goto DesyncImpostor;
                case CustomRoles.Pestilence:
                    // TaskTextPrefix += FakeTasksText;
                    player.CanUseImpostorVent();
                    goto DesyncImpostor;
                case CustomRoles.Juggernaut:
                    //  TaskTextPrefix += FakeTasksText;
                    player.CanUseImpostorVent();
                    goto DesyncImpostor;
                case CustomRoles.BloodKnight:
                    player.CanUseImpostorVent();
                    goto DesyncImpostor;
                case CustomRoles.Werewolf:
                    // TaskTextPrefix += FakeTasksText;
                    player.CanUseImpostorVent();
                    goto DesyncImpostor;
                case CustomRoles.TheGlitch:
                    //TaskTextPrefix += FakeTasksText;
                    player.CanUseImpostorVent();
                    goto DesyncImpostor;
                case CustomRoles.Marksman:
                case CustomRoles.Sidekick:
                case CustomRoles.Jackal:
                    //   TaskTextPrefix += FakeTasksText;
                    player.CanUseImpostorVent();
                    goto DesyncImpostor;

                DesyncImpostor:
                    if (player.Data.Role.Role != RoleTypes.GuardianAngel)
                        player.Data.Role.CanUseKillButton = true;
                    break;
            }

            switch (player.GetCustomRole())
            {
                case CustomRoles.CovenWitch:
                    //   TaskTextPrefix += FakeTasksText;
                    if (player.Data.Role.Role != RoleTypes.GuardianAngel)
                        player.Data.Role.CanUseKillButton = true;
                    if (Main.HasNecronomicon)
                        player.CanUseImpostorVent();
                    break;
                case CustomRoles.HexMaster:
                    ///  TaskTextPrefix += FakeTasksText;
                    if (player.Data.Role.Role != RoleTypes.GuardianAngel)
                        player.Data.Role.CanUseKillButton = true;
                    if (Main.HasNecronomicon)
                        player.CanUseImpostorVent();
                    break;
                case CustomRoles.Medusa:
                    // TaskTextPrefix += FakeTasksText;
                    player.CanUseImpostorVent();
                    if (Main.HasNecronomicon)
                    {
                        if (player.Data.Role.Role != RoleTypes.GuardianAngel)
                            player.Data.Role.CanUseKillButton = true;
                    }
                    else
                        player.Data.Role.CanUseKillButton = false;
                    break;
                case CustomRoles.Conjuror:
                    // TaskTextPrefix += FakeTasksText;
                    break;
                case CustomRoles.Mimic:
                    //  TaskTextPrefix += FakeTasksText;
                    if (player.Data.Role.Role != RoleTypes.GuardianAngel)
                        player.Data.Role.CanUseKillButton = true;
                    break;
            }

            if (!__instance.TaskText.text.Contains(TaskTextPrefix)) __instance.TaskText.text = TaskTextPrefix + "\r\n" + __instance.TaskText.text;

            if (Input.GetKeyDown(KeyCode.Y) && AmongUsClient.Instance.GameMode == GameModes.FreePlay)
            {
                Action<MapBehaviour> tmpAction = (MapBehaviour m) => { m.ShowSabotageMap(); };
                __instance.ShowMap(tmpAction);
                if (player.AmOwner)
                {
                    player.MyPhysics.inputHandler.enabled = true;
                    ConsoleJoystick.SetMode_Task();
                }
            }

            if (AmongUsClient.Instance.GameMode == GameModes.OnlineGame) RepairSender.enabled = false;
            if (Input.GetKeyDown(KeyCode.RightShift) && AmongUsClient.Instance.GameMode != GameModes.OnlineGame)
            {
                RepairSender.enabled = !RepairSender.enabled;
                RepairSender.Reset();
            }
            if (RepairSender.enabled && AmongUsClient.Instance.GameMode != GameModes.OnlineGame)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0)) RepairSender.Input(0);
                if (Input.GetKeyDown(KeyCode.Alpha1)) RepairSender.Input(1);
                if (Input.GetKeyDown(KeyCode.Alpha2)) RepairSender.Input(2);
                if (Input.GetKeyDown(KeyCode.Alpha3)) RepairSender.Input(3);
                if (Input.GetKeyDown(KeyCode.Alpha4)) RepairSender.Input(4);
                if (Input.GetKeyDown(KeyCode.Alpha5)) RepairSender.Input(5);
                if (Input.GetKeyDown(KeyCode.Alpha6)) RepairSender.Input(6);
                if (Input.GetKeyDown(KeyCode.Alpha7)) RepairSender.Input(7);
                if (Input.GetKeyDown(KeyCode.Alpha8)) RepairSender.Input(8);
                if (Input.GetKeyDown(KeyCode.Alpha9)) RepairSender.Input(9);
                if (Input.GetKeyDown(KeyCode.Return)) RepairSender.InputEnter();
                __instance.TaskText.text = RepairSender.GetText();
            }
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ToggleHighlight))]
    class ToggleHighlightPatch
    {
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] bool active, [HarmonyArgument(1)] RoleTeamTypes team)
        {
            var player = PlayerControl.LocalPlayer;
            if ((player.GetCustomRole() == CustomRoles.Sheriff ||
                player.GetCustomRole() == CustomRoles.Investigator ||
                player.GetCustomRole() == CustomRoles.CorruptedSheriff ||
                player.GetCustomRole() == CustomRoles.Arsonist ||
                player.GetCustomRole() == CustomRoles.Jackal ||
                player.GetCustomRole() == CustomRoles.Escort ||
                player.GetCustomRole() == CustomRoles.Crusader ||
                player.GetCustomRole() == CustomRoles.Sidekick ||
                player.GetCustomRole() == CustomRoles.TheGlitch ||
                player.GetCustomRole() == CustomRoles.Werewolf ||
                player.GetCustomRole() == CustomRoles.Painter ||
                player.GetCustomRole() == CustomRoles.Janitor ||
                player.GetCustomRole() == CustomRoles.Juggernaut ||
                player.GetCustomRole() == CustomRoles.Marksman ||
                player.GetCustomRole() == CustomRoles.BloodKnight ||
                player.GetCustomRole() == CustomRoles.PlagueBearer ||
                player.GetCustomRole() == CustomRoles.Pestilence ||
                player.GetRoleType() == RoleType.Coven ||
                player.GetRoleType() == RoleType.Madmate)
            && !player.Data.IsDead)
            {
                ((Renderer)__instance.cosmetics.currentBodySprite.BodySprite).material.SetColor("_OutlineColor", Utils.GetRoleColor(player.GetCustomRole()));
            }
        }
    }
    [HarmonyPatch(typeof(Vent), nameof(Vent.SetOutline))]
    class SetVentOutlinePatch
    {
        public static void Postfix(Vent __instance, [HarmonyArgument(1)] ref bool mainTarget)
        {
            var player = PlayerControl.LocalPlayer;
            Color color = PlayerControl.LocalPlayer.GetRoleColor();
            ((Renderer)__instance.myRend).material.SetColor("_OutlineColor", color);
            ((Renderer)__instance.myRend).material.SetColor("_AddColor", mainTarget ? color : Color.clear);
        }
    }
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive))]
    class SetHudActivePatch
    {
        public static void Postfix(HudManager __instance, [HarmonyArgument(0)] bool isActive)
        {
            var player = PlayerControl.LocalPlayer;
            switch (player.GetCustomRole())
            {
                case CustomRoles.Sheriff:
                case CustomRoles.Investigator:
                case CustomRoles.Arsonist:
                    if (player.Data.Role.Role != RoleTypes.GuardianAngel)
                        __instance.KillButton.ToggleVisible(isActive && !player.Data.IsDead);
                    __instance.SabotageButton.ToggleVisible(false);
                    __instance.ImpostorVentButton.ToggleVisible(false);
                    __instance.AbilityButton.ToggleVisible(false);
                    break;
                case CustomRoles.Janitor:
                case CustomRoles.Painter:
                    if (player.Data.Role.Role != RoleTypes.GuardianAngel)
                        __instance.KillButton.ToggleVisible(isActive && !player.Data.IsDead);
                    __instance.SabotageButton.ToggleVisible(false);
                    __instance.ImpostorVentButton.ToggleVisible(isActive && Options.STIgnoreVent.GetBool());
                    __instance.AbilityButton.ToggleVisible(false);
                    break;
                case CustomRoles.Sidekick:
                case CustomRoles.Jackal:
                    if (player.Data.Role.Role != RoleTypes.GuardianAngel)
                        __instance.KillButton.ToggleVisible(isActive && !player.Data.IsDead);
                    __instance.SabotageButton.ToggleVisible(isActive && Options.JackalCanUseSabotage.GetBool());
                    __instance.ImpostorVentButton.ToggleVisible(isActive && Options.JackalCanVent.GetBool());
                    __instance.AbilityButton.ToggleVisible(false);
                    break;
                case CustomRoles.CorruptedSheriff:
                    if (player.Data.Role.Role != RoleTypes.GuardianAngel)
                        __instance.KillButton.ToggleVisible(isActive && !player.Data.IsDead);
                    __instance.SabotageButton.ToggleVisible(true);
                    __instance.ImpostorVentButton.ToggleVisible(isActive);
                    __instance.AbilityButton.ToggleVisible(false);
                    break;
                case CustomRoles.PlagueBearer:
                    if (player.Data.Role.Role != RoleTypes.GuardianAngel)
                        __instance.KillButton.ToggleVisible(isActive && !player.Data.IsDead);
                    __instance.SabotageButton.ToggleVisible(false);
                    __instance.ImpostorVentButton.ToggleVisible(false);
                    __instance.AbilityButton.ToggleVisible(false);
                    break;
                case CustomRoles.TheGlitch:
                    if (player.Data.Role.Role != RoleTypes.GuardianAngel)
                        __instance.KillButton.ToggleVisible(isActive && !player.Data.IsDead);
                    __instance.SabotageButton.ToggleVisible(false);
                    __instance.ImpostorVentButton.ToggleVisible(true);
                    //__instance.AbilityButton.ToggleVisible(true);
                    break;
                case CustomRoles.Werewolf:
                    if (player.Data.Role.Role != RoleTypes.GuardianAngel)
                        __instance.KillButton.ToggleVisible(isActive && !player.Data.IsDead);
                    __instance.SabotageButton.ToggleVisible(false);
                    __instance.ImpostorVentButton.ToggleVisible(true);
                    __instance.AbilityButton.ToggleVisible(false);
                    break;
                case CustomRoles.Pestilence:
                    if (player.Data.Role.Role != RoleTypes.GuardianAngel)
                        __instance.KillButton.ToggleVisible(isActive && !player.Data.IsDead);
                    __instance.SabotageButton.ToggleVisible(false);
                    __instance.ImpostorVentButton.ToggleVisible(isActive && Options.PestiCanVent.GetBool());
                    __instance.AbilityButton.ToggleVisible(false);
                    break;
                case CustomRoles.Juggernaut:
                    if (player.Data.Role.Role != RoleTypes.GuardianAngel)
                        __instance.KillButton.ToggleVisible(isActive && !player.Data.IsDead);
                    __instance.SabotageButton.ToggleVisible(false);
                    __instance.ImpostorVentButton.ToggleVisible(Options.JuggerCanVent.GetBool());
                    __instance.AbilityButton.ToggleVisible(false);
                    break;
                case CustomRoles.BloodKnight:
                    if (player.Data.Role.Role != RoleTypes.GuardianAngel)
                        __instance.KillButton.ToggleVisible(isActive && !player.Data.IsDead);
                    __instance.SabotageButton.ToggleVisible(false);
                    __instance.ImpostorVentButton.ToggleVisible(Options.BKcanVent.GetBool() && !player.Data.IsDead);
                    __instance.AbilityButton.ToggleVisible(false);
                    break;
                case CustomRoles.Marksman:
                    if (player.Data.Role.Role != RoleTypes.GuardianAngel)
                        __instance.KillButton.ToggleVisible(isActive && !player.Data.IsDead);
                    __instance.SabotageButton.ToggleVisible(false);
                    __instance.ImpostorVentButton.ToggleVisible(Options.MarksmanCanVent.GetBool() && !player.Data.IsDead);
                    __instance.AbilityButton.ToggleVisible(false);
                    break;
                case CustomRoles.Opportunist:
                case CustomRoles.Executioner:
                case CustomRoles.Jester:
                case CustomRoles.Swapper:
                case CustomRoles.Amnesiac:
                    if (player.Data.Role.Role != RoleTypes.GuardianAngel)
                        __instance.KillButton.ToggleVisible(false);
                    __instance.SabotageButton.ToggleVisible(false);
                    __instance.ImpostorVentButton.ToggleVisible(false);
                    if (!player.Is(CustomRoles.Jester))
                        __instance.AbilityButton.ToggleVisible(false);
                    break;
                case CustomRoles.Medusa:
                    if (player.Data.Role.Role != RoleTypes.GuardianAngel)
                        __instance.KillButton.ToggleVisible(isActive && !player.Data.IsDead && Main.HasNecronomicon);
                    __instance.SabotageButton.ToggleVisible(false);
                    __instance.ImpostorVentButton.ToggleVisible(isActive);
                    __instance.AbilityButton.ToggleVisible(false);
                    break;
                case CustomRoles.HexMaster:
                    if (player.Data.Role.Role != RoleTypes.GuardianAngel)
                        __instance.KillButton.ToggleVisible(true);
                    __instance.SabotageButton.ToggleVisible(false);
                    __instance.ImpostorVentButton.ToggleVisible(isActive && Main.HasNecronomicon);
                    __instance.AbilityButton.ToggleVisible(false);
                    break;
                case CustomRoles.Hitman:
                    if (player.Data.Role.Role != RoleTypes.GuardianAngel)
                        __instance.KillButton.ToggleVisible(isActive && !player.Data.IsDead);
                    __instance.SabotageButton.ToggleVisible(false);
                    __instance.ImpostorVentButton.ToggleVisible(isActive && Options.HitmanCanVent.GetBool());
                    __instance.AbilityButton.ToggleVisible(false);
                    break;
                case CustomRoles.CovenWitch:
                    if (player.Data.Role.Role != RoleTypes.GuardianAngel)
                        __instance.KillButton.ToggleVisible(isActive && !player.Data.IsDead);
                    __instance.SabotageButton.ToggleVisible(false);
                    __instance.ImpostorVentButton.ToggleVisible(isActive && Main.HasNecronomicon);
                    __instance.AbilityButton.ToggleVisible(false);
                    break;
                case CustomRoles.Parasite:
                    if (player.Data.Role.Role != RoleTypes.GuardianAngel)
                        __instance.KillButton.ToggleVisible(isActive && !player.Data.IsDead);
                    __instance.SabotageButton.ToggleVisible(false);
                    __instance.ImpostorVentButton.ToggleVisible(isActive);
                    __instance.AbilityButton.ToggleVisible(isActive);
                    break;
                case CustomRoles.Grenadier:
                    if (!Options.GrenadierCanVent.GetBool())
                        __instance.ImpostorVentButton.ToggleVisible(false);
                    break;
                case CustomRoles.Camouflager:
                    if (!Camouflager.CanVent())
                        __instance.ImpostorVentButton.ToggleVisible(false);
                    break;
                case CustomRoles.Escort:
                    if (player.Data.Role.Role != RoleTypes.GuardianAngel)
                        __instance.KillButton.ToggleVisible(isActive && !player.Data.IsDead);
                    __instance.SabotageButton.ToggleVisible(false);
                    __instance.ImpostorVentButton.ToggleVisible(false);
                    __instance.AbilityButton.ToggleVisible(false);
                    break;
                case CustomRoles.Crusader:
                    if (player.Data.Role.Role != RoleTypes.GuardianAngel)
                        __instance.KillButton.ToggleVisible(isActive && !player.Data.IsDead && !Main.HasTarget[player.PlayerId]);
                    __instance.SabotageButton.ToggleVisible(false);
                    __instance.ImpostorVentButton.ToggleVisible(false);
                    __instance.AbilityButton.ToggleVisible(false);
                    break;
            }
            if (__instance.KillButton == null) return;
        }
    }
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.Start))]
    public static class KillButtonAwake
    {
        public static void Prefix(KillButton __instance)
        {
            //if (Main.ButtonImages.Value)
            //    __instance.transform.Find("Text_TMP").gameObject.SetActive(false);
        }
    }
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class HudManagerUpdate
    {
        public static void Postfix(HudManager __instance)
        {
            if (PlayerControl.LocalPlayer.GetCustomSubRole() is CustomRoles.Oblivious)
            {
                try
                {
                    DestroyableSingleton<HudManager>.Instance.ReportButton.SetActive(false);
                }
                catch
                {

                }
            }
        }
    }
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowNormalMap))]
    class ShowNormalMapPatch
    {
        public static void Prefix(ref RoleTeamTypes __state)
        {
            var player = PlayerControl.LocalPlayer;
            if (player.Is(CustomRoles.Sheriff) || player.Is(CustomRoles.BloodKnight) || player.Is(CustomRoles.Escort) || player.Is(CustomRoles.Crusader) || player.Is(CustomRoles.Investigator) || player.Is(CustomRoles.Parasite) || player.Is(CustomRoles.Arsonist) || player.Is(CustomRoles.PlagueBearer) || player.Is(CustomRoles.TheGlitch) || player.Is(CustomRoles.Werewolf) || player.Is(CustomRoles.Opportunist) || player.Is(CustomRoles.Executioner) || player.Is(CustomRoles.Swapper) || player.Is(CustomRoles.Jester) || player.Is(CustomRoles.Pestilence))
            {
                __state = player.Data.Role.TeamType;
                player.Data.Role.TeamType = RoleTeamTypes.Crewmate;
            }
            if (player.Is(CustomRoles.CorruptedSheriff))
            {
                __state = player.Data.Role.TeamType;
                player.Data.Role.TeamType = RoleTeamTypes.Impostor;
            }
        }

        public static void Postfix(ref RoleTeamTypes __state)
        {
            var player = PlayerControl.LocalPlayer;
            if (player.Is(CustomRoles.Sheriff) || player.Is(CustomRoles.BloodKnight) || player.Is(CustomRoles.Escort) || player.Is(CustomRoles.Crusader) || player.Is(CustomRoles.Investigator) || player.Is(CustomRoles.Parasite) || player.Is(CustomRoles.Arsonist) || player.Is(CustomRoles.PlagueBearer) || player.Is(CustomRoles.TheGlitch) || player.Is(CustomRoles.Werewolf) || player.Is(CustomRoles.Pestilence))
            {
                player.Data.Role.TeamType = __state;
            }
            if (player.Is(CustomRoles.CorruptedSheriff))
            {
                player.Data.Role.TeamType = __state;
            }
        }
    }
    class RepairSender
    {
        public static bool enabled = false;
        public static bool TypingAmount = false;

        public static int SystemType;
        public static int amount;

        public static void Input(int num)
        {
            if (!TypingAmount)
            {
                //SystemType入力中
                SystemType *= 10;
                SystemType += num;
            }
            else
            {
                //Amount入力中
                amount *= 10;
                amount += num;
            }
        }
        public static void InputEnter()
        {
            if (!TypingAmount)
            {
                //SystemType入力中
                TypingAmount = true;
            }
            else
            {
                //Amount入力中
                Send();
            }
        }
        public static void Send()
        {
            ShipStatus.Instance.RpcRepairSystem((SystemTypes)SystemType, amount);
            Reset();
        }
        public static void Reset()
        {
            TypingAmount = false;
            SystemType = 0;
            amount = 0;
        }
        public static string GetText()
        {
            return SystemType.ToString() + "(" + ((SystemTypes)SystemType).ToString() + ")\r\n" + amount;
        }
    }
}
