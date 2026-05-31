using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GwConsoleAppCore;

public class AppSettings
{
    public AppSettings()
    {
        LoadFromDisk();
        SaveToDisk(); // TODO: Use it async in TaskDispatcher.Start(), instead of constructor
    }

    private void LoadFromDisk()
    {
        DiskStorages fileObjs = new();
        string fileContents;

        if (Utils.FileExistsAndNotEmpty(DiskStorages.FileNames.Variant1))
        {
            fileContents = File.ReadAllText(DiskStorages.FileNames.Variant1);

            try
            {
                using var doc = JsonDocument.Parse(fileContents); // may throw if completely invalid JSON
                var jsonDocRoot = doc.RootElement;

                if (jsonDocRoot.TryGetProperty(nameof(DiskStorages.V1.file_version), out var versionElem) && versionElem.ValueKind is JsonValueKind.Number && versionElem.TryGetInt32(out var fileVersionFound))
                {
                    if (fileVersionFound == fileObjs.v1.file_version)
                    {
                        if (jsonDocRoot.TryGetProperty(nameof(DiskStorages.V1.auto_save_enabled_status), out var autoSaveEnabledStatusElem) && autoSaveEnabledStatusElem.ValueKind is JsonValueKind.True or JsonValueKind.False)
                            fileObjs.latestVersion.AutoSaveEnabled = autoSaveEnabledStatusElem.GetBoolean();
                        else
                            _foundInvalidFieldsInDiskFile = true;

                        if (jsonDocRoot.TryGetProperty(nameof(DiskStorages.V1.auto_save_interval_in_minutes), out var autoSaveIntervalElem) && autoSaveIntervalElem.ValueKind is JsonValueKind.Number && autoSaveIntervalElem.TryGetInt32(out var autoSaveIntervalFound))
                            fileObjs.latestVersion.AutoSaveIntervalInMinutes = autoSaveIntervalFound;
                        else
                            _foundInvalidFieldsInDiskFile = true;
                    }
                }

                File.Delete(DiskStorages.FileNames.Variant1);
            }
            catch
            {
                // ignore
            }
        }
        else if (Utils.FileExistsAndNotEmpty(DiskStorages.FileNames.LatestVariant))
        {
            fileContents = File.ReadAllText(DiskStorages.FileNames.LatestVariant);

            try
            {
                using var doc = JsonDocument.Parse(fileContents); // may throw if completely invalid JSON
                var jsonDocRoot = doc.RootElement;

                if (jsonDocRoot.TryGetProperty(nameof(DiskStorages.LatestVersion.FileVersion), out var versionElem) && versionElem.ValueKind is JsonValueKind.Number && versionElem.TryGetInt32(out var fileVersionFound))
                {
                    if (fileVersionFound == fileObjs.latestVersion.FileVersion)
                    {
                        if (jsonDocRoot.TryGetProperty(nameof(DiskStorages.LatestVersion.ActiveLanguageCode), out var activeLanguageCodeElem) && activeLanguageCodeElem.ValueKind is JsonValueKind.String)
                        {
                            var value = activeLanguageCodeElem.GetString();

                            if (value != null)
                                fileObjs.latestVersion.ActiveLanguageCode = value;
                            else
                                _foundInvalidFieldsInDiskFile = true;
                        }
                        else
                            _foundInvalidFieldsInDiskFile = true;

                        if (jsonDocRoot.TryGetProperty(nameof(DiskStorages.LatestVersion.AutoSaveEnabled), out var autoSaveEnabledElem) && autoSaveEnabledElem.ValueKind is JsonValueKind.True or JsonValueKind.False)
                            fileObjs.latestVersion.AutoSaveEnabled = autoSaveEnabledElem.GetBoolean();
                        else
                            _foundInvalidFieldsInDiskFile = true;

                        if (jsonDocRoot.TryGetProperty(nameof(DiskStorages.LatestVersion.AutoSaveIntervalInMinutes), out var autoSaveIntervalElem) && autoSaveIntervalElem.ValueKind is JsonValueKind.Number && autoSaveIntervalElem.TryGetInt32(out var autoSaveIntervalFound))
                            fileObjs.latestVersion.AutoSaveIntervalInMinutes = autoSaveIntervalFound;
                        else
                            _foundInvalidFieldsInDiskFile = true;
                    }
                }
            }
            catch
            {
                // ignore
            }
        }
        else
            _foundInvalidFieldsInDiskFile = true;

        if (Enum.TryParse(fileObjs.latestVersion.ActiveLanguageCode, out LanguageManager.LanguageCode parsedCode))
            CurrentLanguageCode = parsedCode;
        else
            _foundInvalidFieldsInDiskFile = true;

        _autoSaveEnabled = fileObjs.latestVersion.AutoSaveEnabled ? 1 : 0;

        if (fileObjs.latestVersion.AutoSaveIntervalInMinutes >= 1)
            _autoSaveIntervalInMinutes = fileObjs.latestVersion.AutoSaveIntervalInMinutes;
        else
        {
            _autoSaveIntervalInMinutes = 5;
            _foundInvalidFieldsInDiskFile = true;
        }
    }

