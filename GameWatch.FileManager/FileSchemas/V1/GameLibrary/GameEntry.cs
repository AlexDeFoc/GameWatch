using System.Text.Json.Serialization;

namespace GameWatch.FileManager.FileSchemas.V1.GameLibrary;

public sealed class GameEntry
{
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("playtime")]
    public long PlayTime { get; init; }
}