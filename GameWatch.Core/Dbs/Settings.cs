using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GameWatch.Core.Helpers;

namespace GameWatch.Core.Dbs;

public static class Settings
{
    private static string _connStr = null!;

    private static async Task EnsureDatabaseCreatedAndSeeded(CancellationToken cancellationToken)
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
                                                   IsLoggingEnabled INTEGER NOT NULL DEFAULT 1,
                                                   ProcessScanInterval INTEGER NOT NULL DEFAULT 10,
                                                   PlayTimeFlushInterval INTEGER NOT NULL DEFAULT 60
                                               ) STRICT;
                                               """, cancellationToken, tran);

        // insert relying on db schema defaults
        const string insertIntoGameMonitorAgentSql = """
                                                     INSERT INTO GameMonitorAgent (Id)
                                                     VALUES (1)
                                                     ON CONFLICT(Id) DO NOTHING;
                                                     """;

        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tran;
        cmd.CommandText = insertIntoGameMonitorAgentSql;
        await cmd.ExecuteNonQueryAsync(cancellationToken);

        await tran.CommitAsync(cancellationToken);
    }

    public sealed class GameMonitorAgent
    {
        private int _isLoggingEnabled;

        public static GameMonitorAgent Instance { get; private set; } = null!;

        public bool CachedSettingIsLoggingEnabled
        {
            get => Volatile.Read(ref _isLoggingEnabled) == 1;
            private set => Interlocked.Exchange(ref _isLoggingEnabled, value ? 1 : 0);
        }

        public long CachedSettingProcessScanInterval
        {
            get => Volatile.Read(ref field);
            private set => Interlocked.Exchange(ref field, value);
        }

        public long CachedSettingPlayTimeFlushInterval
        {
            get => Volatile.Read(ref field);
            private set => Interlocked.Exchange(ref field, value);
        }

        private GameMonitorAgent()
        {
        }

        public static async Task CreateAndInitAsync(string relPathToParent, CancellationToken cancellationToken)
        {
            var settings = new GameMonitorAgent();
            await settings.InitAsync(relPathToParent, cancellationToken);

            Instance = settings;
        }

        public async Task<ModifySettingShouldLoggingBeEnabledResult> ModifySettingShouldLoggingBeEnabled(bool newValue, CancellationToken cancellationToken)
        {
            await using var conn = await Utils.CreateSqlConnAsync(_connStr, cancellationToken: cancellationToken);
            await using var tran = conn.BeginTransaction();
            await using var cmd = conn.CreateCommand();
            cmd.Transaction = tran;
            cmd.CommandText = "UPDATE GameMonitorAgent SET IsLoggingEnabled = @Value WHERE Id = @Id;";
            cmd.Parameters.AddWithValue("@Id", 1);
            cmd.Connection = conn;
            cmd.Transaction = tran;
            cmd.Parameters.AddWithValue("@Value", newValue ? 1 : 0);

            try
            {
                await cmd.ExecuteNonQueryAsync(cancellationToken);

                await tran.CommitAsync(cancellationToken);

                CachedSettingIsLoggingEnabled = newValue;

                return new ModifySettingShouldLoggingBeEnabledResult(Ok: true);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                return new ModifySettingShouldLoggingBeEnabledResult(FailureReason: $"[FAIL] Database error msg: {ex.Message}. Ignoring command...");
            }
        }

        public async Task<ModifySettingProcessScanIntervalResult> ModifySettingProcessScanInterval(long newValue, CancellationToken cancellationToken)
        {
            await using var conn = await Utils.CreateSqlConnAsync(_connStr, cancellationToken: cancellationToken);
            await using var tran = conn.BeginTransaction();
            await using var cmd = conn.CreateCommand();
            cmd.Transaction = tran;
            cmd.CommandText = "UPDATE GameMonitorAgent SET ProcessScanInterval = @Value WHERE Id = @Id;";
            cmd.Parameters.AddWithValue("@Id", 1);
            cmd.Connection = conn;
            cmd.Transaction = tran;
            cmd.Parameters.AddWithValue("@Value", newValue);

            try
            {
                await cmd.ExecuteNonQueryAsync(cancellationToken);

                await tran.CommitAsync(cancellationToken);

                CachedSettingProcessScanInterval = newValue;

                return new ModifySettingProcessScanIntervalResult(Ok: true);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                return new ModifySettingProcessScanIntervalResult(FailureReason: $"[FAIL] Database error msg: {ex.Message}. Ignoring command...");
            }
        }

        public async Task<ModifySettingPlayTimeFlushIntervalResult> ModifySettingPlayTimeFlushInterval(long newValue, CancellationToken cancellationToken)
        {
            await using var conn = await Utils.CreateSqlConnAsync(_connStr, cancellationToken: cancellationToken);
            await using var tran = conn.BeginTransaction();
            await using var cmd = conn.CreateCommand();
            cmd.Transaction = tran;
            cmd.CommandText = "UPDATE GameMonitorAgent SET PlayTimeFlushInterval = @Value WHERE Id = @Id;";
            cmd.Parameters.AddWithValue("@Id", 1);
            cmd.Connection = conn;
            cmd.Transaction = tran;
            cmd.Parameters.AddWithValue("@Value", newValue);

            try
            {
                await cmd.ExecuteNonQueryAsync(cancellationToken);

                await tran.CommitAsync(cancellationToken);

                CachedSettingPlayTimeFlushInterval = newValue;

                return new ModifySettingPlayTimeFlushIntervalResult(Ok: true);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                return new ModifySettingPlayTimeFlushIntervalResult(FailureReason: $"[FAIL] Database error msg: {ex.Message}. Ignoring command...");
            }
        }

        // TODO: DEPRECATED REMOVE/COULD REPLACE MODIFY ONE
        private static async Task ResetSettingIsLoggingEnabled(CancellationToken cancellationToken)
        {
            await using var conn = await Utils.CreateSqlConnAsync(_connStr, cancellationToken: cancellationToken);
            await using var tran = conn.BeginTransaction();
            await using var resetCmd = conn.CreateCommand();
            resetCmd.Transaction = tran;
            resetCmd.Parameters.AddWithValue("@Id", 1);
            resetCmd.CommandText = "UPDATE GameMonitorAgent SET IsLoggingEnabled = @Value WHERE Id = @Id;";
            resetCmd.Parameters.AddWithValue("@Value", Defaults.IsLoggingEnabledStatus ? 1 : 0);

            await resetCmd.ExecuteNonQueryAsync(cancellationToken);

            await tran.CommitAsync(cancellationToken);
        }

        // TODO: DEPRECATED REMOVE/COULD REPLACE MODIFY ONE
        private static async Task ResetSettingProcessScanInterval(CancellationToken cancellationToken)
        {
            await using var conn = await Utils.CreateSqlConnAsync(_connStr, cancellationToken: cancellationToken);
            await using var tran = conn.BeginTransaction();
            await using var resetCmd = conn.CreateCommand();
            resetCmd.Transaction = tran;
            resetCmd.Parameters.AddWithValue("@Id", 1);
            resetCmd.CommandText = "UPDATE GameMonitorAgent SET ProcessScanInterval = @Value WHERE Id = @Id;";
            resetCmd.Parameters.AddWithValue("@Value", Defaults.ProcessScanInterval);

            await resetCmd.ExecuteNonQueryAsync(cancellationToken);

            await tran.CommitAsync(cancellationToken);
        }

        // TODO: DEPRECATED REMOVE/COULD REPLACE MODIFY ONE
        private static async Task ResetPlayTimeFlushInterval(CancellationToken cancellationToken)
        {
            await using var conn = await Utils.CreateSqlConnAsync(_connStr, cancellationToken: cancellationToken);
            await using var tran = conn.BeginTransaction();
            await using var resetCmd = conn.CreateCommand();
            resetCmd.Transaction = tran;
            resetCmd.Parameters.AddWithValue("@Id", 1);
            resetCmd.CommandText = "UPDATE GameMonitorAgent SET PlayTimeFlushInterval = @Value WHERE Id = @Id;";
            resetCmd.Parameters.AddWithValue("@Value", Defaults.PlayTimeFlushInterval);

            await resetCmd.ExecuteNonQueryAsync(cancellationToken);

            await tran.CommitAsync(cancellationToken);
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

            const string readSettings = """
                                        SELECT
                                            IsLoggingEnabled,
                                            ProcessScanInterval,
                                            PlayTimeFlushInterval
                                        FROM GameMonitorAgent
                                        WHERE Id = @Id
                                        """;

            await using var cmd = conn.CreateCommand();
            cmd.Parameters.AddWithValue("@Id", 1);
            cmd.CommandText = readSettings;

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            var isLoggingEnabledStatusContainsInvalidValue = false;
            var processScanIntervalContainsInvalidValue = false;
            var playTimeIntervalContainsInvalidValue = false;

            var configuredIsLoggingEnabledStatus = false;
            var configuredProcessScanInterval = false;
            var configuredPlayTimeInterval = false;

            if (await reader.ReadAsync(cancellationToken))
            {
                // IsLoggingEnabled
                var isLoggingEnabled = reader.GetInt64(0);

                if (isLoggingEnabled is not 1 and not 0)
                {
                    isLoggingEnabledStatusContainsInvalidValue = true;
                }
                else
                {
                    CachedSettingIsLoggingEnabled = isLoggingEnabled is 1;
                    configuredIsLoggingEnabledStatus = true;
                }

                // ProcessScanInterval
                var processScanInterval = reader.GetInt64(1);

                if (processScanInterval < 1L)
                {
                    processScanIntervalContainsInvalidValue = true;
                }
                else
                {
                    CachedSettingProcessScanInterval = processScanInterval;
                    configuredProcessScanInterval = true;
                }

                // PlayTimeFlushInterval
                var playTimeInterval = reader.GetInt64(2);

                if (playTimeInterval < 1L)
                {
                    playTimeIntervalContainsInvalidValue = true;
                }
                else
                {
                    CachedSettingPlayTimeFlushInterval = playTimeInterval;
                    configuredPlayTimeInterval = true;
                }
            }

            if (isLoggingEnabledStatusContainsInvalidValue)
                await ResetSettingIsLoggingEnabled(cancellationToken);

            if (processScanIntervalContainsInvalidValue)
                await ResetSettingProcessScanInterval(cancellationToken);

            if (playTimeIntervalContainsInvalidValue)
                await ResetPlayTimeFlushInterval(cancellationToken);

            if (!configuredIsLoggingEnabledStatus)
                CachedSettingIsLoggingEnabled = Defaults.IsLoggingEnabledStatus;

            if (!configuredProcessScanInterval)
                CachedSettingProcessScanInterval = Defaults.ProcessScanInterval;

            if (!configuredPlayTimeInterval)
                CachedSettingPlayTimeFlushInterval = Defaults.PlayTimeFlushInterval;
        }

        public record struct ModifySettingShouldLoggingBeEnabledResult(bool Ok = false, string? FailureReason = null);
        public record struct ModifySettingProcessScanIntervalResult(bool Ok = false, string? FailureReason = null);
        public record struct ModifySettingPlayTimeFlushIntervalResult(bool Ok = false, string? FailureReason = null);

        public static class Defaults
        {
            public static bool IsLoggingEnabledStatus => true;
            public static long ProcessScanInterval => 60;
            public static long PlayTimeFlushInterval => 60;
        }
    }
}