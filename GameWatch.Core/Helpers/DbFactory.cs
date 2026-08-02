using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dapper;
using GameWatch.Core.Dto;
using GameWatch.Core.GameRecords;
using Microsoft.Data.Sqlite;

// ReSharper disable RedundantAnonymousTypePropertyName

namespace GameWatch.Core.Helpers;

public static class DbFactory
{
    public static class GameLibrary
    {
        private static string _dbFolderPath = null!;
        private static string _dbPath = null!;
        private static string _connString = null!;

        private static List<GameId> _manualGamesTableIds = [];
        private static List<GameId> _autoGamesTableIds = [];

        /// <summary>Call this once at application startup.</summary>
        public static void InitializeDatabase(string relativePathToUserDataFolder)
        {
            // Configure Dapper
            // Register this once at application startup
            SqlMapper.AddTypeHandler(new DapperHelpers.GameIdTypeHandler());
            SqlMapper.AddTypeHandler(new DapperHelpers.GameIdxTypeHandler());
            SqlMapper.AddTypeHandler(new DapperHelpers.PidTypeHandler());
            SqlMapper.AddTypeHandler(new DapperHelpers.ElapsedTimeTypeHandler());

            // Init variables
            _dbFolderPath = PathResolver.ResolveRelativePath(relativePathToUserDataFolder);
            _dbPath = Path.Join(_dbFolderPath, "GameLibrary.db");
            _connString = $"Data Source={_dbPath}";

            // Ensure directory exists before attempting to open the database file
            if (!string.IsNullOrEmpty(_dbFolderPath) && !Directory.Exists(_dbFolderPath))
            {
                Directory.CreateDirectory(_dbFolderPath);
            }

            using var conn = CreateConnection();

            const string dbPragmasSql = """
                                        PRAGMA journal_mode = WAL;
                                        PRAGMA mmap_size = 134217728;
                                        """;
            conn.Execute(dbPragmasSql);

            using var tran = conn.BeginTransaction();

            const string createTableSql = """
                                          CREATE TABLE IF NOT EXISTS ManualGames (
                                              Id INTEGER PRIMARY KEY,
                                              Name TEXT NOT NULL,
                                              PlayTimeSec INTEGER NOT NULL DEFAULT 0
                                          ) STRICT;

                                          CREATE TABLE IF NOT EXISTS AutoGames (
                                              Id INTEGER PRIMARY KEY,
                                              Name TEXT NOT NULL,
                                              PlayTimeSec INTEGER NOT NULL DEFAULT 0,
                                              WindowTitle TEXT,
                                              FilePath TEXT,
                                              WindowRule TEXT,
                                              PathRule TEXT
                                          ) STRICT;
                                          """;

            conn.Execute(createTableSql, transaction: tran);

            // Continue init variables which depend on the db
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

            _manualGamesTableIds = conn.Query<int>(readManualGameTableIdsSql, transaction: tran).Select(v => new GameId(v)).ToList();
            _autoGamesTableIds = conn.Query<int>(readAutoGameTableIdsSql, transaction: tran).Select(v => new GameId(v)).ToList();

            tran.Commit();
        }

        public static GameId AddGame(ManualGame gameRecord)
        {
            using var conn = CreateConnection();
            using var tran = conn.BeginTransaction();

            const string sqlAction = """
                                     INSERT INTO ManualGames(Name, PlayTimeSec)
                                     VALUES (@Name, @PlayTimeSec);
                                     SELECT last_insert_rowid();
                                     """;

            var gameId = conn.ExecuteScalar<GameId>(sqlAction, gameRecord, transaction: tran);
            tran.Commit();

            _manualGamesTableIds.Add(gameId);

            return gameId;
        }

