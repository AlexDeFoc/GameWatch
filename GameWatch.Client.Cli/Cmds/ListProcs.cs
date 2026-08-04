using System;
using System.CommandLine;

namespace GameWatch.Client.Cli.Cmds;

public static class ListProcs
{
    public static Command Build()
    {
        var cmd = new Command("procs", "List all active processes with properties" +
                                       "that can be used to match against when monitoring auto games.");
        cmd.Aliases.Add("p");

        cmd.SetAction(_ =>
        {
            var procs = Core.Helpers.ProcessFinder.GetListOfAvailableProcesses();

            foreach (var proc in procs)
            {
                Console.WriteLine($"Pid: {proc.Pid}");
                Console.WriteLine($"Window Title: {proc.WindowTitle}");
                Console.WriteLine($"File Path: {proc.FilePath}");
                Console.WriteLine();
            }

            return 0;
        });

        return cmd;
    }
}