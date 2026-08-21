using System;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Core.Dbs;

namespace GameWatch.Client.Cli;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        using var cts = new CancellationTokenSource();

        Console.CancelKeyPress += CancelHandler;

        try
        {
            await InitializeDatabasesAsync(cts.Token);

            var rootCmd = BuildRootCommand();
            return await rootCmd.Parse(args).InvokeAsync(cancellationToken: cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\n[INFO] Operation cancelled by user.");
            return 130; // Standard exit code for SIGINT/Ctrl+C
        }
        finally
        {
            Console.CancelKeyPress -= CancelHandler;
        }

        void CancelHandler(object? _, ConsoleCancelEventArgs e)
        {
            e.Cancel = true; // Prevent abrupt process termination
            // ReSharper disable once AccessToDisposedClosure
            cts.Cancel();
        }
    }

    private static async Task InitializeDatabasesAsync(CancellationToken cancellationToken)
    {
        var libraryTask = GameLibrary.CreateAndInitAsync("../../UserData", cancellationToken);
        var presetsTask = GamePresets.CreateAndInitAsync("../../AppData", cancellationToken);
        var settingsTask = Settings.GameMonitorAgent.CreateAndInitAsync("../../AppData", cancellationToken);

        await Task.WhenAll(libraryTask, presetsTask, settingsTask);
    }

    private static RootCommand BuildRootCommand()
    {
        var rootCmd = new RootCommand("GameWatch CLI Client - Control and query active tracking routines.")
        {
            BuildListCommand(),
            Cmds.ToggleGame.Build(),
            BuildEditCommand(),
            BuildAddCommand(),
            Cmds.RemoveGame.Build(),
            Cmds.ResetGame.Build(),
            Cmds.DeleteAllGames.Build(),
            Cmds.UpdateApp.Build(),
            BuildConfigCommand()
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
        var addAutoGameCmd = new Command("auto", "Auto game")
        {
            Cmds.AddAutoGameFromPreset.Build(),
            Cmds.AddAutoGameFromProcess.Build()
        };

        addAutoGameCmd.Aliases.Add("a");

        var addCmd = new Command("add", "Add game");

        var addManualGameCmd = Cmds.AddManualGame.Build();

        addCmd.Add(addManualGameCmd);
        addCmd.Add(addAutoGameCmd);

        return addCmd;
    }

    private static Command BuildEditCommand()
    {
        return new Command("edit", "Edit game")
        {
            Cmds.EditManualGame.Build(),
            Cmds.EditAutoGame.Build()
        };
    }

    private static Command BuildConfigCommand()
    {
        var setCmd = new Command("set", "Configure setting")
        {
            Cmds.Config.GameMonitorAgent.Set.ShouldLoggingBeEnabled.Build(),
            Cmds.Config.GameMonitorAgent.Set.ProcessScanInterval.Build(),
            Cmds.Config.GameMonitorAgent.Set.PlayTimeFlushInterval.Build()
        };

        var resetCmd = new Command("reset", "Reset setting to default value")
        {
            Cmds.Config.GameMonitorAgent.Reset.ShouldLoggingBeEnabled.Build(),
            Cmds.Config.GameMonitorAgent.Reset.ProcessScanInterval.Build(),
            Cmds.Config.GameMonitorAgent.Reset.PlayTimeFlushInterval.Build(),
        };

        var gameMonitorAgentCmd = new Command("GameMonitorAgent", "Modify game monitor agent settings")
        {
            Cmds.Config.GameMonitorAgent.List.Build(),
            setCmd,
            resetCmd
        };

        return new Command("config", "Modify settings")
        {
            gameMonitorAgentCmd
        };
    }
}