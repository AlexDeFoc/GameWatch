using System;
using System.Collections.Generic;

namespace MainApp;

public sealed class GameEntry
{
    // Properties
    public string Title { get; }
    public TimeSpan PlayTime { get; private set; }
    public WorkingMode CurrentWorkingMode { get; private set; }
    public string ExePath { get; private set; }
    public int Pid { get; set; }
    public DateTime ProcessCreationTime { get; set; }
    public bool ProcessIsActive { get; set; }
    public bool ManualWorkingGameIsActive { get; set; }
    public DateTime? SessionStartTime { get; set; }

    // Constructors
    /// <summary>
    /// Create a game entry with automatic working mode
    /// </summary>
    public GameEntry(string title, string exePath)
    {
        Title = title;
        PlayTime = TimeSpan.Zero;
        ExePath = exePath;
        CurrentWorkingMode = WorkingMode.Automatic;
    }

    /// <summary>
    /// Create a game entry with manual working mode
    /// </summary>
    public GameEntry(string title)
    {
        Title = title;
        PlayTime = TimeSpan.Zero;
        ExePath = "";
        CurrentWorkingMode = WorkingMode.Manual;
    }

    /// <summary>
    /// Create a game entry with manual working mode
    /// </summary>
    public GameEntry(string title, TimeSpan playTime)
    {
        Title = title;
        PlayTime = playTime;
        ExePath = "";
        CurrentWorkingMode = WorkingMode.Manual;
    }

    /// <summary>
    /// Create a game entry with automatic/manual working mode
    /// </summary>
    public GameEntry(string title, TimeSpan playtime, WorkingMode workingMode, string exePath = "")
    {
        Title = title;
        PlayTime = playtime;
        ExePath = exePath;
        CurrentWorkingMode = workingMode;
    }

    // Public methods
    public void ResetPlaytime() => PlayTime = TimeSpan.Zero;

    public void AddPlaytime(TimeSpan extraTime) => PlayTime += extraTime;

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

    // Public structures
    public enum WorkingMode
    {
        Automatic,
        Manual
    }
}