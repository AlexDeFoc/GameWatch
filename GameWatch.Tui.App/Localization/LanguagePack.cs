using GameWatch.Tui.App.Localization.Sections;
using System.Text.Json.Serialization;

namespace GameWatch.Tui.App.Localization;

public class LanguagePack
{
    [JsonPropertyName("MainMenuScene")]
    public MainMenuScene MainMenuScene { get; set; } = new();
}