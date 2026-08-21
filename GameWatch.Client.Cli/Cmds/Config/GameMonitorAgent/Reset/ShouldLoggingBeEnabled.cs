using System;
using System.CommandLine;
using GameWatch.Core;
using GameWatch.Core.Dbs;

namespace GameWatch.Client.Cli.Cmds.Config.GameMonitorAgent.Reset;

public static class ShouldLoggingBeEnabled
{
    public static Command Build()
    {
        var cmd = new Command("ShouldLoggingBeEnabled", "Reset logging status to default (enabled)");

        cmd.SetAction(async (_, cliCt) =>
        {
            var value = Settings.GameMonitorAgent.Defaults.IsLoggingEnabledStatus;

            var requestResult = await GameMonitorAgentIpcServer.RequestToModifySettingIsLoggingEnabledAsync(value, cliCt);

            if (!requestResult.Ok)
            {
                Console.WriteLine(requestResult.FailureReason);
                return 1;
            }

            Console.WriteLine($"[OK] Game monitor agent logging set to {value}");
            return 0;
        });

        return cmd;
    }
}