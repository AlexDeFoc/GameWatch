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

[DapperAot]
public static partial class DbFactory
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
            // Init variables
            _dbFolderPath = PathResolver.ResolveRelativePath(relativePathToUserDataFolder);
            _dbPath = Path.Join(_dbFolderPath, "GameLibrary.db");

            _connString = $"Data Source={_dbPath}";

            // Ensure directory exists before attempting to open the database file
            if (!string.IsNullOrEmpty(_dbFolderPath) && !Directory.Exists(_dbFolderPath))
                Directory.CreateDirectory(_dbFolderPath);

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

            _manualGamesTableIds = conn.Query<int>(readManualGameTableIdsSql, transaction: tran)
                                       .Select(v => new GameId(v))
                                       .ToList();

            _autoGamesTableIds = conn.Query<int>(readAutoGameTableIdsSql, transaction: tran)
                                     .Select(v => new GameId(v))
                                     .ToList();

            tran.Commit();
        }

        public static void AddGame(ManualGame gameRecord)
        {
            using var conn = CreateConnection();
            using var tran = conn.BeginTransaction();

            const string sqlAction = """
                                     INSERT INTO ManualGames(Name, PlayTimeSec)
                                     VALUES (@Name, @PlayTimeSec);
                                     SELECT last_insert_rowid();
                                     """;

            var gameIdRaw = conn.ExecuteScalar<int>(sqlAction, new
            {
                Name = gameRecord.Name,
                PlayTimeSec = gameRecord.PlayTimeSec.V
            }, transaction: tran);

            tran.Commit();

            _manualGamesTableIds.Add(new GameId(gameIdRaw));
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

            var gameIdRaw = conn.ExecuteScalar<int>(sqlAction, new
            {
                Name = gameRecord.Name,
                PlayTimeSec = gameRecord.PlayTimeSec.V,
                WindowTitle = gameRecord.WindowTitle,
                FilePath = gameRecord.FilePath,
                WindowRule = gameRecord.WindowRule,
                PathRule = gameRecord.PathRule
            }, transaction: tran);

            tran.Commit();

            _autoGamesTableIds.Add(new GameId(gameIdRaw));
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
                return conn.Query<ManualGameDto>(sql)
                           .Select(dto => new ManualGame
                           {
                               Id = new GameId(dto.Id),
                               Name = dto.Name,
                               PlayTimeSec = new ElapsedTime(dto.PlayTimeSec)
                           })
                           .ToList();
            }
            catch (Exception ex) when (ex is OverflowException || ex.InnerException is OverflowException)
            {
                // 1. Reset any overflowing rows in the DB
                using var tran = conn.BeginTransaction();
                const string fixSql = "UPDATE ManualGames SET PlayTimeSec = 0 WHERE PlayTimeSec > 2147483647;";
                conn.Execute(fixSql, transaction: tran);
                tran.Commit();

                // 2. Retry the query
                return conn.Query<ManualGameDto>(sql)
                           .Select(dto => new ManualGame
                           {
                               Id = new GameId(dto.Id),
                               Name = dto.Name,
                               PlayTimeSec = new ElapsedTime(dto.PlayTimeSec)
                           })
                           .ToList();
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
                return conn.Query<AutoGameDto>(sql)
                           .Select(dto => new AutoGame
                           {
                               Id = new GameId(dto.Id),
                               Name = dto.Name,
                               PlayTimeSec = new ElapsedTime(dto.PlayTimeSec),
                               WindowTitle = dto.WindowTitle,
                               FilePath = dto.FilePath,
                               WindowRule = dto.WindowRule,
                               PathRule = dto.PathRule
                           })
                           .ToList();
            }
            catch (Exception ex) when (ex is OverflowException || ex.InnerException is OverflowException)
            {
                // 1. Reset any overflowing rows in the DB
                using var tran = conn.BeginTransaction();
                const string fixSql = "UPDATE AutoGames SET PlayTimeSec = 0 WHERE PlayTimeSec > 2147483647;";
                conn.Execute(fixSql, transaction: tran);
                tran.Commit();

                // 2. Retry the query
                return conn.Query<AutoGameDto>(sql)
                           .Select(dto => new AutoGame
                           {
                               Id = new GameId(dto.Id),
                               Name = dto.Name,
                               PlayTimeSec = new ElapsedTime(dto.PlayTimeSec),
                               WindowTitle = dto.WindowTitle,
                               FilePath = dto.FilePath,
                               WindowRule = dto.WindowRule,
                               PathRule = dto.PathRule
                           })
                           .ToList();
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

                var param = new { Id = gameId.V };
                var gameTitle = conn.QueryFirstOrDefault<string?>(selectSql, param, transaction: tran);

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
                conn.Execute(deleteSql, param, transaction: tran);

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
                Id = kvp.Key.V,
                SecondsToAdd = kvp.Value.V
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

            conn.Execute(sql, new { SecondsToAdd = secondsToAdd, Id = gameId.V }, transaction: tran);
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

            var gameId = targetGamesTableIds[gameIdx.V - 1];
            conn.Execute(sql, new { Id = gameId.V }, transaction: tran);
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
            var dbParams = new ChangeGamePropertyParams
            {
                GameId = gameId.Value.V
            };

            if (name != null)
            {
                setClauses.Add("Name = @Name");
                dbParams.Name = name;
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
                conn.Execute(updateSql, dbParams, transaction: tran);
                tran.Commit();
                return new ChangeGamePropertyResult(HasSucceeded: true, FailureReason: string.Empty);
            }
            catch (Exception ex)
            {
                return new ChangeGamePropertyResult(HasSucceeded: false, FailureReason: $"⛔ Database error: {ex.Message}");
            }
        }

        public static GetManualGameByIdxResult GetManualGameByIdx(GameIdx idx)
        {
            var id = GetGameIdByIdx(GameMode.Manual, idx);

            if (id == null)
                return new GetManualGameByIdxResult(HasSucceeded: false,
                                                    Game: null,
                                                    FailureReason: "⛔ Provided index is out of range. Ignoring command...");

            using var conn = CreateConnection();
            const string sql = """
                               SELECT
                                   Id,
                                   Name,
                                   PlayTimeSec
                               FROM ManualGames
                               WHERE Id = @Id;
                               """;

            try
            {
                var dto = conn.QueryFirstOrDefault<ManualGameDto>(sql, new { Id = id });

                if (dto == null)
                    return new GetManualGameByIdxResult(HasSucceeded: false,
                                                      Game: null,
                                                      FailureReason: "⛔ Failed to find game inside the database. Ignoring command...");

                return new GetManualGameByIdxResult(HasSucceeded: true,
                                                  Game: new ManualGame
                                                  {
                                                      Id = new GameId(dto.Id),
                                                      Name = dto.Name,
                                                      PlayTimeSec = new ElapsedTime(dto.PlayTimeSec)
                                                  },
                                                  FailureReason: string.Empty);
            }
            catch (Exception ex)
            {
                return new GetManualGameByIdxResult(HasSucceeded: false,
                                                  Game: null,
                                                  FailureReason: $"⛔ Database error msg: {ex.Message}. Ignoring command...");
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
                var dto = conn.QueryFirstOrDefault<AutoGameDto>(sql, new { Id = id });

                if (dto == null)
                    return new GetAutoGameByIdxResult(HasSucceeded: false,
                                                      Game: null,
                                                      FailureReason: "⛔ Failed to find game inside the database. Ignoring command...");

                return new GetAutoGameByIdxResult(HasSucceeded: true,
                                                  Game: new AutoGame
                                                  {
                                                      Id = new GameId(dto.Id),
                                                      Name = dto.Name,
                                                      PlayTimeSec = new ElapsedTime(dto.PlayTimeSec),
                                                      WindowTitle = dto.WindowTitle,
                                                      FilePath = dto.FilePath,
                                                      WindowRule = dto.WindowRule,
                                                      PathRule = dto.PathRule
                                                  },
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

        public record GetManualGameByIdxResult(bool HasSucceeded, ManualGame? Game, string FailureReason);

        public record GetAutoGameByIdxResult(bool HasSucceeded, AutoGame? Game, string FailureReason);

        public sealed class ManualGameDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public int PlayTimeSec { get; set; }
        }

        public sealed class AutoGameDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public int PlayTimeSec { get; set; }
            public string? WindowTitle { get; set; }
            public string? FilePath { get; set; }
            public string? WindowRule { get; set; }
            public string? PathRule { get; set; }
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
}