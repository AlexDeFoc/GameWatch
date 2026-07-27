using System;
using System.Threading;
using Spectre.Console.Cli;
// ReSharper disable ClassNeverInstantiated.Global

namespace GameWatch.Client.Cli.Cmds;

public sealed class ListProcs : Command<ListProcs.Settings>
{
    public class Settings : CommandSettings;

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var procs = Helpers.ProcessFinder.GetListOfAvailableProcesses();

        foreach (var proc in procs)
        {
            Console.WriteLine($"Pid: {proc.Pid}");
            Console.WriteLine($"Window Title: {proc.WindowTitle}");
            Console.WriteLine($"FilePath: {proc.FilePath}");
            Console.WriteLine();
        }

        return 0;
    }
}