using System;
using System.Collections.Generic;
using System.IO;
using GameWatch.Core.Dto;
using GameWatch.Core.GameRecords;
using Microsoft.Data.Sqlite;

namespace GameWatch.Core.Dbs;

public sealed class GameLibrary
{
    private readonly string _connString;
    private readonly List<GameId> _manualGamesTableIds;
    private readonly List<GameId> _autoGamesTableIds;

    public static GameLibrary Instance { get; private set; } = null!;

    public static void Init(string relPathToParent) => Instance = new GameLibrary(relPathToParent);

    private GameLibrary(string relPathToParent)
    {
        var dbParentPath = PathResolver.ResolveRelativePath(relPathToParent);
        var dbPath = Path.Join(dbParentPath, "GameLibrary.db");

        _connString = $"Data Source={dbPath}";

        if (!string.IsNullOrEmpty(dbParentPath) && !Directory.Exists(dbParentPath))
            Directory.CreateDirectory(dbParentPath);

        using var conn = CreateConnection();
        using (var tran = conn.BeginTransaction())
        {
            ExecuteNonQuery(conn, """
                                  PRAGMA journal_mode = WAL;
                                  PRAGMA user_version = 1;
                                  PRAGMA encoding = UTF-8;
                                  """, tran);

            ExecuteNonQuery(conn, """
                                  CREATE TABLE IF NOT EXISTS ManualGames (
                                      Id INTEGER PRIMARY KEY,
                                      Name TEXT NOT NULL,
                                      PlayTimeSec INTEGER NOT NULL DEFAULT 0
                                  ) STRICT;

                                  CREATE TABLE IF NOT EXISTS AutoGames (
                                      Id INTEGER PRIMARY KEY,
                                      Name TEXT NOT NULL,
                                      PlayTimeSec INTEGER NOT NULL DEFAULT 0,
                                      WindowTitle TEXT DEFAULT NULL,
                                      FilePath TEXT DEFAULT NULL,
                                      WindowRule TEXT DEFAULT NULL,
                                      PathRule TEXT DEFAULT NULL
                                  ) STRICT;
                                  """, tran);

            tran.Commit();
        }

        _manualGamesTableIds = ReadGameIds(conn, "SELECT Id FROM ManualGames ORDER BY Id ASC;");
        _autoGamesTableIds = ReadGameIds(conn, "SELECT Id FROM AutoGames ORDER BY Id ASC;");
    }

    public void AddGame(ManualGame gameRecord)
    {
        using var conn = CreateConnection();
        using var tran = conn.BeginTransaction();

        const string sqlAction = """
                                 INSERT INTO ManualGames(Name, PlayTimeSec)
                                 VALUES (@Name, @PlayTimeSec);
                                 SELECT last_insert_rowid();
                                 """;

        using var cmd = conn.CreateCommand();
        cmd.Transaction = tran;
        cmd.CommandText = sqlAction;
        cmd.Parameters.AddWithValue("@Name", gameRecord.Name);
        cmd.Parameters.AddWithValue("@PlayTimeSec", gameRecord.PlayTimeSec.V);

        var gameIdRaw = Convert.ToInt32(cmd.ExecuteScalar());
        tran.Commit();
        _manualGamesTableIds.Add(new GameId(gameIdRaw));
    }

    public void AddGame(AutoGame gameRecord)
    {
        using var conn = CreateConnection();
        using var cmd = conn.CreateCommand();
        using var tran = conn.BeginTransaction();
        cmd.Transaction = tran;
        cmd.CommandText = """
                          INSERT INTO AutoGames(
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
                              @PathRule);
                          SELECT last_insert_rowid();
                          """;

        cmd.Parameters.AddWithValue("@Name", gameRecord.Name);
        cmd.Parameters.AddWithValue("@PlayTimeSec", gameRecord.PlayTimeSec.V);
        cmd.Parameters.AddWithValue("@WindowTitle", gameRecord.WindowTitle);
        cmd.Parameters.AddWithValue("@FilePath", gameRecord.FilePath);
        cmd.Parameters.AddWithValue("@WindowRule", gameRecord.WindowRule);
        cmd.Parameters.AddWithValue("@PathRule", gameRecord.PathRule);

        var gameIdRaw = Convert.ToInt32(cmd.ExecuteScalar());
        tran.Commit();

        _autoGamesTableIds.Add(new GameId(gameIdRaw));
    }

