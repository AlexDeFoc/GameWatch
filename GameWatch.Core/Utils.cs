using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Core.Dbs;
using GameWatch.Core.Types;
using Microsoft.Data.Sqlite;

namespace GameWatch.Core;

public static class Utils
{
    public static bool IsWithinBounds(DisplayId i, List<TableId> collection) => i.V >= 1 && i.V - 1 < collection.Count;

    public static Task<List<TableId>> FetchTableIdsAsync(SqliteConnection conn, GameMode gameMode, CancellationToken cancellationToken)
        => FetchTableIdsByTableAsync(conn, GetTableName(gameMode), cancellationToken);

    public static Task<List<TableId>> FetchTableIdsAsync(SqliteConnection conn, GamePresets.PresetNames preset, CancellationToken cancellationToken)
        => FetchTableIdsByTableAsync(conn, GetTableName(preset), cancellationToken);

    public static string GetTableName(GameMode gameMode) => gameMode switch
    {
        GameMode.Manual => "ManualGames",
        GameMode.Auto => "AutoGames",
        _ => throw new ArgumentOutOfRangeException(nameof(gameMode), gameMode, "Unsupported game mode provided.")
    };

    public static AutoGameDto ReadAutoGame(SqliteDataReader r) => new()
    {
        TableId = r.GetInt32(0),
        Name = r.GetString(1),
        PlayTime = r.GetInt64(2),
        WindowTitle = r.IsDBNull(3) ? null : r.GetString(3),
        FilePath = r.IsDBNull(4) ? null : r.GetString(4),
        WindowRule = r.IsDBNull(5) ? null : r.GetString(5),
        PathRule = r.IsDBNull(6) ? null : r.GetString(6),
    };

    public static AutoGameDto ReadAutoGamePreset(SqliteDataReader r) => new()
    {
        TableId = r.GetInt32(0),
        Name = r.GetString(1),
        WindowTitle = r.IsDBNull(2) ? null : r.GetString(2),
        FilePath = r.IsDBNull(3) ? null : r.GetString(3),
        WindowRule = r.IsDBNull(4) ? null : r.GetString(4),
        PathRule = r.IsDBNull(5) ? null : r.GetString(5),
    };

    public static ManualGameDto ReadManualGame(SqliteDataReader r) => new()
    {
        TableId = r.GetInt32(0),
        Name = r.GetString(1),
        PlayTime = r.GetInt64(2)
    };

    public static async Task<int> ExecuteNonQueryAsync(SqliteConnection conn, string sql, CancellationToken cancellationToken, SqliteTransaction? tran = null)
    {
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = sql;

        if (tran != null) cmd.Transaction = tran;

        return await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public static async Task<SqliteConnection> CreateSqlConnAsync(string connStr, CancellationToken cancellationToken, bool queryOnly = false)
    {
        var conn = new SqliteConnection(connStr);
        try
        {
            conn.DefaultTimeout = 30;
            await conn.OpenAsync(cancellationToken);

            var sb = new StringBuilder();
            sb.Append("""
                      PRAGMA synchronous = NORMAL;
                      PRAGMA busy_timeout = 30000;
                      PRAGMA temp_store = MEMORY;
                      PRAGMA foreign_keys = ON;
                      """);

            if (queryOnly)
                sb.Append("PRAGMA query_only = ON;");

            await ExecuteNonQueryAsync(conn, sb.ToString(), cancellationToken: cancellationToken);
            return conn;
        }
        catch
        {
            await conn.DisposeAsync();
            throw;
        }
    }


    public static async Task<AutoGameRecord[]> QueryAutoGamesAsync(SqliteConnection conn, CancellationToken cancellationToken)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
                          SELECT
                              Id,
                              Name,
                              PlayTime,
                              WindowTitle,
                              FilePath,
                              WindowRule,
                              PathRule
                          FROM AutoGames
                          ORDER BY Id ASC;
                          """;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var list = new List<AutoGameRecord>();

        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(new AutoGameRecord(ReadAutoGame(reader)));
        }

        return [.. list];
    }

    public static async Task<List<ManualGameRecord>> QueryManualGamesAsync(SqliteConnection conn, CancellationToken cancellationToken)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
                          SELECT
                              Id,
                              Name,
                              PlayTime
                          FROM ManualGames
                          ORDER BY Id ASC;
                          """;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        var list = new List<ManualGameRecord>();

        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(new ManualGameRecord(ReadManualGame(reader)));
        }

        return list;
    }

    private static async Task<List<TableId>> FetchTableIdsByTableAsync(SqliteConnection conn, string tableName, CancellationToken cancellationToken)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT Id FROM {tableName} ORDER BY Id ASC;";

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var list = new List<TableId>();

        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(new TableId(reader.GetInt32(0)));
        }

        return list;
    }

    private static string GetTableName(GamePresets.PresetNames preset) => preset switch
    {
        GamePresets.PresetNames.AutoGame => "AutoGamePresets",
        _ => throw new ArgumentOutOfRangeException(nameof(preset), preset, "Unsupported preset provided.")
    };
}