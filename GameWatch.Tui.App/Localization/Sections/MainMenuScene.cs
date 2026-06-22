using Semver;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace GameWatch.Tui.App.Localization.Sections;

public class MainMenuScene
{
    public string AppVersionLabelFormat { get; init; } = string.Empty;

    public string ListGamesOption { get; init; } = string.Empty;
    public string AddGameOption { get; init; } = string.Empty;
    public string ExitAppOption { get; init; } = string.Empty;

    public string AppVersionLabel(SemVersion v) => string.Format(AppVersionLabelFormat, v.Major, v.Minor, v.Patch);
}