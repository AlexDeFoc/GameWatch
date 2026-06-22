using GameWatch.Tui.App.Localization;
using Terminal.Gui.App;
using Terminal.Gui.Views;

namespace GameWatch.Tui.App;

public sealed class AppContext
{
    public AppContext(IApplication appUi, Window rootUiWindow)
    {
        LanguageManager = new LanguageManager(AppSettings);
        GameLibrary = new GameLibrary(this);
        AppUi = appUi;
        RootUiWindow = rootUiWindow;
    }

    public void InitSceneManager(SceneManager mng) => SceneManager = mng;

    public IApplication AppUi { get; }
    public Window RootUiWindow { get; }
    public SceneManager SceneManager { get; private set; } = null!;
    public LanguageManager LanguageManager { get; }
    public AppSettings AppSettings { get; } = new();
    public AppState AppState { get; } = new();
    public GameLibrary GameLibrary { get; }
}