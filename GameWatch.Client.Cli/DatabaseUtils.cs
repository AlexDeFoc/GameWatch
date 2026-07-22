using System;
using Dapper;
using Microsoft.Data.Sqlite;
using System.IO;

namespace GameWatch.Client.Cli;

internal static class DatabaseUtils
{
  internal const string GameLibraryDbFilePath = "../../UserData/GameLibrary.db";

  internal static SqliteConnection GetOpenConnection(string dbPath)
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

  internal static void EnsureDatabaseCreated(string dbPath)
  {
    using var connection = GetOpenConnection(dbPath);
    using var transaction = connection.BeginTransaction();


    string createTableSql;
    if (dbPath == GameLibraryDbFilePath)
    {
      createTableSql = """
                       CREATE TABLE IF NOT EXISTS Games (
                         Id INTEGER PRIMARY KEY AUTOINCREMENT,
                         Title TEXT NOT NULL,
                         PlayTime INTEGER DEFAULT 0
                       );
                       """;
    }
    else
    {
      throw new NotImplementedException("Unimplemented!");
    }

    connection.Execute(createTableSql, transaction: transaction);
    transaction.Commit();
  }
}