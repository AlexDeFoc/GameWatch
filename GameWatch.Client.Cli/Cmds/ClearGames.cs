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

public sealed class ClearGames : Command<ClearGames.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-m|--manual-game")]
        [Description("Delete all games from manual collection")]
        public bool TargetGameModeIsManual { get; init; }

        [CommandOption("-a|--auto-game")]
        [Description("Delete all games from auto collection")]
        public bool TargetGameModeIsAuto { get; init; }
    }

    protected override ValidationResult Validate(CommandContext context, Settings settings)
    {
        return settings switch
        {
            { TargetGameModeIsAuto: false, TargetGameModeIsManual: false } => ValidationResult.Error("Must provide at least one game mode flag to let the app determine what game collection to clear."),
            _ => ValidationResult.Success()
        };
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (settings.TargetGameModeIsAuto)
        {
            var actionStatus = DbFactory.GameLibrary.DeleteAllGames(GameMode.Auto);

            Console.WriteLine(actionStatus.HasSucceeded
                                  ? "Deleted all auto games."
                                  : $"Failed to delete all games from auto games collection. Reason: '{actionStatus.FailureReason}'");
        }

        // ReSharper disable once InvertIf
        if (settings.TargetGameModeIsManual)
        {
            var actionStatus = DbFactory.GameLibrary.DeleteAllGames(GameMode.Manual);

            Console.WriteLine(actionStatus.HasSucceeded
                                  ? "Deleted all manual games."
                                  : $"Failed to delete all games from manual games collection. Reason: '{actionStatus.FailureReason}'");
        }

        return 0;
    }
}