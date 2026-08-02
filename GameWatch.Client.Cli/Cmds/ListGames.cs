using System;
using System.Threading;
using GameWatch.Core.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace GameWatch.Client.Cli.Cmds;

public sealed class ListGames : Command<ListGames.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-v|--verbose")]
        public bool ShouldBeVerbose { get; init; }

        [CommandOption("-m|--manual-only")]
        public bool ShouldDisplayManualGames { get; init; }

        [CommandOption("-a|--auto-only")]
        public bool ShouldDisplayAutoGames { get; init; }
    }

    protected override ValidationResult Validate(CommandContext context, Settings settings)
    {
        return settings switch
        {
            { ShouldDisplayAutoGames: true, ShouldDisplayManualGames: true } => ValidationResult.Error("--manual-only and --auto-only are mutually exclusive flags."),
            _ => ValidationResult.Success()
        };
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var shouldDisplayBothGamesModes = settings is { ShouldDisplayAutoGames: false, ShouldDisplayManualGames: false };
        var gamesHaveBeenDisplayed = false;

        if (shouldDisplayBothGamesModes || (!shouldDisplayBothGamesModes && settings.ShouldDisplayManualGames))
        {
            var manualGames = DbFactory.GameLibrary.GetManualGames();

            if (manualGames.Count > 0)
            {
                gamesHaveBeenDisplayed = true;
                Console.WriteLine("--- Manual games ---");
                foreach (var game in manualGames)
                {
                    Console.WriteLine($"{game.Id}. {TimeSpan.FromSeconds(game.PlayTimeSec.V)} - {game.Name}");
                }
            }
        }

        if (shouldDisplayBothGamesModes || (!shouldDisplayBothGamesModes && settings.ShouldDisplayAutoGames))
        {
            var autoGames = DbFactory.GameLibrary.GetAutoGames();

            if (autoGames.Count > 0)
            {
                if (shouldDisplayBothGamesModes)
                    Console.WriteLine();

                gamesHaveBeenDisplayed = true;
                Console.WriteLine("--- Auto games ---");

                switch (settings.ShouldBeVerbose)
                {
                    case true:
                    {
                        foreach (var game in autoGames)
                        {
                            Console.WriteLine($"{game.Id}. {TimeSpan.FromSeconds(game.PlayTimeSec.V)} - {game.Name}");

                            Console.WriteLine("   Matching rules:");

                            if (game.WindowTitle != null)
                            {
                                Console.WriteLine("      Window Title: should match fully");
                                Console.WriteLine($"         Value: {game.WindowTitle}");
                            }

                            if (game.FilePath != null)
                            {
                                Console.WriteLine("      File Path: should match fully");
                                Console.WriteLine($"         Value: {game.FilePath}");
                            }

                            if (game.WindowRule != null)
                            {
                                Console.WriteLine("      Window Title: should match using pattern");
                                Console.WriteLine($"         Value: {game.WindowRule}");
                            }

                            // ReSharper disable once InvertIf
                            if (game.PathRule != null)
                            {
                                Console.WriteLine("      File Path: should match using pattern");
                                Console.WriteLine($"         Value: {game.PathRule}");
                            }
                        }

                        break;
                    }
                    case false:
                    {
                        foreach (var game in autoGames)
                        {
                            Console.WriteLine($"{game.Id}. {TimeSpan.FromSeconds(game.PlayTimeSec.V)} - {game.Name}");
                        }

                        break;
                    }
                }
            }
        }

        if (!gamesHaveBeenDisplayed)
        {
            Console.WriteLine("ℹ️ No games found which to list");
        }

        return 0;
    }
}