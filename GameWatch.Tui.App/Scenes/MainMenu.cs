using GameWatch.DataTypes;
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
        SetupAddGameTestingUiElems();
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
        var listGamesOption = new Controls.Button(text: _ownStrings.ListGamesOption, visibilityPredicate: () => _gameLibrary.Games.Count > 0);
        var addGameOption = new Controls.Button(text: _ownStrings.AddGameOption, action: () => _sceneMng.ChangeRootScene(new AddGame(appCtx)));
        var exitAppOption = new Controls.Button(text: _ownStrings.ExitAppOption, action: _appState.StopApp);

        _navigationMenu = new(Pos.Center(), Pos.Center(), [listGamesOption, addGameOption, exitAppOption]);
    }

    private void SetupAddGameTestingUiElems()
    {
        var testLabel = new Label()
        {
            X = 0,
            Y = 0,
            Width = Dim.Auto(DimAutoStyle.Text),
            Height = Dim.Auto(DimAutoStyle.Text)
        };

        if (_sceneMng.PrevSceneResult is (string gameTitle, GameMode workingMode))
        {
            testLabel.Text = $"Title: '{gameTitle}' - Working mode: {(workingMode == GameMode.Automatic ? "Automatic" : "Manual")}";
        }
        else
        {
            testLabel.Text = "Status: 'Haven't entered add game scene yet'";
        }

        _mainWindow.Add(testLabel);
    }

    private void RouteUiElements()
    {
        _mainWindow.Add(_navigationMenu);
    }
}