// TODO: Ability to get preset by idx

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dapper;
using GameWatch.Core.SqlParams;
using GameWatch.Core.Wrappers;
using Microsoft.Data.Sqlite;

namespace GameWatch.Core.Dbs;

[DapperAot]
public partial class GamePresets
{
    public static GamePresets Instance { get; private set; } = null!;

    private readonly string _connString;

    public static void Init(string relPathToParent) => Instance = new GamePresets(relPathToParent);

    private readonly ConcurrentList<GameId> _presetIds = [];

    private GamePresets(string relPathToParent)
    {
        var dbParentPath = PathResolver.ResolveRelativePath(relPathToParent);
        var dbPath = Path.Combine(dbParentPath, "GamePresets.db");

        _connString = $"Data Source={dbPath}";

        if (!string.IsNullOrEmpty(dbParentPath) && !Directory.Exists(dbParentPath))
            Directory.CreateDirectory(dbParentPath);

        if (File.Exists(dbPath))
            return;

        using var conn = CreateConnection();
        using var tran = conn.BeginTransaction();

        const string oneTimePragmas = """
                                      PRAGMA journal_mode = WAL;
                                      PRAGMA user_version = 1;
                                      PRAGMA encoding = UTF-8;
                                      """;
        conn.Execute(oneTimePragmas, transaction: tran);

        const string createTableSql = """
                                      CREATE TABLE IF NOT EXISTS AutoGamePresets (
                                          Id INTEGER PRIMARY KEY,
                                          Name TEXT NOT NULL,
                                          PlayTimeSec INTEGER NOT NULL DEFAULT 0,
                                          WindowTitle TEXT DEFAULT NULL,
                                          FilePath TEXT DEFAULT NULL,
                                          WindowRule TEXT DEFAULT NULL,
                                          PathRule TEXT DEFAULT NULL
                                      ) STRICT;
                                      """;

        conn.Execute(createTableSql, transaction: tran);

        const string insertSql = """
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

        conn.Execute(insertSql, GetPresets(), transaction: tran);

        const string readTableIdsSql = """
                                       SELECT Id
                                       FROM AutoGamePresets
                                       ORDER BY Id ASC;
                                       """;

        var ids = conn.Query<int>(readTableIdsSql, transaction: tran)
                      .Select(v => new GameId(v));

        _presetIds.ReplaceAll(ids);

        tran.Commit();
    }

    public static List<Dto.AutoGame> GetPresets() =>
    [
        new()
        {
            Name = "Spotify",
            PathRule = @"[/\\]spotify(\.exe)?$"
        }
    ];

    public GetPresetByIdxResult GetPresetByIdx(GameIdx idx)
    {
        var id = GetPresetIdByIdx(idx);

        if (id is null)
            return new GetPresetByIdxResult(FailureReason: "[FAIL] Cannot find preset with provided id. Ignoring command...");

        using var conn = CreateConnection();

        const string sql = """
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
            var dto = conn.QueryFirstOrDefault<Dto.AutoGame>(sql, new IdParam(id.Value));

            if (dto is null)
                return new GetPresetByIdxResult(FailureReason: "[FAIL] Preset not found in database. Ignoring command...");

            return new GetPresetByIdxResult(HasSucceeded: true,
                                                        Game: new GameRecords.AutoGame(dto));
        }
        catch (Exception ex)
        {
            return new GetPresetByIdxResult(FailureReason: $"[FAIL] Database error msg: {ex.Message}. Ignoring command...");
        }
    }

    private GameId? GetPresetIdByIdx(GameIdx idx)
    {
        if (!Utils.IsIdxWithinBounds(idx, _presetIds))
            return null;

        return _presetIds[idx];
    }

    private SqliteConnection CreateConnection(bool queryOnly = false)
    {
        var conn = new SqliteConnection(_connString);
        conn.Open();

        var sb = new StringBuilder();

        sb.Append("""
                  PRAGMA synchronous = NORMAL;
                  PRAGMA busy_timeout = 5000;
                  PRAGMA temp_store = MEMORY;
                  PRAGMA foreign_keys = ON;
                  """);

        if (queryOnly)
            sb.Append("PRAGMA query_only = ON;");

        conn.Execute(sb.ToString());
        return conn;
    }

    // Results
    public record GetPresetByIdxResult(bool HasSucceeded = false, GameRecords.AutoGame? Game = null, string? FailureReason = null);
}