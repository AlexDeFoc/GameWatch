using System;
using System.CommandLine;
using GameWatch.Core;
using GameWatch.Core.Agents.GameMonitor;
using GameWatch.Core.Dto;
using GameWatch.Core.Ipc;

namespace GameWatch.Client.Cli.Cmds;

public static class ResetGame
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

        var cmd = new Command("reset", "Reset game properties")
        {
            idxOption,
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
                    result.AddError("⛔ Must specify whether to reset a manual or auto game");
                    return;
                case true when resetAuto:
                    result.AddError("⛔ Cannot reset a manual and auto game with the same index");
                    return;
            }

            var idx = result.GetRequiredValue(idxOption);

            var gameMode = resetManual ? GameMode.Manual : GameMode.Auto;
            var r = DbMng.GameLibrary.GetGameIdByIdx(gameMode, new GameIdx(idx));

            if (r.HasValue) return;
            result.AddError(gameMode is GameMode.Manual
                                ? "⛔ Cannot find manual game with specified index"
                                : "⛔ Cannot find auto game with specified index");
        });

        cmd.SetAction(async (result, cancellationToken) =>
        {
            var resetManual = result.GetValue(manualOption);
            var gameMode = resetManual ? GameMode.Manual : GameMode.Auto;
            var idxVal = result.GetRequiredValue(idxOption);
            var idx = new GameIdx(idxVal);
            var id = DbMng.GameLibrary.GetGameIdByIdx(gameMode, idx);

            if (!id.HasValue)
            {
                Console.WriteLine(gameMode is GameMode.Manual
                                      ? "⛔ Cannot find manual game with specified index"
                                      : "⛔ Cannot find auto game with specified index");
                return 1;
            }

            DbMng.GameLibrary.ResetGamePlayTime(gameMode, idx);

            const IpcTarget target = IpcTarget.GameWatchGameMonitorAgent;
            try
            {
                var notified = gameMode is GameMode.Manual
                    ? await IpcClient.SendResetActiveManualGameSignalAsync(target, id.Value, cancellationToken)
                    : await IpcClient.SendResetActiveAutoGameSignalAsync(target, id.Value, cancellationToken);

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

            if (gameMode is GameMode.Manual)
            {
                var requestResult = DbMng.GameLibrary.GetManualGameByIdx(idx);

                if (!requestResult.HasSucceeded || requestResult.Game is null)
                {
                    Console.WriteLine("⚠ Manual game reset, though we failed to grab game name");
                    Console.WriteLine(requestResult.FailureReason);
                    return 0;
                }

                Console.WriteLine($"✅ Game with Name='{requestResult.Game.Name}' reset");
            }
            else
            {

                var requestResult = DbMng.GameLibrary.GetAutoGameByIdx(idx);

                if (!requestResult.HasSucceeded || requestResult.Game is null)
                {
                    Console.WriteLine("⚠ Auto game reset, though we failed to grab game name");
                    Console.WriteLine(requestResult.FailureReason);
                    return 0;
                }

                Console.WriteLine($"✅ Game with Name='{requestResult.Game.Name}' reset");
            }

            return 0;
        });

        return cmd;
    }
}