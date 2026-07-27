using System.IO;
using Dapper;
using Microsoft.Data.Sqlite;

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
                                          -- Data: Games
                                          CREATE TABLE IF NOT EXISTS Games (
                                              Id INTEGER PRIMARY KEY,
                                              PositionIdx INTEGER NOT NULL,
                                              Title TEXT NOT NULL,
                                              PlayTime INTEGER NOT NULL DEFAULT 0,
                                              WindowTitle TEXT,
                                              ProcessName TEXT,
                                              FilePath TEXT
                                          );

                                          -- Index for Fast Vector/List Sorting
                                          CREATE INDEX IF NOT EXISTS idx_games_position ON Games(PositionIdx);

                                          -- View: Manual games
                                          CREATE VIEW IF NOT EXISTS View_ManualGames AS
                                          SELECT Id, PositionIdx, Title, PlayTime
                                          FROM Games
                                          WHERE ProcessName IS NULL OR ProcessName = ''
                                          ORDER BY PositionIdx ASC;

                                          -- View: Auto games
                                          CREATE VIEW IF NOT EXISTS View_AutoGames AS
                                          SELECT Id, PositionIdx, Title, PlayTime, WindowTitle, ProcessName, FilePath
                                          FROM Games
                                          WHERE ProcessName IS NOT NULL AND ProcessName != ''
                                          ORDER BY PositionIdx ASC;
                                          """;

            // Pass the active transaction to Dapper
            conn.Execute(createTableSql, transaction: tran);

            // Commit all schema creations atomically
            tran.Commit();
        }

        /// <summary>
        /// Gets the next vector position index (MAX + 1, or 0 if table is empty).
        /// Accepts an existing transaction to be part of an atomic operation.
        /// </summary>
        public static int GetNextPositionIdx(SqliteConnection conn, SqliteTransaction? tran = null)
        {
            const string sql = "SELECT COALESCE(MAX(PositionIdx) + 1, 0) FROM Games;";
            return conn.ExecuteScalar<int>(sql, transaction: tran);
        }
    }
}