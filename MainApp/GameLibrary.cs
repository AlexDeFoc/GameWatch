using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace MainApp;

public sealed class GameLibrary
{
    // Public properties
    public bool ContainsAnyGames() => Games.Count != 0;

    // Public methods
    /// <summary>
    /// Add a game with automatic working mode
    /// </summary>
    public void AddGame(string title, string exePath)
    {
        Games.Add(new GameEntry(title, exePath));
        SaveToDisk();
    }

    /// <summary>
    /// Add a game with manual working mode
    /// </summary>
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

    public bool ContainsAnyManualWorkingGames()
    {
        bool foundManualWorkingGame = false;
        foreach (var game in Games)
        {
            if (game.CurrentWorkingMode == GameEntry.WorkingMode.Manual)
            {
                foundManualWorkingGame = true;
                break;
            }
        }

        return foundManualWorkingGame;
    }

    public bool AreAllManualWorkingGamesActive()
    {
        bool statement = true;

        foreach (var game in Games)
        {
            if (game is { CurrentWorkingMode: GameEntry.WorkingMode.Manual, ManualWorkingGameIsActive: false })
            {
                statement = false;
                break;
            }
        }

        return statement;
    }

    public bool IsAnyManualWorkingGameActive()
    {
        bool statement = false;

        foreach (var game in Games)
        {
            if (game is { CurrentWorkingMode: GameEntry.WorkingMode.Manual, ManualWorkingGameIsActive: true })
            {
                statement = true;
                break;
            }
        }

        return statement;
    }

    public bool ContainsMultipleManualWorkingActiveGames()
    {
        bool statement = false;
        int count = 0;

        foreach (var game in Games)
        {
            if (game is { CurrentWorkingMode: GameEntry.WorkingMode.Manual, ManualWorkingGameIsActive: true })
            {
                ++count;

                if (count > 1)
                {
                    statement = true;
                    break;
                }
            }
        }

        return statement;
    }

    public string GetSingleActiveManualWorkingGameTitle()
    {
        int gameIndex = 0;

        foreach (var game in Games)
        {
            if (game is { CurrentWorkingMode: GameEntry.WorkingMode.Manual, ManualWorkingGameIsActive: true })
                break;

            ++gameIndex;
        }

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

    public List<GameEntry> GetManualWorkingGames()
    {
        List<GameEntry> games = [];

        foreach (var game in Games)
        {
            if (game.CurrentWorkingMode == GameEntry.WorkingMode.Manual)
                games.Add(game);
        }

        return games;
    }

    public List<GameEntry> GetActiveManualWorkingGames()
    {
        List<GameEntry> games = [];

        foreach (var game in Games)
        {
            if (game is {CurrentWorkingMode: GameEntry.WorkingMode.Manual, ManualWorkingGameIsActive: true})
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
        GameEntry? targetGame = null;

        foreach (var game in Games)
        {
            if (game is { CurrentWorkingMode: GameEntry.WorkingMode.Manual, ManualWorkingGameIsActive: true })
            {
                targetGame = game;
                break;
            }
        }

        if (targetGame is not null)
            OnGameStopped(targetGame);
    }

    // Constructor
    public GameLibrary(AppContext ctx)
    {
        _languageManager = ctx.LanguageManager;
        _logger = ctx.Logger;
        ctx.AppState.AppRunningStatusChanged += OnAppRunningStatusChanged;

        // Note: Order doesn't matter here
        _filePaths = new Dictionary<FileExistenceOrder, Utils.FilePath>
        {
            [FileExistenceOrder.V2] = new(location: Utils.FileLocation.LocalAppDataFolder, fileName: "GameLibrary.json"),
            [FileExistenceOrder.V1] = new(location: Utils.FileLocation.ExeFolder, fileName: "games_library.json")
        };

        StartMonitoring();
        LoadFromDisk();
    }

    // Private variables
    private List<GameEntry> Games { get; set; } = [];
    private readonly Dictionary<FileExistenceOrder, Utils.FilePath> _filePaths;
    private readonly JsonSerializerOptions _fileJsonSerializerOpts = new() { WriteIndented = true };
    private Timer? _monitorTimer;
    private bool _monitorIsWorking;
    private readonly Lock _monitorLock = new();
    private const int MonitorIntervalMs = 5000; // 5 seconds
    private readonly Logger _logger;
    private readonly LanguageManager _languageManager;

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
            [FileSchemaV2.GamePlaytimePropertyName] = game.PlayTime.ToString(FileSchemaV2.GamePlaytimePropertyValueFormat),
            [FileSchemaV2.GameWorkingModePropertyName] = game.CurrentWorkingMode.ToString(),
            [FileSchemaV2.GameExePathModePropertyName] = game.ExePath
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

    private void OnAppRunningStatusChanged(object? sender, bool newState)
    {
        if (newState == false)
        {
            // 1. Finalize all active automatic games
            FinalizeActiveGames();

            // 2. Stop the timer
            StopMonitoring();
        }
    }

    private void FinalizeActiveGames()
    {
        // Take a snapshot to avoid modification during iteration
        List<GameEntry> snapshot;
        lock (_monitorLock)
        {
            snapshot = Games.ToList();
        }

        foreach (var game in snapshot.Where(game => game is { CurrentWorkingMode: GameEntry.WorkingMode.Automatic, ProcessIsActive: true }))
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
            List<GameEntry> snapshot;
            lock (_monitorLock)
            {
                snapshot = Games.ToList();
            }

            foreach (var game in snapshot.Where(game => game.CurrentWorkingMode == GameEntry.WorkingMode.Automatic))
            {
                if (game.ProcessIsActive)
                {
                    if (!ProcessHelper.IsProcessMatching(game.ExePath, game.Pid, game.ProcessCreationTime))
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
                    var found = ProcessHelper.FindProcessByExePath(game.ExePath);
                    if (found.HasValue)
                    {
                        game.ProcessIsActive = true;
                        game.Pid = found.Value.Pid;
                        game.ProcessCreationTime = found.Value.CreationTime;
                        game.SessionStartTime = DateTime.Now;
                    }
                }
            }
        }
        catch (Exception e)
        {
            // Log the error and continue – do NOT re‑throw
            _logger.WriteLine(Logger.Label.Error, _languageManager.Strings.GameLibrary.GameMonitorException(e.Message));
        }
        finally
        {
            _monitorIsWorking = false;
        }
    }

    // These will be called when a game starts/stops.
    // They handle playtime calculation and saving.
    private void OnGameStopped(GameEntry game)
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

                result.Add(new GameEntry(title: title, playTime: playTimeFound));
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
        public const string GameWorkingModePropertyName = "working_mode";
        public const string GameExePathModePropertyName = "exe_path";
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

                if (!gameElem.TryGetProperty(GameWorkingModePropertyName, out var workingModeElem) || workingModeElem.ValueKind != JsonValueKind.String || workingModeElem.GetString() is not "Automatic" and not "Manual")
                    continue;

                if (!gameElem.TryGetProperty(GameExePathModePropertyName, out var exePathElem) || workingModeElem.ValueKind != JsonValueKind.String)
                    continue;

                var title = titleElem.GetString();
                if (title == null)
                    continue;

                var workingMode = workingModeElem.GetString() == "Automatic" ? GameEntry.WorkingMode.Automatic : GameEntry.WorkingMode.Manual;

                var exePath = exePathElem.GetString();
                if (exePath == null)
                    continue;

                result.Add(new GameEntry(title: title, playTimeFound, workingMode, exePath));
            }

            return result;
        }
    }
}