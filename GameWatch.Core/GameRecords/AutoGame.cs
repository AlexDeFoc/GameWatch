using GameWatch.Core.Dto;

namespace GameWatch.Core.GameRecords;

public sealed class AutoGame
{
    public GameId Id { get; init; }
    public required string Name { get; init; }
    public ElapsedTime PlayTimeSec { get; init; }
    public string? WindowTitle { get; set; }
    public string? FilePath { get; set; }
    public string? WindowRule { get; set; }
    public string? PathRule { get; set; }
}