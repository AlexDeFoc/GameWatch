using Dapper;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace GameWatch.FileManager.Migrators.GameLibrary.V1_To_V2;

public static class Migrator
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

            var sourceFileContents = File.ReadAllText(sourceFilePath);
            var v1Data = JsonSerializer.Deserialize<FileSchemas.GameLibrary.V1.GameCollection>(sourceFileContents) ?? throw new InvalidDataException();

            CreateDatabaseAndTable(connectionString);

            if (v1Data.Games.Count == 0)
                return false;

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            const string insertSql = """
                                     INSERT INTO Games (Title,
                                                        PlayTime,
                                                        FingerprintFullPath,
                                                        FingerprintProcessName,
                                                        FingerprintCommandLine,
                                                        FingerprintProductName)
                                     VALUES (@Title,
                                             @PlayTime,
                                             @FingerprintFullPath,
                                             @FingerprintProcessName,
                                             @FingerprintCommandLine,
                                             @FingerprintProductName);
                                     """;

            foreach (var v2Game in v1Data.Games.Select(v1Game => new FileSchemas.GameLibrary.V2.GameEntry
                     {
                         Title = v1Game.Title,
                         PlayTime = v1Game.PlayTime
                     }))
            {
                connection.Execute(insertSql, v2Game);
            }
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

        const string createTableSql = """
                                      CREATE TABLE Games (Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                                          Title TEXT NOT NULL DEFAULT '',
                                                          PlayTime INTEGER NOT NULL,
                                                          FingerprintFullPath TEXT NOT NULL DEFAULT '',
                                                          FingerprintProcessName TEXT NOT NULL DEFAULT '',
                                                          FingerprintCommandLine TEXT NOT NULL DEFAULT '',
                                                          FingerprintProductName TEXT NOT NULL DEFAULT ''
                                                          );
                                      """;

        connection.Execute(createTableSql);
    }
}