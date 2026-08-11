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
        var idOption = new Option<int>("--id", "-i")
        {
            Description = "The game id from (see 'list games')",
            Required = true
        };

        var manualOption = new Option<bool>("--manual", "-m")
        {
            Description = "Sets whether provided game id corresponds to a manual game"
        };

        var autoOption = new Option<bool>("--auto", "-a")
        {
            Description = "Sets whether provided game id corresponds to an auto game"
        };

        var cmd = new Command("remove", "Remove game")
        {
            idOption,
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
            var shouldRemoveManualGame = result.GetValue(manualOption);
            var gameMode = shouldRemoveManualGame ? GameMode.Manual : GameMode.Auto;
            var gameIdx = new GameIdx(result.GetRequiredValue(idOption) - 1);

            var (hasSucceeded, gameId, deletedGameTitle, failureReason) = GameLibrary.Instance.DeleteGame(gameMode, gameIdx);

            if (!hasSucceeded || gameId is null)
            {
                Console.WriteLine(failureReason);
                return 1;
            }

            Console.WriteLine($"[OK] Game with Name='{deletedGameTitle}' deleted");

            const IpcTarget target = IpcTarget.GameWatchGameMonitorAgent;
            try
            {
                var notified = shouldRemoveManualGame
                    ? await IpcClient.SendRemoveManualGameSignalAsync(target, gameId.Value, cancellationToken)
                    : await IpcClient.SendRemoveAutoGameSignalAsync(target, gameId.Value, cancellationToken);

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