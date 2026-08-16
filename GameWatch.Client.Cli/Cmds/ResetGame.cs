using System;
using System.CommandLine;
using GameWatch.Core;
using GameWatch.Core.Dbs;
using GameWatch.Core.Types;

namespace GameWatch.Client.Cli.Cmds;

public static class ResetGame
{
    public static Command Build()
    {
        var displayIdOption = new Option<int>("--id", "-i")
        {
            Description = "The game id from (see 'list games')",
            Required = true
        };

        var manualOption = new Option<bool>("--manual", "-m")
        {
            Description = "Should reset manual game"
        };

        var autoOption = new Option<bool>("--auto", "-a")
        {
            Description = "Should reset auto game"
        };

        var cmd = new Command("reset", "Reset game properties")
        {
            displayIdOption,
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

            var displayId = new DisplayId(result.GetRequiredValue(displayIdOption));
            var tableIdResult = GameLibrary.Instance.GetTableId(GameMode.Manual, displayId);

            if (!tableIdResult.Ok || tableIdResult.TableId is null)
            {
                Console.WriteLine(tableIdResult.FailureReason);
                return 1;
            }

            var tableId = tableIdResult.TableId.Value;

            var resetGameResult = GameLibrary.Instance.ResetGame(gameMode, tableId);

            if (!resetGameResult.Ok || resetGameResult.GameName is null)
            {
                Console.WriteLine(resetGameResult.FailureReason);
                return 1;
            }

            var gameName = resetGameResult.GameName;

            Console.WriteLine($"[OK] Game with Name='{gameName}' got reset");

            var notificationResult = resetManual
                ? await GameMonitorAgentIpcServer.NotifyThatManualGameGotResetAsync(tableId,
                                                                                    gameName,
                                                                                    cancellationToken)
                : await GameMonitorAgentIpcServer.NotifyThatAutoGameGotResetAsync(tableId,
                                                                                  gameName,
                                                                                  cancellationToken);

            if (notificationResult.Ok) return 0;

            Console.WriteLine(notificationResult.FailureReason);
            return 1;
        });

        return cmd;
    }
}