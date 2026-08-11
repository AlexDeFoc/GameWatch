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
public partial class GameLibrary
{
    private readonly string _connString;
    private readonly ConcurrentList<GameId> _manualGamesTableIds = [];
    private readonly ConcurrentList<GameId> _autoGamesTableIds = [];

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
        using var tran = conn.BeginTransaction();

        const string oneTimePragmas = """
                                      PRAGMA journal_mode = WAL;
                                      PRAGMA user_version = 1;
                                      PRAGMA encoding = UTF-8;
                                      """;
        conn.Execute(oneTimePragmas, transaction: tran);

        const string createTablesSql = """
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
                                       """;

        conn.Execute(createTablesSql, transaction: tran);
        tran.Commit();

        const string readManualGameTableIdsSql = """
                                                 SELECT Id
                                                 FROM ManualGames
                                                 ORDER BY Id ASC;
                                                 """;

        const string readAutoGameTableIdsSql = """
                                               SELECT Id
                                               FROM AutoGames
                                               ORDER BY Id ASC;
                                               """;

        var manualIds = conn.Query<int>(readManualGameTableIdsSql, transaction: tran)
                            .Select(v => new GameId(v));

        var autoIds = conn.Query<int>(readAutoGameTableIdsSql, transaction: tran)
                          .Select(v => new GameId(v));

        _manualGamesTableIds.ReplaceAll(manualIds);
        _autoGamesTableIds.ReplaceAll(autoIds);

