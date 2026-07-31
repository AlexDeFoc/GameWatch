using System;
using System.ComponentModel;
using System.Threading;
using GameWatch.Core.Dto;
using GameWatch.Core.Helpers;
using Spectre.Console.Cli;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace GameWatch.Client.Cli.Cmds;

public sealed class EditManualGame : Command<EditManualGame.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-i|--idx <GAME_INDEX>", isRequired: true)]
        [Description("The game index. TIP: Can be gathered from 'list games'")]
        public required int GameIdx { get; init; }

        [CommandOption("-t|--title <TITLE>")]
        [Description("New title for the game record")]
        public string? Title { get; init; }

        [CommandOption("-p|--playtime <SECONDS>")]
        [Description("Forced game record playtime in seconds")]
        public int? PlayTimeSeconds { get; init; }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var result = DbFactory.GameLibrary.ChangeGameProperty(GameMode.Manual, settings.GameIdx,
                                                              title: settings.Title,
                                                              playTimeSeconds: settings.PlayTimeSeconds);

        if (!result.HasSucceeded)
        {
            Console.WriteLine($"Error: {result.FailureReason}.");

            return 1;
        }

        Console.WriteLine("Game edited successfully.");

        return 0;
    }
}