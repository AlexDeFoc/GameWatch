namespace MainApp.Scenes;

public sealed class ChangeGameTitle : Scene
{
    public ChangeGameTitle(AppContext ctx) : base(ctx) {}

    public override void Run(SceneManager manager)
    {
        var selectedGameId = GetGameIdFromUser();

        if (selectedGameId == null)
        {
            Ctx.Logger.WriteLine(Logger.Label.Info, Ctx.LanguageManager.Strings.ChangeGameTitleScene.CancelledActionMsg);
        }
        else
        {
            var newGameTitle = GetGameTitleFromUser();

            if (newGameTitle == null)
            {
                Ctx.Logger.WriteLine(Logger.Label.Info, Ctx.LanguageManager.Strings.ChangeGameTitleScene.CancelledActionMsg);
            }
            else
            {
                Ctx.Logger.WriteLineToCache(Logger.Label.Info, Ctx.LanguageManager.Strings.ChangeGameTitleScene.TitleChangedMsg(Ctx, (int)selectedGameId, newGameTitle));
                Ctx.GameLibrary.ChangeGameTitle(gameId: (int)selectedGameId, newGameTitle: newGameTitle);
            }
        }

        manager.ReturnFrom(this);
    }

    // Menu related methods
    private int? GetGameIdFromUser()
    {
        var strings = Ctx.LanguageManager.Strings.ChangeGameTitleScene;
        var logger = Ctx.Logger;

        while (true)
        {
            Logger.Clear();
            logger.WriteCached();

            var gamesCount = ListGames();

            logger.WriteLine(Logger.Label.Tip, strings.CancelTip);
            logger.Write(Logger.Label.Request, strings.RequestMsgForGameId);
            string? input = System.Console.ReadLine();
            if (input == null)
                return null;

            if (int.TryParse(input, out int choice) && choice >= 1 && choice <= gamesCount)
                return choice;

            logger.WriteLineToCache(Logger.Label.Error, strings.InvalidInputMsg);
        }
    }

    private string? GetGameTitleFromUser()
    {
        var strings = Ctx.LanguageManager.Strings.ChangeGameTitleScene;
        var logger = Ctx.Logger;

        while (true)
        {
            Logger.Clear();
            logger.WriteCached();

            logger.WriteLine(Logger.Label.Tip, strings.CancelTip);

            logger.Write(Logger.Label.Request, strings.RequestMsgForGameTitle);
            return System.Console.ReadLine();
        }
    }

    private int ListGames()
    {
        var logger = Ctx.Logger;
        for (int i = 0; i < Ctx.GameLibrary.Games.Count; i++)
        {
            var curGame = Ctx.GameLibrary.Games[i];
            logger.WriteLine($"{i + 1}. {curGame.Title}");
        }

        return Ctx.GameLibrary.Games.Count;
    }
}