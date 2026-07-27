using System.ComponentModel;
using System.Threading;
using GameWatch.Client.Cli.Dto.GameRecords;
using GameWatch.Client.Cli.Helpers;
using Spectre.Console.Cli;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace GameWatch.Client.Cli.Cmds;

public sealed class AddManualGame : Command<AddManualGame.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-t|--title <TITLE>", isRequired: true)]
        [Description("Title of the game record")]
        public required string GameRecordTitle { get; init; }

        [CommandOption("-p|--playtime <SECONDS>")]
        [Description("Initial game record playtime in seconds")]
        [DefaultValue(0)]
        public int GameRecordPlayTime { get; init; }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var gameRecord = new ManualGameRecord
                         {
                             Title = settings.GameRecordTitle,
                             PlayTime = settings.GameRecordPlayTime
                         };

        DbFactory.GameLibrary.AddGame(gameRecord);

        return 0;
    }
}