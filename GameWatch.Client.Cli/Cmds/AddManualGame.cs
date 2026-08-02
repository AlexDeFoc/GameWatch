using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Core.Dto;
using GameWatch.Core.GameRecords;
using GameWatch.Core.Helpers;
using GameWatch.Core.Ipc;
using Spectre.Console;
using Spectre.Console.Cli;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace GameWatch.Client.Cli.Cmds;

public sealed class AddManualGame : Command<AddManualGame.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-n|--name <NAME>", isRequired: true)]
        [Description("Name of the Game record")]
        public required string Name { get; init; }

        [CommandOption("-p|--playtime <SECONDS>")]
        [Description("Initial Game record playtime in seconds")]
        [DefaultValue(0)]
        public int PlayTimeSeconds { get; init; }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var gameRecord = new ManualGame { Name = settings.Name, PlayTimeSec = new ElapsedTime(settings.PlayTimeSeconds) };

        DbFactory.GameLibrary.AddGame(gameRecord);

        Console.WriteLine("✅ Manual game added to database");

        return 0;
    }
}