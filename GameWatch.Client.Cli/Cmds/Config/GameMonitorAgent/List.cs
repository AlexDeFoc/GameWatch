using System;
using System.CommandLine;
using GameWatch.Core.Dbs;

namespace GameWatch.Client.Cli.Cmds.Config.GameMonitorAgent;

public static class List
{
    public static Command Build()
    {
        var cmd = new Command("list", "List all settings and their current value");

        cmd.SetAction(_ =>
        {
            Console.WriteLine("--- GameMonitorAgent Settings ---");
            Console.WriteLine($"* IsLoggingEnabled={Settings.GameMonitorAgent.Instance.CachedSettingIsLoggingEnabled}");
            Console.WriteLine($"* ProcessScanInterval={Settings.GameMonitorAgent.Instance.CachedSettingProcessScanInterval}s");
            Console.WriteLine($"* PlayTimeFlushInterval={Settings.GameMonitorAgent.Instance.CachedSettingPlayTimeFlushInterval}s");
            return 0;
        });

        return cmd;
    }
}