using System.IO;
using System.Threading;
using GameWatch.Core.Helpers;
using Microsoft.Data.Sqlite;

namespace GameWatch.Core.Dbs;

public sealed class Settings
{
    public static Settings Instance { get; private set; } = null!;

    private readonly string _connStr;

    private Settings(string relPathToParent)
    {
        var dbFolderPath = PathResolver.ResolveRelativePath(relPathToParent);
        var dbPath = Path.Join(dbFolderPath, "Settings.db");
        _connStr = $"Data Source={dbPath}";

        if (!string.IsNullOrEmpty(dbFolderPath) && !Directory.Exists(dbFolderPath))
            Directory.CreateDirectory(dbFolderPath);

        EnsureDatabaseCreatedAndSeeded();

        using var conn = CreateConnection();

        const string readGameMonitorAgentSettings = """
                                                    SELECT
                                                        GamePlayTimeSaveThreshold
                                                    FROM GameMonitorAgent
                                                    WHERE Id = @Id
                                                    """;

        using var cmd = conn.CreateCommand();
        cmd.Parameters.AddWithValue("@Id", 1);
        cmd.CommandText = readGameMonitorAgentSettings;

        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            var threshold = reader.GetInt64(0);
            GameMonitorAgentGamePlayTimeSaveThreshold = threshold < 1L
                ? GetGameMonitorAgentDefaults().GamePlayTimeSaveThreshold
                : threshold;
        }
        else
        {
            GameMonitorAgentGamePlayTimeSaveThreshold = GetGameMonitorAgentDefaults().GamePlayTimeSaveThreshold;
        }
    }

    public long GameMonitorAgentGamePlayTimeSaveThreshold
    {
        get => Interlocked.CompareExchange(ref field, 0L, 0L);
        private set => Interlocked.Exchange(ref field, value);
    }

    public static void Init(string relPathToParent) => Instance = new Settings(relPathToParent);

    private void EnsureDatabaseCreatedAndSeeded()
    {
        using var conn = CreateConnection();

        Utils.ExecuteNonQuery(conn, """
                              PRAGMA journal_mode = WAL;
                              PRAGMA user_version = 1;
                              PRAGMA encoding = UTF-8;
                              """);

        using var tran = conn.BeginTransaction();

        Utils.ExecuteNonQuery(conn, """
                              CREATE TABLE IF NOT EXISTS GameMonitorAgent (
                                  Id INTEGER PRIMARY KEY CHECK (Id = 1),
                                  GamePlayTimeSaveThreshold INTEGER NOT NULL DEFAULT 60
                              ) STRICT;
                              """, tran);

        const string insertIntoGameMonitorAgentSql = """
                                                     INSERT INTO GameMonitorAgent (
                                                         Id,
                                                         GamePlayTimeSaveThreshold
                                                     )
                                                     VALUES (
                                                         1,
                                                         @GamePlayTimeSaveThreshold
                                                     )
                                                     ON CONFLICT(Id) DO NOTHING;
                                                     """;

        using var cmd = conn.CreateCommand();
        cmd.Transaction = tran;
        cmd.CommandText = insertIntoGameMonitorAgentSql;
        cmd.Parameters.AddWithValue("@GamePlayTimeSaveThreshold", GetGameMonitorAgentDefaults().GamePlayTimeSaveThreshold);
        cmd.ExecuteNonQuery();

        tran.Commit();
    }

    private static GameMonitorAgentSettingsDto GetGameMonitorAgentDefaults()
    {
        return new GameMonitorAgentSettingsDto
        {
            GamePlayTimeSaveThreshold = 60
        };
    }

    private SqliteConnection CreateConnection(bool queryOnly = false) => Utils.CreateSqlConnection(_connStr, queryOnly);

    private sealed class GameMonitorAgentSettingsDto
    {
        public long GamePlayTimeSaveThreshold { get; init; }
    }
}