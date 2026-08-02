using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Agent.GameMonitor;
using GameWatch.Core.Agents.GameMonitor;
using GameWatch.Core.Dto;
using GameWatch.Core.GameRecords;
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
        [CommandOption("-n|--name <NAME>", isRequired: true)]
        [Description("Name of the Game record.")]
        public required string Name { get; init; }

        [CommandOption("--pid <PROCESS_ID>", isRequired: true)]
        [Description("ID of the target active process\n    TIP: Can be gathered from 'list procs'")]
        public required int ProcessPid { get; init; }

        [CommandOption("-p|--playtime <SECONDS>")]
        [Description("Initial Game record playtime in seconds")]
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
        public string? WindowTitleRule { get; init; }

        [CommandOption("--path-pattern <REGEX>")]
        [Description("Regex pattern to match against the process file path")]
        public string? FilePathRule { get; init; }
    }

    protected override ValidationResult Validate(CommandContext context, Settings settings)
    {
        if (settings.WindowTitleRule != null && !RegexHandler.IsValidPattern(settings.WindowTitleRule))
        {
            return ValidationResult.Error("⛔ Provided window title pattern is not a valid regex pattern! Ignoring command...");
        }

        if (settings.FilePathRule != null && !RegexHandler.IsValidPattern(settings.FilePathRule))
        {
            return ValidationResult.Error("⛔ Provided file path pattern is not a valid regex pattern! Ignoring command...");
        }

        return ValidationResult.Success();
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var ourProc = ProcessFinder.GetOurProcFromPid(settings.ProcessPid);
        if (ourProc == null)
        {
            Console.WriteLine("⛔ Failed to get process with provided pid. Ignoring command...");
            return 1;
        }

        var gameRecord = new AutoGame { Name = settings.Name, PlayTimeSec = new ElapsedTime(settings.PlayTimeSeconds) };

        if (settings is { MatchWindowTitle: false, MatchFilePath: false, WindowTitleRule: null, FilePathRule: null })
        {
            gameRecord.FilePath = ourProc.FilePath;
            gameRecord.WindowTitle = ourProc.WindowTitle;
        }
        else if (settings.MatchWindowTitle)
        {
            gameRecord.WindowTitle = ourProc.WindowTitle;
        }
        else if (settings.MatchFilePath)
        {
            gameRecord.FilePath = ourProc.FilePath;
        }

        gameRecord.WindowRule = settings.WindowTitleRule;
        gameRecord.PathRule = settings.FilePathRule;

        DbFactory.GameLibrary.AddGame(gameRecord);

        try
        {
            var notified = await IpcClient.SendRefreshSignalForAutoGamesListAsync(IpcTarget.GameWatchGameMonitorAgent, cancellationToken);

            if (!notified)
                Console.WriteLine("⚠️ Game Monitor Agent is not running. " +
                                  "Only issue is that the newly added game won't probably get automatically monitored, " +
                                  "unless you start the agent. Though the database file was updated anyways.");

            Console.WriteLine("✅ Game added successfully");
        }
        catch (Exception)
        {
            Console.WriteLine("⚠ Failed to communicate with the Game Monitor Agent. " +
                              "Failed notify the agent to refresh the auto games list. " +
                              "This can cause your game to not be able to be automatically monitored, so please restart the agent!");
        }

        return 0;
    }
}