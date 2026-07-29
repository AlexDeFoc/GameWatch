using System;
using Spectre.Console.Cli;

namespace GameWatch.Client.Cli;

public static class Program
{
    public static int Main(string[] args)
    {
        Core.Helpers.DbFactory.GameLibrary.InitializeDatabase("../../UserData");

        var app = new CommandApp();

        app.Configure(cfg =>
        {
            cfg.SetApplicationVersion("1.0.0");

            cfg.AddBranch("list", list =>
            {
                list.SetDescription("List recorded games, game details, or active processes\n    Note: If no flags are provided, the cmd will list all games from all user created collections in simplified form\n    Alias: ls");
                list.AddCommand<Cmds.ListGames>("games")
                    .WithDescription("List tracked game records");
                list.AddCommand<Cmds.ListProcs>("procs")
                    .WithDescription("List active running processes available for tracking");
            }).WithAlias("ls");

            cfg.AddCommand<Cmds.ToggleGame>("toggle")
               .WithAlias("tg")
               .WithDescription("Start or stop a certain manual game record\n    Alias: tg");

            cfg.AddBranch("add", add =>
            {
                add.SetDescription("Command for adding");
                add.AddBranch("game", game =>
                {
                    game.SetDescription("Command for adding games");
                    game.AddCommand<Cmds.AddManualGame>("manual").WithDescription("Command for adding a game in manual mode");

                    game.AddBranch("auto", auto =>
                    {
                        auto.AddExample("add", "game", "auto", "proc", "--id 20 --title \"Among Us\" --match-title");
                        auto.AddExample("add", "game", "auto", "proc", "--id 10 --title FortniteTournamentsInstance --match-path --title-pattern \"^Fortnite\\b.*\"");
                        auto.SetDescription("Command for adding a game in auto mode");
                        auto.AddCommand<Cmds.AddAutoGameFromProcess>("proc").WithDescription("Command for adding a game in auto mode and sourcing properties from an active game");
                        auto.AddCommand<Cmds.AddAutoGameFromProcess>("preset").WithDescription("Command for adding a game in auto mode and sourcing properties from an existing preset");
                    });
                });
            });

            cfg.AddCommand<Cmds.RemoveGame>("remove")
               .WithAlias("rm")
               .WithDescription("Remove a single game record from a certain game collection\n    Alias: rm");

            cfg.AddCommand<Cmds.ClearGames>("clear")
               .WithAlias("cl")
               .WithDescription("Clear all records from a certain game collection\n    Alias: cl");

            cfg.AddBranch("update", update =>
            {
                update.SetDescription("Update application binaries or preset databases\n    Alias: up");
                update.AddCommand<Cmds.UpdateApp>("app")
                      .WithDescription("Check for and install application updates");
                update.AddCommand<Cmds.UpdatePresets>("presets")
                      .WithDescription("Download the latest community game presets");
            }).WithAlias("up");
        });

        return app.Run(args);
    }
}