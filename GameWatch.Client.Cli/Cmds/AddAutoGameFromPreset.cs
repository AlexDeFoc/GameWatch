using System;
using System.CommandLine;
using GameWatch.Core;
using GameWatch.Core.Agents.GameMonitor;
using GameWatch.Core.Dbs;
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
            var (hasSucceeded, preset, failureReason) = GamePresets.Instance.GetPresetByIdx(idx);

            if (!hasSucceeded || preset == null)
            {
                Console.WriteLine(failureReason);
                return 1;
            }

            GameLibrary.Instance.AddGame(preset);

            Console.WriteLine("[OK] Game added successfully");

            const IpcTarget target = IpcTarget.GameWatchGameMonitorAgent;
            try
            {
                var notified = await IpcClient.SendRefreshSignalForAutoGamesListAsync(target, cancellationToken);

                if (!notified)
                {
                    Console.WriteLine("[WARN] Unable to communicate with the GameWatch background service. Please ensure the agent is running.");
                    return 1;
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[WARN] Operation canceled.");
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FAIL] Unhandled exception during IPC call to target '{nameof(target)}': {ex}");
                return 1;
            }

            return 0;
        });

        return cmd;
    }
}