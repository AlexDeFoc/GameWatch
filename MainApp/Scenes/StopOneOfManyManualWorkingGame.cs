namespace MainApp.Scenes;

public sealed class StopOneOfManyManualWorkingGame : Scene
{
    public StopOneOfManyManualWorkingGame(AppContext ctx) : base(ctx) {}

    public override void Run(SceneManager manager)
    {
        var selectedGameId = GetUserInput();

        if (selectedGameId == null)
        {
            Ctx.Logger.WriteLine(Logger.Label.Info, Ctx.LanguageManager.Strings.StopOneOfManyManualWorkingGameScene.CancelledActionMsg);
        }
        else
        {
            Ctx.Logger.WriteLine(Logger.Label.Info, Ctx.LanguageManager.Strings.StopOneOfManyManualWorkingGameScene.StoppedGameMsg(Ctx, (int)selectedGameId));
            Ctx.GameLibrary.StopManualWorkingGame(gameId: (int)selectedGameId);
        }

        manager.ReturnFrom(this);
    }

    // Menu related methods
    private int? GetUserInput()
    {
        var strings = Ctx.LanguageManager.Strings.StopOneOfManyManualWorkingGameScene;
        var logger = Ctx.Logger;

        while (true)
        {
            Logger.Clear();
            logger.WriteCached();

            var gamesCount = ListGames();

            logger.WriteLine(Logger.Label.Tip, strings.CancelTip);
            logger.Write(Logger.Label.Request, strings.RequestMsg);
            string? input = System.Console.ReadLine();
            if (input == null)
                return null;

            if (int.TryParse(input, out int choice) && choice >= 1 && choice <= gamesCount)
                return choice;

            logger.WriteLineToCache(Logger.Label.Error, strings.InvalidInputMsg);
        }
    }

    private int ListGames()
    {
        var logger = Ctx.Logger;
        var manualWorkingGames = Ctx.GameLibrary.GetActiveManualWorkingGames();

        for (int i = 0; i < manualWorkingGames.Count; i++)
        {
            var curGame = manualWorkingGames[i];
            logger.WriteLine($"{i + 1}. {curGame.Title}");
        }

        return manualWorkingGames.Count;
    }
}