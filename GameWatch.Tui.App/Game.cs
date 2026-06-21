using System;

namespace GameWatch.Tui.App;

public sealed class Game
{
    public Game(string title, TimeSpan playTime)
    {
        Title = title;
        PlayTime = playTime;
        FilePath = "";
        WorkingMode = WorkingModeType.Manual;
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
    public WorkingModeType WorkingMode { get; private set; }
    public string FilePath { get; private set; }
    public bool ProcessIsActive { get; set; }
    public int Pid { get; set; }
    public DateTime ProcessCreationTime { get; set; }
    public bool ManualWorkingGameIsActive { private get; set; }
    public DateTime? SessionStartTime { get; set; }

    public void AddPlaytime(TimeSpan extraTime) => PlayTime += extraTime;

    public enum WorkingModeType
    {
        Automatic,
        Manual
    }
}