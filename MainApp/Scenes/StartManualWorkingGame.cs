namespace MainApp.Scenes;

public sealed class StartManualWorkingGame : Scene
{
    public StartManualWorkingGame(AppContext ctx) : base(ctx) {}

    public override void Run(SceneManager manager)
    {
        var selectedGameId = GetUserInput();

        if (selectedGameId == null)
        {
            Ctx.Logger.WriteLine(Logger.Label.Info, Ctx.LanguageManager.Strings.StartManualWorkingGameScene.CancelledActionMsg);
        }
        else
        {
            Ctx.Logger.WriteLine(Logger.Label.Info, Ctx.LanguageManager.Strings.StartManualWorkingGameScene.StartedGameMsg(Ctx, (int)selectedGameId));
            Ctx.GameLibrary.StartManualWorkingGame(gameId: (int)selectedGameId);
        }

        manager.ReturnFrom(this);
    }

    // Menu related methods
    private int? GetUserInput()
    {
        var strings = Ctx.LanguageManager.Strings.StartManualWorkingGameScene;
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
        var manualWorkingGames = Ctx.GameLibrary.GetManualWorkingGames();

        for (int i = 0; i < manualWorkingGames.Count; i++)
        {
            var curGame = manualWorkingGames[i];
            logger.WriteLine($"{i + 1}. {curGame.Title} - {curGame.GetPrintablePlaytime()}");
        }

        return manualWorkingGames.Count;
    }
}