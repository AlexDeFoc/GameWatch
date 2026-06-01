using System;

namespace MainApp;

public sealed class GameEntry
{
    public string Title { get; }

    public string GetPrintablePlaytime() => $"{(int)_playTime.TotalHours:F0}:{_playTime:mm\\:ss}";

    private readonly TimeSpan _playTime = TimeSpan.Zero;

    public GameEntry(string title) => Title = title;
}