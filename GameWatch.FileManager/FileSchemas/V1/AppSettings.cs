using System.Text.Json.Serialization;

namespace GameWatch.FileManager.FileSchemas.V1;

public sealed class AppSettings
{
    [JsonPropertyName("file_version")]
    public int FileVersion { get; init; } = 1;

    [JsonPropertyName("auto_save_enabled_status")]
    public bool AutoSaveEnabledStatus { get; init; } = true;

    [JsonPropertyName("auto_save_interval_in_minutes")]
    public int AutoSaveIntervalInMins { get; init; } = 5;
}