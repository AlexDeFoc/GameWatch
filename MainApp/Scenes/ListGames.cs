using System;

namespace MainApp.Scenes;

public sealed class ListGames : Scene
{
    public ListGames(AppContext ctx) : base(ctx)
    {
        _strings = ctx.LanguageManager.Strings.ListGamesScene;
        _logger = ctx.Logger;
        _gameLib = ctx.GameLibrary;
    }

    public override void Run(SceneManager manager)
    {
        ListGameEntries();
        RequestInput();
        manager.ReturnToPreviousScene();
    }

    private void ListGameEntries()
    {
        Console.Clear();
        _logger.WriteCached();

        for (int i = 0; i < _gameLib.Games.Count; i++)
        {
            var curGame = _gameLib.Games[i];
            _logger.WriteLine($"{i + 1}. {curGame.Title} - {curGame.GetPrintablePlaytime()}");
        }
    }

    private void RequestInput()
    {
        _logger.WriteLine(Logger.Label.Request, _strings.RequestMsg);
        Console.ReadKey();
    }

    // Aliases
    private readonly LanguageManager.IListGamesSceneStrings _strings;
    private readonly Logger _logger;
    private readonly GameLibrary _gameLib;
}