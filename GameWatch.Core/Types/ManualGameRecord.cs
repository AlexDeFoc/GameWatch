namespace GameWatch.Core.Types;

public sealed class ManualGameRecord
{
    public TableId TableId { get; }
    public string Name { get; set; } = string.Empty;
    public ElapsedTime PlayTime { get; set; }

    public ManualGameRecord()
    {
    }

    public ManualGameRecord(ManualGameDto dto)
    {
        TableId = new TableId(dto.TableId);
        Name = dto.Name;
        PlayTime = new ElapsedTime(dto.PlayTime);
    }
}