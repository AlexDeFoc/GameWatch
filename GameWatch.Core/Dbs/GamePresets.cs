using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Core.Helpers;
using GameWatch.Core.Types;
using Microsoft.Data.Sqlite;

namespace GameWatch.Core.Dbs;

public sealed class GamePresets
{
    public static GamePresets Instance { get; private set; } = null!;

    private string _connStr = null!;
    private List<TableId> _presetIds = null!;

    private GamePresets()
    {
    }

    public static async Task CreateAndInitAsync(string relPathToParent, CancellationToken cancellationToken)
    {
        var library = new GamePresets();
        await library.InitAsync(relPathToParent, cancellationToken);

        Instance = library;
    }

    private async Task InitAsync(string relPathToParent, CancellationToken cancellationToken)
    {
        var dbParentPath = PathResolver.ResolveRelativePath(relPathToParent);
        var dbPath = Path.Join(dbParentPath, "GamePresets.db");

        _connStr = $"Data Source={dbPath}";

        if (!string.IsNullOrEmpty(dbParentPath) && !Directory.Exists(dbParentPath))
            Directory.CreateDirectory(dbParentPath);

        await using var conn = await Utils.CreateSqlConnAsync(_connStr, cancellationToken: cancellationToken);

        await Utils.ExecuteNonQueryAsync(conn, """
                                               PRAGMA journal_mode = WAL;
                                               PRAGMA user_version = 1;
                                               PRAGMA encoding = UTF-8;
                                               """, cancellationToken: cancellationToken);

        await using (var tran = conn.BeginTransaction())
        {
            await Utils.ExecuteNonQueryAsync(conn, """
                                                   CREATE TABLE IF NOT EXISTS AutoGamePresets (
                                                       Id INTEGER PRIMARY KEY,
                                                       Name TEXT NOT NULL,
                                                       WindowTitle TEXT DEFAULT NULL,
                                                       FilePath TEXT DEFAULT NULL,
                                                       WindowRule TEXT DEFAULT NULL,
                                                       PathRule TEXT DEFAULT NULL
                                                   ) STRICT;
                                                   """, cancellationToken, tran);

            await using var insertPreMadePresetsCmd = conn.CreateCommand();
            insertPreMadePresetsCmd.Transaction = tran;
            insertPreMadePresetsCmd.CommandText = """
                                                  INSERT INTO AutoGamePresets (
                                                      Id,
                                                      Name,
                                                      WindowTitle,
                                                      FilePath,
                                                      WindowRule,
                                                      PathRule
                                                  )
                                                  VALUES (
                                                      @Id,
                                                      @Name,
                                                      @WindowTitle,
                                                      @FilePath,
                                                      @WindowRule,
                                                      @PathRule
                                                  ) ON CONFLICT(Id) DO NOTHING;
                                                  """;

            var presets = GetPreMadePresets();

            var pTableId = insertPreMadePresetsCmd.Parameters.Add("@Id", SqliteType.Integer);
            var pName = insertPreMadePresetsCmd.Parameters.Add("@Name", SqliteType.Text);
            var pWindowTitle = insertPreMadePresetsCmd.Parameters.Add("@WindowTitle", SqliteType.Text);
            var pFilePath = insertPreMadePresetsCmd.Parameters.Add("@FilePath", SqliteType.Text);
            var pWindowRule = insertPreMadePresetsCmd.Parameters.Add("@WindowRule", SqliteType.Text);
            var pPathRule = insertPreMadePresetsCmd.Parameters.Add("@PathRule", SqliteType.Text);

            foreach (var p in presets)
            {
                pTableId.Value = p.TableId;
                pName.Value = p.Name;
                pWindowTitle.Value = (object?)p.WindowTitle ?? DBNull.Value;
                pWindowRule.Value = (object?)p.WindowRule ?? DBNull.Value;
                pFilePath.Value = (object?)p.FilePath ?? DBNull.Value;
                pPathRule.Value = (object?)p.PathRule ?? DBNull.Value;

                await insertPreMadePresetsCmd.ExecuteNonQueryAsync(cancellationToken);
            }

            await tran.CommitAsync(cancellationToken);
        }

        _presetIds = await Utils.FetchTableIdsAsync(conn, PresetNames.AutoGame, cancellationToken);
    }

    public static List<AutoGameDto> GetPreMadePresets() =>
    [
        new()
        {
            TableId = 1,
            Name = "Spotify",
            PathRule = @"[/\\]spotify(\.exe)?$"
        }
    ];

    public QueryTableIdResult GetTableId(DisplayId displayId)
    {
        return !Utils.IsWithinBounds(displayId, _presetIds)
            ? new QueryTableIdResult(FailureReason: $"[FAIL] Cannot find game with provided id because its outside the possible values range. (Max value is {_presetIds.Count}). Ignoring command...")
            : new QueryTableIdResult(Ok: true, TableId: _presetIds[displayId.V - 1]);
    }

    public async Task<QueryPresetResult> GetPresetAsync(TableId tableId, CancellationToken cancellationToken)
    {
        await using var conn = await Utils.CreateSqlConnAsync(_connStr, cancellationToken, queryOnly: true);
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = """
                          SELECT
                              Id,
                              Name,
                              WindowTitle,
                              FilePath,
                              WindowRule,
                              PathRule
                          FROM AutoGamePresets
                          WHERE Id = @Id;
                          """;
        cmd.Parameters.AddWithValue("@Id", tableId.V);

        try
        {
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
                return new QueryPresetResult(FailureReason: "[FAIL] Preset not found in database. Ignoring command...");

            return new QueryPresetResult(Ok: true,
                                         GamePreset: new AutoGameRecord(Utils.ReadAutoGamePreset(reader)));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new QueryPresetResult(FailureReason: $"[FAIL] Database error msg: {ex.Message}. Ignoring command...");
        }
    }

    // Types
    public enum PresetNames
    {
        AutoGame
    }

    // Results
    public record struct QueryTableIdResult(bool Ok = false, TableId? TableId = null, string? FailureReason = null);

    public record struct QueryPresetResult(bool Ok = false, AutoGameRecord? GamePreset = null, string? FailureReason = null);
}