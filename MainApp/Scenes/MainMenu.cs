using MainApp.SceneItems;
using MainApp.SceneTypes;

namespace MainApp.Scenes;

public sealed class MainMenu : IScene
{
    public IScene? Execute()
    {
        IScene nextScene = this;
        var menu = new Menu(_lang, _logger);

        menu.AddOption(new MenuOption(displayText: _lang.ActiveLanguagePack.MainMenu_AddNewGameOption_DisplayText, action: () => { nextScene = new AddNewGame(lang: _lang, logger: _logger, appState: _appState, gameLibrary: _gameLibrary, appSettings: _appSettings); }));
        menu.AddOption(new MenuOption(displayText: _lang.ActiveLanguagePack.MainMenu_ExitAppOption_DisplayText, action: () => { _appState.ToggleAppRunningStatus(); }));

        menu.ReadInputAndProcessOption();

        return _appState.ShouldAppContinueToRun() ? nextScene : null;
    }

    public MainMenu(LanguageManager lang, Logger logger, GameLibrary gameLibrary, AppState appState, AppSettings appSettings)
    {
        _lang = lang;
        _logger = logger;
        _gameLibrary = gameLibrary;
        _appState = appState;
        _appSettings = appSettings;
    }

    private readonly LanguageManager _lang;
    private readonly Logger _logger;
    private readonly GameLibrary _gameLibrary;
    private readonly AppState _appState;
    private readonly AppSettings _appSettings;
}