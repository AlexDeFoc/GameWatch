using MainApp.SceneTypes;

namespace MainApp.Scenes;

public sealed class MainMenu : IScene
{
    public IScene? Execute()
    {
        IScene nextScene = this;
        var menu = new Menu(_lang, _logger);

        menu.AddOption(new MenuOption(displayText: _lang.ActiveLanguagePack.MainMenu_ExitAppOption_DisplayText, action: () => { _appState.ToggleAppRunningStatus(); }));

        menu.ReadInputAndProcessOption();

        return _appState.ShouldAppContinueToRun() ? nextScene : null;
    }

    public MainMenu(LanguageManager lang, Logger logger, AppState appState)
    {
        _lang = lang;
        _logger = logger;
        _appState = appState;
    }

    private readonly LanguageManager _lang;
    private readonly Logger _logger;
    private readonly AppState _appState;
}