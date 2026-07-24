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
                                        PRAGMA foreign_keys = ON;
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
                             -- Core table for all games
                             CREATE TABLE IF NOT EXISTS Games (
                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                Title TEXT NOT NULL,
                                PlayTime INTEGER NOT NULL DEFAULT 0
                             );

                             -- Extension table (presence here means SavingMode = Auto)
                             CREATE TABLE IF NOT EXISTS AutoSavingGames(
                                GameId INTEGER PRIMARY KEY
                                ExePath TEXT NOT NULL,
                                FOREIGN KEY (GameId) REFERENCES Games(Id) ON DELETE CASCADE
                             );

                             -- Unified view mapping the enum states and path
                             CREATE VIEW IF NOT EXISTS View_AllGames AS
                             SELECT
                                g.Id,
                                g.Title,
                                g.PlayTime,
                                CASE
                                    WHEN a.GameId IS NOT NULL THEN 'Auto'
                                    ELSE 'Manual'
                                END AS SavingMode,
                                a.ExePath
                             FROM Games AS g
                             LEFT JOIN AutoSavingGames AS a ON g.Id = a.GameId;
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