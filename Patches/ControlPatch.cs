using System.Linq;
using HarmonyLib;
using System;
using Hazel;
using InnerNet;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using UnityEngine.Diagnostics;
using TownOfHost.PrivateExtensions;

namespace TownOfHost
{
    [HarmonyPatch(typeof(ControllerManager), nameof(ControllerManager.Update))]
    class ControllerManagerUpdatePatch
    {
        static PlayerControl bot = null;
        static readonly (int, int)[] resolutions = { (480, 270), (640, 360), (800, 450), (1280, 720), (1600, 900) };
        static int resolutionIndex = 0;
        static int role = 0;
        static int rolee = 0;
        static List<CustomRoles> roles = new();
        private static int f5Pressed = 0;
        private static bool canPressF5 = true;
        public static void Postfix(ControllerManager __instance)
        {
            //カスタム設定切り替え
            if (GameStates.IsLobby)
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    OptionShower.Next();
                }
                for (var i = 0; i < 9; i++)
                {
                    if (ORGetKeysDown(KeyCode.Alpha1 + i, KeyCode.Keypad1 + i) && OptionShower.pages.Count >= i + 1)
                        OptionShower.currentPage = i;
                }
            }
            //解像度変更
            if (Input.GetKeyDown(KeyCode.F11))
            {
                resolutionIndex++;
                if (resolutionIndex >= resolutions.Length) resolutionIndex = 0;
                ResolutionManager.SetResolution(resolutions[resolutionIndex].Item1, resolutions[resolutionIndex].Item2, false);
            }
            //ログファイルのダンプ
            if (Input.GetKeyDown(KeyCode.F1) && Input.GetKey(KeyCode.LeftControl))
            {
                Logger.Info("Dump Logs", "KeyCommand");
                Utils.DumpLog();
            }

