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
        [Description("The Game index. TIP: Can be gathered from 'list games'")]
        public required int GameIdx { get; init; }

        [CommandOption("-n|--name <NAME>")]
        [Description("New name for the Game record")]
        public string? Name { get; init; }

        [CommandOption("-p|--playtime <SECONDS>")]
        [Description("Forced Game record playtime in seconds")]
        public int? PlayTimeSeconds { get; init; }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var result = DbFactory.GameLibrary.ChangeGameProperty(gameMode: GameMode.Manual,
                                                              gameIdx: new GameIdx(settings.GameIdx),
                                                              name: settings.Name,
                                                              playTimeSec: settings.PlayTimeSeconds != null
                                                                  ? new ElapsedTime(settings.PlayTimeSeconds.Value)
                                                                  : null
        );

        if (!result.HasSucceeded)
        {
            Console.WriteLine(result.FailureReason);

            return 1;
        }

        Console.WriteLine("✅ Game edited successfully");

        return 0;
    }
}