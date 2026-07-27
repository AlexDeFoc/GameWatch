using Spectre.Console.Cli;

namespace GameWatch.Client.Cli;

public static class Program
{
    public static int Main(string[] args)
    {
        Helpers.DbFactory.GameLibrary.InitializeDatabase();

        var app = new CommandApp();

        app.Configure(cfg =>
        {
            cfg.SetApplicationVersion("1.0.0");

            cfg.AddBranch("list", list =>
            {
                list.SetDescription("List recorded games, game details, or active processes.");
                list.AddCommand<Cmds.ListGames>("games")
                    .WithDescription("List tracked game records.");
                list.AddCommand<Cmds.ShowGame>("game")
                    .WithDescription("Display detailed properties for a specific game.");
                list.AddCommand<Cmds.ListProcs>("procs")
                    .WithDescription("List active running processes available for tracking.");
            }).WithAlias("ls");

            cfg.AddBranch("add", add =>
            {
                add.SetDescription("Command for adding.");
                add.AddBranch("game", game =>
                {
                    game.SetDescription("Command for adding games.");
                    game.AddCommand<Cmds.AddManualGame>("manual").WithDescription("Command for adding a game in manual mode.");
                });
            });

            cfg.AddCommand<Cmds.RemoveGame>("remove")
               .WithAlias("rm")
               .WithDescription("Remove a single game record or clear all records.");

            cfg.AddBranch("update", update =>
            {
                update.SetDescription("Update application binaries or preset databases.");
                update.AddCommand<Cmds.UpdateApp>("app")
                      .WithDescription("Check for and install application updates.");
                update.AddCommand<Cmds.UpdatePresets>("presets")
                      .WithDescription("Download the latest community game presets.");
            }).WithAlias("up");
        });

        return app.Run(args);
    }
}