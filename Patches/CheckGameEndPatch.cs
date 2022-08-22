using HarmonyLib;
using Hazel;

namespace TownOfHost
{
    //勝利判定処理
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CheckEndCriteria))]
    class CheckGameEndPatch
    {
        public static bool Prefix(ShipStatus __instance)
        {
            if (!GameData.Instance) return false;
            if (DestroyableSingleton<TutorialManager>.InstanceExists) return true;
            var statistics = new PlayerStatistics(__instance);
            if (Options.NoGameEnd.GetBool()) return false;

            if (CheckAndEndGameForJester(__instance)) return false;
            if (CheckAndEndGameForTerrorist(__instance)) return false;
            if (CheckAndEndGameForExecutioner(__instance)) return false;
            if (CheckAndEndGameForArsonist(__instance)) return false;
            if (Main.currentWinner == CustomWinner.Default)
            {
                if (Options.CurrentGameMode == CustomGameMode.HideAndSeek)
                {
                    if (CheckAndEndGameForHideAndSeek(__instance, statistics)) return false;
                    if (CheckAndEndGameForTroll(__instance)) return false;
                    if (CheckAndEndGameForTaskWin(__instance)) return false;
                }
                else
                {
                    if (CheckAndEndGameForTaskWin(__instance)) return false;
                    if (CheckAndEndGameForSabotageWin(__instance)) return false;
                    if (CheckAndEndGameForEveryoneDied(__instance, statistics)) return false;
                    if (CheckAndEndGameForImpostorWin(__instance, statistics)) return false;
                    if (CheckAndEndGameForJackalWin(__instance, statistics)) return false;
                    if (CheckAndEndGameForVultureWin(__instance, statistics)) return false;
                    if (CheckAndEndGameForPestiWin(__instance, statistics)) return false;
                    if (CheckAndEndGameForCrewmateWin(__instance, statistics)) return false;
                    if (CheckAndEndGameForJuggyWin(__instance, statistics)) return false;
                    if (CheckAndEndGameForCovenWin(__instance, statistics)) return false;
                    if (CheckAndEndGameForWolfWin(__instance, statistics)) return false;
                    if (CheckAndEndGameForGlitchWin(__instance, statistics)) return false;
                }
            }
            return false;
        }

        private static bool CheckAndEndGameForSabotageWin(ShipStatus __instance)
        {
            if (__instance.Systems == null) return false;
            ISystemType systemType = __instance.Systems.ContainsKey(SystemTypes.LifeSupp) ? __instance.Systems[SystemTypes.LifeSupp] : null;
            if (systemType != null)
            {
                LifeSuppSystemType lifeSuppSystemType = systemType.TryCast<LifeSuppSystemType>();
                if (lifeSuppSystemType != null && lifeSuppSystemType.Countdown < 0f)
                {
                    EndGameForSabotage(__instance);
                    lifeSuppSystemType.Countdown = 10000f;
                    return true;
                }
            }
            ISystemType systemType2 = __instance.Systems.ContainsKey(SystemTypes.Reactor) ? __instance.Systems[SystemTypes.Reactor] : null;
            if (systemType2 == null)
            {
                systemType2 = __instance.Systems.ContainsKey(SystemTypes.Laboratory) ? __instance.Systems[SystemTypes.Laboratory] : null;
            }
            if (systemType2 != null)
            {
                ICriticalSabotage criticalSystem = systemType2.TryCast<ICriticalSabotage>();
                if (criticalSystem != null && criticalSystem.Countdown < 0f)
                {
                    EndGameForSabotage(__instance);
                    criticalSystem.ClearSabotage();
                    return true;
                }
            }
            return false;
        }

        private static bool CheckAndEndGameForTaskWin(ShipStatus __instance)
        {
            if (GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
            {
                __instance.enabled = false;
                ResetRoleAndEndGame(GameOverReason.HumansByTask, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForEveryoneDied(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TotalAlive <= 0)
            {
                __instance.enabled = false;
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EndGame, Hazel.SendOption.Reliable, -1);
                writer.Write((int)CustomWinner.None);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPC.EveryoneDied();
                ResetRoleAndEndGame(GameOverReason.ImpostorByKill, false);
                return true;
            }
            return false;
        }
        private static bool CheckAndEndGameForImpostorWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamImpostorsAlive >= statistics.TotalAlive - statistics.TeamImpostorsAlive &&
                statistics.TeamJackalAlive <= 0 && statistics.TeamJuggernautAlive <= 0 && statistics.TeamPestiAlive <= 0
                && statistics.TeamWolfAlive <= 0 && statistics.TeamGlitchAlive <= 0 && statistics.TeamCovenAlive <= 0)
            {
                if (Options.IsStandardHAS && statistics.TotalAlive - statistics.TeamImpostorsAlive != 0) return false;
                __instance.enabled = false;
                var endReason = TempData.LastDeathReason switch
                {
                    DeathReason.Exile => GameOverReason.ImpostorByVote,
                    DeathReason.Kill => GameOverReason.ImpostorByKill,
                    _ => GameOverReason.ImpostorByVote,
                };
                ResetRoleAndEndGame(endReason, false);
                return true;
            }
            return false;
        }
        private static bool CheckAndEndGameForJackalWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamJackalAlive >= statistics.TotalAlive - statistics.TeamJackalAlive &&
                statistics.TeamImpostorsAlive <= 0 && statistics.TeamJuggernautAlive <= 0 && statistics.TeamPestiAlive <= 0
                && statistics.TeamWolfAlive <= 0 && statistics.TeamGlitchAlive <= 0 && statistics.TeamCovenAlive <= 0)
            {
                if (Options.IsStandardHAS && statistics.TotalAlive - statistics.TeamJackalAlive != 0) return false;
                __instance.enabled = false;
                var endReason = TempData.LastDeathReason switch
                {
                    DeathReason.Exile => GameOverReason.ImpostorByVote,
                    DeathReason.Kill => GameOverReason.ImpostorByKill,
                    _ => GameOverReason.ImpostorByVote,
                };

                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EndGame, Hazel.SendOption.Reliable, -1);
                writer.Write((byte)CustomWinner.Jackal);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPC.JackalWin();

                ResetRoleAndEndGame(endReason, false);
                return true;
            }
            return false;
        }
        public static bool CheckAndEndGameForVultureWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (Main.AteBodies == Options.BodiesAmount.GetFloat())
            {
                //Vulture wins.
                __instance.enabled = false;
                var endReason = TempData.LastDeathReason switch
                {
                    DeathReason.Exile => GameOverReason.ImpostorByVote,
                    DeathReason.Kill => GameOverReason.ImpostorByKill,
                    _ => GameOverReason.ImpostorByVote,
                };

                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EndGame, Hazel.SendOption.Reliable, -1);
                writer.Write((byte)CustomWinner.Vulture);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPC.VultureWin();

                ResetRoleAndEndGame(endReason, false);
                return true;
            }
            return false;
        }
        private static bool CheckAndEndGameForPestiWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamPestiAlive >= statistics.TotalAlive - statistics.TeamPestiAlive &&
                statistics.TeamImpostorsAlive <= 0 && statistics.TeamJuggernautAlive <= 0 && statistics.TeamCovenAlive <= 0
                && statistics.TeamWolfAlive <= 0 && statistics.TeamGlitchAlive <= 0)
            {
                if (Options.IsStandardHAS && statistics.TotalAlive - statistics.TeamPestiAlive != 0) return false;
                __instance.enabled = false;
                var endReason = TempData.LastDeathReason switch
                {
                    DeathReason.Exile => GameOverReason.ImpostorByVote,
                    DeathReason.Kill => GameOverReason.ImpostorByKill,
                    _ => GameOverReason.ImpostorByVote,
                };

                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EndGame, Hazel.SendOption.Reliable, -1);
                writer.Write((byte)CustomWinner.Pestilence);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPC.PestiWin();

                ResetRoleAndEndGame(endReason, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForCrewmateWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamImpostorsAlive == 0 && statistics.TeamJackalAlive == 0 && statistics.TeamJuggernautAlive == 0 && statistics.TeamCovenAlive == 0 && statistics.TeamPestiAlive == 0 && statistics.TeamGlitchAlive == 0 && statistics.TeamWolfAlive == 0)
            {
                __instance.enabled = false;
                ResetRoleAndEndGame(GameOverReason.HumansByVote, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForJuggyWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamJuggernautAlive >= statistics.TotalAlive - statistics.TeamJuggernautAlive &&
                statistics.TeamImpostorsAlive <= 0 && statistics.TeamPestiAlive <= 0 && statistics.TeamCovenAlive <= 0
                && statistics.TeamWolfAlive <= 0 && statistics.TeamGlitchAlive <= 0)
            {
                if (Options.IsStandardHAS && statistics.TotalAlive - statistics.TeamJuggernautAlive != 0) return false;
                __instance.enabled = false;
                var endReason = TempData.LastDeathReason switch
                {
                    DeathReason.Exile => GameOverReason.ImpostorByVote,
                    DeathReason.Kill => GameOverReason.ImpostorByKill,
                    _ => GameOverReason.ImpostorByVote,
                };

                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EndGame, Hazel.SendOption.Reliable, -1);
                writer.Write((byte)CustomWinner.Juggernaut);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPC.JugWin();

                ResetRoleAndEndGame(endReason, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForCovenWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamCovenAlive >= statistics.TotalAlive - statistics.TeamCovenAlive &&
                statistics.TeamImpostorsAlive <= 0 && statistics.TeamPestiAlive <= 0 && statistics.TeamJuggernautAlive <= 0
                && statistics.TeamWolfAlive <= 0 && statistics.TeamGlitchAlive <= 0)
            {
                if (Options.IsStandardHAS && statistics.TotalAlive - statistics.TeamCovenAlive != 0) return false;
                __instance.enabled = false;
                var endReason = TempData.LastDeathReason switch
                {
                    DeathReason.Exile => GameOverReason.ImpostorByVote,
                    DeathReason.Kill => GameOverReason.ImpostorByKill,
                    _ => GameOverReason.ImpostorByVote,
                };

                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EndGame, Hazel.SendOption.Reliable, -1);
                writer.Write((byte)CustomWinner.Juggernaut);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPC.CovenWin();

                ResetRoleAndEndGame(endReason, false);
                return true;
            }
            return false;
        }
        private static bool CheckAndEndGameForWolfWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamWolfAlive >= statistics.TotalAlive - statistics.TeamWolfAlive &&
                statistics.TeamImpostorsAlive <= 0 && statistics.TeamJuggernautAlive <= 0 && statistics.TeamPestiAlive <= 0
                && statistics.TeamGlitchAlive <= 0 && statistics.TeamCovenAlive <= 0)
            {
                if (Options.IsStandardHAS && statistics.TotalAlive - statistics.TeamWolfAlive != 0) return false;
                __instance.enabled = false;
                var endReason = TempData.LastDeathReason switch
                {
                    DeathReason.Exile => GameOverReason.ImpostorByVote,
                    DeathReason.Kill => GameOverReason.ImpostorByKill,
                    _ => GameOverReason.ImpostorByVote,
                };

                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EndGame, Hazel.SendOption.Reliable, -1);
                writer.Write((byte)CustomWinner.Werewolf);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPC.WolfWin();

                ResetRoleAndEndGame(endReason, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForGlitchWin(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TeamGlitchAlive >= statistics.TotalAlive - statistics.TeamJackalAlive &&
                statistics.TeamImpostorsAlive <= 0 && statistics.TeamJuggernautAlive <= 0 && statistics.TeamPestiAlive <= 0
                && statistics.TeamWolfAlive <= 0 && statistics.TeamCovenAlive <= 0)
            {
                if (Options.IsStandardHAS && statistics.TotalAlive - statistics.TeamGlitchAlive != 0) return false;
                __instance.enabled = false;
                var endReason = TempData.LastDeathReason switch
                {
                    DeathReason.Exile => GameOverReason.ImpostorByVote,
                    DeathReason.Kill => GameOverReason.ImpostorByKill,
                    _ => GameOverReason.ImpostorByVote,
                };

                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EndGame, Hazel.SendOption.Reliable, -1);
                writer.Write((byte)CustomWinner.TheGlitch);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPC.JackalWin();

                ResetRoleAndEndGame(endReason, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForHideAndSeek(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (statistics.TotalAlive - statistics.TeamImpostorsAlive == 0)
            {
                __instance.enabled = false;
                ResetRoleAndEndGame(GameOverReason.ImpostorByKill, false);
                return true;
            }
            return false;
        }

        private static bool CheckAndEndGameForTroll(ShipStatus __instance)
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                var hasRole = Main.AllPlayerCustomRoles.TryGetValue(pc.PlayerId, out var role);
                if (!hasRole) return false;
                if (role == CustomRoles.HASTroll && pc.Data.IsDead)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.EndGame, Hazel.SendOption.Reliable, -1);
                    writer.Write((byte)CustomWinner.HASTroll);
                    writer.Write(pc.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPC.TrollWin(pc.PlayerId);
                    __instance.enabled = false;
                    ResetRoleAndEndGame(GameOverReason.ImpostorByKill, false);
                    return true;
                }
            }
            return false;
        }

        private static bool CheckAndEndGameForJester(ShipStatus __instance)
        {
            if (Main.currentWinner == CustomWinner.Jester && Main.CustomWinTrigger)
            {
                __instance.enabled = false;
                ResetRoleAndEndGame(GameOverReason.ImpostorByKill, false);
                return true;
            }
            return false;
        }
        private static bool CheckAndEndGameForTerrorist(ShipStatus __instance)
        {
            if (Main.currentWinner == CustomWinner.Terrorist && Main.CustomWinTrigger)
            {
                __instance.enabled = false;
                ResetRoleAndEndGame(GameOverReason.ImpostorByKill, false);
                return true;
            }
            return false;
        }
        private static bool CheckAndEndGameForExecutioner(ShipStatus __instance)
        {
            if (Main.currentWinner == CustomWinner.Executioner && Main.CustomWinTrigger)
            {
                __instance.enabled = false;
                ResetRoleAndEndGame(GameOverReason.ImpostorByKill, false);
                return true;
            }
            return false;
        }
        private static bool CheckAndEndGameForArsonist(ShipStatus __instance)
        {
            if (Main.currentWinner == CustomWinner.Arsonist && Main.CustomWinTrigger)
            {
                __instance.enabled = false;
                ResetRoleAndEndGame(GameOverReason.ImpostorByKill, false);
                return true;
            }
            return false;
        }


        private static void EndGameForSabotage(ShipStatus __instance)
        {
            __instance.enabled = false;
            ResetRoleAndEndGame(GameOverReason.ImpostorBySabotage, false);
            return;
        }
        private static void ResetRoleAndEndGame(GameOverReason reason, bool showAd)
        {
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                var LoseImpostorRole = Main.AliveImpostorCount == 0 ? pc.Is(RoleType.Impostor) : pc.Is(CustomRoles.Egoist);
                if (pc.Is(CustomRoles.Sheriff) ||
                    (!(Main.currentWinner == CustomWinner.Arsonist) && pc.Is(CustomRoles.Arsonist)) || (Main.currentWinner != CustomWinner.Vulture && pc.Is(CustomRoles.Vulture)) ||
                    (Main.currentWinner != CustomWinner.Jackal && pc.Is(CustomRoles.Jackal)) || (Main.currentWinner != CustomWinner.Pestilence && pc.Is(CustomRoles.Pestilence)) ||
                    LoseImpostorRole || (Main.currentWinner != CustomWinner.Werewolf && pc.Is(CustomRoles.Werewolf)) || (Main.currentWinner != CustomWinner.TheGlitch && pc.Is(CustomRoles.TheGlitch)))
                {
                    pc.RpcSetRole(RoleTypes.GuardianAngel);
                }
            }
            new LateTask(() =>
            {
                ShipStatus.RpcEndGame(reason, showAd);
            }, 0.5f, "EndGameTask");
        }
        //プレイヤー統計
        internal class PlayerStatistics
        {
            public int TeamImpostorsAlive { get; set; }
            public int TotalAlive { get; set; }
            public int TeamJackalAlive { get; set; }
            public int TeamPestiAlive { get; set; }
            public int TeamJuggernautAlive { get; set; }
            public int TeamCovenAlive { get; set; }
            public int TeamWolfAlive { get; set; }
            public int TeamGlitchAlive { get; set; }
            //public int TeamJuggernautAlive { get; set; }

            public PlayerStatistics(ShipStatus __instance)
            {
                GetPlayerCounts();
            }

            private void GetPlayerCounts()
            {
                int numImpostorsAlive = 0;
                int numTotalAlive = 0;
                int numJackalsAlive = 0;
                int numCovenAlive = 0;
                int numPestiAlive = 0;
                int numJugAlive = 0;
                int numGlitchAlive = 0;
                int numWolfAlive = 0;

                for (int i = 0; i < GameData.Instance.PlayerCount; i++)
                {
                    GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
                    var hasHideAndSeekRole = Main.AllPlayerCustomRoles.TryGetValue((byte)i, out var role);
                    if (!playerInfo.Disconnected)
                    {
                        if (!playerInfo.IsDead)
                        {
                            if (Options.CurrentGameMode != CustomGameMode.HideAndSeek || !hasHideAndSeekRole)
                            {
                                numTotalAlive++;//HideAndSeek以外
                            }
                            else
                            {
                                //HideAndSeek中
                                if (role is not CustomRoles.HASFox and not CustomRoles.HASTroll) numTotalAlive++;
                            }

                            //PlayerControl player = Utils.GetPlayerById(playerInfo.PlayerId);

                            if (playerInfo.Role.TeamType == RoleTeamTypes.Impostor &&
                            (playerInfo.GetCustomRole() != CustomRoles.Sheriff || playerInfo.GetCustomRole() != CustomRoles.Arsonist))
                            {
                                numImpostorsAlive++;
                            }
                            else if (CustomRolesHelper.IsCoven(playerInfo.GetCustomRole()))
                            {
                                numCovenAlive++;
                            }
                            else if (playerInfo.GetCustomRole() == CustomRoles.Jackal) numJackalsAlive++;
                            else if (playerInfo.GetCustomRole() == CustomRoles.PlagueBearer) numPestiAlive++;
                            else if (playerInfo.GetCustomRole() == CustomRoles.Pestilence) numPestiAlive++;
                            else if (playerInfo.GetCustomRole() == CustomRoles.Juggernaut) numJugAlive++;
                            else if (playerInfo.GetCustomRole() == CustomRoles.Werewolf) numWolfAlive++;
                            else if (playerInfo.GetCustomRole() == CustomRoles.TheGlitch) numGlitchAlive++;
                        }
                    }
                }

                TeamImpostorsAlive = numImpostorsAlive;
                TotalAlive = numTotalAlive;
                TeamJackalAlive = numJackalsAlive;
                TeamCovenAlive = numCovenAlive;
                TeamPestiAlive = numPestiAlive;
                TeamJuggernautAlive = numJugAlive;
                TeamGlitchAlive = numGlitchAlive;
                TeamWolfAlive = numWolfAlive;
            }
        }
    }
}