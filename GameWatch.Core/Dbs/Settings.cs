using System.IO;
using System.Text;
using System.Threading;
using Dapper;
using Microsoft.Data.Sqlite;

namespace GameWatch.Core.Dbs;

[DapperAot]
public partial class Settings
{
    public int GameMonitorAgentGamePlayTimeSaveThreshold
    {
        get => Interlocked.CompareExchange(ref field, 60, 60);
        private set => Interlocked.Exchange(ref field, value);
    }

    public static Settings Instance { get; private set; } = null!;

    private readonly string _connString;

    public static void Init(string relPathToParent) => Instance = new Settings(relPathToParent);

    private Settings(string relPathToParent)
    {
        var dbParentPath = PathResolver.ResolveRelativePath(relPathToParent);
        var dbPath = Path.Join(dbParentPath, "Settings.db");

        _connString = $"Data Source={dbPath}";

        if (!string.IsNullOrEmpty(dbParentPath) && !Directory.Exists(dbParentPath))
            Directory.CreateDirectory(dbParentPath);

        using var conn = CreateConnection();
        using var tran = conn.BeginTransaction();

        const string oneTimePragmas = """
                                      PRAGMA journal_mode = WAL;
                                      PRAGMA user_version = 1;
                                      PRAGMA encoding = UTF-8;
                                      """;
        conn.Execute(oneTimePragmas, transaction: tran);

        const string createTableSql = """
                                       CREATE TABLE IF NOT EXISTS GameMonitorAgent (
                                           Id INTEGER PRIMARY KEY,
                                           AutoSaveOnDiskSec INTEGER NOT NULL DEFAULT 60
                                       ) STRICT;
                                       """;

        conn.Execute(createTableSql, transaction: tran);
        tran.Commit();
    }

    private SqliteConnection CreateConnection(bool queryOnly = false)
    {
        var conn = new SqliteConnection(_connString);
        conn.Open();

        var sb = new StringBuilder();

        sb.Append("""
                  PRAGMA synchronous = NORMAL;
                  PRAGMA busy_timeout = 5000;
                  PRAGMA temp_store = MEMORY;
                  PRAGMA foreign_keys = ON;
                  """);

        if (queryOnly)
            sb.Append("PRAGMA query_only = ON;");

        conn.Execute(sb.ToString());
        return conn;
    }
}