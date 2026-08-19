using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Core.Helpers;

namespace GameWatch.Core.Dbs;

public sealed class Settings
{
    public static Settings Instance { get; private set; } = null!;

    private string _connStr = null!;

    private Settings()
    {
    }

    public static async Task CreateAndInitAsync(string relPathToParent, CancellationToken cancellationToken)
    {
        var settings = new Settings();
        await settings.InitAsync(relPathToParent, cancellationToken);

        Instance = settings;
    }

    private async Task InitAsync(string relPathToParent, CancellationToken cancellationToken)
    {
        var dbFolderPath = PathResolver.ResolveRelativePath(relPathToParent);
        var dbPath = Path.Join(dbFolderPath, "Settings.db");
        _connStr = $"Data Source={dbPath}";

        if (!string.IsNullOrEmpty(dbFolderPath) && !Directory.Exists(dbFolderPath))
            Directory.CreateDirectory(dbFolderPath);

        await EnsureDatabaseCreatedAndSeeded(cancellationToken);

        await using var conn = await Utils.CreateSqlConnAsync(_connStr, cancellationToken: cancellationToken);

        const string readGameMonitorAgentSettings = """
                                                    SELECT
                                                        GamePlayTimeSaveThreshold
                                                    FROM GameMonitorAgent
                                                    WHERE Id = @Id
                                                    """;

        await using var cmd = conn.CreateCommand();
        cmd.Parameters.AddWithValue("@Id", 1);
        cmd.CommandText = readGameMonitorAgentSettings;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
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

    private async Task EnsureDatabaseCreatedAndSeeded(CancellationToken cancellationToken)
    {
        await using var conn = await Utils.CreateSqlConnAsync(_connStr, cancellationToken: cancellationToken);

        await Utils.ExecuteNonQueryAsync(conn, """
                                               PRAGMA journal_mode = WAL;
                                               PRAGMA user_version = 1;
                                               PRAGMA encoding = UTF-8;
                                               """, cancellationToken: cancellationToken);

        await using var tran = conn.BeginTransaction();

        await Utils.ExecuteNonQueryAsync(conn, """
                                               CREATE TABLE IF NOT EXISTS GameMonitorAgent (
                                                   Id INTEGER PRIMARY KEY CHECK (Id = 1),
                                                   GamePlayTimeSaveThreshold INTEGER NOT NULL DEFAULT 60
                                               ) STRICT;
                                               """, cancellationToken, tran);

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

        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tran;
        cmd.CommandText = insertIntoGameMonitorAgentSql;
        cmd.Parameters.AddWithValue("@GamePlayTimeSaveThreshold", GetGameMonitorAgentDefaults().GamePlayTimeSaveThreshold);
        await cmd.ExecuteNonQueryAsync(cancellationToken);

        await tran.CommitAsync(cancellationToken);
    }

    private static GameMonitorAgentSettingsDto GetGameMonitorAgentDefaults()
    {
        return new GameMonitorAgentSettingsDto
        {
            GamePlayTimeSaveThreshold = 60
        };
    }

    private sealed class GameMonitorAgentSettingsDto
    {
        public long GamePlayTimeSaveThreshold { get; init; }
    }
}