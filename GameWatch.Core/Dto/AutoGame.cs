namespace GameWatch.Core.Dto;

public sealed class AutoGame
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public long PlayTimeSec { get; init; }
    public string? WindowTitle { get; init; }
    public string? FilePath { get; init; }
    public string? WindowRule { get; init; }
    public string? PathRule { get; init; }
}