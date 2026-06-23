using System.Threading.Tasks;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace GameWatch.Tui.App.Scenes;

public sealed class MainMenu(AppContext appCtx) : IScene
{
    private readonly Localization.Sections.MainMenuScene _strings = appCtx.LanguageManager.Strings.MainMenuScene;
    private readonly AppState _appState = appCtx.AppState;
    private readonly SceneManager _sceneMng = appCtx.SceneManager;
    private readonly GameLibrary _gameLib = appCtx.GameLibrary;
    private readonly Window _ui = appCtx.RootWindow;

    public void OnStart()
    {
        var listGamesOpt = new MenuItem()
        {
            Title = _strings.ListGamesOption,
            Action = () => { }
        };

        listGamesOpt.Accepting += (_, e) => e.Handled = true;

        var addGameOpt = new MenuItem()
        {
            Title = _strings.AddGameOption,
            Action = () => _sceneMng.ChangeRootScene(new AddGame(appCtx))
        };

        addGameOpt.Accepting += (_, e) => e.Handled = true;

        var exitAppOpt = new MenuItem()
        {
            Title = _strings.ExitAppOption,
            Action = _appState.StopApp
        };

        exitAppOpt.Accepting += (_, e) => e.Handled = true;

        var optMenu = new Menu(
            [
                listGamesOpt, addGameOpt, exitAppOpt
            ]
        )
        {
            X = Pos.Center(),
            Y = Pos.Center(),
            Height = Dim.Auto(),
            Width = Dim.Auto()
        };

        if (_gameLib.Games.Count == 0)
            listGamesOpt.Visible = false;

        _ui.Add(optMenu);
    }
}