using System.Text.Json.Serialization;

namespace GameWatch.FileManager.FileSchemas.GameLibrary.V1;

public class GameEntry
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("playtime")]
    public long PlayTime { get; set; }
}