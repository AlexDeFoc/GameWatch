using System;
using System.Collections.Generic;
using System.Text;
using GameWatch.Core.Wrappers;
using Microsoft.Data.Sqlite;

namespace GameWatch.Core;

public static class Utils
{
    public static int ExecuteNonQuery(SqliteConnection conn, string sql, SqliteTransaction? tran = null)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        if (tran != null) cmd.Transaction = tran;
        return cmd.ExecuteNonQuery();
    }

    public static SqliteConnection CreateSqlConnection(string connStr, bool queryOnly = false)
    {
        var conn = new SqliteConnection(connStr);
        conn.DefaultTimeout = 30;
        conn.Open();

        var sb = new StringBuilder();

        sb.Append("""
                  PRAGMA synchronous = NORMAL;
                  PRAGMA busy_timeout = 30000;
                  PRAGMA temp_store = MEMORY;
                  PRAGMA foreign_keys = ON;
                  """);

        if (queryOnly)
            sb.Append("PRAGMA query_only = ON;");

        ExecuteNonQuery(conn, sb.ToString());

        return conn;
    }

    public static List<TableId> GetDbTableIds(SqliteConnection conn, string tableName)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT Id FROM {tableName} ORDER BY Id ASC;";

        using var reader = cmd.ExecuteReader();
        var list = new List<TableId>();
        while (reader.Read())
        {
            list.Add(new TableId(reader.GetInt32(0)));
        }

        return list;
    }

    public static Dto.AutoGame QueryAutoGameDtoFromDb(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0),
        Name = r.GetString(1),
        PlayTimeSec = r.GetInt64(2),
        WindowTitle = r.IsDBNull(3) ? null : r.GetString(3),
        FilePath = r.IsDBNull(4) ? null : r.GetString(4),
        WindowRule = r.IsDBNull(5) ? null : r.GetString(5),
        PathRule = r.IsDBNull(6) ? null : r.GetString(6),
    };

    public static Dto.ManualGame QueryManualGameDtoFromDb(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0),
        Name = r.GetString(1),
        PlayTimeSec = r.GetInt64(2)
    };

    public static List<GameRecords.AutoGame> QueryAutoGamesFromDb(SqliteConnection conn)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
                          SELECT
                              Id,
                              Name,
                              PlayTimeSec,
                              WindowTitle,
                              FilePath,
                              WindowRule,
                              PathRule
                          FROM AutoGames
                          ORDER BY Id ASC;
                          """;

        using var reader = cmd.ExecuteReader();
        var list = new List<GameRecords.AutoGame>();

        var displayIdValue = 1;
        while (reader.Read())
        {
            var dto = QueryAutoGameDtoFromDb(reader);

            list.Add(new GameRecords.AutoGame(dto, new DisplayId(displayIdValue)));

            displayIdValue++;
        }

        return list;
    }

    public static List<GameRecords.ManualGame> QueryManualGamesFromDb(SqliteConnection conn)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
                          SELECT
                              Id,
                              Name,
                              PlayTimeSec
                          FROM ManualGames
                          ORDER BY Id ASC;
                          """;

        using var reader = cmd.ExecuteReader();
        var list = new List<GameRecords.ManualGame>();

        var displayIdValue = 1;
        while (reader.Read())
        {
            var dto = QueryManualGameDtoFromDb(reader);

            list.Add(new GameRecords.ManualGame(dto, new DisplayId(displayIdValue)));

            displayIdValue++;
        }

        return list;
    }

    public static bool IsDisplayIdWithinBounds(DisplayId i, List<TableId> list) => i.V >= 0 && i.V < list.Count;

    public static string GetTableName(GameMode gameMode) => gameMode switch
    {
        GameMode.Manual => "ManualGames",
        GameMode.Auto => "AutoGames",
        _ => throw new ArgumentOutOfRangeException(nameof(gameMode), gameMode, "Unsupported Game mode.")
    };
}