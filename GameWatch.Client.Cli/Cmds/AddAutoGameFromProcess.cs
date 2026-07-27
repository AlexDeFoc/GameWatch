using System.ComponentModel;
using System.Threading;
using GameWatch.Client.Cli.Dto.GameRecords;
using GameWatch.Client.Cli.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace GameWatch.Client.Cli.Cmds;

public sealed class AddAutoGameFromProcess : Command<AddAutoGameFromProcess.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-t|--title <TITLE>", isRequired: true)]
        [Description("Title of the game record.")]
        public required string GameRecordTitle { get; init; }

        [CommandOption("--pid <PROCESS_ID>", isRequired: true)]
        [Description("ID of the target active process. TIP: Can be gathered from 'list procs'.")]
        public required int ProcessPid { get; init; }

        [CommandOption("-p|--playtime <SECONDS>")]
        [Description("Initial game record playtime in seconds.")]
        [DefaultValue(0)]
        public int GameRecordPlayTime { get; init; }

        [CommandOption("--regex-win-title <REGEX_PATTERN>")]
        [Description("Regex patter to which to match the process window title against.")]
        public string? WindowTitleRegexPattern { get; init; }

        [CommandOption("--regex-fp <REGEX_PATTERN>")]
        [Description("Regex patter to which to match the process filepath against.")]
        public string? FilePathRegexPattern { get; init; }

        [CommandOption("--match-win-title")]
        [Description("Should when monitoring match against the process window title.")]
        public bool ShouldMatchAgainstWindowTitle { get; init; }

        [CommandOption("--match-fp")]
        [Description("Should when monitoring match against the process filepath.")]
        public bool ShouldMatchAgainstFilePath { get; init; }

        [CommandOption("--regex-match-win-title")]
        [Description("Should when monitoring match the process window title against a regex pattern.")]
        public bool ShouldMatchWindowTitleAgainstRegexPattern { get; init; }

        [CommandOption("--regex-match-fp")]
        [Description("Should when monitoring match the process filepath against a regex pattern.")]
        public bool ShouldMatchFilePathAgainstRegexPattern { get; init; }
    }

    protected override ValidationResult Validate(CommandContext context, Settings settings)
    {
        if (settings is { ShouldMatchWindowTitleAgainstRegexPattern: true, WindowTitleRegexPattern: not null })
        {
            if (!RegexHandler.ValidatePattern(settings.WindowTitleRegexPattern))
            {
                ValidationResult.Error("Regex string provided for Process Window Title matching, is invalid regex syntax.");
            }
        }

        // ReSharper disable once InvertIf
        if (settings is { ShouldMatchFilePathAgainstRegexPattern: true, FilePathRegexPattern: not null })
        {
            if (!RegexHandler.ValidatePattern(settings.FilePathRegexPattern))
            {
                ValidationResult.Error("Regex string provided for Process FilePath matching, is invalid regex syntax.");
            }
        }

        return settings switch
        {
            { ShouldMatchAgainstWindowTitle: true, ShouldMatchWindowTitleAgainstRegexPattern: true } => ValidationResult.Error("Cannot match against the full window title string while attempting to match partially using a regex pattern. These flags are mutually exclusive!"),
            { ShouldMatchAgainstFilePath: true, ShouldMatchFilePathAgainstRegexPattern: true } => ValidationResult.Error("Cannot match against the full filepath string while attempting to match partially using a regex pattern. These flags are mutually exclusive!"),
            { ShouldMatchWindowTitleAgainstRegexPattern: true, WindowTitleRegexPattern: null } => ValidationResult.Error("Cannot request to match the process window title against a regex pattern without providing the actual regex pattern string."),
            { ShouldMatchFilePathAgainstRegexPattern: true, FilePathRegexPattern: null } => ValidationResult.Error("Cannot request to match the process filepath against a regex pattern without providing the actual regex pattern string."),
            _ => ValidationResult.Success()
        };
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        using var conn = DbFactory.GameLibrary.CreateConnection();
        using var tran = conn.BeginTransaction();

        var matchAgainstAllAvailableProcessProperties = settings is { ShouldMatchAgainstWindowTitle: false, ShouldMatchAgainstFilePath: false, ShouldMatchWindowTitleAgainstRegexPattern: false, ShouldMatchFilePathAgainstRegexPattern: false };

        var ourProc = ProcessFinder.GetOurProcFromPid(settings.ProcessPid);

        AutoGameRecord gameRecord = new() {Title = settings.GameRecordTitle, PlayTime = settings.GameRecordPlayTime};

        if (matchAgainstAllAvailableProcessProperties)
        {
            gameRecord.ProcessWindowTitle = ourProc.WindowTitle;
            gameRecord.ProcessFilePath = ourProc.FilePath;
            gameRecord.ShouldMatchAgainstProcessWindowTitle = true;
            gameRecord.ShouldMatchAgainstProcessFilePath = true;
        }

        if (settings.ShouldMatchAgainstWindowTitle)
        {
            gameRecord.ProcessWindowTitle = ourProc.WindowTitle;
            gameRecord.ShouldMatchAgainstProcessWindowTitle = true;
        }

        if (settings.ShouldMatchAgainstFilePath)
        {
            gameRecord.ProcessFilePath = ourProc.FilePath;
            gameRecord.ShouldMatchAgainstProcessFilePath = true;
        }

        if (settings.ShouldMatchWindowTitleAgainstRegexPattern)
        {
            gameRecord.ShouldMatchProcessWindowTitleAgainstRegexPattern = true;
            gameRecord.WindowTitleRegexPattern = settings.WindowTitleRegexPattern;
        }

        if (settings.ShouldMatchFilePathAgainstRegexPattern)
        {
            gameRecord.ShouldMatchProcessFilePathAgainstRegexPattern = true;
            gameRecord.FilePathRegexPattern = settings.FilePathRegexPattern;
        }

        DbFactory.GameLibrary.AddGame(conn, tran, gameRecord);

        return 0;
    }
}