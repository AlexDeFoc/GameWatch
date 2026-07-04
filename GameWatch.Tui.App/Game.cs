using System;
using System.Collections.Generic;
using GameWatch.DataTypes;

namespace GameWatch.Tui.App;

public sealed class Game
{
    public Game(string title)
    {
        Title = title;
        PlayTime = TimeSpan.Zero;
        FilePath = "";
        Mode = GameMode.Manual;
    }

    public Game(string title, TimeSpan playTime)
    {
        Title = title;
        PlayTime = playTime;
        FilePath = "";
        Mode = GameMode.Manual;
    }

    public Game(string title, string gameFilePath)
    {
        Title = title;
        PlayTime = TimeSpan.Zero;
        FilePath = gameFilePath;
        Mode = GameMode.Automatic;
    }

    public Game(string title, TimeSpan playtime, GameMode mode, string exePath = "")
    {
        Title = title;
        PlayTime = playtime;
        FilePath = exePath;
        Mode = mode;
    }

    public string Title { get; set; }
    public TimeSpan PlayTime { get; private set; }
    public GameMode Mode { get; set; }
    public string FilePath { get; set; }
    public bool ProcessIsActive { get; set; }
    public int Pid { get; set; }
    public DateTime ProcessCreationTime { get; set; }
    public bool ManualWorkingGameIsActive { get; set; }

    public void AddPlaytime(TimeSpan extraTime) => PlayTime += extraTime;

    public void ResetPlayTime() => PlayTime = TimeSpan.Zero;

    public string PlayTimeString()
    {
        var parts = new List<string>();

        if (PlayTime.Days > 0)
            parts.Add($"{PlayTime.Days} day{(PlayTime.Days > 1 ? "s" : "")}");

        if (PlayTime.Hours > 0)
            parts.Add($"{PlayTime.Hours} h");

        if (PlayTime.Minutes > 0)
            parts.Add($"{PlayTime.Minutes} min");

        if (PlayTime.Seconds > 0 || parts.Count == 0)
            parts.Add($"{PlayTime.Seconds} s");

        return string.Join(" : ", parts);
    }

    public static string GetPrintableCurrentWorkingMode(AppContext ctx, GameMode workingMode)
    {
        return workingMode switch
        {
            GameMode.Manual => ctx.LanguageManager.Strings.GameClass.ManualWorkingModeType,
            GameMode.Automatic => ctx.LanguageManager.Strings.GameClass.AutomaticWorkingModeType,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}