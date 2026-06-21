using GameWatch.Tui.App.Localization;
using Terminal.Gui.App;
using Terminal.Gui.Views;

namespace GameWatch.Tui.App;

public sealed class AppContext
{
    public AppContext(IApplication appUi, Window rootUiWindow)
    {
        LanguageManager = new(AppSettings);
        AppUi = appUi;
        RootUiWindow = rootUiWindow;
    }

    public void InitSceneManager(SceneManager mng) => SceneManager = mng;

    public IApplication AppUi { get; init; }
    public Window RootUiWindow { get; init; }
    public SceneManager SceneManager { get; set; } = null!;
    public LanguageManager LanguageManager { get; init; }
    public AppSettings AppSettings { get; init; } = new();
    public AppState AppState { get; init; } = new();
}