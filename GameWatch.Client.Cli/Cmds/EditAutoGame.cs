using System;
using System.CommandLine;
using GameWatch.Core;
using GameWatch.Core.Dbs;
using GameWatch.Core.Helpers;
using GameWatch.Core.Types;

namespace GameWatch.Client.Cli.Cmds;

public static class EditAutoGame
{
    public static Command Build()
    {
        var displayIdOption = new Option<int>("--id", "-i")
        {
            Description = "The game id from (see 'list games -a')",
            Required = true
        };

        var gameNameOption = new Option<string>("--name", "-n")
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

        var matchTitleExactOption = new Option<string>("--rule-title-exact", "-te")
        {
            Description = "Match rule: Will match, value, fully against process window title"
        };

        var matchTitlePatternOption = new Option<string>("--rule-title-pattern", "-tp")
        {
            Description = "Match rule: Will match, value as regex pattern, against process window title",
        };

        var matchPathExactOption = new Option<string>("--rule-path-exact", "-pe")
        {
            Description = "Match rule: Will match, value, fully against process file path"
        };

        var matchPathPatternOption = new Option<string>("--rule-path-pattern", "-pp")
        {
            Description = "Match rule: Will match, value as regex pattern, against process file path"
        };

        var cmd = new Command("auto", "Edit auto game")
        {
            displayIdOption,
            gameNameOption,
            playTimeOption,
            pidOption,
            matchTitleExactOption,
            matchTitlePatternOption,
            matchPathExactOption,
            matchPathPatternOption
        };
        cmd.Aliases.Add("a");

        cmd.Validators.Add(result =>
        {
            var matchTitleExact = result.GetValue(matchTitleExactOption);
            var matchTitlePattern = result.GetValue(matchTitlePatternOption);

            if (matchTitleExact is not null && matchTitlePattern is not null)
            {
                result.AddError("[FAIL] Cannot specify both exact match and pattern match against process window title. These options are mutually exclusive");
            }

            var matchPathExact = result.GetValue(matchPathExactOption);
            var matchPathPattern = result.GetValue(matchPathPatternOption);

            if (matchPathExact is not null && matchPathPattern is not null)
            {
                result.AddError("[FAIL] Cannot specify both exact match and pattern match against process file path. These options are mutually exclusive");
            }

            var pid = result.GetValue(pidOption);

            if (pid is null) return;

            var proc = ProcGatherer.GetOurProcFromPid(new Pid(pid.Value));

            if (proc is null)
            {
                result.AddError("[FAIL] Cannot find process with provided PID");
            }
        });

        cmd.SetAction(async (result, cancellationToken) =>
        {
            var displayId = new DisplayId(result.GetValue(displayIdOption));

            var tableIdResult = GameLibrary.Instance.GetTableId(GameMode.Auto, displayId);

            if (!tableIdResult.Ok || tableIdResult.TableId is null)
            {
                Console.WriteLine(tableIdResult.FailureReason);
                return 1;
            }

            var tableId = tableIdResult.TableId.Value;

            var autoGameResult = GameLibrary.Instance.GetAutoGame(tableId);

            if (!autoGameResult.Ok || autoGameResult.Game is null)
            {
                Console.WriteLine(autoGameResult.FailureReason);
                return 1;
            }

            var game = autoGameResult.Game;

            var pid = result.GetValue(pidOption);
            var newGameName = result.GetValue(gameNameOption);
            var matchTitleExact = result.GetValue(matchTitleExactOption);
            var matchTitlePattern = result.GetValue(matchTitlePatternOption);
            var matchPathExact = result.GetValue(matchPathExactOption);
            var matchPathPattern = result.GetValue(matchPathPatternOption);
            var playTimeValue = result.GetValue(playTimeOption);

            if (newGameName is not null)
                game.Name = newGameName;

            if (playTimeValue is not null)
                game.PlayTimeSec = new ElapsedTime(playTimeValue.Value);

            if (matchTitleExact is not null && game.WindowRule is not null)
                game.WindowRule = null;
            else if (matchTitlePattern is not null && game.WindowTitle is not null)
                game.WindowTitle = null;

            if (matchPathExact is not null && game.PathRule is not null)
                game.PathRule = null;
            else if (matchPathPattern is not null && game.PathRule is not null)
                game.FilePath = null;

            if (matchTitlePattern is not null)
                game.WindowRule = matchTitlePattern;

            if (matchPathPattern is not null)
                game.PathRule = matchPathPattern;

            if (pid is not null)
            {
                var targetProc = ProcGatherer.GetOurProcFromPid(new Pid(pid.Value));

                if (targetProc is null)
                {
                    Console.WriteLine("[FAIL] Cannot find process with provided pid");
                    return 1;
                }

                if (matchTitleExact is not null)
                    game.WindowTitle = targetProc.WindowTitle;

                if (matchPathExact is not null)
                    game.FilePath = targetProc.FilePath;
            }

            var editedGameResult = GameLibrary.Instance.EditGame(game,
                                                                 tableId,
                                                                 playTimeChanged: playTimeValue is not null,
                                                                 nameChanged: newGameName is null,
                                                                 windowTitleChanged: matchTitleExact is not null,
                                                                 windowRuleChanged: matchTitlePattern is not null,
                                                                 filePathChanged: matchPathExact is not null,
                                                                 pathRuleChanged: matchPathPattern is not null);

            if (!editedGameResult.Ok)
            {
                Console.WriteLine(editedGameResult.FailureReason);
                return 1;
            }

            Console.WriteLine($"[OK] Game with Name='{game.Name}' edited successfully");

            var matchingRulesChanged = playTimeValue is not null
                                       || newGameName is not null
                                       || matchTitleExact is not null
                                       || matchTitlePattern is not null
                                       || matchPathExact is not null
                                       || matchPathPattern is not null;

            var notificationResult = await GameMonitorAgentIpcServer.NotifyThatAutoGameGotEditedAsync(tableId,
                                                                                                      game.Name,
                                                                                                      matchingRulesChanged,
                                                                                                      cancellationToken);

            if (notificationResult.Ok) return 0;

            Console.WriteLine(notificationResult.FailureReason);
            return 1;
        });

        return cmd;
    }
}