        tran.Commit();
    }

    public void AddGame(GameRecords.ManualGame gameRecord)
    {
        using var conn = CreateConnection();
        using var tran = conn.BeginTransaction();

        const string insertionSql = """
                                    INSERT INTO ManualGames(Name, PlayTimeSec)
                                    VALUES (@Name, @PlayTimeSec);
                                    SELECT last_insert_rowid();
                                    """;

        var gameIdValue = conn.ExecuteScalar<int>(insertionSql, new Dto.ManualGame(gameRecord), transaction: tran);
        tran.Commit();

        _manualGamesTableIds.Add(new GameId(gameIdValue));
    }

    public void AddGame(GameRecords.AutoGame gameRecord)
    {
        using var conn = CreateConnection();
        using var tran = conn.BeginTransaction();

        const string insertionSql = """
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

        var gameIdValue = conn.ExecuteScalar<int>(insertionSql, new Dto.AutoGame(gameRecord), transaction: tran);
        tran.Commit();

        _autoGamesTableIds.Add(new GameId(gameIdValue));
    }

    public List<GameRecords.ManualGame> GetManualGames()
    {
        using var conn = CreateConnection(queryOnly: true);
        using var tran = conn.BeginTransaction();

        const string querySql = """
                                SELECT
                                    Id,
                                    Name,
                                    PlayTimeSec
                                FROM ManualGames
                                ORDER BY Id ASC;
                                """;

        try
        {
            var games = conn.Query<Dto.ManualGame>(querySql, transaction: tran)
                            .Select(dto => new GameRecords.ManualGame(dto))
                            .ToList();
            tran.Commit();

            List<long> gameIdsToReset = [];

            foreach (var game in games.Where(game => game.PlayTimeSec.V < 0L))
            {
                gameIdsToReset.Add(game.Id.V);
                game.PlayTimeSec = ElapsedTime.Zero;
            }

            if (gameIdsToReset.Count == 0) return games;

            const string resetSql = "UPDATE ManualGames SET PlayTimeSec = 0 WHERE Id IN (@Ids);";
            conn.Execute(resetSql, new { Ids = gameIdsToReset }, transaction: tran);
            tran.Commit();

            return games;
        }
        catch (Exception ex) when (ex is OverflowException || ex.InnerException is OverflowException)
        {
            const string resetSql = "UPDATE ManualGames SET PlayTimeSec = 0 WHERE PlayTimeSec > 9223372036854775807;";
            conn.Execute(resetSql, transaction: tran);

            var games = conn.Query<Dto.ManualGame>(querySql, transaction: tran)
                            .Select(dto => new GameRecords.ManualGame(dto))
                            .ToList();

            tran.Commit();

            return games;
        }
    }

    public List<GameRecords.AutoGame> GetAutoGames()
    {
        using var conn = CreateConnection(queryOnly: true);
        using var tran = conn.BeginTransaction();

        const string querySql = """
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
            var games = conn.Query<Dto.AutoGame>(querySql, transaction: tran)
                            .Select(dto => new GameRecords.AutoGame(dto))
                            .ToList();
            tran.Commit();

            List<long> gameIdsToReset = [];

            foreach (var game in games.Where(game => game.PlayTimeSec.V < 0L))
            {
                gameIdsToReset.Add(game.Id.V);
                game.PlayTimeSec = ElapsedTime.Zero;
            }

            if (gameIdsToReset.Count == 0) return games;

            const string resetSql = "UPDATE AutoGames SET PlayTimeSec = 0 WHERE Id IN (@Ids);";
            conn.Execute(resetSql, new { Ids = gameIdsToReset }, transaction: tran);
            tran.Commit();

            return games;
        }
        catch (Exception ex) when (ex is OverflowException || ex.InnerException is OverflowException)
        {
            const string resetSql = "UPDATE AutoGames SET PlayTimeSec = 0 WHERE PlayTimeSec > 9223372036854775807;";
            conn.Execute(resetSql, transaction: tran);

            var games = conn.Query<Dto.AutoGame>(querySql, transaction: tran)
                            .Select(dto => new GameRecords.AutoGame(dto))
                            .ToList();

            tran.Commit();

            return games;
        }
    }

    public DeleteGameResult DeleteGame(GameMode gameMode, GameIdx idx)
    {
        var gameIds = GetGameIdsList(gameMode);

        if (!Utils.IsIdxWithinBounds(idx, gameIds))
            return new DeleteGameResult(FailureReason: "[FAIL] Cannot find game with provided id. Ignoring command...");

        using var conn = CreateConnection();
        using var tran = conn.BeginTransaction();

        var tableName = Utils.GetTableName(gameMode);
        var gameId = gameIds[idx];

        try
        {
            var querySql = $"""
                            SELECT Name
                            FROM {tableName} 
                            WHERE Id = @Id;
                            """;

            var param = new IdParam(gameId);
            var gameTitle = conn.QueryFirstOrDefault<string?>(querySql, param, transaction: tran);
            tran.Commit();

            if (gameTitle == null)
                return new DeleteGameResult(FailureReason: "[FAIL] Cannot find game with provided id. Ignoring command...");

            var deleteSql = $"""
                             DELETE
                             FROM {tableName}
                             WHERE Id = @Id;
                             """;
            conn.Execute(deleteSql, param, transaction: tran);

            tran.Commit();

            gameIds.RemoveAt(idx);

            return new DeleteGameResult(
                HasSucceeded: true,
                DeletedGameId: gameId,
                DeletedGameTitle: gameTitle);
        }
        catch (Exception ex)
        {
            return new DeleteGameResult(FailureReason: $"[FAIL] Database error msg: {ex.Message}. Ignoring command...");
        }
    }

    public DeleteAllGamesResult DeleteAllGames(GameMode gameMode)
    {
        using var conn = CreateConnection();
        using var tran = conn.BeginTransaction();

        var tableName = Utils.GetTableName(gameMode);

        try
        {
            var deleteSql = $"DELETE FROM {tableName};";
            var rowsAffected = conn.Execute(deleteSql, transaction: tran);
            tran.Commit();

            if (rowsAffected == 0)
                return new DeleteAllGamesResult(FailureReason: "[INFO] No games found which to delete. Ignoring command...");


            GetGameIdsList(gameMode).Clear();

            return new DeleteAllGamesResult(HasSucceeded: true);
        }
        catch (Exception ex)
        {
            return new DeleteAllGamesResult(FailureReason: $"[FAIL] Database error msg: {ex.Message}. Ignoring command...");
        }
    }

    public void IncrementPlayTime(GameMode gameMode, Dictionary<GameId, ElapsedTime> gamesToUpdate)
    {
        if (gamesToUpdate.Count == 0)
            return;

        var tableName = Utils.GetTableName(gameMode);

        using var conn = CreateConnection();
        using var tran = conn.BeginTransaction();

        var updateSql = $"""
                         UPDATE {tableName}
                         SET PlayTimeSec = PlayTimeSec + @SecondsToAdd
                         WHERE Id = @Id
                         """;

        var parameters = gamesToUpdate.Select(kvp => new IncrementPlayTimeParams
        {
            SecondsToAdd = kvp.Value.V,
            Id = kvp.Key.V
        });

        conn.Execute(updateSql, parameters, transaction: tran);
        tran.Commit();
    }

    public void IncrementPlayTime(GameMode gameMode, GameId gameId, long secondsToAdd)
    {
        var tableName = Utils.GetTableName(gameMode);

        using var conn = CreateConnection();
        using var tran = conn.BeginTransaction();

        var updateSql = $"""
                         UPDATE {tableName}
                         SET PlayTimeSec = PlayTimeSec + @SecondsToAdd
                         WHERE Id = @Id
                         """;

        conn.Execute(updateSql, new IncrementPlayTimeParams { SecondsToAdd = secondsToAdd, Id = gameId.V }, transaction: tran);
        tran.Commit();
    }

    public ResetGamePlayTimeResult ResetGamePlayTime(GameMode gameMode, GameIdx gameIdx)
    {
        using var conn = CreateConnection();
        using var tran = conn.BeginTransaction();

        var tableName = Utils.GetTableName(gameMode);

        var resetSql = $"""
                        UPDATE {tableName}
                        SET PlayTimeSec = 0
                        WHERE Id = @Id;
                        """;

        GameId gameId;
        string? gameName;
        if (gameMode is GameMode.Manual)
        {
            var gameQueryResult = GetManualGameByIdx(gameIdx);

            if (!gameQueryResult.HasSucceeded || gameQueryResult.Game is null)
                return new ResetGamePlayTimeResult(FailureReason: gameQueryResult.FailureReason);

            gameId = gameQueryResult.Game.Id;
            gameName = gameQueryResult.Game.Name;
        }
        else
        {
            var gameQueryResult = GetAutoGameByIdx(gameIdx);

            if (!gameQueryResult.HasSucceeded || gameQueryResult.Game is null)
                return new ResetGamePlayTimeResult(FailureReason: gameQueryResult.FailureReason);

            gameId = gameQueryResult.Game.Id;
            gameName = gameQueryResult.Game.Name;
        }

        conn.Execute(resetSql, new IdParam(gameId), transaction: tran);
        tran.Commit();

        return new ResetGamePlayTimeResult(HasSucceeded: true, GameName: gameName);
    }

    public ChangeGamePropertyResult ChangeGameProperty(GameMode gameMode, GameId? gameId = null, string? gameName = null, ElapsedTime? playTimeSec = null, string? windowTitle = null, string? filePath = null, string? windowRule = null, string? pathRule = null)
    {
        if (gameId == null)
            return new ChangeGamePropertyResult(FailureReason: "[FAIL] Cannot find game with provided id. Ignoring command...");

        var setClauses = new List<string>();
        var dbParams = new ChangeGamePropertyParams
        {
            GameId = gameId.Value.V
        };

        if (gameName != null)
        {
            setClauses.Add("Name = @Name");
            dbParams.Name = gameName;
        }

        if (playTimeSec != null)
        {
            setClauses.Add("PlayTimeSec = @PlayTimeSec");
            dbParams.PlayTimeSec = playTimeSec.Value.V;
        }

        if (gameMode == GameMode.Auto)
        {
            setClauses.Add("WindowTitle = @WindowTitle");
            setClauses.Add("FilePath = @FilePath");
            setClauses.Add("WindowRule = @WindowRule");
            setClauses.Add("PathRule = @PathRule");

            dbParams.WindowTitle = windowTitle;
            dbParams.FilePath = filePath;
            dbParams.WindowRule = windowRule;
            dbParams.PathRule = pathRule;
        }

        if (setClauses.Count == 0)
            return new ChangeGamePropertyResult(FailureReason: "[INFO] Nothing requested to update. Ignoring command...");

        var tableName = Utils.GetTableName(gameMode);

        var updateSql = $"""
                         UPDATE {tableName}
                         SET {string.Join(", ", setClauses)}
                         WHERE Id = @GameId;
                         """;

        using var conn = CreateConnection();
        using var tran = conn.BeginTransaction();

        try
        {
            conn.Execute(updateSql, dbParams, transaction: tran);
            tran.Commit();
            return new ChangeGamePropertyResult(HasSucceeded: true);
        }
        catch (Exception ex)
        {
            return new ChangeGamePropertyResult(FailureReason: $"[FAIL] Database error: {ex.Message}. Ignoring command...");
        }
    }

    public GetManualGameByIdxResult GetManualGameByIdx(GameIdx idx)
    {
        var id = GetGameIdByIdx(GameMode.Manual, idx);

        if (id is null)
            return new GetManualGameByIdxResult(FailureReason: "[FAIL] Cannot find game with provided id. Ignoring command...");

        using var conn = CreateConnection();
        using var tran = conn.BeginTransaction();
        const string querySql = """
                                SELECT
                                    Id,
                                    Name,
                                    PlayTimeSec
                                FROM ManualGames
                                WHERE Id = @Id;
                                """;

        var queryParams = new GetManualGameByIdxQueryParams
        {
            Id = id.Value.V
        };

        try
        {
            var dto = conn.QueryFirstOrDefault<Dto.ManualGame>(querySql, queryParams, transaction: tran);
            tran.Commit();

            return dto is null
                ? new GetManualGameByIdxResult(FailureReason: "[FAIL] Cannot find game in the database. Ignoring command...")
                : new GetManualGameByIdxResult(HasSucceeded: true, Game: new GameRecords.ManualGame(dto));
        }
        catch (Exception ex) when (ex is OverflowException || ex.InnerException is OverflowException)
        {
            const string resetSql = "UPDATE ManualGames SET PlayTimeSec = 0 WHERE Id = @Id;";
            conn.Execute(resetSql, queryParams, transaction: tran);
            var dto = conn.QueryFirstOrDefault<Dto.ManualGame>(querySql, queryParams, transaction: tran);
            tran.Commit();

            return dto is null
                ? new GetManualGameByIdxResult(FailureReason: "[FAIL] Cannot find game in the database. Ignoring command...")
                : new GetManualGameByIdxResult(HasSucceeded: true, Game: new GameRecords.ManualGame(dto));
        }
        catch (Exception ex)
        {
            return new GetManualGameByIdxResult(FailureReason: $"[FAIL] Database error msg: {ex.Message}. Ignoring command...");
        }
    }

    public GetAutoGameByIdxResult GetAutoGameByIdx(GameIdx idx)
    {
        var id = GetGameIdByIdx(GameMode.Auto, idx);

        if (id is null)
            return new GetAutoGameByIdxResult(FailureReason: "[FAIL] Cannot find game with provided id. Ignoring command...");

        using var conn = CreateConnection();
        using var tran = conn.BeginTransaction();
        const string querySql = """
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

        var queryParams = new GetAutoGameByIdxQueryParams
        {
            Id = id.Value.V
        };

        try
        {
            var dto = conn.QueryFirstOrDefault<Dto.AutoGame>(querySql, queryParams, transaction: tran);
            tran.Commit();

            return dto is null
                ? new GetAutoGameByIdxResult(FailureReason: "[FAIL] Cannot find game in the database. Ignoring command...")
                : new GetAutoGameByIdxResult(HasSucceeded: true, Game: new GameRecords.AutoGame(dto));
        }
        catch (Exception ex) when (ex is OverflowException || ex.InnerException is OverflowException)
        {
            const string resetSql = "UPDATE AutoGames SET PlayTimeSec = 0 WHERE Id = @Id;";
            conn.Execute(resetSql, queryParams, transaction: tran);
            var dto = conn.QueryFirstOrDefault<Dto.AutoGame>(querySql, queryParams, transaction: tran);
            tran.Commit();

            return dto is null
                ? new GetAutoGameByIdxResult(FailureReason: "[FAIL] Cannot find game in the database. Ignoring command...")
                : new GetAutoGameByIdxResult(HasSucceeded: true, Game: new GameRecords.AutoGame(dto));
        }
        catch (Exception ex)
        {
            return new GetAutoGameByIdxResult(FailureReason: $"[FAIL] Database error msg: {ex.Message}. Ignoring command...");
        }
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

    // Helpers
    public GameId? GetGameIdByIdx(GameMode mode, GameIdx idx)
    {
        var gameIds = GetGameIdsList(mode);
        if (!Utils.IsIdxWithinBounds(idx, gameIds)) return null;
        return gameIds[idx.V];
    }

    private ConcurrentList<GameId> GetGameIdsList(GameMode m) => m == GameMode.Manual ? _manualGamesTableIds : _autoGamesTableIds;

    // Result records
    public record DeleteGameResult(bool HasSucceeded = false, GameId? DeletedGameId = null, string? DeletedGameTitle = null, string? FailureReason = null);

    public record DeleteAllGamesResult(bool HasSucceeded = false, string? FailureReason = null);

    public record ChangeGamePropertyResult(bool HasSucceeded = false, string? FailureReason = null);

    public record GetManualGameByIdxResult(bool HasSucceeded = false, GameRecords.ManualGame? Game = null, string? FailureReason = null);

    public record GetAutoGameByIdxResult(bool HasSucceeded = false, GameRecords.AutoGame? Game = null, string? FailureReason = null);

    public record ResetGamePlayTimeResult(bool HasSucceeded = false, string? GameName = null, string? FailureReason = null);

    // Params
    public sealed class GetManualGameByIdxQueryParams
    {
        public long Id { get; init; }
    }

    public sealed class GetAutoGameByIdxQueryParams
    {
        public long Id { get; init; }
    }

    public sealed class IncrementPlayTimeParams
    {
        public long Id { get; init; }
        public long SecondsToAdd { get; init; }
    }

    public sealed class ChangeGamePropertyParams
    {
        public long GameId { get; init; }
        public string? Name { get; set; }
        public long? PlayTimeSec { get; set; }
        public string? WindowTitle { get; set; }
        public string? FilePath { get; set; }
        public string? WindowRule { get; set; }
        public string? PathRule { get; set; }
    }
}