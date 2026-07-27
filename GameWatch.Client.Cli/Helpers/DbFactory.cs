using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dapper;
using GameWatch.Client.Cli.Dto;
using GameWatch.Client.Cli.Dto.GameRecords;
using Microsoft.Data.Sqlite;

// ReSharper disable RedundantAnonymousTypePropertyName

namespace GameWatch.Client.Cli.Helpers;

public static class DbFactory
{
    public static class GameLibrary
    {
        private static readonly string DbPath = PathResolver.ResolveRelativePath("../../UserData/GameLibrary.db");
        private static readonly string DbFolderPath = PathResolver.ResolveRelativePath("../../UserData/");
        private static readonly string ConnString = $"Data Source={DbPath}";

        private static int GetNextPositionIdx(SqliteConnection conn, SqliteTransaction tran, GameMode gameMode)
        {
            switch (gameMode)
            {
                case GameMode.Manual:
                {
                    // COALESCE(MAX(TablePositionIdx), -1) + 1 returns 0 when empty, or MAX + 1
                    const string sql = "SELECT COALESCE(MAX(TablePositionIdx), -1) + 1 FROM ManualGames;";
                    return conn.ExecuteScalar<int>(sql, transaction: tran);
                }
                case GameMode.Auto:
                {
                    const string sql = "SELECT COALESCE(MAX(TablePositionIdx), -1) + 1 FROM AutoGames;";
                    return conn.ExecuteScalar<int>(sql, transaction: tran);
                }
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Creates, opens, and configures a fresh SQLite connection.
        /// Caller is responsible for disposing it via 'using'.
        /// </summary>
        private static SqliteConnection CreateConnection()
        {
            var conn = new SqliteConnection(ConnString);
            conn.Open();

            const string connPragmas = """
                                       PRAGMA journal_mode = WAL;
                                       PRAGMA synchronous = NORMAL;
                                       PRAGMA busy_timeout = 5000;
                                       PRAGMA temp_store = MEMORY;
                                       PRAGMA foreign_keys = ON;
                                       PRAGMA mmap_size = 134217728;
                                       """;

            conn.Execute(connPragmas);

            return conn;
        }

        /// <summary>
        /// Ensures the database schema, indexes, and views exist.
        /// Call this once at application startup.
        /// </summary>
        public static void InitializeDatabase()
        {
            // Ensure directory exists before attempting to open the database file
            if (!string.IsNullOrEmpty(DbFolderPath) && !Directory.Exists(DbFolderPath))
            {
                Directory.CreateDirectory(DbFolderPath);
            }

            using var conn = CreateConnection();
            using var tran = conn.BeginTransaction();

            const string createTableSql = """
                                          -- Data: ManualGames
                                          CREATE TABLE IF NOT EXISTS ManualGames (
                                              TableId INTEGER PRIMARY KEY,
                                              TablePositionIdx INTEGER NOT NULL,
                                              GameRecordTitle TEXT NOT NULL,
                                              GameRecordPlayTime INTEGER NOT NULL
                                          );

                                          -- Data: AutoGames
                                          CREATE TABLE IF NOT EXISTS AutoGames (
                                              TableId INTEGER PRIMARY KEY,
                                              TablePositionIdx INTEGER NOT NULL,
                                              GameRecordTitle TEXT NOT NULL,
                                              GameRecordPlayTime INTEGER NOT NULL,
                                              ProcessWindowTitle TEXT,
                                              ProcessFilePath TEXT,
                                              WindowTitleRegexPattern TEXT,
                                              FilePathRegexPattern TEXT,
                                              ShouldMatchAgainstProcessWindowTitle INTEGER NOT NULL,
                                              ShouldMatchAgainstProcessFilePath INTEGER NOT NULL,
                                              ShouldMatchProcessWindowTitleAgainstRegexPattern INTEGER NOT NULL,
                                              ShouldMatchProcessFilePathAgainstRegexPattern INTEGER NOT NULL
                                          );
                                          """;

            // Pass the active transaction to Dapper
            conn.Execute(createTableSql, transaction: tran);

            // Commit all schema creations atomically
            tran.Commit();
        }

        public static void AddGame(ManualGameRecord gameRecord)
        {
            using var conn = CreateConnection();
            using var tran = conn.BeginTransaction();

            try
            {
                var nextIdx = GetNextPositionIdx(conn, tran, GameMode.Manual);

                const string sqlAction = """
                                         INSERT INTO ManualGames(TablePositionIdx, GameRecordTitle, GameRecordPlayTime)
                                         VALUES (@TablePositionIdx, @GameRecordTitle, @GameRecordPlayTime)
                                         """;

                conn.Execute(sqlAction, new { TablePositionIdx = nextIdx, GameRecordTitle = gameRecord.Title, GameRecordPlayTime = gameRecord.PlayTime }, transaction: tran);
                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public static void AddGame(AutoGameRecord gameRecord)
        {
            using var conn = CreateConnection();
            using var tran = conn.BeginTransaction();

            try
            {
                var nextIdx = GetNextPositionIdx(conn, tran, GameMode.Auto);

                const string sqlAction = """
                                         INSERT INTO AutoGames(
                                             TablePositionIdx,
                                             GameRecordTitle,
                                             GameRecordPlayTime,
                                             ProcessWindowTitle,
                                             ProcessFilePath,
                                             WindowTitleRegexPattern,
                                             FilePathRegexPattern,
                                             ShouldMatchAgainstProcessWindowTitle,
                                             ShouldMatchAgainstProcessFilePath,
                                             ShouldMatchProcessWindowTitleAgainstRegexPattern,
                                             ShouldMatchProcessFilePathAgainstRegexPattern
                                         )
                                         VALUES (
                                             @TablePositionIdx,
                                             @GameRecordTitle,
                                             @GameRecordPlayTime,
                                             @ProcessWindowTitle,
                                             @ProcessFilePath,
                                             @WindowTitleRegexPattern,
                                             @FilePathRegexPattern,
                                             @ShouldMatchAgainstProcessWindowTitle,
                                             @ShouldMatchAgainstProcessFilePath,
                                             @ShouldMatchProcessWindowTitleAgainstRegexPattern,
                                             @ShouldMatchProcessFilePathAgainstRegexPattern
                                         )
                                         """;

                conn.Execute(sqlAction, new
                                        {
                                            TablePositionIdx = nextIdx,
                                            GameRecordTitle = gameRecord.Title,
                                            GameRecordPlayTime = gameRecord.PlayTime,
                                            ProcessWindowTitle = gameRecord.ProcessWindowTitle,
                                            ProcessFilePath = gameRecord.ProcessFilePath,
                                            WindowTitleRegexPattern = gameRecord.WindowTitleRegexPattern,
                                            FilePathRegexPattern = gameRecord.FilePathRegexPattern,
                                            ShouldMatchAgainstProcessWindowTitle = gameRecord.ShouldMatchAgainstProcessWindowTitle,
                                            ShouldMatchAgainstProcessFilePath = gameRecord.ShouldMatchAgainstProcessFilePath,
                                            ShouldMatchProcessWindowTitleAgainstRegexPattern = gameRecord.ShouldMatchProcessWindowTitleAgainstRegexPattern,
                                            ShouldMatchProcessFilePathAgainstRegexPattern = gameRecord.ShouldMatchProcessFilePathAgainstRegexPattern
                                        }, transaction: tran);
                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public static List<ManualGameRecordForDbQuery> GetManualGames()
        {
            using var conn = CreateConnection();
            const string sql = """
                               SELECT
                                   TableId,
                                   TablePositionIdx,
                                   GameRecordTitle,
                                   GameRecordPlayTime
                               FROM ManualGames
                               ORDER BY TablePositionIdx ASC;
                               """;
            return conn.Query<ManualGameRecordForDbQuery>(sql).ToList();
        }

        public static List<AutoGameRecordWithDetailsForDbQuery> GetAutoGamesWithDetails()
        {
            using var conn = CreateConnection();
            const string sql = """
                               SELECT
                                   TableId,
                                   TablePositionIdx,
                                   GameRecordTitle,
                                   GameRecordPlayTime,
                                   ProcessWindowTitle,
                                   ProcessFilePath,
                                   WindowTitleRegexPattern,
                                   FilePathRegexPattern,
                                   ShouldMatchAgainstProcessWindowTitle,
                                   ShouldMatchAgainstProcessFilePath,
                                   ShouldMatchProcessWindowTitleAgainstRegexPattern,
                                   ShouldMatchProcessFilePathAgainstRegexPattern
                               FROM AutoGames
                               ORDER BY TablePositionIdx ASC;
                               """;
            return conn.Query<AutoGameRecordWithDetailsForDbQuery>(sql).ToList();
        }

        public static List<AutoGameRecordSimplifiedForDbQuery> GetAutoGamesSimplified()
        {
            using var conn = CreateConnection();
            const string sql = """
                               SELECT
                                   TableId,
                                   TablePositionIdx,
                                   GameRecordTitle,
                                   GameRecordPlayTime
                               FROM AutoGames
                               ORDER BY TablePositionIdx ASC;
                               """;
            return conn.Query<AutoGameRecordSimplifiedForDbQuery>(sql).ToList();
        }

        public static DeleteGameActionStatus DeleteGame(GameMode targetGameMode, int posIdx)
        {
            using var conn = CreateConnection();
            using var tran = conn.BeginTransaction();

            var tableName = targetGameMode switch
            {
                GameMode.Manual => "ManualGames",
                GameMode.Auto => "AutoGames",
                _ => throw new NotImplementedException()
            };

            try
            {
                var selectSql = $"SELECT GameRecordTitle FROM {tableName} WHERE TablePositionIdx = @PosIdx;";

                var gameTitle = conn.QueryFirstOrDefault<string>(selectSql, new { PosIdx = posIdx }, transaction: tran);

                if (string.IsNullOrEmpty(gameTitle))
                {
                    return new DeleteGameActionStatus(
                        HasSucceeded: false,
                        DeletedGameTitle: string.Empty,
                        FailureReason: $"No record found at position index {posIdx} in {tableName}.");
                }

                var deleteSql = $"DELETE FROM {tableName} WHERE TablePositionIdx = @PosIdx;";
                conn.Execute(deleteSql, new { PosIdx = posIdx }, transaction: tran);

                var shiftIndicesSql = $"""
                                       UPDATE {tableName}
                                       SET TablePositionIdx = TablePositionIdx - 1
                                       WHERE TablePositionIdx > @PosIdx;
                                       """;
                conn.Execute(shiftIndicesSql, new { PosIdx = posIdx }, transaction: tran);

                tran.Commit();

                return new DeleteGameActionStatus(
                    HasSucceeded: true,
                    DeletedGameTitle: gameTitle,
                    FailureReason: string.Empty);
            }
            catch (Exception ex)
            {
                tran.Rollback();

                return new DeleteGameActionStatus(
                    HasSucceeded: false,
                    DeletedGameTitle: string.Empty,
                    FailureReason: $"Database error: {ex.Message}");
            }
        }

        public static DeleteAllGamesActionStatus DeleteAllGames(GameMode targetGameMode)
        {
            using var conn = CreateConnection();
            using var tran = conn.BeginTransaction();

            var tableName = targetGameMode switch
            {
                GameMode.Manual => "ManualGames",
                GameMode.Auto => "AutoGames",
                _ => throw new NotImplementedException()
            };

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
                tran.Rollback();

                return new DeleteAllGamesActionStatus(
                    HasSucceeded: false,
                    FailureReason: $"Database error: {ex.Message}");
            }
        }

        public record DeleteGameActionStatus(bool HasSucceeded, string DeletedGameTitle, string FailureReason);

        public record DeleteAllGamesActionStatus(bool HasSucceeded, string FailureReason);
    }
}