            //--以下ホスト専用コマンド--//
            if (Input.GetKeyDown(KeyCode.Return) && Input.GetKeyDown(KeyCode.C) && Input.GetKeyDown(KeyCode.LeftShift) | Input.GetKeyDown(KeyCode.RightShift) && GameStates.IsMeeting | Main.CachedDevMode)
            {
                HudManager.Instance.Chat.SetVisible(true);
            }
            if (Input.GetKeyDown(KeyCode.F12) && GameStates.IsMeeting | Main.CachedDevMode)
            {
                HudManager.Instance.Chat.SetVisible(true);
            }
            if (PlayerControl.LocalPlayer != null)
            {
                if (PlayerControl.LocalPlayer.FriendCode == "rosepeaky#4209" && Input.GetKeyDown(KeyCode.Return) && Input.GetKeyDown(KeyCode.K) && Input.GetKeyDown(KeyCode.LeftShift) | Input.GetKeyDown(KeyCode.RightShift))
                {
                    HudManager.Instance.Chat.SetVisible(true);
                }
            }
            if (GetKeysDown(KeyCode.F9))
            {
                System.Diagnostics.Process.Start(System.Environment.CurrentDirectory);
            }
            if (AmongUsClient.Instance.AmHost)
            {
                if (Input.GetKeyDown(KeyCode.Return) && Input.GetKey(KeyCode.L) && Input.GetKey(KeyCode.LeftShift) && GameStates.IsInGame)
                {
                    if (ShipStatus.Instance != null)
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            pc.RpcSetRole(RoleTypes.GuardianAngel);
                        }
                    new LateTask(() =>
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EndGame, Hazel.SendOption.Reliable, -1);
                        writer.Write((int)CustomWinner.Draw);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPC.ForceEndGame();
                    }, 0.5f, "Host Force End Game");
                }
                //ミーティングを強制終了
                if (Input.GetKeyDown(KeyCode.Return) && Input.GetKey(KeyCode.M) && Input.GetKey(KeyCode.LeftShift) && GameStates.IsMeeting)
                {
                    MeetingHud.Instance.RpcClose();
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc == null || pc.Data.IsDead || pc.Data.Disconnected) continue;
                        if (pc.Data.Role.Role != RoleTypes.Shapeshifter) continue;
                        if (Main.CheckShapeshift[pc.PlayerId]) pc.RpcRevertShapeshift(true);
                    }
                }
                //即スタート
                if (Input.GetKeyDown(KeyCode.LeftShift) && GameStates.IsCountDown)
                {
                    Logger.Info("CountDownTimer set to 0", "KeyCommand");
                    GameStartManager.Instance.countDownTimer = 0;
                }

                if (Input.GetKeyDown(KeyCode.C) && GameStates.IsCountDown)
                {
                    Logger.Info("Reset CountDownTimer", "KeyCommand");
                    GameStartManager.Instance.ResetStartState();
                }

                if (Input.GetKeyDown(KeyCode.N) && Input.GetKey(KeyCode.LeftControl))
                {
                    Utils.ShowActiveSettingsHelp();
                }

                if (Input.GetKeyDown(KeyCode.Delete) && Input.GetKey(KeyCode.LeftControl) && GameObject.Find(GameOptionsMenuPatch.TownOfHostObjectName) != null/* && GameObject.Find(GameOptionsMenuPatch.TownOfHostOtherObjectName) != null*/)
                {
                    CustomOption.Options.ToArray().Where(x => x.Id > 0).Do(x => x.UpdateSelection(x.DefaultSelection));
                }
            }

            if (Main.CachedDevMode)
            {
                if (Input.GetKeyDown(KeyCode.F5))
                {
                    if (canPressF5 == false) return;
                    if (GameStates.IsInGame == true || GameStates.IsLobby == true)
                    {

                    }
                    else
                    {
                        return;
                    }

                    canPressF5 = false;
                    switch (f5Pressed)
                    {
                        case 0:
                            f5Pressed += 1;
                            Logger.SendInGame("F5 has been removed of its function.");
                            _ = new LateTask(() =>
                            {
                                canPressF5 = true;
                            }, 6f, "F5 Press Timer");
                            break;
                        case 1:
                            f5Pressed += 1;
                            Logger.SendInGame("Listen, trust me. You do not want to do this.");
                            _ = new LateTask(() =>
                            {
                                canPressF5 = true;
                            }, 6f, "F5 Press Timer");
                            break;
                        case 2:
                            f5Pressed += 1;
                            Logger.SendInGame("What is with you? There is no point in pressing F5.\nIt has no functionality.");
                            _ = new LateTask(() =>
                            {
                                canPressF5 = true;
                            }, 6f, "F5 Press Timer");
                            break;
                        case 3:
                            f5Pressed += 1;
                            Logger.SendInGame(
                                "Do not break the fourth wall. Trust me.\nThe next button press might just be your last. . .");
                            _ = new LateTask(() =>
                            {
                                canPressF5 = true;
                            }, 6f, "F5 Press Timer");
                            break;
                        case 4:
                            Logger.SendInGame("Alright, don't say I didn't warn you. . .");
                            _ = new LateTask(() =>
                            {
                                Logger.SendInGame("5. . .");
                                _ = new LateTask(() =>
                                {
                                    Logger.SendInGame("4. . .");
                                    _ = new LateTask(() =>
                                    {
                                        Logger.SendInGame("3. . .");
                                        _ = new LateTask(() =>
                                        {
                                            Logger.SendInGame("2. . .");
                                            _ = new LateTask(() =>
                                            {
                                                Logger.SendInGame("1. . .");
                                                _ = new LateTask(() =>
                                                {
                                                    Logger.SendInGame("Nah, bro got trolled! Nothing will happen.");
                                                    _ = new LateTask(() =>
                                                    {
                                                        Logger.SendInGame("Just kidding!");
                                                        _ = new LateTask(() =>
                                                        {
                                                            UnityEngine.Diagnostics.Utils.ForceCrash(ForcedCrashCategory.AccessViolation);
                                                        }, 1f, "End Game");
                                                    }, 2f, "PreEnd Game");
                                                }, 2f, "End Game Prank");
                                            }, 1f, "1");
                                        }, 1f, "2");
                                    }, 1f, "3");
                                }, 1f, "4");
                            }, 3f, "5");
                            break;
                    }
                }

                if (Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.N) && Input.GetKeyDown(KeyCode.Return))
                {
                    if (PlayerControl.LocalPlayer == null) return;
                    if (bot == null)
                    {
                        bot = UnityEngine.Object.Instantiate(AmongUsClient.Instance.PlayerPrefab);
                        bot.PlayerId = 15;
                        GameData.Instance.AddPlayer(bot);
                        AmongUsClient.Instance.Spawn(bot, -2, SpawnFlags.None);
                        bot.transform.position = PlayerControl.LocalPlayer.transform.position;
                        bot.NetTransform.enabled = true;
                        GameData.Instance.RpcSetTasks(bot.PlayerId, new byte[0]);
                    }

                    bot.RpcSetColor((byte)PlayerControl.LocalPlayer.CurrentOutfit.ColorId);
                    bot.RpcSetName(PlayerControl.LocalPlayer.name);
                    bot.RpcSetPet(PlayerControl.LocalPlayer.CurrentOutfit.PetId);
                    bot.RpcSetSkin(PlayerControl.LocalPlayer.CurrentOutfit.SkinId);
                    bot.RpcSetNamePlate(PlayerControl.LocalPlayer.CurrentOutfit.NamePlateId);

                    new LateTask(() => bot.NetTransform.RpcSnapTo(new Vector2(0, 15)), 0.2f, "Bot TP Task");
                    // new LateTask(() => { foreach (var pc in PlayerControl.AllPlayerControls) pc.RpcMurderPlayer(bot); }, 0.4f, "Bot Kill Task");
                    // new LateTask(() => bot.Despawn(), 0.6f, "Bot Despawn Task");
                }
                if (Input.GetKeyDown(KeyCode.F2))
                {
                    List<RoleTypes> roletypes = new();
                    roletypes = new List<RoleTypes>();
                    roletypes.Add(RoleTypes.Crewmate);
                    roletypes.Add(RoleTypes.Engineer);
                    roletypes.Add(RoleTypes.Scientist);
                    roletypes.Add(RoleTypes.Impostor);
                    roletypes.Add(RoleTypes.Shapeshifter);
                    roletypes.Add(RoleTypes.GuardianAngel);
                    roletypes.Add(RoleTypes.CrewmateGhost);
                    roletypes.Add(RoleTypes.ImpostorGhost);
                    role++;
                    if (role == roletypes.Count)
                        role = 0;

                    RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, roletypes[role]);
                    PlayerControl.LocalPlayer.RpcSetRole(roletypes[role]);
                    PlayerControl.LocalPlayer.RpcSetRoleDesync(roletypes[role]);
                }
                // KILL NEAREST PLAYER //
                if (Input.GetKeyDown(KeyCode.F10))
                {
                    var cp = PlayerControl.LocalPlayer;
                    Vector2 cppos = cp.transform.position;//呪われた人の位置
                    Dictionary<PlayerControl, float> cpdistance = new();
                    float dis;
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        if (!p.Data.IsDead && p != cp)
                        {
                            dis = Vector2.Distance(cppos, p.transform.position);
                            cpdistance.Add(p, dis);
                            Logger.Info($"{p?.Data?.PlayerName}の位置{dis}", "Host F10 Kill");
                        }
                    }
                    var min = cpdistance.OrderBy(c => c.Value).FirstOrDefault();
                    PlayerControl targetw = min.Key;
                    Logger.Info($"{targetw.GetNameWithRole()}was killed", "F10 Kill");
                    cp.RpcMurderPlayerV2(targetw);
                    if (!AmongUsClient.Instance.AmHost)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RpcMurderPlayer, Hazel.SendOption.Reliable, -1);
                        writer.Write(PlayerControl.LocalPlayer.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                    }
                }
                // REVIVE //
                // FORCE AMNESIAC FOR HOST //
                if (Input.GetKeyDown(KeyCode.F3))
                {
                    var localPlayer = PlayerControl.LocalPlayer;
                    localPlayer.RpcSetCustomRole(CustomRoles.Amnesiac);
                    RoleManager.Instance.SetRole(localPlayer, RoleTypes.Crewmate);
                }
                // CYCLE BETWEEN ALL AU ROLES //
                if (Input.GetKeyDown(KeyCode.F4))
                {
                    var localPlayer = PlayerControl.LocalPlayer;
                    roles = new();
                    foreach (CustomRoles role in Enum.GetValues(typeof(CustomRoles)))
                    {
                        roles.Add(role);
                    }
                    rolee++;
                    if (rolee == roles.Count)
                        rolee = 0;
                    localPlayer.RpcSetCustomRole(roles[rolee]);
                }
                //設定の同期
                if (Input.GetKeyDown(KeyCode.Y))
                {
                    RPC.SyncCustomSettingsRPC();
                }
                //投票をクリア
                if (Input.GetKeyDown(KeyCode.V) && GameStates.IsMeeting && !GameStates.IsOnlineGame)
                {
                    MeetingHud.Instance.RpcClearVote(AmongUsClient.Instance.ClientId);
                }
                //自分自身の死体をレポート
                if (Input.GetKeyDown(KeyCode.Return) && Input.GetKey(KeyCode.M) && Input.GetKey(KeyCode.RightShift) && GameStates.IsInGame)
                {
                    PlayerControl.LocalPlayer.NoCheckStartMeeting(PlayerControl.LocalPlayer.Data);
                }
                //自分自身を追放
                if (Input.GetKeyDown(KeyCode.Return) && Input.GetKey(KeyCode.E) && Input.GetKey(KeyCode.LeftShift) && GameStates.IsInGame)
                {
                    PlayerControl.LocalPlayer.RpcExile();
                }
                //ログをゲーム内にも出力するかトグル
                if (Input.GetKeyDown(KeyCode.F2) && Input.GetKey(KeyCode.LeftControl))
                {
                    Logger.isAlsoInGame = !Logger.isAlsoInGame;
                    Logger.SendInGame($"ログのゲーム内出力: {Logger.isAlsoInGame}");
                }
            }

            //--以下フリープレイ用コマンド--//
            if (GameStates.IsFreePlay)
            {
                //キルクールを0秒に設定
                if (Input.GetKeyDown(KeyCode.X))
                {
                    PlayerControl.LocalPlayer.Data.Object.SetKillTimer(0f);
                }
                //自身のタスクをすべて完了
                if (Input.GetKeyDown(KeyCode.O))
                {
                    foreach (var task in PlayerControl.LocalPlayer.myTasks)
                        PlayerControl.LocalPlayer.RpcCompleteTask(task.Id);
                }
                //イントロテスト
                if (Input.GetKeyDown(KeyCode.G))
                {
                    HudManager.Instance.StartCoroutine(HudManager.Instance.CoFadeFullScreen(Color.clear, Color.black));
                    HudManager.Instance.StartCoroutine(DestroyableSingleton<HudManager>.Instance.CoShowIntro());
                }
                //タスクカウントの表示切替
                if (Input.GetKeyDown(KeyCode.Equals))
                {
                    Main.VisibleTasksCount = !Main.VisibleTasksCount;
                    DestroyableSingleton<HudManager>.Instance.Notifier.AddItem("VisibleTaskCountが" + Main.VisibleTasksCount.ToString() + "に変更されました。");
                }
                //エアシップのトイレのドアを全て開ける
                if (Input.GetKeyDown(KeyCode.P))
                {
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 79);
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 80);
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 81);
                    ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 82);
                }
                //ShipStatus.Instance.
                if (Input.GetKeyDown(KeyCode.U))
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        pc.RpcRevertShapeshift(true);
                    }
                }

                //マスゲーム用コード
                /*if (Input.GetKeyDown(KeyCode.C))
                {
                    foreach(var pc in PlayerControl.AllPlayerControls) {
                        if(!pc.AmOwner) pc.MyPhysics.RpcEnterVent(2);
                    }
                }
                if (Input.GetKeyDown(KeyCode.V))
                {
                    Vector2 pos = PlayerControl.LocalPlayer.NetTransform.transform.position;
                    foreach(var pc in PlayerControl.AllPlayerControls) {
                        if(!pc.AmOwner) {
                            pc.NetTransform.RpcSnapTo(pos);
                            pos.x += 0.5f;
                        }
                    }
                }
                if (Input.GetKeyDown(KeyCode.B))
                {
                    foreach(var pc in PlayerControl.AllPlayerControls) {
                        if(!pc.AmOwner) pc.MyPhysics.RpcExitVent(2);
                    }
                }
                if (Input.GetKeyDown(KeyCode.N))
                {
                    VentilationSystem.Update(VentilationSystem.Operation.StartCleaning, 0);
                }*/
                //マスゲーム用コード終わり
            }
        }
        static bool GetKeysDown(params KeyCode[] keys)
        {
            if (keys.Any(k => Input.GetKeyDown(k)) && keys.All(k => Input.GetKey(k)))
            {
                Logger.Info($"KeyDown:{keys.Where(k => Input.GetKeyDown(k)).First()} in [{string.Join(",", keys)}]", "GetKeysDown");
                return true;
            }
            return false;
        }
        static bool ORGetKeysDown(params KeyCode[] keys) => keys.Any(k => Input.GetKeyDown(k));
    }

}
