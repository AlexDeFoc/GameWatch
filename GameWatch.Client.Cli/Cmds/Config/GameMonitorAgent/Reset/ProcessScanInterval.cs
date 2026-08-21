using System;
using System.CommandLine;
using GameWatch.Core;
using GameWatch.Core.Dbs;

namespace GameWatch.Client.Cli.Cmds.Config.GameMonitorAgent.Reset;

public static class ProcessScanInterval
{
    public static Command Build()
    {
        var cmd = new Command("ProcessScanInterval", "Setting used by game monitor agent to determine how " +
                                                     "often should it check if an auto game has stopped or started");

        cmd.SetAction(async (_, cliCt) =>
        {
            var value = Settings.GameMonitorAgent.Defaults.ProcessScanInterval;

            var requestResult = await GameMonitorAgentIpcServer.RequestToModifySettingProcessScanIntervalAsync(value, cliCt);

            if (!requestResult.Ok)
            {
                Console.WriteLine(requestResult.FailureReason);
                return 1;
            }

            Console.WriteLine($"[OK] Game monitor agent process scan interval set to {value} seconds");
            return 0;
        });

        return cmd;
    }
}