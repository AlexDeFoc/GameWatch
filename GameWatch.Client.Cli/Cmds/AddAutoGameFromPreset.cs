using System;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Core;
using GameWatch.Core.Dbs;
using GameWatch.Core.Types;

namespace GameWatch.Client.Cli.Cmds;

public static class AddAutoGameFromPreset
{
    public static Task<Command> BuildAsync(CancellationToken callerCancellationToken)
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
            using var ctSrc = CancellationTokenSource.CreateLinkedTokenSource(callerCancellationToken, cliCt);
            var ct = ctSrc.Token;

            var displayId = new DisplayId(result.GetValue(displayIdOption));

            var tableIdResult = GamePresets.Instance.GetTableId(displayId);

            if (!tableIdResult.Ok || tableIdResult.TableId is null)
            {
                Console.WriteLine(tableIdResult.FailureReason);
                return 1;
            }

            var tableId = tableIdResult.TableId.Value;

            var gamePresetResult = await GamePresets.Instance.GetPresetAsync(tableId, ct);

            if (!gamePresetResult.Ok || gamePresetResult.GamePreset is null)
            {
                Console.WriteLine(gamePresetResult.FailureReason);
                return 1;
            }

            var gamePreset = gamePresetResult.GamePreset;

            await GameLibrary.Instance.AddGameAsync(gamePreset, ct);

            Console.WriteLine($"[OK] Game with Name='{gamePreset.Name}' added successfully");

            var notificationResult = await GameMonitorAgentIpcServer.RequestToTrackNewlyAddedAutoGameAsync(
                new AutoGameDto(gamePreset), ct);

            if (notificationResult.Ok) return 0;

            Console.WriteLine(notificationResult.FailureReason);
            return 1;
        });

        return Task.FromResult(cmd);
    }
}