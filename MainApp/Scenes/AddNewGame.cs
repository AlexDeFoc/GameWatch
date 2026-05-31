using MainApp.SceneTypes;

namespace MainApp.Scenes;

public sealed class AddNewGame : IScene
{
    public IScene Execute()
    {
        var menu = new Form(lang: _lang,
                            logger: _logger,
                            cancellationTipMsg: _lang.ActiveLanguagePack.AddNewGame_CancellationTipMsg,
                            requestMsg: _lang.ActiveLanguagePack.AddNewGame_RequestMsg);

        string? title = menu.ReadInput();

        // ReSharper disable once InvertIf
        if (title != null)
        {
            _logger.WriteLineToCache(Logger.Label.Success, _lang.ActiveLanguagePack.AddNewGame_SuccessfullyAddedNewGameMsg(title));
            _gameLibrary.AddGame(title);
        }

        return new MainMenu(lang: _lang, logger: _logger, gameLibrary: _gameLibrary, appState: _appState, appSettings: _appSettings);
    }

    public AddNewGame(LanguageManager lang, Logger logger, GameLibrary gameLibrary, AppState appState, AppSettings appSettings)
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