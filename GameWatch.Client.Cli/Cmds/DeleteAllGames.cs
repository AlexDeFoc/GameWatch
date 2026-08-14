using System;
using System.CommandLine;
using GameWatch.Core.Agents.GameMonitor;
using GameWatch.Core.Dbs;
using GameWatch.Core.Ipc;
using GameWatch.Core.Wrappers;

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

            GameLibrary.DeleteAllGamesResult deleteAllGamesResult;
            if (clearManualGames)
            {
                deleteAllGamesResult = GameLibrary.Instance.DeleteAllGames(GameMode.Manual);

                if (!deleteAllGamesResult.Ok)
                {
                    Console.WriteLine(deleteAllGamesResult.FailureReason);
                    return 1;
                }

                Console.WriteLine("[OK] Deleted all manual games");
            }

            if (!clearAutoGames) return 0;

            deleteAllGamesResult = GameLibrary.Instance.DeleteAllGames(GameMode.Auto);

            if (!deleteAllGamesResult.Ok)
            {
                Console.WriteLine(deleteAllGamesResult.FailureReason);
                return 1;
            }

            Console.WriteLine("[OK] Deleted all auto games");

            var notificationResult = await IpcClient.SendRefreshSignalForAutoGamesListAsync(IpcTarget.GameWatchGameMonitorAgent,
                                                                                            cancellationToken);

            if (notificationResult.Ok) return 0;

            Console.WriteLine(notificationResult.FailureReason);
            return 1;
        });

        return cmd;
    }
}