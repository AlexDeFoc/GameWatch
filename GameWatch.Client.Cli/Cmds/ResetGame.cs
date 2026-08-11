using System;
using System.CommandLine;
using GameWatch.Core.Agents.GameMonitor;
using GameWatch.Core.Dbs;
using GameWatch.Core.Ipc;
using GameWatch.Core.Wrappers;

namespace GameWatch.Client.Cli.Cmds;

public static class ResetGame
{
    public static Command Build()
    {
        var idOption = new Option<int>("--id", "-i")
        {
            Description = "The game id from (see 'list games')",
            Required = true
        };

        var manualOption = new Option<bool>("--manual", "-m")
        {
            Description = "Sets whether provided game id corresponds to a manual game"
        };

        var autoOption = new Option<bool>("--auto", "-a")
        {
            Description = "Sets whether provided game id corresponds to an auto game"
        };

        var cmd = new Command("reset", "Reset game properties")
        {
            idOption,
            manualOption,
            autoOption
        };
        cmd.Aliases.Add("rs");

        cmd.Validators.Add(result =>
        {
            var resetManual = result.GetValue(manualOption);
            var resetAuto = result.GetValue(autoOption);

            switch (resetManual)
            {
                case false when !resetAuto:
                    result.AddError("[FAIL] Must specify whether to reset a manual or auto game");
                    return;
                case true when resetAuto:
                    result.AddError("[FAIL] Cannot reset a manual and auto game with the same index");
                    return;
            }
        });

        cmd.SetAction(async (result, cancellationToken) =>
        {
            var resetManual = result.GetValue(manualOption);
            var gameMode = resetManual ? GameMode.Manual : GameMode.Auto;
            var gameIdx = new GameIdx(result.GetRequiredValue(idOption) - 1);
            var gameIdResult = GameLibrary.Instance.GetGameIdByIdx(gameMode, gameIdx);

            if (!gameIdResult.HasValue)
            {
                Console.WriteLine(gameMode is GameMode.Manual
                                      ? "[FAIL] Cannot find manual game with provided id"
                                      : "[FAIL] Cannot find auto game with provided id");
                return 1;
            }

            var resetGameResult = GameLibrary.Instance.ResetGamePlayTime(gameMode, gameIdx);

            if (!resetGameResult.HasSucceeded || resetGameResult.GameName is null)
            {
                Console.WriteLine(resetGameResult.FailureReason);
                return 1;
            }

            Console.WriteLine($"[OK] Game with Name='{resetGameResult.GameName}' reset");

            const IpcTarget target = IpcTarget.GameWatchGameMonitorAgent;
            try
            {
                var notified = gameMode is GameMode.Manual
                    ? await IpcClient.SendResetActiveManualGameSignalAsync(target, gameIdResult.Value, cancellationToken)
                    : await IpcClient.SendResetActiveAutoGameSignalAsync(target, gameIdResult.Value, cancellationToken);

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