using GameWatch.Core.Dto;
using GameWatch.Core.Wrappers;

namespace GameWatch.Core.GameRecords;

public sealed class AutoGame
{
    public GameId Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public ElapsedTime PlayTimeSec { get; set; }
    public string? WindowTitle { get; set; }
    public string? FilePath { get; set; }
    public string? WindowRule { get; set; }
    public string? PathRule { get; set; }

    public AutoGame()
    {
    }

    public AutoGame(Dto.AutoGame g)
    {
        Id = new GameId(g.Id);
        Name = g.Name;
        PlayTimeSec = new ElapsedTime(g.PlayTimeSec);
        WindowTitle = g.WindowTitle;
        FilePath = g.FilePath;
        WindowRule = g.WindowRule;
        PathRule = g.PathRule;
    }
}