// ReSharper disable RedundantDefaultMemberInitializer
// ReSharper disable PropertyCanBeMadeInitOnly.Global
namespace GameWatch.Client.Cli.Dto.GameRecords;

public sealed class AutoGameRecord
{
    public required string Title { get; set; }
    public required int PlayTime { get; set; }
    public string? ProcessWindowTitle { get; set; } = null;
    public string? ProcessFilePath { get; set; } = null;
    public string? WindowTitleRegexPattern { get; set; } = null;
    public string? FilePathRegexPattern { get; set; } = null;
    public bool ShouldMatchAgainstProcessWindowTitle { get; set; } = false;
    public bool ShouldMatchAgainstProcessFilePath { get; set; } = false;
    public bool ShouldMatchProcessWindowTitleAgainstRegexPattern { get; set; } = false;
    public bool ShouldMatchProcessFilePathAgainstRegexPattern { get; set; } = false;
}