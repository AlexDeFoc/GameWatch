using GameWatch.Tui.App.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace GameWatch.Tui.App;

public sealed class GameLibrary
{
    private readonly JsonSerializerOptions _fileJsonStyle = new() { WriteIndented = true };
    private FilePath _currentFilePath;
    private Timer? _monitorTimer;
    private bool _monitorIsWorking;
    private readonly Lock _monitorLock = new();
    private const int MonitorIntervalMs = 5000; // 5 seconds

    public GameLibrary(AppContext appCtx)
    {
        appCtx.AppState.AppRunningStatusChanged += OnAppRunningStatusChanged;

        _currentFilePath = new(FolderPath.LocationCode.OurUserDataDirectory) { BaseName = "GameLibrary", Extension = "json" };

        // order doesn't matter
        var filePaths = new Dictionary<FileExistenceOrder, FilePath>
        {
            [FileExistenceOrder.V2] = _currentFilePath,
            [FileExistenceOrder.V1] = new(FolderPath.LocationCode.BinaryDirectory) { BaseName = "game_library", Extension = "json" }
        };

        LoadFromDisk(filePaths);
        StartMonitoring();
    }

    private List<Game> Games { get; set; } = [];

    private static int? GetFileVersion(JsonElement root)
    {
        if (root.TryGetProperty(FileVersionPropertyName.Type1, out var verType1Elem))
        {
            if (verType1Elem.ValueKind is not JsonValueKind.Number && !verType1Elem.TryGetInt32(out var verFound))
            {
                if (verFound is FileSchemaV1.FileVersion or FileSchemaV2.FileVersion)
                    return verFound;
            }
        }
        else if (root.TryGetProperty(FileVersionPropertyName.Type2, out var verType2Elem))
        {
            if (verType2Elem.ValueKind is not JsonValueKind.Number && !verType2Elem.TryGetInt32(out var verFound))
            {
                if (verFound is FileSchemaV1.FileVersion or FileSchemaV2.FileVersion)
                    return verFound;
            }
        }

        return null;
    }

    private void OnAppRunningStatusChanged(AffirmationStatus isAppStillRunning)
    {
        if (isAppStillRunning == AffirmationStatus.No)
        {
            // 1. Finalize all active automatic games
            FinalizeActiveGames();

            // 2. Stop the timer
            StopMonitoring();
        }
    }

