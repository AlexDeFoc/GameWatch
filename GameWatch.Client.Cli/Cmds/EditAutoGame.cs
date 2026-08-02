using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Agent.GameMonitor;
using GameWatch.Core.Agents.GameMonitor;
using GameWatch.Core.Dto;
using GameWatch.Core.Helpers;
using GameWatch.Core.Ipc;
using Spectre.Console.Cli;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace GameWatch.Client.Cli.Cmds;

public sealed class EditAutoGame : AsyncCommand<EditAutoGame.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-i|--idx <GAME_INDEX>", isRequired: true)]
        [Description("The Game index. TIP: Can be gathered from 'list games'")]
        public required int GameIdx { get; init; }

        [CommandOption("-n|--name <NAME>")]
        [Description("New name for the Game record")]
        public string? Name { get; init; }

        [CommandOption("--pid <PROCESS_ID>")]
        [Description("The process id of a different game. If not specified will use the original game process.")]
        public int? DifferentPid { get; init; }

        [CommandOption("-p|--playtime <SECONDS>")]
        [Description("Forced Game record playtime in seconds")]
        public int? PlayTimeSeconds { get; init; }

        [CommandOption("--match-title")]
        [Description("Match against the process window title")]
        public bool MatchWindowTitle { get; init; }

        [CommandOption("--match-path")]
        [Description("Match against the process file path")]
        public bool MatchFilePath { get; init; }

        [CommandOption("--title-rule <REGEX_PATTERN>")]
        [Description("Regex pattern to match against the process window title")]
        public string? WindowTitleRule { get; init; }

        [CommandOption("--path-rule <REGEX_PATTERN>")]
        [Description("Regex pattern to match against the process file path")]
        public string? FilePathRule { get; init; }

        [CommandOption("--clear-title")]
        [Description("Stop matching against the process window title")]
        public bool ClearWindowTitle { get; init; }

        [CommandOption("--clear-path")]
        [Description("Stop matching against the process file path")]
        public bool ClearFilePath { get; init; }

        [CommandOption("--clear-title-rule")]
        [Description("Stop matching the process window title against the regex pattern")]
        public bool ClearTitleRule { get; init; }

        [CommandOption("--clear-path-rule")]
        [Description("Stop matching the process file path against the regex pattern")]
        public bool ClearPathRule { get; init; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var gameIdx = new GameIdx(settings.GameIdx);
        var (hasSucceeded, game, failureReason) = DbFactory.GameLibrary.GetAutoGameByIdx(gameIdx);
        var wasGameActive = false;

        if (!hasSucceeded || game == null)
        {
            Console.WriteLine(failureReason);
            return 1;
        }

        switch (settings)
        {
            // Validate
            case { MatchWindowTitle: true, WindowTitleRule: not null }:
                Console.WriteLine("⛔ Matching fully and partially against the window title is not allowed. Ignoring command...");
                return 1;
            case { MatchFilePath: true, FilePathRule: not null }:
                Console.WriteLine("⛔ Matching fully and partially against the file path is not allowed. Ignoring command...");
                return 1;
            case { MatchWindowTitle: true, ClearTitleRule: false } when game.WindowRule != null:
                Console.WriteLine("⛔ Matching fully and partially against the window title is not allowed, without clearing the window title pattern/rule first! Ignoring command...");
                return 1;
            case { MatchFilePath: true, ClearPathRule: false } when game.PathRule != null:
                Console.WriteLine("⛔ Matching fully and partially against the file path is not allowed, without clearing the file path pattern/rule first! Ignoring command...");
                return 1;
            case { WindowTitleRule: not null, ClearWindowTitle: false } when game.WindowTitle != null:
                Console.WriteLine("⛔ Matching fully and partially against the window title is not allowed, without clearing the window title fully match first! Ignoring command...");
                return 1;
            case { FilePathRule: not null, ClearFilePath: false } when game.FilePath != null:
                Console.WriteLine("⛔ Matching fully and partially against the file path is not allowed, without clearing the file path fully match first! Ignoring command...");
                return 1;
        }

        // Rest of work
        var gameName = settings.Name ?? game.Name;
        var gamePlayTimeSec = settings.PlayTimeSeconds == null ? game.PlayTimeSec : new ElapsedTime(settings.PlayTimeSeconds.Value);
        var gameWindowRule = settings.WindowTitleRule ?? (settings.ClearTitleRule ? null : game.WindowRule);
        var gamePathRule = settings.FilePathRule ?? (settings.ClearPathRule ? null : game.PathRule);

        OurProc? matchingProc = null;
        if (settings.MatchWindowTitle || settings.MatchFilePath)
        {
            wasGameActive = true;

            if (settings.DifferentPid == null)
            {
                var procList = ProcessFinder.GetListOfAvailableProcesses();
                matchingProc = procList.FirstOrDefault(proc => RuleMatcher.IsMatch(proc, game));
            }
            else
            {
                matchingProc = ProcessFinder.GetOurProcFromPid(settings.DifferentPid.Value);
            }

            if (matchingProc == null)
            {
                Console.WriteLine("⛔ Target Game is not active! Before editing it to match against its Window Title or File Path, open the Game first. Ignoring command...");
                return 1;
            }
        }

        var procWindowTitle = settings.ClearWindowTitle
            ? null
            : settings.MatchWindowTitle
                ? matchingProc!.WindowTitle
                : game.WindowTitle;

        var procFilePath = settings.ClearFilePath
            ? null
            : settings.MatchFilePath
                ? matchingProc!.FilePath
                : game.FilePath;

        var result = DbFactory.GameLibrary.ChangeGameProperty(GameMode.Auto,
                                                              new GameIdx(settings.GameIdx),
                                                              name: gameName,
                                                              playTimeSec: gamePlayTimeSec,
                                                              windowTitle: procWindowTitle,
                                                              filePath: procFilePath,
                                                              windowRule: gameWindowRule,
                                                              pathRule: gamePathRule
        );

        if (!result.HasSucceeded)
        {
            Console.WriteLine(result.FailureReason);

            return 1;
        }

        try
        {
            var notified = await IpcClient.SendEditAutoGameSignalAsync(IpcTarget.GameWatchGameMonitorAgent, gameIdx, wasGameActive, cancellationToken);

            if (!notified)
                Console.WriteLine("⚠️ Game Monitor Agent is not running. " +
                                  "Only issue is that the edited added game won't probably get automatically monitored, " +
                                  "unless you start the agent. Though the database file was updated anyways.");

            Console.WriteLine("✅ Game edited successfully");
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