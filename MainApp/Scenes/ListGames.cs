using System.Collections.Generic;
using MainApp.SceneTypes;

namespace MainApp.Scenes;

public sealed class ListGames : IScene
{
    public IScene Execute()
    {
        var menu = new Info(lang: _lang,
            logger: _logger,
            collectionWhichToList: ComposeWhichToListCollection(),
            emptyCollectionErrMsg: _lang.ActiveLanguagePack.ListGames_NoGamesFoundMsg);

        menu.ListCollectionAndRequestAnyKeyPress();

        return new MainMenu(colorManager: _colorManager, lang: _lang, logger: _logger, gameLibrary: _gameLibrary, appState: _appState, appSettings: _appSettings);
    }

    private List<string> ComposeWhichToListCollection()
    {
        List<string> collection = [];

        int gameIndex = 1;
        foreach (var game in _gameLibrary.Games)
        {
            collection.Add($"{gameIndex}. {game.Title} - {game.GetPrintablePlaytime()}");
            ++gameIndex;
        }

        return collection;
    }

    public ListGames(ColorManager colorManager, LanguageManager lang, Logger logger, GameLibrary gameLibrary, AppState appState, AppSettings appSettings)
    {
        _colorManager = colorManager;
        _lang = lang;
        _logger = logger;
        _gameLibrary = gameLibrary;
        _appState = appState;
        _appSettings = appSettings;
    }

    private readonly ColorManager _colorManager;
    private readonly LanguageManager _lang;
    private readonly Logger _logger;
    private readonly GameLibrary _gameLibrary;
    private readonly AppState _appState;
    private readonly AppSettings _appSettings;
}