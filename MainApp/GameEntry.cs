using System;

namespace MainApp;

public sealed class GameEntry
{
    public string Title { get; set; }
    public TimeSpan Playtime { get; set; } = TimeSpan.FromMinutes(5);

    public GameEntry(string title) => Title = title;
}