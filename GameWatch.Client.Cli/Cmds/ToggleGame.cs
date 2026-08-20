using System;
using System.CommandLine;
using GameWatch.Core;
using GameWatch.Core.Dbs;
using GameWatch.Core.Types;

namespace GameWatch.Client.Cli.Cmds;

public static class ToggleGame
{
    public static Command Build()
    {
        var displayIdOption = new Option<int>("--id", "-i")
        {
            Description = "The manual game id from (see 'list games -m')",
            Required = true
        };

        var cmd = new Command("toggle", "Start or stop a certain manual game record")
        {
            displayIdOption
        };
        cmd.Aliases.Add("tg");

        cmd.SetAction(async (parseResult, cliCt) =>
        {
            var displayId = new DisplayId(parseResult.GetValue(displayIdOption));
            var tableIdResult = GameLibrary.Instance.GetTableId(GameMode.Manual, displayId);

            if (!tableIdResult.Ok || tableIdResult.TableId is null)
            {
                Console.WriteLine(tableIdResult.FailureReason);
                return 1;
            }

            var tableId = tableIdResult.TableId.Value;

            var manualGameResult = await GameLibrary.Instance.GetManualGameAsync(tableId, cliCt);

            if (!manualGameResult.Ok || manualGameResult.Game is null)
            {
                Console.WriteLine(manualGameResult.FailureReason);
                return 1;
            }

            var game = manualGameResult.Game;
            var gameName = game.Name;

            var notificationResult = await GameMonitorAgentIpcServer.RequestToToggleManualGameAsync(tableId,
                                                                                                    gameName,
                                                                                                    cliCt);

            if (notificationResult.Ok)
            {
                Console.WriteLine(notificationResult.StartedGame
                                      ? $"[OK] Game with Name='{gameName}' started"
                                      : $"[OK] Game with Name='{gameName}' stopped");

                return 0;
            }

            Console.WriteLine(notificationResult.FailureReason);
            return 1;
        });

        return cmd;
    }
}