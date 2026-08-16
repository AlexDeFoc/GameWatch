using System;
using System.Collections.Generic;
using System.IO;
using GameWatch.Core.Helpers;
using GameWatch.Core.Types;
using Microsoft.Data.Sqlite;

namespace GameWatch.Core.Dbs;

public sealed class GamePresets
{
    public static GamePresets Instance { get; private set; } = null!;

    private readonly string _connStr;
    private readonly List<TableId> _presetIds = [];

    public static void Init(string relPathToParent) => Instance = new GamePresets(relPathToParent);

    private GamePresets(string relPathToParent)
    {
        var dbParentPath = PathResolver.ResolveRelativePath(relPathToParent);
        var dbPath = Path.Join(dbParentPath, "GamePresets.db");

        _connStr = $"Data Source={dbPath}";

        if (!string.IsNullOrEmpty(dbParentPath) && !Directory.Exists(dbParentPath))
            Directory.CreateDirectory(dbParentPath);

        if (!File.Exists(dbPath))
            return;

        using var conn = CreateConnection();

        Utils.ExecuteNonQuery(conn, """
                                    PRAGMA journal_mode = WAL;
                                    PRAGMA user_version = 1;
                                    PRAGMA encoding = UTF-8;
                                    """);

        using (var tran = conn.BeginTransaction())
        {
            Utils.ExecuteNonQuery(conn, """
                                        CREATE TABLE IF NOT EXISTS AutoGamePresets (
                                            Id INTEGER PRIMARY KEY,
                                            Name TEXT NOT NULL,
                                            PlayTimeSec INTEGER NOT NULL DEFAULT 0,
                                            WindowTitle TEXT DEFAULT NULL,
                                            FilePath TEXT DEFAULT NULL,
                                            WindowRule TEXT DEFAULT NULL,
                                            PathRule TEXT DEFAULT NULL
                                        ) STRICT;
                                        """, tran);

            using var insertPreMadePresetsCmd = conn.CreateCommand();
            insertPreMadePresetsCmd.Transaction = tran;
            insertPreMadePresetsCmd.CommandText = """
                                                  INSERT INTO AutoGamePresets (
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
                                                      @PathRule
                                                  );
                                                  """;

            var presets = GetPreMadePresets();

            var pName = insertPreMadePresetsCmd.Parameters.Add("@Name", SqliteType.Text);
            var pPlayTimeSec = insertPreMadePresetsCmd.Parameters.Add("@PlayTimeSec", SqliteType.Integer);
            var pWindowTitle = insertPreMadePresetsCmd.Parameters.Add("@WindowTitle", SqliteType.Text);
            var pFilePath = insertPreMadePresetsCmd.Parameters.Add("@FilePath", SqliteType.Text);
            var pWindowRule = insertPreMadePresetsCmd.Parameters.Add("@WindowRule", SqliteType.Text);
            var pPathRule = insertPreMadePresetsCmd.Parameters.Add("@PathRule", SqliteType.Text);

            foreach (var p in presets)
            {
                pName.Value = p.Name;
                pPlayTimeSec.Value = p.PlayTimeSec;
                pWindowTitle.Value = p.WindowTitle;
                pFilePath.Value = p.FilePath;
                pWindowRule.Value = p.WindowRule;
                pPathRule.Value = p.PathRule;

                insertPreMadePresetsCmd.ExecuteNonQuery();
            }
        }

        _presetIds = Utils.FetchTableIds(conn, "AutoGamePresets");
    }

    public static List<AutoGameDto> GetPreMadePresets() =>
    [
        new()
        {
            Name = "Spotify",
            PathRule = @"[/\\]spotify(\.exe)?$"
        }
    ];

    public QueryTableIdResult GetTableId(DisplayId displayId)
    {
        return !Utils.IsWithinBounds(displayId, _presetIds)
            ? new QueryTableIdResult(FailureReason: "[FAIL] Cannot find game with provided id. Ignoring command...")
            : new QueryTableIdResult(Ok: true, TableId: _presetIds[displayId.V - 1]);
    }

    public QueryPresetResult GetPreset(TableId tableId)
    {
        using var conn = CreateConnection(queryOnly: true);
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
                          FROM AutoGamePresets
                          WHERE Id = @Id;
                          """;
        cmd.Parameters.AddWithValue("@Id", tableId.V);

        try
        {
            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return new QueryPresetResult(FailureReason: "[FAIL] Preset not found in database. Ignoring command...");

            return new QueryPresetResult(Ok: true,
                                         GamePreset: new AutoGameRecord(Utils.ReadAutoGame(reader)));
        }
        catch (Exception ex)
        {
            return new QueryPresetResult(FailureReason: $"[FAIL] Database error msg: {ex.Message}. Ignoring command...");
        }
    }

    private SqliteConnection CreateConnection(bool queryOnly = false) => Utils.CreateSqlConnection(_connStr, queryOnly);

    public record struct QueryTableIdResult(bool Ok = false, TableId? TableId = null, string? FailureReason = null);

    public record struct QueryPresetResult(bool Ok = false, AutoGameRecord? GamePreset = null, string? FailureReason = null);
}