using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Agent.GameMonitor;
using GameWatch.Core.Agents.GameMonitor;
using GameWatch.Core.Dto;
using GameWatch.Core.Helpers;
using GameWatch.Core.Ipc;
using Spectre.Console;
using Spectre.Console.Cli;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace GameWatch.Client.Cli.Cmds;

public sealed class ResetGame : AsyncCommand<ResetGame.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-i|--idx <GAME_INDEX>", isRequired: true)]
        [Description("The Game index. TIP: Can be gathered from 'list games'")]
        public required int GameIdx { get; init; }

        [CommandOption("-m|--manual-Game")]
        [Description("Delete Game from manual collection")]
        public bool TargetGameModeIsManual { get; init; }

        [CommandOption("-a|--auto-Game")]
        [Description("Delete Game from auto collection")]
        public bool TargetGameModeIsAuto { get; init; }
    }

    protected override ValidationResult Validate(CommandContext context, Settings settings)
    {
        return settings switch
        {
            { TargetGameModeIsAuto: false, TargetGameModeIsManual: false } => ValidationResult.Error("Must provide at least one Game mode flag to let the app determine from which collection to delete the Game."),
            { TargetGameModeIsAuto: true, TargetGameModeIsManual: true } => ValidationResult.Error("Cannot delete a Game from both Game collections because the provided id will most certainly mean a different Game in both collections."),
            _ => ValidationResult.Success()
        };
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var targetGameMode = settings.TargetGameModeIsAuto ? GameMode.Auto : GameMode.Manual;

        var gameId = DbFactory.GameLibrary.GetGameIdByIdx(GameMode.Manual, new GameIdx(settings.GameIdx));

        if (gameId == null)
        {
            Console.WriteLine("⛔ Provided Game index is out of range. Ignoring command...");
            return 1;
        }

        DbFactory.GameLibrary.ResetGamePlayTime(targetGameMode, new GameIdx(settings.GameIdx));

        try
        {
            var notified = targetGameMode == GameMode.Auto
                ? await IpcClient.SendResetActiveAutoGameSignalAsync(IpcTarget.GameWatchGameMonitorAgent, gameId.Value, cancellationToken)
                : await IpcClient.SendResetActiveManualGameSignalAsync(IpcTarget.GameWatchGameMonitorAgent, gameId.Value, cancellationToken);

            if (notified) return 0;

            Console.WriteLine("⚠️ Game Monitor Agent is not running. No problem, the database file was updated anyways.");
        }
        catch (Exception)
        {
            Console.WriteLine("⚠ Failed to communicate with the Game Monitor Agent. Failed notify the agent to reset the 60 second interval. Though the database file was updated anyways.");
        }

        return 0;
    }
}