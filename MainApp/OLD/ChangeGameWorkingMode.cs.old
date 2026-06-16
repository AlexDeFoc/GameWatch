namespace MainApp.Scenes;

public sealed class ChangeGameWorkingMode : Scene
{
    public ChangeGameWorkingMode(AppContext ctx) : base(ctx)
    {
        _strings = ctx.LanguageManager.Strings.ChangeGameWorkingModeScene;
        _logger = ctx.Logger;
        _gameLib = ctx.GameLibrary;
    }

    public override void Run(SceneManager manager)
    {
        _sceneManager = manager;

        var gottenGameId = GetGameIdFromUser();

        if (gottenGameId is null)
        {
            _logger.WriteLineToCache(Logger.Label.Info, _strings.CancelledActionMsg);
            manager.ReturnToPreviousScene();
        }
        else
        {
            _selectedGameId = (int)gottenGameId;
            manager.NavigateTo(new GetGameWorkingMode(Ctx, purposeId: "request_of_new_game_working_mode"));
        }
    }

    public override void OnReturnedFrom(Scene from, SceneManager.SceneResult? result)
    {
        if (from is GetGameWorkingMode)
        {
            if (result is { PurposeId: var purposeId, Data: var rawValue })
            {
                if (rawValue is null)
                {
                    _logger.WriteLineToCache(Logger.Label.Info, _strings.CancelledActionMsg);
                    _sceneManager.ReturnToPreviousScene();
                }
                else if (rawValue is GameEntry.WorkingMode workingMode && purposeId == "request_of_new_game_working_mode")
                {
                    _gameWorkingMode = workingMode;

                    if (_gameWorkingMode == _gameLib.GetGameWorkingMode(_selectedGameId))
                    {
                        _logger.WriteLineToCache(Logger.Label.Error, _strings.ModeAlreadyThisValueMsg(Ctx, _gameWorkingMode));
                        _sceneManager.ReturnToPreviousScene();
                    }
                    else if (_gameWorkingMode is GameEntry.WorkingMode.Manual)
                    {
                        _logger.WriteLineToCache(Logger.Label.Success, _strings.ChangedModeTo(Ctx, _gameWorkingMode));
                        _gameLib.SetGameWorkingMode(_selectedGameId, _gameWorkingMode);
                        _sceneManager.ReturnToPreviousScene();
                    }
                    else if (_gameWorkingMode is GameEntry.WorkingMode.Automatic)
                    {
                        _sceneManager.NavigateTo(new GetGameExePath(Ctx, purposeId: "request_of_game_exe_path"));
                    }
                }
            }
        }
        else if (from is GetGameExePath)
        {
            if (result is { PurposeId: var purposeId, Data: var rawValue })
            {
                if (rawValue is null)
                {
                    _logger.WriteLineToCache(Logger.Label.Info, _strings.CancelledActionMsg);
                    _sceneManager.ReturnToPreviousScene();
                }
                else if (rawValue is string exePath && purposeId == "request_of_game_exe_path")
                {
                    _logger.WriteLineToCache(Logger.Label.Success, _strings.ChangedModeTo(Ctx, _gameWorkingMode));
                    _gameLib.SetGameWorkingMode(_selectedGameId, _gameWorkingMode, exePath);
                    _sceneManager.ReturnToPreviousScene();
                }
            }
        }
        else
        {
            _sceneManager.ReturnToPreviousScene();
        }
    }

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

    private int ListGames()
    {
        for (int i = 0; i < _gameLib.Games.Count; i++)
        {
            var curGame = _gameLib.Games[i];

            _logger.WriteLine($"{i + 1}. {curGame.Title} - {GameEntry.GetPrintableCurrentWorkingMode(Ctx, curGame.CurrentWorkingMode)}");
        }

        return _gameLib.Games.Count;
    }

    // Private
    private int _selectedGameId;
    private GameEntry.WorkingMode _gameWorkingMode;
    private SceneManager _sceneManager = null!;

    // Aliases
    private readonly LanguageManager.IChangeGameWorkingModeSceneStrings _strings;
    private readonly Logger _logger;
    private readonly GameLibrary _gameLib;
}