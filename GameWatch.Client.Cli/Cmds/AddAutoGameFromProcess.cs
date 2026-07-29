using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Core.Dto.GameRecords;
using GameWatch.Core.Helpers;
using GameWatch.Core.Ipc;
using Spectre.Console;
using Spectre.Console.Cli;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace GameWatch.Client.Cli.Cmds;

public sealed class AddAutoGameFromProcess : AsyncCommand<AddAutoGameFromProcess.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-t|--title <TITLE>", isRequired: true)]
        [Description("Title of the game record.")]
        public required string Title { get; init; }

        [CommandOption("--pid <PROCESS_ID>", isRequired: true)]
        [Description("ID of the target active process\n    TIP: Can be gathered from 'list procs'")]
        public required int ProcessPid { get; init; }

        [CommandOption("-p|--playtime <SECONDS>")]
        [Description("Initial game record playtime in seconds")]
        [DefaultValue(0)]
        public int PlayTimeSeconds { get; init; }

        [CommandOption("--match-title")]
        [Description("Match against the process window title")]
        public bool MatchWindowTitle { get; init; }

        [CommandOption("--match-path")]
        [Description("Match against the process file path")]
        public bool MatchFilePath { get; init; }

        [CommandOption("--title-pattern <REGEX>")]
        [Description("Regex pattern to match against the process window title")]
        public string? WindowTitlePattern { get; init; }

        [CommandOption("--path-pattern <REGEX>")]
        [Description("Regex pattern to match against the process file path")]
        public string? FilePathPattern { get; init; }
    }

    protected override ValidationResult Validate(CommandContext context, Settings settings)
    {
        if (settings.WindowTitlePattern != null && !RegexHandler.IsValidPattern(settings.WindowTitlePattern))
        {
            return ValidationResult.Error("Window Title Pattern: invalid regex syntax.");
        }

        if (settings.FilePathPattern != null && !RegexHandler.IsValidPattern(settings.FilePathPattern))
        {
            return ValidationResult.Error("File Path Pattern: invalid regex syntax.");
        }

        return ValidationResult.Success();
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var ourProc = ProcessFinder.GetOurProcFromPid(settings.ProcessPid);
        if (ourProc == null)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Could not locate process with PID [bold]{settings.ProcessPid}[/].");
            return 1;
        }

        var gameRecord = new AutoGame { Title = settings.Title, PlayTimeSeconds = settings.PlayTimeSeconds };

        if (settings is { MatchWindowTitle: false, MatchFilePath: false, WindowTitlePattern: null, FilePathPattern: null })
        {
            gameRecord.ProcessFilePath = ourProc.FilePath;
            gameRecord.ProcessWindowTitle = ourProc.WindowTitle;
        }
        else
        {
            if (settings.MatchWindowTitle)
            {
                gameRecord.ProcessWindowTitle = ourProc.WindowTitle;
            }

            if (settings.MatchFilePath)
            {
                gameRecord.ProcessFilePath = ourProc.FilePath;
            }
        }

        gameRecord.ProcessWindowTitlePattern = settings.WindowTitlePattern;
        gameRecord.ProcessFilePathPattern = settings.FilePathPattern;

        // Add rule to DB
        DbFactory.GameLibrary.AddGame(gameRecord);
        AnsiConsole.MarkupLine($"[green]✓[/] Successfully added auto-game rule for [bold]{settings.Title}[/].");

        // Notify running Agent over IPC to reload rules instantly
        try
        {
            var notified = await IpcClient.SendRefreshSignalAsync(IpcTarget.GameWatchGameMonitorAgent, cancellationToken);
            if (!notified)
            {
                AnsiConsole.MarkupLine("[yellow]⚠ Note:[/] Background agent is not running. Rules will load on next agent start.");
            }
        }
        catch (Exception)
        {
            AnsiConsole.MarkupLine("[yellow]⚠ Note:[/] Failed to ping background agent. Rules saved to DB successfully.");
        }

        return 0;
    }
}