    public List<ManualGame> GetManualGames()
    {
        using var conn = CreateConnection();
        const string sql = """
                           SELECT
                               Id,
                               Name,
                               PlayTimeSec
                           FROM ManualGames
                           ORDER BY Id ASC;
                           """;

        try
        {
            return QueryManualGames(conn, sql);
        }
        catch (Exception ex) when (ex is OverflowException || ex.InnerException is OverflowException)
        {
            using var tran = conn.BeginTransaction();
            ExecuteNonQuery(conn, "UPDATE ManualGames SET PlayTimeSec = 0 WHERE PlayTimeSec > 2147483647;", tran);
            tran.Commit();

            return QueryManualGames(conn, sql);
        }
    }

    public List<AutoGame> GetAutoGames()
    {
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
                           FROM AutoGames
                           ORDER BY Id ASC;
                           """;

        try
        {
            return QueryAutoGames(conn, sql);
        }
        catch (Exception ex) when (ex is OverflowException || ex.InnerException is OverflowException)
        {
            using var tran = conn.BeginTransaction();
            ExecuteNonQuery(conn, "UPDATE AutoGames SET PlayTimeSec = 0 WHERE PlayTimeSec > 2147483647;", tran);
            tran.Commit();

            return QueryAutoGames(conn, sql);
        }
    }

    public DeleteGameActionStatus DeleteGame(GameMode targetGameMode, GameIdx pos)
    {
        var targetGamesTableIds = targetGameMode == GameMode.Auto ? _autoGamesTableIds : _manualGamesTableIds;

        if (pos.V <= 0 || pos.V > targetGamesTableIds.Count)
        {
            return new DeleteGameActionStatus(
                HasSucceeded: false,
                DeletedGameId: GameId.Zero,
                DeletedGameTitle: string.Empty,
                FailureReason: "[FAIL] Provided game index is out of range. Ignoring command...");
        }

        using var conn = CreateConnection();
        using var tran = conn.BeginTransaction();

        var tableName = GetTableName(targetGameMode);
        var gameId = targetGamesTableIds[pos.V - 1];

        try
        {
            var selectSql = $"""
                             SELECT Name
                             FROM {tableName}
                             WHERE Id = @Id;
                             """;

            using var selectCmd = conn.CreateCommand();
            selectCmd.Transaction = tran;
            selectCmd.CommandText = selectSql;
            selectCmd.Parameters.AddWithValue("@Id", gameId.V);

            using var reader = selectCmd.ExecuteReader();
            if (!reader.Read())
            {
                return new DeleteGameActionStatus(
                    HasSucceeded: false,
                    DeletedGameId: GameId.Zero,
                    DeletedGameTitle: string.Empty,
                    FailureReason: $"[FAIL] No game found to delete at Idx={pos.V} in Table={tableName}. Ignoring command...");
            }

            var gameTitle = reader.GetString(0);
            reader.Close();

            var deleteSql = $"DELETE FROM {tableName} WHERE Id = @Id;";
            using var deleteCmd = conn.CreateCommand();
            deleteCmd.Transaction = tran;
            deleteCmd.CommandText = deleteSql;
            deleteCmd.Parameters.AddWithValue("@Id", gameId.V);
            deleteCmd.ExecuteNonQuery();

            tran.Commit();

            targetGamesTableIds.RemoveAt(pos.V - 1);

            return new DeleteGameActionStatus(
                HasSucceeded: true,
                DeletedGameId: gameId,
                DeletedGameTitle: gameTitle,
                FailureReason: string.Empty);
        }
        catch (Exception ex)
        {
            return new DeleteGameActionStatus(
                HasSucceeded: false,
                DeletedGameId: GameId.Zero,
                DeletedGameTitle: string.Empty,
                FailureReason: $"[FAIL] Database error msg: {ex.Message}. Ignoring command...");
        }
    }

    public DeleteAllGamesActionStatus DeleteAllGames(GameMode targetGameMode)
    {
        using var conn = CreateConnection();
        using var tran = conn.BeginTransaction();

        var tableName = GetTableName(targetGameMode);

        try
        {
            var deleteSql = $"DELETE FROM {tableName};";
            var rowsAffected = ExecuteNonQuery(conn, deleteSql, tran);

            if (rowsAffected == 0)
                return new DeleteAllGamesActionStatus(HasSucceeded: false,
                                                      FailureReason: "[INFO] No games found which to delete, ignoring command...");

            tran.Commit();

            var targetGamesTableIds = targetGameMode == GameMode.Auto ? _autoGamesTableIds : _manualGamesTableIds;
            targetGamesTableIds.Clear();

            return new DeleteAllGamesActionStatus(
                HasSucceeded: true,
                FailureReason: string.Empty);
        }
        catch (Exception ex)
        {
            return new DeleteAllGamesActionStatus(
                HasSucceeded: false,
                FailureReason: $"[FAIL] Database error msg: {ex.Message}. Ignoring command...");
        }
    }

    public void IncrementPlayTime(GameMode gameMode, Dictionary<GameId, ElapsedTime> gamesToUpdate)
    {
        if (gamesToUpdate.Count == 0)
            return;

        var tableName = GetTableName(gameMode);

        using var conn = CreateConnection();
        using var tran = conn.BeginTransaction();

        var sql = $"""
                   UPDATE {tableName}
                   SET PlayTimeSec = PlayTimeSec + @SecondsToAdd
                   WHERE Id = @Id
                   """;

        using var cmd = conn.CreateCommand();
        cmd.Transaction = tran;
        cmd.CommandText = sql;

        var pId = cmd.Parameters.Add("@Id", SqliteType.Integer);
        var pSeconds = cmd.Parameters.Add("@SecondsToAdd", SqliteType.Integer);

        foreach (var (key, value) in gamesToUpdate)
        {
            pId.Value = key.V;
            pSeconds.Value = value.V;
            cmd.ExecuteNonQuery();
        }

        tran.Commit();
    }

    public void IncrementPlayTime(GameMode gameMode, GameId gameId, ElapsedTime secondsToAdd)
    {
        var tableName = GetTableName(gameMode);

        using var conn = CreateConnection();
        using var tran = conn.BeginTransaction();

        var sql = $"""
                   UPDATE {tableName}
                   SET PlayTimeSec = PlayTimeSec + @SecondsToAdd
                   WHERE Id = @Id
                   """;

        using var cmd = conn.CreateCommand();
        cmd.Transaction = tran;
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@Id", gameId.V);
        cmd.Parameters.AddWithValue("@SecondsToAdd", secondsToAdd.V);
        cmd.ExecuteNonQuery();

        tran.Commit();
    }

    public void ResetGamePlayTime(GameMode gameMode, GameIdx gameIdx)
    {
        var tableName = GetTableName(gameMode);
        var targetGamesTableIds = gameMode == GameMode.Auto ? _autoGamesTableIds : _manualGamesTableIds;

        if (gameIdx.V <= 0 || gameIdx.V > targetGamesTableIds.Count)
            return;

        using var conn = CreateConnection();
        using var tran = conn.BeginTransaction();

        var sql = $"""
                   UPDATE {tableName}
                   SET PlayTimeSec = 0
                   WHERE Id = @Id;
                   """;

        var gameId = targetGamesTableIds[gameIdx.V - 1];

        using var cmd = conn.CreateCommand();
        cmd.Transaction = tran;
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@Id", gameId.V);
        cmd.ExecuteNonQuery();

        tran.Commit();
    }

    public ChangeGamePropertyResult ChangeGameProperty(GameMode gameMode,
                                                       GameIdx gameIdx,
                                                       string? name = null,
                                                       ElapsedTime? playTimeSec = null,
                                                       string? windowTitle = null,
                                                       string? filePath = null,
                                                       string? windowRule = null,
                                                       string? pathRule = null)
    {
        var gameId = GetGameIdByIdx(gameMode, gameIdx);

        if (gameId == null)
            return new ChangeGamePropertyResult(HasSucceeded: false, FailureReason: "[FAIL] Provided index is out of range. Ignoring command...");

        var setClauses = new List<string>();
        using var cmd = new SqliteCommand();

        cmd.Parameters.AddWithValue("@GameId", gameId.Value.V);

        if (name != null)
        {
            setClauses.Add("Name = @Name");
            cmd.Parameters.AddWithValue("@Name", name);
        }

        if (playTimeSec != null)
        {
            setClauses.Add("PlayTimeSec = @PlayTimeSec");
            cmd.Parameters.AddWithValue("@PlayTimeSec", playTimeSec.Value.V);
        }

        if (gameMode == GameMode.Auto)
        {
            setClauses.Add("WindowTitle = @WindowTitle");
            setClauses.Add("FilePath = @FilePath");
            setClauses.Add("WindowRule = @WindowRule");
            setClauses.Add("PathRule = @PathRule");

            cmd.Parameters.AddWithValue("@WindowTitle", windowTitle);
            cmd.Parameters.AddWithValue("@FilePath", filePath);
            cmd.Parameters.AddWithValue("@WindowRule", windowRule);
            cmd.Parameters.AddWithValue("@PathRule", pathRule);
        }

        if (setClauses.Count == 0)
            return new ChangeGamePropertyResult(HasSucceeded: false, FailureReason: "[INFO] Nothing to update, ignoring command...");

        var tableName = GetTableName(gameMode);
        cmd.CommandText = $"""
                           UPDATE {tableName}
                           SET {string.Join(", ", setClauses)}
                           WHERE Id = @GameId;
                           """;

        using var conn = CreateConnection();
        using var tran = conn.BeginTransaction();
        cmd.Connection = conn;
        cmd.Transaction = tran;

        try
        {
            cmd.ExecuteNonQuery();
            tran.Commit();
            return new ChangeGamePropertyResult(HasSucceeded: true, FailureReason: string.Empty);
        }
        catch (Exception ex)
        {
            return new ChangeGamePropertyResult(HasSucceeded: false, FailureReason: $"[FAIL] Database error: {ex.Message}");
        }
    }

    public GetManualGameByIdxResult GetManualGameByIdx(GameIdx idx)
    {
        var id = GetGameIdByIdx(GameMode.Manual, idx);

        if (id == null)
            return new GetManualGameByIdxResult(HasSucceeded: false,
                                                Game: null,
                                                FailureReason: "[FAIL] Provided index is out of range. Ignoring command...");

        using var conn = CreateConnection();
        const string sql = """
                           SELECT
                               Id,
                               Name,
                               PlayTimeSec
                           FROM ManualGames
                           WHERE Id = @Id;
                           """;
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@Id", id.Value.V);

        try
        {
            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return new GetManualGameByIdxResult(HasSucceeded: false,
                                                    Game: null,
                                                    FailureReason: "[FAIL] Failed to find game inside the database. Ignoring command...");

            return new GetManualGameByIdxResult(HasSucceeded: true,
                                                Game: new ManualGame
                                                {
                                                    Id = new GameId(reader.GetInt32(0)),
                                                    Name = reader.GetString(1),
                                                    PlayTimeSec = new ElapsedTime(reader.GetInt64(2))
                                                },
                                                FailureReason: string.Empty);
        }
        catch (Exception ex)
        {
            return new GetManualGameByIdxResult(HasSucceeded: false,
                                                Game: null,
                                                FailureReason: $"[FAIL] Database error msg: {ex.Message}. Ignoring command...");
        }
    }

    public GetAutoGameByIdxResult GetAutoGameByIdx(GameIdx idx)
    {
        var id = GetGameIdByIdx(GameMode.Auto, idx);

        if (id == null)
            return new GetAutoGameByIdxResult(HasSucceeded: false,
                                                    Game: null,
                                                    FailureReason: "[FAIL] Provided index is out of range. Ignoring command...");

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
                           FROM AutoGames
                           WHERE Id = @Id;
                           """;

        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@Id", id.Value.V);

        try
        {
            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return new GetAutoGameByIdxResult(HasSucceeded: false,
                                                        Game: null,
                                                        FailureReason: "[FAIL] Failed to find game inside the database. Ignoring command...");

            return new GetAutoGameByIdxResult(HasSucceeded: true,
                                                    Game: ReadAutoGame(reader),
                                                    FailureReason: string.Empty);
        }
        catch (Exception ex)
        {
            return new GetAutoGameByIdxResult(HasSucceeded: false,
                                                    Game: null,
                                                    FailureReason: $"[FAIL] Database error msg: {ex.Message}. Ignoring command...");
        }
    }

