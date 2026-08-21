using System;
using System.CommandLine;
using GameWatch.Core;
using GameWatch.Core.Dbs;
using GameWatch.Core.Types;

namespace GameWatch.Client.Cli.Cmds;

public static class AddAutoGameFromPreset
{
    public static Command Build()
    {
        var displayIdOption = new Option<int>("--id", "-i")
        {
            Description = "The preset id from (see 'list games -p')",
            Required = true
        };

        var cmd = new Command("preset", "Add auto game from a preset")
        {
            displayIdOption
        };
        cmd.Aliases.Add("p");

        cmd.SetAction(async (result, cliCt) =>
        {
            var displayId = new DisplayId(result.GetValue(displayIdOption));

            var tableIdResult = GamePresets.Instance.GetTableId(displayId);

            if (!tableIdResult.Ok || tableIdResult.TableId is null)
            {
                Console.WriteLine(tableIdResult.FailureReason);
                return 1;
            }

            var tableId = tableIdResult.TableId.Value;

            var gamePresetResult = await GamePresets.Instance.GetPresetAsync(tableId, cliCt);

            if (!gamePresetResult.Ok || gamePresetResult.GamePreset is null)
            {
                Console.WriteLine(gamePresetResult.FailureReason);
                return 1;
            }

            var gamePreset = gamePresetResult.GamePreset;

            var addedGameResult = await GameLibrary.Instance.AddGameAsync(gamePreset, cliCt);

            if (!addedGameResult.Ok || addedGameResult.TableId is null)
            {
                Console.WriteLine(addedGameResult.FailureReason);
                return 1;
            }


            Console.WriteLine($"[OK] Game with Name='{gamePreset.Name}' added successfully");

            gamePreset.TableId = addedGameResult.TableId.Value;
            var notificationResult = await GameMonitorAgentIpcServer.RequestToTrackNewlyAddedAutoGameAsync(
                new AutoGameDto(gamePreset), cliCt);

            if (notificationResult.Ok) return 0;

            Console.WriteLine(notificationResult.FailureReason);
            return 1;
        });

        return cmd;
    }
}