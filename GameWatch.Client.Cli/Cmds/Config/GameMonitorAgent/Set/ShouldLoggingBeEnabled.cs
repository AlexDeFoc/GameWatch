using System;
using System.CommandLine;
using GameWatch.Core;

namespace GameWatch.Client.Cli.Cmds.Config.GameMonitorAgent.Set;

public static class ShouldLoggingBeEnabled
{
    public static Command Build()
    {
        var valueArg = new Argument<bool>("value")
        {
            Description = "The setting value (in seconds)",
        };

        var cmd = new Command("ShouldLoggingBeEnabled", "Setting used by game monitor agent to " +
                                                        "determine whether to log to logs database or not")
        {
            valueArg
        };

        cmd.SetAction(async (result, cliCt) =>
        {
            var value = result.GetValue(valueArg);

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