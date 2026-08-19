using System;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Core.Helpers;

namespace GameWatch.Client.Cli.Cmds;

public static class ListProcs
{
    public static Task<Command> BuildAsync(CancellationToken callerCancellationToken)
    {
        var cmd = new Command("procs", "List all active processes with properties" +
                                       "that can be used to match against when monitoring auto games.");
        cmd.Aliases.Add("p");

        cmd.SetAction(async (_, cliCt) =>
        {
            using var ctSrc = CancellationTokenSource.CreateLinkedTokenSource(callerCancellationToken, cliCt);
            var ct = ctSrc.Token;

            await foreach (var proc in ProcGatherer.StreamAvailableProcessesAsync(ct))
            {
                Console.WriteLine($"Pid: {proc.Pid}");
                Console.WriteLine($"Window Title: {proc.WindowTitle}");
                Console.WriteLine($"File Path: {proc.FilePath}");
                Console.WriteLine();
            }

            return 0;
        });

        return Task.FromResult(cmd);
    }
}