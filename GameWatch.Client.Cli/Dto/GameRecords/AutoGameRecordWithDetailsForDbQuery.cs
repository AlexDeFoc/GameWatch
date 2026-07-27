// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global
namespace GameWatch.Client.Cli.Dto.GameRecords;

public sealed class AutoGameRecordWithDetailsForDbQuery
{
    public int TableId { get; init; }
    public int TablePositionIdx { get; init; }
    public string GameRecordTitle { get; init; } = null!;
    public int GameRecordPlayTime { get; init; }
    public string? ProcessWindowTitle { get; init; }
    public string? ProcessFilePath { get; init; }
    public string? WindowTitleRegexPattern { get; init; }
    public string? FilePathRegexPattern { get; init; }
    public bool ShouldMatchAgainstProcessWindowTitle { get; init; }
    public bool ShouldMatchAgainstProcessFilePath { get; init; }
    public bool ShouldMatchProcessWindowTitleAgainstRegexPattern { get; init; }
    public bool ShouldMatchProcessFilePathAgainstRegexPattern { get; init; }
}