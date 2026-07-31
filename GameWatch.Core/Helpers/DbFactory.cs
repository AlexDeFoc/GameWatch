using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dapper;
using GameWatch.Core.Dto;
using GameWatch.Core.Dto.GameRecords;
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

        private static List<int> _manualGamesTableIds = [];
        private static List<int> _autoGamesTableIds = [];

        /// <summary>Call this once at application startup.</summary>
        public static void InitializeDatabase(string relativePathToUserDataFolder)
        {
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
                                              Title TEXT NOT NULL,
                                              PlayTimeSeconds INTEGER NOT NULL
                                          ) STRICT;

                                          CREATE TABLE IF NOT EXISTS AutoGames (
                                              Id INTEGER PRIMARY KEY,
                                              Title TEXT NOT NULL,
                                              PlayTimeSeconds INTEGER NOT NULL,
                                              ProcessWindowTitle TEXT,
                                              ProcessFilePath TEXT,
                                              ProcessWindowTitlePattern TEXT,
                                              ProcessFilePathPattern TEXT
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

            _manualGamesTableIds = conn.Query<int>(readManualGameTableIdsSql, transaction: tran).ToList();
            _autoGamesTableIds = conn.Query<int>(readAutoGameTableIdsSql, transaction: tran).ToList();

            tran.Commit();
        }

        public static int AddGame(ManualGame gameRecord)
        {
            using var conn = CreateConnection();
            using var tran = conn.BeginTransaction();

            const string sqlAction = """
                                     INSERT INTO ManualGames(Title, PlayTimeSeconds)
                                     VALUES (@Title, @PlayTimeSeconds);
                                     SELECT last_insert_rowid();
                                     """;

            var gameId = conn.ExecuteScalar<int>(sqlAction, gameRecord, transaction: tran);
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
                                         Title,
                                         PlayTimeSeconds,
                                         ProcessWindowTitle,
                                         ProcessFilePath,
                                         ProcessWindowTitlePattern,
                                         ProcessFilePathPattern
                                     )
                                     VALUES (
                                         @Title,
                                         @PlayTimeSeconds,
                                         @ProcessWindowTitle,
                                         @ProcessFilePath,
                                         @ProcessWindowTitlePattern,
                                         @ProcessFilePathPattern;
                                     SELECT last_insert_rowid();
                                     """;

            var gameId = conn.ExecuteScalar<int>(sqlAction, gameRecord, transaction: tran);
            tran.Commit();

            _autoGamesTableIds.Add(gameId);
        }

        public static List<ManualGame> GetManualGames()
        {
            using var conn = CreateConnection();
            const string sql = """
                               SELECT
                                   ROW_NUMBER() OVER (ORDER BY Id ASC) AS Idx,
                                   Title,
                                   PlayTimeSeconds
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
                const string fixSql = "UPDATE ManualGames SET PlayTimeSeconds = 0 WHERE PlayTimeSeconds > 2147483647;";
                conn.Execute(fixSql, transaction: tran);
                tran.Commit();

                // 2. Retry the query
                return conn.Query<ManualGame>(sql).ToList();
            }
        }

        public static List<AutoGame> GetAutoGamesWithDetails()
        {
            using var conn = CreateConnection();
            const string sql = """
                               SELECT
                                   ROW_NUMBER() OVER (ORDER BY Id ASC) AS Idx,
                                   Title,
                                   PlayTimeSeconds,
                                   ProcessWindowTitle,
                                   ProcessFilePath,
                                   ProcessWindowTitlePattern,
                                   ProcessFilePathPattern
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
                const string fixSql = "UPDATE AutoGames SET PlayTimeSeconds = 0 WHERE PlayTimeSeconds > 2147483647;";
                conn.Execute(fixSql, transaction: tran);
                tran.Commit();

                // 2. Retry the query
                return conn.Query<AutoGame>(sql).ToList();
            }
        }

        public static List<AutoGame> GetAutoGamesWithDetailsWithIdInsteadOfPosIdx()
        {
            using var conn = CreateConnection();
            const string sql = """
                               SELECT
                                   Id AS Idx,
                                   Title,
                                   PlayTimeSeconds,
                                   ProcessWindowTitle,
                                   ProcessFilePath,
                                   ProcessWindowTitlePattern,
                                   ProcessFilePathPattern
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
                const string fixSql = "UPDATE AutoGames SET PlayTimeSeconds = 0 WHERE PlayTimeSeconds > 2147483647;";
                conn.Execute(fixSql, transaction: tran);
                tran.Commit();

                // 2. Retry the query
                return conn.Query<AutoGame>(sql).ToList();
            }
        }

        public static List<AutoGame> GetAutoGamesSimplified()
        {
            using var conn = CreateConnection();
            const string sql = """
                               SELECT
                                   ROW_NUMBER() OVER (ORDER BY Id ASC) AS Idx,
                                   Title,
                                   PlayTimeSeconds 
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
                const string fixSql = "UPDATE AutoGames SET PlayTimeSeconds = 0 WHERE PlayTimeSeconds > 2147483647;";
                conn.Execute(fixSql, transaction: tran);
                tran.Commit();

                // 2. Retry the query
                return conn.Query<AutoGame>(sql).ToList();
            }
        }

        public static DeleteGameActionStatus DeleteGame(GameMode targetGameMode, int posIdx)
        {
            var targetGamesTableIds = targetGameMode == GameMode.Auto ? _autoGamesTableIds : _manualGamesTableIds;

            if (posIdx <= 0 || posIdx > targetGamesTableIds.Count)
            {
                return new DeleteGameActionStatus(
                    HasSucceeded: false,
                    DeletedGameId: 0,
                    DeletedGameTitle: string.Empty,
                    FailureReason: $"Invalid position index {posIdx}. Position index must be 1 or greater.");
            }

            using var conn = CreateConnection();
            using var tran = conn.BeginTransaction();

            var tableName = GetTableName(targetGameMode);
            var gameId = targetGamesTableIds[posIdx - 1];

            try
            {
                var selectSql = $"""
                                 SELECT Title
                                 FROM {tableName}
                                 WHERE Id = @Id;
                                 """;

                var gameTitle = conn.QueryFirstOrDefault<string?>(selectSql, new { Id = gameId }, transaction: tran);

                if (gameTitle == null)
                {
                    return new DeleteGameActionStatus(
                        HasSucceeded: false,
                        DeletedGameId: 0,
                        DeletedGameTitle: string.Empty,
                        FailureReason: $"No record found at position index {posIdx} in {tableName}.");
                }

                // Delete using primary key directly
                var deleteSql = $"DELETE FROM {tableName} WHERE Id = @Id;";
                conn.Execute(deleteSql, new { Id = gameId }, transaction: tran);

                tran.Commit();

                targetGamesTableIds.RemoveAt(posIdx - 1);

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
                    DeletedGameId: 0,
                    DeletedGameTitle: string.Empty,
                    FailureReason: $"Database error: {ex.Message}");
            }
        }

        public static DeleteAllGamesActionStatus DeleteAllGames(GameMode targetGameMode)
        {
            // TODO: impl way of telling user that the db doesn't have anything to delete when .count is empty
            using var conn = CreateConnection();
            using var tran = conn.BeginTransaction();

            var tableName = GetTableName(targetGameMode);

            try
            {
                var deleteSql = $"DELETE FROM {tableName};";
                conn.Execute(deleteSql, transaction: tran);

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
                    FailureReason: $"Database error: {ex.Message}");
            }
        }

        public static void IncrementPlayTime(GameMode gameMode, int? gameId, int secondsToAdd = 60)
        {
            if (gameId == null)
                return;

            var tableName = GetTableName(gameMode);

            using var conn = CreateConnection();
            using var tran = conn.BeginTransaction();

            var sql = $"""
                       UPDATE {tableName}
                       SET PlayTimeSeconds = PlayTimeSeconds + @SecondsToAdd
                       WHERE Id = @Id
                       """;

            conn.Execute(sql, new { SecondsToAdd = secondsToAdd, Id = gameId }, transaction: tran);
            tran.Commit();
        }

        public static void ResetGamePlayTime(GameMode gameMode, int gameIdx)
        {
            var tableName = GetTableName(gameMode);
            var targetGamesTableIds = gameMode == GameMode.Auto ? _autoGamesTableIds : _manualGamesTableIds;

            if (gameIdx <= 0 || gameIdx > targetGamesTableIds.Count)
                return;

            using var conn = CreateConnection();
            using var tran = conn.BeginTransaction();

            var sql = $"""
                       UPDATE {tableName}
                       SET PlayTimeSeconds = 0
                       WHERE Id = @Id
                       """;

            conn.Execute(sql, new { Id = targetGamesTableIds[gameIdx - 1] }, transaction: tran);
            tran.Commit();
        }

        public static ChangeGamePropertyResult ChangeGameProperty(GameMode gameMode, int gameIdx,
                                                                  string? title = null,
                                                                  int? playTimeSeconds = null,
                                                                  string? procWindowTitle = null,
                                                                  string? procFilePath = null,
                                                                  string? windowTitlePattern = null,
                                                                  string? filePathPattern = null)
        {
            var gameId = GetGameIdByPosition(gameMode, gameIdx);

            if (gameId == null)
                return new ChangeGamePropertyResult(HasSucceeded: false, FailureReason: "Game index out of range");

            var setClauses = new List<string>();
            var parameters = new DynamicParameters();
            parameters.Add("GameId", gameId.Value);

            if (title != null)
            {
                setClauses.Add("Title = @Title");
                parameters.Add("Title", title);
            }

            if (playTimeSeconds != null)
            {
                setClauses.Add("PlayTimeSeconds = @PlayTimeSeconds");
                parameters.Add("PlayTimeSeconds", playTimeSeconds);
            }

            if (gameMode == GameMode.Auto)
            {
                setClauses.Add("ProcessWindowTitle = @ProcessWindowTitle");
                setClauses.Add("ProcessFilePath = @ProcessFilePath");
                setClauses.Add("ProcessWindowTitlePattern = @ProcessWindowTitlePattern");
                setClauses.Add("ProcessFilePathPattern = @ProcessFilePathPattern");
                parameters.Add("ProcessWindowTitle", procWindowTitle);
                parameters.Add("ProcessFilePath", procFilePath);
                parameters.Add("ProcessWindowTitlePattern", windowTitlePattern);
                parameters.Add("ProcessFilePathPattern", filePathPattern);
            }

            if (setClauses.Count == 0)
                return new ChangeGamePropertyResult(HasSucceeded: false, FailureReason: "Nothing to update");

            var tableName = GetTableName(gameMode);
            var updateSql = $"""
                             UPDATE {tableName}
                             SET {string.Join(", ", setClauses)}
                             WHERE Id = @GameId
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
                return new ChangeGamePropertyResult(HasSucceeded: false, FailureReason: $"Db error: {ex.Message}");
            }
        }

        public static int? GetGameIdByPosition(GameMode mode, int posIdx)
        {
            var targetGamesTableIds = mode == GameMode.Auto ? _autoGamesTableIds : _manualGamesTableIds;
            if (posIdx <= 0 || posIdx > targetGamesTableIds.Count) return null;
            return targetGamesTableIds[posIdx - 1];
        }

        private static string GetTableName(GameMode gameMode) => gameMode switch
        {
            GameMode.Manual => "ManualGames",
            GameMode.Auto => "AutoGames",
            _ => throw new ArgumentOutOfRangeException(nameof(gameMode), gameMode, "Unsupported game mode.")
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

        public record DeleteGameActionStatus(bool HasSucceeded, int DeletedGameId, string DeletedGameTitle, string FailureReason);

        public record DeleteAllGamesActionStatus(bool HasSucceeded, string FailureReason);

        public record ChangeGamePropertyResult(bool HasSucceeded, string FailureReason);
    }
}