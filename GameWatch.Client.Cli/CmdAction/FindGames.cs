using System;
using System.Threading;
using Spectre.Console.Cli;

namespace GameWatch.Client.Cli.CmdAction;

public sealed class FindGames : Command<CmdCfg.FindGames>
{
    protected override int Execute(CommandContext context, CmdCfg.FindGames settings, CancellationToken cancellationToken)
    {
        var procs = Helpers.ProcessFinder.GetListOfAvailableProcesses();

        foreach (var proc in procs)
        {
            Console.WriteLine($"Pid: {proc.Pid}");
            Console.WriteLine($"Window Title: {proc.WindowTitle}");
            Console.WriteLine($"Process Name: {proc.ProcName}");
            Console.WriteLine($"FilePath: {proc.FilePath}");
            Console.WriteLine();
        }

        return 0;
    }
}