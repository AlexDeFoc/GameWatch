using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Core.Helpers;
using GameWatch.Core.Types;
using Microsoft.Data.Sqlite;

namespace GameWatch.Core.Dbs;

public sealed class GameLibrary
{
    public static GameLibrary Instance { get; private set; } = null!;

    private string _connStr = null!;
    private TableIdMap _manualGameTableIds = null!;
    private TableIdMap _autoGameTableIds = null!;

    private GameLibrary()
    {
    }

    public static async Task CreateAndInitAsync(string relPathToParent, CancellationToken cancellationToken)
    {
        var library = new GameLibrary();
        await library.InitAsync(relPathToParent, cancellationToken);

        Instance = library;
    }

    private async Task InitAsync(string relPathToParent, CancellationToken cancellationToken)
    {
        var dbParentPath = PathResolver.ResolveRelativePath(relPathToParent);
        var dbPath = Path.Join(dbParentPath, "GameLibrary.db");

        _connStr = $"Data Source={dbPath}";

        if (!string.IsNullOrEmpty(dbParentPath) && !Directory.Exists(dbParentPath))
            Directory.CreateDirectory(dbParentPath);

        await using var conn = await Utils.CreateSqlConnAsync(_connStr, cancellationToken: cancellationToken);
        await Utils.ExecuteNonQueryAsync(conn, """
                                               PRAGMA journal_mode = WAL;
                                               PRAGMA user_version = 1;
                                               PRAGMA encoding = UTF-8;
                                               """, cancellationToken: cancellationToken);

        // Await the async transaction opening
        await using (var tran = conn.BeginTransaction())
        {
            // Await the table creation non-query
            await Utils.ExecuteNonQueryAsync(conn, """
                                                   CREATE TABLE IF NOT EXISTS ManualGames (
                                                       Id INTEGER PRIMARY KEY,
                                                       Name TEXT NOT NULL,
                                                       PlayTime INTEGER NOT NULL DEFAULT 0
                                                   ) STRICT;

                                                   CREATE TABLE IF NOT EXISTS AutoGames (
                                                       Id INTEGER PRIMARY KEY,
                                                       Name TEXT NOT NULL,
                                                       PlayTime INTEGER NOT NULL DEFAULT 0,
                                                       WindowTitle TEXT DEFAULT NULL,
                                                       FilePath TEXT DEFAULT NULL,
                                                       WindowRule TEXT DEFAULT NULL,
                                                       PathRule TEXT DEFAULT NULL
                                                   ) STRICT;
                                                   """, cancellationToken, tran);

            await tran.CommitAsync(cancellationToken);
        }

        // Await table id fetches
        _manualGameTableIds = [.. await Utils.FetchTableIdsAsync(conn, GameMode.Manual, cancellationToken)];
        _autoGameTableIds = [.. await Utils.FetchTableIdsAsync(conn, GameMode.Auto, cancellationToken)];
    }

