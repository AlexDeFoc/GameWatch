using GameWatch.Tui.App.Localization;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace GameWatch.Tui.App;

public static class Program
{
    public static void Main()
    {
        using var uiApp = Application.Create().Init();
        var rootWindow = new Window()
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        AppContext appCtx = new();
        AppState appState = new();
        AppSettings appSettings = new();
        LanguageManager languageManager = new(appSettings);
        GameLibrary gameLibrary = new(appState);
        SceneManager sceneManager = new(appState, appCtx, rootWindow, uiApp);

        appCtx.RootWindow = rootWindow;
        appCtx.AppState = appState;
        appCtx.AppSettings = appSettings;
        appCtx.LanguageManager = languageManager;
        appCtx.GameLibrary = gameLibrary;
        appCtx.SceneManager = sceneManager;

        sceneManager.ChangeRootScene(new Scenes.MainMenu(appCtx));

        uiApp.Run(rootWindow);
    }
}