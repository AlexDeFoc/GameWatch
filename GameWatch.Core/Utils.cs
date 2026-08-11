using System;
using GameWatch.Core.Wrappers;

namespace GameWatch.Core;

public static class Utils
{
    public static string GetTableName(GameMode gameMode) => gameMode switch
    {
        GameMode.Manual => "ManualGames",
        GameMode.Auto => "AutoGames",
        _ => throw new ArgumentOutOfRangeException(nameof(gameMode), gameMode, "Unsupported Game mode.")
    };

    public static bool IsIdxWithinBounds(GameIdx i, ConcurrentList<GameId> list) => i.V >= 0 && i.V < list.Count;
}