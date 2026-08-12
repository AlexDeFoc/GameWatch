using System;
using System.CommandLine;
using GameWatch.Core;
using GameWatch.Core.Agents.GameMonitor;
using GameWatch.Core.Dbs;
using GameWatch.Core.Dto;
using GameWatch.Core.Ipc;

namespace GameWatch.Client.Cli.Cmds;

public static class DeleteAllGames
{
    public static Command Build()
    {
        var manualOption = new Option<bool>("--manual", "-m")
        {
            Description = "Delete all manual games"
        };

        var autoOption = new Option<bool>("--auto", "-a")
        {
            Description = "Delete all auto games"
        };

        var cmd = new Command("clear", "Remove all games of a certain mode")
        {
            manualOption,
            autoOption
        };
        cmd.Aliases.Add("cl");

        cmd.SetAction(async (result, cancellationToken) =>
        {
            var clearManualGames = result.GetValue(manualOption);
            var clearAutoGames = result.GetValue(autoOption);

            if (!clearAutoGames && !clearManualGames)
            {
                clearManualGames = true;
                clearAutoGames = true;
            }

            GameLibrary.DeleteAllGamesActionStatus? status;
            if (clearManualGames)
            {
                status = GameLibrary.Instance.DeleteAllGames(GameMode.Manual);

                if (!status.HasSucceeded)
                {
                    Console.WriteLine(status.FailureReason);
                    return 1;
                }

                Console.WriteLine("[OK] Deleted all manual games");
            }

            if (!clearAutoGames) return 0;

            status = GameLibrary.Instance.DeleteAllGames(GameMode.Auto);

            if (!status.HasSucceeded)
            {
                Console.WriteLine(status.FailureReason);
                return 1;
            }

            Console.WriteLine("[OK] Deleted all auto games");

            const IpcTarget target = IpcTarget.GameWatchGameMonitorAgent;
            try
            {
                var notified = await IpcClient.SendRefreshSignalForAutoGamesListAsync(target, cancellationToken);

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