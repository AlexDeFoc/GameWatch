namespace MainApp.Scenes;

public sealed class ChangeAutoSaveInterval : Scene
{
    public ChangeAutoSaveInterval(AppContext ctx) : base(ctx) {}

    public override void Run(SceneManager manager) => manager.ReturnFrom(this, GetUserInput());

    private int? GetUserInput()
    {
        var strings = Ctx.LanguageManager.Strings.ChangeAutoSaveIntervalScene;
        var logger = Ctx.Logger;

        while (true)
        {
            Logger.Clear();
            logger.WriteCached();

            logger.WriteLine(Logger.Label.Tip, strings.CancelTip);
            logger.WriteLine(Logger.Label.Info, strings.CurrentAutoSaveInterval(Ctx));

            logger.Write(Logger.Label.Request, strings.RequestMsg);
            string? input = System.Console.ReadLine();

            if (input == null)
                return null;

            if (int.TryParse(input, out int choice) && choice >= 1)
                return choice;

            logger.WriteLineToCache(Logger.Label.Error, strings.InvalidInputMsg);
        }
    }
}