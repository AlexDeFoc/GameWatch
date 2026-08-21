using System;
using System.CommandLine;
using GameWatch.Core;
using GameWatch.Core.Dbs;

namespace GameWatch.Client.Cli.Cmds.Config.GameMonitorAgent.Reset;

public static class PlayTimeFlushInterval
{
    public static Command Build()
    {
        var cmd = new Command("PlayTimeFlushInterval", "Reset playtime flush interval to default value (60 sec)");

        cmd.SetAction(async (_, cliCt) =>
        {
            var value = Settings.GameMonitorAgent.Defaults.PlayTimeFlushInterval;

            var requestResult = await GameMonitorAgentIpcServer.RequestToModifySettingPlayTimeFlushIntervalAsync(value, cliCt);

            if (!requestResult.Ok)
            {
                Console.WriteLine(requestResult.FailureReason);
                return 1;
            }

            Console.WriteLine($"[OK] Game monitor agent playtime flushing interval set to {value} seconds");
            return 0;
        });

        return cmd;
    }
}