    private async Task SaveToDiskAsync()
    {
        if (!_foundInvalidFieldsInDiskFile)
            return;

        var fileObj = new DiskStorages.LatestVersion()
        {
            ActiveLanguageCode = nameof(CurrentLanguageCode),
            AutoSaveEnabled = _autoSaveEnabled == 1,
            AutoSaveIntervalInMinutes = _autoSaveIntervalInMinutes
        };

        await using var fileStream = File.Create(DiskStorages.FileNames.LatestVariant);
        await JsonSerializer.SerializeAsync(fileStream, fileObj, _diskFileSerializerOpts);
    }

    private void SaveToDisk()
    {
        if (!_foundInvalidFieldsInDiskFile)
                    return;

        var fileObj = new DiskStorages.LatestVersion()
        {
            ActiveLanguageCode = CurrentLanguageCode.ToString(),
            AutoSaveEnabled = _autoSaveEnabled == 1,
            AutoSaveIntervalInMinutes = _autoSaveIntervalInMinutes
        };

        var jsonString = JsonSerializer.Serialize(fileObj, _diskFileSerializerOpts);
        File.WriteAllText(DiskStorages.FileNames.LatestVariant, jsonString);
    }

    public LanguageManager.LanguageCode CurrentLanguageCode { get; set; }
    private int _autoSaveEnabled; // 1 = enabled, 0 = disabled
    private int _autoSaveIntervalInMinutes;
    private bool _foundInvalidFieldsInDiskFile;
    private readonly JsonSerializerOptions _diskFileSerializerOpts = new(){ WriteIndented = true };

    public bool IsAutoSaveEnabled() => Interlocked.CompareExchange(ref _autoSaveEnabled, 1, 1) == 1;
    public TimeSpan AutoSaveInterval() => TimeSpan.FromTicks(Interlocked.CompareExchange(ref _autoSaveIntervalInMinutes, 1, 1));

    public async Task ToggleAutoSaveStatus()
    {
        int initial, desired;

        do
        {
            initial = _autoSaveEnabled;
            desired = initial ^ 1;
        } while (Interlocked.CompareExchange(ref _autoSaveEnabled, desired, initial) != initial);

        await SaveToDiskAsync();
    }

    public async Task ChangeAutoSaveInterval(TimeSpan newInterval)
    {
        int initial, desired;

        do
        {
            initial = _autoSaveIntervalInMinutes;
            desired = newInterval.Minutes;
        } while (Interlocked.CompareExchange(ref _autoSaveIntervalInMinutes, desired, initial) != initial);

        await SaveToDiskAsync();
    }

    private class DiskStorages
    {
        // ReSharper disable InconsistentNaming
        public V1 v1 { get; } = new();
        public LatestVersion latestVersion { get; } = new();

        public record struct FileNames
        {
            public static string Variant1 { get; } = Path.Combine(AppContext.BaseDirectory, "settings.json");
            public static string LatestVariant { get; } = Path.Combine(AppContext.BaseDirectory, "Settings.json");
        }

        public class V1
        {
            public int file_version { get; } = 1;
            public bool auto_save_enabled_status { get; init; } = true;
            public int auto_save_interval_in_minutes { get; init; } = 5;
        }

        public class LatestVersion
        {
            public int FileVersion { get; } = 2;
            public string ActiveLanguageCode { get; set; } = nameof(LanguageManager.LanguageCode.en_US);
            public bool AutoSaveEnabled { get; set; } = true;
            public int AutoSaveIntervalInMinutes { get; set; } = 5;
        }
        // ReSharper restore InconsistentNaming
    }
}