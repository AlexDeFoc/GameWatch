using System;
using System.CommandLine;
using GameWatch.Core;
using GameWatch.Core.Agents.GameMonitor;
using GameWatch.Core.Dto;
using GameWatch.Core.Ipc;

namespace GameWatch.Client.Cli.Cmds;

public static class RemoveGame
{
    public static Command Build()
    {
        var idxOption = new Option<int>("--index", "-i")
        {
            Description = "The game index from (see 'list games')",
            Required = true
        };

        var manualOption = new Option<bool>("--manual", "-m")
        {
            Description = "Index corresponds to a manual game"
        };

        var autoOption = new Option<bool>("--auto", "-a")
        {
            Description = "Index corresponds to an auto game"
        };

        var cmd = new Command("remove", "Remove game")
        {
            idxOption,
            manualOption,
            autoOption
        };
        cmd.Aliases.Add("rm");

        cmd.Validators.Add(result =>
        {
            var removeManual = result.GetValue(manualOption);
            var removeAuto = result.GetValue(autoOption);

            switch (removeManual)
            {
                case false when !removeAuto:
                    result.AddError("⛔ Must specify whether to remove a manual or auto game");
                    return;
                case true when removeAuto:
                    result.AddError("⛔ Cannot remove a manual and auto game with the same index");
                    return;
            }

            var idx = result.GetRequiredValue(idxOption);

            var gameMode = removeManual ? GameMode.Manual : GameMode.Auto;
            var r = DbMng.GameLibrary.GetGameIdByIdx(gameMode, new GameIdx(idx));

            if (r.HasValue) return;
            result.AddError(gameMode is GameMode.Manual
                                ? "⛔ Cannot find manual game with specified index"
                                : "⛔ Cannot find auto game with specified index");
        });

        cmd.SetAction(async (result, cancellationToken) =>
        {
            var removeManual = result.GetValue(manualOption);
            var gameMode = removeManual ? GameMode.Manual : GameMode.Auto;
            var idx = new GameIdx(result.GetRequiredValue(idxOption));

            var (hasSucceeded, id, deletedGameTitle, failureReason) = DbMng.GameLibrary.DeleteGame(gameMode, idx);

            if (!hasSucceeded)
            {
                Console.WriteLine(failureReason);
                return 1;
            }

            const IpcTarget target = IpcTarget.GameWatchGameMonitorAgent;
            try
            {
                var notified = gameMode is GameMode.Manual
                    ? await IpcClient.SendResetActiveManualGameSignalAsync(target, id, cancellationToken)
                    : await IpcClient.SendResetActiveAutoGameSignalAsync(target, id, cancellationToken);

                if (!notified)
                {
                    Console.WriteLine("⚠️ Unable to communicate with the GameWatch background service. Please ensure the agent is running.");
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

            Console.WriteLine($"✅ Game with Name='{deletedGameTitle}' deleted");

            return 0;
        });

        return cmd;
    }
}