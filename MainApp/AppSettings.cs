using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MainApp;

public sealed class AppSettings
{
    public event EventHandler<LanguageManager.LanguageCode>? LanguageChanged;

    // Note: Single-threaded reader & writer
    public LanguageManager.LanguageCode LanguageCode
    {
        get => _activeLanguageCode;
        private set
        {
            if (_activeLanguageCode == value) return;
            _activeLanguageCode = value;
            OnLanguageChanged(value);
        }
    }

    // Note: Single-threaded writer, Multi-threaded reader
    public int AutoSaveIntervalInMinutes
    {
        get => _autoSaveIntervalInMinutes;
        set
        {
            _autoSaveIntervalInMinutes = value;
            SaveToDisk();
        }
    }

    // Note: Single-threaded writer, Multi-threaded reader
    public bool IsAutoSaveEnabled() => _autoSaveEnabledStatus;

    // Note: Single-threaded writer, Multi-threaded reader
    public void ToggleAutoSaveStatus()
    {
        _autoSaveEnabledStatus = !_autoSaveEnabledStatus;
        SaveToDisk();
    }

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
                    _autoSaveEnabledStatus = Defaults.AutoSaveEnabledStatus;

                    // Note: Forcefully migrate with default; Reason: the app in newer version, auto elapses game playtime whenever the game is active & stops when its inactive
                    _autoSaveIntervalInMinutes = Defaults.AutoSaveIntervalInMinutes;

                    break;
                }

                case FileSchemaV2.FileVersion:
                {
                    _autoSaveEnabledStatus = FileSchemaV2.LoadAutoSaveEnabledStatusProperty(jsonDocRoot) ?? _autoSaveEnabledStatus;

                    _autoSaveIntervalInMinutes = FileSchemaV2.LoadAutoSaveIntervalInMinutesProperty(jsonDocRoot) ?? _autoSaveIntervalInMinutes;

                    _activeLanguageCode = FileSchemaV2.LoadActiveLanguageCodeProperty(jsonDocRoot) ?? _activeLanguageCode;

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
            File.Delete(filePath.RealPath);
        }

        SaveToDisk();
    }

    private void SaveToDisk()
    {
        var fileSchema = new Dictionary<string, object>
        {
            [FileVersionPropertyName.Type1] = FileSchemaV2.FileVersion,
            // ReSharper disable once RedundantTernaryExpression
            [FileSchemaV2.AutoSaveEnabledStatusPropertyName] = _autoSaveEnabledStatus,
            [FileSchemaV2.AutoSaveIntervalInMinutesPropertyName] = _autoSaveIntervalInMinutes,
            [FileSchemaV2.ActiveLanguageCodePropertyName] = LanguageCode.ToString()
        };

        var jsonString = JsonSerializer.Serialize(fileSchema, _fileJsonSerializerOpts);
        File.WriteAllText(_filePaths[FileExistenceOrder.V2].RealPath, jsonString);
    }

    private void LoadDefaults()
    {
        LanguageCode = Defaults.LanguageCode;
        _autoSaveEnabledStatus = Defaults.AutoSaveEnabledStatus;
        _autoSaveIntervalInMinutes = Defaults.AutoSaveIntervalInMinutes;
    }

    // Load from disk Utility methods
    private static int? LoadFileVersion(JsonElement jsonDocRoot)
    {
        if (!jsonDocRoot.TryGetProperty(FileVersionPropertyName.Type1, out var verType1Elem)) return null;

        if (verType1Elem.ValueKind is not JsonValueKind.Number || !verType1Elem.TryGetInt32(out var verFound)) return null;

        if (verFound is FileSchemaV1.FileVersion or FileSchemaV2.FileVersion)
            return verFound;

        return null;
    }

    private volatile bool _autoSaveEnabledStatus;
    private volatile int _autoSaveIntervalInMinutes;
    private LanguageManager.LanguageCode _activeLanguageCode;
    private readonly Dictionary<FileExistenceOrder, Utils.FilePath> _filePaths;
    private readonly JsonSerializerOptions _fileJsonSerializerOpts = new() { WriteIndented = true };

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
        public const string AutoSaveEnabledStatusPropertyName = "auto_save_enabled_status";
        public const string AutoSaveIntervalInMinutesPropertyName = "auto_save_interval_in_minutes";
        public const string ActiveLanguageCodePropertyName = "active_language_code";

        public static bool? LoadAutoSaveEnabledStatusProperty(JsonElement jsonDocRoot)
        {
            if (!jsonDocRoot.TryGetProperty(AutoSaveEnabledStatusPropertyName, out var autoSaveEnabledStatusElem)) return null;

            if (autoSaveEnabledStatusElem.ValueKind is JsonValueKind.True or JsonValueKind.False)
                return autoSaveEnabledStatusElem.GetBoolean();

            return null;
        }

        public static int? LoadAutoSaveIntervalInMinutesProperty(JsonElement jsonDocRoot)
        {
            if (!jsonDocRoot.TryGetProperty(AutoSaveIntervalInMinutesPropertyName, out var autoSaveIntervalInMinutesElem)) return null;

            if (autoSaveIntervalInMinutesElem.ValueKind is JsonValueKind.Number && autoSaveIntervalInMinutesElem.TryGetInt32(out var autoSaveIntervalInMinutesFound))
                return autoSaveIntervalInMinutesFound;

            return null;
        }

        public static LanguageManager.LanguageCode? LoadActiveLanguageCodeProperty(JsonElement jsonDocRoot)
        {
            if (!jsonDocRoot.TryGetProperty(ActiveLanguageCodePropertyName, out var activeLanguageCodeElem)) return null;

            if (activeLanguageCodeElem.ValueKind is not JsonValueKind.String) return null;

            if (Enum.TryParse(activeLanguageCodeElem.GetString(), out LanguageManager.LanguageCode parsedCode))
                return parsedCode;

            return null;
        }
    }
}