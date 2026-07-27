namespace GameWatch.Client.Cli.Dto;

public record GameRecord(int PositionIdx, string Title, long PlayTime = 0, GameMode Mode = GameMode.Manual, string? WindowTitle = null, string? FilePath = null);