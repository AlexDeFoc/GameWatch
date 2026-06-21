using GameWatch.Tui.App.FileSystem;
using GameWatch.Tui.App.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace GameWatch.Tui.App;

public sealed class AppSettings
{
    private readonly JsonSerializerOptions _fileJsonStyle = new() { WriteIndented = true };
    private FilePath _currentFilePath = null!;

    public AppSettings()
    {
        _currentFilePath = new(FolderPath.LocationCode.OurUserDataDirectory) { BaseName = "AppSettings", Extension = "json" };

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
            if (field != value)
            {
                field = value;
                LanguageChanged?.Invoke(value);
            }
        }
    }

    public event Action<LanguageManager.LanguageTag>? LanguageChanged;

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

    public void ResetAllToDefault()
    {
        LoadDefaults();
        SaveToDisk();
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
        public const EnabledStatus AutoSaveGamesStatus = EnabledStatus.Enabled;
        public const int AutoSaveGamesIntervalInMinutes = 1;
        public const LanguageManager.LanguageTag ActiveAppLanguageTag = LanguageManager.LanguageTag.en_US;

        public static EnabledStatus? GetAutoSaveGamesStatus(JsonElement root)
        {
            if (!root.TryGetProperty(AutoSaveGamesStatusPropertyName, out var autoSaveGamesStatusElem))
                return null;

            if (autoSaveGamesStatusElem.ValueKind is not JsonValueKind.String)
                return null;

            if (Enum.TryParse(autoSaveGamesStatusElem.GetString()?.ToLower(), out EnabledStatus parsedStatus))
                return parsedStatus;

            return null;
        }

        public static int? GetAutoSaveGamesIntervalInMinutes(JsonElement root)
        {
            if (!root.TryGetProperty(AutoSaveGamesIntervalInMinutesPropertyName, out var autoSaveIntervalInMinutesElem))
                return null;

            if (autoSaveIntervalInMinutesElem.ValueKind is JsonValueKind.Number && autoSaveIntervalInMinutesElem.TryGetInt32(out var autoSaveIntervalInMinutesFound))
                return autoSaveIntervalInMinutesFound;

            return null;
        }

        public static LanguageManager.LanguageTag? GetActiveAppLanguageTag(JsonElement root)
        {
            if (!root.TryGetProperty(ActiveAppLanguageTagPropertyName, out var activeLanguageCodeElem))
                return null;

            if (activeLanguageCodeElem.ValueKind is not JsonValueKind.String)
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
