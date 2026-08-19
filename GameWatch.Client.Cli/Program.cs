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

            var rootCmd = await BuildRootCommandAsync(cts.Token);
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
        await GameLibrary.CreateAndInitAsync("../../UserData", cancellationToken);
        await GamePresets.CreateAndInitAsync("../../AppData", cancellationToken);
        await Settings.CreateAndInitAsync("../../AppData", cancellationToken);
    }

    private static async Task<RootCommand> BuildRootCommandAsync(CancellationToken cancellationToken)
    {
        var rootCmd = new RootCommand("GameWatch CLI Client - Control and query active tracking routines.");

        var removeTask = Cmds.RemoveGame.BuildAsync(cancellationToken);
        var deleteAllTask = Cmds.DeleteAllGames.BuildAsync(cancellationToken);
        var resetTask = Cmds.ResetGame.BuildAsync(cancellationToken);
        var updateTask = Cmds.UpdateApp.BuildAsync(cancellationToken);
        var toggleTask = Cmds.ToggleGame.BuildAsync(cancellationToken);

        var editTask = BuildEditCommandAsync(cancellationToken);
        var addTask = BuildAddCommandAsync(cancellationToken);
        var listTask = BuildListCommandAsync(cancellationToken);

        await Task.WhenAll(
            removeTask,
            deleteAllTask,
            resetTask,
            updateTask,
            toggleTask,
            editTask,
            addTask,
            listTask
        );

        rootCmd.Add(removeTask.Result);
        rootCmd.Add(deleteAllTask.Result);
        rootCmd.Add(resetTask.Result);
        rootCmd.Add(updateTask.Result);
        rootCmd.Add(toggleTask.Result);
        rootCmd.Add(editTask.Result);
        rootCmd.Add(addTask.Result);
        rootCmd.Add(listTask.Result);

        return rootCmd;
    }

    private static async Task<Command> BuildListCommandAsync(CancellationToken cancellationToken)
    {
        var listCmd = new Command(
            name: "list",
            description: "List recorded games, Game details, or active processes\n" +
                         "Note: If no flags are provided, the cmd will list all games from all user created collections in simplified form"
        );
        listCmd.Aliases.Add("ls");

        var listGamesTask = Cmds.ListGames.BuildAsync(cancellationToken);
        var listProcsTask = Cmds.ListProcs.BuildAsync(cancellationToken);

        await Task.WhenAll(listGamesTask, listProcsTask);

        listCmd.Add(listGamesTask.Result);
        listCmd.Add(listProcsTask.Result);

        return listCmd;
    }

    private static async Task<Command> BuildAddCommandAsync(CancellationToken cancellationToken)
    {
        var addAutoGameCmd = new Command("auto", "Command for adding a Game in auto mode");
        addAutoGameCmd.Aliases.Add("a");

        var addFromPresetTask = Cmds.AddAutoGameFromPreset.BuildAsync(cancellationToken);
        var addFromProcTask = Cmds.AddAutoGameFromProcess.BuildAsync(cancellationToken);

        await Task.WhenAll(addFromPresetTask, addFromProcTask);

        addAutoGameCmd.Add(addFromPresetTask.Result);
        addAutoGameCmd.Add(addFromProcTask.Result);

        var addManualGameTask = Cmds.AddManualGame.BuildAsync(cancellationToken);

        var addCmd = new Command("add", "Command for adding games");

        var addManualGameCmd = await addManualGameTask;

        addCmd.Add(addManualGameCmd);
        addCmd.Add(addAutoGameCmd);

        return addCmd;
    }

    private static async Task<Command> BuildEditCommandAsync(CancellationToken cancellationToken)
    {
        var cmd = new Command("edit", "Command for editing Game recorded properties");

        var editManualGameTask = Cmds.EditManualGame.BuildAsync(cancellationToken);
        var editAutoGameTask = Cmds.EditAutoGame.BuildAsync(cancellationToken);

        await Task.WhenAll(editManualGameTask, editAutoGameTask);

        cmd.Add(editManualGameTask.Result);
        cmd.Add(editAutoGameTask.Result);

        return cmd;
    }
}