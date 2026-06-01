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
        get;
        private set
        {
            if (field == value) return;
            field = value;
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

        var fileNeedsToBeRebuilt = false;
        int foundFileVersion = 0;

        try
        {
            using var doc = JsonDocument.Parse(fileContents); // may throw if completely invalid JSON
            var jsonDocRoot = doc.RootElement;

            if (jsonDocRoot.TryGetProperty(FileVersionPropertyName.Type1, out var verType1Elem))
            {
                if (verType1Elem.ValueKind is JsonValueKind.Number && verType1Elem.TryGetInt32(out var verFound))
                {
                    if (verFound is FileSchemaV1.FileVersion or FileSchemaV2.FileVersion)
                    {
                        foundFileVersion = verFound;
                    }
                    else
                    {
                        fileNeedsToBeRebuilt = true;
                    }
                }
                else
                {
                    fileNeedsToBeRebuilt = true;
                }
            }
            else
            {
                fileNeedsToBeRebuilt = true;
            }

            if (fileNeedsToBeRebuilt)
            {
                // skip other branches
            }
            else switch (foundFileVersion)
            {
                case FileSchemaV1.FileVersion:
                {
                    if (jsonDocRoot.TryGetProperty(FileSchemaV1.AutoSaveEnabledStatusPropertyName, out var autoSaveEnabledStatusElem))
                    {
                        if (autoSaveEnabledStatusElem.ValueKind is JsonValueKind.True or JsonValueKind.False)
                        {
                            // ReSharper disable once RedundantBoolCompare
                            _autoSaveEnabledStatus = autoSaveEnabledStatusElem.GetBoolean();
                        }
                        else
                        {
                            fileNeedsToBeRebuilt = true;
                        }
                    }
                    else
                    {
                        fileNeedsToBeRebuilt = true;
                    }

                    if (jsonDocRoot.TryGetProperty(FileSchemaV1.AutoSaveIntervalInMinutesPropertyName, out var autoSaveIntervalInMinutesElem))
                    {
                        if (autoSaveIntervalInMinutesElem.ValueKind is JsonValueKind.Number && autoSaveIntervalInMinutesElem.TryGetInt32(out var autoSaveIntervalInMinutesFound))
                        {
                            _autoSaveIntervalInMinutes = autoSaveIntervalInMinutesFound;
                        }
                        else
                        {
                            fileNeedsToBeRebuilt = true;
                        }
                    }
                    else
                    {
                        fileNeedsToBeRebuilt = true;
                    }

                    break;
                }

                case FileSchemaV2.FileVersion:
                {
                    if (jsonDocRoot.TryGetProperty(FileSchemaV2.AutoSaveEnabledStatusPropertyName, out var autoSaveEnabledStatusElem))
                    {
                        if (autoSaveEnabledStatusElem.ValueKind is JsonValueKind.True or JsonValueKind.False)
                        {
                            // ReSharper disable once RedundantBoolCompare
                            _autoSaveEnabledStatus = autoSaveEnabledStatusElem.GetBoolean();
                        }
                        else
                        {
                            fileNeedsToBeRebuilt = true;
                        }
                    }
                    else
                    {
                        fileNeedsToBeRebuilt = true;
                    }

                    if (jsonDocRoot.TryGetProperty(FileSchemaV2.AutoSaveIntervalInMinutesPropertyName, out var autoSaveIntervalInMinutesElem))
                    {
                        if (autoSaveIntervalInMinutesElem.ValueKind is JsonValueKind.Number && autoSaveIntervalInMinutesElem.TryGetInt32(out var autoSaveIntervalInMinutesFound))
                        {
                            _autoSaveIntervalInMinutes = autoSaveIntervalInMinutesFound;
                        }
                        else
                        {
                            fileNeedsToBeRebuilt = true;
                        }
                    }
                    else
                    {
                        fileNeedsToBeRebuilt = true;
                    }

                    if (jsonDocRoot.TryGetProperty(FileSchemaV2.ActiveLanguageCodePropertyName, out var activeLanguageCodeElem))
                    {
                        if (activeLanguageCodeElem.ValueKind is JsonValueKind.String)
                        {
                            if (Enum.TryParse(activeLanguageCodeElem.GetString(), out LanguageManager.LanguageCode parsedCode))
                            {
                                LanguageCode = parsedCode;
                            }
                            else
                            {
                                fileNeedsToBeRebuilt = true;
                            }
                        }
                        else
                        {
                            fileNeedsToBeRebuilt = true;
                        }
                    }
                    else
                    {
                        fileNeedsToBeRebuilt = true;
                    }

                    break;
                }
            }
        }
        catch
        {
            fileNeedsToBeRebuilt = true;
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
        LanguageCode = LanguageManager.LanguageCode.en_US;
        _autoSaveEnabledStatus = true;
        _autoSaveIntervalInMinutes = 1;
    }

    private volatile bool _autoSaveEnabledStatus;
    private volatile int _autoSaveIntervalInMinutes;
    private readonly Dictionary<FileExistenceOrder, Utils.FilePath> _filePaths;
    private readonly JsonSerializerOptions _fileJsonSerializerOpts = new(){ WriteIndented = true };

    // NOTE: Keep in descending order
    private enum FileExistenceOrder
    {
        V2,
        V1
    }

    private record struct FileVersionPropertyName
    {
        public const string Type1 = "file_version";
    }

    private record struct FileSchemaV1
    {
        public const int FileVersion = 1;
        public const string AutoSaveEnabledStatusPropertyName = "auto_save_enabled_status";
        public const string AutoSaveIntervalInMinutesPropertyName = "auto_save_interval_in_minutes";
    }

    // NOTE: Latest file schema
    private record struct FileSchemaV2
    {
        public const int FileVersion = 2;
        public const string AutoSaveEnabledStatusPropertyName = "auto_save_enabled_status";
        public const string AutoSaveIntervalInMinutesPropertyName = "auto_save_interval_in_minutes";
        public const string ActiveLanguageCodePropertyName = "active_language_code";
    }
}