    private void LoadFromDisk(Dictionary<FileExistenceOrder, FilePath> filePaths)
    {
        var existingFiles = filePaths.Where(f => f.Value.Exists()).OrderBy(f => f.Key).ToDictionary(f => f.Key, f => f.Value);

        if (existingFiles.Count == 0)
            return;

        var chosenFile = existingFiles.First().Value;
        _currentFilePath = chosenFile;
        var fileContents = File.ReadAllText(chosenFile.Path);

        List<Game>? loadedGames = null;

        try
        {
            using var doc = JsonDocument.Parse(fileContents);
            var jsonDocRoot = doc.RootElement;
            var fileVer = GetFileVersion(doc.RootElement);

            switch (fileVer)
            {
                case FileSchemaV1.FileVersion:
                    loadedGames = FileSchemaV1.LoadGames(jsonDocRoot);
                    break;

                case FileSchemaV2.FileVersion:
                    loadedGames = FileSchemaV2.LoadGames(jsonDocRoot);
                    break;
            }
        }
        catch
        {
            // ignore, considering all fields invalid
        }

        if (loadedGames is not null)
            Games = loadedGames;

        foreach (var filePath in existingFiles.Values)
        {
            try
            {
                File.Delete(filePath.Path);
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
        var gamesArray = Games.Select(game => new Dictionary<string, object>
        {
            [FileSchemaV2.GameTitlePropertyName] = game.Title,
            [FileSchemaV2.GamePlayTimePropertyName] = game.PlayTime.ToString(FileSchemaV2.GamePlayTimePropertyValueFormat),
            [FileSchemaV2.GameWorkingModePropertyName] = game.WorkingMode.ToString(),
            [FileSchemaV2.GameFilePathModePropertyName] = game.Title
        }).ToList();

        var fileSchema = new Dictionary<string, object>
        {
            [FileVersionPropertyName.Type2] = FileSchemaV2.FileVersion,
            [FileSchemaV2.GamesArrayPropertyName] = gamesArray
        };

        var jsonString = JsonSerializer.Serialize(fileSchema, _fileJsonStyle);
        Directory.CreateDirectory(_currentFilePath.ParentPath);

        File.WriteAllText(_currentFilePath.Path, jsonString);
    }

    private void StartMonitoring()
    {
        if (_monitorTimer != null)
            return; // already running

        _monitorTimer = new Timer(MonitorTick, null, 0, MonitorIntervalMs);
    }

    private void StopMonitoring()
    {
        _monitorTimer?.Dispose();
        _monitorTimer = null;
    }

    private void MonitorTick(object? state)
    {
        if (_monitorIsWorking)
            return;

        _monitorIsWorking = true;

        try
        {
            // Snapshot list to safely iterate while UI may modify it
            List<Game> snapshot;
            lock(_monitorLock)
            {
                snapshot = Games.ToList();
            }

            foreach(var game in snapshot.Where(game => game.WorkingMode == Game.WorkingModeType.Automatic))
            {
                if(game.ProcessIsActive)
                {
                    if (!ProcessHelper.IsProcessMatching(game.FilePath, game.Pid, game.ProcessCreationTime))
                    {
                        // Game stopped
                        game.ProcessIsActive = false;
                        game.Pid = 0;
                        game.ProcessCreationTime = default;
                        OnGameStopped(game);
                    }
                }
                else
                {
                    var found = ProcessHelper.FindProcessByExePath(game.FilePath);
                    if(found.HasValue)
                    {
                        game.ProcessIsActive = true;
                        game.Pid = found.Value.Pid;
                        game.ProcessCreationTime = found.Value.CreationTime;
                        game.SessionStartTime = DateTime.Now;
                    }
                }
            }
        }
        catch
        {
            // ignore...
        }
        finally
        {
            _monitorIsWorking = false;
        }
    }

    /// <summary>
    /// <para>This will be called when a game starts/stops.</para>
    /// <para>Handles playtime calculation and saving.</para>
    /// </summary>
    private void OnGameStopped(Game game)
    {
        // 1. Calculate elapsed time since last start
        if (game.SessionStartTime.HasValue)
        {
            var sessionLength = DateTime.Now - game.SessionStartTime.Value;
            game.AddPlaytime(sessionLength);
            game.SessionStartTime = null;
            game.ManualWorkingGameIsActive = false;
        }

        // 2. Save to disk immediately
        SaveToDisk();
    }

    private void FinalizeActiveGames()
    {
        // Take a snapshot to avoid modification during iteration
        List<Game> snapshot;
        lock (_monitorLock)
        {
            snapshot = Games.ToList();
        }

        foreach (var game in snapshot.Where(game => game is { WorkingMode: Game.WorkingModeType.Automatic, ProcessIsActive: true }))
        {
            // Simulate a clean stop
            game.ProcessIsActive = false;
            game.Pid = 0;
            game.ProcessCreationTime = default;

            if (game.SessionStartTime.HasValue)
            {
                var sessionLength = DateTime.Now - game.SessionStartTime.Value;
                game.AddPlaytime(sessionLength);
                game.SessionStartTime = null;
            }
        }

        // 3. Persist to disk
        SaveToDisk();
    }

    private enum FileExistenceOrder { V2, V1 }

    private static class FileVersionPropertyName
    {
        public const string Type2 = "fileVersion";
        public const string Type1 = "file_version";
    }

    private record struct FileSchemaV2
    {
        public const int FileVersion = 2;
        public const string GamesArrayPropertyName = "games";
        public const string GameTitlePropertyName = "title";
        public const string GamePlayTimePropertyName = "playTime";
        public const string GameWorkingModePropertyName = "workingMode";
        public const string GamePlayTimePropertyValueFormat = @"d\.hh\:mm\:ss";
        public const string GameFilePathModePropertyName = "gameFilePath";

        public static List<Game>? LoadGames(JsonElement jsonDocRoot)
        {
            if (!jsonDocRoot.TryGetProperty(GamesArrayPropertyName, out var gamesArray) || gamesArray.ValueKind != JsonValueKind.Array)
                return null;

            var result = new List<Game>();
            foreach (var gameElem in gamesArray.EnumerateArray())
            {
                if (!gameElem.TryGetProperty(GameTitlePropertyName, out var titleElem) || titleElem.ValueKind != JsonValueKind.String)
                    continue;

                if (!gameElem.TryGetProperty(GamePlayTimePropertyName, out var playtimeElem) || playtimeElem.ValueKind != JsonValueKind.String)
                    continue;

                if (!TimeSpan.TryParseExact(playtimeElem.GetString(), GamePlayTimePropertyValueFormat, null, out var playTimeFound))
                    continue;

                if (!gameElem.TryGetProperty(GameWorkingModePropertyName, out var workingModeElem) || workingModeElem.ValueKind != JsonValueKind.String || workingModeElem.GetString() is not "Automatic" and not "Manual")
                    continue;

                if (!gameElem.TryGetProperty(GameFilePathModePropertyName, out var gameFilePathElem) || workingModeElem.ValueKind != JsonValueKind.String)
                    continue;

                var title = titleElem.GetString();
                if (title == null)
                    continue;

                var workingMode = workingModeElem.GetString() == "Automatic" ? Game.WorkingModeType.Automatic : Game.WorkingModeType.Manual;

                var gameFilePath = gameFilePathElem.GetString();
                if (gameFilePath == null)
                    continue;

                result.Add(new Game(title, playTimeFound, workingMode, gameFilePath));
            }

            return result;
        }
    }

    private record struct FileSchemaV1
    {
        public const int FileVersion = 1;
        private const string GamesArrayPropertyName = "games";
        private const string GameTitlePropertyName = "title";
        private const string GamePlayTimePropertyName = "playtime";

        public static List<Game>? LoadGames(JsonElement root)
        {
            if (!root.TryGetProperty(GamesArrayPropertyName, out var gamesArray) || gamesArray.ValueKind != JsonValueKind.Array)
                return null;

            var result = new List<Game>();
            foreach (var gameElem in gamesArray.EnumerateArray())
            {
                if (!gameElem.TryGetProperty(GameTitlePropertyName, out var titleElem) || titleElem.ValueKind != JsonValueKind.String)
                    continue;

                if (!gameElem.TryGetProperty(GamePlayTimePropertyName, out var playTimeElem) || playTimeElem.ValueKind != JsonValueKind.Number || !playTimeElem.TryGetInt32(out int playTimeInSeconds))
                    continue;

                var playTimeFound = TimeSpan.FromSeconds(playTimeInSeconds);

                var title = titleElem.GetString();
                if (title == null)
                    continue;

                result.Add(new Game(title: title, playTime: playTimeFound));
            }

            return result;
        }
    }
}