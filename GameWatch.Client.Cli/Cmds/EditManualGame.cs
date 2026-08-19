using System;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Core.Dbs;
using GameWatch.Core.Types;

namespace GameWatch.Client.Cli.Cmds;

public static class EditManualGame
{
    public static Task<Command> BuildAsync(CancellationToken callerCancellationToken)
    {
        var displayIdOption = new Option<int>("--id", "-i")
        {
            Description = "The game id from (see 'list games -m')",
            Required = true
        };

        var newGameNameOption = new Option<string>("--name", "-n")
        {
            Description = "New name for the game"
        };

        var playTimeOption = new Option<int?>("--playtime", "-p")
        {
            Description = "Set game playtime"
        };

        var cmd = new Command("manual", "Edit manual game")
        {
            displayIdOption,
            newGameNameOption,
            playTimeOption
        };
        cmd.Aliases.Add("m");

        cmd.SetAction(async (result, cliCt) =>
        {
            using var ctSrc = CancellationTokenSource.CreateLinkedTokenSource(callerCancellationToken, cliCt);
            var ct = ctSrc.Token;

            var displayId = new DisplayId(result.GetRequiredValue(displayIdOption));

            var tableIdResult = GameLibrary.Instance.GetTableId(GameMode.Manual, displayId);

            if (!tableIdResult.Ok || tableIdResult.TableId is null)
            {
                Console.WriteLine(tableIdResult.FailureReason);
                return 1;
            }

            var tableId = tableIdResult.TableId.Value;

            var manualGameResult = await GameLibrary.Instance.GetManualGameAsync(tableId, ct);

            if (!manualGameResult.Ok || manualGameResult.Game is null)
            {
                Console.WriteLine(manualGameResult.FailureReason);
                return 1;
            }

            var game = manualGameResult.Game;

            var newGameName = result.GetValue(newGameNameOption);
            var playTimeValue = result.GetValue(playTimeOption);

            if (newGameName is not null)
                game.Name = newGameName;

            if (playTimeValue is not null)
                game.PlayTimeSec = new ElapsedTime(playTimeValue.Value);

            var editedGameResult = await GameLibrary.Instance.EditGameAsync(game,
                                                                            tableId,
                                                                            ct,
                                                                            nameChanged: newGameName is not null,
                                                                            playTimeChanged: playTimeValue is not null);

            if (!editedGameResult.Ok)
            {
                Console.WriteLine(editedGameResult.FailureReason);
                return 1;
            }

            Console.WriteLine($"[OK] Game with Name='{game.Name}' edited successfully");

            return 0;
        });

        return Task.FromResult(cmd);
    }
}