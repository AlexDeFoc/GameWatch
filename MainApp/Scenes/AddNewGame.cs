namespace MainApp.Scenes;

public sealed class AddNewGame : Scene
{
    public AddNewGame(AppContext ctx) : base(ctx) {}

    public override void Run(SceneManager manager)
    {
        _sceneManager = manager;

        manager.NavigateTo(new GetNewGameTitle(Ctx));
    }

    public override void OnReturnedFrom(Scene from, object? result)
    {
        var strings = Ctx.LanguageManager.Strings.AddNewGameScene;

        if (from is GetNewGameTitle)
        {
            if (result == null)
            {
                Ctx.Logger.WriteLineToCache(Logger.Label.Info, strings.CancelledActionMsg);
                _sceneManager.ReturnFrom(this);
            }
            else
            {
                _retrievedGameTitle = (string)result;
                _sceneManager.NavigateTo(new GetGameSavingMode(Ctx));
            }
        }
        else if (from is GetGameSavingMode)
        {
            if (result == null)
            {
                Ctx.Logger.WriteLineToCache(Logger.Label.Info, strings.CancelledActionMsg);
                _sceneManager.ReturnFrom(this);
            }
            else
            {
                _gameWorkingMode = (GameEntry.WorkingMode)result;

                if (_gameWorkingMode == GameEntry.WorkingMode.Manual)
                {
                    ExecuteAddGameWithManualWorkingMode();
                    _sceneManager.ReturnFrom(this);
                }
                else
                {
                    Ctx.Logger.WriteLineToCache(Logger.Label.Error, strings.NotYetImplementedMsg);
                    _sceneManager.ReturnFrom(this);
                }
            }
        }
    }

    private void ExecuteAddGameWithManualWorkingMode()
    {
        var strings = Ctx.LanguageManager.Strings.AddNewGameScene;
        Ctx.GameLibrary.AddGame(_retrievedGameTitle);
        Ctx.Logger.WriteLineToCache(Logger.Label.Success, strings.SuccessfullyAddedGame(_retrievedGameTitle));
    }

    private string _retrievedGameTitle = "";
    private GameEntry.WorkingMode _gameWorkingMode;
    private SceneManager _sceneManager = null!;
}