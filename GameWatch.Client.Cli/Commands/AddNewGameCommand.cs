// ReSharper disable ClassNeverInstantiated.Global

using System.ComponentModel;
using System.IO;
using System.Threading;
using Dapper;
using Microsoft.Data.Sqlite;
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
    const string dbPath = "../../UserData/GameLibrary.db";

    EnsureDatabaseCreated(dbPath);

    using var connection = GetOpenConnection(dbPath);
    using var transaction = connection.BeginTransaction();

    const string insertSql = "INSERT INTO Games (Title, PlayTime) VALUES (@Title, @PlayTime);";
    connection.Execute(insertSql, new { Title = settings.Title, PlayTime = settings.PlayTime });

    transaction.Commit();

    return 0;
  }

  private static SqliteConnection GetOpenConnection(string dbPath)
  {
    // Ensure directory exists
    var fileInfo = new FileInfo(dbPath);
    if (fileInfo.Directory is { Exists: false })
    {
      fileInfo.Directory.Create();
    }

    var connection = new SqliteConnection($"Data Source={dbPath}");
    connection.Open();

    const string configurePragmas = """
                                    PRAGMA journal_mode = WAL;
                                    PRAGMA synchronous = FULL;
                                    PRAGMA busy_timeout = 5000;
                                    PRAGMA temp_store = MEMORY;
                                    """;

    connection.Execute(configurePragmas);

    return connection;
  }

  private static void EnsureDatabaseCreated(string dbPath)
  {
    using var connection = GetOpenConnection(dbPath);
    using var transaction = connection.BeginTransaction();

    const string createTableSql = """
                                  CREATE TABLE IF NOT EXISTS Games (
                                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                    Title TEXT NOT NULL,
                                    PlayTime INTEGER DEFAULT 0
                                  );
                                  """;

    connection.Execute(createTableSql, transaction: transaction);
    transaction.Commit();
  }
}