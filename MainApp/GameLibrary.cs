using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MainApp;

public sealed class GameLibrary
{
    public List<GameEntry> Games { get; private set; } = [];

    // Public methods
    public void AddGame(string title)
    {
        Games.Add(new GameEntry(title));
        SaveToDisk();
    }

    public void CreateGameLibraryBackup()
    {
        var backupFilePath = new Utils.FilePath(location: Utils.FileLocation.LocalAppDataFolder, fileName: "GameLibrary.bak.json");

        try
        {
            File.Copy(_filePaths[FileExistenceOrder.V2].RealPath, backupFilePath.RealPath, true);
        }
        catch
        {
            // ignore
        }
    }

    public void ResetAllGames()
    {
        foreach (var game in Games)
            game.ResetPlaytime();

        SaveToDisk();
    }

    public void DeleteAllGames()
    {
        Games.Clear();
        SaveToDisk();
    }

    // Constructor
    public GameLibrary()
    {
        // Note: Order doesn't matter here
        _filePaths = new Dictionary<FileExistenceOrder, Utils.FilePath>
        {
            [FileExistenceOrder.V2] = new(location: Utils.FileLocation.LocalAppDataFolder, fileName: "GameLibrary.json"),
            [FileExistenceOrder.V1] = new(location: Utils.FileLocation.ExeFolder, fileName: "games_library.json")
        };

        LoadFromDisk();
    }

    // Private variables
    private readonly Dictionary<FileExistenceOrder, Utils.FilePath> _filePaths;
    private readonly JsonSerializerOptions _fileJsonSerializerOpts = new() { WriteIndented = true };

    // Private methods
    private void LoadFromDisk()
    {
        var foundFilesPaths = _filePaths.Where(file => file.Value.Exists).OrderBy(file => file.Key).ToDictionary(file => file.Key, file => file.Value);

        if (foundFilesPaths.Count == 0)
            return;

        var chosenFilePath = foundFilesPaths.First().Value;

        string fileContents = File.ReadAllText(chosenFilePath.RealPath);

        List<GameEntry>? loadedGames = null;

        try
        {
            using var doc = JsonDocument.Parse(fileContents); // may throw if completely invalid JSON
            var jsonDocRoot = doc.RootElement;

            int? foundFileVersion = LoadFileVersion(jsonDocRoot);
            loadedGames = foundFileVersion switch
            {
                FileSchemaV1.FileVersion => FileSchemaV1.LoadGames(jsonDocRoot),
                FileSchemaV2.FileVersion => FileSchemaV2.LoadGames(jsonDocRoot),
                _ => null
            };
        }
        catch
        {
            // ignore, consider all fields invalid
        }

        if (loadedGames is not null)
            Games = loadedGames;

        foreach (var filePath in foundFilesPaths.Values)
        {
            try
            {
                File.Delete(filePath.RealPath);
            }
            catch
            {
                // ignore
            }
        }

        SaveToDisk();
    }

    private void SaveToDisk()
    {
        var gamesCollectionSchemaPart = Games.Select(game => new Dictionary<string, object>
        {
            [FileSchemaV2.GameTitlePropertyName] = game.Title,
            [FileSchemaV2.GamePlaytimePropertyName] = game.PlayTime.ToString(FileSchemaV2.GamePlaytimePropertyValueFormat)
        }).ToList();

        var fileSchema = new Dictionary<string, object>
        {
            [FileVersionPropertyName.Type1] = FileSchemaV2.FileVersion,
            [FileSchemaV2.GamesCollectionPropertyName] = gamesCollectionSchemaPart
        };

        var jsonString = JsonSerializer.Serialize(fileSchema, _fileJsonSerializerOpts);
        File.WriteAllText(_filePaths[FileExistenceOrder.V2].RealPath, jsonString);
    }

    private static int? LoadFileVersion(JsonElement jsonDocRoot)
    {
        if (!jsonDocRoot.TryGetProperty(FileVersionPropertyName.Type1, out var verType1Elem)) return null;

        if (verType1Elem.ValueKind is not JsonValueKind.Number || !verType1Elem.TryGetInt32(out var verFound)) return null;

        if (verFound is FileSchemaV1.FileVersion or FileSchemaV2.FileVersion)
            return verFound;

        return null;
    }

    // Private structures
    // NOTE: Keep in descending order
    private enum FileExistenceOrder
    {
        V2,
        V1
    }

    private static class FileVersionPropertyName
    {
        public const string Type1 = "file_version";
    }

    private static class FileSchemaV1
    {
        public const int FileVersion = 1;
        private const string GamesCollectionPropertyName = "games";
        private const string GameTitlePropertyName = "title";
        private const string GamePlaytimePropertyName = "playtime";

        public static List<GameEntry>? LoadGames(JsonElement jsonDocRoot)
        {
            if (!jsonDocRoot.TryGetProperty(GamesCollectionPropertyName, out var gamesArray) || gamesArray.ValueKind != JsonValueKind.Array)
                return null;

            var result = new List<GameEntry>();
            foreach (var gameElem in gamesArray.EnumerateArray())
            {
                if (!gameElem.TryGetProperty(GameTitlePropertyName, out var titleElem) || titleElem.ValueKind != JsonValueKind.String)
                    continue;

                if (!gameElem.TryGetProperty(GamePlaytimePropertyName, out var playtimeElem) || playtimeElem.ValueKind != JsonValueKind.Number || !playtimeElem.TryGetInt32(out int playtimeInSeconds))
                    continue;

                var playTimeFound = TimeSpan.FromSeconds(playtimeInSeconds);

                var title = titleElem.GetString();
                if (title == null)
                    continue;

                result.Add(new GameEntry(title: title, playTimeFound));
            }

            return result;
        }
    }

    // NOTE: Latest file schema
    private record struct FileSchemaV2
    {
        public const int FileVersion = 2;
        public const string GamesCollectionPropertyName = "games";
        public const string GameTitlePropertyName = "title";
        public const string GamePlaytimePropertyName = "playtime";
        public const string GamePlaytimePropertyValueFormat = @"d\.hh\:mm\:ss";

        public static List<GameEntry>? LoadGames(JsonElement jsonDocRoot)
        {
            if (!jsonDocRoot.TryGetProperty(GamesCollectionPropertyName, out var gamesArray) || gamesArray.ValueKind != JsonValueKind.Array)
                return null;

            var result = new List<GameEntry>();
            foreach (var gameElem in gamesArray.EnumerateArray())
            {
                if (!gameElem.TryGetProperty(GameTitlePropertyName, out var titleElem) || titleElem.ValueKind != JsonValueKind.String)
                    continue;

                if (!gameElem.TryGetProperty(GamePlaytimePropertyName, out var playtimeElem) || playtimeElem.ValueKind != JsonValueKind.String)
                    continue;

                if (!TimeSpan.TryParseExact(playtimeElem.GetString(), GamePlaytimePropertyValueFormat, null, out var playTimeFound))
                    continue;

                var title = titleElem.GetString();
                if (title == null)
                    continue;

                result.Add(new GameEntry(title: title, playTimeFound));
            }

            return result;
        }
    }
}