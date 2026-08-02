using GameWatch.Core.Dto;

namespace GameWatch.Core.GameRecords;

public sealed class ManualGame
{
    public GameId Id { get; set; }
    public required string Name { get; set; }
    public ElapsedTime PlayTimeSec { get; set; }
}