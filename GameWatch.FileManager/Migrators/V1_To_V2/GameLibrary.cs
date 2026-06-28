using System.IO;
using System.Linq;
using System.Text.Json;
using Dapper;
using Microsoft.Data.Sqlite;

namespace GameWatch.FileManager.Migrators.V1_To_V2;

public static class GameLibrary
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
            var v1Data = JsonSerializer.Deserialize<FileSchemas.V1.GameLibrary.MetadataAndGameCollection>(sourceFileContents) ?? throw new InvalidDataException();

            CreateDatabaseAndTable(connectionString);

            if (v1Data.Games.Count == 0)
                return false;

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            const string insertIntoMetadataTableCmd = """
                                                      INSERT INTO Metadata (FileVersion)
                                                      VALUES (@FileVersion);
                                                      """;

            const string insertIntoGameCollectionTableCmd = """
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

            var metadataData = new FileSchemas.V2.GameLibrary.Metadata();

            connection.Execute(insertIntoMetadataTableCmd, metadataData);

            foreach (var v2Game in v1Data.Games.Select(v1Game => new FileSchemas.V2.GameLibrary.GameEntry
                     {
                         Title = v1Game.Title,
                         PlayTime = v1Game.PlayTime
                     }))
            {
                connection.Execute(insertIntoGameCollectionTableCmd, v2Game);
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

        const string createMetadataTableCmd = """
                                              CREATE TABLE Metadata (Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                                                                FileVersion INTEGER NOT NULL
                                                                                );
                                              """;

        const string createGameCollectionTableCmd = """
                                                    CREATE TABLE Games (Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                                                        Title TEXT NOT NULL DEFAULT '',
                                                                        PlayTime INTEGER NOT NULL,
                                                                        FingerprintFullPath TEXT NOT NULL DEFAULT '',
                                                                        FingerprintProcessName TEXT NOT NULL DEFAULT '',
                                                                        FingerprintCommandLine TEXT NOT NULL DEFAULT '',
                                                                        FingerprintProductName TEXT NOT NULL DEFAULT ''
                                                                        );
                                                    """;

        connection.Execute(createMetadataTableCmd);
        connection.Execute(createGameCollectionTableCmd);
    }
}