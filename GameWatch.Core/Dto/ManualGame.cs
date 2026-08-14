namespace GameWatch.Core.Dto;

public sealed class ManualGame
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public long PlayTimeSec { get; init; }
}