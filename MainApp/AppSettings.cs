using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MainApp;

public sealed class AppSettings
{
    // Public variables
    public event EventHandler<LanguageManager.LanguageCode>? LanguageChanged;

    // Public fields
    // Note: Single-threaded reader & writer
    public LanguageManager.LanguageCode ActiveAppLanguageCode
    {
        get => _activeActiveAppLanguageCode;
        private set
        {
            if (_activeActiveAppLanguageCode == value) return;
            _activeActiveAppLanguageCode = value;
            OnLanguageChanged(value);
        }
    }

    // Note: Single-threaded writer, Multi-threaded reader
    public int GameAutoSaveIntervalInMinutes
    {
        get => _gameAutoSaveIntervalInMinutes;
        set
        {
            _gameAutoSaveIntervalInMinutes = value;
            SaveToDisk();
        }
    }

    // Public methods
    // Note: Single-threaded writer, Multi-threaded reader
    public bool IsGameAutoSaveEnabled() => _gameAutoSaveEnabledStatus;

    // Note: Single-threaded writer, Multi-threaded reader
    public void ToggleGameAutoSaveStatus()
    {
        _gameAutoSaveEnabledStatus = !_gameAutoSaveEnabledStatus;
        SaveToDisk();
    }

    public string GetPrintableGameAutoSaveInterval()
    {
        var parts = new List<string>();
        var playTime = TimeSpan.FromMinutes(GameAutoSaveIntervalInMinutes);

        if (playTime.Days > 0)
            parts.Add($"{playTime.Days} day{(playTime.Days > 1 ? "s" : "")}");

        if (playTime.Hours > 0)
            parts.Add($"{playTime.Hours} h");

        if (playTime.Minutes > 0)
            parts.Add($"{playTime.Minutes} min");

        return string.Join(" : ", parts);
    }

    public void ResetAllToDefault()
    {
        LoadDefaults();
        SaveToDisk();
    }

    // Constructor
    public AppSettings()
    {
        // Note: Order doesn't matter here
        _filePaths = new Dictionary<FileExistenceOrder, Utils.FilePath>
        {
            [FileExistenceOrder.V2] = new(location: Utils.FileLocation.LocalAppDataFolder, fileName: "Settings.json"),
            [FileExistenceOrder.V1] = new(location: Utils.FileLocation.ExeFolder, fileName: "settings.json")
        };

        LoadFromDisk();
    }

    // Private variables
    private volatile bool _gameAutoSaveEnabledStatus;
    private volatile int _gameAutoSaveIntervalInMinutes;
    private LanguageManager.LanguageCode _activeActiveAppLanguageCode;
    private readonly Dictionary<FileExistenceOrder, Utils.FilePath> _filePaths;
    private readonly JsonSerializerOptions _fileJsonSerializerOpts = new() { WriteIndented = true };

    // Private methods
    private void OnLanguageChanged(LanguageManager.LanguageCode newLang)
    {
        LanguageChanged?.Invoke(this, newLang);
    }

