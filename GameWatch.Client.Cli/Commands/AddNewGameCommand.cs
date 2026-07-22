// ReSharper disable ClassNeverInstantiated.Global

using System.ComponentModel;
using System.Threading;
using Dapper;
using Spectre.Console.Cli;

namespace GameWatch.Client.Cli.Commands;

internal sealed class AddNewGameCommand : Command<AddNewGameCommand.Settings>
{
  internal class Settings : CommandSettings
  {
    [CommandOption("-t|--title", isRequired: true)]
    [Description("How should the game be called")]
    public required string Title { get; init; }

    [CommandOption("-p|--playtime", isRequired: false)]
    [Description("Starting game playtime")]
    [DefaultValue(0)]
    public required int PlayTime { get; init; }
  }

  protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
  {
    const string dbPath = DatabaseUtils.GameLibraryDbFilePath;

    DatabaseUtils.EnsureDatabaseCreated(dbPath);

    using var connection = DatabaseUtils.GetOpenConnection(dbPath);
    using var transaction = connection.BeginTransaction();

    const string insertSql = "INSERT INTO Games (Title, PlayTime) VALUES (@Title, @PlayTime);";
    connection.Execute(insertSql, new GameEntry { Title = settings.Title, PlayTime = settings.PlayTime });

    transaction.Commit();

    return 0;
  }
}