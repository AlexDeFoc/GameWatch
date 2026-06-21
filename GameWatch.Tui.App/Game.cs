using System;

namespace GameWatch.Tui.App;

public sealed class Game
{
    public Game(string title)
    {
        Title = title;
        PlayTime = TimeSpan.Zero;
        FilePath = "";
        WorkingMode = WorkingModeType.Manual;
    }

    public Game(string title, TimeSpan playTime)
    {
        Title = title;
        PlayTime = playTime;
        FilePath = "";
        WorkingMode = WorkingModeType.Manual;
    }

    public Game(string title, string gameFilePath)
    {
        Title = title;
        PlayTime = TimeSpan.Zero;
        FilePath = gameFilePath;
        WorkingMode = WorkingModeType.Automatic;
    }

    public Game(string title, TimeSpan playtime, WorkingModeType workingMode, string exePath = "")
    {
        Title = title;
        PlayTime = playtime;
        FilePath = exePath;
        WorkingMode = workingMode;
    }

    public string Title { get; set; }
    public TimeSpan PlayTime { get; private set; }
    public WorkingModeType WorkingMode { get; set; }
    public string FilePath { get; set; }
    public bool ProcessIsActive { get; set; }
    public int Pid { get; set; }
    public DateTime ProcessCreationTime { get; set; }
    public bool ManualWorkingGameIsActive { get; set; }
    public DateTime? SessionStartTime { get; set; }

    public void AddPlaytime(TimeSpan extraTime) => PlayTime += extraTime;

    public void ResetPlayTime() => PlayTime = TimeSpan.Zero;

    public enum WorkingModeType
    {
        Automatic,
        Manual
    }
}