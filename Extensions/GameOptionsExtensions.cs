#nullable enable
using System;
using AmongUs.GameOptions;

namespace TownOfHost.PrivateExtensions;

public static class GameOptionsExtensions
{
    public static NormalGameOptionsV07? AsNormalOptions(this IGameOptions options)
    {
        return options.Cast<NormalGameOptionsV07>();
    }

    public static HideNSeekGameOptionsV07? AsHnsOptions(this IGameOptions options)
    {
        return options.Cast<HideNSeekGameOptionsV07>();
    }

    public static IGameOptions? AsGameOptions(this IGameOptions options)
    {
        return options.Cast<IGameOptions>();
    }

    public static Byte[] ToBytes(this IGameOptions gameOptions)
    {
        return GameOptionsManager.Instance.gameOptionsFactory.ToBytes(gameOptions);
    }
}