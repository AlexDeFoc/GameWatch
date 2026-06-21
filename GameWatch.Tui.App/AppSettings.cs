using GameWatch.Tui.App.FileSystem;
using GameWatch.Tui.App.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace GameWatch.Tui.App;

public sealed class AppSettings
{
    private readonly JsonSerializerOptions _fileJsonStyle = new() { WriteIndented = true };
    private FilePath _currentFilePath;

    public AppSettings()
    {
        _currentFilePath = new FilePath(FolderPath.LocationCode.OurUserDataDirectory) { BaseName = "AppSettings", Extension = "json" };

        // order doesn't matter
        var filePaths = new Dictionary<FileExistenceOrder, FilePath>
        {
            [FileExistenceOrder.V2] = _currentFilePath,
            [FileExistenceOrder.V1] = new(FolderPath.LocationCode.BinaryDirectory) { BaseName = "settings", Extension = "json" }
        };

        LoadFromDisk(filePaths);
    }

    public EnabledStatus AutoSaveGamesStatus { get; private set; }

    public int AutoSaveGamesIntervalInMinutes { get; private set; }

    public LanguageManager.LanguageTag ActiveAppLanguageTag
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            LanguageChanged?.Invoke(value);
        }
    }

    public event Action<LanguageManager.LanguageTag>? LanguageChanged;

    // Note: Single-threaded writer, Multi-threaded reader
    public bool IsGameAutoSaveEnabled() => AutoSaveGamesStatus == EnabledStatus.Enabled;

    // Note: Single-threaded writer, Multi-threaded reader
    public void ToggleGameAutoSave()
    {
        AutoSaveGamesStatus = AutoSaveGamesStatus == EnabledStatus.Enabled ? EnabledStatus.Disabled : EnabledStatus.Enabled;
        SaveToDisk();
    }

    public void ResetAllToDefault()
    {
        LoadDefaults();
        SaveToDisk();
    }

    public string GetPrintableGameAutoSaveInterval()
    {
        var parts = new List<string>();
        var playTime = TimeSpan.FromMinutes(AutoSaveGamesIntervalInMinutes);

        if (playTime.Days > 0)
            parts.Add($"{playTime.Days} day{(playTime.Days > 1 ? "s" : "")}");

        if (playTime.Hours > 0)
            parts.Add($"{playTime.Hours} h");

        if (playTime.Minutes > 0)
            parts.Add($"{playTime.Minutes} min");

        return string.Join(" : ", parts);
    }

    private static int? GetFileVersion(JsonElement root)
    {
        var properties = new[] { FileVersionPropertyName.Type1, FileVersionPropertyName.Type2 };

        foreach (var prop in properties)
        {
            if (root.TryGetProperty(prop, out var elem)
                && elem.ValueKind == JsonValueKind.Number
                && elem.TryGetInt32(out var ver)
                && ver is FileSchemaV1.FileVersion or FileSchemaV2.FileVersion)
            {
                return ver;
            }
        }

        return null;
    }

    private void LoadFromDisk(Dictionary<FileExistenceOrder, FilePath> filePaths)
    {
        var existingFiles = filePaths.Where(f => f.Value.Exists()).OrderBy(f => f.Key).ToDictionary(f => f.Key, f => f.Value);

        if (existingFiles.Count == 0)
        {
            ResetAllToDefault();
            return;
        }

        var chosenFile = existingFiles.First().Value;
        _currentFilePath = chosenFile;
        var fileContents = File.ReadAllText(chosenFile.Path);

        try
        {
            using var doc = JsonDocument.Parse(fileContents);
            var jsonDocRoot = doc.RootElement;
            var fileVer = GetFileVersion(doc.RootElement);

            switch (fileVer)
            {
                case FileSchemaV1.FileVersion:
                    LoadDefaults();
                    break;

                case FileSchemaV2.FileVersion:
                    AutoSaveGamesStatus = FileSchemaV2.GetAutoSaveGamesStatus(jsonDocRoot) ?? AutoSaveGamesStatus;
                    AutoSaveGamesIntervalInMinutes = FileSchemaV2.GetAutoSaveGamesIntervalInMinutes(jsonDocRoot) ?? AutoSaveGamesIntervalInMinutes;
                    ActiveAppLanguageTag = FileSchemaV2.GetActiveAppLanguageTag(jsonDocRoot) ?? ActiveAppLanguageTag;
                    break;
            }
        }
        catch
        {
            // ignore, considering all fields invalid
        }

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
        var fileSchema = new Dictionary<string, object>
        {
            [FileVersionPropertyName.Type2] = FileSchemaV2.FileVersion,
            [FileSchemaV2.AutoSaveGamesStatusPropertyName] = AutoSaveGamesStatus.ToString(),
            [FileSchemaV2.AutoSaveGamesIntervalInMinutesPropertyName] = AutoSaveGamesIntervalInMinutes,
            [FileSchemaV2.ActiveAppLanguageTagPropertyName] = ActiveAppLanguageTag.ToString()
        };

        var jsonString = JsonSerializer.Serialize(fileSchema, _fileJsonStyle);
        Directory.CreateDirectory(_currentFilePath.ParentPath);

        File.WriteAllText(_currentFilePath.Path, jsonString);
    }

    private void LoadDefaults()
    {
        AutoSaveGamesStatus = EnabledStatus.Enabled;
        AutoSaveGamesIntervalInMinutes = 1;
        ActiveAppLanguageTag = LanguageManager.LanguageTag.en_US;
    }

    private enum FileExistenceOrder { V2, V1 }

    private static class FileVersionPropertyName
    {
        public const string Type2 = "fileVersion";
        public const string Type1 = "file_version";
    }

    private record struct FileSchemaV2
    {
        public const string AutoSaveGamesStatusPropertyName = "autoSaveGamesStatus";
        public const string AutoSaveGamesIntervalInMinutesPropertyName = "autoSaveGamesIntervalInMinutes";
        public const string ActiveAppLanguageTagPropertyName = "activeAppLanguageTag";

        public const int FileVersion = 2;

        public static EnabledStatus? GetAutoSaveGamesStatus(JsonElement root)
        {
            if (!root.TryGetProperty(AutoSaveGamesStatusPropertyName, out var autoSaveGamesStatusElem))
                return null;

            if (autoSaveGamesStatusElem.ValueKind != JsonValueKind.String)
                return null;

            if (Enum.TryParse(autoSaveGamesStatusElem.GetString()?.ToLower(), out EnabledStatus parsedStatus))
                return parsedStatus;

            return null;
        }

        public static int? GetAutoSaveGamesIntervalInMinutes(JsonElement root)
        {
            if (!root.TryGetProperty(AutoSaveGamesIntervalInMinutesPropertyName, out var autoSaveIntervalInMinutesElem))
                return null;

            if (autoSaveIntervalInMinutesElem.ValueKind == JsonValueKind.Number && autoSaveIntervalInMinutesElem.TryGetInt32(out var autoSaveIntervalInMinutesFound))
                return autoSaveIntervalInMinutesFound;

            return null;
        }

        public static LanguageManager.LanguageTag? GetActiveAppLanguageTag(JsonElement root)
        {
            if (!root.TryGetProperty(ActiveAppLanguageTagPropertyName, out var activeLanguageCodeElem))
                return null;

            if (activeLanguageCodeElem.ValueKind != JsonValueKind.String)
                return null;

            if (Enum.TryParse(activeLanguageCodeElem.GetString(), out LanguageManager.LanguageTag parsedCode))
                return parsedCode;

            return null;
        }
    }

    private record struct FileSchemaV1
    {
        public const int FileVersion = 1;
        // rest get discarded to enforce new defaults & usage of new features
    }
}