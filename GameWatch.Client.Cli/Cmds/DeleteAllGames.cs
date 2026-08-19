using System;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Core;
using GameWatch.Core.Dbs;
using GameWatch.Core.Types;

namespace GameWatch.Client.Cli.Cmds;

public static class DeleteAllGames
{
    public static Task<Command> BuildAsync(CancellationToken callerCancellationToken)
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

        cmd.SetAction(async (result, cliCt) =>
        {
            using var ctSrc = CancellationTokenSource.CreateLinkedTokenSource(callerCancellationToken, cliCt);
            var ct = ctSrc.Token;

            var clearManualGames = result.GetValue(manualOption);
            var clearAutoGames = result.GetValue(autoOption);

            if (clearManualGames)
            {
                var res = await GameLibrary.Instance.DeleteAllGamesAsync(GameMode.Manual, ct);
                if (!res.Ok) { Console.WriteLine(res.FailureReason); return 1; }
                Console.WriteLine("[OK] Deleted all manual games");
            }

            if (clearAutoGames)
            {
                var res = await GameLibrary.Instance.DeleteAllGamesAsync(GameMode.Auto, ct);
                if (!res.Ok) { Console.WriteLine(res.FailureReason); return 1; }
                Console.WriteLine("[OK] Deleted all auto games");
            }

            var notificationResult = await GameMonitorAgentIpcServer.RequestToStopTrackingAllGamesAsync(clearAutoGames, clearManualGames, ct);
            if (notificationResult.Ok) return 0;
            Console.WriteLine(notificationResult.FailureReason);

            return 1;
        });

        return Task.FromResult(cmd);
    }
}