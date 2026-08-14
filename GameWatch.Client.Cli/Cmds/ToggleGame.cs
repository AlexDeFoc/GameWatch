using System;
using System.CommandLine;
using GameWatch.Core;
using GameWatch.Core.Agents.GameMonitor;
using GameWatch.Core.Dbs;
using GameWatch.Core.Dto;
using GameWatch.Core.Ipc;
using GameWatch.Core.Wrappers;

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

        cmd.SetAction(async (parseResult, cancellationToken) =>
        {
            var displayId = new DisplayId(parseResult.GetValue(displayIdOption));
            var tableIdResult = GameLibrary.Instance.GetTableId(GameMode.Manual, displayId);

            if (!tableIdResult.Ok || tableIdResult.TableId is null)
            {
                Console.WriteLine(tableIdResult.FailureReason);
                return 1;
            }

            var tableId = tableIdResult.TableId.Value;

            var manualGameResult = GameLibrary.Instance.GetManualGame(tableId, displayId);

            if (!manualGameResult.Ok || manualGameResult.Game is null)
            {
                Console.WriteLine(manualGameResult.FailureReason);
                return 1;
            }

            var game = manualGameResult.Game;

            var notificationResult = await IpcClient.SendToggleManualGameSignalAsync(IpcTarget.GameWatchGameMonitorAgent,
                                                                                     tableId,
                                                                                     cancellationToken);

            if (notificationResult.Ok)
            {
                Console.WriteLine(notificationResult.StartedGame
                                      ? $"[OK] Game with Name='{game.Name}' started"
                                      : $"[OK] Game with Name='{game.Name}' stopped");

                return 0;
            }

            Console.WriteLine(notificationResult.FailureReason);
            return 1;
        });

        return cmd;
    }
}