    public GameId? GetGameIdByIdx(GameMode mode, GameIdx pos)
    {
        var targetGamesTableIds = mode == GameMode.Auto ? _autoGamesTableIds : _manualGamesTableIds;
        if (pos.V <= 0 || pos.V > targetGamesTableIds.Count) return null;
        return targetGamesTableIds[pos.V - 1];
    }

    private static string GetTableName(GameMode gameMode) => gameMode switch
    {
        GameMode.Manual => "ManualGames",
        GameMode.Auto => "AutoGames",
        _ => throw new ArgumentOutOfRangeException(nameof(gameMode), gameMode, "Unsupported Game mode.")
    };

    private SqliteConnection CreateConnection()
    {
        var conn = new SqliteConnection(_connString);
        conn.Open();

        ExecuteNonQuery(conn, """
                              PRAGMA synchronous = NORMAL;
                              PRAGMA busy_timeout = 5000;
                              PRAGMA temp_store = MEMORY;
                              """);

        return conn;
    }

    private static List<GameId> ReadGameIds(SqliteConnection conn, string sql)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        using var reader = cmd.ExecuteReader();
        var list = new List<GameId>();
        while (reader.Read())
        {
            list.Add(new GameId(reader.GetInt32(0)));
        }

        return list;
    }

    private static List<ManualGame> QueryManualGames(SqliteConnection conn, string sql)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        using var reader = cmd.ExecuteReader();
        var list = new List<ManualGame>();
        while (reader.Read())
        {
            list.Add(new ManualGame
            {
                Id = new GameId(reader.GetInt32(0)),
                Name = reader.GetString(1),
                PlayTimeSec = new ElapsedTime(reader.GetInt64(2))
            });
        }
        return list;
    }

    private static List<AutoGame> QueryAutoGames(SqliteConnection conn, string sql)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        using var reader = cmd.ExecuteReader();
        var list = new List<AutoGame>();
        while (reader.Read())
        {
            list.Add(ReadAutoGame(reader));
        }
        return list;
    }

    private static AutoGame ReadAutoGame(SqliteDataReader reader) => new()
    {
        Id = new GameId(reader.GetInt32(0)),
        Name = reader.GetString(1),
        PlayTimeSec = new ElapsedTime(reader.GetInt64(2)),
        WindowTitle = reader.IsDBNull(3) ? null : reader.GetString(3),
        FilePath = reader.IsDBNull(4) ? null : reader.GetString(4),
        WindowRule = reader.IsDBNull(5) ? null : reader.GetString(5),
        PathRule = reader.IsDBNull(6) ? null : reader.GetString(6)
    };

    public record GetAutoGameByIdxResult(bool HasSucceeded, AutoGame? Game, string FailureReason);

    public record DeleteGameActionStatus(bool HasSucceeded, GameId DeletedGameId, string DeletedGameTitle, string FailureReason);

    public record DeleteAllGamesActionStatus(bool HasSucceeded, string FailureReason);

    public record ChangeGamePropertyResult(bool HasSucceeded, string FailureReason);

    public record GetManualGameByIdxResult(bool HasSucceeded, ManualGame? Game, string FailureReason);

    public sealed class ManualGameDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PlayTimeSec { get; set; }
    }

    private static int ExecuteNonQuery(SqliteConnection conn, string sql, SqliteTransaction? tran = null)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        if (tran != null) cmd.Transaction = tran;
        return cmd.ExecuteNonQuery();
    }

    public sealed class ChangeGamePropertyParams
    {
        public int GameId { get; init; }
        public string? Name { get; set; }
        public int? PlayTimeSec { get; set; }
        public string? WindowTitle { get; set; }
        public string? FilePath { get; set; }
        public string? WindowRule { get; set; }
        public string? PathRule { get; set; }
    }
}