using System;
using System.CommandLine;
using GameWatch.Core;
using GameWatch.Core.Agents.GameMonitor;
using GameWatch.Core.Dbs;
using GameWatch.Core.Dto;
using GameWatch.Core.Ipc;

namespace GameWatch.Client.Cli.Cmds;

public static class ToggleGame
{
    public static Command Build()
    {
        var idxOption = new Option<int>("--index", "-i")
        {
            Description = "The Game index from (see 'list games -m')",
            Required = true
        };

        var cmd = new Command("toggle", "Start or stop a certain manual Game record") { idxOption };
        cmd.Aliases.Add("tg");

        cmd.SetAction(async (parseResult, cancellationToken) =>
        {
            var gameIdx = parseResult.GetValue(idxOption);

            var gameId = GameLibrary.Instance.GetGameIdByIdx(GameMode.Manual, new GameIdx(gameIdx));

            if (gameId == null)
            {
                Console.WriteLine("[FAIL] Provided Game index is out of range. Ignoring command...");
                return 1;
            }

            try
            {
                var notified = await IpcClient.SendToggleManualGameSignalAsync(IpcTarget.GameWatchGameMonitorAgent,
                                                                               gameId.Value,
                                                                               cancellationToken);

                if (notified) return 0;

                Console.WriteLine("[FAIL] Game Monitor Agent is not running. Failed to toggle manual Game!");
            }
            catch (Exception)
            {
                Console.WriteLine("[FAIL] Failed to communicate with the Game Monitor Agent. Failed to toggle manual Game!");
            }

            return 0;
        });

        return cmd;
    }
}