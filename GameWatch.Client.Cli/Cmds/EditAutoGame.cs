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

            var proc = ProcGatherer.GetOurProcFromPid(new ProcPid(pid.Value));

            if (proc is null)
            {
                result.AddError("[FAIL] Cannot find process with provided PID");
            }
        });

        cmd.SetAction(async (result, cliCt) =>
        {
            var displayId = new DisplayId(result.GetValue(displayIdOption));

            var tableIdResult = GameLibrary.Instance.GetTableId(GameMode.Auto, displayId);

            if (!tableIdResult.Ok || tableIdResult.TableId is null)
            {
                Console.WriteLine(tableIdResult.FailureReason);
                return 1;
            }

            var tableId = tableIdResult.TableId.Value;

            var autoGameResult = await GameLibrary.Instance.GetAutoGameAsync(tableId, cliCt);

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

            // 1. Basic properties update
            if (newGameName is not null)
                game.Name = newGameName;

            if (playTimeValue is not null)
                game.PlayTime = new ElapsedTime(playTimeValue.Value);

            // 2. Track option requests
            var titleExactRequested = matchTitleExact is not null;
            var titlePatternRequested = matchTitlePattern is not null;
            var pathExactRequested = matchPathExact is not null;
            var pathPatternRequested = matchPathPattern is not null;

            // 3. Detect if we need to clear an old opposite property in memory
            var clearWindowRule = titleExactRequested && game.WindowRule is not null;
            var clearWindowTitle = titlePatternRequested && game.WindowTitle is not null;
            var clearPathRule = pathExactRequested && game.PathRule is not null;
            var clearFilePath = pathPatternRequested && game.FilePath is not null;

            if (clearWindowRule) game.WindowRule = null;
            if (clearWindowTitle) game.WindowTitle = null;
            if (clearPathRule) game.PathRule = null;
            if (clearFilePath) game.FilePath = null;

            // 4. Assign new pattern rules if provided directly
            if (titlePatternRequested)
                game.WindowRule = matchTitlePattern;

            if (pathPatternRequested)
                game.PathRule = matchPathPattern;

            // 5. Query target process if PID or exact rules were provided
            if (pid is not null)
            {
                var targetProc = ProcGatherer.GetOurProcFromPid(new ProcPid(pid.Value));

                if (targetProc is null)
                {
                    Console.WriteLine("[FAIL] Cannot find process with provided pid");
                    return 1;
                }

                if (titleExactRequested)
                    game.WindowTitle = targetProc.WindowTitle;

                if (pathExactRequested)
                    game.FilePath = targetProc.FilePath;
            }

            // 6. Execute DB update (Pass true if setting a new value OR clearing an old value to NULL)
            var windowTitleChanged = titleExactRequested || clearWindowTitle;
            var windowRuleChanged = titlePatternRequested || clearWindowRule;
            var filePathChanged = pathExactRequested || clearFilePath;
            var pathRuleChanged = pathPatternRequested || clearPathRule;

            var editedGameResult = await GameLibrary.Instance.EditGameAsync(game,
                                                                            tableId,
                                                                            cliCt,
                                                                            nameChanged: newGameName is not null,
                                                                            playTimeChanged: playTimeValue is not null,
                                                                            windowTitleChanged: windowTitleChanged,
                                                                            windowRuleChanged: windowRuleChanged,
                                                                            filePathChanged: filePathChanged,
                                                                            pathRuleChanged: pathRuleChanged);

            if (!editedGameResult.Ok)
            {
                Console.WriteLine(editedGameResult.FailureReason);
                return 1;
            }

            Console.WriteLine($"[OK] Game with Name='{game.Name}' edited successfully");

            var matchingRulesChanged = playTimeValue is not null
                                       || newGameName is not null
                                       || windowTitleChanged
                                       || windowRuleChanged
                                       || filePathChanged
                                       || pathRuleChanged;

            var notificationResult = await GameMonitorAgentIpcServer.NotifyThatAutoGameGotEditedAsync(tableId,
                                                                                                      game.Name,
                                                                                                      matchingRulesChanged,
                                                                                                      cliCt);

            if (notificationResult.Ok) return 0;

            Console.WriteLine(notificationResult.FailureReason);
            return 1;
        });

        return cmd;
    }
}