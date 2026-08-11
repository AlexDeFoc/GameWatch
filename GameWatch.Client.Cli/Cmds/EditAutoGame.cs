// TODO: Change window exact and path exact to be string? so that they can be just raw strings without needing to specify the pid and having the exact pid

using System;
using System.CommandLine;
using System.Linq;
using GameWatch.Core;
using GameWatch.Core.Agents.GameMonitor;
using GameWatch.Core.Dbs;
using GameWatch.Core.Dto;
using GameWatch.Core.Ipc;
using GameWatch.Core.Wrappers;

namespace GameWatch.Client.Cli.Cmds;

public static class EditAutoGame
{
    public static Command Build()
    {
        var idxOption = new Option<int>("--id", "-i")
        {
            Description = "The game id from (see 'list games -a')",
            Required = true
        };

        var nameOption = new Option<string>("--name", "-n")
        {
            Description = "New name for the game"
        };

        var playTimeOption = new Option<int?>("--playtime", "-p")
        {
            Description = "Set game playtime"
        };

        var pidOption = new Option<int?>("--pid")
        {
            Description = "Target process PID from (see 'list procs'). Provide only when you are targeting a different process then the original one"
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

        var cmd = new Command("auto", "Edit auto game")
        {
            idxOption,
            nameOption,
            playTimeOption,
            pidOption,
            ruleWindowExactOption,
            ruleWindowPatternOption,
            rulePathExactOption,
            rulePathPatternOption
        };
        cmd.Aliases.Add("a");

        cmd.Validators.Add(result =>
        {
            var ruleWindowExact = result.GetValue(ruleWindowExactOption);
            var ruleWindowPattern = result.GetValue(ruleWindowPatternOption);

            if (ruleWindowExact && ruleWindowPattern is not null)
            {
                result.AddError("[FAIL] Cannot specify both exact match and a title pattern. These options are mutually exclusive");
            }

            var rulePathExact = result.GetValue(rulePathExactOption);
            var rulePathPattern = result.GetValue(rulePathPatternOption);

            if (rulePathExact && rulePathPattern is not null)
            {
                result.AddError("[FAIL] Cannot specify both exact match and a exe path pattern. These options are mutually exclusive");
            }

            var pid = result.GetRequiredValue(pidOption);

            if (pid is null) return;

            var proc = ProcGatherer.GetOurProcFromPid(new Pid(pid.Value));

            if (proc is null)
            {
                result.AddError("[FAIL] Cannot find process with provided PID");
            }
        });

        cmd.SetAction(async (result, cancellationToken) =>
        {
            var gameIdx = new GameIdx(result.GetValue(idxOption) - 1);
            var (hasSucceeded, game, failureReason) = GameLibrary.Instance.GetAutoGameByIdx(gameIdx);

            if (!hasSucceeded || game == null)
            {
                Console.WriteLine(failureReason);
                return 1;
            }

            var pid = result.GetValue(pidOption);
            var ruleWindowExact = result.GetValue(ruleWindowExactOption);
            var rulePathExact = result.GetValue(rulePathExactOption);
            var ruleWindowPattern = result.GetValue(ruleWindowPatternOption);
            var rulePathPattern = result.GetValue(rulePathPatternOption);
            var playTimeGotten = result.GetValue(playTimeOption);

            var gameName = result.GetValue(nameOption) ?? game.Name;
            var gamePlayTime = playTimeGotten is null ? game.PlayTimeSec : new ElapsedTime(playTimeGotten.Value);
            OurProc? targetProc = null;

            if (ruleWindowExact || rulePathExact)
            {
                targetProc = pid is null
                    ? ProcGatherer.GetListOfAvailableProcesses().FirstOrDefault(proc => RuleMatcher.IsMatch(proc, game))
                    : ProcGatherer.GetOurProcFromPid(new Pid(pid.Value));

                if (targetProc is null)
                {
                    Console.WriteLine("[FAIL] Cannot set the matching rule to be window title exact or exe path exact without the game being active!");
                    return 1;
                }
            }

            var shouldClearWindowTitle = ruleWindowPattern is not null && game.WindowTitle is not null;
            var shouldClearFilePath = rulePathPattern is not null && game.FilePath is not null;

            var procWindowTitle = shouldClearWindowTitle
                ? null
                : ruleWindowExact
                    ? targetProc!.WindowTitle
                    : game.WindowTitle;

            var procFilePath = shouldClearFilePath
                ? null
                : rulePathExact
                    ? targetProc!.FilePath
                    : game.FilePath;

            var status = GameLibrary.Instance.ChangeGameProperty(GameMode.Auto,
                                                                  game.Id,
                                                                  gameName,
                                                                  gamePlayTime,
                                                                  procWindowTitle,
                                                                  procFilePath,
                                                                  ruleWindowPattern,
                                                                  rulePathPattern
            );

            if (!status.HasSucceeded)
            {
                Console.WriteLine(status.FailureReason);
                return 1;
            }

            Console.WriteLine($"[OK] Game with Name='{gameName}' edited successfully");

            const IpcTarget target = IpcTarget.GameWatchGameMonitorAgent;
            try
            {
                var notified = await IpcClient.SendEditAutoGameSignalAsync(target, gameIdx, cancellationToken);

                if (!notified)
                {
                    Console.WriteLine("[WARN] Unable to communicate with the GameWatch background service. Please ensure the agent is running.");
                    return 1;
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[WARN] Operation canceled.");
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FAIL] Unhandled exception during IPC call to target '{nameof(target)}': {ex}");
                return 1;
            }

            return 0;
        });

        return cmd;
    }
}