        public static void AddGame(AutoGame gameRecord)
        {
            using var conn = CreateConnection();
            using var tran = conn.BeginTransaction();

            const string sqlAction = """
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

            var gameId = conn.ExecuteScalar<GameId>(sqlAction, gameRecord, transaction: tran);
            tran.Commit();

            _autoGamesTableIds.Add(gameId);
        }

        public static List<ManualGame> GetManualGames()
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
                return conn.Query<ManualGame>(sql).ToList();
            }
            catch (Exception ex) when (ex is OverflowException || ex.InnerException is OverflowException)
            {
                // 1. Reset any overflowing rows in the DB
                using var tran = conn.BeginTransaction();
                const string fixSql = "UPDATE ManualGames SET PlayTimeSec = 0 WHERE PlayTimeSec > 2147483647;";
                conn.Execute(fixSql, transaction: tran);
                tran.Commit();

                // 2. Retry the query
                return conn.Query<ManualGame>(sql).ToList();
            }
        }

        public static List<AutoGame> GetAutoGames()
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
                return conn.Query<AutoGame>(sql).ToList();
            }
            catch (Exception ex) when (ex is OverflowException || ex.InnerException is OverflowException)
            {
                // 1. Reset any overflowing rows in the DB
                using var tran = conn.BeginTransaction();
                const string fixSql = "UPDATE AutoGames SET PlayTimeSec = 0 WHERE PlayTimeSec > 2147483647;";
                conn.Execute(fixSql, transaction: tran);
                tran.Commit();

                // 2. Retry the query
                return conn.Query<AutoGame>(sql).ToList();
            }
        }

        public static DeleteGameActionStatus DeleteGame(GameMode targetGameMode, GameIdx pos)
        {
            var targetGamesTableIds = targetGameMode == GameMode.Auto ? _autoGamesTableIds : _manualGamesTableIds;

            if (pos.V <= 0 || pos.V > targetGamesTableIds.Count)
            {
                return new DeleteGameActionStatus(
                    HasSucceeded: false,
                    DeletedGameId: GameId.Zero,
                    DeletedGameTitle: string.Empty,
                    FailureReason: "⛔ Provided game index is out of range. Ignoring command...");
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

                var gameTitle = conn.QueryFirstOrDefault<string?>(selectSql, new { Id = gameId }, transaction: tran);

                if (gameTitle == null)
                {
                    return new DeleteGameActionStatus(
                        HasSucceeded: false,
                        DeletedGameId: GameId.Zero,
                        DeletedGameTitle: string.Empty,
                        FailureReason: $"⛔ No game found to delete at Idx={pos.V} in Table={tableName}. Ignoring command...");
                }

                // Delete using primary key directly
                var deleteSql = $"DELETE FROM {tableName} WHERE Id = @Id;";
                conn.Execute(deleteSql, new { Id = gameId }, transaction: tran);

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
                    FailureReason: $"⛔ Database error msg: {ex.Message}. Ignoring command...");
            }
        }

        public static DeleteAllGamesActionStatus DeleteAllGames(GameMode targetGameMode)
        {
            using var conn = CreateConnection();
            using var tran = conn.BeginTransaction();

            var tableName = GetTableName(targetGameMode);

            try
            {
                var deleteSql = $"DELETE FROM {tableName};";
                var rowsAffected = conn.Execute(deleteSql, transaction: tran);

                if (rowsAffected == 0)
                    return new DeleteAllGamesActionStatus(HasSucceeded: false,
                                                          FailureReason: "ℹ️ No games found which to delete, ignoring command...");

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
                    FailureReason: $"⛔ Database error msg: {ex.Message}. Ignoring command...");
            }
        }

        public static void IncrementPlayTime(GameMode gameMode, Dictionary<GameId, ElapsedTime> gamesToUpdate)
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

            var parameters = gamesToUpdate.Select(kvp => new
                                                         {
                                                             Id = kvp.Key,
                                                             SecondsToAdd = kvp.Value
                                                         });

            conn.Execute(sql, parameters, transaction: tran);
            tran.Commit();
        }

        public static void IncrementPlayTime(GameMode gameMode, GameId gameId, int secondsToAdd = 60)
        {
            var tableName = GetTableName(gameMode);

            using var conn = CreateConnection();
            using var tran = conn.BeginTransaction();

            var sql = $"""
                       UPDATE {tableName}
                       SET PlayTimeSec = PlayTimeSec + @SecondsToAdd
                       WHERE Id = @Id
                       """;

            conn.Execute(sql, new { SecondsToAdd = secondsToAdd, Id = gameId }, transaction: tran);
            tran.Commit();
        }

        public static void ResetGamePlayTime(GameMode gameMode, GameIdx gameIdx)
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

            conn.Execute(sql, new { Id = targetGamesTableIds[gameIdx.V - 1] }, transaction: tran);
            tran.Commit();
        }

        public static ChangeGamePropertyResult ChangeGameProperty(GameMode gameMode,
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
                return new ChangeGamePropertyResult(HasSucceeded: false, FailureReason: "⛔ Provided index is out of range. Ignoring command...");

            var setClauses = new List<string>();
            var parameters = new DynamicParameters();
            parameters.Add("GameId", gameId.Value);

            if (name != null)
            {
                setClauses.Add("Name = @Name");
                parameters.Add("Name", name);
            }

            if (playTimeSec != null)
            {
                setClauses.Add("PlayTimeSec = @PlayTimeSec");
                parameters.Add("PlayTimeSec", playTimeSec.Value);
            }

            if (gameMode == GameMode.Auto)
            {
                setClauses.Add("WindowTitle = @WindowTitle");
                setClauses.Add("FilePath = @FilePath");
                setClauses.Add("WindowRule = @WindowRule");
                setClauses.Add("PathRule = @PathRule");

                parameters.Add("WindowTitle", windowTitle);
                parameters.Add("FilePath", filePath);
                parameters.Add("WindowRule", windowRule);
                parameters.Add("PathRule", pathRule);
            }

            if (setClauses.Count == 0)
                return new ChangeGamePropertyResult(HasSucceeded: false, FailureReason: "ℹ️ Nothing to update, ignoring command...");

            var tableName = GetTableName(gameMode);
            var updateSql = $"""
                             UPDATE {tableName}
                             SET {string.Join(", ", setClauses)}
                             WHERE Id = @GameId;
                             """;

            using var conn = CreateConnection();
            using var tran = conn.BeginTransaction();

            try
            {
                conn.Execute(updateSql, parameters, transaction: tran);
                tran.Commit();
                return new ChangeGamePropertyResult(HasSucceeded: true, FailureReason: string.Empty);
            }
            catch (Exception ex)
            {
                return new ChangeGamePropertyResult(HasSucceeded: false, FailureReason: $"⛔ Database error: {ex.Message}");
            }
        }

        public static GetAutoGameByIdxResult GetAutoGameByIdx(GameIdx idx)
        {
            var id = GetGameIdByIdx(GameMode.Auto, idx);

            if (id == null)
                return new GetAutoGameByIdxResult(HasSucceeded: false,
                                                  Game: null,
                                                  FailureReason: "⛔ Provided index is out of range. Ignoring command...");

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

            try
            {
                var game = conn.QueryFirstOrDefault<AutoGame>(sql, new { Id = id });

                if (game == null)
                    return new GetAutoGameByIdxResult(HasSucceeded: false,
                                                      Game: null,
                                                      FailureReason: "⛔ Failed to find game inside the database. Ignoring command...");

                return new GetAutoGameByIdxResult(HasSucceeded: true,
                                                  Game: game,
                                                  FailureReason: string.Empty);
            }
            catch (Exception ex)
            {
                return new GetAutoGameByIdxResult(HasSucceeded: false,
                                                  Game: null,
                                                  FailureReason: $"⛔ Database error msg: {ex.Message}. Ignoring command...");
            }
        }

        public static GameId? GetGameIdByIdx(GameMode mode, GameIdx pos)
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

        /// <summary>Call this every time we want to perform a db action.</summary>
        private static SqliteConnection CreateConnection()
        {
            var conn = new SqliteConnection(_connString);
            conn.Open();

            const string connPragmas = """
                                       PRAGMA synchronous = NORMAL;
                                       PRAGMA busy_timeout = 5000;
                                       PRAGMA temp_store = MEMORY;
                                       PRAGMA foreign_keys = ON;
                                       """;

            conn.Execute(connPragmas);
            return conn;
        }

        public record DeleteGameActionStatus(bool HasSucceeded, GameId DeletedGameId, string DeletedGameTitle, string FailureReason);

        public record DeleteAllGamesActionStatus(bool HasSucceeded, string FailureReason);

        public record ChangeGamePropertyResult(bool HasSucceeded, string FailureReason);

        public record GetAutoGameByIdxResult(bool HasSucceeded, AutoGame? Game, string FailureReason);
    }
}