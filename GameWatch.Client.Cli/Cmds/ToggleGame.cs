using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Agent.GameMonitor;
using GameWatch.Core.Agents.GameMonitor;
using GameWatch.Core.Dto;
using GameWatch.Core.Helpers;
using GameWatch.Core.Ipc;
using Spectre.Console.Cli;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace GameWatch.Client.Cli.Cmds;

public sealed class ToggleGame : AsyncCommand<ToggleGame.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-i|--idx <GAME_INDEX>", isRequired: true)]
        [Description("The Game index. TIP: Can be gathered from 'list games --manual-only'")]
        public required int GameIdx { get; init; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var gameId = DbFactory.GameLibrary.GetGameIdByIdx(GameMode.Manual, new GameIdx(settings.GameIdx));

        if (gameId == null)
        {
            Console.WriteLine("⛔ Provided Game index is out of range. Ignoring command...");
            return 1;
        }

        try
        {
            var notified = await IpcClient.SendToggleManualGameSignalAsync(IpcTarget.GameWatchGameMonitorAgent, gameId.Value, cancellationToken);

            if (notified) return 0;

            Console.WriteLine("⛔ Game Monitor Agent is not running. Failed to toggle manual Game!");
        }
        catch (Exception)
        {
            Console.WriteLine("⛔ Failed to communicate with the Game Monitor Agent. Failed to toggle manual Game!");
        }

        return 0;
    }
}