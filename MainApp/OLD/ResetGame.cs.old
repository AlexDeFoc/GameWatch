namespace MainApp.Scenes;

public sealed class ResetGame : Scene
{
    public ResetGame(AppContext ctx) : base(ctx)
    {
        _strings = ctx.LanguageManager.Strings.ResetGameScene;
        _logger = ctx.Logger;
        _gameLib = ctx.GameLibrary;
    }

    public override void Run(SceneManager manager)
    {
        _sceneManager = manager;
        var gottenGameId = GetUserInput();

        if (gottenGameId is null)
        {
            _logger.WriteLineToCache(Logger.Label.Info, _strings.CancelledActionMsg);
            _sceneManager.ReturnToPreviousScene();
        }
        else
        {
            _selectedGameId = (int)gottenGameId;
            manager.NavigateTo(new ConfirmDecisionMenu(Ctx, purposeId: "reset_game"));
        }
    }

    public override void OnReturnedFrom(Scene from, SceneManager.SceneResult? result)
    {
        if (from is ConfirmDecisionMenu)
        {
            if (result is { PurposeId: var purposeId, Data: bool condition })
            {
                if (purposeId == "reset_game")
                {
                    if (condition)
                    {
                        _logger.WriteLineToCache(Logger.Label.Success, _strings.SuccessfullyResetGame(Ctx, _selectedGameId));
                        _gameLib.ResetGame(_selectedGameId);
                    }
                    else
                    {
                        _logger.WriteLineToCache(Logger.Label.Info, _strings.CancelledActionMsg);
                    }
                }
            }
        }
        else
        {
            _sceneManager.ReturnToPreviousScene();
        }
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
            _logger.Write(Logger.Label.Request, _strings.RequestMsgForGameId);
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
        for (int i = 0; i < _gameLib.Games.Count; i++)
        {
            var curGame = _gameLib.Games[i];
            _logger.WriteLine($"{i + 1}. {curGame.Title} - {curGame.GetPrintablePlaytime()}");
        }

        return _gameLib.Games.Count;
    }

    // Private variables
    private SceneManager _sceneManager = null!;
    private int _selectedGameId;

    // Aliases
    private readonly LanguageManager.IResetGameSceneStrings _strings;
    private readonly Logger _logger;
    private readonly GameLibrary _gameLib;
}