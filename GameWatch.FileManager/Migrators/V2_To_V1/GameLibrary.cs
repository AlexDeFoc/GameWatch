using System.IO;
using System.Linq;
using System.Text.Json;
using Dapper;
using Microsoft.Data.Sqlite;

namespace GameWatch.FileManager.Migrators.V2_To_V1;

public static class GameLibrary
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true
    };

    /// <summary>
    /// Migrates src file to dest file
    /// </summary>
    /// <returns>Whether migration was succesful or not</returns>
    public static bool Run(string sourceFilePath, string destFilePath)
    {
        var connectionString = $"Data Source={sourceFilePath}"; // location where the db file is

        if (!File.Exists(sourceFilePath))
            return false;

        if (File.Exists(destFilePath))
            File.Delete(destFilePath);

        var destDir = Path.GetDirectoryName(destFilePath);
        if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
            Directory.CreateDirectory(destDir);

        try
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var selectSql = Generators.V2.GameLibrary.GetStringToQueryGameCollection();

            var v2Games = connection.Query<FileSchemas.V2.GameLibrary.GameEntry>(selectSql);

            var v1Games = v2Games.Select(dbGame => new FileSchemas.V1.GameLibrary.GameEntry { Title = dbGame.Title, PlayTime = dbGame.PlayTime }).ToList();

            var v1Data = new FileSchemas.V1.GameLibrary.MetadataAndGameCollection
            {
                Games = v1Games
            };

            var json = JsonSerializer.Serialize(v1Data, JsonSerializerOptions);

            File.WriteAllText(destFilePath, json);
        }
        catch
        {
            return false;
        }

        return true;
    }
}