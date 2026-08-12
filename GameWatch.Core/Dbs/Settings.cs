using System;
using System.IO;
using System.Threading;
using Microsoft.Data.Sqlite;

namespace GameWatch.Core.Dbs;

public sealed class Settings
{
    public static Settings Instance { get; private set; } = null!;

    public static void Init(string relPathToParent) => Instance = new Settings(relPathToParent);

    public int GameMonitorAgentGamePlayTimeSaveThreshold
    {
        get => Interlocked.CompareExchange(ref field, 0, 0);
        private set => Interlocked.Exchange(ref field, value);
    }

    private readonly string _connString;

    private Settings(string relativePathToAppDataFolder)
    {
        var dbFolderPath = PathResolver.ResolveRelativePath(relativePathToAppDataFolder);
        var dbPath = Path.Join(dbFolderPath, "Settings.db");
        _connString = $"Data Source={dbPath}";

        if (!string.IsNullOrEmpty(dbFolderPath) && !Directory.Exists(dbFolderPath))
            Directory.CreateDirectory(dbFolderPath);

        EnsureDatabaseCreatedAndSeeded();

        // Init variables
        // Gathering stage
        using var conn = CreateConnection();

        const string readGameMonitorAgentSettings = """
                                                    SELECT
                                                        GamePlayTimeSaveThreshold
                                                    FROM GameMonitorAgent
                                                    WHERE Id = @Id
                                                    """;

        using var cmd = conn.CreateCommand();
        cmd.CommandText = readGameMonitorAgentSettings;

        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            var threshold = reader.GetInt32(0);
            GameMonitorAgentGamePlayTimeSaveThreshold = threshold < 1
                ? GetGameMonitorAgentDefaults().GamePlayTimeSaveThreshold
                : threshold;
        }
        else
        {
            GameMonitorAgentGamePlayTimeSaveThreshold = GetGameMonitorAgentDefaults().GamePlayTimeSaveThreshold;
        }
    }

    private void EnsureDatabaseCreatedAndSeeded()
    {
        using var conn = CreateConnection();

        ExecuteNonQuery(conn, """
                              PRAGMA journal_mode = WAL;
                              PRAGMA mmap_size = 134217728;
                              """);

        using var tran = conn.BeginTransaction();

        ExecuteNonQuery(conn, """
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

    private GameMonitorAgentSettingsDto GetGameMonitorAgentDefaults()
    {
        return new GameMonitorAgentSettingsDto
        {
            GamePlayTimeSaveThreshold = 60
        };
    }

    public void ResetGameMonitorAgentSettings(SqliteConnection conn, SqliteTransaction tran)
    {
        const string resetSql = """
                                INSERT INTO GameMonitorAgent (Id, GamePlayTimeSaveThreshold)
                                VALUES (1, @GamePlayTimeSaveThreshold)
                                ON CONFLICT(Id) DO UPDATE SET 
                                    GamePlayTimeSaveThreshold = EXCLUDED.GamePlayTimeSaveThreshold;
                                """;

        using var cmd = conn.CreateCommand();
        cmd.Transaction = tran;
        cmd.CommandText = resetSql;
        cmd.Parameters.AddWithValue("@GamePlayTimeSaveThreshold", GetGameMonitorAgentDefaults().GamePlayTimeSaveThreshold);
        cmd.ExecuteNonQuery();
    }

    private string GetTableName(SettingsTarget target) => target switch
    {
        SettingsTarget.GameMonitorAgent => "GameMonitorAgent",
        _ => throw new ArgumentOutOfRangeException(nameof(target), target, "Unsupported target.")
    };

    private SqliteConnection CreateConnection()
    {
        var conn = new SqliteConnection(_connString);
        conn.Open();

        ExecuteNonQuery(conn, """
                              PRAGMA synchronous = NORMAL;
                              PRAGMA busy_timeout = 5000;
                              PRAGMA temp_store = MEMORY;
                              """);

        return conn;
    }

    private static void ExecuteNonQuery(SqliteConnection conn, string sql, SqliteTransaction? tran = null)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        if (tran != null) cmd.Transaction = tran;
        cmd.ExecuteNonQuery();
    }

    private enum SettingsTarget
    {
        GameMonitorAgent
    }

    private sealed class GameMonitorAgentSettingsDto
    {
        public int Id { get; set; }
        public int GamePlayTimeSaveThreshold { get; set; }
    }
}