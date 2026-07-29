using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                                         @ProcessFilePathPattern
                                     );
                                     """;

            conn.Execute(sqlAction, gameRecord, transaction: tran);
            tran.Commit();
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
            return conn.Query<ManualGame>(sql).ToList();
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
            return conn.Query<AutoGame>(sql).ToList();
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
            return conn.Query<AutoGame>(sql).ToList();
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
            return conn.Query<AutoGame>(sql).ToList();
        }

        public static DeleteGameActionStatus DeleteGame(GameMode targetGameMode, int posIdx)
        {
            if (posIdx <= 0)
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
            var offset = posIdx - 1; // Translate 1-based CLI input to 0-based OFFSET

            try
            {
                var selectSql = $"""
                                 SELECT Id, Title
                                 FROM {tableName}
                                 ORDER BY Id ASC
                                 Limit 1 OFFSET @Offset;
                                 """;

                var targetGame = conn.QueryFirstOrDefault<(int Id, string? Title)>(selectSql, new { Offset = offset }, transaction: tran);

                if (targetGame.Title == null)
                {
                    return new DeleteGameActionStatus(
                        HasSucceeded: false,
                        DeletedGameId: 0,
                        DeletedGameTitle: string.Empty,
                        FailureReason: $"No record found at position index {posIdx} in {tableName}.");
                }

                // Delete using primary key directly

                var deleteSql = $"DELETE FROM {tableName} WHERE Id = @Id;";
                conn.Execute(deleteSql, new { Id = targetGame.Id }, transaction: tran);
                tran.Commit();

                return new DeleteGameActionStatus(
                    HasSucceeded: true,
                    DeletedGameId: targetGame.Id,
                    DeletedGameTitle: targetGame.Title,
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
            using var conn = CreateConnection();
            using var tran = conn.BeginTransaction();

            var tableName = GetTableName(targetGameMode);

            try
            {
                var deleteSql = $"DELETE FROM {tableName};";
                conn.Execute(deleteSql, transaction: tran);
                tran.Commit();

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

        /// <summary>Increments PlayTimeSeconds by secondsToAdd for games matching the provided 1-based position indices (Idx).</summary>
        public static void IncrementPlayTime(GameMode gameMode, IEnumerable<int> posIndexes, int secondsToAdd = 60)
        {
            var ids = posIndexes.Distinct().ToList();
            if (ids.Count == 0) return;

            var tableName = GetTableName(gameMode);

            using var conn = CreateConnection();
            using var tran = conn.BeginTransaction();

            var sql = $"""
                       UPDATE {tableName}
                       SET PlayTimeSeconds = PlayTimeSeconds + @SecondsToAdd
                       WHERE Id IN @Ids
                       """;

            conn.Execute(sql, new { SecondsToAdd = secondsToAdd, Ids = ids }, transaction: tran);
            tran.Commit();
        }

        public static int? GetGameIdByPosition(GameMode mode, int posIdx)
        {
            if (posIdx <= 0) return null;

            using var conn = CreateConnection();
            var tableName = GetTableName(mode);
            var offset = posIdx - 1;

            var sql = $"""
                       SELECT Id
                       FROM {tableName}
                       ORDER BY Id ASC
                       LIMIT 1 OFFSET @Offset;
                       """;

            return conn.QueryFirstOrDefault<int?>(sql, new { Offset = offset });
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
    }
}