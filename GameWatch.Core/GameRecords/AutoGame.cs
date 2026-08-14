using GameWatch.Core.Wrappers;

namespace GameWatch.Core.GameRecords;

public sealed class AutoGame
{
    public DisplayId Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public ElapsedTime PlayTimeSec { get; set; }
    public string? WindowTitle { get; set; }
    public string? FilePath { get; set; }
    public string? WindowRule { get; set; }
    public string? PathRule { get; set; }

    public AutoGame()
    {
    }

    public AutoGame(Dto.AutoGame dto, DisplayId displayId)
    {
        Id = displayId;
        Name = dto.Name;
        PlayTimeSec = new ElapsedTime(dto.PlayTimeSec);
        WindowTitle = dto.WindowTitle;
        WindowRule = dto.WindowRule;
        FilePath = dto.FilePath;
    }
}