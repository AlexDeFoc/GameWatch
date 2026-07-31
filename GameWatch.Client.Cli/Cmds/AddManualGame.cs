using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Core.Dto.GameRecords;
using GameWatch.Core.Helpers;
using GameWatch.Core.Ipc;
using Spectre.Console;
using Spectre.Console.Cli;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace GameWatch.Client.Cli.Cmds;

public sealed class AddManualGame : AsyncCommand<AddManualGame.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-t|--title <TITLE>", isRequired: true)]
        [Description("Title of the game record")]
        public required string Title { get; init; }

        [CommandOption("-p|--playtime <SECONDS>")]
        [Description("Initial game record playtime in seconds")]
        [DefaultValue(0)]
        public int PlayTimeSeconds { get; init; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var gameRecord = new ManualGame { Title = settings.Title, PlayTimeSeconds = settings.PlayTimeSeconds };

        var gameId = DbFactory.GameLibrary.AddGame(gameRecord);
        AnsiConsole.MarkupLine($"[green]✓[/] Successfully added manual game [bold]{settings.Title}[/].");

        // Notify running Agent over IPC with the specific Game Id
        try
        {
            var notified = await IpcClient.SendToggleManualGameSignalAsync(IpcTarget.GameWatchGameMonitorAgent, gameId, cancellationToken);
            if (!notified)
            {
                AnsiConsole.MarkupLine("[yellow]⚠ Note:[/] Background agent is not running. Game added to database.");
            }
        }
        catch (Exception)
        {
            AnsiConsole.MarkupLine("[yellow]⚠ Note:[/] Failed to ping background agent. Game saved to DB successfully.");
        }

        return 0;
    }
}