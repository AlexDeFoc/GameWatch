namespace GameWatch.Core.Types;

public sealed class AutoGameRecord
{
    public TableId TableId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ElapsedTime PlayTime { get; set; }
    public string? WindowTitle { get; set; }
    public string? FilePath { get; set; }
    public string? WindowRule { get; set; }
    public string? PathRule { get; set; }

    public AutoGameRecord()
    {
    }

    public AutoGameRecord(AutoGameDto dto)
    {
        TableId = new TableId(dto.TableId);
        Name = dto.Name;
        PlayTime = new ElapsedTime(dto.PlayTime);
        WindowTitle = dto.WindowTitle;
        WindowRule = dto.WindowRule;
        FilePath = dto.FilePath;
        PathRule = dto.PathRule;
    }
}