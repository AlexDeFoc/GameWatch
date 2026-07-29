// ReSharper disable PropertyCanBeMadeInitOnly.Global
namespace GameWatch.Core.Dto.GameRecords;

public sealed class AutoGame
{
    public int Idx { get; set; }
    public required string Title { get; set; }
    public int PlayTimeSeconds { get; set; }

    // Exact match criteria (null = disabled)
    public string? ProcessWindowTitle { get; set; }
    public string? ProcessFilePath { get; set; }

    // Pattern/regex criteria (null = disabled)
    public string? ProcessWindowTitlePattern { get; set; }
    public string? ProcessFilePathPattern { get; set; }
}