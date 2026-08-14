using System;
using System.CommandLine;
using GameWatch.Core.Agents.GameMonitor;
using GameWatch.Core.Dbs;
using GameWatch.Core.Ipc;
using GameWatch.Core.Wrappers;

namespace GameWatch.Client.Cli.Cmds;

public static class RemoveGame
{
    public static Command Build()
    {
        var displayIdOption = new Option<int>("--id", "-i")
        {
            Description = "The game id from (see 'list games')",
            Required = true
        };

        var manualOption = new Option<bool>("--manual", "-m")
        {
            Description = "Should delete manual game"
        };

        var autoOption = new Option<bool>("--auto", "-a")
        {
            Description = "Should delete auto game"
        };

        var cmd = new Command("remove", "Remove game")
        {
            displayIdOption,
            manualOption,
            autoOption
        };
        cmd.Aliases.Add("rm");

        cmd.Validators.Add(result =>
        {
            var removeManual = result.GetValue(manualOption);
            var removeAuto = result.GetValue(autoOption);

            switch (removeManual)
            {
                case false when !removeAuto:
                    result.AddError("[FAIL] Must specify whether to remove a manual or auto game");
                    return;
                case true when removeAuto:
                    result.AddError("[FAIL] Cannot remove a manual and auto game with the same index");
                    return;
            }
        });

        cmd.SetAction(async (result, cancellationToken) =>
        {
            var removeManual = result.GetValue(manualOption);
            var gameMode = removeManual ? GameMode.Manual : GameMode.Auto;

            var displayId = new DisplayId(result.GetRequiredValue(displayIdOption));
            var tableIdResult = GameLibrary.Instance.GetTableId(GameMode.Manual, displayId);

            if (!tableIdResult.Ok || tableIdResult.TableId is null)
            {
                Console.WriteLine(tableIdResult.FailureReason);
                return 1;
            }

            var tableId = tableIdResult.TableId.Value;

            var deleteGameResult = GameLibrary.Instance.DeleteGame(gameMode, tableId, displayId);

            if (!deleteGameResult.Ok)
            {
                Console.WriteLine(deleteGameResult.FailureReason);
                return 1;
            }

            Console.WriteLine($"[OK] Game with Name='{deleteGameResult.GameName}' deleted");

            const IpcTarget ipcTarget = IpcTarget.GameWatchGameMonitorAgent;
            var notificationResult = removeManual
                ? await IpcClient.NotifyAboutManualGameRemovalAsync(ipcTarget,
                                                                    tableId,
                                                                    cancellationToken)
                : await IpcClient.NotifyAboutAutoGameRemovalAsync(ipcTarget,
                                                                  tableId,
                                                                  cancellationToken);

            if (notificationResult.Ok) return 0;

            Console.WriteLine(notificationResult.FailureReason);
            return 1;
        });

        return cmd;
    }
}