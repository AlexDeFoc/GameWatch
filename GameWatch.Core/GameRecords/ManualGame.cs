using GameWatch.Core.Dto;
using GameWatch.Core.Wrappers;

namespace GameWatch.Core.GameRecords;

public sealed class ManualGame
{
    public GameId Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ElapsedTime PlayTimeSec { get; set; }

    public ManualGame() {}

    public ManualGame(Dto.ManualGame g)
    {
        Id = new GameId(g.Id);
        Name = g.Name;
        PlayTimeSec = new ElapsedTime(g.PlayTimeSec);
    }
}