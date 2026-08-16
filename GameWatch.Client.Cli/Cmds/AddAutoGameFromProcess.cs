using System;
using System.CommandLine;
using GameWatch.Core;
using GameWatch.Core.Dbs;
using GameWatch.Core.Helpers;
using GameWatch.Core.Types;

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

        var pidOption = new Option<int?>("--pid")
        {
            Description = "Target process PID from (see 'list procs')",
            Required = true
        };

        var playTimeOption = new Option<int>("--playtime", "-p")
        {
            Description = "Set initial playtime"
        };

        var ruleWindowExactOption = new Option<string>("--rule-window-exact", "-we")
        {
            Description = "Match rule: Require exact match on the target process window title"
        };

        var ruleWindowPatternOption = new Option<string>("--rule-window-pattern", "-wp")
        {
            Description = "Match rule: Pattern/regex to match against the process window title",
        };

        var rulePathExactOption = new Option<string>("--rule-path-exact", "-pe")
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

        cmd.Description = "You can add by providing a process pid and optionally matching filters, or only matching filters. Also if you don't provide any filters but do provide a pid, it will match exactly both the window title and the file path.";

        cmd.Validators.Add(result =>
        {
            var ruleWindowExact = result.GetValue(ruleWindowExactOption);
            var ruleWindowPattern = result.GetValue(ruleWindowPatternOption);

            if (ruleWindowExact is not null && ruleWindowPattern is not null)
            {
                result.AddError("[FAIL] Cannot specify both exact match and a title pattern. These options are mutually exclusive");
            }

            var rulePathExact = result.GetValue(rulePathExactOption);
            var rulePathPattern = result.GetValue(rulePathPatternOption);

            if (rulePathExact is not null && rulePathPattern is not null)
            {
                result.AddError("[FAIL] Cannot specify both exact match and a exe path pattern. These options are mutually exclusive");
            }

            if (rulePathPattern is not null && !RegexProcessor.IsValidPattern(rulePathPattern))
                result.AddError("[FAIL] Provided exe path match rule is not a valid regex pattern");

            if (ruleWindowPattern is not null && !RegexProcessor.IsValidPattern(ruleWindowPattern))
                result.AddError("[FAIL] Provided windows title match rule is not a valid regex pattern");

            var pid = result.GetValue(pidOption);

            ProcDto? proc = null;
            if (pid is not null)
                proc = ProcGatherer.GetOurProcFromPid(new Pid(pid.Value));

            if (proc is null && pid is not null)
            {
                result.AddError("[FAIL] Cannot find process with provided PID");
            }
        });

        cmd.SetAction(async (result, cancellationToken) =>
        {
            var pid = result.GetValue(pidOption);

            ProcDto? proc = null;

            if (pid is not null)
                proc = ProcGatherer.GetOurProcFromPid(new Pid(pid.Value));

            if (proc is null && pid is not null)
            {
                Console.WriteLine("[FAIL] Process with provided PID closed while processing it.");
                return 1;
            }

            var game = new AutoGameRecord
            {
                Name = result.GetRequiredValue(nameOption),
                PlayTimeSec = new ElapsedTime(result.GetValue(playTimeOption))
            };

            var ruleWindowExact = result.GetValue(ruleWindowExactOption);
            var ruleWindowPattern = result.GetValue(ruleWindowPatternOption);
            var rulePathExact = result.GetValue(rulePathExactOption);
            var rulePathPattern = result.GetValue(rulePathPatternOption);

            if (ruleWindowExact is null && ruleWindowPattern is null && rulePathExact is null && rulePathPattern is null && proc is not null)
            {
                game.WindowTitle = proc.WindowTitle;
                game.FilePath = proc.FilePath;
            }
            else
            {
                if (ruleWindowExact is not null)
                {
                    game.WindowTitle = proc is not null ? proc.WindowTitle : ruleWindowExact;
                }
                else if (ruleWindowPattern is not null)
                {
                    game.WindowRule = ruleWindowPattern;
                }

                if (rulePathExact is not null)
                {
                    game.FilePath = proc is not null ? proc.FilePath : rulePathExact;
                }
                else if (rulePathPattern is not null)
                {
                    game.PathRule = rulePathPattern;
                }
            }

            GameLibrary.Instance.AddGame(game);

            Console.WriteLine($"[OK] Game with Name='{game.Name}' added successfully");

            var notificationResult = await GameMonitorAgentIpcServer.RequestToRefreshAutoGamesCacheAsync(cancellationToken);

            if (notificationResult.Ok) return 0;

            Console.WriteLine(notificationResult.FailureReason);
            return 1;
        });

        return cmd;
    }
}