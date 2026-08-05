using System.CommandLine;
using System.Threading.Tasks;
using GameWatch.Core;

namespace GameWatch.Client.Cli;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        InitializeDatabases();

        var rootCmd = BuildRootCommand();
        return await rootCmd.Parse(args).InvokeAsync();
    }

    private static void InitializeDatabases()
    {
        DbMng.GameLibrary.InitializeDatabase("../../UserData");
        DbMng.GameLibraryPresets.InitializeDatabase("../../AppData");
    }

    private static RootCommand BuildRootCommand()
    {
        var rootCmd = new RootCommand("GameWatch CLI Client - Control and query active tracking routines.")
        {
            Cmds.RemoveGame.Build(),
            Cmds.DeleteAllGames.Build(),
            Cmds.ResetGame.Build(),
            Cmds.UpdateApp.Build(),
            Cmds.ToggleGame.Build(),
            BuildEditCommand(),
            BuildAddCommand(),
            BuildListCommand()
        };

        return rootCmd;
    }

    private static Command BuildListCommand()
    {
        var listCmd = new Command(
            name: "list",
            description: "List recorded games, Game details, or active processes\n" +
                         "Note: If no flags are provided, the cmd will list all games from all user created collections in simplified form"
        )
        {
            Cmds.ListGames.Build(),
            Cmds.ListProcs.Build()
        };
        listCmd.Aliases.Add("ls");

        return listCmd;
    }

    private static Command BuildAddCommand()
    {
        var addAutoCmd = new Command("auto", "Command for adding a Game in auto mode")
        {
            Cmds.AddAutoGameFromProcess.Build(),
            Cmds.AddAutoGameFromPreset.Build()
        };
        addAutoCmd.Aliases.Add("a");

        return new Command("add", "Command for adding games")
        {
            Cmds.AddManualGame.Build(),
            addAutoCmd
        };
    }

    private static Command BuildEditCommand()
    {
        return new Command("edit", "Command for editing Game recorded properties")
        {
            Cmds.EditManualGame.Build(),
            Cmds.EditAutoGame.Build()
        };
    }
}