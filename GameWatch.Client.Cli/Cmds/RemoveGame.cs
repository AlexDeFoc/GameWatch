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

public sealed class RemoveGame : AsyncCommand<RemoveGame.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-i|--idx <GAME_INDEX>", isRequired: true)]
        [Description("The game index. TIP: Can be gathered from 'list games'")]
        public required int GameIdx { get; init; }

        [CommandOption("-m|--manual-game")]
        [Description("Delete game from manual collection")]
        public bool TargetGameModeIsManual { get; init; }

        [CommandOption("-a|--auto-game")]
        [Description("Delete game from auto collection")]
        public bool TargetGameModeIsAuto { get; init; }
    }

    protected override ValidationResult Validate(CommandContext context, Settings settings)
    {
        return settings switch
        {
            { TargetGameModeIsAuto: false, TargetGameModeIsManual: false } => ValidationResult.Error("Must provide at least one game mode flag to let the app determine from which collection to delete the game."),
            { TargetGameModeIsAuto: true, TargetGameModeIsManual: true } => ValidationResult.Error("Cannot delete a game from both game collections because the provided id will most certainly mean a different game in both collections."),
            _ => ValidationResult.Success()
        };
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var targetGameMode = settings.TargetGameModeIsAuto ? GameMode.Auto : GameMode.Manual;

        var actionStatus = DbFactory.GameLibrary.DeleteGame(targetGameMode, settings.GameIdx);

        // Notify running Agent over IPC with the specific Game ID
        try
        {
            var notified = targetGameMode == GameMode.Auto
                ? await IpcClient.SendAutoManualGameSignalAsync(IpcTarget.GameWatchGameMonitorAgent, actionStatus.DeletedGameId, cancellationToken)
                : await IpcClient.SendRemoveManualGameSignalAsync(IpcTarget.GameWatchGameMonitorAgent, actionStatus.DeletedGameId, cancellationToken);

            if (!notified)
            {
                AnsiConsole.MarkupLine("[yellow]⚠ Note:[/] Background agent is not running. Game removed from database.");
            }
        }
        catch (Exception)
        {
            AnsiConsole.MarkupLine("[yellow]⚠ Note:[/] Failed to ping background agent. Game removed from DB successfully.");
        }

        return 0;
    }
}