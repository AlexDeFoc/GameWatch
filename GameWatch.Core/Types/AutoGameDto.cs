namespace GameWatch.Core.Types;

public sealed class AutoGameDto
{
    public int TableId { get; init; }
    public string Name { get; init; } = string.Empty;
    public long PlayTimeSec { get; init; }
    public string? WindowTitle { get; init; }
    public string? FilePath { get; init; }
    public string? WindowRule { get; init; }
    public string? PathRule { get; init; }

    public AutoGameDto()
    {
    }

    public AutoGameDto(AutoGameRecord g)
    {
        TableId = g.TableId.V;
        Name = g.Name;
        PlayTimeSec = g.PlayTimeSec.V;
        WindowTitle = g.WindowTitle;
        WindowRule = g.WindowRule;
        FilePath = g.FilePath;
        PathRule = g.PathRule;
    }
}