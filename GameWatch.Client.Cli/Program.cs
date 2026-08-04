using System.CommandLine;
using System.Threading.Tasks;
using Command = System.CommandLine.Command;

namespace GameWatch.Client.Cli;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        Core.Helpers.DbFactory.GameLibrary.InitializeDatabase("../../UserData");

        // =================
        // Branch: list / ls
        // =================
        var listCmd = new Command("list",
                                  "List recorded games, Game details, or active processes\n" +
                                  "Note: If no flags are provided, the cmd will list all games from all user created collections in simplified form")
        {
            Cmds.ListGames.Build(),
            Cmds.ListProcs.Build()
        };
        listCmd.Aliases.Add("ls");

        // ===========
        // Branch: add
        // ===========
        // Sub-branch: add auto
        var addAutoCmd = new Command("auto", "Command for adding a Game in auto mode")
        {
            Cmds.AddAutoGameFromProcess.Build(),
            Cmds.AddAutoGameFromPreset.Build()
        };

        var addCmd = new Command("add", "Command for adding games")
        {
            // Sub-cmd: add manual
            Cmds.AddManualGame.Build(),
            addAutoCmd
        };

        // ===========
        // Branch: edit
        // ===========
        var editCmd = new Command("edit", "Command for editing Game recorded properties")
        {
            Cmds.EditManualGame.Build(),
            Cmds.EditAutoGame.Build()
        };

        var rootCmd = new RootCommand("GameWatch CLI Client - Control and query active tracking routines.")
        {
            // =============================================
            // Direct commands: remove, clear, reset, update
            // =============================================
            Cmds.RemoveGame.Build(),
            Cmds.DeleteAllGames.Build(),
            Cmds.ResetGame.Build(),
            Cmds.UpdateApp.Build(),
            Cmds.ToggleGame.Build(),
            editCmd,
            addCmd,
            listCmd
        };

        // Parse & execute
        var parseResult = rootCmd.Parse(args);
        return await parseResult.InvokeAsync();
    }
}