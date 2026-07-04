using System.IO;
using System.Linq;
using System.Text.Json;
using Dapper;
using GameWatch.DataTypes;
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

            var insertIntoMetadataTableCmd = Generators.V2.GameLibrary.GetStringToUpdateMetadata();

            var insertIntoGameCollectionTableCmd = Generators.V2.GameLibrary.GetStringToUpdateGameEntry();

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

        var createMetadataTableCmd = Generators.V2.GameLibrary.GetStringToCreateMetadataTable();

        var createGameCollectionTableCmd = Generators.V2.GameLibrary.GetStringToCreateGameCollectionTable();

        connection.Execute(createMetadataTableCmd);
        connection.Execute(createGameCollectionTableCmd);
    }
}