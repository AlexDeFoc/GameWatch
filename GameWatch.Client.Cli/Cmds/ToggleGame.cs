using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Core.Dto;
using GameWatch.Core.Helpers;
using GameWatch.Core.Ipc;
using Spectre.Console;
using Spectre.Console.Cli;

// ReSharper disable UnusedAutoPropertyAccessor.Global

// ReSharper disable ClassNeverInstantiated.Global

namespace GameWatch.Client.Cli.Cmds;

public sealed class ToggleGame : AsyncCommand<ToggleGame.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-i|--idx <GAME_INDEX>", isRequired: true)]
        [Description("The game index. TIP: Can be gathered from 'list games --manual-only'")]
        public required int GameIdx { get; init; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var gameIdGotten = DbFactory.GameLibrary.GetGameIdByPosition(GameMode.Manual, settings.GameIdx);

        if (gameIdGotten == null)
        {
            AnsiConsole.MarkupLine("[red]⛔ Failure Reason:[/] Index provided is out of range.");
            return 1;
        }

        var gameId = gameIdGotten.Value;

        // Notify running Agent over IPC with the specific Game Idx
        try
        {

            var notified = await IpcClient.SendToggleManualGameSignalAsync(IpcTarget.GameWatchGameMonitorAgent, gameId, cancellationToken);

            if (!notified)
            {
                AnsiConsole.MarkupLine("[red]⛔ Failure Reason:[/] Background agent is not running.");
            }
        }
        catch (Exception)
        {
            AnsiConsole.MarkupLine("[red]⛔ Failure Reason:[/] Failed to ping/reach the background agent.");
        }

        return 0;
    }
}