using System;
using System.CommandLine;
using GameWatch.Core;
using GameWatch.Core.Agents.GameMonitor;
using GameWatch.Core.Dbs;
using GameWatch.Core.Dto;
using GameWatch.Core.Ipc;
using GameWatch.Core.Wrappers;

namespace GameWatch.Client.Cli.Cmds;

public static class ToggleGame
{
    public static Command Build()
    {
        var idOption = new Option<int>("--id", "-i")
        {
            Description = "The game id from (see 'list games -m')",
            Required = true
        };

        var cmd = new Command("toggle", "Start or stop a certain manual Game record") { idOption };
        cmd.Aliases.Add("tg");

        cmd.SetAction(async (parseResult, cancellationToken) =>
        {
            var gameIdx = new GameIdx(parseResult.GetValue(idOption) - 1);

            var gameId = GameLibrary.Instance.GetGameIdByIdx(GameMode.Manual, gameIdx);

            if (gameId == null)
            {
                Console.WriteLine("[FAIL] Cannot find game with provided id. Ignoring command...");
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