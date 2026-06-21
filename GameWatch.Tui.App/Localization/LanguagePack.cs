using GameWatch.Tui.App.Localization.Sections;
using System.Text.Json.Serialization;

namespace GameWatch.Tui.App.Localization;

public class LanguagePack
{
    [JsonPropertyName("MainMenuScene")]
    public MainMenuScene MainMenuScene { get; set; } = new();

    [JsonPropertyName("ListGamesScene")]
    public ListGamesScene ListGamesScene { get; set; } = new();

    [JsonPropertyName("GameClass")]
    public GameClass GameClass { get; set; } = new();
}