namespace MainApp.Scenes;

public sealed class StopOneOfManyManualWorkingGame : Scene
{
    public StopOneOfManyManualWorkingGame(AppContext ctx) : base(ctx)
    {
        _strings = ctx.LanguageManager.Strings.StopOneOfManyManualWorkingGameScene;
        _logger = ctx.Logger;
        _gameLib = ctx.GameLibrary;
    }

    public override void Run(SceneManager manager)
    {
        var selectedGameId = GetUserInput();

        if (selectedGameId == null)
        {
            _logger.WriteLineToCache(Logger.Label.Info, _strings.CancelledActionMsg);
        }
        else
        {
            _logger.WriteLine(Logger.Label.Success, _strings.StoppedGameMsg(Ctx, (int)selectedGameId));
            _gameLib.StopManualWorkingGame(gameId: (int)selectedGameId);
        }

        manager.ReturnToPreviousScene();
    }

    // Menu related methods
    private int? GetUserInput()
    {
        while (true)
        {
            Logger.Clear();
            _logger.WriteCached();

            var gamesCount = ListGames();

            _logger.WriteLine(Logger.Label.Tip, _strings.CancelTip);
            _logger.Write(Logger.Label.Request, _strings.RequestMsg);
            string? input = System.Console.ReadLine();
            if (input == null)
                return null;

            if (int.TryParse(input, out int choice) && choice >= 1 && choice <= gamesCount)
                return choice;

            _logger.WriteLineToCache(Logger.Label.Error, _strings.InvalidInputMsg);
        }
    }

    private int ListGames()
    {
        var manualWorkingGames = _gameLib.GetActiveManualWorkingGames();

        for (int i = 0; i < manualWorkingGames.Count; i++)
        {
            var curGame = manualWorkingGames[i];
            _logger.WriteLine($"{i + 1}. {curGame.Title}");
        }

        return manualWorkingGames.Count;
    }

    // Aliases
    private readonly LanguageManager.IStopOneOfManyManualWorkingGameSceneStrings _strings;
    private readonly Logger _logger;
    private readonly GameLibrary _gameLib;
}