using System;
using System.Threading;
using GameWatch.Client.Cli.Helpers;
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
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        using var conn = DbFactory.GameLibrary.CreateConnection();

        var manualGames = DbFactory.GameLibrary.GetManualGames(conn);

        var gamesHaveBeenDisplayed = false;

        if (manualGames.Count > 0)
        {
            gamesHaveBeenDisplayed = true;
            Console.WriteLine("--- Manual games ---");
            foreach (var game in manualGames)
            {
                Console.WriteLine($"{game.TablePositionIdx + 1}. {TimeSpan.FromSeconds(game.GameRecordPlayTime)} - {game.GameRecordTitle}");
            }
        }

        if (settings.ShouldBeVerbose)
        {
            var autoGames = DbFactory.GameLibrary.GetAutoGamesWithDetails(conn);

            if (autoGames.Count > 0)
            {
                gamesHaveBeenDisplayed = true;
                Console.WriteLine("--- Auto games ---");

                foreach (var game in autoGames)
                {
                    Console.WriteLine($"{game.TablePositionIdx + 1}. {TimeSpan.FromSeconds(game.GameRecordPlayTime)} - {game.GameRecordTitle}");

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

                    if (game.WindowTitleRegexPattern != null)
                    {
                        Console.WriteLine("      Window Title: regex pattern");
                        Console.WriteLine($"         Value: {game.WindowTitleRegexPattern}");
                    }

                    // ReSharper disable once InvertIf
                    if (game.FilePathRegexPattern != null)
                    {
                        Console.WriteLine("      FilePath: regex pattern");
                        Console.WriteLine($"         Value: {game.FilePathRegexPattern}");
                    }
                }
            }
        }
        else if (!settings.ShouldBeVerbose)
        {
            var autoGames = DbFactory.GameLibrary.GetAutoGamesSimplified(conn);

            if (autoGames.Count > 0)
            {
                gamesHaveBeenDisplayed = true;
                Console.WriteLine("--- Auto games ---");

                foreach (var game in autoGames)
                {
                    Console.WriteLine($"{game.TablePositionIdx + 1}. {TimeSpan.FromSeconds(game.GameRecordPlayTime)} - {game.GameRecordTitle}");
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