    private void LoadFromDisk()
    {
        var foundFilesPaths = _filePaths.Where(file => file.Value.Exists).OrderBy(file => file.Key).ToDictionary(file => file.Key, file => file.Value);

        if (foundFilesPaths.Count == 0)
        {
            LoadDefaults();
            SaveToDisk();
            return;
        }

        var chosenFilePath = foundFilesPaths.First().Value;

        string fileContents = File.ReadAllText(chosenFilePath.RealPath);

        try
        {
            using var doc = JsonDocument.Parse(fileContents); // may throw if completely invalid JSON
            var jsonDocRoot = doc.RootElement;

            var foundFileVersion = LoadFileVersion(jsonDocRoot) ?? 0;

            switch (foundFileVersion)
            {
                case FileSchemaV1.FileVersion:
                {
                    // Note: Forcefully migrate with default; Reason: the app in newer version, auto elapses game playtime whenever the game is active & stops when its inactive
                    _gameAutoSaveEnabledStatus = Defaults.AutoSaveEnabledStatus;

                    // Note: Forcefully migrate with default; Reason: the app in newer version, auto elapses game playtime whenever the game is active & stops when its inactive
                    _gameAutoSaveIntervalInMinutes = Defaults.AutoSaveIntervalInMinutes;

                    break;
                }

                case FileSchemaV2.FileVersion:
                {
                    _gameAutoSaveEnabledStatus = FileSchemaV2.LoadGameAutoSaveEnabledStatusProperty(jsonDocRoot) ?? _gameAutoSaveEnabledStatus;

                    _gameAutoSaveIntervalInMinutes = FileSchemaV2.LoadGameAutoSaveIntervalInMinutesProperty(jsonDocRoot) ?? _gameAutoSaveIntervalInMinutes;

                    _activeActiveAppLanguageCode = FileSchemaV2.LoadActiveAppLanguageCodeProperty(jsonDocRoot) ?? _activeActiveAppLanguageCode;

                    break;
                }
            }
        }
        catch
        {
            // ignore, consider all fields invalid
        }

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
        var fileSchema = new Dictionary<string, object>
        {
            [FileVersionPropertyName.Type1] = FileSchemaV2.FileVersion,
            [FileSchemaV2.GameAutoSaveEnabledStatusPropertyName] = _gameAutoSaveEnabledStatus,
            [FileSchemaV2.GameAutoSaveIntervalInMinutesPropertyName] = _gameAutoSaveIntervalInMinutes,
            [FileSchemaV2.ActiveAppLanguageCodePropertyName] = ActiveAppLanguageCode.ToString()
        };

        var jsonString = JsonSerializer.Serialize(fileSchema, _fileJsonSerializerOpts);
        File.WriteAllText(_filePaths[FileExistenceOrder.V2].RealPath, jsonString);
    }

    private void LoadDefaults()
    {
        ActiveAppLanguageCode = Defaults.LanguageCode;
        _gameAutoSaveEnabledStatus = Defaults.AutoSaveEnabledStatus;
        _gameAutoSaveIntervalInMinutes = Defaults.AutoSaveIntervalInMinutes;
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
    private static class Defaults
    {
        public static LanguageManager.LanguageCode LanguageCode { get; } = LanguageManager.LanguageCode.en_US;
        public static bool AutoSaveEnabledStatus { get; } = true;
        public static int AutoSaveIntervalInMinutes { get; } = 1;
    }

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
    }

    // NOTE: Latest file schema
    private record struct FileSchemaV2
    {
        public const int FileVersion = 2;
        public const string GameAutoSaveEnabledStatusPropertyName = "game_auto_save_enabled_status";
        public const string GameAutoSaveIntervalInMinutesPropertyName = "game_auto_save_interval_in_minutes";
        public const string ActiveAppLanguageCodePropertyName = "active_app_language_code";

        public static bool? LoadGameAutoSaveEnabledStatusProperty(JsonElement jsonDocRoot)
        {
            if (!jsonDocRoot.TryGetProperty(GameAutoSaveEnabledStatusPropertyName, out var autoSaveEnabledStatusElem)) return null;

            if (autoSaveEnabledStatusElem.ValueKind is JsonValueKind.True or JsonValueKind.False)
                return autoSaveEnabledStatusElem.GetBoolean();

            return null;
        }

        public static int? LoadGameAutoSaveIntervalInMinutesProperty(JsonElement jsonDocRoot)
        {
            if (!jsonDocRoot.TryGetProperty(GameAutoSaveIntervalInMinutesPropertyName, out var autoSaveIntervalInMinutesElem)) return null;

            if (autoSaveIntervalInMinutesElem.ValueKind is JsonValueKind.Number && autoSaveIntervalInMinutesElem.TryGetInt32(out var autoSaveIntervalInMinutesFound))
                return autoSaveIntervalInMinutesFound;

            return null;
        }

        public static LanguageManager.LanguageCode? LoadActiveAppLanguageCodeProperty(JsonElement jsonDocRoot)
        {
            if (!jsonDocRoot.TryGetProperty(ActiveAppLanguageCodePropertyName, out var activeLanguageCodeElem)) return null;

            if (activeLanguageCodeElem.ValueKind is not JsonValueKind.String) return null;

            if (Enum.TryParse(activeLanguageCodeElem.GetString(), out LanguageManager.LanguageCode parsedCode))
                return parsedCode;

            return null;
        }
    }
}