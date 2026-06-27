using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GameWatch.FileManager.FileSchemas.GameLibrary.V1;

public class GameCollection
{
    [JsonPropertyName("file_version")]
    public int FileVersion { get; set; }

    [JsonPropertyName("games")]
    public List<GameEntry> Games { get; set; } = [];
}