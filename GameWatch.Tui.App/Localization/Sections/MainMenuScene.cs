using Semver;

namespace GameWatch.Tui.App.Localization.Sections;

public class MainMenuScene
{
    public string AppVersionLabelFormat { get; set; } = string.Empty;

    public string ListGamesOption { get; set; } = string.Empty;
    public string ExitAppOption { get; set; } = string.Empty;

    public string AppVersionLabel(SemVersion v) => string.Format(AppVersionLabelFormat, v.Major, v.Minor, v.Patch);
}
