using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using InnerNet;
using UnhollowerBaseLib.Runtime.VersionSpecific.FieldInfo;

namespace TownOfHost
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
    class OnGameJoinedPatch
    {
        public static void Postfix(AmongUsClient __instance)
        {
            Logger.Info($"{__instance.GameId}に参加", "OnGameJoined");
            Main.playerVersion = new Dictionary<byte, PlayerVersion>();
            Main.devNames = new Dictionary<byte, string>();
            RPC.RpcVersionCheck();

            NameColorManager.Begin();
            Options.Load();
            if (AmongUsClient.Instance.AmHost) //以下、ホストのみ実行
            {
                if (PlayerControl.GameOptions.killCooldown == 0.1f)
                    PlayerControl.GameOptions.killCooldown = Main.LastKillCooldown.Value;
                new LateTask(() =>
                {
                    string rname = PlayerControl.LocalPlayer.Data.PlayerName;
                    string fontSize = "1.5";
                    string dev = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.TheGlitch), "Dev")}</size>";
                    string name = dev + "\r\n" + rname;
                    if (PlayerControl.LocalPlayer.Data.FriendCode is "nullrelish#9615" or "tillhoppy#6167" or "gnuedaphic#7196" or "pingrating#9371")
                    {
                        PlayerControl.LocalPlayer.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.TheGlitch), name)}");
                        Main.devNames.Add(PlayerControl.LocalPlayer.Data.PlayerId, rname);
                    }
                }, 3f, "Name Check for Host");
            }
        }
    }
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
    class OnPlayerJoinedPatch
    {
        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
        {
            Logger.Info($"{client.PlayerName}(ClientID:{client.Id}) (FreindCode:{client.FriendCode}) joined the game.", "Session");
            if (DestroyableSingleton<FriendsListManager>.Instance.IsPlayerBlockedUsername(client.FriendCode) && AmongUsClient.Instance.AmHost)
            {
                AmongUsClient.Instance.KickPlayer(client.Id, true);
                Logger.Info($"This is a blocked player. {client?.PlayerName}({client.FriendCode}) was banned.", "BAN");
            }
            Main.playerVersion = new Dictionary<byte, PlayerVersion>();
            RPC.RpcVersionCheck();
            if (AmongUsClient.Instance.AmHost)
            {
                new LateTask(() =>
                {
                    if (client.Character != null) ChatCommands.SendTemplate("welcome", client.Character.PlayerId, true);
                    string fontSize2 = "0.7";
                    string fontSize3 = "0.5";
                    string fontSize4 = "0.75";
                    string fontSize1 = "0.8";
                    string fontSize = "1.5";
                    string fontSize5 = "1";
                    string rname = client.Character.Data.PlayerName;
                    string dev = $"<size={fontSize2}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.TheGlitch), "Developer")}</size>";
                    string dscfr = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.GM), "ToH:ToR Discord Member")}</size>"; //GENERAL TITLE
                    string sns = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.pinkcolor), "♥")}</size>"; //HEART
                    //LILY TITLE
                    string lily = $"<size={fontSize4}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Amnesiac), "Aviator")}</size>";
                    //SILENCE TITLE
                    string slnc = $"<size={fontSize4}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Amnesiac), "Icyyy")}</size>";
                    string snf = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Amnesiac), "米")}</size>";
                    //AUGUST TITLE
                    string aug1 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.aug1), "♡")}</size>";
                    string augc = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.aug2), "C")}</size>";
                    string augu = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.aug3), "u")}</size>";
                    string augt = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.aug4), "t")}</size>";
                    string augi = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.aug5), "i")}</size>";
                    string auge = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.aug6), "e")}</size>";
                    string aug2 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.aug7), "♡")}</size>";
                    //ROSE TITLE START
                    string sns1 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns1), "♡")}</size>";
                    string sns2 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns2), "H")}</size>";
                    string sns3 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns3), "o")}</size>";
                    string sns4 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns4), "t")}</size>";
                    string sns5 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns5), "t")}</size>";
                    string sns6 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns6), "i")}</size>";
                    string sns7 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns7), "e")}</size>";
                    string sns8 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns8), "♡")}</size>";
                    //ROSE NAME START
                    string sns91 = $"<size={fontSize5}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns1), "♡")}</size>";
                    string sns9 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns1), "shi")}</size>";
                    string sns0 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns3), "ft")}</size>";
                    string sns01 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns5), "yr")}</size>";
                    string sns02 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns6), "os")}</size>";
                    string sns03 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns6), "e")}</size>";
                    string sns92 = $"<size={fontSize5}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.sns7), "♡")}</size>";
                    //PUSHIN P TITLE START
                    string p1 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh1), "♡")}</size>";
                    string p2 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh2), "S")}</size>";
                    string p3 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh3), "w")}</size>";
                    string p4 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh4), "e")}</size>";
                    string p5 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh5), "e")}</size>";
                    string p6 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh6), "t")}</size>";
                    string p7 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh7), "i")}</size>";
                    string p8 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh8), "e")}</size>";
                    string p9 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh9), "♡")}</size>";
                    //PUSHIN P NAME START
                    string ps1 = $"<size={fontSize5}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh1), "♡")}</size>";
                    string ps2 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh1), "p")}</size>";
                    string ps3 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh3), "us")}</size>";
                    string ps4 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh5), "hi")}</size>";
                    string ps5 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh7), "nP")}</size>";
                    string ps6 = $"<size={fontSize5}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh9), "♡")}</size>";
                    //FEEFEE TITLE START
                    string fee1 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh9), "♡")}</size>";
                    string fee2 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh8), "Kr")}</size>";
                    string fee3 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh7), "am")}</size>";
                    string fee4 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh6), "ps")}</size>";
                    string fee5 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh5), "B")}</size>";
                    string fee6 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh4), "es")}</size>";
                    string fee7 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh3), "ti")}</size>";
                    string fee8 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh2), "e")}</size>";
                    string fee9 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh1), "♡")}</size>";
                    //FEEFEE NAME START
                    string fe1 = $"<size={fontSize5}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.aug1), "♡")}</size>";
                    string fe2 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.aug2), "F")}</size>";
                    string fe3 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.aug2), "e")}</size>";
                    string fe4 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.aug3), "e")}</size>";
                    string fe5 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.aug3), "F")}</size>";
                    string fe6 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.aug4), "e")}</size>";
                    string fe7 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.aug5), "e")}</size>";
                    string fe8 = $"<size={fontSize5}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.aug6), "♡")}</size>";
                    //JESSI TITLE START
                    string js0 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.jss1), "♡")}</size>";
                    string js1 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.jss1), "R")}</size>";
                    string js2 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.jss2), "e")}</size>";
                    string js3 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.jss3), "d")}</size>";
                    string js4 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.jss4), " ")}</size>";
                    string js5 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.jss5), "R")}</size>";
                    string js6 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.jss6), "i")}</size>";
                    string js7 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.jss7), "s")}</size>";
                    string js8 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.jss7), "i")}</size>";
                    string js9 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.jss8), "n")}</size>";
                    string js10 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.jss9), "g")}</size>";
                    string js11 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.jss9), "♡")}</size>";
                    //CANDY TITLE START
                    string cn0 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cnd9), "♡")}</size>";
                    string cn1 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cnd9), "A")}</size>";
                    string cn2 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cnd8), "l")}</size>";
                    string cn3 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cnd7), "w")}</size>";
                    string cn4 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cnd6), "a")}</size>";
                    string cn5 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cnd5), "y")}</size>";
                    string cn6 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cnd4), "s")}</size>";
                    string cn7 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cnd4), " ")}</size>";
                    string cn8 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cnd3), "S")}</size>";
                    string cn9 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cnd2), "u")}</size>";
                    string cn10 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cnd1), "S")}</size>";
                    string cn11 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cnd1), "♡")}</size>";
                    //MITSKI NAME START
                    string cnd1 = $"<size={fontSize5}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cnd1), "♡")}</size>";
                    string cnd2 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cnd1), "m")}</size>";
                    string cnd3 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cnd3), "i")}</size>";
                    string cnd4 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cnd3), "t")}</size>";
                    string cnd5 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cnd5), "s")}</size>";
                    string cnd6 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cnd7), "k")}</size>";
                    string cnd7 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cnd7), "i")}</size>";
                    string cnd8 = $"<size={fontSize5}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.cnd9), "♡")}</size>";
                    //CANDY TITLE START
                    string kr0 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh1), "♡")}</size>";
                    string kr1 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh1), "T")}</size>";
                    string kr2 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh2), "r")}</size>";
                    string kr3 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh3), "u")}</size>";
                    string kr4 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh4), "s")}</size>";
                    string kr5 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh5), "t")}</size>";
                    string kr6 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh6), " ")}</size>";
                    string kr7 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh7), "H")}</size>";
                    string kr8 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh7), "o")}</size>";
                    string kr9 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh8), "s")}</size>";
                    string kr10 = $"<size={fontSize1}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh9), "t")}</size>";
                    string kr11 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh9), "♡")}</size>";
                    //CANDY NAME START
                    string krz1 = $"<size={fontSize5}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh1), "♡")}</size>";
                    string krz2 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh1), "c")}</size>";
                    string krz3 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh3), "a")}</size>";
                    string krz4 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh5), "n")}</size>";
                    string krz5 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh7), "d")}</size>";
                    string krz6 = $"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh7), "y")}</size>";
                    string krz7 = $"<size={fontSize5}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.psh9), "♡")}</size>";
                    //NOODLES NAME
                    string nd1 = $"<size={fontSize3}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.nd2), "♡")}</size>";
                    string nd2 = $"<size={fontSize1}1>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.nd1), "Virgin")}</size>";
                    //NAME COMPOSITIONS
                    string name = dev + "\r\n" + rname; //DEVS
                    string named = dscfr + "\r\n" + rname; //DISCORD FRIENDS
                    string snsname = sns1 + sns2 + sns3 + sns4 + sns5 + sns6 + sns7 + sns8 + "\r\n" + sns91 + sns9 + sns0 + sns01 + sns02 + sns03 + sns92; //ROSE NAME & TITLE
                    string nameb = lily + "\r\n" + rname; //LILY TITLE
                    string namesilence = slnc + snf + "\r\n" + rname; //SILENCE TITLE
                    string nameaugust = aug1 + augc+augu+augt+augi+auge + aug2 + "\r\n" + rname; //AUGUST TITLE
                    string pushinp = p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9 + "\r\n" + ps1 + ps2 + ps3 + ps4 + ps5 + ps6; //PUSHINP NAME & TITLE
                    string feename = fee1 + fee2 + fee3 + fee4 + fee5 + fee6 + fee7 + fee8 + fee9 + "\r\n" + fe1 + fe2 + fe3 + fe4 + fe5 + fe6 + fe7 + fe8; //FEEFEE NAME & TITLE
                    string jssname = js0 + js1 + js2 + js3 + js4 + js5 + js6 + js7 + js8 + js9 + js10 + js11 + "\r\n" + rname;
                    string cndname = cn0 + cn1 + cn2 + cn3 + cn4 + cn5 + cn6 + cn7 + cn8 + cn9 + cn10 + cn11 + "\r\n" + cnd1 + cnd2 + cnd3 + cnd4 + cnd5 + cnd6 + cnd7 + cnd8;
                    string krzname = kr1 + kr2 + kr3 + kr4 + kr5 + kr6 + kr7 + kr8 + kr9 + kr10 + kr11 + "\r\n" + krz1 + krz2 + krz3 + krz4 + krz5 + krz6 + krz7;//KRZ NAME
                    string ndsname = nd1+nd2+nd1+"\r\n"+rname;
                    if (client.FriendCode is "nullrelish#9615" or "vastblaze#8009" or "ironbling#3600" or "tillhoppy#6167" or "gnuedaphic#7196" or "pingrating#9371")
                    {
                        client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.rosecolor), name)}");
                        Main.devNames.Add(client.Character.PlayerId, rname);    
                    }
                    if (client.FriendCode is "Available#9898" or "mossmodel#2348" or "Mossmodel#2348")
                    {
                        client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.aug4), nameaugust)}");
                        Main.devNames.Add(client.Character.PlayerId, rname);
                    }
                  if (client.FriendCode is "leafywinch#2382" or "jailtoy#0133" or "alphabye#3999" or "walkingdice#5285" or "scoopgooey#9820" or "innfancy#2127" or "artfulcod#9001" or "frostmolar#1359" or "everyswap#7877" or "iconicoar#2342" or "steamquits#4906" or "ruffseated#8388" or "nicestone#7505" or "ravencalyx#2196" or "iconicpun#5624" or "flathomey#1351" or "talentsalt#4516" or "namebasic#9510" or "envykindly#7034" or "waterpupal#6193" or "privyeater#0729" or "tigerbitty#4312" or "honeytired#7330" or "waryclaw#7449" or "basicstork#6394" or "mobileswap#4514" or "sparebank#8022" or "artfulcod#9001" or "sunnysolid#5221" or "nmossmodel#2348" or "beansimple#8487" or "epicflower#1116" or "fuzzytub#9375" or "earthygale#6105" or "bestgalaxy#3894" or "earthlycat#1182" or "Cornolive#0328" or "justlotto#9472" or "numbmoss#9309" or "misygulf#7381" or "singlequay#0547" or "dupletoad#0685" or "raggedsofa#2041" or "wavealpha#6327" or "easybling#2701" or "livemice#2626" or "losermore#5811" or "toadcomic#5559")
                    {
                        client.Character.RpcSetName($"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.thirdcolor), named)}</size>");
                        Main.devNames.Add(client.Character.PlayerId, rname);
                    }
                    if (client.FriendCode is "available#2385" or "envykindly#7034")
                    {
                        client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.rosecolor), snsname)}");
                        Main.devNames.Add(client.Character.PlayerId, rname);
                    }
                    if (client.FriendCode is "available#2385" or "sunlitmoon#2472")
                    {
                        client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.rosecolor), nameb)}");
                        Main.devNames.Add(client.Character.PlayerId, rname);
                    }
                    if (client.FriendCode is "available#2110" or "testfly#6512")
                    {
                        client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.Demolitionist), pushinp)}");
                        Main.devNames.Add(client.Character.PlayerId, rname);
                    }
                    if (client.FriendCode is "riskylatte#0409" or "shotnote#2620" or "furrycoin#0508")
                    {
                        client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.rosecolor), namesilence)}");
                        Main.devNames.Add(client.Character.PlayerId, rname);
                    }
                    if (client.FriendCode is "waterpupal#6193" or "Waterpupal#6193")
                    {
                        client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.rosecolor), feename)}");
                        Main.devNames.Add(client.Character.PlayerId, rname);
                    }
                    if (client.FriendCode is "epicflower#1116" or "Epicflower#1116")
                    {
                        client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.jss8), jssname)}");
                        Main.devNames.Add(client.Character.PlayerId, rname);
                    }
                    if (client.FriendCode is "supbay#9710" or "Supbay#9710")
                    {
                        client.Character.RpcSetName($"{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.rosecolor), cndname)}");
                        Main.devNames.Add(client.Character.PlayerId, rname);
                    }
                    if (client.FriendCode is "luckyplus#8283" or "Luckyplus#8283")
                    {
                        client.Character.RpcSetName($"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.rosecolor), krzname)}</size>");
                        Main.devNames.Add(client.Character.PlayerId, rname);
                    }
                    if (client.FriendCode is "waryclaw#7449" or "Waryclaw#7449")
                    {
                        client.Character.RpcSetName($"<size={fontSize}>{Helpers.ColorString(Utils.GetRoleColor(CustomRoles.nd2), ndsname)}</size>");
                        Main.devNames.Add(client.Character.PlayerId, rname);
                    }
                }, 3f, "Welcome Message & Name Check");
            }
        }
    }
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
    class OnPlayerLeftPatch
    {
        public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData data, [HarmonyArgument(1)] DisconnectReasons reason)
        {
            //            Logger.info($"RealNames[{data.Character.PlayerId}]を削除");
            //            main.RealNames.Remove(data.Character.PlayerId);
            if (!AmongUsClient.Instance.AmHost) return;
            if (GameStates.IsInGame)
            {
                Utils.CountAliveImpostors();
                if (data.Character.Is(CustomRoles.TimeThief))
                    data.Character.ResetVotingTime();
                if (data.Character.GetCustomSubRole() == CustomRoles.LoversRecode && !data.Character.Data.IsDead)
                    foreach (var lovers in Main.LoversPlayers.ToArray())
                    {
                        Main.isLoversDead = true;
                        Main.LoversPlayers.Remove(lovers);
                        Main.HasModifier.Remove(lovers.PlayerId);
                        Main.AllPlayerCustomSubRoles[lovers.PlayerId] = CustomRoles.NoSubRoleAssigned;
                    }
                if (data.Character.Is(CustomRoles.Executioner) && Main.ExecutionerTarget.ContainsKey(data.Character.PlayerId) && Main.ExeCanChangeRoles)
                {
                    data.Character.RpcSetCustomRole(Options.CRoleExecutionerChangeRoles[Options.ExecutionerChangeRolesAfterTargetKilled.GetSelection()]);
                    Main.ExecutionerTarget.Remove(data.Character.PlayerId);
                    RPC.RemoveExecutionerKey(data.Character.PlayerId);
                }
                if (data.Character.Is(CustomRoles.GuardianAngelTOU) && Main.GuardianAngelTarget.ContainsKey(data.Character.PlayerId))
                {
                    data.Character.RpcSetCustomRole(Options.CRoleGuardianAngelChangeRoles[Options.WhenGaTargetDies.GetSelection()]);
                    if (data.Character.IsModClient())
                        data.Character.RpcSetCustomRole(Options.CRoleGuardianAngelChangeRoles[Options.WhenGaTargetDies.GetSelection()]); //対象がキルされたらオプションで設定した役職にする
                    else
                    {
                        if (Options.CRoleGuardianAngelChangeRoles[Options.WhenGaTargetDies.GetSelection()] != CustomRoles.Amnesiac)
                            data.Character.RpcSetCustomRole(Options.CRoleGuardianAngelChangeRoles[Options.WhenGaTargetDies.GetSelection()]); //対象がキルされたらオプションで設定した役職にする
                        else
                            data.Character.RpcSetCustomRole(Options.CRoleGuardianAngelChangeRoles[2]);
                    }
                    Main.GuardianAngelTarget.Remove(data.Character.PlayerId);
                    RPC.RemoveGAKey(data.Character.PlayerId);
                }
                if (data.Character.Is(CustomRoles.Jackal))
                {
                    Main.JackalDied = true;
                    if (Options.SidekickGetsPromoted.GetBool())
                    {
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc.Is(CustomRoles.Sidekick))
                                pc.RpcSetCustomRole(CustomRoles.Jackal);
                        }
                    }
                }
                if (Main.ColliderPlayers.Contains(data.Character) && CustomRoles.YingYanger.IsEnable() && Options.ResetToYinYang.GetBool())
                {
                    Main.DoingYingYang = false;
                }
                if (Main.ColliderPlayers.Contains(data.Character))
                    Main.ColliderPlayers.Remove(data.Character);
                if (data.Character.LastImpostor() || data.Character.Is(CustomRoles.Egoist))
                {
                    //Main.currentWinner = CustomWinner.None;
                    /*bool egoist = false;
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc.Data.Disconnected || pc == null) continue;
                        CustomRoles pc_role = pc.GetCustomRole();
                        if (pc_role == CustomRoles.Egoist && !pc.Data.IsDead) egoist = true;
                    }
                    if (data.Character.Is(CustomRoles.Egoist) && egoist)
                    {
                        if (Main.AliveImpostorCount != 1) egoist = false;
                    }
                    if (!egoist)*/
                    ShipStatus.Instance.enabled = false;
                    ShipStatus.RpcEndGame(GameOverReason.ImpostorDisconnect, false);
                }
                if (Main.ExecutionerTarget.ContainsValue(data.Character.PlayerId) && Main.ExeCanChangeRoles)
                {
                    byte Executioner = 0x73;
                    Main.ExecutionerTarget.Do(x =>
                    {
                        if (x.Value == data.Character.PlayerId)
                            Executioner = x.Key;
                    });
                    Utils.GetPlayerById(Executioner).RpcSetCustomRole(Options.CRoleExecutionerChangeRoles[Options.ExecutionerChangeRolesAfterTargetKilled.GetSelection()]);
                    Main.ExecutionerTarget.Remove(Executioner);
                    RPC.RemoveExecutionerKey(Executioner);
                    Utils.NotifyRoles();
                }
                if (Main.GuardianAngelTarget.ContainsValue(data.Character.PlayerId))
                {
                    byte GA = 0x73;
                    Main.ExecutionerTarget.Do(x =>
                    {
                        if (x.Value == data.Character.PlayerId)
                            GA = x.Key;
                    });
                    // Utils.GetPlayerById(GA).RpcSetCustomRole(Options.CRoleGuardianAngelChangeRoles[Options.WhenGaTargetDies.GetSelection()]);
                    if (Utils.GetPlayerById(GA).IsModClient())
                        Utils.GetPlayerById(GA).RpcSetCustomRole(Options.CRoleGuardianAngelChangeRoles[Options.WhenGaTargetDies.GetSelection()]); //対象がキルされたらオプションで設定した役職にする
                    else
                    {
                        if (Options.CRoleGuardianAngelChangeRoles[Options.WhenGaTargetDies.GetSelection()] != CustomRoles.Amnesiac)
                            Utils.GetPlayerById(GA).RpcSetCustomRole(Options.CRoleGuardianAngelChangeRoles[Options.WhenGaTargetDies.GetSelection()]); //対象がキルされたらオプションで設定した役職にする
                        else
                            Utils.GetPlayerById(GA).RpcSetCustomRole(Options.CRoleGuardianAngelChangeRoles[2]);
                    }
                    Main.GuardianAngelTarget.Remove(GA);
                    RPC.RemoveGAKey(GA);
                    Utils.NotifyRoles();
                }
                if (PlayerState.GetDeathReason(data.Character.PlayerId) == PlayerState.DeathReason.etc) //死因が設定されていなかったら
                {
                    PlayerState.SetDeathReason(data.Character.PlayerId, PlayerState.DeathReason.Disconnected);
                    PlayerState.SetDead(data.Character.PlayerId);
                }
                if (AmongUsClient.Instance.AmHost && GameStates.IsLobby)
                {
                    _ = new LateTask(() =>
                    {
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            pc.RpcSetNameEx(pc.GetRealName(isMeeting: true));
                        }
                    }, 1f, "SetName To Chat");
                }
            }
if (Main.devNames.ContainsKey(data.Character.Data.PlayerId))
                Main.devNames.Remove(data.Character.Data.PlayerId);
            Logger.Info($"{data.PlayerName}(ClientID:{data.Id})が切断(理由:{reason})", "Session");
        }
    }
}
