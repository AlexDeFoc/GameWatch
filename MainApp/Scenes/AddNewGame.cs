using MainApp.SceneTypes;

namespace MainApp.Scenes;

public sealed class AddNewGame : IScene
{
    public IScene Execute()
    {
        var menu = new FormText(lang: _lang,
                            logger: _logger,
                            requestMsg: _lang.ActiveLanguagePack.AddNewGame_RequestMsg);

        string? title = menu.ReadInput();

        // ReSharper disable once InvertIf
        if (title != null)
        {
            _logger.WriteLineToCache(Logger.Label.Success, _lang.ActiveLanguagePack.AddNewGame_SuccessfullyAddedNewGameMsg(title));
            _gameLibrary.AddGame(title);
        }

        return _previousScene;
    }

    public AddNewGame(IScene previousScene, LanguageManager lang, Logger logger, GameLibrary gameLibrary)
    {
        _previousScene = previousScene;
        _lang = lang;
        _logger = logger;
        _gameLibrary = gameLibrary;
    }

    private readonly IScene _previousScene;
    private readonly LanguageManager _lang;
    private readonly Logger _logger;
    private readonly GameLibrary _gameLibrary;
}