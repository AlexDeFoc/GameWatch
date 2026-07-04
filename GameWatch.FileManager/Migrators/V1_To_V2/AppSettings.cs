using System.IO;
using Dapper;
using Microsoft.Data.Sqlite;

namespace GameWatch.FileManager.Migrators.V1_To_V2;

public class AppSettings
{
    /// <summary>
    /// Migrates src file to dest file
    /// </summary>
    /// <returns>Whether migration was succesful or not</returns>
    public static bool Run(string sourceFilePath, string destFilePath)
    {
        try
        {
            var connectionString = $"Data Source={destFilePath}"; // location where the db file will be

            if (!File.Exists(sourceFilePath))
                return false;

            if (File.Exists(destFilePath))
                File.Delete(destFilePath);

            var destDir = Path.GetDirectoryName(destFilePath);
            if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            CreateDatabaseAndTable(connectionString);

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            const string insertIntoMetadataTableCmd = """
                                                      INSERT INTO Metadata (FileVersion)
                                                      VALUES (@FileVersion);
                                                      """;

            const string insertIntoSettingsTableCmd = """
                                                      INSERT INTO Settings (AppLanguageTag)
                                                      VALUES (@AppLanguageTag);
                                                      """;

            var metadataData = new FileSchemas.V2.AppSettings.Metadata();

            connection.Execute(insertIntoMetadataTableCmd, metadataData);

            var appSettingsData = new FileSchemas.V2.AppSettings.Settings();

            connection.Execute(insertIntoSettingsTableCmd, appSettingsData);
        }
        catch
        {
            return false;
        }

        return true;
    }

    private static void CreateDatabaseAndTable(string connectionString)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        const string createMetadataTableCmd = """
                                              CREATE TABLE Metadata (Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                                                     FileVersion INTEGER NOT NULL);
                                              """;

        const string createSettingsTableCmd = $"""
                                               CREATE TABLE Settings (Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                                                      AppLanguageTag TEXT NOT NULL);
                                               """;

        connection.Execute(createMetadataTableCmd);
        connection.Execute(createSettingsTableCmd);
    }
}