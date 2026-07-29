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
                    Console.WriteLine($"{game.Idx}. {TimeSpan.FromSeconds(game.PlayTimeSeconds)} - {game.Title}");
                }
            }
        }

        if (shouldDisplayBothGamesModes || (!shouldDisplayBothGamesModes && settings.ShouldDisplayAutoGames))
        {
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (settings.ShouldBeVerbose)
            {
                var autoGames = DbFactory.GameLibrary.GetAutoGamesWithDetails();

                if (autoGames.Count > 0)
                {
                    if (shouldDisplayBothGamesModes)
                        Console.WriteLine();

                    gamesHaveBeenDisplayed = true;
                    Console.WriteLine("--- Auto games ---");

                    foreach (var game in autoGames)
                    {
                        Console.WriteLine($"{game.Idx}. {TimeSpan.FromSeconds(game.PlayTimeSeconds)} - {game.Title}");

                        Console.WriteLine("   Matching rules:");

                        if (game.ProcessWindowTitle != null)
                        {
                            Console.WriteLine("      Window Title: exact match");
                            Console.WriteLine($"         Value: {game.ProcessWindowTitle}");
                        }

                        if (game.ProcessFilePath != null)
                        {
                            Console.WriteLine("      FilePath: exact match");
                            Console.WriteLine($"         Value: {game.ProcessFilePath}");
                        }

                        if (game.ProcessWindowTitlePattern != null)
                        {
                            Console.WriteLine("      Window Title: regex pattern");
                            Console.WriteLine($"         Value: {game.ProcessWindowTitlePattern}");
                        }

                        // ReSharper disable once InvertIf
                        if (game.ProcessFilePathPattern != null)
                        {
                            Console.WriteLine("      FilePath: regex pattern");
                            Console.WriteLine($"         Value: {game.ProcessFilePathPattern}");
                        }
                    }
                }
            }
            else if (!settings.ShouldBeVerbose)
            {
                var autoGames = DbFactory.GameLibrary.GetAutoGamesSimplified();

                if (autoGames.Count > 0)
                {
                    if (shouldDisplayBothGamesModes)
                        Console.WriteLine();

                    gamesHaveBeenDisplayed = true;
                    Console.WriteLine("--- Auto games ---");

                    foreach (var game in autoGames)
                    {
                        Console.WriteLine($"{game.Idx}. {TimeSpan.FromSeconds(game.PlayTimeSeconds)} - {game.Title}");
                    }
                }
            }
        }

        if (!gamesHaveBeenDisplayed)
        {
            Console.WriteLine("No games found");
        }

        return 0;
    }
}