using System;
using System.Collections.Generic;
using System.IO;
using GameWatch.Core.Dto;
using GameWatch.Core.GameRecords;
using Microsoft.Data.Sqlite;

namespace GameWatch.Core.Dbs;

public sealed class GamePresets
{
    public static GamePresets Instance { get; private set; } = null!;

    private readonly string _connString;
    private readonly string _dbPath;

    public static void Init(string relPathToParent) => Instance = new GamePresets(relPathToParent);

    private readonly List<GameId> _presetIds = [];

    private GamePresets(string relPathToParent)
    {
        var dbParentPath = PathResolver.ResolveRelativePath(relPathToParent);
        _dbPath = Path.Join(dbParentPath, "GamePresets.db");
        _connString = $"Data Source={_dbPath};Mode=ReadOnly;";

        if (!string.IsNullOrEmpty(dbParentPath) && !Directory.Exists(dbParentPath))
            Directory.CreateDirectory(dbParentPath);

        if (!File.Exists(_dbPath))
        {
            CreateAndSeedDatabase();
        }

        using var conn = CreateReadOnlyConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
                          SELECT Id
                          FROM AutoGamePresets
                          ORDER BY Id ASC;
                          """;

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            _presetIds.Add(new GameId(reader.GetInt32(0)));
        }
    }

    public List<AutoGame> GetPresets()
    {
        using var conn = CreateReadOnlyConnection();
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
                          FROM AutoGamePresets
                          ORDER BY Id ASC;
                          """;

        using var reader = cmd.ExecuteReader();
        var result = new List<AutoGame>();

        while (reader.Read())
        {
            result.Add(ReadAutoGame(reader));
        }

        return result;
    }

    private static AutoGame ReadAutoGame(SqliteDataReader r) => new()
    {
        Id = new GameId(r.GetInt32(0)),
        Name = r.GetString(1),
        PlayTimeSec = new ElapsedTime(r.GetInt32(2)),
        WindowTitle = r.IsDBNull(3) ? null : r.GetString(3),
        FilePath = r.IsDBNull(4) ? null : r.GetString(4),
        WindowRule = r.IsDBNull(5) ? null : r.GetString(5),
        PathRule = r.IsDBNull(6) ? null : r.GetString(6),
    };

    public GetAutoGameByIdxResult GetPresetByIdx(GameIdx idx)
    {
        var id = GetPresetIdByIdx(idx);
        if (id == null)
        {
            return new GetAutoGameByIdxResult(
                HasSucceeded: false,
                Game: null,
                FailureReason: "[FAIL] Provided index is out of range. Ignoring command...");
        }

        using var conn = CreateReadOnlyConnection();
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
                          FROM AutoGamePresets
                          WHERE Id = @Id;
                          """;

        try
        {
            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
            {
                return new GetAutoGameByIdxResult(
                    HasSucceeded: false,
                    Game: null,
                    FailureReason: "[FAIL] Preset not found in database.");
            }

            return new GetAutoGameByIdxResult(
                HasSucceeded: true,
                Game: ReadAutoGame(reader),
                FailureReason: string.Empty);
        }
        catch (Exception ex)
        {
            return new GetAutoGameByIdxResult(
                HasSucceeded: false,
                Game: null,
                FailureReason: $"[FAIL] Database error msg: {ex.Message}");
        }
    }

    private GameId? GetPresetIdByIdx(GameIdx pos)
    {
        if (pos.V <= 0 || pos.V > _presetIds.Count) return null;
        return _presetIds[pos.V - 1];
    }

    private void CreateAndSeedDatabase()
    {
        var writeConnString = $"Data Source={_dbPath};Mode=ReadWriteCreate;";
        using var conn = new SqliteConnection(writeConnString);
        conn.Open();

        ExecuteNonQuery(conn, """
                              PRAGMA journal_mode = OFF;
                              PRAGMA synchronous = OFF;
                              PRAGMA temp_store = MEMORY;
                              """);

        using var tran = conn.BeginTransaction();

        ExecuteNonQuery(conn, """
                              CREATE TABLE IF NOT EXISTS AutoGamePresets (
                                  Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                  Name TEXT NOT NULL,
                                  PlayTimeSec INTEGER NOT NULL DEFAULT 0,
                                  WindowTitle TEXT,
                                  FilePath TEXT,
                                  WindowRule TEXT,
                                  PathRule TEXT
                              ) STRICT;
                              """, tran);

        var insertCmd = conn.CreateCommand();

        insertCmd.CommandText = """
                                INSERT INTO AutoGamePresets (
                                    Name,
                                    PlayTimeSec,
                                    WindowTitle,
                                    FilePath,
                                    WindowRule,
                                    PathRule
                                )
                                VALUES (
                                    @Name,
                                    @PlayTimeSec,
                                    @WindowTitle,
                                    @FilePath,
                                    @WindowRule,
                                    @PathRule
                                );
                                """;

        var pName = insertCmd.Parameters.Add("@Name", SqliteType.Text);
        var pPlayTime = insertCmd.Parameters.Add("@PlayTimeSec", SqliteType.Integer);
        var pWindowTitle = insertCmd.Parameters.Add("@WindowTitle", SqliteType.Text);
        var pFilePath = insertCmd.Parameters.Add("@FilePath", SqliteType.Text);
        var pWindowRule = insertCmd.Parameters.Add("@WindowRule", SqliteType.Text);
        var pPathRule = insertCmd.Parameters.Add("@PathRule", SqliteType.Text);

        var defaultPresets = GetHardcodedDefaultPresets();

        foreach (var g in defaultPresets)
        {
            pName.Value = g.Name;
            pPlayTime.Value = g.PlayTimeSec.V;
            pWindowTitle.Value = g.WindowTitle;
            pFilePath.Value = g.FilePath;
            pWindowRule.Value = g.WindowRule;
            pPathRule.Value = g.PathRule;

            insertCmd.ExecuteNonQuery();
        }

        tran.Commit();
    }

    private SqliteConnection CreateReadOnlyConnection()
    {
        var conn = new SqliteConnection(_connString);
        conn.Open();

        ExecuteNonQuery(conn, """
                              PRAGMA query_only = ON;
                              PRAGMA mmap_size = 268435456;
                              PRAGMA cache_size = -64000;
                              PRAGMA temp_store = MEMORY;
                              PRAGMA busy_timeout = 2000;
                              """);

        return conn;
    }

    private static void ExecuteNonQuery(SqliteConnection conn, string sql, SqliteTransaction? tran = null)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        if (tran != null) cmd.Transaction = tran;
        cmd.ExecuteNonQuery();
    }

    public record GetAutoGameByIdxResult(bool HasSucceeded, AutoGame? Game, string FailureReason);

    /// <summary>
    /// Default game presets shipped with the release.
    /// Add or modify presets here.
    /// </summary>
    private static List<AutoGame> GetHardcodedDefaultPresets()
    {
        return
        [
            new AutoGame
            {
                Name = "Spotify",
                PathRule = @"[/\\]spotify(\.exe)?$"
            }
        ];
    }
}