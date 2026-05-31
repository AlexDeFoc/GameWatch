using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace MainApp;

using LatestFileTable = AppSettings.FileSettingsTableV2;

public sealed class AppSettings
{
    public event EventHandler<LanguageManager.LanguageCode>? LanguageChanged;

    public LanguageManager.LanguageCode LanguageCode
    {
        get;
        private init
        {
            if (field == value) return;
            field = value;
            OnLanguageChanged(value);
        }
    }

    private void OnLanguageChanged(LanguageManager.LanguageCode newLang)
    {
        LanguageChanged?.Invoke(this, newLang);
    }

    private AppSettings()
    {
        LanguageCode = LanguageManager.LanguageCode.en_US;
    }

    private class FileVersionTable
    {
        public int Id { get; init; }
        public int Value { get; init; }
    }

    internal class FileSettingsTableV2
    {
        public int Id { get; init; }
        public string ActiveLanguageCode { get; init; } = nameof(LanguageManager.LanguageCode.en_US);
    }

    private class FileDbContext : DbContext
    {
        public DbSet<FileVersionTable> FileVersions { get; set; }
        public DbSet<LatestFileTable> AppSettings { get; set; }

        private readonly string _dbPath;

        public FileDbContext(string dbPath)
        {
            _dbPath = dbPath;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder opts) => opts.UseSqlite($"Data Source ={_dbPath}");
    }

    public class SettingsLoader
    {
        private readonly string _dbPath = Utils.GetFilepathInUserAppData("Settings.db");
        private const int LatestVersion = 2;

        public AppSettings LoadSettings()
        {
            if (!File.Exists(_dbPath))
                return CreateFreshDatabase();

            int version = ReadVersion();

            if (version == LatestVersion)
            {
                using var db = new FileDbContext(_dbPath);
                var row = db.AppSettings.FirstOrDefault();
                return ConvertToAppSettings(row);
            }

            // Version mismatch - read old version using raw SQL
            var oldData = ReadOldSettings(version);
            var newSettings = MigrateToLatest(oldData);

            // Delete and recreate with latest schema
            File.Delete(_dbPath);
            {
                using var newDb = new FileDbContext(_dbPath);
                newDb.Database.EnsureCreated();
                newDb.FileVersions.Add(new FileVersionTable { Value = LatestVersion });
                newDb.AppSettings.Add(ConvertToLatestFileTable(newSettings));
                newDb.SaveChanges();
            }

            return newSettings;
        }

        public void SaveSettings(AppSettings settings)
        {
            using var db = new FileDbContext(_dbPath);
            var row = db.AppSettings.FirstOrDefault();
            if (row == null)
            {
                db.AppSettings.Add(ConvertToLatestFileTable(settings));
            }
        }

        private int ReadVersion()
        {
            try
            {
                using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={_dbPath}");
                connection.Open();
                var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT Value FROM FileVersions LIMIT 1";
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch
            {
                return 0;
            }
        }

        // NOTE: This will not be called until we add newer file version
        private object ReadOldSettings(int version)
        {
            using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            if (version == 3)
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT ActiveLanguageCode FROM AppSettings LIMIT 1";
                // example code, maybe language thingy will be gone...
                var lang = cmd.ExecuteScalar()?.ToString() ?? nameof(LanguageManager.LanguageCode.en_US);
                return new { ActiveLanguageCode = lang };
            }
            else if (version == 4)
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT ActiveLanguageCode FROM AppSettings LIMIT 1";
                // example code, maybe language thingy will be gone...
                var lang = cmd.ExecuteScalar()?.ToString() ?? nameof(LanguageManager.LanguageCode.en_US);
                return new { ActiveLanguageCode = lang };
            }
            else
            {
                // example code, maybe language thingy will be gone...
                return new { ActiveLanguageCode = nameof(LanguageManager.LanguageCode.en_US) }; // default
            }
        }

        private static AppSettings MigrateToLatest(object oldData)
        {
            dynamic data = oldData;
            return new AppSettings
            {
                LanguageCode = ParseLanguageCode(data.ActiveLanguageCode)
            };
        }

        private static AppSettings ConvertToAppSettings(LatestFileTable? row)
        {
            if (row == null)
                return new AppSettings();

            return new AppSettings
            {
                LanguageCode = ParseLanguageCode(row.ActiveLanguageCode)
            };
        }

        private static LatestFileTable ConvertToLatestFileTable(AppSettings appSettings) => ConvertToFileTableV2(appSettings);

        // ReSharper disable once UseSymbolAlias
        private static FileSettingsTableV2 ConvertToFileTableV2(AppSettings appSettings)
        {
            // ReSharper disable once UseSymbolAlias
            return new FileSettingsTableV2
            {
                ActiveLanguageCode = appSettings.LanguageCode.ToString()
            };
        }

        private AppSettings CreateFreshDatabase()
        {
            var defaultSettings = new AppSettings();
            using var db = new FileDbContext(_dbPath);
            db.Database.EnsureCreated();
            db.FileVersions.Add(new FileVersionTable { Value = LatestVersion });
            db.AppSettings.Add(ConvertToLatestFileTable(defaultSettings));
            db.SaveChanges();
            return defaultSettings;
        }

        private static LanguageManager.LanguageCode ParseLanguageCode(string code)
        {
            return Enum.TryParse<LanguageManager.LanguageCode>(code, out var result) ? result : LanguageManager.LanguageCode.en_US;
        }
    }
}