using System;
using System.IO;
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

        /// <summary>
        /// Creates, opens, and configures a fresh SQLite connection.
        /// Caller is responsible for disposing it via 'using'.
        /// </summary>
        public static SqliteConnection CreateConnection()
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

        public static void AddGame(SqliteConnection conn, SqliteTransaction tran, ManualGameRecord gameRecord)
        {
            try
            {
                var nextIdx = GetNextPositionIdx(conn, tran, GameMode.Auto);

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

        public static void AddGame(SqliteConnection conn, SqliteTransaction tran, AutoGameRecord gameRecord)
        {
            try
            {
                var nextIdx = GetNextPositionIdx(conn, tran, GameMode.Manual);

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

        private static int GetNextPositionIdx(SqliteConnection conn, SqliteTransaction tran, GameMode gameMode)
        {
            switch (gameMode)
            {
                // Gets the next vector position index (MAX + 1, or 0 if table is empty).
                case GameMode.Manual:
                {
                    const string sql = "SELECT COALESCE(MAX(TablePositionIdx) + 1, 0) FROM ManualGames;";
                    return conn.ExecuteScalar<int>(sql, transaction: tran);
                }
                case GameMode.Auto:
                {
                    const string sql = "SELECT COALESCE(MAX(TablePositionIdx) + 1, 0) FROM AutoGames;";
                    return conn.ExecuteScalar<int>(sql, transaction: tran);
                }
                default:
                    throw new NotImplementedException();
            }
        }
    }
}