    public async Task AddGameAsync(ManualGameRecord game, CancellationToken cancellationToken)
    {
        await using var conn = await Utils.CreateSqlConnAsync(_connStr, cancellationToken: cancellationToken);
        await using var tran = conn.BeginTransaction();

        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tran;
        cmd.CommandText = """
                          INSERT INTO ManualGames(Name, PlayTime)
                          VALUES (@Name, @PlayTime);
                          SELECT last_insert_rowid();
                          """;
        cmd.Parameters.AddWithValue("@Name", game.Name);
        cmd.Parameters.AddWithValue("@PlayTime", game.PlayTime.V);
        var gameIdValue = Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));
        await tran.CommitAsync(cancellationToken);

        _manualGameTableIds.Add(new TableId(gameIdValue));
    }

    public async Task<AddAutoGameResult> AddGameAsync(AutoGameRecord game, CancellationToken cancellationToken)
    {
        await using var conn = await Utils.CreateSqlConnAsync(_connStr, cancellationToken: cancellationToken);

        try
        {
            await using var tran = conn.BeginTransaction();
            await using var cmd = conn.CreateCommand();
            cmd.Transaction = tran;
            cmd.CommandText = """
                              INSERT INTO AutoGames(
                                  Name,
                                  PlayTime,
                                  WindowTitle,
                                  FilePath,
                                  WindowRule,
                                  PathRule
                              )
                              VALUES (
                                  @Name,
                                  @PlayTime,
                                  @WindowTitle,
                                  @FilePath,
                                  @WindowRule,
                                  @PathRule);
                              SELECT last_insert_rowid();
                              """;

            cmd.Parameters.AddWithValue("@Name", game.Name);
            cmd.Parameters.AddWithValue("@PlayTime", game.PlayTime.V);
            cmd.Parameters.AddWithValue("@WindowTitle", (object?)game.WindowTitle ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FilePath", (object?)game.FilePath ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@WindowRule", (object?)game.WindowRule ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PathRule", (object?)game.PathRule ?? DBNull.Value);

            var insertionTableId = Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));
            await tran.CommitAsync(cancellationToken);

            _autoGameTableIds.Add(new TableId(insertionTableId));

            return new AddAutoGameResult
            {
                Ok = true,
                TableId = new TableId(insertionTableId)
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new AddAutoGameResult(FailureReason: $"[FAIL] Database error msg: {ex.Message}. Ignoring command...");
        }
    }

    public async Task<List<ManualGameRecord>> GetManualGamesAsync(CancellationToken cancellationToken)
    {
        await using var conn = await Utils.CreateSqlConnAsync(_connStr, cancellationToken: cancellationToken);

        try
        {
            var games = await Utils.QueryManualGamesAsync(conn, cancellationToken);
            List<int> gameTableIdsToReset = [];

            foreach (var game in games.Where(game => game.PlayTime.V < 0L))
            {
                gameTableIdsToReset.Add(game.TableId.V);
                game.PlayTime = ElapsedTime.Zero;
            }

            if (gameTableIdsToReset.Count is 0)
                return games;

            await using var tran = conn.BeginTransaction();
            await using var fixCmd = conn.CreateCommand();
            fixCmd.Transaction = tran;
            fixCmd.CommandText = "UPDATE ManualGames SET PlayTime = 0 WHERE Id = @Id;";
            var pTableId = fixCmd.Parameters.Add("@Id", SqliteType.Integer);

            foreach (var i in gameTableIdsToReset)
            {
                pTableId.Value = i;

                await fixCmd.ExecuteNonQueryAsync(cancellationToken);
            }

            await tran.CommitAsync(cancellationToken);

            return games;
        }
        catch (Exception ex) when (ex is OverflowException || ex.InnerException is OverflowException)
        {
            await using var tran = conn.BeginTransaction();
            await Utils.ExecuteNonQueryAsync(conn, "UPDATE ManualGames SET PlayTime = 0 WHERE PlayTime < 0;", cancellationToken, tran);
            await tran.CommitAsync(cancellationToken);

            return await Utils.QueryManualGamesAsync(conn, cancellationToken);
        }
    }

    public async Task<AutoGameRecord[]> GetAutoGamesAsync(CancellationToken cancellationToken)
    {
        await using var conn = await Utils.CreateSqlConnAsync(_connStr, cancellationToken: cancellationToken);

        try
        {
            var games = await Utils.QueryAutoGamesAsync(conn, cancellationToken);
            List<int> gameTableIdsToReset = [];

            foreach (var game in games.Where(game => game.PlayTime.V < 0L))
            {
                gameTableIdsToReset.Add(game.TableId.V);
                game.PlayTime = ElapsedTime.Zero;
            }

            if (gameTableIdsToReset.Count is 0)
                return games;

            await using var tran = conn.BeginTransaction();
            await using var fixCmd = conn.CreateCommand();

            fixCmd.Transaction = tran;
            fixCmd.CommandText = "UPDATE AutoGames SET PlayTime = 0 WHERE Id = @Id;";
            var pTableId = fixCmd.Parameters.Add("@Id", SqliteType.Integer);

            foreach (var i in gameTableIdsToReset)
            {
                pTableId.Value = i;

                await fixCmd.ExecuteNonQueryAsync(cancellationToken);
            }

            await tran.CommitAsync(cancellationToken);

            return games;
        }
        catch (Exception ex) when (ex is OverflowException || ex.InnerException is OverflowException)
        {
            await using var tran = conn.BeginTransaction();
            await Utils.ExecuteNonQueryAsync(conn, "UPDATE AutoGames SET PlayTime = 0 WHERE PlayTime < 0;", cancellationToken, tran);
            await tran.CommitAsync(cancellationToken);

            return await Utils.QueryAutoGamesAsync(conn, cancellationToken);
        }
    }

    public async Task<DeleteGameResult> RemoveGameAsync(GameMode gameMode, TableId tableId, CancellationToken cancellationToken)
    {
        var tableName = Utils.GetTableName(gameMode);
        var tableIds = GetTableIdMap(gameMode);

        try
        {
            var gameNameResult = await ReadGameNameAsync(gameMode, tableId, cancellationToken);

            if (!gameNameResult.Ok || gameNameResult.GameName is null)
                return new DeleteGameResult(Ok: false, FailureReason: gameNameResult.FailureReason);

            await using var conn = await Utils.CreateSqlConnAsync(_connStr, cancellationToken: cancellationToken);
            await using (var tran = conn.BeginTransaction())
            {
                await using var deleteCmd = conn.CreateCommand();

                deleteCmd.Transaction = tran;
                deleteCmd.CommandText = $"DELETE FROM {tableName} WHERE Id = @Id;";
                deleteCmd.Parameters.AddWithValue("@Id", tableId.V);
                await deleteCmd.ExecuteNonQueryAsync(cancellationToken);

                await tran.CommitAsync(cancellationToken);
            }

            tableIds.Remove(tableId);

            return new DeleteGameResult(Ok: true,
                                        GameName: gameNameResult.GameName);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new DeleteGameResult(FailureReason: $"[FAIL] Database error msg: {ex.Message}. Ignoring command...");
        }
    }

    public async Task<DeleteAllGamesResult> DeleteAllGamesAsync(GameMode gameMode, CancellationToken cancellationToken)
    {
        var tableName = Utils.GetTableName(gameMode);

        try
        {
            await using var conn = await Utils.CreateSqlConnAsync(_connStr, cancellationToken: cancellationToken);

            await using (var tran = conn.BeginTransaction())
            {
                var deleteSql = $"DELETE FROM {tableName};";
                var rowsAffected = await Utils.ExecuteNonQueryAsync(conn, deleteSql, cancellationToken, tran);

                if (rowsAffected is 0)
                    return new DeleteAllGamesResult(FailureReason: "[INFO] No games found which to delete. Ignoring command...");

                await tran.CommitAsync(cancellationToken);
            }

            GetTableIdMap(gameMode).Clear();

            return new DeleteAllGamesResult(Ok: true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new DeleteAllGamesResult(FailureReason: $"[FAIL] Database error msg: {ex.Message}. Ignoring command...");
        }
    }

    public async Task IncrementPlayTimeAsync(GameMode gameMode, Dictionary<TableId, ElapsedTime> gamesToUpdate, CancellationToken cancellationToken)
    {
        if (gamesToUpdate.Count == 0)
            return;

        var tableName = Utils.GetTableName(gameMode);

        await using var conn = await Utils.CreateSqlConnAsync(_connStr, cancellationToken: cancellationToken);
        await using var tran = conn.BeginTransaction();

        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tran;
        cmd.CommandText = $"""
                           UPDATE {tableName}
                           SET PlayTime = PlayTime + @SecondsToAdd
                           WHERE Id = @Id
                           """;

        var pId = cmd.Parameters.Add("@Id", SqliteType.Integer);
        var pSecondsToAdd = cmd.Parameters.Add("@SecondsToAdd", SqliteType.Integer);

        foreach (var (key, value) in gamesToUpdate)
        {
            pId.Value = key.V;
            pSecondsToAdd.Value = value.V;
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        await tran.CommitAsync(cancellationToken);
    }

    public async Task IncrementPlayTimeAsync(GameMode gameMode, TableId tableId, ElapsedTime secondsToAdd, CancellationToken cancellationToken)
    {
        var tableName = Utils.GetTableName(gameMode);

        await using var conn = await Utils.CreateSqlConnAsync(_connStr, cancellationToken: cancellationToken);
        await using var tran = conn.BeginTransaction();

        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tran;
        cmd.CommandText = $"""
                           UPDATE {tableName}
                           SET PlayTime = PlayTime + @SecondsToAdd
                           WHERE Id = @Id
                           """;

        cmd.Parameters.AddWithValue("@Id", tableId.V);
        cmd.Parameters.AddWithValue("@SecondsToAdd", secondsToAdd.V);
        await cmd.ExecuteNonQueryAsync(cancellationToken);

        await tran.CommitAsync(cancellationToken);
    }

    public async Task<ResetGameResult> ResetGameAsync(GameMode gameMode, TableId tableId, CancellationToken cancellationToken)
    {
        await using var conn = await Utils.CreateSqlConnAsync(_connStr, cancellationToken: cancellationToken);
        await using var tran = conn.BeginTransaction();
        await using var resetCmd = conn.CreateCommand();
        resetCmd.Transaction = tran;
        var tableName = Utils.GetTableName(gameMode);
        resetCmd.CommandText = $"UPDATE {tableName} SET PlayTime = 0 WHERE Id = @Id;";
        resetCmd.Parameters.AddWithValue("@Id", tableId.V);

        try
        {
            var queryGameNameResult = await ReadGameNameAsync(gameMode, tableId, cancellationToken);

            if (!queryGameNameResult.Ok || queryGameNameResult.GameName is null)
                return new ResetGameResult(Ok: false, FailureReason: queryGameNameResult.FailureReason);
            var gameName = queryGameNameResult.GameName;

            await resetCmd.ExecuteNonQueryAsync(cancellationToken);

            await tran.CommitAsync(cancellationToken);

            return new ResetGameResult(Ok: true,
                                       GameName: gameName);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new ResetGameResult(FailureReason: $"[FAIL] Database error msg: {ex.Message}. Ignoring command...");
        }
    }

    public async Task<EditAutoGameResult> EditGameAsync(AutoGameRecord gameRecord,
                                                        TableId tableId,
                                                        CancellationToken cancellationToken,
                                                        bool nameChanged = false,
                                                        bool playTimeChanged = false,
                                                        bool windowTitleChanged = false,
                                                        bool windowRuleChanged = false,
                                                        bool filePathChanged = false,
                                                        bool pathRuleChanged = false)
    {
        var setClauses = new List<string>();
        await using var cmd = new SqliteCommand();

        cmd.Parameters.AddWithValue("@Id", tableId.V);

        if (nameChanged)
        {
            setClauses.Add("Name = @Name");
            cmd.Parameters.AddWithValue("@Name", gameRecord.Name);
        }

        if (playTimeChanged)
        {
            setClauses.Add("PlayTime = @PlayTime");
            cmd.Parameters.AddWithValue("@PlayTime", gameRecord.PlayTime.V);
        }

        if (windowTitleChanged)
        {
            setClauses.Add("WindowTitle = @WindowTitle");
            cmd.Parameters.AddWithValue("@WindowTitle", gameRecord.WindowTitle);
        }

        if (windowRuleChanged)
        {
            setClauses.Add("WindowRule = @WindowRule");
            cmd.Parameters.AddWithValue("@WindowRule", gameRecord.WindowRule);
        }

        if (filePathChanged)
        {
            setClauses.Add("FilePath = @FilePath");
            cmd.Parameters.AddWithValue("@FilePath", gameRecord.FilePath);
        }

        if (pathRuleChanged)
        {
            setClauses.Add("PathRule = @PathRule");
            cmd.Parameters.AddWithValue("@PathRule", gameRecord.PathRule);
        }

        if (setClauses.Count is 0)
            return new EditAutoGameResult(FailureReason: "[INFO] Nothing requested to update. Ignoring command...");

        await using var conn = await Utils.CreateSqlConnAsync(_connStr, cancellationToken: cancellationToken);
        await using var tran = conn.BeginTransaction();
        cmd.Connection = conn;
        cmd.Transaction = tran;
        cmd.CommandText = $"""
                           UPDATE AutoGames
                           SET {string.Join(", ", setClauses)}
                           WHERE Id = @Id;
                           """;

        try
        {
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            await tran.CommitAsync(cancellationToken);
            return new EditAutoGameResult(Ok: true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new EditAutoGameResult(FailureReason: $"[FAIL] Database error msg: {ex.Message}. Ignoring command...");
        }
    }

    public async Task<EditAutoGameResult> EditGameAsync(ManualGameRecord gameRecord, TableId tableId, CancellationToken cancellationToken, bool nameChanged = false, bool playTimeChanged = false)
    {
        var setClauses = new List<string>();
        await using var cmd = new SqliteCommand();

        cmd.Parameters.AddWithValue("@Id", tableId.V);

        if (nameChanged)
        {
            setClauses.Add("Name = @Name");
            cmd.Parameters.AddWithValue("@Name", gameRecord.Name);
        }

        if (playTimeChanged)
        {
            setClauses.Add("PlayTime = @PlayTime");
            cmd.Parameters.AddWithValue("@PlayTime", gameRecord.PlayTime.V);
        }

        if (setClauses.Count is 0)
            return new EditAutoGameResult(FailureReason: "[INFO] Nothing requested to update. Ignoring command...");

        await using var conn = await Utils.CreateSqlConnAsync(_connStr, cancellationToken: cancellationToken);
        await using var tran = conn.BeginTransaction();
        cmd.Connection = conn;
        cmd.Transaction = tran;
        cmd.CommandText = $"""
                           UPDATE ManualGames
                           SET {string.Join(", ", setClauses)}
                           WHERE Id = @Id;
                           """;

        try
        {
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            await tran.CommitAsync(cancellationToken);
            return new EditAutoGameResult(Ok: true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new EditAutoGameResult(FailureReason: $"[FAIL] Database error msg: {ex.Message}. Ignoring command...");
        }
    }

    public GetTableIdResult GetTableId(GameMode gameMode, DisplayId displayId)
    {
        var tableIds = GetTableIdMap(gameMode);

        return tableIds.Contains(displayId)
            ? new GetTableIdResult(Ok: true, TableId: tableIds[displayId])
            : new GetTableIdResult(FailureReason: $"Cannot find game table id with provided DisplayId='{displayId.V}'");
    }

    public async Task<GetManualGameResult> GetManualGameAsync(TableId tableId, CancellationToken cancellationToken)
    {
        await using var conn = await Utils.CreateSqlConnAsync(_connStr, cancellationToken: cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
                          SELECT
                              Id,
                              Name,
                              PlayTime
                          FROM ManualGames
                          WHERE Id = @Id;
                          """;
        cmd.Parameters.AddWithValue("@Id", tableId.V);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        try
        {
            if (!await reader.ReadAsync(cancellationToken))
                return new GetManualGameResult(FailureReason: "[FAIL] Cannot find game in database. Ignoring command...");

            return new GetManualGameResult(Ok: true,
                                           Game: new ManualGameRecord(Utils.ReadManualGame(reader)));
        }
        catch (Exception ex) when (ex is OverflowException || ex.InnerException is OverflowException)
        {
            await using var tran = conn.BeginTransaction();
            await Utils.ExecuteNonQueryAsync(conn, "UPDATE ManualGames SET PlayTime = 0 WHERE PlayTime < 0;", cancellationToken, tran);
            await tran.CommitAsync(cancellationToken);

            return new GetManualGameResult(Ok: true,
                                           Game: new ManualGameRecord(Utils.ReadManualGame(reader)));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new GetManualGameResult(FailureReason: $"[FAIL] Database error msg: {ex.Message}. Ignoring command...");
        }
    }

    public async Task<GetAutoGameResult> GetAutoGameAsync(TableId tableId, CancellationToken cancellationToken)
    {
        await using var conn = await Utils.CreateSqlConnAsync(_connStr, cancellationToken: cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
                          SELECT
                              Id,
                              Name,
                              PlayTime,
                              WindowTitle,
                              FilePath,
                              WindowRule,
                              PathRule
                          FROM AutoGames
                          WHERE Id = @Id;
                          """;
        cmd.Parameters.AddWithValue("@Id", tableId.V);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        try
        {
            if (!await reader.ReadAsync(cancellationToken))
                return new GetAutoGameResult(FailureReason: "[FAIL] Cannot find game in database. Ignoring command...");

            return new GetAutoGameResult(Ok: true,
                                         Game: new AutoGameRecord(Utils.ReadAutoGame(reader)));
        }
        catch (Exception ex) when (ex is OverflowException || ex.InnerException is OverflowException)
        {
            await using var tran = conn.BeginTransaction();
            await Utils.ExecuteNonQueryAsync(conn, "UPDATE AutoGames SET PlayTime = 0 WHERE PlayTime < 0;", cancellationToken, tran);
            await tran.CommitAsync(cancellationToken);

            return new GetAutoGameResult(Ok: true,
                                         Game: new AutoGameRecord(Utils.ReadAutoGame(reader)));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new GetAutoGameResult(FailureReason: $"[FAIL] Database error msg: {ex.Message}. Ignoring command...");
        }
    }

    private async Task<ReadGameNameResult> ReadGameNameAsync(GameMode gameMode, TableId tableId, CancellationToken cancellationToken)
    {
        var tableName = Utils.GetTableName(gameMode);

        await using var conn = await Utils.CreateSqlConnAsync(_connStr, cancellationToken: cancellationToken);
        await using var selectCmd = conn.CreateCommand();

        selectCmd.CommandText = $"SELECT Name FROM {tableName} WHERE Id = @Id;";
        selectCmd.Parameters.AddWithValue("@Id", tableId.V);

        await using var reader = await selectCmd.ExecuteReaderAsync(cancellationToken);

        return !await reader.ReadAsync(cancellationToken)
            ? new ReadGameNameResult(FailureReason: "[FAIL] Cannot find game in database. Ignoring command...")
            : new ReadGameNameResult(Ok: true, GameName: reader.GetString(0));
    }

    private TableIdMap GetTableIdMap(GameMode m) => m == GameMode.Manual ? _manualGameTableIds : _autoGameTableIds;

    // Results
    public record struct DeleteAllGamesResult(bool Ok = false, string? FailureReason = null);

    public record struct EditAutoGameResult(bool Ok = false, string? FailureReason = null);

    public record struct DeleteGameResult(bool Ok = false, string? GameName = null, string? FailureReason = null);

    public record struct AddAutoGameResult(bool Ok = false, TableId? TableId = null, string? FailureReason = null);

    public record struct ResetGameResult(bool Ok = false, string? GameName = null, string? FailureReason = null);

    public record struct GetTableIdResult(bool Ok = false, TableId? TableId = null, string? FailureReason = null);

    public record struct GetAutoGameResult(bool Ok = false, AutoGameRecord? Game = null, string? FailureReason = null);

    public record struct GetManualGameResult(bool Ok = false, ManualGameRecord? Game = null, string? FailureReason = null);

    private record struct ReadGameNameResult(bool Ok = false, string? GameName = null, string? FailureReason = null);
}