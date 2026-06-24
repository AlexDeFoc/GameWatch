using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace GameWatch.Tui.App.Scenes;

public sealed class MainMenu(AppContext appCtx) : IScene
{
    private readonly IApplication _appUi = appCtx.AppUi;
    private readonly Localization.Sections.MainMenuScene _ownStrings = appCtx.LanguageManager.Strings.MainMenuScene;
    private readonly SceneManager _sceneMng = appCtx.SceneManager;
    private readonly GameLibrary _gameLibrary = appCtx.GameLibrary;
    private readonly AppState _appState = appCtx.AppState;
    private Window _mainWindow = null!;
    private Controls.Menu _navigationMenu = null!;

    public void OnStart()
    {
        InitMainWindow();
        SetupMenu();
        RouteUiElements();

        _appUi.Run(_mainWindow);
    }

    private void InitMainWindow()
    {
        _mainWindow = new Window
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
    }

    private void SetupMenu()
    {
        var listGamesOption = new Controls.Button(text: _ownStrings.ListGamesOption);
        var exitAppOption = new Controls.Button(text: _ownStrings.ExitAppOption, action: _appState.StopApp);

        _navigationMenu = new(Pos.Center(), Pos.Center(), [listGamesOption, exitAppOption]);
        // _mainWindow.Add(listGamesOption, exitAppOption);
    }

    private void RouteUiElements()
    {
        _mainWindow.Add(_navigationMenu);
    }
}