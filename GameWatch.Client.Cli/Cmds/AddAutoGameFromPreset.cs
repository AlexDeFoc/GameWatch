using System;
using System.CommandLine;
using GameWatch.Core;
using GameWatch.Core.Agents.GameMonitor;
using GameWatch.Core.Dto;
using GameWatch.Core.Ipc;

namespace GameWatch.Client.Cli.Cmds;

public static class AddAutoGameFromPreset
{
    public static Command Build()
    {
        var idxOption = new Option<int>("--index", "-i")
        {
            Description = "The preset index from (see 'list games -p')",
            Required = true
        };

        var cmd = new Command("preset", "Add auto game from a preset")
        {
            idxOption
        };
        cmd.Aliases.Add("p");

        cmd.SetAction(async (result, cancellationToken) =>
        {
            var idx = new GameIdx(result.GetValue(idxOption));
            var (hasSucceeded, preset, failureReason) = DbMng.GameLibraryPresets.GetPresetByIdx(idx);

            if (!hasSucceeded || preset == null)
            {
                Console.WriteLine(failureReason);
                return 1;
            }

            DbMng.GameLibrary.AddGame(preset);

            const IpcTarget target = IpcTarget.GameWatchGameMonitorAgent;
            try
            {
                var notified = await IpcClient.SendRefreshSignalForAutoGamesListAsync(target, cancellationToken);

                if (!notified)
                {
                    Console.WriteLine("⚠ Unable to communicate with the GameWatch background service. Please ensure the agent is running.");
                    return 1;
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("⚠ Operation canceled.");
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⛔ Unhandled exception during IPC call to target '{nameof(target)}': {ex}");
                return 1;
            }

            Console.WriteLine("✅ Game added successfully");

            return 0;
        });

        return cmd;
    }
}