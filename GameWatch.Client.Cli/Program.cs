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
                list.SetDescription("List recorded games, Game details, or active processes\n    Note: If no flags are provided, the cmd will list all games from all user created collections in simplified form\n    Alias: ls");
                list.AddCommand<Cmds.ListGames>("games")
                    .WithDescription("List tracked Game records");
                list.AddCommand<Cmds.ListProcs>("procs")
                    .WithDescription("List active running processes available for tracking");
            }).WithAlias("ls");

            cfg.AddCommand<Cmds.ToggleGame>("toggle")
               .WithAlias("tg")
               .WithDescription("Start or stop a certain manual Game record\n    Alias: tg");

            cfg.AddBranch("add", add =>
            {
                add.SetDescription("Command for adding games");
                add.AddCommand<Cmds.AddManualGame>("manual").WithDescription("Command for adding a Game in manual mode");

                add.AddBranch("auto", auto =>
                {
                    auto.AddExample("add", "Game", "auto", "proc", "--id 20 --title \"Among Us\" --match-title");
                    auto.AddExample("add", "Game", "auto", "proc", "--id 10 --title FortniteTournamentsInstance --match-path --title-pattern \"^Fortnite\\b.*\"");
                    auto.SetDescription("Command for adding a Game in auto mode");
                    auto.AddCommand<Cmds.AddAutoGameFromProcess>("proc").WithDescription("Command for adding a Game in auto mode and sourcing properties from an active Game");
                    auto.AddCommand<Cmds.AddAutoGameFromProcess>("preset").WithDescription("Command for adding a Game in auto mode and sourcing properties from an existing preset");
                });
            });

            cfg.AddBranch("edit", edit =>
            {
                edit.SetDescription("Command for editing Game record properties");
                edit.AddCommand<Cmds.EditManualGame>("manual").WithDescription("Command for editing manual Game properties");
                edit.AddCommand<Cmds.EditAutoGame>("auto").WithDescription("Command for editing auto Game properties");
            });

            cfg.AddCommand<Cmds.RemoveGame>("remove")
               .WithAlias("rm")
               .WithDescription("Remove a single Game record from a certain Game collection\n    Alias: rm");

            cfg.AddCommand<Cmds.DeleteAllGames>("clear")
               .WithAlias("cl")
               .WithDescription("Clear all records from a certain Game collection\n    Alias: cl");

            cfg.AddCommand<Cmds.ResetGame>("reset")
               .WithAlias("rs")
               .WithDescription("Reset a single Game record from a certain Game collection\n    Alias: rs");

            cfg.AddCommand<Cmds.UpdateApp>("update")
               .WithAlias("up")
               .WithDescription("Check for and install application updates\n    Alias: up");
        });

        return app.Run(args);
    }
}