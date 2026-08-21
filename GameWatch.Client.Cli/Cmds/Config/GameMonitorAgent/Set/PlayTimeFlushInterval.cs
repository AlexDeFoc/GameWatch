using System;
using System.CommandLine;
using GameWatch.Core;

namespace GameWatch.Client.Cli.Cmds.Config.GameMonitorAgent.Set;

public static class PlayTimeFlushInterval
{
    public static Command Build()
    {
        var valueArg = new Argument<long>("value")
        {
            Description = "The setting value (in seconds)",
        };

        var cmd = new Command("PlayTimeFlushInterval", "Setting used by game monitor agent to determine how " +
                                                       "often should games playtime be flushed to the database")
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