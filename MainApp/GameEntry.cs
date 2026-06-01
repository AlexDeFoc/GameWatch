using System;
using System.Collections.Generic;

namespace MainApp;

public sealed class GameEntry
{
    public string Title { get; }

    public TimeSpan PlayTime { get; private set; }

    public string GetPrintablePlaytime()
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

    public GameEntry(string title) => Title = title;

    public GameEntry(string title, TimeSpan playtime)
    {
        Title = title;
        PlayTime = playtime;
    }

    public void ResetPlaytime() => PlayTime = TimeSpan.Zero;

    public void AddPlaytime(TimeSpan extraTime) => PlayTime += extraTime;
}