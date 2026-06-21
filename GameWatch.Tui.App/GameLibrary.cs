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
    private const int MonitorIntervalMs = 1000; // 1 seconds

    private FilePath _currentFilePath;
    private Timer? _monitorTimer;
    private bool _monitorIsWorking;
    private readonly JsonSerializerOptions _fileJsonStyle = new() { WriteIndented = true };
    private readonly Lock _monitorLock = new();

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

    public void AddGame(string title, string gameFilePath)
    {
        Games.Add(new Game(title, gameFilePath));
        SaveToDisk();
    }

    public void AddGame(string title)
    {
        Games.Add(new Game(title));
        SaveToDisk();
    }

    public void CreateGameLibraryBackup()
    {
        var backupFilePath = new FilePath(FolderPath.LocationCode.OurUserDataDirectory)
        {
            BaseName = "GameLibrary",
            Extension = "bak.json"
        };

        try
        {
            File.Copy(_currentFilePath.Path, backupFilePath.Path, true);
        }
        catch
        {
            // NOTE: Could use some logging to user
            // ignore
        }
    }

    public void ResetAllGames()
    {
        foreach (var game in Games)
            game.ResetPlayTime();

        SaveToDisk();
    }

    /// <param name="gameId">1 indexed</param>
    public void ResetGame(int gameId)
    {
        Games[gameId - 1].ResetPlayTime();
        SaveToDisk();
    }

    public void DeleteAllGames()
    {
        Games.Clear();
        SaveToDisk();
    }

    /// <param name="gameId">1 indexed</param>
    public void DeleteGame(int gameId)
    {
        Games.RemoveAt(gameId - 1);
        SaveToDisk();
    }

    public bool ContainsAnyManualWorkingGames() => Games.Any(game => game.WorkingMode == Game.WorkingModeType.Manual);

    public bool AreAllManualWorkingGamesActive()
    {
        var statement = true;

        foreach (var game in Games)
        {
            if (game is not { WorkingMode: Game.WorkingModeType.Manual, ManualWorkingGameIsActive: false }) continue;
            statement = false;
            break;
        }

        return statement;
    }

    public bool IsAnyManualWorkingGameActive()
    {
        var statement = false;

        foreach (var game in Games)
        {
            if (game is not { WorkingMode: Game.WorkingModeType.Manual, ManualWorkingGameIsActive: true }) continue;
            statement = true;
            break;
        }

        return statement;
    }

    public bool ContainsMultipleManualWorkingActiveGames()
    {
        var statement = false;
        var count = 0;

        foreach (var game in Games)
        {
            if (game is not { WorkingMode: Game.WorkingModeType.Manual, ManualWorkingGameIsActive: true }) continue;
            ++count;

            if (count <= 1) continue;
            statement = true;
            break;
        }

        return statement;
    }

    public string GetSingleActiveManualWorkingGameTitle()
    {
        var gameIndex = Games.TakeWhile(game => game is not { WorkingMode: Game.WorkingModeType.Manual, ManualWorkingGameIsActive: true }).Count();

        return Games[gameIndex].Title;
    }

    /// <param name="gameId">1 indexed</param>
    public string GetActiveManualWorkingGameTitle(int gameId)
    {
        var games = GetActiveManualWorkingGames();
        return games[gameId - 1].Title;
    }

    /// <param name="gameId">1 indexed</param>
    public string GetManualWorkingGameTitle(int gameId)
    {
        var games = GetManualWorkingGames();
        return games[gameId - 1].Title;
    }

    public List<Game> GetManualWorkingGames()
    {
        List<Game> games = [];
        games.AddRange(Games.Where(game => game.WorkingMode == Game.WorkingModeType.Manual));

        return games;
    }

    public List<Game> GetActiveManualWorkingGames()
    {
        List<Game> games = [];

        foreach (var game in Games)
        {
            if (game is {WorkingMode: Game.WorkingModeType.Manual, ManualWorkingGameIsActive: true})
                games.Add(game);
        }

        return games;
    }

    /// <param name="gameId">1 indexed</param>
    public void StartManualWorkingGame(int gameId)
    {
        var games = GetManualWorkingGames();
        games[gameId - 1].ManualWorkingGameIsActive = true;
        games[gameId - 1].SessionStartTime = DateTime.Now;
    }

    /// <param name="gameId">1 indexed</param>
    public void StopManualWorkingGame(int gameId)
    {
        var games = GetActiveManualWorkingGames();
        OnGameStopped(games[gameId - 1]);
    }

    public void StopSingleManualWorkingActiveGame()
    {
        Game? targetGame = null;

        foreach (var game in Games)
        {
            if (game is not { WorkingMode: Game.WorkingModeType.Manual, ManualWorkingGameIsActive: true }) continue;
            targetGame = game;
            break;
        }

        if (targetGame is not null)
            OnGameStopped(targetGame);
    }

    /// <param name="gameId">1 indexed</param>
    public void ChangeGameTitle(int gameId, string newGameTitle)
    {
        Games[gameId - 1].Title = newGameTitle;
        SaveToDisk();
    }

    /// <param name="gameId">1 indexed</param>
    public string GetGameTitle(int gameId)
    {
        return Games[gameId - 1].Title;
    }

    /// <param name="gameId">1 indexed</param>
    public Game.WorkingModeType GetGameWorkingMode(int gameId)
    {
        return Games[gameId - 1].WorkingMode;
    }

    /// <param name="gameId">1 indexed</param>
    public void SetGameWorkingMode(int gameId, Game.WorkingModeType workingMode, string? exePath = null)
    {
        switch (workingMode)
        {
            case Game.WorkingModeType.Manual:
            {
                var target = Games[gameId - 1];

                target.WorkingMode = workingMode;
                var tmpValueOfGameActiveStatus = target.ProcessIsActive;
                target.ProcessIsActive = false;
                target.FilePath = "";
                if (tmpValueOfGameActiveStatus)
                    OnGameStopped(target);
                target.ManualWorkingGameIsActive = tmpValueOfGameActiveStatus;
                break;
            }
            case Game.WorkingModeType.Automatic:
            {
                var target = Games[gameId - 1];

                target.WorkingMode = workingMode;
                target.FilePath = exePath ?? "";
                OnGameStopped(target);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(workingMode), workingMode, null);
        }
    }

    private static int? GetFileVersion(JsonElement root)
    {
        if (root.TryGetProperty(FileVersionPropertyName.Type1, out var verType1Elem))
        {
            if (verType1Elem.ValueKind is JsonValueKind.Number || verType1Elem.TryGetInt32(out var verFound)) return null;
            if (verFound is FileSchemaV1.FileVersion or FileSchemaV2.FileVersion)
                return verFound;
        }
        else if (root.TryGetProperty(FileVersionPropertyName.Type2, out var verType2Elem))
        {
            if (verType2Elem.ValueKind is JsonValueKind.Number || verType2Elem.TryGetInt32(out var verFound)) return null;
            if (verFound is FileSchemaV1.FileVersion or FileSchemaV2.FileVersion)
                return verFound;
        }

        return null;
    }

    private void OnAppRunningStatusChanged()
    {
        // 1. Finalize all active automatic games
        FinalizeAllGames();

        // 2. Stop the timer
        StopMonitoring();
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
                    if (ProcessHelper.IsProcessMatching(game.FilePath, game.Pid, game.ProcessCreationTime)) continue;

                    // Game stopped
                    game.ProcessIsActive = false;
                    game.Pid = 0;
                    game.ProcessCreationTime = default;
                    OnGameStopped(game);
                }
                else
                {
                    var found = ProcessHelper.FindProcessByExePath(game.FilePath);
                    if (!found.HasValue) continue;

                    game.ProcessIsActive = true;
                    game.Pid = found.Value.Pid;
                    game.ProcessCreationTime = found.Value.CreationTime;
                    game.SessionStartTime = DateTime.Now;
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

    private void FinalizeAllGames()
    {
        // Take a snapshot to avoid modification during iteration
        List<Game> snapshot;
        lock (_monitorLock)
        {
            snapshot = Games.ToList();
        }

        foreach (var game in snapshot.Where(game => game is { ProcessIsActive: true }))
        {
            game.ProcessIsActive = false;
            game.Pid = 0;
            game.ProcessCreationTime = default;
            game.ManualWorkingGameIsActive = false;

            if (!game.SessionStartTime.HasValue) continue;
            var sessionLength = DateTime.Now - game.SessionStartTime.Value;
            game.AddPlaytime(sessionLength);
            game.SessionStartTime = null;
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

                if (!gameElem.TryGetProperty(GamePlayTimePropertyName, out var playTimeElem) || playTimeElem.ValueKind != JsonValueKind.Number || !playTimeElem.TryGetInt32(out var playTimeInSeconds))
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