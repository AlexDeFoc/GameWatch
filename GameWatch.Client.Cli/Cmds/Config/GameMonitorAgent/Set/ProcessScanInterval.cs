using System;
using System.CommandLine;
using GameWatch.Core;

namespace GameWatch.Client.Cli.Cmds.Config.GameMonitorAgent.Set;

public static class ProcessScanInterval
{
    public static Command Build()
    {
        var valueArg = new Argument<long>("value")
        {
            Description = "The setting value (in seconds)",
        };

        var cmd = new Command("ProcessScanInterval", "Setting used by game monitor agent to determine how " +
                                                     "often should it check if an auto game has stopped or started")
        {
            valueArg
        };

        cmd.SetAction(async (result, cliCt) =>
        {
            var value = result.GetValue(valueArg);

            if (value < 1L)
            {
                Console.WriteLine("[ERROR] Cannot set interval to a value less then 1");
                return 1;
            }

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