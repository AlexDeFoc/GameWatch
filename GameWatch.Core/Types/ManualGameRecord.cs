namespace GameWatch.Core.Types;

public sealed class ManualGameRecord
{
    public TableId TableId { get; }
    public string Name { get; set; } = string.Empty;
    public ElapsedTime PlayTimeSec { get; set; }

    public ManualGameRecord()
    {
    }

    public ManualGameRecord(ManualGameDto dto)
    {
        TableId = new TableId(dto.TableId);
        Name = dto.Name;
        PlayTimeSec = new ElapsedTime(dto.PlayTimeSec);
    }
}