using GameWatch.Core.Wrappers;

namespace GameWatch.Core.GameRecords;

public sealed class ManualGame
{
    public DisplayId Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public ElapsedTime PlayTimeSec { get; set; }

    public ManualGame()
    {
    }

    public ManualGame(Dto.ManualGame dto, DisplayId displayId)
    {
        Id = displayId;
        Name = dto.Name;
        PlayTimeSec = new ElapsedTime(dto.PlayTimeSec);
    }
}