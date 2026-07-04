using GameWatch.Tui.App.Localization.Sections;
using System.Text.Json.Serialization;

namespace GameWatch.Tui.App.Localization;

public class LanguagePack
{
    [JsonPropertyName("GeneralStrings")]
    public GeneralStrings GeneralStrings { get; init; } = new();

    [JsonPropertyName("MainMenuScene")]
    public MainMenuScene MainMenuScene { get; init; } = new();

    [JsonPropertyName("AddGameScene")]
    public AddGameScene AddGameScene { get; init; } = new();

    [JsonPropertyName("GameClass")]
    public GameClass GameClass { get; init; } = new();
}