using System;
using System.CommandLine;
using GameWatch.Core;

namespace GameWatch.Client.Cli.Cmds;

public static class ListGames
{
    public static Command Build()
    {
        var verboseOption = new Option<bool>("--verbose", "-v")
        {
            Description = "List games with additional detailed matching rule metadata."
        };

        var manualOnlyOption = new Option<bool>("--manual-only", "-m")
        {
            Description = "Display only manual game records."
        };

        var autoOnlyOption = new Option<bool>("--auto-only", "-a")
        {
            Description = "Display only auto game records."
        };

        var showPresetsOption = new Option<bool>("--show-presets", "-p")
        {
            Description = "Additionally display presets"
        };

        var cmd = new Command("games", "List all or filtered games in the library")
        {
            verboseOption,
            manualOnlyOption,
            autoOnlyOption,
            showPresetsOption
        };

        cmd.Aliases.Add("g");


        cmd.Validators.Add(result =>
        {
            var isManual = result.GetValue(manualOnlyOption);
            var isAuto = result.GetValue(autoOnlyOption);

            if (isManual && isAuto)
            {
                result.AddError("⛔ --manual-only and --auto-only are mutually exclusive flags.");
            }
        });

        cmd.SetAction(parseResult =>
        {
            var verbose = parseResult.GetValue(verboseOption);
            var manualOnly = parseResult.GetValue(manualOnlyOption);
            var autoOnly = parseResult.GetValue(autoOnlyOption);
            var shouldShowPresets = parseResult.GetValue(showPresetsOption);

            var shouldDisplayBothGameModes = !manualOnly && !autoOnly;
            var gamesHaveBeenDisplayed = false;

            if (shouldDisplayBothGameModes || manualOnly)
            {
                var manualGames = DbMng.GameLibrary.GetManualGames();

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

            if (shouldDisplayBothGameModes || autoOnly)
            {
                var autoGames = DbMng.GameLibrary.GetAutoGames();

                if (autoGames.Count > 0)
                {
                    if (shouldDisplayBothGameModes && gamesHaveBeenDisplayed)
                        Console.WriteLine();

                    gamesHaveBeenDisplayed = true;
                    Console.WriteLine("--- Auto games ---");

                    if (verbose)
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

                            if (game.PathRule != null)
                            {
                                Console.WriteLine("      File Path: should match using pattern");
                                Console.WriteLine($"         Value: {game.PathRule}");
                            }
                        }
                    }
                    else
                    {
                        foreach (var game in autoGames)
                        {
                            Console.WriteLine($"{game.Id}. {TimeSpan.FromSeconds(game.PlayTimeSec.V)} - {game.Name}");
                        }
                    }
                }
            }

            if (shouldShowPresets)
            {
                if (gamesHaveBeenDisplayed)
                    Console.WriteLine();

                var presets = DbMng.GameLibraryPresets.GetPresets();

                if (presets.Count > 0)
                {
                    gamesHaveBeenDisplayed = true;
                    Console.WriteLine("--- Auto game presets ---");

                    if (verbose)
                    {
                        foreach (var game in presets)
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

                            if (game.PathRule != null)
                            {
                                Console.WriteLine("      File Path: should match using pattern");
                                Console.WriteLine($"         Value: {game.PathRule}");
                            }
                        }
                    }
                    else
                    {
                        foreach (var game in presets)
                        {
                            Console.WriteLine($"{game.Id}. {TimeSpan.FromSeconds(game.PlayTimeSec.V)} - {game.Name}");
                        }
                    }
                }
            }

            if (!gamesHaveBeenDisplayed)
            {
                Console.WriteLine("ℹ️ No games found which to list");
            }

            return 0;
        });

        return cmd;
    }
}