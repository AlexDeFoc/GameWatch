using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameWatch.Core.Wrappers;
using Microsoft.Data.Sqlite;

namespace GameWatch.Core.Dbs;

public sealed class GameLibrary
{
    private readonly string _connStr;
    private readonly List<TableId> _manualGameTableIds;
    private readonly List<TableId> _autoGameTableIds;

    public static GameLibrary Instance { get; private set; } = null!;

    public static void Init(string relPathToParent) => Instance = new GameLibrary(relPathToParent);

    private SqliteConnection CreateConnection(bool queryOnly = false) => Utils.CreateSqlConnection(_connStr, queryOnly);

    private GameLibrary(string relPathToParent)
    {
        var dbParentPath = PathResolver.ResolveRelativePath(relPathToParent);
        var dbPath = Path.Join(dbParentPath, "GameLibrary.db");

        _connStr = $"Data Source={dbPath}";

        if (!string.IsNullOrEmpty(dbParentPath) && !Directory.Exists(dbParentPath))
            Directory.CreateDirectory(dbParentPath);

        using var conn = CreateConnection();
        using (var tran = conn.BeginTransaction())
        {
            Utils.ExecuteNonQuery(conn, """
                                        PRAGMA journal_mode = WAL;
                                        PRAGMA user_version = 1;
                                        PRAGMA encoding = UTF-8;
                                        """, tran);

            Utils.ExecuteNonQuery(conn, """
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

        _manualGameTableIds = Utils.GetDbTableIds(conn, "ManualGames");
        _autoGameTableIds = Utils.GetDbTableIds(conn, "AutoGames");
    }

    public void AddGame(GameRecords.ManualGame gameRecord)
    {
        using var conn = CreateConnection();
        using var tran = conn.BeginTransaction();

        using var cmd = conn.CreateCommand();
        cmd.Transaction = tran;
        cmd.CommandText = """
                          INSERT INTO ManualGames(Name, PlayTimeSec)
                          VALUES (@Name, @PlayTimeSec);
                          SELECT last_insert_rowid();
                          """;
        cmd.Parameters.AddWithValue("@Name", gameRecord.Name);
        cmd.Parameters.AddWithValue("@PlayTimeSec", gameRecord.PlayTimeSec.V);

        var gameIdValue = Convert.ToInt32(cmd.ExecuteScalar());
        tran.Commit();

        _manualGameTableIds.Add(new TableId(gameIdValue));
    }

    public void AddGame(GameRecords.AutoGame gameRecord)
    {
        using var conn = CreateConnection();
        using var tran = conn.BeginTransaction();
        using var cmd = conn.CreateCommand();
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

        var gameIdValue = Convert.ToInt32(cmd.ExecuteScalar());
        tran.Commit();

        _autoGameTableIds.Add(new TableId(gameIdValue));
    }

    public List<GameRecords.ManualGame> GetManualGames()
    {
        using var conn = CreateConnection();

        try
        {
            var games = Utils.QueryManualGamesFromDb(conn);
            List<int> gameIdsToReset = [];

            foreach (var game in games.Where(game => game.PlayTimeSec.V < 0L))
            {
                gameIdsToReset.Add(game.Id.V);
                game.PlayTimeSec = ElapsedTime.Zero;
            }

            if (gameIdsToReset.Count is 0)
                return games;

            using var tran = conn.BeginTransaction();
            using var fixCmd = conn.CreateCommand();
            fixCmd.Transaction = tran;
            fixCmd.CommandText = "UPDATE ManualGames SET PlayTimeSec = 0 WHERE Id = @Id;";
            var pId = fixCmd.Parameters.Add("@Id", SqliteType.Integer);

            foreach (var i in gameIdsToReset)
            {
                pId.Value = i;

                fixCmd.ExecuteNonQuery();
            }

            return games;
        }
        catch (Exception ex) when (ex is OverflowException || ex.InnerException is OverflowException)
        {
            using var tran = conn.BeginTransaction();
            Utils.ExecuteNonQuery(conn, "UPDATE ManualGames SET PlayTimeSec = 0 WHERE PlayTimeSec < 0;", tran);
            tran.Commit();

            return Utils.QueryManualGamesFromDb(conn);
        }
    }

    public List<GameRecords.AutoGame> GetAutoGames()
    {
        using var conn = CreateConnection();

        try
        {
            var games = Utils.QueryAutoGamesFromDb(conn);
            List<int> gameIdsToReset = [];

            foreach (var game in games.Where(game => game.PlayTimeSec.V < 0L))
            {
                gameIdsToReset.Add(game.Id.V);
                game.PlayTimeSec = ElapsedTime.Zero;
            }

            if (gameIdsToReset.Count is 0)
                return games;

            using var tran = conn.BeginTransaction();
            using var fixCmd = conn.CreateCommand();
            fixCmd.Transaction = tran;
            fixCmd.CommandText = "UPDATE AutoGames SET PlayTimeSec = 0 WHERE Id = @Id;";
            var pId = fixCmd.Parameters.Add("@Id", SqliteType.Integer);

            foreach (var i in gameIdsToReset)
            {
                pId.Value = i;

                fixCmd.ExecuteNonQuery();
            }

            return games;
        }
        catch (Exception ex) when (ex is OverflowException || ex.InnerException is OverflowException)
        {
            using var tran = conn.BeginTransaction();
            Utils.ExecuteNonQuery(conn, "UPDATE AutoGames SET PlayTimeSec = 0 WHERE PlayTimeSec < 0;", tran);
            tran.Commit();

            return Utils.QueryAutoGamesFromDb(conn);
        }
    }

    public DeleteGameResult DeleteGame(GameMode gameMode, TableId tableId, DisplayId displayId)
    {
        using var conn = CreateConnection();
        using var tran = conn.BeginTransaction();

        var tableName = Utils.GetTableName(gameMode);
        var gameIds = GetTableIdList(gameMode);

        try
        {
            var queryGameNameResult = QueryGameName(gameMode, tableId);

            if (!queryGameNameResult.Ok || queryGameNameResult.GameName is null)
                return new DeleteGameResult(Ok: false, FailureReason: queryGameNameResult.FailureReason);

            using var deleteCmd = conn.CreateCommand();
            deleteCmd.Transaction = tran;
            deleteCmd.CommandText = $"DELETE FROM {tableName} WHERE Id = @Id;";
            deleteCmd.Parameters.AddWithValue("@Id", tableId.V);
            deleteCmd.ExecuteNonQuery();

            tran.Commit();

            gameIds.RemoveAt(displayId.V - 1);

            return new DeleteGameResult(Ok: true,
                                        GameName: queryGameNameResult.GameName);
        }
        catch (Exception ex)
        {
            return new DeleteGameResult(FailureReason: $"[FAIL] Database error msg: {ex.Message}. Ignoring command...");
        }
    }

    private List<TableId> GetTableIdList(GameMode m) => m == GameMode.Manual ? _manualGameTableIds : _autoGameTableIds;

    public DeleteAllGamesResult DeleteAllGames(GameMode gameMode)
    {
        using var conn = CreateConnection();
        using var tran = conn.BeginTransaction();

        var tableName = Utils.GetTableName(gameMode);

        try
        {
            var deleteSql = $"DELETE FROM {tableName};";
            var rowsAffected = Utils.ExecuteNonQuery(conn, deleteSql, tran);

            if (rowsAffected is 0)
                return new DeleteAllGamesResult(FailureReason: "[INFO] No games found which to delete. Ignoring command...");

            tran.Commit();

            GetTableIdList(gameMode).Clear();

            return new DeleteAllGamesResult(Ok: true);
        }
        catch (Exception ex)
        {
            return new DeleteAllGamesResult(FailureReason: $"[FAIL] Database error msg: {ex.Message}. Ignoring command...");
        }
    }

    public void IncrementPlayTime(GameMode gameMode, Dictionary<TableId, ElapsedTime> gamesToUpdate)
    {
        if (gamesToUpdate.Count == 0)
            return;

        var tableName = Utils.GetTableName(gameMode);

        using var conn = CreateConnection();
        using var tran = conn.BeginTransaction();

        using var cmd = conn.CreateCommand();
        cmd.Transaction = tran;
        cmd.CommandText = $"""
                           UPDATE {tableName}
                           SET PlayTimeSec = PlayTimeSec + @SecondsToAdd
                           WHERE Id = @Id
                           """;

        var pId = cmd.Parameters.Add("@Id", SqliteType.Integer);
        var pSecondsToAdd = cmd.Parameters.Add("@SecondsToAdd", SqliteType.Integer);

        foreach (var (key, value) in gamesToUpdate)
        {
            pId.Value = key.V;
            pSecondsToAdd.Value = value.V;
            cmd.ExecuteNonQuery();
        }

        tran.Commit();
    }

    public void IncrementPlayTime(GameMode gameMode, TableId tableId, ElapsedTime secondsToAdd)
    {
        var tableName = Utils.GetTableName(gameMode);

        using var conn = CreateConnection();
        using var tran = conn.BeginTransaction();

        using var cmd = conn.CreateCommand();
        cmd.Transaction = tran;
        cmd.CommandText = $"""
                           UPDATE {tableName}
                           SET PlayTimeSec = PlayTimeSec + @SecondsToAdd
                           WHERE Id = @Id
                           """;

        cmd.Parameters.AddWithValue("@Id", tableId.V);
        cmd.Parameters.AddWithValue("@SecondsToAdd", secondsToAdd.V);
        cmd.ExecuteNonQuery();

        tran.Commit();
    }

    public QueryGameNameResult QueryGameName(GameMode gameMode, TableId tableId)
    {
        var tableName = Utils.GetTableName(gameMode);

        using var conn = CreateConnection();
        using var selectCmd = conn.CreateCommand();

        selectCmd.CommandText = $"SELECT Name FROM {tableName} WHERE Id = @Id;";
        selectCmd.Parameters.AddWithValue("@Id", tableId.V);

        using var reader = selectCmd.ExecuteReader();

        return !reader.Read()
            ? new QueryGameNameResult(FailureReason: "[FAIL] Cannot find game in database. Ignoring command...")
            : new QueryGameNameResult(Ok: true, GameName: reader.GetString(0));
    }

    public ResetGameResult ResetGame(GameMode gameMode, TableId tableId)
    {
        using var conn = CreateConnection();
        using var tran = conn.BeginTransaction();

        try
        {
            var queryGameNameResult = QueryGameName(gameMode, tableId);

            if (!queryGameNameResult.Ok || queryGameNameResult.GameName is null)
                return new ResetGameResult(Ok: false, FailureReason: queryGameNameResult.FailureReason);
            var gameName = queryGameNameResult.GameName;

            var tableName = Utils.GetTableName(gameMode);
            using var resetCmd = conn.CreateCommand();
            resetCmd.Transaction = tran;
            resetCmd.CommandText = $"UPDATE {tableName} SET PlayTimeSec = 0 WHERE Id = @Id;";
            resetCmd.Parameters.AddWithValue("@Id", tableId.V);
            resetCmd.ExecuteNonQuery();

            tran.Commit();

            return new ResetGameResult(Ok: true,
                                       GameName: gameName);
        }
        catch (Exception ex)
        {
            return new ResetGameResult(FailureReason: $"[FAIL] Database error msg: {ex.Message}. Ignoring command...");
        }
    }

    public EditAutoGameResult EditGame(GameRecords.AutoGame game, TableId tableId, bool nameChanged = false, bool playTimeChanged = false, bool windowTitleChanged = false, bool windowRuleChanged = false, bool filePathChanged = false, bool pathRuleChanged = false)
    {
        var setClauses = new List<string>();
        using var cmd = new SqliteCommand();

        cmd.Parameters.AddWithValue("@Id", tableId.V);

        if (nameChanged)
        {
            setClauses.Add("Name = @Name");
            cmd.Parameters.AddWithValue("@Name", game.Name);
        }

        if (playTimeChanged)
        {
            setClauses.Add("PlayTimeSec = @PlayTimeSec");
            cmd.Parameters.AddWithValue("@PlayTimeSec", game.PlayTimeSec.V);
        }

        if (windowTitleChanged)
        {
            setClauses.Add("WindowTitle = @WindowTitle");
            cmd.Parameters.AddWithValue("@WindowTitle", game.WindowTitle);
        }

        if (windowRuleChanged)
        {
            setClauses.Add("WindowRule = @WindowRule");
            cmd.Parameters.AddWithValue("@WindowRule", game.WindowRule);
        }

        if (filePathChanged)
        {
            setClauses.Add("FilePath = @FilePath");
            cmd.Parameters.AddWithValue("@FilePath", game.FilePath);
        }

        if (pathRuleChanged)
        {
            setClauses.Add("PathRule = @PathRule");
            cmd.Parameters.AddWithValue("@PathRule", game.PathRule);
        }

        if (setClauses.Count is 0)
            return new EditAutoGameResult(FailureReason: "[INFO] Nothing requested to update. Ignoring command...");

        using var conn = CreateConnection();
        using var tran = conn.BeginTransaction();
        cmd.Connection = conn;
        cmd.Transaction = tran;
        cmd.CommandText = $"""
                           UPDATE AutoGames
                           SET {string.Join(", ", setClauses)}
                           WHERE Id = @Id;
                           """;

        try
        {
            cmd.ExecuteNonQuery();
            tran.Commit();
            return new EditAutoGameResult(Ok: true);
        }
        catch (Exception ex)
        {
            return new EditAutoGameResult(FailureReason: $"[FAIL] Database error msg: {ex.Message}. Ignoring command...");
        }
    }

    public EditAutoGameResult EditGame(GameRecords.ManualGame game, TableId tableId, bool nameChanged = false, bool playTimeChanged = false)
    {
        var setClauses = new List<string>();
        using var cmd = new SqliteCommand();

        cmd.Parameters.AddWithValue("@Id", tableId.V);

        if (nameChanged)
        {
            setClauses.Add("Name = @Name");
            cmd.Parameters.AddWithValue("@Name", game.Name);
        }

        if (playTimeChanged)
        {
            setClauses.Add("PlayTimeSec = @PlayTimeSec");
            cmd.Parameters.AddWithValue("@PlayTimeSec", game.PlayTimeSec.V);
        }

        if (setClauses.Count is 0)
            return new EditAutoGameResult(FailureReason: "[INFO] Nothing requested to update. Ignoring command...");

        using var conn = CreateConnection();
        using var tran = conn.BeginTransaction();
        cmd.Connection = conn;
        cmd.Transaction = tran;
        cmd.CommandText = $"""
                           UPDATE ManualGames
                           SET {string.Join(", ", setClauses)}
                           WHERE Id = @Id;
                           """;

        try
        {
            cmd.ExecuteNonQuery();
            tran.Commit();
            return new EditAutoGameResult(Ok: true);
        }
        catch (Exception ex)
        {
            return new EditAutoGameResult(FailureReason: $"[FAIL] Database error msg: {ex.Message}. Ignoring command...");
        }
    }

    public GetTableIdResult GetTableId(GameMode gameMode, DisplayId displayId)
    {
        var tableIdList = GetTableIdList(gameMode);

        return !Utils.IsDisplayIdWithinBounds(displayId, tableIdList)
            ? new GetTableIdResult(FailureReason: "[FAIL] Cannot find game with provided id. Ignoring command...")
            : new GetTableIdResult(Ok: true, TableId: tableIdList[displayId.V - 1]);
    }

    public GetDisplayIdResult GetDisplayId(GameMode gameMode, TableId tableId)
    {
        var tableIdList = GetTableIdList(gameMode);

        var displayId = tableIdList.IndexOf(tableId);

        return displayId is -1
            ? new GetDisplayIdResult(FailureReason: "[FAIL] Cannot find table id with provided display id. Ignoring command...")
            : new GetDisplayIdResult(Ok: true, DisplayId: new DisplayId(displayId));
    }

    public GetManualGameResult GetManualGame(TableId tableId, DisplayId displayId)
    {
        using var conn = CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
                          SELECT
                              Id,
                              Name,
                              PlayTimeSec
                          FROM ManualGames
                          WHERE Id = @Id;
                          """;
        cmd.Parameters.AddWithValue("@Id", tableId.V);

        using var reader = cmd.ExecuteReader();

        try
        {
            if (!reader.Read())
                return new GetManualGameResult(FailureReason: "[FAIL] Cannot find game in database. Ignoring command...");

            return new GetManualGameResult(Ok: true,
                                           Game: new GameRecords.ManualGame(Utils.QueryManualGameDtoFromDb(reader), displayId));
        }
        catch (Exception ex) when (ex is OverflowException || ex.InnerException is OverflowException)
        {
            using var tran = conn.BeginTransaction();
            Utils.ExecuteNonQuery(conn, "UPDATE ManualGames SET PlayTimeSec = 0 WHERE PlayTimeSec < 0;", tran);
            tran.Commit();

            return new GetManualGameResult(Ok: true,
                                           Game: new GameRecords.ManualGame(Utils.QueryManualGameDtoFromDb(reader), displayId));
        }
        catch (Exception ex)
        {
            return new GetManualGameResult(FailureReason: $"[FAIL] Database error msg: {ex.Message}. Ignoring command...");
        }
    }

    public GetAutoGameResult GetAutoGame(TableId tableId, DisplayId displayId)
    {
        using var conn = CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
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
        cmd.Parameters.AddWithValue("@Id", tableId.V);

        using var reader = cmd.ExecuteReader();

        try
        {
            if (!reader.Read())
                return new GetAutoGameResult(FailureReason: "[FAIL] Cannot find game in database. Ignoring command...");

            return new GetAutoGameResult(Ok: true,
                                         Game: new GameRecords.AutoGame(Utils.QueryAutoGameDtoFromDb(reader), displayId));
        }
        catch (Exception ex) when (ex is OverflowException || ex.InnerException is OverflowException)
        {
            using var tran = conn.BeginTransaction();
            Utils.ExecuteNonQuery(conn, "UPDATE AutoGames SET PlayTimeSec = 0 WHERE PlayTimeSec < 0;", tran);
            tran.Commit();

            return new GetAutoGameResult(Ok: true,
                                         Game: new GameRecords.AutoGame(Utils.QueryAutoGameDtoFromDb(reader), displayId));
        }
        catch (Exception ex)
        {
            return new GetAutoGameResult(FailureReason: $"[FAIL] Database error msg: {ex.Message}. Ignoring command...");
        }
    }

    public record struct DeleteAllGamesResult(bool Ok = false, string? FailureReason = null);

    public record struct EditAutoGameResult(bool Ok = false, string? FailureReason = null);

    public record struct DeleteGameResult(bool Ok = false, string? GameName = null, string? FailureReason = null);

    public record struct ResetGameResult(bool Ok = false, string? GameName = null, string? FailureReason = null);

    public record struct QueryGameNameResult(bool Ok = false, string? GameName = null, string? FailureReason = null);

    public record struct GetTableIdResult(bool Ok = false, TableId? TableId = null, string? FailureReason = null);

    public record struct GetDisplayIdResult(bool Ok = false, DisplayId? DisplayId = null, string? FailureReason = null);

    public record struct GetAutoGameResult(bool Ok = false, GameRecords.AutoGame? Game = null, string? FailureReason = null);

    public record struct GetManualGameResult(bool Ok = false, GameRecords.ManualGame? Game = null, string? FailureReason = null);
}