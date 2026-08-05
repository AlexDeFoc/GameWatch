using System;
using System.CommandLine;
using GameWatch.Core;
using GameWatch.Core.Agents.GameMonitor;
using GameWatch.Core.Dto;
using GameWatch.Core.GameRecords;
using GameWatch.Core.Ipc;

namespace GameWatch.Client.Cli.Cmds;

public static class AddAutoGameFromProcess
{
    public static Command Build()
    {
        var nameOption = new Option<string>("--name", "-n")
        {
            Description = "The name of the game record",
            Required = true
        };

        var pidOption = new Option<int>("--pid")
        {
            Description = "Target process PID from (see 'list procs')",
            Required = true
        };

        var playTimeOption = new Option<int>("--playtime", "-p")
        {
            Description = "Set initial playtime"
        };

        var ruleWindowExactOption = new Option<bool>("--rule-window-exact", "-we")
        {
            Description = "Match rule: Require exact match on the target process window title"
        };

        var ruleWindowPatternOption = new Option<string>("--rule-window-pattern", "-wp")
        {
            Description = "Match rule: Pattern/regex to match against the process window title",
        };

        var rulePathExactOption = new Option<bool>("--rule-path-exact", "-pe")
        {
            Description = "Match rule: Require exact match on the target process executable path"
        };

        var rulePathPatternOption = new Option<string>("--rule-path-pattern", "-pp")
        {
            Description = "Match rule: Pattern/regex to match against the target process executable path"
        };

        var cmd = new Command("proc", "Add auto game from a process")
        {
            nameOption,
            pidOption,
            playTimeOption,
            ruleWindowExactOption,
            ruleWindowPatternOption,
            rulePathExactOption,
            rulePathPatternOption
        };

        cmd.Validators.Add(result =>
        {
            var ruleWindowExact = result.GetValue(ruleWindowExactOption);
            var ruleWindowPattern = result.GetValue(ruleWindowPatternOption);

            if (ruleWindowExact && ruleWindowPattern is not null)
            {
                result.AddError("⛔ Cannot specify both exact match and a title pattern. These options are mutually exclusive");
            }

            var rulePathExact = result.GetValue(rulePathExactOption);
            var rulePathPattern = result.GetValue(rulePathPatternOption);

            if (rulePathExact && rulePathPattern is not null)
            {
                result.AddError("⛔ Cannot specify both exact match and a exe path pattern. These options are mutually exclusive");
            }

            if (rulePathPattern is not null && !RegexProcessor.IsValidPattern(rulePathPattern))
                result.AddError("⛔ Provided exe path match rule is not a valid regex pattern");

            if (ruleWindowPattern is not null && !RegexProcessor.IsValidPattern(ruleWindowPattern))
                result.AddError("⛔ Provided windows title match rule is not a valid regex pattern");

            var pid = result.GetRequiredValue(pidOption);

            var proc = ProcGatherer.GetOurProcFromPid(new Pid(pid));

            if (proc is null)
            {
                result.AddError("⛔ Cannot find process with provided PID");
            }
        });

        cmd.SetAction(async (result, cancellationToken) =>
        {
            var pid = result.GetRequiredValue(pidOption);

            var proc = ProcGatherer.GetOurProcFromPid(new Pid(pid));

            if (proc is null)
            {
                Console.WriteLine("⛔ Process with provided PID closed while processing it.");
                return 1;
            }

            var game = new AutoGame()
            {
                Name = result.GetRequiredValue(nameOption),
                PlayTimeSec = new ElapsedTime(result.GetValue(playTimeOption))
            };

            var ruleWindowExact = result.GetValue(ruleWindowExactOption);
            var ruleWindowPattern = result.GetValue(ruleWindowPatternOption);
            var rulePathExact = result.GetValue(rulePathExactOption);
            var rulePathPattern = result.GetValue(rulePathPatternOption);

            if (!ruleWindowExact && ruleWindowPattern is null && !rulePathExact && rulePathPattern is null)
            {
                game.WindowTitle = proc.WindowTitle;
                game.FilePath = proc.FilePath;
            }
            else
            {
                if (ruleWindowExact)
                {
                    game.WindowTitle = proc.WindowTitle;
                }
                else if (ruleWindowPattern is not null)
                {
                    game.WindowRule = ruleWindowPattern;
                }

                if (rulePathExact)
                {
                    game.FilePath = proc.FilePath;
                }
                else if (rulePathPattern is not null)
                {
                    game.PathRule = rulePathPattern;
                }
            }

            DbMng.GameLibrary.AddGame(game);

            Console.WriteLine("✅ Game added successfully");

            const IpcTarget target = IpcTarget.GameWatchGameMonitorAgent;
            try
            {
                var notified = await IpcClient.SendRefreshSignalForAutoGamesListAsync(target, cancellationToken);

                if (!notified)
                {
                    Console.WriteLine("⚠ Unable to communicate with the GameWatch background service. Please ensure the agent is running.");
                    return 1;
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("⚠ Operation canceled.");
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⛔ Unhandled exception during IPC call to target '{nameof(target)}': {ex}");
                return 1;
            }

            return 0;
        });

        return cmd;
    }
}