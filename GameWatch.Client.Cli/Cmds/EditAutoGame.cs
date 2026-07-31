using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Core.Dto;
using GameWatch.Core.Helpers;
using GameWatch.Core.Ipc;
using Spectre.Console;
using Spectre.Console.Cli;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace GameWatch.Client.Cli.Cmds;

public sealed class EditAutoGame : AsyncCommand<EditAutoGame.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-i|--idx <GAME_INDEX>", isRequired: true)]
        [Description("The game index. TIP: Can be gathered from 'list games'")]
        public required int GameIdx { get; init; }

        [CommandOption("--pid <PROCESS_ID>", isRequired: true)]
        [Description("ID of the target active process\n    TIP: Can be gathered from 'list procs'")]
        public required int ProcessPid { get; init; }

        [CommandOption("-t|--title <TITLE>")]
        [Description("Title of the game record.")]
        public string? Title { get; init; }

        [CommandOption("-p|--playtime <SECONDS>")]
        [Description("Forced game record playtime in seconds")]
        public int? PlayTimeSeconds { get; init; }

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

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        string? procWindowTitle = null;
        string? procFilePath = null;

        if (settings.MatchWindowTitle || settings.MatchFilePath)
        {
            var targetProc = ProcessFinder.GetOurProcFromPid(settings.ProcessPid);

            if (targetProc == null)
            {
                Console.WriteLine("Error: Provided pid wasn't of an active process or an issue with it has happened, which to target when editing; because the app needs to extract the window title or file path of the process to match it exactly (because you provided the flags to match one of " +
                    "those or both exactly).");
                return 1;
            }

            if (settings.MatchWindowTitle)
                procWindowTitle = targetProc.WindowTitle;

            if (settings.MatchFilePath)
                procFilePath = targetProc.FilePath;
        }

        var result = DbFactory.GameLibrary.ChangeGameProperty(GameMode.Auto, settings.GameIdx,
                                                              title: settings.Title,
                                                              playTimeSeconds: settings.PlayTimeSeconds,
                                                              procWindowTitle: procWindowTitle,
                                                              procFilePath: procFilePath,
                                                              windowTitlePattern: settings.WindowTitlePattern,
                                                              filePathPattern: settings.FilePathPattern
        );

        if (!result.HasSucceeded)
        {
            Console.WriteLine($"Error: {result.FailureReason}.");

            return 1;
        }

        Console.WriteLine("Game edited successfully.");

        try
        {
            var notified = await IpcClient.SendRefreshSignalForAutoGamesListAsync(IpcTarget.GameWatchGameMonitorAgent, cancellationToken);
            if (!notified)
            {
                AnsiConsole.MarkupLine("[yellow]⚠ Note:[/] Background agent is not running. New Rules will load on next agent start.");
            }
        }
        catch (Exception)
        {
            AnsiConsole.MarkupLine("[yellow]⚠ Note:[/] Failed to ping background agent. New Rules saved to DB successfully.");
        }

        return 0;
    }
}