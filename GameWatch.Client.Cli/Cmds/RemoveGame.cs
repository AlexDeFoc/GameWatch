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

public sealed class RemoveGame : AsyncCommand<RemoveGame.Settings>
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

        var actionStatus = DbFactory.GameLibrary.DeleteGame(targetGameMode, new GameIdx(settings.GameIdx));

        if (!actionStatus.HasSucceeded)
        {
            Console.WriteLine(actionStatus.FailureReason);

            return 1;
        }

        try
        {
            var notified = targetGameMode == GameMode.Auto
                ? await IpcClient.SendRemoveAutoGameSignalAsync(IpcTarget.GameWatchGameMonitorAgent, actionStatus.DeletedGameId, cancellationToken)
                : await IpcClient.SendRemoveManualGameSignalAsync(IpcTarget.GameWatchGameMonitorAgent, actionStatus.DeletedGameId, cancellationToken);

            if (notified) return 0;

            Console.WriteLine("⚠️ Game Monitor Agent is not running. No problem, the database file was updated anyways.");
        }
        catch (Exception)
        {
            Console.WriteLine("⚠️ Failed to communicate with the Game Monitor Agent." +
                              "Failed notify the agent to remove the Game from active games list;" +
                              "this may cause the Game to be saved to the database even if you just deleted it," +
                              "in that case close the Game then delete the Game record from the app.");
        }

        return 0;
    }
}