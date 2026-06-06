namespace MainApp.Scenes;

public sealed class AddNewGame : Scene
{
    public AddNewGame(AppContext ctx) : base(ctx)
    {
        _strings = ctx.LanguageManager.Strings.AddNewGameScene;
        _logger = ctx.Logger;
        _gameLib = ctx.GameLibrary;
    }

    public override void Run(SceneManager manager)
    {
        _sceneManager = manager;

        manager.NavigateTo(new GetNewGameTitle(Ctx, purposeId: "request_of_new_game_title"));
    }

    public override void OnReturnedFrom(Scene from, SceneManager.SceneResult? result)
    {
        if (from is GetNewGameTitle)
        {
            if (result is { PurposeId: var purposeId, Data: var rawValue })
            {
                if (rawValue is null)
                {
                    _logger.WriteLineToCache(Logger.Label.Info, _strings.CancelledActionMsg);
                    _sceneManager.ReturnToPreviousScene();
                }
                else if (rawValue is string newGameTitle && purposeId == "request_of_new_game_title")
                {
                    _retrievedGameTitle = newGameTitle;
                    _sceneManager.NavigateTo(new GetGameSavingMode(Ctx, purposeId: "request_of_game_saving_mode"));
                }
            }
        }
        else if (from is GetGameSavingMode)
        {
            if (result is { PurposeId: var purposeId, Data: var rawValue })
            {
                if (rawValue is null)
                {
                    _logger.WriteLineToCache(Logger.Label.Info, _strings.CancelledActionMsg);
                    _sceneManager.ReturnToPreviousScene();
                }
                else if (rawValue is GameEntry.WorkingMode workingMode && purposeId == "request_of_game_saving_mode")
                {
                    _gameWorkingMode = workingMode;

                    if (_gameWorkingMode is GameEntry.WorkingMode.Manual)
                    {
                        ExecuteAddGameWithManualWorkingMode();
                        _sceneManager.ReturnToPreviousScene();
                    }
                    else
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
                    _retrievedExePath = exePath;
                    ExecuteAddGameWithAutomaticWorkingMode();
                    _sceneManager.ReturnToPreviousScene();
                }
            }
        }

        _sceneManager.ReturnToPreviousScene();
    }

    private void ExecuteAddGameWithManualWorkingMode()
    {
        _gameLib.AddGame(_retrievedGameTitle);
        _logger.WriteLineToCache(Logger.Label.Success, _strings.SuccessfullyAddedGame(_retrievedGameTitle));
    }

    private void ExecuteAddGameWithAutomaticWorkingMode()
    {
        _gameLib.AddGame(_retrievedGameTitle, _retrievedExePath);
        _logger.WriteLineToCache(Logger.Label.Success, _strings.SuccessfullyAddedGame(_retrievedGameTitle));
    }

    // Private variables
    private SceneManager _sceneManager = null!;
    private string _retrievedGameTitle = "";
    private GameEntry.WorkingMode _gameWorkingMode;
    private string _retrievedExePath = "";

    // Aliases
    private readonly LanguageManager.IAddNewGameSceneStrings _strings;
    private readonly Logger _logger;
    private readonly GameLibrary _gameLib;
}