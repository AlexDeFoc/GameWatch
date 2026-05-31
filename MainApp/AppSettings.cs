using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace MainApp;

public sealed class AppSettings
{
    public event EventHandler<LanguageManager.LanguageCode>? LanguageChanged;

    public LanguageManager.LanguageCode LanguageCode
    {
        get;
        private set
        {
            if (field == value) return;
            field = value;
            OnLanguageChanged(value);
        }
    }

    public int AutoSaveIntervalInMinutes
    {
        get => Interlocked.CompareExchange(ref _autoSaveIntervalInMinutes, 1, 1);
        set
        {
            int initial, desired;

            do
            {
                initial = _autoSaveIntervalInMinutes;
                desired = value;
            } while (Interlocked.CompareExchange(ref _autoSaveIntervalInMinutes, desired, initial) != initial);

            SaveToDisk();
        }
    }

    public bool IsAutoSaveEnabled() => Interlocked.CompareExchange(ref _autoSaveEnabledStatus, 1, 1) == 1;

    public void ToggleAutoSaveStatus()
    {
        int initial, desired;

        do
        {
            initial = _autoSaveEnabledStatus;
            desired = initial ^ 1;
        } while (Interlocked.CompareExchange(ref _autoSaveEnabledStatus, desired, initial) != initial);

        SaveToDisk();
    }

    public AppSettings()
    {
        // NOTE: Keep these in the order from 1 -> latest, cuz when we load the file, we check in that order which file exists first
        // NOTE 2: The version isn't used, its being kept just to make it easier to keep in order filenames & filepaths
        _filePaths = new Dictionary<FileVersionForFilePaths, Utils.FilePath>
        {
            [FileVersionForFilePaths.V1] = new(location: Utils.FileLocation.ExeFolder, fileName: "settings.json"),
            [FileVersionForFilePaths.V2] = new(location: Utils.FileLocation.LocalAppDataFolder, fileName: "Settings.json")
        };

        LoadFromDisk();
    }

    private void OnLanguageChanged(LanguageManager.LanguageCode newLang)
    {
        LanguageChanged?.Invoke(this, newLang);
    }

    private void LoadFromDisk()
    {
        var chosenFilePath = _filePaths.Values.FirstOrDefault(filePath => filePath.Exists);

        if (chosenFilePath == null)
        {
            LoadDefaults();
            SaveToDisk();
            return;
        }

        string fileContents = File.ReadAllText(chosenFilePath.RealPath);

        FileVersionForFilePaths confirmedFileVersion;

        try
        {
            using var doc = JsonDocument.Parse(fileContents);
            var root = doc.RootElement;

            // Confirm which version the file is
            if (root.TryGetProperty(FileSchemaV1.FileVersionPropertyName, out var versionV1Elem))
            {
                if (versionV1Elem.ValueKind is JsonValueKind.Number && versionV1Elem.TryGetInt32(out var fileVersionFound))
                {
                    if (fileVersionFound == FileSchemaV1.FileVersion)
                        confirmedFileVersion = FileVersionForFilePaths.V1;
                }
                else
                {
                    LoadDefaults();
                    SaveToDisk();
                    return;
                }
            }
            else if (root.TryGetProperty(FileSchemaV2.FileVersionPropertyName, out var versionV2Elem))
            {
                if (versionV2Elem.ValueKind is JsonValueKind.Number && versionV2Elem.TryGetInt32(out var fileVersionFound))
                {
                    if (fileVersionFound == FileSchemaV2.FileVersion)
                        confirmedFileVersion = FileVersionForFilePaths.V2;
                }
                else
                {
                    LoadDefaults();
                    SaveToDisk();
                    return;
                }
            }
        }
        catch
        {
            LoadDefaults();
            SaveToDisk();
            return;
        }


    }

    private void LoadDefaults()
    {
        LanguageCode = LanguageManager.LanguageCode.en_US;
        _autoSaveEnabledStatus = 1;
        _autoSaveIntervalInMinutes = 1;
    }

    private int _autoSaveEnabledStatus;
    private int _autoSaveIntervalInMinutes;
    private readonly Dictionary<FileVersionForFilePaths, Utils.FilePath> _filePaths;

    private enum FileVersionForFilePaths
    {
        V1,
        V2
    }

    private class FileSchemaV1
    {
        public const int FileVersion = 1;
        public const string FileVersionPropertyName = "file_version";
        public const string AutoSaveEnabledStatusPropertyName = "auto_save_enabled_status";
        public const string AutoSaveIntervalInMinutesPropertyName = "auto_save_interval_in_minutes";

        public bool AutoSaveEnabledStatus { get; init; }
        public int AutoSaveIntervalInMinutes { get; init; }
    }

    private class FileSchemaV2
    {
        public const int FileVersion = 2;
        public const string FileVersionPropertyName = "file_version";
        public const string AutoSaveEnabledStatusPropertyName = "auto_save_enabled_status";
        public const string AutoSaveIntervalInMinutesPropertyName = "auto_save_interval_in_minutes";
        public const string ActiveLanguageCodePropertyName = "active_language_code";

        public bool AutoSaveEnabledStatus { get; init; }
        public int AutoSaveIntervalInMinutes { get; init; }
        public string ActiveLanguageCode { get; init; }
    }
}