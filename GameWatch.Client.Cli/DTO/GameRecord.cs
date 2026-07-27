namespace GameWatch.Client.Cli.DTO;

public record GameRecord(int PositionIdx, string Title, long PlayTime = 0, GameMode Mode = GameMode.Manual, string? WindowTitle = null, string? ProcessName = null, string? FilePath = null);