namespace MainApp.Scenes;

public sealed class ChangeGameTitle : Scene
{
    public ChangeGameTitle(AppContext ctx) : base(ctx)
    {
        _strings = ctx.LanguageManager.Strings.ChangeGameTitleScene;
        _logger = ctx.Logger;
        _gameLib = ctx.GameLibrary;
    }

    public override void Run(SceneManager manager)
    {
        var selectedGameId = GetGameIdFromUser();

        if (selectedGameId == null)
        {
            _logger.WriteLine(Logger.Label.Info, _strings.CancelledActionMsg);
        }
        else
        {
            var newGameTitle = GetGameTitleFromUser();

            if (newGameTitle == null)
            {
                _logger.WriteLine(Logger.Label.Info, _strings.CancelledActionMsg);
            }
            else
            {
                _logger.WriteLineToCache(Logger.Label.Success, _strings.TitleChangedMsg(Ctx, (int)selectedGameId, newGameTitle));
                _gameLib.ChangeGameTitle(gameId: (int)selectedGameId, newGameTitle: newGameTitle);
            }
        }

        manager.ReturnToPreviousScene();
    }

    // Menu related methods
    private int? GetGameIdFromUser()
    {
        while (true)
        {
            Logger.Clear();
            _logger.WriteCached();

            var gamesCount = ListGames();

            _logger.WriteLine(Logger.Label.Tip, _strings.CancelTip);
            _logger.Write(Logger.Label.Request, _strings.RequestMsgForGameId);
            string? input = System.Console.ReadLine();
            if (input == null)
                return null;

            if (int.TryParse(input, out int choice) && choice >= 1 && choice <= gamesCount)
                return choice;

            _logger.WriteLineToCache(Logger.Label.Error, _strings.InvalidInputMsg);
        }
    }

    private string? GetGameTitleFromUser()
    {
        while (true)
        {
            Logger.Clear();
            _logger.WriteCached();

            _logger.WriteLine(Logger.Label.Tip, _strings.CancelTip);

            _logger.Write(Logger.Label.Request, _strings.RequestMsgForGameTitle);
            return System.Console.ReadLine();
        }
    }

    private int ListGames()
    {
        for (int i = 0; i < _gameLib.Games.Count; i++)
        {
            var curGame = _gameLib.Games[i];
            _logger.WriteLine($"{i + 1}. {curGame.Title}");
        }

        return _gameLib.Games.Count;
    }

    // Aliases
    private readonly LanguageManager.IChangeGameTitleSceneStrings _strings;
    private readonly Logger _logger;
    private readonly GameLibrary _gameLib;
}