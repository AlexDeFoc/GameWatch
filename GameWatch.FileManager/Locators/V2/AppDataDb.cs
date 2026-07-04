using System.Collections.Generic;
using System.IO;
using Dapper;
using GameWatch.DataTypes;
using Microsoft.Data.Sqlite;

namespace GameWatch.FileManager.Locators.V2;

public static class AppDataDb
{
    public static string GetFilePath() => Path.Combine(GetFolderPath(), "AppData.db");

    public static bool FileExists() => Path.Exists(GetFilePath());

    public static void EnsureFileExists()
    {
        Directory.CreateDirectory(GetFolderPath());
        File.Create(GetFilePath());
    }

    public static void CreateFile()
    {
        var connectionString = $"Data Source={GetFilePath()}";

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        {
            var createFileMetadataTableCmd = Generators.V2.AppDataDb.CreateFileMetadataTableCmd();
            var createGameLibraryTableCmd = Generators.V2.AppDataDb.CreateGameLibraryTableCmd();
            var createSettingsTableCmd = Generators.V2.AppDataDb.CreateSettingsTableCmd();

            connection.Execute(createFileMetadataTableCmd);
            connection.Execute(createGameLibraryTableCmd);
            connection.Execute(createSettingsTableCmd);
        }

        {
            var addNewEntryToFileMetadataTableCmd = Generators.V2.AppDataDb.AddNewEntryToFileMetadataTableCmd();
            var createSettingsTableCmd = Generators.V2.AppDataDb.CreateSettingsTableCmd();

            connection.Execute(createFileMetadataTableCmd);
            connection.Execute(createGameLibraryTableCmd);
            connection.Execute(createSettingsTableCmd);
        }
    }

    private static string GetFolderPath() => PathTagTranslator.GetFolderPath(PathTag.UserDataFolderInsideExeFolder);

    public record SettingsData(GameMode gameMode);
    public record GameLibraryData(List<FileSchemas>);
}