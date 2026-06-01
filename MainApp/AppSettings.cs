using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
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
        _filePaths = new Dictionary<FileExistanceOrder, Utils.FilePath>
        {
            [FileExistanceOrder.V1] = new(location: Utils.FileLocation.ExeFolder, fileName: "settings.json"),
            [FileExistanceOrder.V2] = new(location: Utils.FileLocation.LocalAppDataFolder, fileName: "Settings.json")
        };

        LoadFromDisk();
    }

    private void OnLanguageChanged(LanguageManager.LanguageCode newLang)
    {
        LanguageChanged?.Invoke(this, newLang);
    }

    private void LoadFromDisk()
    {
        // var foundFiles = _filePaths.Values.Where(filePath => filePath.Exists).ToList();

        var chosenFilePath = _filePaths.Values.FirstOrDefault(filePath => filePath.Exists);

        bool fileNeedsToBeRebuilt = false;

        if (chosenFilePath == null)
        {
            LoadDefaults();
            SaveToDisk();
            return;
        }

        string fileContents = File.ReadAllText(chosenFilePath.RealPath);

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
                            _autoSaveEnabledStatus = autoSaveEnabledStatusElem.GetBoolean() == true ? 1 : 0;
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
                            _autoSaveEnabledStatus = autoSaveEnabledStatusElem.GetBoolean() == true ? 1 : 0;
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

        if (fileNeedsToBeRebuilt)
            SaveToDisk();
    }

    private void SaveToDisk()
    {
        var fileSchema = new Dictionary<string, object>
        {
            [FileVersionPropertyName.Type1] = FileSchemaV2.FileVersion,
            // ReSharper disable once RedundantTernaryExpression
            [FileSchemaV2.AutoSaveEnabledStatusPropertyName] = _autoSaveEnabledStatus == 1 ? true : false,
            [FileSchemaV2.AutoSaveIntervalInMinutesPropertyName] = _autoSaveIntervalInMinutes,
            [FileSchemaV2.ActiveLanguageCodePropertyName] = LanguageCode.ToString()
        };

        var jsonString = JsonSerializer.Serialize(fileSchema, _fileJsonSerializerOpts);
        File.WriteAllText(_filePaths[FileExistanceOrder.V2].RealPath, jsonString);
    }

    private void LoadDefaults()
    {
        LanguageCode = LanguageManager.LanguageCode.en_US;
        _autoSaveEnabledStatus = 1;
        _autoSaveIntervalInMinutes = 1;
    }

    private int _autoSaveEnabledStatus;
    private int _autoSaveIntervalInMinutes;
    private readonly Dictionary<FileExistanceOrder, Utils.FilePath> _filePaths;
    private readonly JsonSerializerOptions _fileJsonSerializerOpts = new(){ WriteIndented = true };

    private enum FileExistanceOrder
    {
        V1,
        V2
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