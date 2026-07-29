using System;
using System.ComponentModel;
using System.Threading;
using GameWatch.Core.Dto;
using GameWatch.Core.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;

// ReSharper disable UnusedAutoPropertyAccessor.Global

// ReSharper disable ClassNeverInstantiated.Global

namespace GameWatch.Client.Cli.Cmds;

public sealed class ToggleGame : Command<ToggleGame.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-i|--idx <GAME_INDEX>", isRequired: true)]
        [Description("The game index. TIP: Can be gathered from 'list games'")]
        public required int GameIdx { get; init; }

        [CommandOption("-m|--manual-game")]
        [Description("Toggle game from manual collection")]
        public bool TargetGameModeIsManual { get; init; }

        [CommandOption("-a|--auto-game")]
        [Description("Toggle game from auto collection")]
        public bool TargetGameModeIsAuto { get; init; }
    }

    protected override ValidationResult Validate(CommandContext context, Settings settings)
    {
        return settings switch
        {
            { TargetGameModeIsAuto: false, TargetGameModeIsManual: false } => ValidationResult.Error("Must provide at least one game mode flag to let the app determine from which collection to select the game."),
            { TargetGameModeIsAuto: true, TargetGameModeIsManual: true } => ValidationResult.Error("Cannot toggle a game from both game collections because the provided id will most certainly mean a different game in both collections."),
            _ => ValidationResult.Success()
        };
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();

        // var targetGameMode = settings.TargetGameModeIsAuto ? GameMode.Auto : GameMode.Manual;
        //
        // var actionStatus = DbFactory.GameLibrary.ToggleGame(targetGameMode, settings.GameIdx);
        //
        // Console.WriteLine(actionStatus.HasSucceeded
        //                       ? $"Deleted game called '{actionStatus.DeletedGameTitle}'."
        //                       : $"Failed to delete game. Reason: '{actionStatus.FailureReason}'");
        //
        // return 0;
    }
}