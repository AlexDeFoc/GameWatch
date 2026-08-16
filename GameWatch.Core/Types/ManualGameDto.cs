namespace GameWatch.Core.Types;

public sealed class ManualGameDto
{
    public int TableId { get; init; }
    public string Name { get; init; } = string.Empty;
    public long PlayTimeSec { get; init; }
}