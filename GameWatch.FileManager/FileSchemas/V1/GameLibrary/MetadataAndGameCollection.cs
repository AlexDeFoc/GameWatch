using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GameWatch.FileManager.FileSchemas.V1.GameLibrary;

public sealed class MetadataAndGameCollection
{
    [JsonPropertyName("file_version")]
    public int FileVersion { get; init; } = 1;

    [JsonPropertyName("games")]
    public List<GameEntry> Games { get; init; } = [];
}