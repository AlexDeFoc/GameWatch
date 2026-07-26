using Spectre.Console.Cli;

namespace GameWatch.Client.Cli;

public static class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandApp();

        app.Configure(cfg =>
        {
            cfg.AddBranch("list", list =>
            {
                list.SetDescription("Commands for listing and filtering tracked games.");
                list.AddCommand<CmdAction.ListGames>("games")
                    .WithDescription("Lists games. By default if no flags are provided it lists all games.");
            });

            cfg.AddBranch("find", find =>
            {
                find.SetDescription("Commands for discovering active running processes.");
                find.AddCommand<CmdAction.FindGames>("games")
                    .WithDescription("Searches and lists available active processes (games).");
            });

            cfg.AddBranch("add", add =>
            {
                add.SetDescription("Commands for adding new games to tracking.");
                add.AddBranch("game", game =>
                {
                    game.SetDescription("Add a game manually or via active process tracking.");
                    game.AddCommand<CmdAction.AddManualGame>("manual")
                        .WithDescription("Adds a game record, set on manual mode.");
                    game.AddCommand<CmdAction.AddAutoGame>("auto")
                        .WithDescription("Adds a game record, set on auto mode.");
                });
            });

            cfg.AddBranch("remove", remove =>
            {
                remove.SetDescription("Commands for removing tracked games.");
                remove.AddBranch("game", game =>
                {
                    game.SetDescription("Remove a game record.");
                    game.AddCommand<CmdAction.RemoveManualGame>("manual")
                        .WithDescription("Removes a manual game record.");
                    game.AddCommand<CmdAction.RemoveAutoGame>("auto")
                        .WithDescription("Removes an auto game record.");
                });
            });

            cfg.AddBranch("edit", edit =>
            {
                edit.SetDescription("Commands for modifying existing game data.");
                edit.AddBranch("set", set =>
                {
                    set.SetDescription("Set specific game attributes.");
                    set.AddCommand<CmdAction.EditSetGame>("game")
                       .WithDescription("Edit certain game properties.");
                });
            });

            cfg.AddBranch("update", update =>
            {
                update.SetDescription("Commands for updating the app or database presets.");
                update.AddCommand<CmdAction.UpdateApp>("app")
                      .WithDescription("Checks for available updates & performs it.");
                update.AddCommand<CmdAction.UpdateApp>("preset-games")
                      .WithDescription("Checks for available updates & performs it.");
            });
        });

        return app.Run(args);
    }
}