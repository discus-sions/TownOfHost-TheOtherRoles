using System;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using static TownOfHost.Translator;

namespace TownOfHost
{
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.ShowRole))]
    class SetUpRoleTextPatch
    {
        public static void Postfix(IntroCutscene __instance)
        {
            new LateTask(() =>
            {
                CustomRoles role = PlayerControl.LocalPlayer.GetCustomRole();
                if (!role.IsVanilla())
                {
                    __instance.YouAreText.color = Utils.GetRoleColor(role);
                    __instance.RoleText.text = Utils.GetRoleName(role);
                    __instance.RoleText.color = Utils.GetRoleColor(role);
                    __instance.RoleBlurbText.color = Utils.GetRoleColor(role);

                    __instance.RoleBlurbText.text = PlayerControl.LocalPlayer.Is(CustomRoles.EvilWatcher) || PlayerControl.LocalPlayer.Is(CustomRoles.NiceWatcher)
                        ? GetString("WatcherInfo")
                        : GetString(role.ToString() + "Info");
                    /*__instance.RoleBlurbText.text = PlayerControl.LocalPlayer.Is(CustomRoles.EvilGuesser) || PlayerControl.LocalPlayer.Is(CustomRoles.NiceGuesser)
                        ? GetString("GuesserInfo")
                        : GetString(role.ToString() + "Info");*/
                    if (PlayerControl.LocalPlayer.Is(CustomRoles.Executioner) | PlayerControl.LocalPlayer.Is(CustomRoles.Swapper))
                    {
                        byte target = 0x6;
                        foreach (var player in Main.ExecutionerTarget)
                        {
                            if (player.Key == PlayerControl.LocalPlayer.PlayerId)
                                target = player.Value;
                        }
                        if (PlayerControl.LocalPlayer.Is(CustomRoles.Executioner) | PlayerControl.LocalPlayer.Is(CustomRoles.Swapper))
                            __instance.RoleBlurbText.text = "Vote " + Utils.GetPlayerById(target).GetRealName(isMeeting: true) + " Out";
                    }
                }
                else
                {
                    switch (role)
                    {
                        case CustomRoles.Crewmate:
                            break;
                        case CustomRoles.Engineer:
                            break;
                        case CustomRoles.Scientist:
                            break;
                        case CustomRoles.Impostor:
                            break;
                        case CustomRoles.Shapeshifter:
                            break;
                    }
                }

                __instance.RoleText.text += Utils.GetShowLastSubRolesText(PlayerControl.LocalPlayer.PlayerId);

            }, 0.01f, "Override Role Text");

        }
    }
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
    class CoBeginPatch
    {
        public static void Prefix()
        {
            if (!AmongUsClient.Instance.AmHost)
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    switch (pc.GetCustomRole())
                    {
                        case CustomRoles.Egoist:
                            Egoist.Add(pc.PlayerId);
                            break;
                    }
                }
            Logger.Info("------------名前表示------------", "Info");
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                Logger.Info($"{(pc.AmOwner ? "[*]" : ""),-3}{pc.PlayerId,-2}:{pc.name.PadRightV2(20)}:{pc.cosmetics.nameText.text}", "Info");
                pc.cosmetics.nameText.text = pc.name;
            }
            Logger.Info("----------役職割り当て----------", "Info");
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                Logger.Info($"{(pc.AmOwner ? "[*]" : ""),-3}{pc.PlayerId,-2}:{pc?.Data?.PlayerName?.PadRightV2(20)}:{pc.GetAllRoleName()}", "Info");
            }
            Logger.Info("--------------環境--------------", "Info");
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                var text = pc.AmOwner ? "[*]" : "   ";
                text += $"{pc.PlayerId,-2}:{pc.Data?.PlayerName?.PadRightV2(20)}:{pc.GetClient().PlatformData.Platform.ToString().Replace("Standalone", ""),-11}";
                if (Main.playerVersion.TryGetValue(pc.PlayerId, out PlayerVersion pv))
                    text += $":Mod({pv.version}:{pv.tag})";
                else text += ":Vanilla";
                Logger.Info(text, "Info");
            }
            Logger.Info("------------基本設定------------", "Info");
            var tmp = PlayerControl.GameOptions.ToHudString(GameData.Instance ? GameData.Instance.PlayerCount : 10).Split("\r\n").Skip(1);
            foreach (var t in tmp) Logger.Info(t, "Info");
            Logger.Info("------------詳細設定------------", "Info");
            foreach (var o in CustomOption.Options)
                if (!o.IsHidden(Options.CurrentGameMode()) && (o.Parent == null ? !o.GetString().Equals("0%") : o.Parent.Enabled))
                    Logger.Info($"{(o.Parent == null ? o.Name.PadRightV2(40) : $"┗ {o.Name}".PadRightV2(41))}:{o.GetString().RemoveHtmlTags()}", "Info");
            Logger.Info("-------------その他-------------", "Info");
            Logger.Info($"プレイヤー数: {PlayerControl.AllPlayerControls.Count}人", "Info");
            PlayerControl.AllPlayerControls.ToArray().Do(x => PlayerState.InitTask(x));

            Utils.NotifyRoles();

            GameStates.InGame = true;
        }
    }
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
    class BeginCrewmatePatch
    {
        public static void Prefix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
        {
            if (PlayerControl.LocalPlayer.Is(RoleType.Neutral))
            {
                //ぼっち役職
                var soloTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                soloTeam.Add(PlayerControl.LocalPlayer);
                teamToDisplay = soloTeam;
            }
            if (PlayerControl.LocalPlayer.Is(RoleType.Coven))
            {
                //ぼっち役職
                var covenTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                covenTeam.Add(PlayerControl.LocalPlayer);
                foreach (var ar in PlayerControl.AllPlayerControls)
                {
                    if (ar.GetCustomRole().IsCoven() && ar != PlayerControl.LocalPlayer)
                        covenTeam.Add(ar);
                }
                teamToDisplay = covenTeam;
            }
            if (PlayerControl.LocalPlayer.GetCustomRole().IsJackalTeam())
            {
                //ぼっち役職
                var jackalTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                jackalTeam.Add(PlayerControl.LocalPlayer);
                foreach (var ar in PlayerControl.AllPlayerControls)
                {
                    if (ar.GetCustomRole().IsJackalTeam() && ar != PlayerControl.LocalPlayer)
                        jackalTeam.Add(ar);
                }
                teamToDisplay = jackalTeam;
            }
            if (PlayerControl.LocalPlayer.Is(CustomRoles.GuardianAngelTOU))
            {
                var gaTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                gaTeam.Add(PlayerControl.LocalPlayer);
                foreach (var protect in Main.GuardianAngelTarget)
                {
                    //PlayerControl ga = Utils.GetPlayerById(protect.Key);
                    PlayerControl protecting = Utils.GetPlayerById(protect.Value);
                    // if (ga == PlayerControl.LocalPlayer) continue;
                    gaTeam.Add(protecting);
                }
                teamToDisplay = gaTeam;
            }
        }
        public static void Postfix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay)
        {
            //チーム表示変更
            var rand = new System.Random();
            CustomRoles role = PlayerControl.LocalPlayer.GetCustomRole();
            CustomRoles modifier = PlayerControl.LocalPlayer.GetCustomSubRole();
            RoleType roleType = role.GetRoleType();

            switch (roleType)
            {
                case RoleType.Neutral:
                    __instance.TeamTitle.text = "NEUTRAL";
                    //__instance.TeamTitle.color = Utils.GetRoleColor(role);
                    __instance.TeamTitle.color = Utils.GetRoleColor(CustomRoles.Child);
                    //__instance.ImpostorText.gameObject.SetActive(true);
                    //__instance.ImpostorText.text = GetString("NeutralInfo");
                    if (PlayerControl.LocalPlayer.Is(CustomRoles.Executioner) | PlayerControl.LocalPlayer.Is(CustomRoles.Swapper))
                    {
                        byte target = 0x6;
                        foreach (var player in Main.ExecutionerTarget)
                        {
                            if (player.Key == PlayerControl.LocalPlayer.PlayerId)
                                target = player.Value;
                        }
                        if (PlayerControl.LocalPlayer.Is(CustomRoles.Executioner) | PlayerControl.LocalPlayer.Is(CustomRoles.Swapper))
                            __instance.ImpostorText.text += "\nVote " + Utils.GetPlayerById(target).GetRealName(isMeeting: true) + " Out";
                    }
                    __instance.BackgroundBar.material.color = Utils.GetRoleColor(role);
                    break;
                case RoleType.Madmate:
                    __instance.TeamTitle.text = GetString("Madmate");
                    __instance.TeamTitle.color = Utils.GetRoleColor(CustomRoles.Madmate);
                    __instance.ImpostorText.gameObject.SetActive(true);
                    __instance.ImpostorText.text = GetString("TeamImpostor");
                    StartFadeIntro(__instance, Palette.CrewmateBlue, Palette.ImpostorRed);
                    PlayerControl.LocalPlayer.Data.Role.IntroSound = GetIntroSound(RoleTypes.Impostor);
                    break;
                case RoleType.Crewmate:
                    //__instance.TeamTitle.color = Utils.GetRoleColor(role);
                    __instance.BackgroundBar.material.color = Utils.GetRoleColor(role);
                    break;
                case RoleType.Coven:
                    __instance.TeamTitle.text = "COVEN";
                    __instance.TeamTitle.color = Utils.GetRoleColor(CustomRoles.Coven);
                    __instance.ImpostorText.gameObject.SetActive(true);
                    __instance.ImpostorText.text = GetString("CovenIntroInfo");
                    __instance.BackgroundBar.material.color = Utils.GetRoleColor(CustomRoles.Coven);
                    break;
            }
            switch (modifier)
            {
                default:
                    if (modifier == CustomRoles.NoSubRoleAssigned) break;
                    break;
            }
            switch (role)
            {
                case CustomRoles.CrewPostor:
                    __instance.TeamTitle.text = "NEUTRAL";
                    __instance.TeamTitle.color = Utils.GetRoleColor(CustomRoles.Child);
                    //__instance.ImpostorText.gameObject.SetActive(true);
                    //__instance.ImpostorText.text = GetString("NeutralInfo");
                    __instance.BackgroundBar.material.color = Utils.GetRoleColor(role);
                    //StartFadeIntro(__instance, Color.black, Color.black);
                    PlayerControl.LocalPlayer.Data.Role.IntroSound = GetIntroSound(RoleTypes.Impostor);
                    break;
                case CustomRoles.Painter:
                    __instance.TeamTitle.text = "SPLATOON";
                    __instance.TeamTitle.color = Utils.GetRoleColor(CustomRoles.Painter);
                    __instance.ImpostorText.gameObject.SetActive(true);
                    __instance.ImpostorText.text = "Be the last color standing.";
                    __instance.BackgroundBar.material.color = Utils.GetRoleColor(role);
                    StartFadeIntro(__instance, Color.white, Color.black);
                    PlayerControl.LocalPlayer.Data.Role.IntroSound = GetIntroSound(RoleTypes.Shapeshifter);
                    break;
                case CustomRoles.Janitor:
                    __instance.TeamTitle.text = "SPLATOON";
                    __instance.TeamTitle.color = Utils.GetRoleColor(CustomRoles.Child);
                    __instance.ImpostorText.gameObject.SetActive(true);
                    __instance.ImpostorText.text = "Undo all of the paints Painters have done.";
                    __instance.BackgroundBar.material.color = Utils.GetRoleColor(role);
                    StartFadeIntro(__instance, Color.white, Color.black);
                    PlayerControl.LocalPlayer.Data.Role.IntroSound = GetIntroSound(RoleTypes.Impostor);
                    break;
                case CustomRoles.Supporter:
                    __instance.TeamTitle.text = "SPLATOON";
                    __instance.TeamTitle.color = Utils.GetRoleColor(CustomRoles.Supporter);
                    __instance.ImpostorText.gameObject.SetActive(true);
                    __instance.ImpostorText.text = "Do your tasks to win against Painters.";
                    __instance.BackgroundBar.material.color = Utils.GetRoleColor(role);
                    StartFadeIntro(__instance, Color.white, Color.black);
                    PlayerControl.LocalPlayer.Data.Role.IntroSound = ShipStatus.Instance.SabotageSound;
                    break;
                case CustomRoles.Jackal:
                    if (Options.FreeForAllOn.GetBool())
                    {
                        __instance.TeamTitle.text = "FREE FOR ALL";
                        __instance.TeamTitle.color = Utils.GetRoleColor(CustomRoles.Child);
                        __instance.ImpostorText.gameObject.SetActive(true);
                        __instance.ImpostorText.text = "Be the last killer standing.";
                        __instance.BackgroundBar.material.color = Utils.GetRoleColor(role);
                    }
                    PlayerControl.LocalPlayer.Data.Role.IntroSound = GetIntroSound(RoleTypes.Impostor);
                    break;
                case CustomRoles.Terrorist:
                    var sound = ShipStatus.Instance.CommonTasks.Where(task => task.TaskType == TaskTypes.FixWiring).FirstOrDefault()
                    .MinigamePrefab.OpenSound;
                    PlayerControl.LocalPlayer.Data.Role.IntroSound = sound;
                    break;

                case CustomRoles.Swapper:
                case CustomRoles.Executioner:
                case CustomRoles.Vampire:
                case CustomRoles.Medusa:
                case CustomRoles.HexMaster:
                case CustomRoles.CovenWitch:
                case CustomRoles.Conjuror:
                case CustomRoles.Coven:
                case CustomRoles.Silencer:
                    PlayerControl.LocalPlayer.Data.Role.IntroSound = GetIntroSound(RoleTypes.Shapeshifter);
                    break;

                case CustomRoles.SabotageMaster:
                    PlayerControl.LocalPlayer.Data.Role.IntroSound = ShipStatus.Instance.SabotageSound;
                    break;

                case CustomRoles.Sheriff:
                    PlayerControl.LocalPlayer.Data.Role.IntroSound = PlayerControl.LocalPlayer.KillSfx;
                    break;
                case CustomRoles.Investigator:
                case CustomRoles.Bastion:
                case CustomRoles.Arsonist:
                case CustomRoles.Veteran:
                case CustomRoles.Jester:
                case CustomRoles.GuardianAngelTOU:
                case CustomRoles.Survivor:
                case CustomRoles.Vulture:
                case CustomRoles.Werewolf:
                case CustomRoles.Mayor:
                case CustomRoles.PlagueBearer:
                    PlayerControl.LocalPlayer.Data.Role.IntroSound = GetIntroSound(RoleTypes.Crewmate);
                    break;

                case CustomRoles.BloodKnight:
                case CustomRoles.Ninja:
                case CustomRoles.Marksman:
                case CustomRoles.Miner:
                case CustomRoles.TheGlitch:
                case CustomRoles.Camouflager:
                case CustomRoles.Pestilence:
                case CustomRoles.Sidekick:
                case CustomRoles.Juggernaut:
                case CustomRoles.SchrodingerCat:
                    PlayerControl.LocalPlayer.Data.Role.IntroSound = GetIntroSound(RoleTypes.Impostor);
                    break;

                case CustomRoles.GM:
                    __instance.TeamTitle.text = Utils.GetRoleName(role);
                    __instance.TeamTitle.color = Utils.GetRoleColor(role);
                    __instance.BackgroundBar.material.color = Utils.GetRoleColor(role);
                    __instance.ImpostorText.gameObject.SetActive(false);
                    break;

            }

            if (Input.GetKey(KeyCode.RightShift))
            {
                __instance.TeamTitle.text = "Town Of Host: \r\nThe Other Roles";
                __instance.ImpostorText.gameObject.SetActive(true);
                __instance.ImpostorText.text = "https://github.com/music-discussion/TownOfHost-TheOtherRoles" +
                    "\r\nOut Now on Github";
                __instance.TeamTitle.color = Color.cyan;
                StartFadeIntro(__instance, Color.cyan, Color.green);
            }
            if (Input.GetKey(KeyCode.RightControl))
            {
                __instance.TeamTitle.text = "Discord Server";
                __instance.ImpostorText.gameObject.SetActive(true);
                __instance.ImpostorText.text = "https://discord.gg/v8SFfdebpz";
                __instance.TeamTitle.color = Color.magenta;
                StartFadeIntro(__instance, Color.magenta, Color.magenta);
            }
        }
        private static AudioClip GetIntroSound(RoleTypes roleType)
        {
            return RoleManager.Instance.AllRoles.Where((role) => role.Role == roleType).FirstOrDefault().IntroSound;
        }
        private static async void StartFadeIntro(IntroCutscene __instance, Color start, Color end)
        {
            await Task.Delay(1000);
            int milliseconds = 0;
            while (true)
            {
                await Task.Delay(20);
                milliseconds += 20;
                float time = (float)milliseconds / (float)500;
                Color LerpingColor = Color.Lerp(start, end, time);
                if (__instance == null || milliseconds > 500)
                {
                    Logger.Info("ループを終了します", "StartFadeIntro");
                    break;
                }
                __instance.BackgroundBar.material.color = LerpingColor;
            }
        }
    }
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
    class BeginImpostorPatch
    {
        public static bool Prefix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            if (PlayerControl.LocalPlayer.Is(CustomRoles.Sheriff) || PlayerControl.LocalPlayer.Is(CustomRoles.Investigator) || PlayerControl.LocalPlayer.Is(CustomRoles.Janitor))
            {
                //シェリフの場合はキャンセルしてBeginCrewmateに繋ぐ
                yourTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                yourTeam.Add(PlayerControl.LocalPlayer);
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (!pc.AmOwner) yourTeam.Add(pc);
                }
                __instance.BeginCrewmate(yourTeam);
                __instance.overlayHandle.color = Palette.CrewmateBlue;
                return false;
            }
            BeginCrewmatePatch.Prefix(__instance, ref yourTeam);
            return true;
        }
        public static void Postfix(IntroCutscene __instance, ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
        {
            BeginCrewmatePatch.Postfix(__instance, ref yourTeam);
        }
    }
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    class IntroCutsceneDestroyPatch
    {
        public static void Postfix(IntroCutscene __instance)
        {
            Main.introDestroyed = true;
            if (AmongUsClient.Instance.AmHost)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    pc.RpcSetRole(RoleTypes.Shapeshifter);
                    pc.RpcResetAbilityCooldown();
                }
                if (PlayerControl.LocalPlayer.Is(CustomRoles.GM)) PlayerControl.LocalPlayer.RpcExile();
            }
            Logger.Info("OnDestroy", "IntroCutscene");
        }
    }
}
