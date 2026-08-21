using System;
using System.CommandLine;
using GameWatch.Core.Dbs;

namespace GameWatch.Client.Cli.Cmds;

public static class ListGames
{
    public static Command Build()
    {
        var verboseOption = new Option<bool>("--verbose", "-v")
        {
            Description = "When listing games and/or records, display additionally the matching rules."
        };

        var showManualGamesOption = new Option<bool>("--manual-only", "-m")
        {
            Description = "Display only manual game records if specified alone, or along side other games if specified along other flags"
        };

        var showAutoGamesOption = new Option<bool>("--auto-only", "-a")
        {
            Description = "Display only auto game records if specified alone, or along side other games if specified along other flags"
        };

        var showPresetsOption = new Option<bool>("--show-presets", "-p")
        {
            Description = "Display only presets if specified alone, or along side other games if specified along other flags"
        };

        var cmd = new Command("games", "List all or filtered games in the library")
        {
            verboseOption,
            showManualGamesOption,
            showAutoGamesOption,
            showPresetsOption
        };

        cmd.Aliases.Add("g");

        cmd.SetAction(async (parseResult, cliCt) =>
        {
            var verbose = parseResult.GetValue(verboseOption);
            var showManualGames = parseResult.GetValue(showManualGamesOption);
            var showAutoGames = parseResult.GetValue(showAutoGamesOption);
            var showPresets = parseResult.GetValue(showPresetsOption);

            // Default category selection no flag behavior
            if (!showManualGames && !showAutoGames && !showPresets)
            {
                showManualGames = true;
                showAutoGames = true;
            }

            var anyGamesDisplayed = false;

            // Kick off only needed tasks in parallel
            var manualGamesTask = showManualGames
                ? GameLibrary.Instance.GetManualGamesAsync(cliCt)
                : null;

            var autoGamesTask = showAutoGames
                ? GameLibrary.Instance.GetAutoGamesAsync(cliCt)
                : null;

            if (manualGamesTask is not null)
            {
                var manualGames = await manualGamesTask;

                if (manualGames.Count > 0)
                {
                    anyGamesDisplayed = true;
                    Console.WriteLine("--- Manual games ---");
                    for (var i = 0; i < manualGames.Count; ++i)
                    {
                        var game = manualGames[i];
                        Console.WriteLine($"{i + 1}. {TimeSpan.FromSeconds(game.PlayTime.V)} - {game.Name}");
                    }

                    if (showAutoGames || showPresets)
                        Console.WriteLine();
                }
            }

            if (autoGamesTask is not null)
            {
                var autoGames = await autoGamesTask;
                if (autoGames.Length > 0)
                {
                    anyGamesDisplayed = true;
                    Console.WriteLine("--- Auto games ---");

                    if (!verbose)
                    {
                        for (var i = 0; i < autoGames.Length; ++i)
                        {
                            var game = autoGames[i];
                            Console.WriteLine($"{i + 1}. {TimeSpan.FromSeconds(game.PlayTime.V)} - {game.Name}");
                        }
                    }
                    else
                    {
                        for (var i = 0; i < autoGames.Length; ++i)
                        {
                            var game = autoGames[i];
                            Console.WriteLine($"{i + 1}. {TimeSpan.FromSeconds(game.PlayTime.V)} - {game.Name}");

                            Console.WriteLine("  Matching rules:");
                            if (game.WindowTitle is not null)
                                Console.WriteLine($"* Window title={game.WindowTitle}");
                            if (game.WindowRule is not null)
                                Console.WriteLine($"* Window title rule={game.WindowRule}");
                            if (game.FilePath is not null)
                                Console.WriteLine($"* File path={game.FilePath}");
                            if (game.PathRule is not null)
                                Console.WriteLine($"* File path rule={game.PathRule}");

                            if (i < autoGames.Length - 1)
                                Console.WriteLine();
                        }
                    }

                    if (showPresets)
                        Console.WriteLine();
                }
            }

            if (showPresets)
            {
                var presets = GamePresets.GetPreMadePresets();
                if (presets.Count > 0)
                {
                    anyGamesDisplayed = true;
                    Console.WriteLine("--- Presets ---");

                    if (!verbose)
                    {
                        for (var i = 0; i < presets.Count; ++i)
                        {
                            var game = presets[i];
                            Console.WriteLine($"{i + 1}. {TimeSpan.FromSeconds(game.PlayTime)} - {game.Name}");
                        }
                    }
                    else
                    {
                        for (var i = 0; i < presets.Count; ++i)
                        {
                            var game = presets[i];
                            Console.WriteLine($"{i + 1}. {TimeSpan.FromSeconds(game.PlayTime)} - {game.Name}");

                            Console.WriteLine("  Matching rules:");
                            if (game.WindowTitle is not null)
                                Console.WriteLine($"* Window title={game.WindowTitle}");
                            if (game.WindowRule is not null)
                                Console.WriteLine($"* Window title rule={game.WindowRule}");
                            if (game.FilePath is not null)
                                Console.WriteLine($"* File path={game.FilePath}");
                            if (game.PathRule is not null)
                                Console.WriteLine($"* File path rule={game.PathRule}");

                            if (i < presets.Count - 1)
                                Console.WriteLine();
                        }
                    }
                }
            }

            if (!anyGamesDisplayed)
            {
                Console.WriteLine("[INFO] No games or presets found which to list");
            }

            return 0;
        });

        return cmd;
    }
}