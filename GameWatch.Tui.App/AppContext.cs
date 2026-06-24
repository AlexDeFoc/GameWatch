using GameWatch.Tui.App.Localization;
using Terminal.Gui.App;

namespace GameWatch.Tui.App;

public sealed class AppContext
{
    public IApplication AppUi { get; set; } = null!;
    public AppState AppState { get; set; } = null!;
    public AppSettings AppSettings { get; set; } = null!;
    public LanguageManager LanguageManager { get; set; } = null!;
    public GameLibrary GameLibrary { get; set; } = null!;
    public SceneManager SceneManager { get; set; } = null!;
}